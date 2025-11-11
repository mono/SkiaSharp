#nullable disable

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

		public override GRBackend Backend => base.Backend;

		public override bool IsAbandoned => SkiaApi.gr_direct_context_is_abandoned (Handle);

		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources)
				SkiaApi.gr_direct_context_release_resources_and_abandon_context (Handle);
			else
				SkiaApi.gr_direct_context_abandon_context (Handle);
		}

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

		public void Flush (SKSurface surface)
		{
			if (surface == null)
				throw new ArgumentNullException(nameof(surface));
			SkiaApi.gr_direct_context_flush_surface(Handle, surface.Handle);
		}

		public new int GetMaxSurfaceSampleCount (SKColorType colorType) =>
			base.GetMaxSurfaceSampleCount (colorType);

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

		internal static GRContext GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new GRContext (h, o));
	}
}
