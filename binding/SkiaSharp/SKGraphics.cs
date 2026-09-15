#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Provides global settings and cache management for Skia graphics operations.</summary>
	/// <remarks />
	public static unsafe class SKGraphics
	{
		/// <summary>Initializes the Skia graphics system.</summary>
		/// <remarks />
		public static void Init () =>
			SkiaApi.sk_graphics_init ();

		// purge

		/// <summary>Purges all cached font data.</summary>
		/// <remarks />
		public static void PurgeFontCache () =>
			SkiaApi.sk_graphics_purge_font_cache ();

		/// <summary>Purges all cached resource data.</summary>
		/// <remarks />
		public static void PurgeResourceCache () =>
			SkiaApi.sk_graphics_purge_resource_cache ();

		/// <summary>Purges all cached data, including both the font cache and resource cache.</summary>
		/// <remarks />
		public static void PurgeAllCaches () =>
			SkiaApi.sk_graphics_purge_all_caches ();

		// font cache

		/// <summary>Gets the number of bytes currently used by the font cache.</summary>
		/// <returns>The number of bytes used by the font cache.</returns>
		/// <remarks />
		public static long GetFontCacheUsed () =>
			(long)SkiaApi.sk_graphics_get_font_cache_used ();

		/// <summary>Gets the maximum number of bytes allowed for the font cache.</summary>
		/// <returns>The maximum number of bytes for the font cache.</returns>
		/// <remarks />
		public static long GetFontCacheLimit () =>
			(long)SkiaApi.sk_graphics_get_font_cache_limit ();

		/// <summary>Sets the maximum number of bytes allowed for the font cache.</summary>
		/// <param name="bytes">The new maximum number of bytes for the font cache.</param>
		/// <returns>The previous maximum number of bytes for the font cache.</returns>
		/// <remarks />
		public static long SetFontCacheLimit (long bytes) =>
			(long)SkiaApi.sk_graphics_set_font_cache_limit ((IntPtr)bytes);

		/// <summary>Gets the number of entries currently in the font cache.</summary>
		/// <returns>The number of font cache entries currently in use.</returns>
		/// <remarks />
		public static int GetFontCacheCountUsed () =>
			SkiaApi.sk_graphics_get_font_cache_count_used ();

		/// <summary>Gets the maximum number of entries allowed in the font cache.</summary>
		/// <returns>The maximum number of font cache entries.</returns>
		/// <remarks />
		public static int GetFontCacheCountLimit () =>
			SkiaApi.sk_graphics_get_font_cache_count_limit ();

		/// <summary>Sets the maximum number of entries allowed in the font cache.</summary>
		/// <param name="count">The new maximum number of font cache entries.</param>
		/// <returns>The previous maximum number of font cache entries.</returns>
		/// <remarks />
		public static int SetFontCacheCountLimit (int count) =>
			SkiaApi.sk_graphics_set_font_cache_count_limit (count);

		// resource cache

		/// <summary>Gets the number of bytes currently used by the resource cache.</summary>
		/// <returns>The number of bytes used by the resource cache.</returns>
		/// <remarks />
		public static long GetResourceCacheTotalBytesUsed () =>
			(long)SkiaApi.sk_graphics_get_resource_cache_total_bytes_used ();

		/// <summary>Gets the maximum number of bytes allowed for the resource cache.</summary>
		/// <returns>The maximum number of bytes for the resource cache.</returns>
		/// <remarks />
		public static long GetResourceCacheTotalByteLimit () =>
			(long)SkiaApi.sk_graphics_get_resource_cache_total_byte_limit ();

		/// <summary>Sets the maximum number of bytes allowed for the resource cache.</summary>
		/// <param name="bytes">The new maximum number of bytes for the resource cache.</param>
		/// <returns>The previous maximum number of bytes for the resource cache.</returns>
		/// <remarks />
		public static long SetResourceCacheTotalByteLimit (long bytes) =>
			(long)SkiaApi.sk_graphics_set_resource_cache_total_byte_limit ((IntPtr)bytes);

		/// <summary>Gets the maximum number of bytes that can be allocated for a single resource cache entry.</summary>
		/// <returns>The maximum number of bytes for a single resource cache allocation.</returns>
		/// <remarks />
		public static long GetResourceCacheSingleAllocationByteLimit () =>
			(long)SkiaApi.sk_graphics_get_resource_cache_single_allocation_byte_limit ();

		/// <summary>Sets the maximum number of bytes that can be allocated for a single resource cache entry.</summary>
		/// <param name="bytes">The new maximum number of bytes for a single resource cache allocation.</param>
		/// <returns>The previous maximum number of bytes for a single resource cache allocation.</returns>
		/// <remarks />
		public static long SetResourceCacheSingleAllocationByteLimit (long bytes) =>
			(long)SkiaApi.sk_graphics_set_resource_cache_single_allocation_byte_limit ((IntPtr)bytes);

		// dump

		/// <summary>Dumps memory statistics to the specified trace memory dump object.</summary>
		/// <param name="dump">The memory dump object to receive the statistics.</param>
		/// <remarks />
		public static void DumpMemoryStatistics (SKTraceMemoryDump dump)
		{
			SkiaApi.sk_graphics_dump_memory_statistics (dump?.Handle ?? throw new ArgumentNullException (nameof (dump)));
			GC.KeepAlive (dump);
		}
	}
}
