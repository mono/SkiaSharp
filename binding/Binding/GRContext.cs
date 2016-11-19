//
// Bindings for GRContext
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//

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
			return Create (backend, IntPtr.Zero);
		}

		public static GRContext Create (GRBackend backend, IntPtr backendContext)
		{
			return GetObject<GRContext> (SkiaApi.gr_context_create_with_defaults (backend, backendContext));
		}

		public static GRContext Create (GRBackend backend, GRGlInterface backendContext)
		{
			if (backendContext == null) {
				throw new ArgumentNullException (nameof (backendContext));
			}
			return GetObject<GRContext> (SkiaApi.gr_context_create_with_defaults (backend, backendContext.Handle));
		}

		public static GRContext Create (GRBackend backend, IntPtr backendContext, GRContextOptions options)
		{
			return GetObject<GRContext> (SkiaApi.gr_context_create (backend, backendContext, ref options));
		}

		public static GRContext Create (GRBackend backend, GRGlInterface backendContext, GRContextOptions options)
		{
			if (backendContext == null) {
				throw new ArgumentNullException (nameof (backendContext));
			}
			return GetObject<GRContext> (SkiaApi.gr_context_create (backend, backendContext.Handle, ref options));
		}

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
			IntPtr maxResourceBytesPtr;
			SkiaApi.gr_context_get_resource_cache_limits (Handle, out maxResources, out maxResourceBytesPtr);
			maxResourceBytes = (long)maxResourceBytesPtr;
		}

		public void SetResourceCacheLimits (int maxResources, long maxResourceBytes)
		{
			SkiaApi.gr_context_set_resource_cache_limits (Handle, maxResources, (IntPtr)maxResourceBytes);
		}

		public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes)
		{
			IntPtr maxResourceBytesPtr;
			SkiaApi.gr_context_get_resource_cache_usage (Handle, out maxResources, out maxResourceBytesPtr);
			maxResourceBytes = (long)maxResourceBytesPtr;
		}
		
		public void Flush ()
		{
			SkiaApi.gr_context_flush (Handle);
		}

		[Obsolete ("Use Flush() instead.")]
		public void Flush (GRContextFlushBits flagsBitfield)
		{
			Flush ();
		}

		public int GetRecommendedSampleCount (GRPixelConfig config, float dpi)
		{
			return SkiaApi.gr_context_get_recommended_sample_count (Handle, config, dpi);
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

