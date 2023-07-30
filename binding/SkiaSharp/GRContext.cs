using System;
using System.ComponentModel;

namespace SkiaSharp
{
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

		// Create

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateGl() instead.")]
		public static GRContext Create (GRBackend backend) =>
			backend switch
			{
				GRBackend.Metal => throw new NotSupportedException (),
				GRBackend.OpenGL => CreateGl (),
				GRBackend.Vulkan => throw new NotSupportedException (),
				GRBackend.Dawn => throw new NotSupportedException (),
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateGl(GRGlInterface) instead.")]
		public static GRContext Create (GRBackend backend, GRGlInterface backendContext) =>
			backend switch
			{
				GRBackend.Metal => throw new NotSupportedException (),
				GRBackend.OpenGL => CreateGl (backendContext),
				GRBackend.Vulkan => throw new NotSupportedException (),
				GRBackend.Dawn => throw new NotSupportedException (),
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateGl(GRGlInterface) instead.")]
		public static GRContext Create (GRBackend backend, IntPtr backendContext) =>
			backend switch
			{
				GRBackend.Metal => throw new NotSupportedException (),
				GRBackend.OpenGL => GetObject (SkiaApi.gr_direct_context_make_gl (backendContext)),
				GRBackend.Vulkan => throw new NotSupportedException (),
				GRBackend.Dawn => throw new NotSupportedException (),
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		// CreateGl

		public static GRContext CreateGl () =>
			CreateGl (null, null);

		public static GRContext CreateGl (GRGlInterface backendContext) =>
			CreateGl (backendContext, null);

		public static GRContext CreateGl (GRContextOptions options) =>
			CreateGl (null, options);

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

		public static GRContext CreateVulkan (GRVkBackendContext backendContext) =>
			CreateVulkan (backendContext, null);

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

#if __IOS__ || __MACOS__

		// CreateMetal

		public static GRContext CreateMetal (GRMtlBackendContext backendContext) =>
			CreateMetal (backendContext, null);

		public static GRContext CreateMetal (GRMtlBackendContext backendContext, GRContextOptions options)
		{
			if (backendContext == null)
				throw new ArgumentNullException (nameof (backendContext));

			var device = backendContext.Device;
			var queue = backendContext.Queue;

			if (options == null) {
				return GetObject (SkiaApi.gr_direct_context_make_metal ((void*)(IntPtr)device.Handle, (void*)(IntPtr)queue.Handle));
			} else {
				var opts = options.ToNative ();
				return GetObject (SkiaApi.gr_direct_context_make_metal_with_options ((void*)(IntPtr)device.Handle, (void*)(IntPtr)queue.Handle, &opts));
			}
		}

#endif

		//

		public new GRBackend Backend => base.Backend;

		public bool IsAbandoned => SkiaApi.gr_direct_context_is_abandoned (Handle);

		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources)
				SkiaApi.gr_direct_context_release_resources_and_abandon_context (Handle);
			else
				SkiaApi.gr_direct_context_abandon_context (Handle);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetResourceCacheLimit() instead.")]
		public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes)
		{
			maxResources = -1;
			maxResourceBytes = GetResourceCacheLimit ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use SetResourceCacheLimit(long) instead.")]
		public void SetResourceCacheLimits (int maxResources, long maxResourceBytes) =>
			SetResourceCacheLimit (maxResourceBytes);

		public long GetResourceCacheLimit () =>
			(long)SkiaApi.gr_direct_context_get_resource_cache_limit (Handle);

		public void SetResourceCacheLimit (long maxResourceBytes) =>
			SkiaApi.gr_direct_context_set_resource_cache_limit (Handle, (IntPtr)maxResourceBytes);

		public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes)
		{
			IntPtr maxResBytes;
			fixed (int* maxRes = &maxResources) {
				SkiaApi.gr_direct_context_get_resource_cache_usage (Handle, maxRes, &maxResBytes);
			}
			maxResourceBytes = (long)maxResBytes;
		}

		public void ResetContext (GRGlBackendState state) =>
			ResetContext ((uint)state);

		public void ResetContext (GRBackendState state = GRBackendState.All) =>
			ResetContext ((uint)state);

		public void ResetContext (uint state) =>
			SkiaApi.gr_direct_context_reset_context (Handle, state);

		public void Flush () => Flush (true);

		public void Flush (bool submit, bool synchronous = false)
		{
			if (submit)
				SkiaApi.gr_direct_context_flush_and_submit (Handle, synchronous);
			else
				SkiaApi.gr_direct_context_flush (Handle);
		}

		public void Submit (bool synchronous = false) =>
			SkiaApi.gr_direct_context_submit (Handle, synchronous);

		public new int GetMaxSurfaceSampleCount (SKColorType colorType) =>
			base.GetMaxSurfaceSampleCount (colorType);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public int GetRecommendedSampleCount (GRPixelConfig config, float dpi) => 0;

		public void DumpMemoryStatistics (SKTraceMemoryDump dump) =>
			SkiaApi.gr_direct_context_dump_memory_statistics (Handle, dump?.Handle ?? throw new ArgumentNullException (nameof (dump)));

		public void PurgeResources () =>
			SkiaApi.gr_direct_context_free_gpu_resources (Handle);

		public void PurgeUnusedResources (long milliseconds) =>
			SkiaApi.gr_direct_context_perform_deferred_cleanup (Handle, milliseconds);

		public void PurgeUnlockedResources (bool scratchResourcesOnly) =>
			SkiaApi.gr_direct_context_purge_unlocked_resources (Handle, scratchResourcesOnly);

		public void PurgeUnlockedResources (long bytesToPurge, bool preferScratchResources) =>
			SkiaApi.gr_direct_context_purge_unlocked_resources_bytes (Handle, (IntPtr)bytesToPurge, preferScratchResources);

		internal static GRContext GetObject (IntPtr handle, bool owns = true) =>
			GetOrAddObject (handle, owns, (h, o) => new GRContext (h, o));
	}
}
