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

		/// <summary>True if the requested Graphite backend was compiled into this build of libSkiaSharp.</summary>
		public static bool IsBackendAvailable (SKGraphiteBackend backend) =>
			SkiaApi.sk_graphite_backend_is_available (backend);

		/// <summary>
		/// Create a Graphite context for the Vulkan backend. Returns null when the backend isn't
		/// available (see <see cref="IsBackendAvailable"/>) or when the driver rejected the call.
		/// The caller may dispose <paramref name="backendContext"/> as soon as this returns.
		/// </summary>
		public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext) =>
			CreateVulkan (backendContext, default);

		/// <summary>
		/// Create a Graphite context for the Metal backend (macOS / iOS only). Returns null when
		/// the backend isn't available (see <see cref="IsBackendAvailable"/>) or when the driver
		/// rejected the call. The caller may drop <paramref name="backendContext"/> as soon as
		/// this returns.
		/// </summary>
		public static SKGraphiteContext CreateMetal (SKGraphiteMtlBackendContext backendContext) =>
			CreateMetal (backendContext, default);

		/// <summary>
		/// Create a Graphite context for the Dawn (WebGPU) backend. Returns null when the backend
		/// isn't available (see <see cref="IsBackendAvailable"/>) or when Dawn rejected the call.
		/// The caller may drop <paramref name="backendContext"/> as soon as this returns.
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
			CreateRecorder (recorderBudgetBytes, findOrCreate: null, findOrCreateDispose: null);

		/// <summary>
		/// Vend a recorder with a managed image-upload callback attached. Without one, Graphite
		/// drops every draw whose source <see cref="SKImage"/> is not already Graphite-backed
		/// (it does not auto-upload like Ganesh). For a ready-made LRU policy pass
		/// <c>cache.FindOrCreate</c> + <c>cache.Dispose</c> from a new
		/// <see cref="SKGraphiteImageCache"/>.
		///
		/// <paramref name="findOrCreateDispose"/>, if supplied, runs at recorder teardown
		/// <em>before</em> the native recorder is destroyed — use it to release any
		/// graphite-backed images the callback's closure captured (releasing them after
		/// destruction is undefined behavior).
		/// </summary>
		public SKGraphiteRecorder CreateRecorder (
			long recorderBudgetBytes,
			SKGraphiteFindOrCreateImageDelegate findOrCreate,
			Action findOrCreateDispose = null)
		{
			IntPtr providerHandle = IntPtr.Zero;
			GCHandle pinnedCallback = default;

			if (findOrCreate != null) {
				SKGraphiteFindOrCreateImageProxy proxy = (rh, ih, mipmapped) =>
					InvokeFindOrCreate (findOrCreate, rh, ih, mipmapped);
				DelegateProxies.Create (proxy, out pinnedCallback, out var ctx);
#if USE_LIBRARY_IMPORT
				providerHandle = SkiaApi.sk_graphite_image_provider_new (
					(delegate* unmanaged[Cdecl] <void*, IntPtr, IntPtr, Int32, IntPtr>)
						DelegateProxies.SKGraphiteImageProviderProxy,
					(void*)ctx);
#else
				providerHandle = SkiaApi.sk_graphite_image_provider_new (
					DelegateProxies.SKGraphiteImageProviderProxy,
					(void*)ctx);
#endif
				if (providerHandle == IntPtr.Zero) {
					pinnedCallback.Free ();
					throw new InvalidOperationException ("sk_graphite_image_provider_new failed (Graphite not built into libSkiaSharp?)");
				}
			}

			IntPtr handle = SkiaApi.sk_graphite_context_make_recorder (Handle, recorderBudgetBytes, providerHandle);

			// makeRecorder copied our sp out; the wrapper can be freed regardless of success.
			if (providerHandle != IntPtr.Zero)
				SkiaApi.sk_graphite_image_provider_delete (providerHandle);

			if (handle == IntPtr.Zero) {
				if (pinnedCallback.IsAllocated) pinnedCallback.Free ();
				return null;
			}

			var rec = new SKGraphiteRecorder (handle, true);
			if (findOrCreate != null)
				rec.AttachImageCallback (pinnedCallback, findOrCreateDispose);
			return rec;
		}

		private static IntPtr InvokeFindOrCreate (
			SKGraphiteFindOrCreateImageDelegate callback,
			IntPtr recorderHandle, IntPtr imageHandle, bool mipmapped)
		{
			if (recorderHandle == IntPtr.Zero || imageHandle == IntPtr.Zero)
				return IntPtr.Zero;
			// Wrap the handles in non-owning managed views. unrefExisting:false because
			// these are borrowed handles owned by Skia for the duration of this call —
			// decrementing on dispose would crash later.
			var recorder = SKObject.GetOrAddObject<SKGraphiteRecorder> (recorderHandle, false, false, (h, o) => new SKGraphiteRecorder (h, o));
			var image    = SKObject.GetOrAddObject<SKImage>            (imageHandle,    false, false, (h, o) => new SKImage (h, o));
			var result   = callback (recorder, image, mipmapped);
			if (result == null) return IntPtr.Zero;
			// Skia consumes the +1 ref on `result`. Detach the managed wrapper so Dispose
			// doesn't decrement the reference Skia just took.
			var raw = result.Handle;
			result.RevokeOwnership (null);
			return raw;
		}

		/// <summary>
		/// Submit a recording for execution. Non-success values are returned verbatim,
		/// not thrown — callers branch on the enum.
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
		/// <para><see cref="SKGraphiteSubmitInfo.Sync"/> = true blocks until the GPU is done.
		/// Throws <see cref="InvalidOperationException"/> on contexts created in a non-yielding
		/// (browser/WASM) environment; use <see cref="CheckAsyncWorkCompletion"/> instead.</para>
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
		/// Schedule a backend texture for release. Equivalent to
		/// <see cref="SKGraphiteRecorder.DeleteBackendTexture"/>.
		/// </summary>
		public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture)
		{
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));
			SkiaApi.sk_graphite_context_delete_backend_texture (Handle, backendTexture.Handle);
		}

		/// <summary>
		/// Drive pending async readback / finished callbacks. Callbacks fire on the calling thread.
		/// </summary>
		public void CheckAsyncWorkCompletion () =>
			SkiaApi.sk_graphite_context_check_async_work_completion (Handle);

		/// <summary>
		/// Kick off an async pixel readback. <paramref name="callback"/> fires on whichever thread
		/// next drives completion via <see cref="CheckAsyncWorkCompletion"/> (or
		/// <see cref="Submit(SKGraphiteSubmitInfo)"/> with <c>Sync = true</c>), with <c>null</c>
		/// on failure. The caller drives completion themselves — this method does not block.
		/// </summary>
		/// <param name="dstInfo">Target pixel format. RGBA_8888/Premul is supported on every backend.</param>
		/// <param name="srcRect">Region of <paramref name="surface"/> to read.</param>
		public void RequestReadPixels (
			SKSurface surface,
			SKImageInfo dstInfo,
			SKRectI srcRect,
			SKGraphiteRescaleGamma rescaleGamma,
			SKGraphiteRescaleMode rescaleMode,
			Action<SKGraphiteAsyncReadResult> callback)
		{
			if (surface is null) throw new ArgumentNullException (nameof (surface));
			if (callback is null) throw new ArgumentNullException (nameof (callback));

			Action<IntPtr> handler = raw => {
				using var result = raw == IntPtr.Zero ? null : new SKGraphiteAsyncReadResult (raw);
				callback (result);
			};

			DelegateProxies.Create (handler, out _, out var ctxPtr);

			var nativeInfo = SKImageInfoNative.FromManaged (ref dstInfo);
			SkiaApi.sk_graphite_context_async_rescale_and_read_pixels_surface (
				Handle, surface.Handle, &nativeInfo, &srcRect,
				rescaleGamma, rescaleMode,
				DelegateProxies.SKGraphiteAsyncReadPixelsProxy,
				(void*)ctxPtr);
		}

		/// <inheritdoc cref="RequestReadPixels(SKSurface, SKImageInfo, SKRectI, SKGraphiteRescaleGamma, SKGraphiteRescaleMode, Action{SKGraphiteAsyncReadResult})"/>
		public void RequestReadPixels (
			SKSurface surface,
			SKImageInfo dstInfo,
			SKRectI srcRect,
			Action<SKGraphiteAsyncReadResult> callback) =>
			RequestReadPixels (surface, dstInfo, srcRect,
				SKGraphiteRescaleGamma.Src, SKGraphiteRescaleMode.RepeatedLinear, callback);
	}
}
