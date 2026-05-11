using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Process-singleton transport for the WASM render host:
	/// <list type="bullet">
	///   <item>Tiny <see cref="HttpListener"/> that serves the published wwwroot</item>
	///   <item>Chromium tab via Microsoft.Playwright with WebGPU + SwiftShader flags</item>
	///   <item>Marshalling helpers to call <c>globalThis.skiaTestHost.renderScene(...)</c>
	///         via <c>page.evaluateAsync</c> and return RGBA bytes</item>
	/// </list>
	/// All three WASM renderers share a single browser + page — the navigate cost
	/// (loading dotnet runtime + SkiaSharp WASM, ~12 MB) is paid once per test session.
	/// </summary>
	internal sealed class WasmHostSession : IAsyncDisposable
	{
		private static WasmHostSession _instance;
		private static readonly SemaphoreSlim _instanceLock = new SemaphoreSlim (1, 1);

		// Lazy: bring up the host on first RenderAsync, share across renderers.
		public static async Task<WasmHostSession> GetAsync (CancellationToken ct)
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

		public string FailureReason { get; private set; }
		public bool   IsAvailable   { get; private set; }

		private IPlaywright _pw;
		private IBrowser _browser;
		private IPage _page;
		private HttpListener _http;
		private CancellationTokenSource _serverCts;
		private int _port;

		private static async Task<WasmHostSession> Create (CancellationToken ct)
		{
			var session = new WasmHostSession ();
			try {
				var wwwroot = LocateWwwroot ();
				if (wwwroot == null) {
					session.FailureReason = "RenderHost.Wasm publish output not found; expected publish at tests/Hosts/RenderHost.Wasm/bin/Release/net10.0/publish/wwwroot";
					return session;
				}
				session.StartHttpServer (wwwroot);

				// Playwright lazy-installs browsers under ~/.cache/ms-playwright.
				// First time may be slow; subsequent runs are instant.
				Microsoft.Playwright.Program.Main (new[] { "install", "chromium" });

				session._pw = await Playwright.CreateAsync ();
				session._browser = await session._pw.Chromium.LaunchAsync (new BrowserTypeLaunchOptions {
					Headless = true,
					Args = new[] {
						"--enable-unsafe-webgpu",
						"--enable-features=Vulkan",
						"--use-vulkan=swiftshader",
						"--use-angle=swiftshader",
						"--no-sandbox",
					},
				});
				var ctx = await session._browser.NewContextAsync (new BrowserNewContextOptions {
					ViewportSize = new ViewportSize { Width = 64, Height = 64 },
				});
				session._page = await ctx.NewPageAsync ();
				// Capture page-side diagnostics into FailureReason if init
				// doesn't complete; helps diagnose runtime boot or JS failures.
				var consoleLog = new System.Text.StringBuilder ();
				session._page.Console += (_, m) => {
					if (m.Type == "error" || m.Type == "warning")
						consoleLog.AppendLine ($"[page {m.Type}] {m.Text}");
				};
				session._page.PageError += (_, err) => consoleLog.AppendLine ($"[pageerror] {err}");
				await session._page.GotoAsync ($"http://127.0.0.1:{session._port}/index.html",
					new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 60000 });
				try {
					await session._page.WaitForFunctionAsync (
						"() => globalThis.skiaTestHost && globalThis.skiaTestHost.ready === true",
						null, new PageWaitForFunctionOptions { Timeout = 60000 });
				} catch (TimeoutException) {
					var snippet = consoleLog.Length == 0 ? "<no page errors>" : consoleLog.ToString ();
					throw new TimeoutException ($"WASM payload never set globalThis.skiaTestHost.ready. Page diagnostics:\n{snippet}");
				}
				session.IsAvailable = true;
				return session;
			} catch (Exception ex) {
				session.FailureReason = $"WASM host bring-up failed: {ex.GetType ().Name}: {ex.Message}";
				return session;
			}
		}

		public async Task<byte[]> RenderAsync (string rendererName, string sceneName,
			int width, int height, CancellationToken ct)
		{
			if (!IsAvailable)
				throw new InvalidOperationException (FailureReason);

			var b64 = await _page.EvaluateAsync<string> (
				@"async ([r, s, w, h]) => await globalThis.skiaTestHost.renderScene(r, s, w, h)",
				new object[] { rendererName, sceneName, width, height });
			if (string.IsNullOrEmpty (b64))
				throw new InvalidOperationException ($"WASM host returned empty pixels for {rendererName}/{sceneName}");
			return Convert.FromBase64String (b64);
		}

		// ---- Tiny HTTP server (PUBLISH-only, no caching, no security) ----

		private static readonly System.Collections.Generic.Dictionary<string, string> MimeTypes = new () {
			{ ".html", "text/html" },
			{ ".js",   "application/javascript" },
			{ ".mjs",  "application/javascript" },
			{ ".wasm", "application/wasm" },
			{ ".json", "application/json" },
			{ ".webmanifest", "application/manifest+json" },
			{ ".css",  "text/css" },
			{ ".png",  "image/png" },
			{ ".svg",  "image/svg+xml" },
			{ ".dll",  "application/octet-stream" },
			{ ".pdb",  "application/octet-stream" },
			{ ".dat",  "application/octet-stream" },
			{ ".br",   "application/octet-stream" },
			{ ".gz",   "application/octet-stream" },
		};

		private void StartHttpServer (string wwwroot)
		{
			_port = GetFreePort ();
			_http = new HttpListener ();
			_http.Prefixes.Add ($"http://127.0.0.1:{_port}/");
			_http.Start ();
			_serverCts = new CancellationTokenSource ();
			_ = Task.Run (() => ServeLoop (wwwroot, _serverCts.Token));
		}

		private async Task ServeLoop (string wwwroot, CancellationToken token)
		{
			while (!token.IsCancellationRequested) {
				HttpListenerContext ctx;
				try { ctx = await _http.GetContextAsync (); }
				catch { return; }
				_ = Task.Run (() => HandleRequest (ctx, wwwroot));
			}
		}

		private static void HandleRequest (HttpListenerContext ctx, string wwwroot)
		{
			try {
				var pathInfo = ctx.Request.Url.AbsolutePath;
				if (pathInfo == "/" || string.IsNullOrEmpty (pathInfo)) pathInfo = "/index.html";
				var full = Path.Combine (wwwroot, pathInfo.TrimStart ('/'));
				if (!File.Exists (full)) {
					ctx.Response.StatusCode = 404;
					ctx.Response.Close ();
					return;
				}
				var bytes = File.ReadAllBytes (full);
				ctx.Response.ContentType = MimeTypes.TryGetValue (Path.GetExtension (full), out var mt)
					? mt : "application/octet-stream";
				// Required for SharedArrayBuffer / threaded WASM; harmless otherwise.
				ctx.Response.Headers["Cross-Origin-Opener-Policy"]   = "same-origin";
				ctx.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
				ctx.Response.Headers["Cache-Control"]                = "no-store";
				ctx.Response.ContentLength64 = bytes.Length;
				ctx.Response.OutputStream.Write (bytes, 0, bytes.Length);
				ctx.Response.Close ();
			} catch { /* swallow — test transport */ }
		}

		private static int GetFreePort ()
		{
			using var l = new System.Net.Sockets.TcpListener (System.Net.IPAddress.Loopback, 0);
			l.Start ();
			var port = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
			l.Stop ();
			return port;
		}

		private static string LocateWwwroot ()
		{
			// Walk up from the test assembly directory to find the repo root,
			// then check the published payload.
			var dir = Path.GetDirectoryName (typeof (WasmHostSession).Assembly.Location);
			for (int i = 0; i < 12 && dir != null; i++) {
				var candidate = Path.Combine (dir, "tests", "Hosts", "RenderHost.Wasm",
					"bin", "Release", "net10.0", "publish", "wwwroot");
				if (Directory.Exists (candidate)) return candidate;
				dir = Path.GetDirectoryName (dir);
			}
			return null;
		}

		public async ValueTask DisposeAsync ()
		{
			_serverCts?.Cancel ();
			try { _http?.Stop (); } catch { }
			if (_browser != null)
				await _browser.CloseAsync ();
			_pw?.Dispose ();
		}
	}
}
