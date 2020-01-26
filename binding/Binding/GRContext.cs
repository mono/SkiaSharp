using System;

namespace SkiaSharp
{
	public unsafe class GRContext : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal GRContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public static GRContext CreateGl (GRGlInterface backendContext = null) =>
			GetObject<GRContext> (SkiaApi.gr_context_make_gl (backendContext?.Handle ?? IntPtr.Zero));

		public GRBackend Backend =>
			SkiaApi.gr_context_get_backend (Handle);

		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources)
				SkiaApi.gr_context_release_resources_and_abandon_context (Handle);
			else
				SkiaApi.gr_context_abandon_context (Handle);
		}

		public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes)
		{
			IntPtr maxResBytes;
			fixed (int* maxRes = &maxResources) {
				SkiaApi.gr_context_get_resource_cache_limits (Handle, maxRes, &maxResBytes);
			}
			maxResourceBytes = (long)maxResBytes;
		}

		public void SetResourceCacheLimits (int maxResources, long maxResourceBytes) =>
			SkiaApi.gr_context_set_resource_cache_limits (Handle, maxResources, (IntPtr)maxResourceBytes);

		public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes)
		{
			IntPtr maxResBytes;
			fixed (int* maxRes = &maxResources) {
				SkiaApi.gr_context_get_resource_cache_usage (Handle, maxRes, &maxResBytes);
			}
			maxResourceBytes = (long)maxResBytes;
		}

		public void ResetContext (GRGlBackendState state) =>
			ResetContext ((uint)state);

		public void ResetContext (GRBackendState state = GRBackendState.All) =>
			ResetContext ((uint)state);

		public void ResetContext (uint state) =>
			SkiaApi.gr_context_reset_context (Handle, state);

		public void Flush () =>
			SkiaApi.gr_context_flush (Handle);

		public int GetMaxSurfaceSampleCount (SKColorType colorType) =>
			SkiaApi.gr_context_get_max_surface_sample_count_for_color_type (Handle, colorType);
	}
}
