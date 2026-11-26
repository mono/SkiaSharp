#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>Represents a Graphite GPU context that owns backend resources and executes recordings against a Dawn, Metal, or Vulkan backend.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Create a context from a backend-specific backend context, for example <xref:SkiaSharp.SKGraphiteContext.CreateVulkan(SkiaSharp.SKGraphiteVkBackendContext)>, <xref:SkiaSharp.SKGraphiteContext.CreateMetal(SkiaSharp.SKGraphiteMtlBackendContext)>, or <xref:SkiaSharp.SKGraphiteContext.CreateDawn(SkiaSharp.SKGraphiteDawnBackendContext)>. Create one or more recorders with <xref:SkiaSharp.SKGraphiteContext.CreateRecorder(System.Int64)>, draw into Graphite-backed surfaces, snap the work into recordings, insert them into the context, and submit.
	///
	/// This type wraps a native Skia resource and implements `IDisposable`. Dispose it after all its recorders and surfaces have been disposed.
	///
	/// ## Examples
	///
	/// ```csharp
	/// using var context = SKGraphiteContext.CreateMetal(backendContext);
	/// using var recorder = context.CreateRecorder();
	///
	/// var info = new SKImageInfo(256, 256);
	/// using var surface = SKSurface.Create(recorder, info);
	/// surface.Canvas.Clear(SKColors.CornflowerBlue);
	///
	/// using var recording = recorder.Snap();
	/// context.InsertRecording(recording);
	/// context.Submit();
	/// ```
	/// ]]></format></remarks>
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

		/// <summary>Determines whether the specified Graphite backend is available in this build of SkiaSharp.</summary>
		/// <param name="backend">One of the enumeration values that specifies the backend to check.</param>
		/// <returns><see langword="true" /> if the backend is available; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool IsBackendAvailable (SKGraphiteBackend backend) =>
			SkiaApi.sk_graphite_backend_is_available (backend);

		// default(SKGraphiteContextOptions) zero-initialises every field, but a 0
		// GpuBudgetInBytes is an *explicit* 0-byte GPU resource cache, not "use Skia's
		// default" — the C shim's sentinel for the latter is a negative value (see
		// sk_graphite.h: "-1 to use Skia's default"). Left as default, the no-options
		// factories would silently disable Skia's 256 MB resource cache and thrash. Seed
		// -1 so they get Skia's real default budget. (InternalMultisampleCount 0 is
		// already the "use Skia default" sentinel and the bool fields match Skia's
		// release defaults, so only the budget needs seeding here.)
		private static readonly SKGraphiteContextOptions DefaultOptions = new () { GpuBudgetInBytes = -1 };

		/// <summary>Creates a Vulkan-backed Graphite context using the default options.</summary>
		/// <param name="backendContext">The Vulkan backend context that supplies the device and function loader.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteContext" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext) =>
			CreateVulkan (backendContext, DefaultOptions);

		/// <summary>Creates a Metal-backed Graphite context using the default options.</summary>
		/// <param name="backendContext">The Metal backend context that supplies the device and queue.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteContext" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKGraphiteContext CreateMetal (SKGraphiteMtlBackendContext backendContext) =>
			CreateMetal (backendContext, DefaultOptions);

		/// <summary>Creates a Dawn (WebGPU) backed Graphite context using the default options.</summary>
		/// <param name="backendContext">The Dawn backend context that supplies the instance, device, and queue.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteContext" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKGraphiteContext CreateDawn (SKGraphiteDawnBackendContext backendContext) =>
			CreateDawn (backendContext, DefaultOptions);

		/// <summary>Creates a Dawn (WebGPU) backed Graphite context using the specified options.</summary>
		/// <param name="backendContext">The Dawn backend context that supplies the instance, device, and queue.</param>
		/// <param name="options">The options that configure the context.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteContext" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
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

		/// <summary>Creates a Metal-backed Graphite context using the specified options.</summary>
		/// <param name="backendContext">The Metal backend context that supplies the device and queue.</param>
		/// <param name="options">The options that configure the context.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteContext" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
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

		/// <summary>Creates a Vulkan-backed Graphite context using the specified options.</summary>
		/// <param name="backendContext">The Vulkan backend context that supplies the device and function loader.</param>
		/// <param name="options">The options that configure the context.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteContext" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKGraphiteContext CreateVulkan (SKGraphiteVkBackendContext backendContext, SKGraphiteContextOptions options)
		{
			if (backendContext is null)
				throw new ArgumentNullException (nameof (backendContext));
			ValidateOptions (options);

			var init = backendContext.ToNative ();
			IntPtr handle = SkiaApi.sk_graphite_context_make_vulkan (init, &options);
			if (handle == IntPtr.Zero)
				return null;

			var ctx = new SKGraphiteContext (handle, true);

			// The Skia context's Vulkan dispatch lambda captured the function
			// pointer + userData by value, so SKGraphiteContext takes over keeping
			// the underlying managed delegate alive.
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

		/// <summary>Releases the native resources used by the context.</summary>
		/// <remarks />
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

		/// <summary>Gets the graphics backend that this context uses.</summary>
		/// <value>One of the enumeration values that indicates the backend.</value>
		/// <remarks />
		public SKGraphiteBackend Backend =>
			SkiaApi.sk_graphite_context_get_backend (Handle);

		/// <summary>Gets a value indicating whether the underlying GPU device has been lost.</summary>
		/// <value><see langword="true" /> if the device has been lost; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsDeviceLost =>
			SkiaApi.sk_graphite_context_is_device_lost (Handle);

		/// <summary>Gets the maximum texture dimension supported by the context's backend, in pixels.</summary>
		/// <value>The maximum texture size, in pixels.</value>
		/// <remarks />
		public int MaxTextureSize =>
			SkiaApi.sk_graphite_context_get_max_texture_size (Handle);

		/// <summary>Gets a value indicating whether the context supports protected content.</summary>
		/// <value><see langword="true" /> if protected content is supported; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool SupportsProtectedContent =>
			SkiaApi.sk_graphite_context_supports_protected_content (Handle);

		/// <summary>Gets the number of bytes of GPU memory currently used by budgeted resources.</summary>
		/// <value>The current budgeted GPU memory usage, in bytes.</value>
		/// <remarks />
		public long CurrentBudgetedBytes =>
			SkiaApi.sk_graphite_context_get_current_budgeted_bytes (Handle);

		/// <summary>Gets or sets the maximum number of bytes of GPU memory the resource cache may use.</summary>
		/// <value>The maximum budgeted GPU memory, in bytes.</value>
		/// <remarks />
		public long MaxBudgetedBytes {
			get => SkiaApi.sk_graphite_context_get_max_budgeted_bytes (Handle);
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException (nameof (value), value, "Must be non-negative.");
				SkiaApi.sk_graphite_context_set_max_budgeted_bytes (Handle, value);
			}
		}

		// Recording

		/// <summary>Creates a new recorder associated with this context.</summary>
		/// <param name="recorderBudgetBytes">The GPU memory budget for the recorder, in bytes, or -1 to use the Skia default.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteRecorder" />, which the caller must dispose before disposing this context, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public SKGraphiteRecorder CreateRecorder (long recorderBudgetBytes = -1) =>
			CreateRecorder (recorderBudgetBytes, findOrCreate: null, findOrCreateDispose: null);

		/// <summary>Creates a new recorder associated with this context, using the specified callback to provide Graphite-backed images.</summary>
		/// <param name="recorderBudgetBytes">The GPU memory budget for the recorder, in bytes, or -1 to use the Skia default.</param>
		/// <param name="findOrCreate">The callback that finds or uploads a Graphite-backed image for a source image, or <see langword="null" /> for none.</param>
		/// <param name="findOrCreateDispose">An optional cleanup action invoked before the recorder is destroyed, or <see langword="null" /> for none.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteRecorder" />, which the caller must dispose before disposing this context, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
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
					(delegate* unmanaged[Cdecl] <void*, IntPtr, IntPtr, bool, IntPtr>)
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

		/// <summary>Inserts a recording into the context so its commands are executed on the next submit.</summary>
		/// <param name="recording">The recording to insert.</param>
		/// <returns>One of the enumeration values that indicates the result of the insertion.</returns>
		/// <remarks />
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteRecording recording)
		{
			if (recording is null)
				throw new ArgumentNullException (nameof (recording));

			var info = new SKGraphiteInsertRecordingInfo {
				Recording = recording.Handle,
			};
			return SkiaApi.sk_graphite_context_insert_recording (Handle, &info);
		}

		/// <summary>Inserts a recording into the context using the specified insertion parameters.</summary>
		/// <param name="info">The parameters that describe the recording to insert and where to place it.</param>
		/// <returns>One of the enumeration values that indicates the result of the insertion.</returns>
		/// <remarks />
		public SKGraphiteInsertStatus InsertRecording (SKGraphiteInsertRecordingInfo info) =>
			SkiaApi.sk_graphite_context_insert_recording (Handle, &info);

		/// <summary>Submits all inserted recordings to the GPU without waiting for completion.</summary>
		/// <returns><see langword="true" /> if the work was submitted successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Submit () =>
			SkiaApi.sk_graphite_context_submit (Handle, null);

		/// <summary>Submits all inserted recordings to the GPU using the specified submission options.</summary>
		/// <param name="submitInfo">The options that control how the work is submitted.</param>
		/// <returns><see langword="true" /> if the work was submitted successfully; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Submit (SKGraphiteSubmitInfo submitInfo)
		{
			if (submitInfo.Sync && isNonYielding)
				throw new InvalidOperationException (
					"SKGraphiteSubmitInfo.Sync = true is not supported in this environment. " +
					"Submit without sync and drive readbacks via CheckAsyncWorkCompletion instead.");
			return SkiaApi.sk_graphite_context_submit (Handle, &submitInfo);
		}

		// Resource management

		/// <summary>Frees GPU resources held by the context's resource cache.</summary>
		/// <remarks />
		public void FreeGpuResources () =>
			SkiaApi.sk_graphite_context_free_gpu_resources (Handle);

		/// <summary>Purges GPU resources that have not been used for at least the specified duration.</summary>
		/// <param name="duration">The minimum time a resource must have been unused before it is purged.</param>
		/// <remarks />
		public void PerformDeferredCleanup (TimeSpan duration)
		{
			if (duration < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException (nameof (duration), duration, "Must be non-negative.");
			SkiaApi.sk_graphite_context_perform_deferred_cleanup (Handle, (long)duration.TotalMilliseconds);
		}

		/// <summary>Deletes a backend texture that was created by this context.</summary>
		/// <param name="backendTexture">The backend texture to delete.</param>
		/// <remarks />
		public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture)
		{
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));
			SkiaApi.sk_graphite_context_delete_backend_texture (Handle, backendTexture.Handle);
		}

		/// <summary>Checks for and processes any completed asynchronous GPU work, invoking pending callbacks.</summary>
		/// <remarks />
		public void CheckAsyncWorkCompletion () =>
			SkiaApi.sk_graphite_context_check_async_work_completion (Handle);

		/// <summary>Asynchronously reads and rescales pixels from the specified surface into a result delivered to a callback.</summary>
		/// <param name="surface">The surface to read pixels from.</param>
		/// <param name="dstInfo">The image info describing the desired size and format of the result.</param>
		/// <param name="srcRect">The rectangle of the surface to read, in pixels.</param>
		/// <param name="rescaleGamma">One of the enumeration values that specifies the gamma space used for rescaling.</param>
		/// <param name="rescaleMode">One of the enumeration values that specifies the sampling algorithm used for rescaling.</param>
		/// <param name="callback">The callback invoked with the read result, or <see langword="null" /> if the read fails; the result is valid only for the duration of the call.</param>
		/// <remarks />
		public void RequestReadPixels (
			SKSurface surface,
			SKImageInfo dstInfo,
			SKRectI srcRect,
			SKImageRescaleGamma rescaleGamma,
			SKImageRescaleMode rescaleMode,
			Action<SKImageReadPixelsResult> callback)
		{
			if (surface is null) throw new ArgumentNullException (nameof (surface));
			if (callback is null) throw new ArgumentNullException (nameof (callback));

			Action<IntPtr> handler = raw => {
				using var result = raw == IntPtr.Zero ? null : new SKImageReadPixelsResult (raw, dstInfo);
				callback (result);
				// Keep the context and surface alive until the (possibly deferred) callback fires.
				GC.KeepAlive (this);
				GC.KeepAlive (surface);
			};

			DelegateProxies.Create (handler, out _, out var ctxPtr);

			var nativeInfo = SKImageInfoNative.FromManaged (ref dstInfo);
			SkiaApi.sk_graphite_context_async_rescale_and_read_pixels_surface (
				Handle, surface.Handle, &nativeInfo, &srcRect,
				rescaleGamma, rescaleMode,
				DelegateProxies.SKImageAsyncReadPixelsProxy,
				(void*)ctxPtr);
			GC.KeepAlive (this);
			GC.KeepAlive (surface);
		}

		/// <summary>Asynchronously reads pixels from the specified surface into a result delivered to a callback, using nearest sampling.</summary>
		/// <param name="surface">The surface to read pixels from.</param>
		/// <param name="dstInfo">The image info describing the desired size and format of the result.</param>
		/// <param name="srcRect">The rectangle of the surface to read, in pixels.</param>
		/// <param name="callback">The callback invoked with the read result, which is valid only for the duration of the call.</param>
		/// <remarks />
		public void RequestReadPixels (
			SKSurface surface,
			SKImageInfo dstInfo,
			SKRectI srcRect,
			Action<SKImageReadPixelsResult> callback) =>
			RequestReadPixels (surface, dstInfo, srcRect,
				SKImageRescaleGamma.Src, SKImageRescaleMode.Nearest, callback);
	}
}
