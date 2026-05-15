using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Tests.Visual;

namespace SkiaSharp.Tests.RenderHost.Wasm;

// In-page renderers, one per backend. Caller passes a scene + size; we
// return raw RGBA8888/Premul pixels.
//
// raster        — pure CPU, always works
// graphite-dawn — WebGPU via Skia Graphite, no canvas: we acquire an
//                 adapter/device, build an SKGraphiteDawnBackendContext
//                 from numeric handles registered with the Emscripten
//                 $WebGPU manager tables, render into a GPUTexture we
//                 created ourselves, and read it back asynchronously.
internal static partial class WasmRenderers
{
	public static byte[] RenderRaster (ISkiaScene scene, SKImageInfo info)
	{
		using var surface = SKSurface.Create (info)
			?? throw new InvalidOperationException ("SKSurface.Create returned null for raster");
		scene.Draw (surface.Canvas);
		return ReadRgbaPremul (surface, info);
	}

	// Lazy: created on first call, kept for the WASM process lifetime.
	// emscripten exposes only one "current" GL context at a time per
	// thread, so MakeCurrent before every render keeps us sticky against
	// any other (hypothetical) GL consumers in the same module.
	private static int s_glContextHandle;
	private static bool s_glReady;

	public static Task<byte[]> RenderGaneshGlAsync (ISkiaScene scene, SKImageInfo info)
	{
		if (!s_glReady) {
			s_glContextHandle = SKWebGl.InitOffscreenContext ();
			if (s_glContextHandle == 0)
				throw new InvalidOperationException (
					"Module.GL.registerContext returned 0 — OffscreenCanvas/WebGL2 unavailable");
			s_glReady = true;
		}
		if (SKWebGl.MakeCurrent (s_glContextHandle) == 0)
			throw new InvalidOperationException (
				$"Module.GL.makeContextCurrent({s_glContextHandle}) failed");

		using var glInterface = GRGlInterface.Create ()
			?? throw new InvalidOperationException ("GRGlInterface.Create returned null");
		using var ctx = GRContext.CreateGl (glInterface)
			?? throw new InvalidOperationException ("GRContext.CreateGl returned null");
		using var surface = SKSurface.Create (ctx, budgeted: true, info)
			?? throw new InvalidOperationException ("SKSurface.Create returned null on Ganesh/WebGL2");

		scene.Draw (surface.Canvas);
		ctx.Flush ();
		// WebGL is synchronous: queued commands run on the GPU process and
		// glReadPixels blocks until they're done. No mapAsync dance needed
		// (unlike WebGPU); we can read pixels straight back from C#.
		ctx.Submit (synchronous: true);

		var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
		var pixels = new byte[rgba.BytesSize];
		unsafe {
			fixed (byte* p = pixels) {
				if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
					throw new InvalidOperationException ("SKSurface.ReadPixels failed on Ganesh/WebGL2");
			}
		}
		return Task.FromResult (pixels);
	}

	// Non-yielding mode disallows SKGraphiteContext.Dispose() while any GPU
	// work is in flight — that path asserts ("all GPU work must be finished
	// before destroying Context") and in WASM the assert becomes a fatal
	// `unreachable` trap. Bring up the Context + Recorder once per session,
	// leave them alive for the lifetime of the WASM process, and only cycle
	// the per-render Surface + backend texture. Process-exit means the Skia
	// warning never fires anyway.
	private static SKGraphiteContext s_ctx;
	private static SKGraphiteRecorder s_recorder;
	private static JSObject s_offscreenDevice;
	private static bool s_dawnReady;

