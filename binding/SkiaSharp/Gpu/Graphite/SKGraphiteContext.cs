#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>
	/// Root object for the Graphite GPU backend. Vends recorders, accepts and submits recordings,
	/// and owns GPU resources. Single-thread-affine — do not share across threads.
	/// Must outlive every <see cref="SKGraphiteRecorder"/>, <see cref="SKGraphiteRecording"/>,
	/// and Graphite-backed <see cref="SKSurface"/> derived from it.
	/// </summary>
	public unsafe class SKGraphiteContext : SKObject
	{
		// Pinned managed delegate the Vulkan dispatch lambda calls back into. Ownership is transferred
		// from the SKGraphiteVkBackendContext at CreateVulkan time so callers can safely Dispose
		// the backend context immediately afterwards. Freed in DisposeNative AFTER the native context
		// is deleted (which tears down the lambda).
		private GCHandle pinnedBackendDelegate;

		// Set in CreateDawn when the backend context detected a non-yielding (WASM/browser)
		// environment. Submit(Sync=true) is rejected up-front for these contexts because Dawn
		// cannot pump its event loop from inside a managed call frame and the Skia-side wait
		// would deadlock — see SKGraphiteDawnBackendContext remarks.
		private bool isNonYielding;

		internal SKGraphiteContext (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		internal void AttachPinnedBackendDelegate (GCHandle gch)
		{
			// Only ever set once, immediately after construction by a backend factory.
			pinnedBackendDelegate = gch;
		}

		/// <summary>
		/// Returns true if the requested Graphite backend was compiled into this build of libSkiaSharp.
		/// Safe to call without first creating a context.
		/// </summary>
		public static bool IsBackendAvailable (SKGraphiteBackend backend) =>
			SkiaApi.sk_graphite_backend_is_available (backend);

		/// <summary>
		/// Create a Graphite context for the Vulkan backend. Returns null when Vulkan/Graphite is
		/// not built into libSkiaSharp, when the backend context is invalid, or when the underlying
		/// driver rejected the create call. Use <see cref="IsBackendAvailable"/> to disambiguate.
		///
		/// The caller may safely <see cref="IDisposable.Dispose"/> <paramref name="backendContext"/>
		/// immediately after this returns; its Vulkan handles (instance/device/queue) remain owned
		/// by the caller for the lifetime of the returned context.
		/// </summary>
		public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext) =>
			CreateVulkan (backendContext, default);

		/// <summary>
		/// Create a Graphite context for the Metal backend (macOS / iOS only).
		/// Returns null when Metal/Graphite is not built into libSkiaSharp,
		/// when the backend context is invalid, or when the underlying driver
		/// rejected the create call.
		///
		/// The caller may drop <paramref name="backendContext"/> at any time after this returns;
		/// the MTLDevice/MTLCommandQueue handles remain owned by the caller for the lifetime
		/// of the returned context.
		/// </summary>
		public static SKGraphiteContext CreateMetal (SKGraphiteMtlBackendContext backendContext) =>
			CreateMetal (backendContext, default);

		/// <summary>
		/// Create a Graphite context for the Dawn (WebGPU) backend. Returns null
		/// when Dawn/Graphite is not built into libSkiaSharp, when the backend
		/// context is invalid, or when Dawn rejected the create call.
		///
		/// The caller may drop <paramref name="backendContext"/> at any time after this returns;
		/// the WGPUInstance/Device/Queue handles remain owned by the caller for the lifetime of
		/// the returned context.
		/// </summary>
		public static SKGraphiteContext CreateDawn (SKGraphiteDawnBackendContext backendContext) =>
			CreateDawn (backendContext, default);

		/// <inheritdoc cref="CreateDawn(SKGraphiteDawnBackendContext)"/>
		public static SKGraphiteContext CreateDawn (SKGraphiteDawnBackendContext backendContext, SKGraphiteContextOptions options)
		{
			if (backendContext is null)
				throw new ArgumentNullException (nameof (backendContext));
			ValidateOptions (options);

			var init = backendContext.ToNative ();
			IntPtr handle = SkiaApi.sk_graphite_context_make_dawn (&init, &options);
			if (handle == IntPtr.Zero)
				return null;

			return new SKGraphiteContext (handle, true) {
				isNonYielding = backendContext.IsNonYielding,
			};
		}

		/// <inheritdoc cref="CreateMetal(SKGraphiteMtlBackendContext)"/>
		public static SKGraphiteContext CreateMetal (SKGraphiteMtlBackendContext backendContext, SKGraphiteContextOptions options)
		{
			if (backendContext is null)
				throw new ArgumentNullException (nameof (backendContext));
			ValidateOptions (options);

			var init = backendContext.ToNative ();
			IntPtr handle = SkiaApi.sk_graphite_context_make_metal (&init, &options);
			if (handle == IntPtr.Zero)
				return null;

			return new SKGraphiteContext (handle, true);
		}

		/// <inheritdoc cref="CreateVulkan(SKGraphiteVkBackendContext)"/>
		public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext, SKGraphiteContextOptions options)
		{
			if (backendContext is null)
				throw new ArgumentNullException (nameof (backendContext));
			ValidateOptions (options);

			var init = backendContext.ToNative ();
			IntPtr handle = SkiaApi.sk_graphite_context_make_vulkan (&init, &options);
			if (handle == IntPtr.Zero)
				return null;

			var ctx = new SKGraphiteContext (handle, true);

			// Variant A — transfer GCHandle ownership. The Skia context's Vulkan dispatch
			// lambda captured the function pointer + userData by value; SKGraphiteContext
			// is now responsible for keeping the underlying managed delegate alive.
			ctx.AttachPinnedBackendDelegate (backendContext.TransferGetProcHandle ());

			return ctx;
		}

		// Reject options values the native shim's validator would refuse so the
		// caller sees ArgumentException with a clear message instead of a null
		// from sk_graphite_context_make_*. fInternalMultisampleCount == 0 is the
		// documented "use Skia default" sentinel — matches default(SKGraphiteContextOptions).
		private static void ValidateOptions (in SKGraphiteContextOptions options)
		{
			var msaa = options.InternalMultisampleCount;
			if (msaa != 0 && msaa != 1 && msaa != 2 && msaa != 4 && msaa != 8 && msaa != 16) {
				throw new ArgumentException (
					$"InternalMultisampleCount must be 0 (use Skia default) or one of 1, 2, 4, 8, 16. Got {msaa}.",
					nameof (options));
			}
		}

		protected override void DisposeNative ()
		{
			SkiaApi.sk_graphite_context_delete (Handle);
			// Free AFTER the native context tears down its lambda — the lambda may dispatch
			// during destruction (rare, but possible if Skia drains queues).
			if (pinnedBackendDelegate.IsAllocated) {
				pinnedBackendDelegate.Free ();
				pinnedBackendDelegate = default;
			}
		}

		// Properties

		public SKGraphiteBackend Backend =>
			SkiaApi.sk_graphite_context_get_backend (Handle);

		public bool IsDeviceLost =>
			SkiaApi.sk_graphite_context_is_device_lost (Handle);

		public int MaxTextureSize =>
			SkiaApi.sk_graphite_context_get_max_texture_size (Handle);

		public bool SupportsProtectedContent =>
			SkiaApi.sk_graphite_context_supports_protected_content (Handle);

		public long CurrentBudgetedBytes =>
			SkiaApi.sk_graphite_context_get_current_budgeted_bytes (Handle);

		public long MaxBudgetedBytes {
			get => SkiaApi.sk_graphite_context_get_max_budgeted_bytes (Handle);
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value), value, "Must be non-negative.");
				SkiaApi.sk_graphite_context_set_max_budgeted_bytes (Handle, value);
			}
		}

		// Recording

		/// <summary>
		/// Vend a fresh recorder. Each recorder is single-thread-affine; multiple recorders may
		/// be used concurrently on different threads against the same context.
		/// </summary>
		public SKGraphiteRecorder CreateRecorder (long recorderBudgetBytes = -1) =>
			CreateRecorder (recorderBudgetBytes, imageProvider: null);

		/// <summary>
		/// Vend a recorder with a managed <see cref="SKGraphiteImageProvider"/> attached.
		/// Without one, Graphite drops every draw whose source <see cref="SKImage"/> is not
		/// already Graphite-backed (it does not auto-upload like Ganesh). Use
		/// <see cref="SKGraphiteImageProvider.CreateDefault"/> for an upload-with-LRU-cache
		/// policy, or subclass <see cref="SKGraphiteImageProvider"/> for custom caching.
		///
		/// On a successful return the recorder takes ownership of <paramref name="imageProvider"/>;
		/// calling <see cref="IDisposable.Dispose"/> on it becomes a no-op. Do NOT share a single
		/// <see cref="SKGraphiteImageProvider"/> instance across multiple recorders — construct
		/// a fresh provider per recorder.
		/// </summary>
		public SKGraphiteRecorder CreateRecorder (long recorderBudgetBytes, SKGraphiteImageProvider imageProvider)
		{
			IntPtr ipHandle = imageProvider?.Handle ?? IntPtr.Zero;
			IntPtr handle = SkiaApi.sk_graphite_context_make_recorder (Handle, recorderBudgetBytes, ipHandle);
			if (handle == IntPtr.Zero)
				return null;

			var rec = new SKGraphiteRecorder (handle, true);
			// Recorder took ownership of the native wrapper; transfer to the C#
			// recorder so the pinned managed delegate (referenced by Skia via the
			// captured function-pointer + userData) outlives the native recorder.
			rec.AttachImageProvider (imageProvider);
			return rec;
		}

		/// <summary>
		/// Submit a recording for execution. Returns the status verbatim from the underlying
		/// Graphite engine — non-success values are not exceptions; callers branch on the enum.
		/// </summary>
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording)
		{
			if (recording is null)
				throw new ArgumentNullException (nameof (recording));

			var info = new SKGraphiteInsertRecordingInfo {
				Recording = recording.Handle,
			};
			return SkiaApi.sk_graphite_context_insert_recording (Handle, &info);
		}

		/// <inheritdoc cref="InsertRecording(SKGraphiteRecording)"/>
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteInsertRecordingInfo info) =>
			SkiaApi.sk_graphite_context_insert_recording (Handle, &info);

		/// <summary>
		/// Submit pending GPU work. Returns false if submission failed.
		/// </summary>
		public bool Submit () =>
			SkiaApi.sk_graphite_context_submit (Handle, null);

		/// <summary>
		/// Submit pending GPU work with explicit options. Returns false if submission failed.
		///
		/// <para>Setting <see cref="SKGraphiteSubmitInfo.Sync"/> to true blocks the calling
		/// thread until the GPU has finished. Not supported on contexts created in a
		/// non-yielding (browser/WASM) environment — calling Submit with <c>Sync = true</c>
		/// on such a context throws <see cref="InvalidOperationException"/>. Drive readbacks
		/// with <see cref="CheckAsyncWorkCompletion"/> instead.</para>
		/// </summary>
		public bool Submit (SKGraphiteSubmitInfo submitInfo)
		{
			if (submitInfo.Sync && isNonYielding)
				throw new InvalidOperationException (
					"SKGraphiteSubmitInfo.Sync = true is not supported in this environment. " +
					"Submit without sync and drive readbacks via CheckAsyncWorkCompletion instead.");
			return SkiaApi.sk_graphite_context_submit (Handle, &submitInfo);
		}

		// Resource management

		public void FreeGpuResources () =>
			SkiaApi.sk_graphite_context_free_gpu_resources (Handle);

		public void PerformDeferredCleanup (TimeSpan duration)
		{
			if (duration < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException (nameof (duration), duration, "Must be non-negative.");
			SkiaApi.sk_graphite_context_perform_deferred_cleanup (Handle, (long)duration.TotalMilliseconds);
		}

		/// <summary>
		/// Schedule a backend texture (created by <see cref="SKGraphiteRecorder.CreateBackendTexture"/>
		/// or wrapped from a caller-allocated GPU resource) for release through the context.
		/// Equivalent to <see cref="SKGraphiteRecorder.DeleteBackendTexture"/>; use whichever
		/// matches your code's existing recorder/context split.
		/// </summary>
		public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture)
		{
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));
			SkiaApi.sk_graphite_context_delete_backend_texture (Handle, backendTexture.Handle);
		}

		/// <summary>
		/// Drives any pending async readback or finished-callback work. Callbacks fire on the
		/// thread that calls this method.
		/// </summary>
		public void CheckAsyncWorkCompletion () =>
			SkiaApi.sk_graphite_context_check_async_work_completion (Handle);

		/// <summary>
		/// Synchronous pixel readback from a Graphite-backed surface. Use this in place of
		/// <see cref="SKSurface.ReadPixels(SKImageInfo, IntPtr, int, int, int)"/>, which is
		/// unavailable for Graphite-backed surfaces in production builds.
		///
		/// THREADING: Blocks the calling thread until the GPU readback completes (cost
		/// roughly equivalent to glFinish). Don't call from a render thread that needs to
		/// keep doing GPU work in parallel.
		/// </summary>
		/// <param name="dstPixels">Caller-owned buffer at least <paramref name="dstRowBytes"/> *
		///   <paramref name="dstInfo"/>.Height bytes. RGBA_8888/Premul are supported on every backend;
		///   format conversions follow Skia's standard rules.</param>
		/// <param name="dstRowBytes">Stride in bytes. May be tighter than the source's natural row.</param>
		public bool ReadPixels (SKSurface surface, SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY)
		{
			if (surface is null) throw new ArgumentNullException (nameof (surface));
			if (dstPixels == IntPtr.Zero) throw new ArgumentNullException (nameof (dstPixels));
			if (dstRowBytes <= 0) throw new ArgumentOutOfRangeException (nameof (dstRowBytes));

			bool done = false;
			bool success = false;
			int height = dstInfo.Height;

			Action<IntPtr> handler = result => {
				try {
					if (result == IntPtr.Zero)
						return;
					int count = SkiaApi.sk_graphite_async_read_result_get_count (result);
					if (count == 0)
						return;
					var src = SkiaApi.sk_graphite_async_read_result_get_data (result, 0);
					if (src == null)
						return;
					int srcRowBytes = (int)SkiaApi.sk_graphite_async_read_result_get_row_bytes (result, 0);
					int copy = Math.Min (srcRowBytes, dstRowBytes);
					byte* srcBytes = (byte*)src;
					byte* dstBytes = (byte*)dstPixels;
					for (int y = 0; y < height; y++) {
						Buffer.MemoryCopy (
							srcBytes + (long)y * srcRowBytes,
							dstBytes + (long)y * dstRowBytes,
							dstRowBytes,
							copy);
					}
					success = true;
				} finally {
					done = true;
				}
			};

			DelegateProxies.Create (handler, out _, out var ctxPtr);

			var nativeInfo = SKImageInfoNative.FromManaged (ref dstInfo);
			var srcRect = new SKRectI (srcX, srcY, srcX + dstInfo.Width, srcY + dstInfo.Height);

			SkiaApi.sk_graphite_context_async_rescale_and_read_pixels_surface (
				Handle, surface.Handle, &nativeInfo, &srcRect,
				SKGraphiteRescaleGamma.Src,
				SKGraphiteRescaleMode.RepeatedLinear,
				DelegateProxies.SKGraphiteAsyncReadPixelsProxy,
				(void*)ctxPtr);

			// Drive completion. Submit non-syncing first so any pending recordings are flushed,
			// then spin on checkAsyncWorkCompletion. The proxy frees its GCHandle inside its
			// finally — `done` is set just before that, so we observe it correctly here.
			Submit ();
			while (!done) {
				SkiaApi.sk_graphite_context_check_async_work_completion (Handle);
			}

			return success;
		}
	}
}
