using System;

namespace SkiaSharp
{
	public class GRContext : SKObject
	{
		[Preserve]
		internal GRContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}
		
		public static GRContext Create (GRBackend backend)
		{
			switch (backend) {
				case GRBackend.Metal:
					throw new NotSupportedException ();
				case GRBackend.OpenGL:
					return CreateGl ();
				case GRBackend.Vulkan:
					throw new NotSupportedException ();
				default:
					throw new ArgumentOutOfRangeException (nameof (backend));
			}
		}

		public static GRContext Create (GRBackend backend, GRGlInterface backendContext)
		{
			switch (backend) {
				case GRBackend.Metal:
					throw new NotSupportedException ();
				case GRBackend.OpenGL:
					return CreateGl (backendContext);
				case GRBackend.Vulkan:
					throw new NotSupportedException ();
				default:
					throw new ArgumentOutOfRangeException (nameof (backend));
			}
		}

		[Obsolete ("Use Create(GRBackend, GRGlInterface) instead.")]
		public static GRContext Create (GRBackend backend, IntPtr backendContext)
		{
			switch (backend) {
				case GRBackend.Metal:
					throw new NotSupportedException ();
				case GRBackend.OpenGL:
					return GetObject<GRContext> (SkiaApi.gr_context_make_gl (backendContext));
				case GRBackend.Vulkan:
					throw new NotSupportedException ();
				default:
					throw new ArgumentOutOfRangeException (nameof (backend));
			}
		}

		public static GRContext CreateGl ()
		{
			return CreateGl (null);
		}

		public static GRContext CreateGl (GRGlInterface backendContext)
		{
			return GetObject<GRContext> (SkiaApi.gr_context_make_gl (backendContext == null ? IntPtr.Zero : backendContext.Handle));
		}

		public GRBackend Backend => SkiaApi.gr_context_get_backend (Handle);

		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources) {
				SkiaApi.gr_context_release_resources_and_abandon_context (Handle);
			} else {
				SkiaApi.gr_context_abandon_context (Handle);
			}
		}

		public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes)
		{
			SkiaApi.gr_context_get_resource_cache_limits (Handle, out maxResources, out var maxResourceBytesPtr);
			maxResourceBytes = (long)maxResourceBytesPtr;
		}

		public void SetResourceCacheLimits (int maxResources, long maxResourceBytes)
		{
			SkiaApi.gr_context_set_resource_cache_limits (Handle, maxResources, (IntPtr)maxResourceBytes);
		}

		public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes)
		{
			SkiaApi.gr_context_get_resource_cache_usage (Handle, out maxResources, out var maxResourceBytesPtr);
			maxResourceBytes = (long)maxResourceBytesPtr;
		}
		
		public void ResetContext (GRGlBackendState state)
		{
			ResetContext ((uint) state);
		}

		public void ResetContext (GRBackendState state = GRBackendState.All)
		{
			ResetContext ((uint) state);
		}

		public void ResetContext (uint state)
		{
			SkiaApi.gr_context_reset_context (Handle, state);
		}

		public void Flush ()
		{
			SkiaApi.gr_context_flush (Handle);
		}

		public int GetMaxSurfaceSampleCount (SKColorType colorType)
		{
			return SkiaApi.gr_context_get_max_surface_sample_count_for_color_type (Handle, colorType);
		}

		[Obsolete ("Use GetMaxSurfaceSampleCount(SKColorType) instead.")]
		public int GetRecommendedSampleCount (GRPixelConfig config, float dpi)
		{
			return GetMaxSurfaceSampleCount (config.ToColorType ());
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_context_unref (Handle);
			}

			base.Dispose (disposing);
		}
	}
}
