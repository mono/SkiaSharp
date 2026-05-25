using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using SkiaSharp;
using SkiaSharp.Tests.Visual;
using UIKit;

namespace SkiaSharp.Tests.RenderHost.iOS;

// Headless-ish iOS app: minimal UIWindow + UIViewController to satisfy
// UIApplicationMain, plus a TcpListener that dispatches incoming JSON
// render requests against our scene/renderer catalog. The iOS Simulator
// shares the host's loopback, so the host-side iOSHostSession just
// connects to 127.0.0.1:<Port> directly — no port forwarding required.
[Register ("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	private const int Port = 7772;

	private UIWindow? _window;
	private TcpListener? _listener;
	private CancellationTokenSource? _cts;

	public override bool FinishedLaunching (UIApplication app, NSDictionary options)
	{
		_window = new UIWindow (UIScreen.MainScreen.Bounds);
		_window.RootViewController = new UIViewController {
			View = { BackgroundColor = UIColor.Black },
		};
		_window.MakeKeyAndVisible ();

		_cts = new CancellationTokenSource ();
		_listener = new TcpListener (IPAddress.Any, Port);
		_listener.Start ();
		_ = AcceptLoopAsync (_cts.Token);
		return true;
	}

	public override void WillTerminate (UIApplication application)
	{
		_cts?.Cancel ();
		try { _listener?.Stop (); } catch { }
	}

	private async Task AcceptLoopAsync (CancellationToken ct)
	{
		while (!ct.IsCancellationRequested) {
			TcpClient client;
			try {
				client = await _listener!.AcceptTcpClientAsync (ct);
			} catch { return; }
			_ = HandleClientAsync (client, ct);
		}
	}

	private async Task HandleClientAsync (TcpClient client, CancellationToken ct)
	{
		using (client)
		using (var stream = client.GetStream ())
		using (var reader = new StreamReader (stream, Encoding.UTF8))
		using (var writer = new StreamWriter (stream, new UTF8Encoding (encoderShouldEmitUTF8Identifier: false)) { AutoFlush = true }) {
			string? line;
			while (!ct.IsCancellationRequested && (line = await reader.ReadLineAsync (ct)) != null) {
				string responseJson;
				try {
					var req = JsonSerializer.Deserialize<RenderRequest> (line);
					var pixels = await RenderAsync (req!.renderer, req.scene, req.w, req.h, ct);
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

	private static async Task<byte[]> RenderAsync (string rendererName, string sceneName, int w, int h, CancellationToken ct)
	{
		var scene = SceneCatalog.Get (sceneName);
		var info = new SKImageInfo (w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
		// Reuse the same IRenderer impls the desktop tests use — their OS
		// gates already pass on iOS, and the Metal P/Invoke targets
		// /System/Library/Frameworks/Metal.framework/Metal which exists on
		// the simulator and on devices. The renderers we expose on the
		// network are renamed to ios-* for clarity in the matrix.
		IRenderer renderer = rendererName switch {
			"ios-raster"         => new RasterRenderer (),
			"ios-ganesh-metal"   => new GaneshMetalRenderer (),
			"ios-graphite-metal" => new GraphiteMetalRenderer (),
			_ => throw new ArgumentException ($"Unknown renderer '{rendererName}'"),
		};
		using (renderer)
			return await renderer.RenderAsync (scene, info, ct);
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
