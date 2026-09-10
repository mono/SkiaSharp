#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>Represents an underlying backend 3D API context.</summary>
	/// <remarks />
	public unsafe class GRContext : GRRecordingContext
	{
		internal GRContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.GRContext" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.GRContext" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases the native resources used by the <see cref="T:SkiaSharp.GRContext" />.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			AbandonContext ();

			base.DisposeNative ();
		}

		// CreateGl

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for an OpenGL context.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateGl () =>
			CreateGl (null, null);

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for an OpenGL context.</summary>
		/// <param name="backendContext">The OpenGL interface to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateGl (GRGlInterface backendContext) =>
			CreateGl (backendContext, null);

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for an OpenGL context.</summary>
		/// <param name="options">The context-creation options.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateGl (GRContextOptions options) =>
			CreateGl (null, options);

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for an OpenGL context.</summary>
		/// <param name="backendContext">The OpenGL interface to use.</param>
		/// <param name="options">The context-creation options.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateGl (GRGlInterface backendContext, GRContextOptions options)
		{
			var ctx = backendContext == null ? IntPtr.Zero : backendContext.Handle;

			if (options == null) {
				var context = GetObject (SkiaApi.gr_direct_context_make_gl (ctx));
				GC.KeepAlive (backendContext);
				return context;
			} else {
				var opts = options.ToNative ();
				var context = GetObject (SkiaApi.gr_direct_context_make_gl_with_options (ctx, &opts));
				GC.KeepAlive (backendContext);
				return context;
			}
		}

		// CreateVulkan

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for a Vulkan context.</summary>
		/// <param name="backendContext">The Vulkan backend context to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateVulkan (GRVkBackendContext backendContext) =>
			CreateVulkan (backendContext, null);

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for a Vulkan context.</summary>
		/// <param name="backendContext">The Vulkan backend context to use.</param>
		/// <param name="options">The context-creation options.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateVulkan (GRVkBackendContext backendContext, GRContextOptions options)
		{
			if (backendContext == null)
				throw new ArgumentNullException (nameof (backendContext));

			if (options == null) {
				return GetObject (SkiaApi.gr_direct_context_make_vulkan (backendContext.ToNative ()));
			} else {
				var opts = options.ToNative ();
				return GetObject (SkiaApi.gr_direct_context_make_vulkan_with_options (backendContext.ToNative (), &opts));
			}
		}

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for a Direct3D context.</summary>
		/// <param name="backendContext">The Direct3D backend context to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateDirect3D (GRD3DBackendContext backendContext) =>
			CreateDirect3D (backendContext, null);

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for a Direct3D context.</summary>
		/// <param name="backendContext">The Direct3D backend context to use.</param>
		/// <param name="options">The context-creation options.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateDirect3D (GRD3DBackendContext backendContext, GRContextOptions options)
		{
			if (backendContext == null)
				throw new ArgumentNullException (nameof (backendContext));
			if (options == null) {
				return GetObject (SkiaApi.gr_direct_context_make_direct3d (backendContext.ToNative ()));
			} else {
				var opts = options.ToNative ();
				return GetObject (SkiaApi.gr_direct_context_make_direct3d_with_options (backendContext.ToNative (), &opts));
			}
		}

		// CreateMetal

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for a Metal context.</summary>
		/// <param name="backendContext">The Metal backend context to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateMetal (GRMtlBackendContext backendContext) =>
			CreateMetal (backendContext, null);

		/// <summary>Creates a <see cref="T:SkiaSharp.GRContext" /> for a Metal context.</summary>
		/// <param name="backendContext">The Metal backend context to use.</param>
		/// <param name="options">The context-creation options.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public static GRContext CreateMetal (GRMtlBackendContext backendContext, GRContextOptions options)
		{
			if (backendContext == null)
				throw new ArgumentNullException (nameof (backendContext));

			var device = backendContext.DeviceHandle;
			var queue = backendContext.QueueHandle;

			if (options == null) {
				return GetObject (SkiaApi.gr_direct_context_make_metal ((void*)device, (void*)queue));
			} else {
				var opts = options.ToNative ();
				return GetObject (SkiaApi.gr_direct_context_make_metal_with_options ((void*)device, (void*)queue, &opts));
			}
		}

		//

		/// <summary>Gets the backend that this context is wrapping.</summary>
		/// <value>The backend type.</value>
		/// <remarks />
		public override GRBackend Backend => base.Backend;

		/// <summary>Gets a value indicating whether the context has been abandoned.</summary>
		/// <value><see langword="true" /> if the context has been abandoned; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public override bool IsAbandoned {
			get {
				var result = SkiaApi.gr_direct_context_is_abandoned (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Abandons all GPU resources and assumes the underlying backend 3D API context is no longer usable. After returning it will assume that the underlying context may no longer be valid.</summary>
		/// <param name="releaseResources">Use true to indicate that the underlying 3D context is not yet lost and the <see cref="T:SkiaSharp.GRContext" /> will cleanup all allocated resources before returning. Using false will ensure that the destructors of the <see cref="T:SkiaSharp.GRContext" /> and any of its created resource objects will not make backend 3D API calls.</param>
		/// <remarks />
		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources)
				SkiaApi.gr_direct_context_release_resources_and_abandon_context (Handle);
			else
				SkiaApi.gr_direct_context_abandon_context (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Returns the maximum number of bytes of video memory that can be held in the cache.</summary>
		/// <returns>The maximum number of bytes of video memory that can be held in the cache.</returns>
		/// <remarks />
		public long GetResourceCacheLimit ()
		{
			var result = (long)SkiaApi.gr_direct_context_get_resource_cache_limit (Handle);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Specifies the GPU resource cache limit.</summary>
		/// <param name="maxResourceBytes">The maximum number of bytes of video memory that can be held in the cache.</param>
		/// <remarks>If the current cache exceeds this limit, it will be purged (LRU) to keep the cache within this limit.</remarks>
		public void SetResourceCacheLimit (long maxResourceBytes)
		{
			SkiaApi.gr_direct_context_set_resource_cache_limit (Handle, (IntPtr)maxResourceBytes);
			GC.KeepAlive (this);
		}

		/// <summary>Returns the current GPU resource cache usage.</summary>
		/// <param name="maxResources">The number of resources that are held in the cache.</param>
		/// <param name="maxResourceBytes">The total number of bytes of video memory held in the cache.</param>
		/// <remarks />
		public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes)
		{
			IntPtr maxResBytes;
			fixed (int* maxRes = &maxResources) {
				SkiaApi.gr_direct_context_get_resource_cache_usage (Handle, maxRes, &maxResBytes);
				GC.KeepAlive (this);
			}
			maxResourceBytes = (long)maxResBytes;
		}

		/// <summary>Informs the context that the state was modified and should resend.</summary>
		/// <param name="state">Flags to control what is reset.</param>
		/// <remarks>The context normally assumes that no outsider is setting state within the underlying 3D API's context/device/whatever. This method shouldn't be called frequently for good performance.</remarks>
		public void ResetContext (GRGlBackendState state) =>
			ResetContext ((uint)state);

		/// <summary>Informs the context that the state was modified and should resend.</summary>
		/// <param name="state">Flags to control what is reset.</param>
		/// <remarks>The context normally assumes that no outsider is setting state within the underlying 3D API's context/device/whatever. This method shouldn't be called frequently for good performance.</remarks>
		public void ResetContext (GRBackendState state = GRBackendState.All) =>
			ResetContext ((uint)state);

		/// <summary>Informs the context that the state was modified and should resend.</summary>
		/// <param name="state">Flags to control what is reset.</param>
		/// <remarks>The context normally assumes that no outsider is setting state within the underlying 3D API's context/device/whatever. This method shouldn't be called frequently for good performance.</remarks>
		public void ResetContext (uint state)
		{
			SkiaApi.gr_direct_context_reset_context (Handle, state);
			GC.KeepAlive (this);
		}

		/// <summary>Call to ensure all drawing to the context has been issued to the underlying 3D API.</summary>
		/// <remarks />
		public void Flush () => Flush (true);

		/// <summary>Flushes all pending drawing operations to the underlying 3D API, optionally submitting work to the GPU.</summary>
		/// <param name="submit"><see langword="true" /> to submit the work to the GPU after flushing; otherwise, <see langword="false" />.</param>
		/// <param name="synchronous"><see langword="true" /> to wait for the GPU to complete the work; otherwise, <see langword="false" />.</param>
		/// <remarks />
		public void Flush (bool submit, bool synchronous = false)
		{
			if (submit)
				SkiaApi.gr_direct_context_flush_and_submit (Handle, synchronous);
			else
				SkiaApi.gr_direct_context_flush (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Submits outstanding work to the GPU.</summary>
		/// <param name="synchronous"><see langword="true" /> to wait for the GPU to complete all outstanding work; otherwise, <see langword="false" />.</param>
		/// <remarks />
		public void Submit (bool synchronous = false)
		{
			SkiaApi.gr_direct_context_submit (Handle, synchronous);
			GC.KeepAlive (this);
		}

		/// <summary>Checks for and processes any completed asynchronous GPU work, invoking pending callbacks.</summary>
		/// <remarks />
		public void CheckAsyncWorkCompletion ()
		{
			SkiaApi.gr_direct_context_check_async_work_completion (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Flushes any pending GPU drawing work associated with the specified image.</summary>
		/// <param name="image">The image whose associated GPU work should be flushed.</param>
		/// <remarks />
		public void Flush (SKImage image)
		{
			if (image == null) {
				throw new ArgumentNullException (nameof (image));
			}

			SkiaApi.gr_direct_context_flush_image (Handle, image.Handle);
			GC.KeepAlive (image);
			GC.KeepAlive (this);
		}

		/// <summary>Flushes any pending GPU drawing work associated with the specified surface.</summary>
		/// <param name="surface">The surface whose associated GPU work should be flushed.</param>
		/// <remarks />
		public void Flush (SKSurface surface)
		{
			if (surface == null) {
				throw new ArgumentNullException (nameof (surface));
			}

			SkiaApi.gr_direct_context_flush_surface (Handle, surface.Handle);
			GC.KeepAlive (surface);
			GC.KeepAlive (this);
		}

		/// <summary>Gets the maximum supported sample count for the specified color type.</summary>
		/// <param name="colorType">The color type.</param>
		/// <returns>Returns the maximum supported sample count.</returns>
		/// <remarks>1 is returned if only non-MSAA rendering is supported for the color type. 0 is returned if rendering to this color type is not supported at all.</remarks>
		public new int GetMaxSurfaceSampleCount (SKColorType colorType) =>
			base.GetMaxSurfaceSampleCount (colorType);

		/// <summary>Dumps memory statistics for the context to the specified dump object.</summary>
		/// <param name="dump">The memory dump object to populate with statistics.</param>
		/// <remarks />
		public void DumpMemoryStatistics (SKTraceMemoryDump dump)
		{
			SkiaApi.gr_direct_context_dump_memory_statistics (Handle, dump?.Handle ?? throw new ArgumentNullException (nameof (dump)));
			GC.KeepAlive (dump);
			GC.KeepAlive (this);
		}

		/// <summary>Frees all GPU resources held by the context.</summary>
		/// <remarks />
		public void PurgeResources ()
		{
			SkiaApi.gr_direct_context_free_gpu_resources (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Purges resources that have not been used within the specified time period.</summary>
		/// <param name="milliseconds">The time period in milliseconds. Resources that have not been used within this time period will be purged.</param>
		/// <remarks />
		public void PurgeUnusedResources (long milliseconds)
		{
			SkiaApi.gr_direct_context_perform_deferred_cleanup (Handle, milliseconds);
			GC.KeepAlive (this);
		}

		/// <summary>Purges unlocked resources from the cache.</summary>
		/// <param name="scratchResourcesOnly"><see langword="true" /> to purge only scratch resources; otherwise, <see langword="false" /> to purge all unlocked resources.</param>
		/// <remarks />
		public void PurgeUnlockedResources (bool scratchResourcesOnly)
		{
			SkiaApi.gr_direct_context_purge_unlocked_resources (Handle, scratchResourcesOnly);
			GC.KeepAlive (this);
		}

		/// <summary>Purges unlocked resources from the cache until the specified number of bytes have been freed.</summary>
		/// <param name="bytesToPurge">The number of bytes to attempt to purge.</param>
		/// <param name="preferScratchResources"><see langword="true" /> to prefer purging scratch resources first; otherwise, <see langword="false" />.</param>
		/// <remarks />
		public void PurgeUnlockedResources (long bytesToPurge, bool preferScratchResources)
		{
			SkiaApi.gr_direct_context_purge_unlocked_resources_bytes (Handle, (IntPtr)bytesToPurge, preferScratchResources);
			GC.KeepAlive (this);
		}

		internal new static GRContext GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new GRContext (h, o));
	}
}
