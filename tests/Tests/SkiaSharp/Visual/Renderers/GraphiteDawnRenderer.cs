#if !NETFRAMEWORK
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Graphite GPU backend over Dawn (WebGPU), for the Blazor WASM host.
	/// Only reports available when running in a browser environment; every
	/// other host skips the cell. Bring-up walks
	/// <c>JSHost.DotnetInstance → Module → WebGPU → mgr{Device,Queue,Texture}</c>
	/// (the Emscripten-internal manager tables that vend integer handles for
	/// WGPU* objects across the C ABI) and dispatches JS method calls through a
	/// single variadic helper installed on <c>globalThis.__skiaCallMember</c>.
	///
	/// <para>
	/// Not compiled on <c>net48</c> (JSHost is unavailable there); the base
	/// test assembly's Console host will not see this renderer.
	/// </para>
	/// </summary>
	public sealed class GraphiteDawnRenderer : IRenderer
	{
		public string Name => "graphite-dawn";

		public bool IsAvailable =>
#if NET5_0_OR_GREATER
			OperatingSystem.IsBrowser();
#else
			false;
#endif

		public string UnavailableReason =>
			IsAvailable ? null : "graphite-dawn requires a WebGPU-capable browser host.";

		// Non-yielding mode disallows SKGraphiteContext.Dispose() while any GPU
		// work is in flight; keep the Context + Recorder alive for the WASM
		// process lifetime, cycle only the per-render Surface + backend texture.
		private static SKGraphiteContext s_ctx;
		private static SKGraphiteRecorder s_recorder;
		private static JSObject s_offscreenDevice;
		private static bool s_dawnReady;

		public async Task<byte[]> RenderAsync(ISkiaScene scene, SKImageInfo info, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsAvailable)
				throw new RendererUnavailableException(UnavailableReason);

			if (!s_dawnReady)
			{
				// SKWebGpu's static ctor walks JSHost.DotnetInstance → Module → WebGPU →
				// mgr{Device,Queue,Texture}. Any failure there (Module.WebGPU missing
				// because EXPORTED_RUNTIME_METHODS lacks 'WebGPU', browser without
				// WebGPU support, etc.) becomes a TypeInitializationException on the
				// first SKWebGpu call. Convert it to an unavailable signal so the
				// matrix skips the cell cleanly instead of failing hard.
				try
				{
					var adapter = await SKWebGpu.RequestAdapter()
						?? throw new RendererUnavailableException("navigator.gpu.requestAdapter returned null — WebGPU unavailable.");
					var device = await SKWebGpu.RequestDevice(adapter)
						?? throw new RendererUnavailableException("adapter.requestDevice returned null.");
					s_offscreenDevice = device;

					var queue = SKWebGpu.GetDeviceQueue(device);
					int queueId = SKWebGpu.RegisterQueue(queue);
					int deviceId = SKWebGpu.RegisterDevice(device);
					// Emscripten 3.1.56 has no mgrInstance — wgpuCreateInstance returns
					// a hard-coded value. Skia's DawnBackendContext stores fInstance
					// opaquely, so any non-null value works.
					int instanceId = SKWebGpu.CreateInstance();

					var bc = new SKGraphiteDawnBackendContext
					{
						WgpuInstance = (IntPtr)instanceId,
						WgpuDevice = (IntPtr)deviceId,
						WgpuQueue = (IntPtr)queueId,
					};
					s_ctx = SKGraphiteContext.CreateDawn(bc)
						?? throw new InvalidOperationException("SKGraphiteContext.CreateDawn returned null.");
					s_recorder = s_ctx.CreateRecorder()
						?? throw new InvalidOperationException("SKGraphiteContext.CreateRecorder returned null.");
					s_dawnReady = true;
				}
				catch (TypeInitializationException ex)
				{
					throw new RendererUnavailableException(
						$"WebGPU host bring-up failed: {ex.InnerException?.Message ?? ex.Message}", ex);
				}
			}

			var texture = SKWebGpu.CreateTexture(s_offscreenDevice, info.Width, info.Height);
			int textureId = SKWebGpu.RegisterTexture(texture);
			try
			{
				using var backendTex = SKGraphiteBackendTexture.CreateDawn((IntPtr)textureId)
					?? throw new InvalidOperationException("SKGraphiteBackendTexture.CreateDawn returned null.");
				using var surface = SKSurface.Create(s_recorder, backendTex, SKColorType.Rgba8888)
					?? throw new InvalidOperationException("SKSurface.Create returned null on Graphite/Dawn.");

				scene.Draw(surface.Canvas);

				using (var recording = s_recorder.Snap()
					?? throw new InvalidOperationException("Recorder.Snap() returned null."))
				{
					if (s_ctx.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
						throw new InvalidOperationException("InsertRecording did not report Success.");
				}
				s_ctx.Submit(new SKGraphiteSubmitInfo { Sync = false });

				// WebGPU spec: copyTextureToBuffer.bytesPerRow must be a multiple of 256.
				int bytesPerRow = ((info.Width * 4 + 255) / 256) * 256;
				int bufSize = bytesPerRow * info.Height;
				var buffer = SKWebGpu.CreateBuffer(s_offscreenDevice, bufSize);
				var encoder = SKWebGpu.CreateCommandEncoder(s_offscreenDevice);
				SKWebGpu.CopyTextureToBuffer(encoder, texture, buffer, bytesPerRow, info.Width, info.Height);
				SKWebGpu.SubmitEncoder(s_offscreenDevice, encoder);
				await SKWebGpu.MapBufferReadAsync(buffer);

				var b64 = SKWebGpu.GetMappedBase64(buffer, bytesPerRow, info.Width, info.Height);
				return Convert.FromBase64String(b64);
			}
			finally
			{
				SKWebGpu.ReleaseTexture(textureId);
			}
		}

		public void Dispose()
		{
		}
	}

	// All Graphite/Dawn JS interop. The .NET WASM SDK wraps dotnet.native.js
	// in a -sMODULARIZE IIFE, so Emscripten's `Module` is hidden from external
	// JS — but JSHost.DotnetInstance hands us a JSObject handle from inside the
	// IIFE. At static-ctor time we install helpers on globalThis and dispatch
	// through them; that's the smallest globalThis footprint [JSImport] still
	// permits (it binds free functions but not methods on a JSObject).
	//
	// The helpers accept native `Module.WebGPU` handles under either port:
	//
	//   * emsdk 3.1.34 with -sUSE_WEBGPU=1 (net8):  Module.WebGPU exposes
	//     mgr{Device,Queue,Texture} HandleAllocator tables. Registering an
	//     object gets an int handle back; releasing calls .release on the
	//     table.
	//   * emsdk 3.1.56 with Dawn's emdawnwebgpu port (net9+): the tables are
	//     gone; Module.WebGPU exposes importJs{Device,Queue,Texture} which
	//     allocate a real refcounted WGPU* handle via _emwgpuCreate*.
	//     Release goes through the C ABI's wgpuTextureRelease
	//     (Module._wgpuTextureRelease — exported by IncludeNativeAssets.SkiaSharp.targets
	//     under the emdawnwebgpu path).
	//
	// Since the same test assembly runs against both ports, the JS helpers
	// probe which surface is present and dispatch accordingly.
	internal static partial class SKWebGpu
	{
		[JSImport("globalThis.eval")]
		private static partial void Eval(string expr);

		static SKWebGpu()
		{
			// Fail fast at static-ctor time if we're not inside a .NET WASM
			// runtime that exposes Module.WebGPU — the JS helpers below all
			// dereference it.
			var module = JSHost.DotnetInstance.GetPropertyAsJSObject("Module")
				?? throw new InvalidOperationException("JSHost.DotnetInstance.Module unavailable — incompatible .NET WASM runtime.");
			_ = module.GetPropertyAsJSObject("WebGPU")
				?? throw new InvalidOperationException("Module.WebGPU missing — EXPORTED_RUNTIME_METHODS lacks 'WebGPU'.");

			Eval(@"
				globalThis.skiaSharpWebGpu = {
					requestAdapter: () => navigator.gpu && navigator.gpu.requestAdapter({ powerPreference: 'low-power' }),
					createInstance: () => (typeof _wgpuCreateInstance === 'function') ? _wgpuCreateInstance(0) : 1,
					// Port-agnostic handle registration. emdawnwebgpu ships
					// importJs* on Module.WebGPU; the legacy -sUSE_WEBGPU=1
					// port shipped mgr* HandleAllocator tables with .create.
					registerDevice: (d) => Module.WebGPU.importJsDevice
						? Module.WebGPU.importJsDevice(d)
						: Module.WebGPU.mgrDevice.create(d),
					registerQueue: (q) => Module.WebGPU.importJsQueue
						? Module.WebGPU.importJsQueue(q)
						: Module.WebGPU.mgrQueue.create(q),
					registerTexture: (t) => Module.WebGPU.importJsTexture
						? Module.WebGPU.importJsTexture(t)
						: Module.WebGPU.mgrTexture.create(t),
					// Under emdawnwebgpu, released handles hold real
					// refcounted C-side WGPUTexture objects — call the C ABI
					// via the exported symbol. Under the legacy port they
					// were HandleAllocator table entries with a JS-side
					// .release. Try the C ABI first (it's the mandatory
					// path under emdawnwebgpu), fall back to the JS table.
					releaseTexture: (id) => {
						if (typeof Module._wgpuTextureRelease === 'function') {
							Module._wgpuTextureRelease(id);
						} else if (Module.WebGPU.mgrTexture) {
							Module.WebGPU.mgrTexture.release(id);
						}
					},
					requestDevice: (adapter) => adapter.requestDevice(),
					deviceQueue: (d) => d.queue,
					createTexture: (d, w, h) => d.createTexture({
						size: { width: w, height: h, depthOrArrayLayers: 1 },
						format: 'rgba8unorm',
						usage: 0x10 | 0x01,
					}),
					createBuffer: (d, sz) => d.createBuffer({ size: sz, usage: 0x09 }),
					createCommandEncoder: (d) => d.createCommandEncoder(),
					copyTextureToBuffer: (e, tex, buf, bpr, w, h) => e.copyTextureToBuffer(
						{ texture: tex },
						{ buffer: buf, bytesPerRow: bpr, rowsPerImage: h },
						{ width: w, height: h, depthOrArrayLayers: 1 }),
					submitEncoder: (d, e) => d.queue.submit([e.finish()]),
					mapBufferRead: (b) => b.mapAsync(0x01),
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

		[JSImport("globalThis.skiaSharpWebGpu.requestAdapter")]
		internal static partial Task<JSObject> RequestAdapter();

		[JSImport("globalThis.skiaSharpWebGpu.requestDevice")]
		internal static partial Task<JSObject> RequestDevice(JSObject adapter);

		[JSImport("globalThis.skiaSharpWebGpu.deviceQueue")]
		internal static partial JSObject GetDeviceQueue(JSObject device);

		[JSImport("globalThis.skiaSharpWebGpu.registerDevice")]
		internal static partial int RegisterDevice(JSObject device);

		[JSImport("globalThis.skiaSharpWebGpu.registerQueue")]
		internal static partial int RegisterQueue(JSObject queue);

		[JSImport("globalThis.skiaSharpWebGpu.registerTexture")]
		internal static partial int RegisterTexture(JSObject texture);

		[JSImport("globalThis.skiaSharpWebGpu.releaseTexture")]
		internal static partial void ReleaseTexture(int textureId);

		[JSImport("globalThis.skiaSharpWebGpu.createInstance")]
		internal static partial int CreateInstance();

		[JSImport("globalThis.skiaSharpWebGpu.createTexture")]
		internal static partial JSObject CreateTexture(JSObject device, int width, int height);

		[JSImport("globalThis.skiaSharpWebGpu.createBuffer")]
		internal static partial JSObject CreateBuffer(JSObject device, int size);

		[JSImport("globalThis.skiaSharpWebGpu.createCommandEncoder")]
		internal static partial JSObject CreateCommandEncoder(JSObject device);

		[JSImport("globalThis.skiaSharpWebGpu.copyTextureToBuffer")]
		internal static partial void CopyTextureToBuffer(JSObject encoder, JSObject texture, JSObject buffer, int bytesPerRow, int width, int height);

		[JSImport("globalThis.skiaSharpWebGpu.submitEncoder")]
		internal static partial void SubmitEncoder(JSObject device, JSObject encoder);

		[JSImport("globalThis.skiaSharpWebGpu.mapBufferRead")]
		internal static partial Task MapBufferReadAsync(JSObject buffer);

		[JSImport("globalThis.skiaSharpWebGpu.getMappedBase64")]
		internal static partial string GetMappedBase64(JSObject buffer, int bytesPerRow, int width, int height);
	}
}
#endif
