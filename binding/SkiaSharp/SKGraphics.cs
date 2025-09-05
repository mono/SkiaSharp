#nullable disable

using System;

namespace SkiaSharp
{
	public static unsafe class SKGraphics
	{
		public static void Init () =>
			SkiaApi.sk_graphics_init ();

		// purge

		public static void PurgeFontCache () =>
			SkiaApi.sk_graphics_purge_font_cache ();

		public static void PurgeResourceCache () =>
			SkiaApi.sk_graphics_purge_resource_cache ();

		public static void PurgeAllCaches () =>
			SkiaApi.sk_graphics_purge_all_caches ();

		// font cache

		public static long GetFontCacheUsed () =>
			(long)SkiaApi.sk_graphics_get_font_cache_used ();

		public static long GetFontCacheLimit () =>
			(long)SkiaApi.sk_graphics_get_font_cache_limit ();

		/// <param name="bytes"></param>
		public static long SetFontCacheLimit (long bytes) =>
			(long)SkiaApi.sk_graphics_set_font_cache_limit ((IntPtr)bytes);

		public static int GetFontCacheCountUsed () =>
			SkiaApi.sk_graphics_get_font_cache_count_used ();

		public static int GetFontCacheCountLimit () =>
			SkiaApi.sk_graphics_get_font_cache_count_limit ();

		/// <param name="count"></param>
		public static int SetFontCacheCountLimit (int count) =>
			SkiaApi.sk_graphics_set_font_cache_count_limit (count);

		// resource cache

		public static long GetResourceCacheTotalBytesUsed () =>
			(long)SkiaApi.sk_graphics_get_resource_cache_total_bytes_used ();

		public static long GetResourceCacheTotalByteLimit () =>
			(long)SkiaApi.sk_graphics_get_resource_cache_total_byte_limit ();

		/// <param name="bytes"></param>
		public static long SetResourceCacheTotalByteLimit (long bytes) =>
			(long)SkiaApi.sk_graphics_set_resource_cache_total_byte_limit ((IntPtr)bytes);

		public static long GetResourceCacheSingleAllocationByteLimit () =>
			(long)SkiaApi.sk_graphics_get_resource_cache_single_allocation_byte_limit ();

		/// <param name="bytes"></param>
		public static long SetResourceCacheSingleAllocationByteLimit (long bytes) =>
			(long)SkiaApi.sk_graphics_set_resource_cache_single_allocation_byte_limit ((IntPtr)bytes);

		// dump

		/// <param name="dump"></param>
		public static void DumpMemoryStatistics (SKTraceMemoryDump dump) =>
			SkiaApi.sk_graphics_dump_memory_statistics (dump?.Handle ?? throw new ArgumentNullException (nameof (dump)));
	}
}
