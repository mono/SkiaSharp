using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Process-singleton transport for the Android render host. Mirrors
	/// <see cref="WasmHostSession"/> in spirit:
	/// <list type="number">
	///   <item>Find <c>adb</c> on the host machine (PATH or ANDROID_HOME).</item>
	///   <item>Confirm a device or emulator is connected.</item>
	///   <item><c>adb install -r</c> the RenderHost.Android APK.</item>
	///   <item><c>adb shell am start</c> the activity.</item>
	///   <item><c>adb forward tcp:N tcp:N</c> the port the activity listens on.</item>
	///   <item>Connect a long-lived TCP socket; one line of JSON per render.</item>
	/// </list>
	/// All Android cells share the single session.
	/// </summary>
	internal sealed class AndroidHostSession : IAsyncDisposable
	{
		private const int Port = 7771;
		private const string PackageId = "com.skiasharp.renderhost";

		private static AndroidHostSession _instance;
		private static readonly SemaphoreSlim _instanceLock = new (1, 1);

		public static async Task<AndroidHostSession> GetAsync (CancellationToken ct)
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
		public string FailureReason { get; private set; }

		private string _adb;
		private TcpClient _tcp;
		private StreamReader _reader;
		private StreamWriter _writer;
		private readonly SemaphoreSlim _ioLock = new (1, 1);

		private static async Task<AndroidHostSession> Create (CancellationToken ct)
		{
			var s = new AndroidHostSession ();
			try {
				s._adb = LocateAdb ();
				if (s._adb == null) {
					s.FailureReason = "adb not found in PATH or ANDROID_HOME/platform-tools — install Android SDK platform-tools";
					return s;
				}

				var devices = await s.RunAdb ("devices", ct);
				if (!devices.Contains ("\tdevice")) {
					s.FailureReason = "no Android device or emulator attached (`adb devices` empty)";
					return s;
				}

				var apk = LocateApk ();
				if (apk == null) {
					s.FailureReason = "RenderHost.Android APK not found — run `dotnet build tests/Hosts/RenderHost.Android -c Release`";
					return s;
				}

				await s.RunAdb ($"install -r \"{apk}\"", ct);
				// `monkey` launches the package's MAIN activity without us
				// having to know the JNI-mangled class name .NET Android
				// generated for [Activity] MainActivity.
				await s.RunAdb ($"shell monkey -p {PackageId} -c android.intent.category.LAUNCHER 1", ct);
				await s.RunAdb ($"forward tcp:{Port} tcp:{Port}", ct);

				// Wait a moment for the activity's TcpListener to be up.
				await Task.Delay (1500, ct);

				s._tcp = new TcpClient ();
				await s._tcp.ConnectAsync ("127.0.0.1", Port, ct);
				var stream = s._tcp.GetStream ();
				s._reader = new StreamReader (stream, Encoding.UTF8);
				s._writer = new StreamWriter (stream, new UTF8Encoding (false)) { AutoFlush = true };

				s.IsAvailable = true;
				return s;
			} catch (Exception ex) {
				s.FailureReason = $"Android host bring-up failed: {ex.GetType ().Name}: {ex.Message}";
				return s;
			}
		}

		public async Task<byte[]> RenderAsync (string rendererName, string sceneName,
			int width, int height, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (FailureReason);

			await _ioLock.WaitAsync (ct);
			try {
				var reqJson = JsonSerializer.Serialize (new {
					renderer = rendererName, scene = sceneName, w = width, h = height,
				});
				await _writer.WriteLineAsync (reqJson.AsMemory (), ct);

				var line = await _reader.ReadLineAsync (ct)
					?? throw new InvalidOperationException ("Android host closed the socket");
				var resp = JsonSerializer.Deserialize<Response> (line)
					?? throw new InvalidOperationException ($"unparseable response: {line}");
				if (!resp.ok)
					throw new InvalidOperationException ($"android render failed: {resp.error}");
				return Convert.FromBase64String (resp.pixels_b64 ?? "");
			} finally {
				_ioLock.Release ();
			}
		}

		private sealed class Response
		{
			public bool ok { get; set; }
			public string pixels_b64 { get; set; }
			public string error { get; set; }
		}

		// ---- adb plumbing ----

		private static string LocateAdb ()
		{
			// PATH first.
			var pathExt = OperatingSystem.IsWindows () ? ".exe" : "";
			foreach (var dir in (Environment.GetEnvironmentVariable ("PATH") ?? "")
					.Split (Path.PathSeparator)) {
				var c = Path.Combine (dir, "adb" + pathExt);
				if (File.Exists (c)) return c;
			}
			// ANDROID_HOME / ANDROID_SDK_ROOT / ~/Library/Android/sdk
			foreach (var root in new[] {
				Environment.GetEnvironmentVariable ("ANDROID_HOME"),
				Environment.GetEnvironmentVariable ("ANDROID_SDK_ROOT"),
				Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile),
					"Library/Android/sdk"),
				Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
					"Android", "Sdk"),
			}) {
				if (string.IsNullOrEmpty (root)) continue;
				var c = Path.Combine (root, "platform-tools", "adb" + pathExt);
				if (File.Exists (c)) return c;
			}
			return null;
		}

		private static string LocateApk ()
		{
			// Walk up from this assembly to find the RenderHost.Android publish output.
			var dir = AppContext.BaseDirectory.TrimEnd (Path.DirectorySeparatorChar);
			for (int i = 0; i < 12 && dir != null; i++) {
				// Standard build layout: bin/Release/net10.0-android*/com.skiasharp.renderhost-Signed.apk
				var hostBin = Path.Combine (dir, "tests", "Hosts", "RenderHost.Android",
					"bin", "Release");
				if (Directory.Exists (hostBin)) {
					foreach (var tfm in Directory.GetDirectories (hostBin)) {
						foreach (var apk in Directory.GetFiles (tfm, "*-Signed.apk")) return apk;
						foreach (var apk in Directory.GetFiles (tfm, "*.apk")) return apk;
					}
				}
				dir = Path.GetDirectoryName (dir);
			}
			return null;
		}

		private async Task<string> RunAdb (string args, CancellationToken ct)
		{
			var psi = new ProcessStartInfo (_adb, args) {
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};
			using var p = Process.Start (psi)
				?? throw new InvalidOperationException ($"adb {args}: failed to start");
			var stdout = await p.StandardOutput.ReadToEndAsync (ct);
			var stderr = await p.StandardError.ReadToEndAsync (ct);
			await p.WaitForExitAsync (ct);
			if (p.ExitCode != 0)
				throw new InvalidOperationException ($"adb {args} exited {p.ExitCode}: {stderr.Trim ()}");
			return stdout;
		}

		public async ValueTask DisposeAsync ()
		{
			try {
				if (_adb != null)
					await RunAdb ($"forward --remove tcp:{Port}", CancellationToken.None);
			} catch { }
			try { _writer?.Dispose (); } catch { }
			try { _reader?.Dispose (); } catch { }
			try { _tcp?.Dispose (); } catch { }
		}
	}
}
