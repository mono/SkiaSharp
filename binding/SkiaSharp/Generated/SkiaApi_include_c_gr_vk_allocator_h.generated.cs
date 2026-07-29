using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region gr_vk_allocator.h

		// gr_vk_memory_allocator_t* gr_vk_memory_allocator_make_default(const gr_vk_allocator_default_options_t options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial gr_vk_memory_allocator_t gr_vk_memory_allocator_make_default (GRVkAllocatorDefaultOptionsNative options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_vk_memory_allocator_t gr_vk_memory_allocator_make_default (GRVkAllocatorDefaultOptionsNative options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_vk_memory_allocator_t gr_vk_memory_allocator_make_default (GRVkAllocatorDefaultOptionsNative options);
		}
		private static Delegates.gr_vk_memory_allocator_make_default gr_vk_memory_allocator_make_default_delegate;
		internal static gr_vk_memory_allocator_t gr_vk_memory_allocator_make_default (GRVkAllocatorDefaultOptionsNative options) =>
			(gr_vk_memory_allocator_make_default_delegate ??= GetSymbol<Delegates.gr_vk_memory_allocator_make_default> ("gr_vk_memory_allocator_make_default")).Invoke (options);
		#endif

		// void gr_vk_memory_allocator_ref(gr_vk_memory_allocator_t* allocator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_vk_memory_allocator_ref (gr_vk_memory_allocator_t allocator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_vk_memory_allocator_ref (gr_vk_memory_allocator_t allocator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_vk_memory_allocator_ref (gr_vk_memory_allocator_t allocator);
		}
		private static Delegates.gr_vk_memory_allocator_ref gr_vk_memory_allocator_ref_delegate;
		internal static void gr_vk_memory_allocator_ref (gr_vk_memory_allocator_t allocator) =>
			(gr_vk_memory_allocator_ref_delegate ??= GetSymbol<Delegates.gr_vk_memory_allocator_ref> ("gr_vk_memory_allocator_ref")).Invoke (allocator);
		#endif

		// void gr_vk_memory_allocator_unref(gr_vk_memory_allocator_t* allocator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void gr_vk_memory_allocator_unref (gr_vk_memory_allocator_t allocator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void gr_vk_memory_allocator_unref (gr_vk_memory_allocator_t allocator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void gr_vk_memory_allocator_unref (gr_vk_memory_allocator_t allocator);
		}
		private static Delegates.gr_vk_memory_allocator_unref gr_vk_memory_allocator_unref_delegate;
		internal static void gr_vk_memory_allocator_unref (gr_vk_memory_allocator_t allocator) =>
			(gr_vk_memory_allocator_unref_delegate ??= GetSymbol<Delegates.gr_vk_memory_allocator_unref> ("gr_vk_memory_allocator_unref")).Invoke (allocator);
		#endif

		#endregion

	}
}