	public static async Task<byte[]> RenderGraphiteDawnAsync (ISkiaScene scene, SKImageInfo info)
	{
		if (!s_dawnReady) {
			var adapter = await SKWebGpu.RequestAdapter ()
				?? throw new InvalidOperationException ("navigator.gpu.requestAdapter returned null — WebGPU unavailable");
			var device = await SKWebGpu.RequestDevice (adapter)
				?? throw new InvalidOperationException ("adapter.requestDevice returned null");
			s_offscreenDevice = device;

			var queue = SKWebGpu.GetDeviceQueue (device);
			int queueId  = SKWebGpu.RegisterQueue (queue);
			int deviceId = SKWebGpu.RegisterDevice (device);
			// emscripten 3.1.56 has no mgrInstance — wgpuCreateInstance returns
			// a hard-coded value. Newer Emscripten added a real table. Skia's
			// DawnBackendContext stores fInstance opaquely, any non-null number works.
			int instanceId = SKWebGpu.CreateInstance ();

			var bc = new SKGraphiteDawnBackendContext {
				WgpuInstance = (IntPtr)instanceId,
				WgpuDevice   = (IntPtr)deviceId,
				WgpuQueue    = (IntPtr)queueId,
			};
			s_ctx = SKGraphiteContext.CreateDawn (bc)
				?? throw new InvalidOperationException ("SKGraphiteContext.CreateDawn returned null");
			s_recorder = s_ctx.CreateRecorder ()
				?? throw new InvalidOperationException ("CreateRecorder returned null");
			s_dawnReady = true;
		}

		// Create our own GPUTexture, wrap it as a Graphite BackendTexture,
		// render into it, then read it back asynchronously. We CAN'T use
		// SKGraphiteContext.RequestReadPixels here: in non-yielding mode it
		// would deadlock on a mapAsync that needs the JS event loop to tick.
		// JS-side readback keeps the GPU→CPU copy on the async path and
		// never blocks a C# stack on JS work.
		var texture = SKWebGpu.CreateTexture (s_offscreenDevice, info.Width, info.Height);
		int textureId = SKWebGpu.RegisterTexture (texture);
		try {
			using var backendTex = SKGraphiteBackendTexture.CreateDawn ((IntPtr)textureId)
				?? throw new InvalidOperationException ("SKGraphiteBackendTexture.CreateDawn returned null");
			using var surface = SKSurface.Create (s_recorder, backendTex, SKColorType.Rgba8888)
				?? throw new InvalidOperationException ("SKSurface.Create returned null on Graphite/Dawn");

			scene.Draw (surface.Canvas);

			using (var recording = s_recorder.Snap ()
				?? throw new InvalidOperationException ("Recorder.Snap returned null")) {
				var status = s_ctx.InsertRecording (recording);
				if (status != SKGraphiteInsertStatus.Success)
					throw new InvalidOperationException ($"InsertRecording status = {status}");
			}
			s_ctx.Submit (new SKGraphiteSubmitInfo { Sync = false });

			// WebGPU spec: bytesPerRow on copyTextureToBuffer must be a multiple of 256.
			int bytesPerRow = ((info.Width * 4 + 255) / 256) * 256;
			int bufSize = bytesPerRow * info.Height;
			var buffer = SKWebGpu.CreateBuffer (s_offscreenDevice, bufSize);
			var encoder = SKWebGpu.CreateCommandEncoder (s_offscreenDevice);
			SKWebGpu.CopyTextureToBuffer (encoder, texture, buffer, bytesPerRow, info.Width, info.Height);
			SKWebGpu.SubmitEncoder (s_offscreenDevice, encoder);
			await SKWebGpu.MapBufferReadAsync (buffer);

			var b64 = SKWebGpu.GetMappedBase64 (buffer, bytesPerRow, info.Width, info.Height);
			return Convert.FromBase64String (b64);
		} finally {
			SKWebGpu.ReleaseTexture (textureId);
		}
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

	internal static partial class JsBridge
	{
		[JSImport ("globalThis.skiaSharpWebGpu.initOffscreenAsync")]
		internal static partial Task<JSObject?> InitWebGpuOffscreenAsync ();

		[JSImport ("globalThis.skiaSharpWebGpu.createOffscreenTexture")]
		internal static partial int CreateOffscreenTexture (int width, int height);

		[JSImport ("globalThis.skiaSharpWebGpu.readTextureRgbaAsync")]
		internal static partial Task<string?> ReadTextureRgbaAsync (int textureId, int width, int height);

		[JSImport ("globalThis.skiaSharpWebGpu.releaseOffscreenTexture")]
		internal static partial void ReleaseOffscreenTexture (int textureId);
	}

	// Offscreen-WebGL2 bring-up. emscripten's `$GL` runtime registers GL
	// contexts under integer handles that the gl* shims dispatch through.
	// We don't reach $GL via the link-time merge-into mechanism here — we
	// rely on `EmccExportedRuntimeMethod Include="GL"` exposing it as
	// `Module.GL`, walk that JSObject from C# via JSHost.DotnetInstance,
	// then dispatch the two calls we need (registerContext, makeContext-
	// Current) through a tiny eval'd helper that owns the OffscreenCanvas +
	// webgl2-attrs construction (those are JS object literals C# can't
	// build directly).
	internal static partial class SKWebGl
	{
		[JSImport ("globalThis.eval")]
		private static partial void Eval (string expr);

		[JSImport ("globalThis.skiaSharpWebGl.initOffscreen")]
		private static partial int InitOffscreenImpl (JSObject gl);

		[JSImport ("globalThis.skiaSharpWebGl.makeCurrent")]
		private static partial int MakeCurrentImpl (JSObject gl, int handle);

		private static readonly JSObject s_gl;

		static SKWebGl ()
		{
			var module = JSHost.DotnetInstance.GetPropertyAsJSObject ("Module")
				?? throw new InvalidOperationException ("JSHost.DotnetInstance.Module unavailable — incompatible .NET WASM runtime.");
			s_gl = module.GetPropertyAsJSObject ("GL")
				?? throw new InvalidOperationException ("Module.GL missing — EXPORTED_RUNTIME_METHODS lacks 'GL'.");

			Eval (@"
				globalThis.skiaSharpWebGl = {
					initOffscreen: gl => {
						if (typeof OffscreenCanvas === 'undefined') return 0;
						const c = new OffscreenCanvas(1, 1);
						const ctx = c.getContext('webgl2', {
							alpha: true, depth: false, stencil: true,
							antialias: false, premultipliedAlpha: true,
							preserveDrawingBuffer: false, powerPreference: 'low-power',
						});
						if (!ctx) return 0;
						// majorVersion: 2 tells emscripten's $GL shim to wire up the
						// WebGL2 entrypoints (glGetStringi, glDrawArraysInstanced, …)
						// that Ganesh checks for to enable GLES3 features.
						return gl.registerContext(ctx, { majorVersion: 2 }) | 0;
					},
					makeCurrent: (gl, handle) => gl.makeContextCurrent(handle) ? 1 : 0,
				};
			");
		}

		internal static int InitOffscreenContext () => InitOffscreenImpl (s_gl);
		internal static int MakeCurrent (int handle) => MakeCurrentImpl (s_gl, handle);
	}

	// All Graphite/Dawn orchestration lives in C#. The bridge JS does
	// exactly one thing: publish Emscripten's Module onto globalThis so
	// `globalThis.skiaSharpModule.WebGPU.mgr*` is reachable via [JSImport]
	// from out here. Everything else — method calls on JSObject instances
	// (`adapter.requestDevice`, `device.createTexture`, `buf.mapAsync`,
	// etc.) — is bridged by a small set of helper closures we install via
	// `eval` once at static-ctor time, then bind via [JSImport].
	internal static partial class SKWebGpu
	{
		[JSImport ("globalThis.eval")]
		private static partial void Eval (string expr);

		static SKWebGpu ()
		{
			// One-time install of method-dispatch helpers. They exist only to
			// turn `obj.method(...)` calls into top-level functions reachable
			// by dotted path — that's the shape [JSImport] can bind. Each is
			// a one-liner; the only complex one is GetMappedBase64, which
			// fuses the repack-out-of-padded-rows step with the base64 pack.
			Eval (@"
				globalThis.skiaSharpWebGpu = {
					requestAdapter: () => navigator.gpu && navigator.gpu.requestAdapter({ powerPreference: 'low-power' }),
					requestDevice: a => a.requestDevice(),
					deviceQueue: d => d.queue,
					createInstance: () => (typeof _wgpuCreateInstance === 'function') ? _wgpuCreateInstance(0) : 1,
					createTexture: (d, w, h) => d.createTexture({
						size: { width: w, height: h, depthOrArrayLayers: 1 },
						format: 'rgba8unorm',
						usage: 0x10 | 0x01, // RENDER_ATTACHMENT | COPY_SRC
					}),
					createBuffer: (d, sz) => d.createBuffer({ size: sz, usage: 0x09 /* COPY_DST | MAP_READ */ }),
					createCommandEncoder: d => d.createCommandEncoder(),
					copyTextureToBuffer: (e, tex, buf, bpr, w, h) => e.copyTextureToBuffer(
						{ texture: tex },
						{ buffer: buf, bytesPerRow: bpr, rowsPerImage: h },
						{ width: w, height: h, depthOrArrayLayers: 1 }),
					submitEncoder: (d, e) => d.queue.submit([e.finish()]),
					mapAsync: b => b.mapAsync(0x01 /* GPUMapMode.READ */),
					getMappedBase64: (b, bpr, w, h) => {
						const mapped = new Uint8Array(b.getMappedRange());
						const widthBytes = w * 4;
						const packed = new Uint8Array(widthBytes * h);
						for (let r = 0; r < h; r++)
							packed.set(mapped.subarray(r * bpr, r * bpr + widthBytes), r * widthBytes);
						b.unmap();
						b.destroy();
						let s = '';
						const CHUNK = 0x8000;
						for (let i = 0; i < packed.length; i += CHUNK)
							s += String.fromCharCode.apply(null, packed.subarray(i, i + CHUNK));
						return btoa(s);
					},
				};
			");
		}

		// Free dotted-path functions — no helper needed.
		[JSImport ("globalThis.skiaSharpWebGpu.requestAdapter")]
		internal static partial Task<JSObject?> RequestAdapter ();

		[JSImport ("globalThis.skiaSharpModule.WebGPU.mgrDevice.create")]
		internal static partial int RegisterDevice (JSObject device);

		[JSImport ("globalThis.skiaSharpModule.WebGPU.mgrQueue.create")]
		internal static partial int RegisterQueue (JSObject queue);

		[JSImport ("globalThis.skiaSharpModule.WebGPU.mgrTexture.create")]
		internal static partial int RegisterTexture (JSObject texture);

		[JSImport ("globalThis.skiaSharpModule.WebGPU.mgrTexture.release")]
		internal static partial void ReleaseTexture (int textureId);

		// Method-on-JSObject calls dispatched via eval'd helpers.
		[JSImport ("globalThis.skiaSharpWebGpu.requestDevice")]
		internal static partial Task<JSObject?> RequestDevice (JSObject adapter);

		[JSImport ("globalThis.skiaSharpWebGpu.deviceQueue")]
		internal static partial JSObject GetDeviceQueue (JSObject device);

		[JSImport ("globalThis.skiaSharpWebGpu.createInstance")]
		internal static partial int CreateInstance ();

		[JSImport ("globalThis.skiaSharpWebGpu.createTexture")]
		internal static partial JSObject CreateTexture (JSObject device, int width, int height);

		[JSImport ("globalThis.skiaSharpWebGpu.createBuffer")]
		internal static partial JSObject CreateBuffer (JSObject device, int size);

		[JSImport ("globalThis.skiaSharpWebGpu.createCommandEncoder")]
		internal static partial JSObject CreateCommandEncoder (JSObject device);

		[JSImport ("globalThis.skiaSharpWebGpu.copyTextureToBuffer")]
		internal static partial void CopyTextureToBuffer (JSObject encoder, JSObject texture, JSObject buffer, int bytesPerRow, int width, int height);

		[JSImport ("globalThis.skiaSharpWebGpu.submitEncoder")]
		internal static partial void SubmitEncoder (JSObject device, JSObject encoder);

		[JSImport ("globalThis.skiaSharpWebGpu.mapAsync")]
		internal static partial Task MapBufferReadAsync (JSObject buffer);

		[JSImport ("globalThis.skiaSharpWebGpu.getMappedBase64")]
		internal static partial string GetMappedBase64 (JSObject buffer, int bytesPerRow, int width, int height);
	}
}
