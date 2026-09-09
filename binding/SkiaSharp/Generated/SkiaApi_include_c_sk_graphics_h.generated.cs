using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_graphics.h

		// void sk_graphics_dump_memory_statistics(sk_tracememorydump_t* dump)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphics_dump_memory_statistics (IntPtr dump);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_dump_memory_statistics (IntPtr dump);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_dump_memory_statistics (IntPtr dump);
		}
		private static Delegates.sk_graphics_dump_memory_statistics sk_graphics_dump_memory_statistics_delegate;
		internal static void sk_graphics_dump_memory_statistics (IntPtr dump) =>
			(sk_graphics_dump_memory_statistics_delegate ??= GetSymbol<Delegates.sk_graphics_dump_memory_statistics> ("sk_graphics_dump_memory_statistics")).Invoke (dump);
		#endif

		// int sk_graphics_get_font_cache_count_limit()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_graphics_get_font_cache_count_limit ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_get_font_cache_count_limit ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_get_font_cache_count_limit ();
		}
		private static Delegates.sk_graphics_get_font_cache_count_limit sk_graphics_get_font_cache_count_limit_delegate;
		internal static Int32 sk_graphics_get_font_cache_count_limit () =>
			(sk_graphics_get_font_cache_count_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_count_limit> ("sk_graphics_get_font_cache_count_limit")).Invoke ();
		#endif

		// int sk_graphics_get_font_cache_count_used()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_graphics_get_font_cache_count_used ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_get_font_cache_count_used ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_get_font_cache_count_used ();
		}
		private static Delegates.sk_graphics_get_font_cache_count_used sk_graphics_get_font_cache_count_used_delegate;
		internal static Int32 sk_graphics_get_font_cache_count_used () =>
			(sk_graphics_get_font_cache_count_used_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_count_used> ("sk_graphics_get_font_cache_count_used")).Invoke ();
		#endif

		// size_t sk_graphics_get_font_cache_limit()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_get_font_cache_limit ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_font_cache_limit ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_font_cache_limit ();
		}
		private static Delegates.sk_graphics_get_font_cache_limit sk_graphics_get_font_cache_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_font_cache_limit () =>
			(sk_graphics_get_font_cache_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_limit> ("sk_graphics_get_font_cache_limit")).Invoke ();
		#endif

		// size_t sk_graphics_get_font_cache_used()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_get_font_cache_used ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_font_cache_used ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_font_cache_used ();
		}
		private static Delegates.sk_graphics_get_font_cache_used sk_graphics_get_font_cache_used_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_font_cache_used () =>
			(sk_graphics_get_font_cache_used_delegate ??= GetSymbol<Delegates.sk_graphics_get_font_cache_used> ("sk_graphics_get_font_cache_used")).Invoke ();
		#endif

		// size_t sk_graphics_get_resource_cache_single_allocation_byte_limit()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit ();
		}
		private static Delegates.sk_graphics_get_resource_cache_single_allocation_byte_limit sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_resource_cache_single_allocation_byte_limit () =>
			(sk_graphics_get_resource_cache_single_allocation_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_resource_cache_single_allocation_byte_limit> ("sk_graphics_get_resource_cache_single_allocation_byte_limit")).Invoke ();
		#endif

		// size_t sk_graphics_get_resource_cache_total_byte_limit()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit ();
		}
		private static Delegates.sk_graphics_get_resource_cache_total_byte_limit sk_graphics_get_resource_cache_total_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_resource_cache_total_byte_limit () =>
			(sk_graphics_get_resource_cache_total_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_get_resource_cache_total_byte_limit> ("sk_graphics_get_resource_cache_total_byte_limit")).Invoke ();
		#endif

		// size_t sk_graphics_get_resource_cache_total_bytes_used()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used ();
		}
		private static Delegates.sk_graphics_get_resource_cache_total_bytes_used sk_graphics_get_resource_cache_total_bytes_used_delegate;
		internal static /* size_t */ IntPtr sk_graphics_get_resource_cache_total_bytes_used () =>
			(sk_graphics_get_resource_cache_total_bytes_used_delegate ??= GetSymbol<Delegates.sk_graphics_get_resource_cache_total_bytes_used> ("sk_graphics_get_resource_cache_total_bytes_used")).Invoke ();
		#endif

		// void sk_graphics_init()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphics_init ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_init ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_init ();
		}
		private static Delegates.sk_graphics_init sk_graphics_init_delegate;
		internal static void sk_graphics_init () =>
			(sk_graphics_init_delegate ??= GetSymbol<Delegates.sk_graphics_init> ("sk_graphics_init")).Invoke ();
		#endif

		// void sk_graphics_purge_all_caches()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphics_purge_all_caches ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_purge_all_caches ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_purge_all_caches ();
		}
		private static Delegates.sk_graphics_purge_all_caches sk_graphics_purge_all_caches_delegate;
		internal static void sk_graphics_purge_all_caches () =>
			(sk_graphics_purge_all_caches_delegate ??= GetSymbol<Delegates.sk_graphics_purge_all_caches> ("sk_graphics_purge_all_caches")).Invoke ();
		#endif

		// void sk_graphics_purge_font_cache()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphics_purge_font_cache ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_purge_font_cache ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_purge_font_cache ();
		}
		private static Delegates.sk_graphics_purge_font_cache sk_graphics_purge_font_cache_delegate;
		internal static void sk_graphics_purge_font_cache () =>
			(sk_graphics_purge_font_cache_delegate ??= GetSymbol<Delegates.sk_graphics_purge_font_cache> ("sk_graphics_purge_font_cache")).Invoke ();
		#endif

		// void sk_graphics_purge_resource_cache()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_graphics_purge_resource_cache ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_graphics_purge_resource_cache ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_graphics_purge_resource_cache ();
		}
		private static Delegates.sk_graphics_purge_resource_cache sk_graphics_purge_resource_cache_delegate;
		internal static void sk_graphics_purge_resource_cache () =>
			(sk_graphics_purge_resource_cache_delegate ??= GetSymbol<Delegates.sk_graphics_purge_resource_cache> ("sk_graphics_purge_resource_cache")).Invoke ();
		#endif

		// int sk_graphics_set_font_cache_count_limit(int count)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_graphics_set_font_cache_count_limit (Int32 count);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_graphics_set_font_cache_count_limit (Int32 count);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_graphics_set_font_cache_count_limit (Int32 count);
		}
		private static Delegates.sk_graphics_set_font_cache_count_limit sk_graphics_set_font_cache_count_limit_delegate;
		internal static Int32 sk_graphics_set_font_cache_count_limit (Int32 count) =>
			(sk_graphics_set_font_cache_count_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_font_cache_count_limit> ("sk_graphics_set_font_cache_count_limit")).Invoke (count);
		#endif

		// size_t sk_graphics_set_font_cache_limit(size_t bytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes);
		}
		private static Delegates.sk_graphics_set_font_cache_limit sk_graphics_set_font_cache_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_set_font_cache_limit (/* size_t */ IntPtr bytes) =>
			(sk_graphics_set_font_cache_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_font_cache_limit> ("sk_graphics_set_font_cache_limit")).Invoke (bytes);
		#endif

		// size_t sk_graphics_set_resource_cache_single_allocation_byte_limit(size_t newLimit)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit);
		}
		private static Delegates.sk_graphics_set_resource_cache_single_allocation_byte_limit sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_set_resource_cache_single_allocation_byte_limit (/* size_t */ IntPtr newLimit) =>
			(sk_graphics_set_resource_cache_single_allocation_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_resource_cache_single_allocation_byte_limit> ("sk_graphics_set_resource_cache_single_allocation_byte_limit")).Invoke (newLimit);
		#endif

		// size_t sk_graphics_set_resource_cache_total_byte_limit(size_t newLimit)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit);
		}
		private static Delegates.sk_graphics_set_resource_cache_total_byte_limit sk_graphics_set_resource_cache_total_byte_limit_delegate;
		internal static /* size_t */ IntPtr sk_graphics_set_resource_cache_total_byte_limit (/* size_t */ IntPtr newLimit) =>
			(sk_graphics_set_resource_cache_total_byte_limit_delegate ??= GetSymbol<Delegates.sk_graphics_set_resource_cache_total_byte_limit> ("sk_graphics_set_resource_cache_total_byte_limit")).Invoke (newLimit);
		#endif

		#endregion

	}
}
