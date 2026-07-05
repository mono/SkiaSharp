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

	// All Graphite/Dawn JS interop. The .NET 10 WASM SDK wraps dotnet.native.js
	// in a -sMODULARIZE IIFE, so Emscripten's `Module` is hidden from external
	// JS — but JSHost.DotnetInstance hands us a JSObject handle from inside the
	// IIFE. At static-ctor time we walk Module → WebGPU → mgr{Device,Queue,
	// Texture} via JSObject property reads and cache the tables. Method calls
	// are dispatched through a single variadic helper on globalThis
	// (`__skiaCallMember`), which is the smallest globalThis footprint
	// [JSImport] still permits (it binds a free function but not a method on a
	// JSObject). A handful of helpers that need JS object/array literals
	// (descriptors) stay as named entries on `skiaSharpWebGpu`.
	internal static partial class SKWebGpu
	{
		[JSImport("globalThis.eval")]
		private static partial void Eval(string expr);

		[JSImport("globalThis.__skiaCallMember")]
		private static partial int CallInt(JSObject obj, string member, JSObject arg);

		[JSImport("globalThis.__skiaCallMember")]
		private static partial void CallVoid(JSObject obj, string member, int arg);

		[JSImport("globalThis.__skiaCallMember")]
		private static partial JSObject CallObj(JSObject obj, string member);

		[JSImport("globalThis.__skiaCallMember")]
		private static partial Task<JSObject> CallObjAsync(JSObject obj, string member);

		[JSImport("globalThis.__skiaCallMember")]
		private static partial Task CallVoidAsync(JSObject obj, string member, int arg);

		private static readonly JSObject s_mgrDevice;
		private static readonly JSObject s_mgrQueue;
		private static readonly JSObject s_mgrTexture;

		static SKWebGpu()
		{
			var module = JSHost.DotnetInstance.GetPropertyAsJSObject("Module")
				?? throw new InvalidOperationException("JSHost.DotnetInstance.Module unavailable — incompatible .NET WASM runtime.");
			var webgpu = module.GetPropertyAsJSObject("WebGPU")
				?? throw new InvalidOperationException("Module.WebGPU missing — EXPORTED_RUNTIME_METHODS lacks 'WebGPU'.");
			s_mgrDevice = webgpu.GetPropertyAsJSObject("mgrDevice");
			s_mgrQueue = webgpu.GetPropertyAsJSObject("mgrQueue");
			s_mgrTexture = webgpu.GetPropertyAsJSObject("mgrTexture");

			Eval(@"
				globalThis.__skiaCallMember = (o, m, ...a) => o[m](...a);
				globalThis.skiaSharpWebGpu = {
					requestAdapter: () => navigator.gpu && navigator.gpu.requestAdapter({ powerPreference: 'low-power' }),
					createInstance: () => (typeof _wgpuCreateInstance === 'function') ? _wgpuCreateInstance(0) : 1,
					createTexture: (d, w, h) => d.createTexture({
						size: { width: w, height: h, depthOrArrayLayers: 1 },
						format: 'rgba8unorm',
						usage: 0x10 | 0x01,
					}),
					createBuffer: (d, sz) => d.createBuffer({ size: sz, usage: 0x09 }),
					copyTextureToBuffer: (e, tex, buf, bpr, w, h) => e.copyTextureToBuffer(
						{ texture: tex },
						{ buffer: buf, bytesPerRow: bpr, rowsPerImage: h },
						{ width: w, height: h, depthOrArrayLayers: 1 }),
					submitEncoder: (d, e) => d.queue.submit([e.finish()]),
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

		internal static int RegisterDevice(JSObject device) => CallInt(s_mgrDevice, "create", device);
		internal static int RegisterQueue(JSObject queue) => CallInt(s_mgrQueue, "create", queue);
		internal static int RegisterTexture(JSObject texture) => CallInt(s_mgrTexture, "create", texture);
		internal static void ReleaseTexture(int textureId) => CallVoid(s_mgrTexture, "release", textureId);

		internal static Task<JSObject> RequestDevice(JSObject adapter) => CallObjAsync(adapter, "requestDevice");
		internal static JSObject CreateCommandEncoder(JSObject device) => CallObj(device, "createCommandEncoder");
		internal static Task MapBufferReadAsync(JSObject buffer) => CallVoidAsync(buffer, "mapAsync", 0x01);

		internal static JSObject GetDeviceQueue(JSObject device) => device.GetPropertyAsJSObject("queue");

		[JSImport("globalThis.skiaSharpWebGpu.requestAdapter")]
		internal static partial Task<JSObject> RequestAdapter();

		[JSImport("globalThis.skiaSharpWebGpu.createInstance")]
		internal static partial int CreateInstance();

		[JSImport("globalThis.skiaSharpWebGpu.createTexture")]
		internal static partial JSObject CreateTexture(JSObject device, int width, int height);

		[JSImport("globalThis.skiaSharpWebGpu.createBuffer")]
		internal static partial JSObject CreateBuffer(JSObject device, int size);

		[JSImport("globalThis.skiaSharpWebGpu.copyTextureToBuffer")]
		internal static partial void CopyTextureToBuffer(JSObject encoder, JSObject texture, JSObject buffer, int bytesPerRow, int width, int height);

		[JSImport("globalThis.skiaSharpWebGpu.submitEncoder")]
		internal static partial void SubmitEncoder(JSObject device, JSObject encoder);

		[JSImport("globalThis.skiaSharpWebGpu.getMappedBase64")]
		internal static partial string GetMappedBase64(JSObject buffer, int bytesPerRow, int width, int height);
	}
}
#endif
