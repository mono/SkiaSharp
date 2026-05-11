using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	// One IRenderer per in-page WASM renderer. All three share a single
	// browser session via WasmHostSession; the renderer name acts as the
	// dispatch key on the JS side. Availability gates the whole row of
	// cells in the matrix — if Playwright / Chromium can't start, every
	// wasm-* cell skips with the same reason.

	public abstract class WasmRendererBase : IRenderer
	{
		public abstract string Name { get; }
		public abstract RendererCapabilities Caps { get; }

		// Cached lazily on first access. Bringing up Playwright is heavy
		// (~3-10 s); we pay it once per test session.
		private static volatile WasmHostSession _session;
		private static readonly SemaphoreSlim _initLock = new SemaphoreSlim (1, 1);

		public bool IsAvailable
		{
			get {
				// Cheap probe: just check that the WASM publish output exists
				// next to the test assembly. Actually launching Playwright happens
				// on first RenderAsync (so test-discovery doesn't pay it).
				return WasmHostSessionExists ();
			}
		}

		public string UnavailableReason =>
			WasmHostSessionExists () ? null
				: "RenderHost.Wasm publish output missing — run `dotnet publish tests/Hosts/RenderHost.Wasm -c Release`";

		public async Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			var session = await EnsureSession (ct);
			return await session.RenderAsync (Name, scene.Name, info.Width, info.Height, ct);
		}

		private static async Task<WasmHostSession> EnsureSession (CancellationToken ct)
		{
			if (_session != null) {
				if (!_session.IsAvailable)
					throw new InvalidOperationException (_session.FailureReason);
				return _session;
			}
			await _initLock.WaitAsync (ct);
			try {
				if (_session == null)
					_session = await WasmHostSession.GetAsync (ct);
				if (!_session.IsAvailable)
					throw new InvalidOperationException (_session.FailureReason);
				return _session;
			} finally {
				_initLock.Release ();
			}
		}

		private static bool WasmHostSessionExists ()
		{
			// We don't actually probe Playwright here (it's expensive). Just
			// check that the published wwwroot is on disk — if it's not,
			// every wasm-* cell is going to skip anyway, and we can give a
			// helpful unavailability reason without paying browser-launch cost.
			var dir = System.IO.Path.GetDirectoryName (typeof (WasmRendererBase).Assembly.Location);
			for (int i = 0; i < 12 && dir != null; i++) {
				var p = System.IO.Path.Combine (dir, "tests", "Hosts", "RenderHost.Wasm",
					"bin", "Release", "net10.0", "publish", "wwwroot", "index.html");
				if (System.IO.File.Exists (p)) return true;
				dir = System.IO.Path.GetDirectoryName (dir);
			}
			return false;
		}

		public void Dispose () { /* session is process-singleton; teardown at AppDomain unload */ }
	}

	public sealed class WasmRasterRenderer : WasmRendererBase
	{
		public override string Name => "wasm-raster";
		public override RendererCapabilities Caps => RendererCapabilities.Cpu;
	}

	public sealed class WasmGraphiteDawnRenderer : WasmRendererBase
	{
		public override string Name => "wasm-graphite-dawn";
		public override RendererCapabilities Caps => RendererCapabilities.Gpu;
	}

	// Ganesh-WebGL2 host-side renderer NOT registered yet — the in-page
	// implementation needs an emscripten_webgl_create_context + make_current
	// JS-library shim; tracked as a follow-up. When that lands, add the
	// renderer here:
	// public sealed class WasmGaneshGlEsRenderer : WasmRendererBase {
	//     public override string Name => "wasm-ganesh-gles";
	//     public override RendererCapabilities Caps => RendererCapabilities.Gpu;
	// }
}
