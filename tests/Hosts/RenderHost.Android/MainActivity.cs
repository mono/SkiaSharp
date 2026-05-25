using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
			"android-ganesh-gles"    => RenderGaneshGles (scene, info),
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

	private static unsafe byte[] RenderGaneshGles (ISkiaScene scene, SKImageInfo info)
	{
		// Android-specific EGL + GLES3 bring-up. The desktop EglLoader's Linux
		// path uses EGL_EXT_platform_device + EGL_OPENGL_API, neither of which
		// is available here — Android only ships GLES, on the default display,
		// against a PBuffer (or surfaceless) surface.
		var display = eglGetDisplay (IntPtr.Zero); // EGL_DEFAULT_DISPLAY
		if (display == IntPtr.Zero)
			throw new InvalidOperationException ("eglGetDisplay returned EGL_NO_DISPLAY");
		if (eglInitialize (display, out _, out _) == 0)
			throw new InvalidOperationException ($"eglInitialize failed: 0x{eglGetError ():X}");
		if (eglBindAPI (EGL_OPENGL_ES_API) == 0)
			throw new InvalidOperationException ($"eglBindAPI(GLES) failed: 0x{eglGetError ():X}");

		var configAttribs = stackalloc int[] {
			EGL_RED_SIZE,        8,
			EGL_GREEN_SIZE,      8,
			EGL_BLUE_SIZE,       8,
			EGL_ALPHA_SIZE,      8,
			EGL_SURFACE_TYPE,    EGL_PBUFFER_BIT,
			EGL_RENDERABLE_TYPE, EGL_OPENGL_ES3_BIT,
			EGL_NONE,
		};
		IntPtr config;
		int numConfigs;
		if (eglChooseConfig (display, configAttribs, &config, 1, &numConfigs) == 0 || numConfigs == 0)
			throw new InvalidOperationException ($"eglChooseConfig found 0 GLES3 configs: 0x{eglGetError ():X}");

		var ctxAttribs = stackalloc int[] { EGL_CONTEXT_CLIENT_VERSION, 3, EGL_NONE };
		var context = eglCreateContext (display, config, IntPtr.Zero, ctxAttribs);
		if (context == IntPtr.Zero)
			throw new InvalidOperationException ($"eglCreateContext (GLES3) failed: 0x{eglGetError ():X}");

		// PBuffer size is irrelevant — Skia allocates its own offscreen FBO
		// inside the GRContext for every SKSurface. 1×1 is enough.
		var pbufferAttribs = stackalloc int[] { EGL_WIDTH, 1, EGL_HEIGHT, 1, EGL_NONE };
		var surface = eglCreatePbufferSurface (display, config, pbufferAttribs);
		if (surface == IntPtr.Zero)
			throw new InvalidOperationException ($"eglCreatePbufferSurface failed: 0x{eglGetError ():X}");

		if (eglMakeCurrent (display, surface, surface, context) == 0)
			throw new InvalidOperationException ($"eglMakeCurrent failed: 0x{eglGetError ():X}");

		try {
			using var glInterface = GRGlInterface.CreateGles (name => eglGetProcAddress (name))
				?? throw new InvalidOperationException ("GRGlInterface.CreateGles returned null");
			using var ctx = GRContext.CreateGl (glInterface)
				?? throw new InvalidOperationException ("GRContext.CreateGl returned null on Android GLES");
			using var skSurface = SKSurface.Create (ctx, budgeted: true, info)
				?? throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/GLES");
			scene.Draw (skSurface.Canvas);
			ctx.Flush ();
			ctx.Submit (synchronous: true);
			return ReadRgbaPremul (skSurface, info);
		} finally {
			eglMakeCurrent (display, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
			eglDestroySurface (display, surface);
			eglDestroyContext (display, context);
			// Leave the display initialised — terminating it would tear down
			// any other EGL state in this process (there isn't any, but a
			// future render call would have to re-initialise).
		}
	}

	// ---- EGL P/Invoke (Android libEGL.so) ----

	private const int EGL_RED_SIZE              = 0x3024;
	private const int EGL_GREEN_SIZE            = 0x3023;
	private const int EGL_BLUE_SIZE             = 0x3022;
	private const int EGL_ALPHA_SIZE            = 0x3021;
	private const int EGL_SURFACE_TYPE          = 0x3033;
	private const int EGL_PBUFFER_BIT           = 0x0001;
	private const int EGL_RENDERABLE_TYPE       = 0x3040;
	private const int EGL_OPENGL_ES3_BIT        = 0x0040;
	private const int EGL_NONE                  = 0x3038;
	private const int EGL_OPENGL_ES_API         = 0x30A0;
	private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
	private const int EGL_WIDTH                 = 0x3057;
	private const int EGL_HEIGHT                = 0x3056;

	[DllImport ("libEGL.so")] private static extern IntPtr eglGetDisplay (IntPtr displayId);
	[DllImport ("libEGL.so")] private static extern int eglInitialize (IntPtr display, out int major, out int minor);
	[DllImport ("libEGL.so")] private static extern int eglBindAPI (int api);
	[DllImport ("libEGL.so")] private static extern unsafe int eglChooseConfig (IntPtr display, int* attribList, IntPtr* configs, int configSize, int* numConfigs);
	[DllImport ("libEGL.so")] private static extern unsafe IntPtr eglCreateContext (IntPtr display, IntPtr config, IntPtr shareContext, int* attribList);
	[DllImport ("libEGL.so")] private static extern unsafe IntPtr eglCreatePbufferSurface (IntPtr display, IntPtr config, int* attribList);
	[DllImport ("libEGL.so")] private static extern int eglMakeCurrent (IntPtr display, IntPtr draw, IntPtr read, IntPtr context);
	[DllImport ("libEGL.so")] private static extern int eglDestroySurface (IntPtr display, IntPtr surface);
	[DllImport ("libEGL.so")] private static extern int eglDestroyContext (IntPtr display, IntPtr context);
	[DllImport ("libEGL.so", CharSet = CharSet.Ansi)] private static extern IntPtr eglGetProcAddress (string name);
	[DllImport ("libEGL.so")] private static extern uint eglGetError ();

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
