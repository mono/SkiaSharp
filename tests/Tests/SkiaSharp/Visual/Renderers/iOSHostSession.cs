using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Process-singleton transport for the iOS render host. macOS-only:
	/// uses <c>xcrun simctl</c> to install the .app bundle into the booted
	/// iOS simulator and launch it. The simulator shares the host's
	/// loopback, so we connect a long-lived TCP socket to 127.0.0.1:7772
	/// directly — no <c>adb forward</c>-style port plumbing needed.
	/// </summary>
	internal sealed class iOSHostSession : IAsyncDisposable
	{
		private const int Port = 7772;
		private const string BundleId = "com.skiasharp.renderhost.ios";

		private static iOSHostSession? _instance;
		private static readonly SemaphoreSlim _instanceLock = new (1, 1);

		public static async Task<iOSHostSession> GetAsync (CancellationToken ct)
		{
			if (_instance != null) return _instance;
			await _instanceLock.WaitAsync (ct);
			try {
				if (_instance != null) return _instance;
				_instance = await Create (ct);
				return _instance;
			} finally {
				_instanceLock.Release ();
			}
		}

		public bool   IsAvailable   { get; private set; }
		public string? FailureReason { get; private set; }

		private TcpClient? _tcp;
		private StreamReader? _reader;
		private StreamWriter? _writer;
		private readonly SemaphoreSlim _ioLock = new (1, 1);

		private static async Task<iOSHostSession> Create (CancellationToken ct)
		{
			var s = new iOSHostSession ();
			try {
				if (!RuntimeInformation.IsOSPlatform (OSPlatform.OSX)) {
					s.FailureReason = "iOS Simulator requires macOS";
					return s;
				}

				var bootedUdid = await GetBootedSimulator (ct);
				if (bootedUdid == null) {
					s.FailureReason = "no booted iOS simulator (`xcrun simctl list devices booted` empty) — open Simulator.app or run `xcrun simctl boot <device-udid>`";
					return s;
				}

				var appBundle = LocateAppBundle ();
				if (appBundle == null) {
					s.FailureReason = "RenderHost.iOS .app bundle not found — run `dotnet build tests/Hosts/RenderHost.iOS -c Release` on macOS";
					return s;
				}

				await RunXcrun ($"simctl install {bootedUdid} \"{appBundle}\"", ct);
				await RunXcrun ($"simctl launch {bootedUdid} {BundleId}", ct);

				// Give the AppDelegate's TcpListener a moment to come up.
				await Task.Delay (1500, ct);

				s._tcp = new TcpClient ();
				await s._tcp.ConnectAsync ("127.0.0.1", Port, ct);
				var stream = s._tcp.GetStream ();
				s._reader = new StreamReader (stream, Encoding.UTF8);
				s._writer = new StreamWriter (stream, new UTF8Encoding (false)) { AutoFlush = true };

				s.IsAvailable = true;
				return s;
			} catch (Exception ex) {
				s.FailureReason = $"iOS host bring-up failed: {ex.GetType ().Name}: {ex.Message}";
				return s;
			}
		}

		public async Task<byte[]> RenderAsync (string rendererName, string sceneName,
			int width, int height, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new RendererUnavailableException (FailureReason ?? "iOS host unavailable");

			await _ioLock.WaitAsync (ct);
			try {
				var reqJson = JsonSerializer.Serialize (new {
					renderer = rendererName, scene = sceneName, w = width, h = height,
				});
				await _writer!.WriteLineAsync (reqJson.AsMemory (), ct);

				var line = await _reader!.ReadLineAsync (ct)
					?? throw new InvalidOperationException ("iOS host closed the socket");
				var resp = JsonSerializer.Deserialize<Response> (line)
					?? throw new InvalidOperationException ($"unparseable response: {line}");
				if (!resp.ok)
					throw new InvalidOperationException ($"iOS render failed: {resp.error}");
				return Convert.FromBase64String (resp.pixels_b64 ?? "");
			} finally {
				_ioLock.Release ();
			}
		}

		private sealed class Response
		{
			public bool ok { get; set; }
			public string? pixels_b64 { get; set; }
			public string? error { get; set; }
		}

		// ---- simctl helpers ----

		private static async Task<string?> GetBootedSimulator (CancellationToken ct)
		{
			var output = await RunXcrun ("simctl list devices booted", ct);
			// Output format: "    Device-Name (UDID) (Booted)" for each booted simulator.
			foreach (var rawLine in output.Split ('\n')) {
				var line = rawLine.Trim ();
				if (!line.Contains ("(Booted)")) continue;
				var open = line.LastIndexOf ('(', line.IndexOf (") (Booted)"));
				var close = line.IndexOf (')', open);
				if (open < 0 || close < 0) continue;
				return line.Substring (open + 1, close - open - 1);
			}
			return null;
		}

		private static string? LocateAppBundle ()
		{
			// Walk up from this assembly to find the RenderHost.iOS .app bundle.
			// Typical layout: bin/Release/net10.0-ios<v>/iossimulator-<rid>/RenderHost.iOS.app
			var dir = AppContext.BaseDirectory.TrimEnd (Path.DirectorySeparatorChar);
			for (int i = 0; i < 12 && dir != null; i++) {
				var hostBin = Path.Combine (dir, "tests", "Hosts", "RenderHost.iOS", "bin", "Release");
				if (Directory.Exists (hostBin)) {
					foreach (var tfm in Directory.GetDirectories (hostBin)) {
						foreach (var rid in Directory.GetDirectories (tfm)) {
							foreach (var app in Directory.GetDirectories (rid, "*.app")) return app;
						}
					}
				}
				dir = Path.GetDirectoryName (dir);
			}
			return null;
		}

		private static async Task<string> RunXcrun (string args, CancellationToken ct)
		{
			var psi = new ProcessStartInfo ("xcrun", args) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			using var p = Process.Start (psi)
				?? throw new InvalidOperationException ($"xcrun {args}: failed to start");
			var stdout = await p.StandardOutput.ReadToEndAsync (ct);
			var stderr = await p.StandardError.ReadToEndAsync (ct);
			await p.WaitForExitAsync (ct);
			if (p.ExitCode != 0)
				throw new InvalidOperationException ($"xcrun {args} exited {p.ExitCode}: {stderr.Trim ()}");
			return stdout;
		}

		public async ValueTask DisposeAsync ()
		{
			try { _writer?.Dispose (); } catch { }
			try { _reader?.Dispose (); } catch { }
			try { _tcp?.Dispose (); } catch { }
			await Task.CompletedTask;
		}
	}
}
