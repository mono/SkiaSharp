using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Widget;
using SkiaSharp;
using SkiaSharp.Tests.Visual;

namespace SkiaSharp.Tests.RenderHost.Android;

[Activity (Label = "SkiaSharp Render Host", MainLauncher = true)]
public class MainActivity : Activity
{
	// Port matches AndroidHostSession on the host side. The host forwards
	// tcp:<this port> via `adb forward` so localhost on the host machine
	// reaches the device's listener.
	private const int Port = 7771;

	private TcpListener _listener;
	private CancellationTokenSource _cts;
	private TextView _status;

	protected override void OnCreate (Bundle savedInstanceState)
	{
		base.OnCreate (savedInstanceState);

		// Minimal UI — just so the activity is foreground-eligible and the
		// process isn't killed by the OS. Nothing actually renders to the
		// screen; rendering goes into Skia-allocated offscreen surfaces.
		_status = new TextView (this) { Text = "starting…" };
		SetContentView (_status);

		_cts = new CancellationTokenSource ();
		_listener = new TcpListener (IPAddress.Any, Port);
		_listener.Start ();
		_status.Text = $"listening on :{Port}";
		_ = AcceptLoopAsync (_cts.Token);
	}

	protected override void OnDestroy ()
	{
		_cts?.Cancel ();
		try { _listener?.Stop (); } catch { }
		base.OnDestroy ();
	}

	private async System.Threading.Tasks.Task AcceptLoopAsync (CancellationToken ct)
	{
		while (!ct.IsCancellationRequested) {
			TcpClient client;
			try {
				client = await _listener.AcceptTcpClientAsync (ct);
			} catch { return; }
			_ = HandleClientAsync (client, ct);
		}
	}

	private async System.Threading.Tasks.Task HandleClientAsync (TcpClient client, CancellationToken ct)
	{
		using (client)
		using (var stream = client.GetStream ())
		using (var reader = new StreamReader (stream, Encoding.UTF8))
		using (var writer = new StreamWriter (stream, new UTF8Encoding (encoderShouldEmitUTF8Identifier: false)) { AutoFlush = true }) {
			string line;
			while (!ct.IsCancellationRequested && (line = await reader.ReadLineAsync (ct)) != null) {
				string responseJson;
				try {
					var req = JsonSerializer.Deserialize<RenderRequest> (line);
					var pixels = Render (req!.renderer, req.scene, req.w, req.h);
					responseJson = JsonSerializer.Serialize (new RenderResponse {
						ok = true,
						pixels_b64 = Convert.ToBase64String (pixels),
					});
				} catch (Exception ex) {
					responseJson = JsonSerializer.Serialize (new RenderResponse {
						ok = false,
						error = $"{ex.GetType ().Name}: {ex.Message}",
					});
				}
				await writer.WriteLineAsync (responseJson.AsMemory (), ct);
			}
		}
	}

	private static byte[] Render (string renderer, string sceneName, int w, int h)
	{
		var scene = SceneCatalog.Get (sceneName);
		var info = new SKImageInfo (w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
		return renderer switch {
			"android-raster"         => RenderRaster (scene, info),
			"android-ganesh-vulkan"  => RenderGaneshVulkan (scene, info),
			_ => throw new ArgumentException ($"Unknown renderer '{renderer}'"),
		};
	}

	private static byte[] RenderRaster (ISkiaScene scene, SKImageInfo info)
	{
		using var surface = SKSurface.Create (info)
			?? throw new InvalidOperationException ("SKSurface.Create returned null for raster");
		scene.Draw (surface.Canvas);
		return ReadRgbaPremul (surface, info);
	}

	private static byte[] RenderGaneshVulkan (ISkiaScene scene, SKImageInfo info)
	{
		// VulkanLoader.Shared works on Android: prefers a CPU-typed device
		// (none on a real Android phone) → falls back to devices[0] which
		// is the actual GPU. SKGraphiteContext.IsBackendAvailable check
		// stays the same.
		var vk = VulkanLoader.Shared;
		if (!vk.IsAvailable)
			throw new InvalidOperationException ($"Vulkan loader unavailable: {vk.FailureReason}");

		using var bc = new GRVkBackendContext {
			VkInstance         = vk.Instance,
			VkPhysicalDevice   = vk.PhysicalDevice,
			VkDevice           = vk.Device,
			VkQueue            = vk.Queue,
			GraphicsQueueIndex = vk.QueueFamilyIndex,
			MaxAPIVersion      = VulkanLoader.VK_API_VERSION_1_3,
			ProtectedContext   = false,
			GetProcedureAddress = vk.GetProc,
		};
		using var ctx = GRContext.CreateVulkan (bc)
			?? throw new InvalidOperationException ("GRContext.CreateVulkan returned null on Android");
		using var surface = SKSurface.Create (ctx, budgeted: true, info)
			?? throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/Vulkan");
		scene.Draw (surface.Canvas);
		ctx.Flush ();
		ctx.Submit (synchronous: true);
		return ReadRgbaPremul (surface, info);
	}

	private static byte[] ReadRgbaPremul (SKSurface surface, SKImageInfo info)
	{
		var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
		var pixels = new byte[rgba.BytesSize];
		unsafe {
			fixed (byte* p = pixels) {
				if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
					throw new InvalidOperationException ("SKSurface.ReadPixels failed");
			}
		}
		return pixels;
	}

	private sealed class RenderRequest
	{
		public string renderer { get; set; } = "";
		public string scene { get; set; } = "";
		public int w { get; set; }
		public int h { get; set; }
	}

	private sealed class RenderResponse
	{
		public bool ok { get; set; }
		public string? pixels_b64 { get; set; }
		public string? error { get; set; }
	}
}
