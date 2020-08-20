﻿using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public unsafe class GRContext : SKObject, ISKReferenceCounted, ISKSkipObjectRegistration
	{
		internal GRContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

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
				GRBackend.OpenGL => GetObject (SkiaApi.gr_context_make_gl (backendContext)),
				GRBackend.Vulkan => throw new NotSupportedException (),
				GRBackend.Dawn => throw new NotSupportedException (),
				_ => throw new ArgumentOutOfRangeException (nameof (backend)),
			};

		// CreateGl

		public static GRContext CreateGl () =>
			CreateGl (null);

		public static GRContext CreateGl (GRGlInterface backendContext) =>
			GetObject (SkiaApi.gr_context_make_gl (backendContext == null ? IntPtr.Zero : backendContext.Handle));

		// CreateVulkan

		public static GRContext CreateVulkan (GRVkBackendContext backendContext)
		{
			if (backendContext == null)
				throw new ArgumentNullException (nameof (backendContext));

			return GetObject (SkiaApi.gr_context_make_vulkan (backendContext.ToNative ()));
		}

		//

		public GRBackend Backend => SkiaApi.gr_context_get_backend (Handle).FromNative ();

		public void AbandonContext (bool releaseResources = false)
		{
			if (releaseResources)
				SkiaApi.gr_context_release_resources_and_abandon_context (Handle);
			else
				SkiaApi.gr_context_abandon_context (Handle);
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
			(long)SkiaApi.gr_context_get_resource_cache_limit (Handle);

		public void SetResourceCacheLimit (long maxResourceBytes) =>
			SkiaApi.gr_context_set_resource_cache_limit (Handle, (IntPtr)maxResourceBytes);

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
			SkiaApi.gr_context_get_max_surface_sample_count_for_color_type (Handle, colorType.ToNative ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public int GetRecommendedSampleCount (GRPixelConfig config, float dpi) => 0;

		public void DumpMemoryStatistics (SKTraceMemoryDump dump) =>
			SkiaApi.gr_context_dump_memory_statistics (Handle, dump?.Handle ?? throw new ArgumentNullException (nameof (dump)));

		internal static GRContext GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new GRContext (handle, true);
	}
}
