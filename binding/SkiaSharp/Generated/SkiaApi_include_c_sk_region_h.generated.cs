using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_region.h

		// void sk_region_cliperator_delete(sk_region_cliperator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_cliperator_delete (sk_region_cliperator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_delete (sk_region_cliperator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_cliperator_delete (sk_region_cliperator_t iter);
		}
		private static Delegates.sk_region_cliperator_delete sk_region_cliperator_delete_delegate;
		internal static void sk_region_cliperator_delete (sk_region_cliperator_t iter) =>
			(sk_region_cliperator_delete_delegate ??= GetSymbol<Delegates.sk_region_cliperator_delete> ("sk_region_cliperator_delete")).Invoke (iter);
		#endif

		// bool sk_region_cliperator_done(sk_region_cliperator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_cliperator_done (sk_region_cliperator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_cliperator_done (sk_region_cliperator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_cliperator_done (sk_region_cliperator_t iter);
		}
		private static Delegates.sk_region_cliperator_done sk_region_cliperator_done_delegate;
		internal static bool sk_region_cliperator_done (sk_region_cliperator_t iter) =>
			(sk_region_cliperator_done_delegate ??= GetSymbol<Delegates.sk_region_cliperator_done> ("sk_region_cliperator_done")).Invoke (iter);
		#endif

		// sk_region_cliperator_t* sk_region_cliperator_new(const sk_region_t* region, const sk_irect_t* clip)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip);
		}
		private static Delegates.sk_region_cliperator_new sk_region_cliperator_new_delegate;
		internal static sk_region_cliperator_t sk_region_cliperator_new (sk_region_t region, SKRectI* clip) =>
			(sk_region_cliperator_new_delegate ??= GetSymbol<Delegates.sk_region_cliperator_new> ("sk_region_cliperator_new")).Invoke (region, clip);
		#endif

		// void sk_region_cliperator_next(sk_region_cliperator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_cliperator_next (sk_region_cliperator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_next (sk_region_cliperator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_cliperator_next (sk_region_cliperator_t iter);
		}
		private static Delegates.sk_region_cliperator_next sk_region_cliperator_next_delegate;
		internal static void sk_region_cliperator_next (sk_region_cliperator_t iter) =>
			(sk_region_cliperator_next_delegate ??= GetSymbol<Delegates.sk_region_cliperator_next> ("sk_region_cliperator_next")).Invoke (iter);
		#endif

		// void sk_region_cliperator_rect(const sk_region_cliperator_t* iter, sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect);
		}
		private static Delegates.sk_region_cliperator_rect sk_region_cliperator_rect_delegate;
		internal static void sk_region_cliperator_rect (sk_region_cliperator_t iter, SKRectI* rect) =>
			(sk_region_cliperator_rect_delegate ??= GetSymbol<Delegates.sk_region_cliperator_rect> ("sk_region_cliperator_rect")).Invoke (iter, rect);
		#endif

		// bool sk_region_contains(const sk_region_t* r, const sk_region_t* region)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_contains (sk_region_t r, sk_region_t region);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains (sk_region_t r, sk_region_t region);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_contains (sk_region_t r, sk_region_t region);
		}
		private static Delegates.sk_region_contains sk_region_contains_delegate;
		internal static bool sk_region_contains (sk_region_t r, sk_region_t region) =>
			(sk_region_contains_delegate ??= GetSymbol<Delegates.sk_region_contains> ("sk_region_contains")).Invoke (r, region);
		#endif

		// bool sk_region_contains_point(const sk_region_t* r, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y);
		}
		private static Delegates.sk_region_contains_point sk_region_contains_point_delegate;
		internal static bool sk_region_contains_point (sk_region_t r, Int32 x, Int32 y) =>
			(sk_region_contains_point_delegate ??= GetSymbol<Delegates.sk_region_contains_point> ("sk_region_contains_point")).Invoke (r, x, y);
		#endif

		// bool sk_region_contains_rect(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_contains_rect (sk_region_t r, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_contains_rect (sk_region_t r, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_contains_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_contains_rect sk_region_contains_rect_delegate;
		internal static bool sk_region_contains_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_contains_rect_delegate ??= GetSymbol<Delegates.sk_region_contains_rect> ("sk_region_contains_rect")).Invoke (r, rect);
		#endif

		// void sk_region_delete(sk_region_t* r)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_delete (sk_region_t r);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_delete (sk_region_t r);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_delete (sk_region_t r);
		}
		private static Delegates.sk_region_delete sk_region_delete_delegate;
		internal static void sk_region_delete (sk_region_t r) =>
			(sk_region_delete_delegate ??= GetSymbol<Delegates.sk_region_delete> ("sk_region_delete")).Invoke (r);
		#endif

		// bool sk_region_get_boundary_path(const sk_region_t* r, sk_path_t* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path);
		}
		private static Delegates.sk_region_get_boundary_path sk_region_get_boundary_path_delegate;
		internal static bool sk_region_get_boundary_path (sk_region_t r, sk_path_t path) =>
			(sk_region_get_boundary_path_delegate ??= GetSymbol<Delegates.sk_region_get_boundary_path> ("sk_region_get_boundary_path")).Invoke (r, path);
		#endif

		// void sk_region_get_bounds(const sk_region_t* r, sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_get_bounds (sk_region_t r, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_get_bounds (sk_region_t r, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_get_bounds (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_get_bounds sk_region_get_bounds_delegate;
		internal static void sk_region_get_bounds (sk_region_t r, SKRectI* rect) =>
			(sk_region_get_bounds_delegate ??= GetSymbol<Delegates.sk_region_get_bounds> ("sk_region_get_bounds")).Invoke (r, rect);
		#endif

		// bool sk_region_intersects(const sk_region_t* r, const sk_region_t* src)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_intersects (sk_region_t r, sk_region_t src);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_intersects (sk_region_t r, sk_region_t src);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_intersects (sk_region_t r, sk_region_t src);
		}
		private static Delegates.sk_region_intersects sk_region_intersects_delegate;
		internal static bool sk_region_intersects (sk_region_t r, sk_region_t src) =>
			(sk_region_intersects_delegate ??= GetSymbol<Delegates.sk_region_intersects> ("sk_region_intersects")).Invoke (r, src);
		#endif

		// bool sk_region_intersects_rect(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_intersects_rect sk_region_intersects_rect_delegate;
		internal static bool sk_region_intersects_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_intersects_rect_delegate ??= GetSymbol<Delegates.sk_region_intersects_rect> ("sk_region_intersects_rect")).Invoke (r, rect);
		#endif

		// bool sk_region_is_complex(const sk_region_t* r)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_is_complex (sk_region_t r);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_complex (sk_region_t r);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_is_complex (sk_region_t r);
		}
		private static Delegates.sk_region_is_complex sk_region_is_complex_delegate;
		internal static bool sk_region_is_complex (sk_region_t r) =>
			(sk_region_is_complex_delegate ??= GetSymbol<Delegates.sk_region_is_complex> ("sk_region_is_complex")).Invoke (r);
		#endif

		// bool sk_region_is_empty(const sk_region_t* r)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_is_empty (sk_region_t r);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_empty (sk_region_t r);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_is_empty (sk_region_t r);
		}
		private static Delegates.sk_region_is_empty sk_region_is_empty_delegate;
		internal static bool sk_region_is_empty (sk_region_t r) =>
			(sk_region_is_empty_delegate ??= GetSymbol<Delegates.sk_region_is_empty> ("sk_region_is_empty")).Invoke (r);
		#endif

		// bool sk_region_is_rect(const sk_region_t* r)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_is_rect (sk_region_t r);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_is_rect (sk_region_t r);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_is_rect (sk_region_t r);
		}
		private static Delegates.sk_region_is_rect sk_region_is_rect_delegate;
		internal static bool sk_region_is_rect (sk_region_t r) =>
			(sk_region_is_rect_delegate ??= GetSymbol<Delegates.sk_region_is_rect> ("sk_region_is_rect")).Invoke (r);
		#endif

		// void sk_region_iterator_delete(sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_iterator_delete (sk_region_iterator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_delete (sk_region_iterator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_iterator_delete (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_delete sk_region_iterator_delete_delegate;
		internal static void sk_region_iterator_delete (sk_region_iterator_t iter) =>
			(sk_region_iterator_delete_delegate ??= GetSymbol<Delegates.sk_region_iterator_delete> ("sk_region_iterator_delete")).Invoke (iter);
		#endif

		// bool sk_region_iterator_done(const sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_iterator_done (sk_region_iterator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_iterator_done (sk_region_iterator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_iterator_done (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_done sk_region_iterator_done_delegate;
		internal static bool sk_region_iterator_done (sk_region_iterator_t iter) =>
			(sk_region_iterator_done_delegate ??= GetSymbol<Delegates.sk_region_iterator_done> ("sk_region_iterator_done")).Invoke (iter);
		#endif

		// sk_region_iterator_t* sk_region_iterator_new(const sk_region_t* region)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_region_iterator_t sk_region_iterator_new (sk_region_t region);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_iterator_t sk_region_iterator_new (sk_region_t region);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_iterator_t sk_region_iterator_new (sk_region_t region);
		}
		private static Delegates.sk_region_iterator_new sk_region_iterator_new_delegate;
		internal static sk_region_iterator_t sk_region_iterator_new (sk_region_t region) =>
			(sk_region_iterator_new_delegate ??= GetSymbol<Delegates.sk_region_iterator_new> ("sk_region_iterator_new")).Invoke (region);
		#endif

		// void sk_region_iterator_next(sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_iterator_next (sk_region_iterator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_next (sk_region_iterator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_iterator_next (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_next sk_region_iterator_next_delegate;
		internal static void sk_region_iterator_next (sk_region_iterator_t iter) =>
			(sk_region_iterator_next_delegate ??= GetSymbol<Delegates.sk_region_iterator_next> ("sk_region_iterator_next")).Invoke (iter);
		#endif

		// void sk_region_iterator_rect(const sk_region_iterator_t* iter, sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect);
		}
		private static Delegates.sk_region_iterator_rect sk_region_iterator_rect_delegate;
		internal static void sk_region_iterator_rect (sk_region_iterator_t iter, SKRectI* rect) =>
			(sk_region_iterator_rect_delegate ??= GetSymbol<Delegates.sk_region_iterator_rect> ("sk_region_iterator_rect")).Invoke (iter, rect);
		#endif

		// bool sk_region_iterator_rewind(sk_region_iterator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_iterator_rewind (sk_region_iterator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_iterator_rewind (sk_region_iterator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_iterator_rewind (sk_region_iterator_t iter);
		}
		private static Delegates.sk_region_iterator_rewind sk_region_iterator_rewind_delegate;
		internal static bool sk_region_iterator_rewind (sk_region_iterator_t iter) =>
			(sk_region_iterator_rewind_delegate ??= GetSymbol<Delegates.sk_region_iterator_rewind> ("sk_region_iterator_rewind")).Invoke (iter);
		#endif

		// sk_region_t* sk_region_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_region_t sk_region_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_t sk_region_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_t sk_region_new ();
		}
		private static Delegates.sk_region_new sk_region_new_delegate;
		internal static sk_region_t sk_region_new () =>
			(sk_region_new_delegate ??= GetSymbol<Delegates.sk_region_new> ("sk_region_new")).Invoke ();
		#endif

		// bool sk_region_op(sk_region_t* r, const sk_region_t* region, sk_region_op_t op)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op);
		}
		private static Delegates.sk_region_op sk_region_op_delegate;
		internal static bool sk_region_op (sk_region_t r, sk_region_t region, SKRegionOperation op) =>
			(sk_region_op_delegate ??= GetSymbol<Delegates.sk_region_op> ("sk_region_op")).Invoke (r, region, op);
		#endif

		// bool sk_region_op_rect(sk_region_t* r, const sk_irect_t* rect, sk_region_op_t op)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op);
		}
		private static Delegates.sk_region_op_rect sk_region_op_rect_delegate;
		internal static bool sk_region_op_rect (sk_region_t r, SKRectI* rect, SKRegionOperation op) =>
			(sk_region_op_rect_delegate ??= GetSymbol<Delegates.sk_region_op_rect> ("sk_region_op_rect")).Invoke (r, rect, op);
		#endif

		// bool sk_region_quick_contains(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_quick_contains (sk_region_t r, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_contains (sk_region_t r, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_quick_contains (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_quick_contains sk_region_quick_contains_delegate;
		internal static bool sk_region_quick_contains (sk_region_t r, SKRectI* rect) =>
			(sk_region_quick_contains_delegate ??= GetSymbol<Delegates.sk_region_quick_contains> ("sk_region_quick_contains")).Invoke (r, rect);
		#endif

		// bool sk_region_quick_reject(const sk_region_t* r, const sk_region_t* region)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_quick_reject (sk_region_t r, sk_region_t region);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_reject (sk_region_t r, sk_region_t region);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_quick_reject (sk_region_t r, sk_region_t region);
		}
		private static Delegates.sk_region_quick_reject sk_region_quick_reject_delegate;
		internal static bool sk_region_quick_reject (sk_region_t r, sk_region_t region) =>
			(sk_region_quick_reject_delegate ??= GetSymbol<Delegates.sk_region_quick_reject> ("sk_region_quick_reject")).Invoke (r, region);
		#endif

		// bool sk_region_quick_reject_rect(const sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_quick_reject_rect sk_region_quick_reject_rect_delegate;
		internal static bool sk_region_quick_reject_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_quick_reject_rect_delegate ??= GetSymbol<Delegates.sk_region_quick_reject_rect> ("sk_region_quick_reject_rect")).Invoke (r, rect);
		#endif

		// bool sk_region_set_empty(sk_region_t* r)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_set_empty (sk_region_t r);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_empty (sk_region_t r);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_empty (sk_region_t r);
		}
		private static Delegates.sk_region_set_empty sk_region_set_empty_delegate;
		internal static bool sk_region_set_empty (sk_region_t r) =>
			(sk_region_set_empty_delegate ??= GetSymbol<Delegates.sk_region_set_empty> ("sk_region_set_empty")).Invoke (r);
		#endif

		// bool sk_region_set_path(sk_region_t* r, const sk_path_t* t, const sk_region_t* clip)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip);
		}
		private static Delegates.sk_region_set_path sk_region_set_path_delegate;
		internal static bool sk_region_set_path (sk_region_t r, sk_path_t t, sk_region_t clip) =>
			(sk_region_set_path_delegate ??= GetSymbol<Delegates.sk_region_set_path> ("sk_region_set_path")).Invoke (r, t, clip);
		#endif

		// bool sk_region_set_rect(sk_region_t* r, const sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_set_rect (sk_region_t r, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_rect (sk_region_t r, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_rect (sk_region_t r, SKRectI* rect);
		}
		private static Delegates.sk_region_set_rect sk_region_set_rect_delegate;
		internal static bool sk_region_set_rect (sk_region_t r, SKRectI* rect) =>
			(sk_region_set_rect_delegate ??= GetSymbol<Delegates.sk_region_set_rect> ("sk_region_set_rect")).Invoke (r, rect);
		#endif

		// bool sk_region_set_rects(sk_region_t* r, const sk_irect_t* rects, int count)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count);
		}
		private static Delegates.sk_region_set_rects sk_region_set_rects_delegate;
		internal static bool sk_region_set_rects (sk_region_t r, SKRectI* rects, Int32 count) =>
			(sk_region_set_rects_delegate ??= GetSymbol<Delegates.sk_region_set_rects> ("sk_region_set_rects")).Invoke (r, rects, count);
		#endif

		// bool sk_region_set_region(sk_region_t* r, const sk_region_t* region)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_set_region (sk_region_t r, sk_region_t region);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_set_region (sk_region_t r, sk_region_t region);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_set_region (sk_region_t r, sk_region_t region);
		}
		private static Delegates.sk_region_set_region sk_region_set_region_delegate;
		internal static bool sk_region_set_region (sk_region_t r, sk_region_t region) =>
			(sk_region_set_region_delegate ??= GetSymbol<Delegates.sk_region_set_region> ("sk_region_set_region")).Invoke (r, region);
		#endif

		// void sk_region_spanerator_delete(sk_region_spanerator_t* iter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_spanerator_delete (sk_region_spanerator_t iter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_spanerator_delete (sk_region_spanerator_t iter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_spanerator_delete (sk_region_spanerator_t iter);
		}
		private static Delegates.sk_region_spanerator_delete sk_region_spanerator_delete_delegate;
		internal static void sk_region_spanerator_delete (sk_region_spanerator_t iter) =>
			(sk_region_spanerator_delete_delegate ??= GetSymbol<Delegates.sk_region_spanerator_delete> ("sk_region_spanerator_delete")).Invoke (iter);
		#endif

		// sk_region_spanerator_t* sk_region_spanerator_new(const sk_region_t* region, int y, int left, int right)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right);
		}
		private static Delegates.sk_region_spanerator_new sk_region_spanerator_new_delegate;
		internal static sk_region_spanerator_t sk_region_spanerator_new (sk_region_t region, Int32 y, Int32 left, Int32 right) =>
			(sk_region_spanerator_new_delegate ??= GetSymbol<Delegates.sk_region_spanerator_new> ("sk_region_spanerator_new")).Invoke (region, y, left, right);
		#endif

		// bool sk_region_spanerator_next(sk_region_spanerator_t* iter, int* left, int* right)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right);
		}
		private static Delegates.sk_region_spanerator_next sk_region_spanerator_next_delegate;
		internal static bool sk_region_spanerator_next (sk_region_spanerator_t iter, Int32* left, Int32* right) =>
			(sk_region_spanerator_next_delegate ??= GetSymbol<Delegates.sk_region_spanerator_next> ("sk_region_spanerator_next")).Invoke (iter, left, right);
		#endif

		// void sk_region_translate(sk_region_t* r, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_region_translate (sk_region_t r, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_region_translate (sk_region_t r, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_region_translate (sk_region_t r, Int32 x, Int32 y);
		}
		private static Delegates.sk_region_translate sk_region_translate_delegate;
		internal static void sk_region_translate (sk_region_t r, Int32 x, Int32 y) =>
			(sk_region_translate_delegate ??= GetSymbol<Delegates.sk_region_translate> ("sk_region_translate")).Invoke (r, x, y);
		#endif

		#endregion

	}
}
