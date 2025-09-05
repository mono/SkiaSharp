#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Represents an underlying backend 3D API context.
	/// </summary>
	public unsafe class GRContext : GRRecordingContext
	{
		internal GRContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			AbandonContext ();

			base.DisposeNative ();
		}

		// CreateGl

		/// <summary>
		/// Creates a <see cref="GRContext" /> for an OpenGL context.
		/// </summary>
		/// <returns>Returns the new <see cref="GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		public static GRContext CreateGl () =>
			CreateGl (null, null);

		/// <summary>
		/// Creates a <see cref="GRContext" /> for an OpenGL context.
		/// </summary>
		/// <param name="backendContext">The OpenGL interface to use.</param>
		/// <returns>Returns the new <see cref="GRContext" /> if one was created, otherwise <see langword="null" />.</returns>
		public static GRContext CreateGl (GRGlInterface backendContext) =>
			CreateGl (backendContext, null);

		/// <param name="options"></param>
		public static GRContext CreateGl (GRContextOptions options) =>
			CreateGl (null, options);

		/// <param name="backendContext"></param>
		/// <param name="options"></param>
		public static GRContext CreateGl (GRGlInterface backendContext, GRContextOptions options)
		{
			var ctx = backendContext == null ? IntPtr.Zero : backendContext.Handle;

			if (options == null) {
				return GetObject (SkiaApi.gr_direct_context_make_gl (ctx));
			} else {
				var opts = options.ToNative ();
				return GetObject (SkiaApi.gr_direct_context_make_gl_with_options (ctx, &opts));
			}
		}

		// CreateVulkan

		/// <param name="backendContext"></param>
		public static GRContext CreateVulkan (GRVkBackendContext backendContext) =>
			CreateVulkan (backendContext, null);

		/// <param name="backendContext"></param>
		/// <param name="options"></param>
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

		public static GRContext CreateDirect3D (GRD3DBackendContext backendContext) =>
			CreateDirect3D (backendContext, null);

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

		public static GRContext CreateMetal (GRMtlBackendContext backendContext) =>
			CreateMetal (backendContext, null);

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

		/// <summary>
		/// Gets the backend that this context is wrapping.
		/// </summary>
		public override GRBackend Backend => base.Backend;

		public override bool IsAbandoned => SkiaApi.gr_direct_context_is_abandoned (Handle);

		/// <summary>
		/// Abandons all GPU resources and assumes the underlying backend 3D API context is not longer usable. After returning it will assume that the underlying context may no longer be valid.
		/// </summary>
		/// <param name="releaseResources">Use true to indicate that the underlying 3D context is not yet lost and the <see cref="GRContext" /> will cleanup all allocated resources before returning. Using false will ensure that the destructors of the <see cref="GRContext" /> and any of its created resource objects will not make backend 3D API calls.</param>
		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources)
				SkiaApi.gr_direct_context_release_resources_and_abandon_context (Handle);
			else
				SkiaApi.gr_direct_context_abandon_context (Handle);
		}

		public long GetResourceCacheLimit () =>
			(long)SkiaApi.gr_direct_context_get_resource_cache_limit (Handle);

		/// <param name="maxResourceBytes"></param>
		public void SetResourceCacheLimit (long maxResourceBytes) =>
			SkiaApi.gr_direct_context_set_resource_cache_limit (Handle, (IntPtr)maxResourceBytes);

		/// <summary>
		/// Returns the current GPU resource cache usage.
		/// </summary>
		/// <param name="maxResources">The number of resources that are held in the cache.</param>
		/// <param name="maxResourceBytes">The total number of bytes of video memory held in the cache.</param>
		public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes)
		{
			IntPtr maxResBytes;
			fixed (int* maxRes = &maxResources) {
				SkiaApi.gr_direct_context_get_resource_cache_usage (Handle, maxRes, &maxResBytes);
			}
			maxResourceBytes = (long)maxResBytes;
		}

		/// <summary>
		/// Informs the context that the state was modified and should resend.
		/// </summary>
		/// <param name="state">Flags to control what is reset.</param>
		/// <remarks>
		/// The context normally assumes that no outsider is setting state within the underlying 3D API's context/device/whatever. This method shouldn't be called frequently for good performance.
		/// </remarks>
		public void ResetContext (GRGlBackendState state) =>
			ResetContext ((uint)state);

		/// <summary>
		/// Informs the context that the state was modified and should resend.
		/// </summary>
		/// <param name="state">Flags to control what is reset.</param>
		/// <remarks>
		/// The context normally assumes that no outsider is setting state within the underlying 3D API's context/device/whatever. This method shouldn't be called frequently for good performance.
		/// </remarks>
		public void ResetContext (GRBackendState state = GRBackendState.All) =>
			ResetContext ((uint)state);

		/// <summary>
		/// Informs the context that the state was modified and should resend.
		/// </summary>
		/// <param name="state">Flags to control what is reset.</param>
		/// <remarks>
		/// The context normally assumes that no outsider is setting state within the underlying 3D API's context/device/whatever. This method shouldn't be called frequently for good performance.
		/// </remarks>
		public void ResetContext (uint state) =>
			SkiaApi.gr_direct_context_reset_context (Handle, state);

		/// <summary>
		/// Call to ensure all drawing to the context has been issued to the underlying 3D API.
		/// </summary>
		public void Flush () => Flush (true);

		/// <param name="submit"></param>
		/// <param name="synchronous"></param>
		public void Flush (bool submit, bool synchronous = false)
		{
			if (submit)
				SkiaApi.gr_direct_context_flush_and_submit (Handle, synchronous);
			else
				SkiaApi.gr_direct_context_flush (Handle);
		}

		/// <param name="synchronous"></param>
		public void Submit (bool synchronous = false) =>
			SkiaApi.gr_direct_context_submit (Handle, synchronous);

		/// <summary>
		/// Get the maximum supported sample count for the specified color type.
		/// </summary>
		/// <param name="colorType">The color type.</param>
		/// <returns>Returns the maximum supported sample count.</returns>
		/// <remarks>
		/// 1 is returned if only non-MSAA rendering is supported for the color type. 0 is returned if rendering to this color type is not supported at all.
		/// </remarks>
		public new int GetMaxSurfaceSampleCount (SKColorType colorType) =>
			base.GetMaxSurfaceSampleCount (colorType);

		/// <param name="dump"></param>
		public void DumpMemoryStatistics (SKTraceMemoryDump dump) =>
			SkiaApi.gr_direct_context_dump_memory_statistics (Handle, dump?.Handle ?? throw new ArgumentNullException (nameof (dump)));

		public void PurgeResources () =>
			SkiaApi.gr_direct_context_free_gpu_resources (Handle);

		/// <param name="milliseconds"></param>
		public void PurgeUnusedResources (long milliseconds) =>
			SkiaApi.gr_direct_context_perform_deferred_cleanup (Handle, milliseconds);

		/// <param name="scratchResourcesOnly"></param>
		public void PurgeUnlockedResources (bool scratchResourcesOnly) =>
			SkiaApi.gr_direct_context_purge_unlocked_resources (Handle, scratchResourcesOnly);

		/// <param name="bytesToPurge"></param>
		/// <param name="preferScratchResources"></param>
		public void PurgeUnlockedResources (long bytesToPurge, bool preferScratchResources) =>
			SkiaApi.gr_direct_context_purge_unlocked_resources_bytes (Handle, (IntPtr)bytesToPurge, preferScratchResources);

		internal static new GRContext GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new GRContext (h, o));
	}
}
