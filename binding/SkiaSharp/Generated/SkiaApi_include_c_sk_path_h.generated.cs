using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_path.h

		// void sk_opbuilder_add(sk_opbuilder_t* builder, const sk_path_t* path, sk_pathop_t op)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_opbuilder_add (IntPtr builder, IntPtr path, SKPathOp op);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_opbuilder_add (IntPtr builder, IntPtr path, SKPathOp op);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_opbuilder_add (IntPtr builder, IntPtr path, SKPathOp op);
		}
		private static Delegates.sk_opbuilder_add sk_opbuilder_add_delegate;
		internal static void sk_opbuilder_add (IntPtr builder, IntPtr path, SKPathOp op) =>
			(sk_opbuilder_add_delegate ??= GetSymbol<Delegates.sk_opbuilder_add> ("sk_opbuilder_add")).Invoke (builder, path, op);
		#endif

		// void sk_opbuilder_destroy(sk_opbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_opbuilder_destroy (IntPtr builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_opbuilder_destroy (IntPtr builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_opbuilder_destroy (IntPtr builder);
		}
		private static Delegates.sk_opbuilder_destroy sk_opbuilder_destroy_delegate;
		internal static void sk_opbuilder_destroy (IntPtr builder) =>
			(sk_opbuilder_destroy_delegate ??= GetSymbol<Delegates.sk_opbuilder_destroy> ("sk_opbuilder_destroy")).Invoke (builder);
		#endif

		// sk_opbuilder_t* sk_opbuilder_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_opbuilder_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_opbuilder_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_opbuilder_new ();
		}
		private static Delegates.sk_opbuilder_new sk_opbuilder_new_delegate;
		internal static IntPtr sk_opbuilder_new () =>
			(sk_opbuilder_new_delegate ??= GetSymbol<Delegates.sk_opbuilder_new> ("sk_opbuilder_new")).Invoke ();
		#endif

		// bool sk_opbuilder_resolve(sk_opbuilder_t* builder, sk_path_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_opbuilder_resolve (IntPtr builder, IntPtr result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_opbuilder_resolve (IntPtr builder, IntPtr result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_opbuilder_resolve (IntPtr builder, IntPtr result);
		}
		private static Delegates.sk_opbuilder_resolve sk_opbuilder_resolve_delegate;
		internal static bool sk_opbuilder_resolve (IntPtr builder, IntPtr result) =>
			(sk_opbuilder_resolve_delegate ??= GetSymbol<Delegates.sk_opbuilder_resolve> ("sk_opbuilder_resolve")).Invoke (builder, result);
		#endif

		// sk_path_t* sk_path_clone(const sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_path_clone (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_path_clone (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_path_clone (IntPtr cpath);
		}
		private static Delegates.sk_path_clone sk_path_clone_delegate;
		internal static IntPtr sk_path_clone (IntPtr cpath) =>
			(sk_path_clone_delegate ??= GetSymbol<Delegates.sk_path_clone> ("sk_path_clone")).Invoke (cpath);
		#endif

		// void sk_path_compute_tight_bounds(const sk_path_t*, sk_rect_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_compute_tight_bounds (IntPtr param0, SKRect* param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_compute_tight_bounds (IntPtr param0, SKRect* param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_compute_tight_bounds (IntPtr param0, SKRect* param1);
		}
		private static Delegates.sk_path_compute_tight_bounds sk_path_compute_tight_bounds_delegate;
		internal static void sk_path_compute_tight_bounds (IntPtr param0, SKRect* param1) =>
			(sk_path_compute_tight_bounds_delegate ??= GetSymbol<Delegates.sk_path_compute_tight_bounds> ("sk_path_compute_tight_bounds")).Invoke (param0, param1);
		#endif

		// bool sk_path_contains(const sk_path_t* cpath, float x, float y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_contains (IntPtr cpath, Single x, Single y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_contains (IntPtr cpath, Single x, Single y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_contains (IntPtr cpath, Single x, Single y);
		}
		private static Delegates.sk_path_contains sk_path_contains_delegate;
		internal static bool sk_path_contains (IntPtr cpath, Single x, Single y) =>
			(sk_path_contains_delegate ??= GetSymbol<Delegates.sk_path_contains> ("sk_path_contains")).Invoke (cpath, x, y);
		#endif

		// int sk_path_convert_conic_to_quads(const sk_point_t* p0, const sk_point_t* p1, const sk_point_t* p2, float w, sk_point_t* pts, int pow2)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2);
		}
		private static Delegates.sk_path_convert_conic_to_quads sk_path_convert_conic_to_quads_delegate;
		internal static Int32 sk_path_convert_conic_to_quads (SKPoint* p0, SKPoint* p1, SKPoint* p2, Single w, SKPoint* pts, Int32 pow2) =>
			(sk_path_convert_conic_to_quads_delegate ??= GetSymbol<Delegates.sk_path_convert_conic_to_quads> ("sk_path_convert_conic_to_quads")).Invoke (p0, p1, p2, w, pts, pow2);
		#endif

		// int sk_path_count_points(const sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_path_count_points (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_count_points (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_count_points (IntPtr cpath);
		}
		private static Delegates.sk_path_count_points sk_path_count_points_delegate;
		internal static Int32 sk_path_count_points (IntPtr cpath) =>
			(sk_path_count_points_delegate ??= GetSymbol<Delegates.sk_path_count_points> ("sk_path_count_points")).Invoke (cpath);
		#endif

		// int sk_path_count_verbs(const sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_path_count_verbs (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_count_verbs (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_count_verbs (IntPtr cpath);
		}
		private static Delegates.sk_path_count_verbs sk_path_count_verbs_delegate;
		internal static Int32 sk_path_count_verbs (IntPtr cpath) =>
			(sk_path_count_verbs_delegate ??= GetSymbol<Delegates.sk_path_count_verbs> ("sk_path_count_verbs")).Invoke (cpath);
		#endif

		// sk_path_iterator_t* sk_path_create_iter(sk_path_t* cpath, int forceClose)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_path_create_iter (IntPtr cpath, Int32 forceClose);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_path_create_iter (IntPtr cpath, Int32 forceClose);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_path_create_iter (IntPtr cpath, Int32 forceClose);
		}
		private static Delegates.sk_path_create_iter sk_path_create_iter_delegate;
		internal static IntPtr sk_path_create_iter (IntPtr cpath, Int32 forceClose) =>
			(sk_path_create_iter_delegate ??= GetSymbol<Delegates.sk_path_create_iter> ("sk_path_create_iter")).Invoke (cpath, forceClose);
		#endif

		// sk_path_rawiterator_t* sk_path_create_rawiter(sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_path_create_rawiter (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_path_create_rawiter (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_path_create_rawiter (IntPtr cpath);
		}
		private static Delegates.sk_path_create_rawiter sk_path_create_rawiter_delegate;
		internal static IntPtr sk_path_create_rawiter (IntPtr cpath) =>
			(sk_path_create_rawiter_delegate ??= GetSymbol<Delegates.sk_path_create_rawiter> ("sk_path_create_rawiter")).Invoke (cpath);
		#endif

		// void sk_path_delete(sk_path_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_delete (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_delete (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_delete (IntPtr param0);
		}
		private static Delegates.sk_path_delete sk_path_delete_delegate;
		internal static void sk_path_delete (IntPtr param0) =>
			(sk_path_delete_delegate ??= GetSymbol<Delegates.sk_path_delete> ("sk_path_delete")).Invoke (param0);
		#endif

		// void sk_path_get_bounds(const sk_path_t*, sk_rect_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_get_bounds (IntPtr param0, SKRect* param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_get_bounds (IntPtr param0, SKRect* param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_get_bounds (IntPtr param0, SKRect* param1);
		}
		private static Delegates.sk_path_get_bounds sk_path_get_bounds_delegate;
		internal static void sk_path_get_bounds (IntPtr param0, SKRect* param1) =>
			(sk_path_get_bounds_delegate ??= GetSymbol<Delegates.sk_path_get_bounds> ("sk_path_get_bounds")).Invoke (param0, param1);
		#endif

		// sk_path_filltype_t sk_path_get_filltype(sk_path_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPathFillType sk_path_get_filltype (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathFillType sk_path_get_filltype (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathFillType sk_path_get_filltype (IntPtr param0);
		}
		private static Delegates.sk_path_get_filltype sk_path_get_filltype_delegate;
		internal static SKPathFillType sk_path_get_filltype (IntPtr param0) =>
			(sk_path_get_filltype_delegate ??= GetSymbol<Delegates.sk_path_get_filltype> ("sk_path_get_filltype")).Invoke (param0);
		#endif

		// bool sk_path_get_last_point(const sk_path_t* cpath, sk_point_t* point)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_get_last_point (IntPtr cpath, SKPoint* point);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_get_last_point (IntPtr cpath, SKPoint* point);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_get_last_point (IntPtr cpath, SKPoint* point);
		}
		private static Delegates.sk_path_get_last_point sk_path_get_last_point_delegate;
		internal static bool sk_path_get_last_point (IntPtr cpath, SKPoint* point) =>
			(sk_path_get_last_point_delegate ??= GetSymbol<Delegates.sk_path_get_last_point> ("sk_path_get_last_point")).Invoke (cpath, point);
		#endif

		// void sk_path_get_point(const sk_path_t* cpath, int index, sk_point_t* point)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_get_point (IntPtr cpath, Int32 index, SKPoint* point);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_get_point (IntPtr cpath, Int32 index, SKPoint* point);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_get_point (IntPtr cpath, Int32 index, SKPoint* point);
		}
		private static Delegates.sk_path_get_point sk_path_get_point_delegate;
		internal static void sk_path_get_point (IntPtr cpath, Int32 index, SKPoint* point) =>
			(sk_path_get_point_delegate ??= GetSymbol<Delegates.sk_path_get_point> ("sk_path_get_point")).Invoke (cpath, index, point);
		#endif

		// int sk_path_get_points(const sk_path_t* cpath, sk_point_t* points, int max)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_path_get_points (IntPtr cpath, SKPoint* points, Int32 max);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_get_points (IntPtr cpath, SKPoint* points, Int32 max);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_get_points (IntPtr cpath, SKPoint* points, Int32 max);
		}
		private static Delegates.sk_path_get_points sk_path_get_points_delegate;
		internal static Int32 sk_path_get_points (IntPtr cpath, SKPoint* points, Int32 max) =>
			(sk_path_get_points_delegate ??= GetSymbol<Delegates.sk_path_get_points> ("sk_path_get_points")).Invoke (cpath, points, max);
		#endif

		// uint32_t sk_path_get_segment_masks(sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_path_get_segment_masks (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_path_get_segment_masks (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_path_get_segment_masks (IntPtr cpath);
		}
		private static Delegates.sk_path_get_segment_masks sk_path_get_segment_masks_delegate;
		internal static UInt32 sk_path_get_segment_masks (IntPtr cpath) =>
			(sk_path_get_segment_masks_delegate ??= GetSymbol<Delegates.sk_path_get_segment_masks> ("sk_path_get_segment_masks")).Invoke (cpath);
		#endif

		// bool sk_path_is_convex(const sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_is_convex (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_convex (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_convex (IntPtr cpath);
		}
		private static Delegates.sk_path_is_convex sk_path_is_convex_delegate;
		internal static bool sk_path_is_convex (IntPtr cpath) =>
			(sk_path_is_convex_delegate ??= GetSymbol<Delegates.sk_path_is_convex> ("sk_path_is_convex")).Invoke (cpath);
		#endif

		// bool sk_path_is_line(sk_path_t* cpath, sk_point_t[2] line = 2)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_is_line (IntPtr cpath, SKPoint* line);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_line (IntPtr cpath, SKPoint* line);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_line (IntPtr cpath, SKPoint* line);
		}
		private static Delegates.sk_path_is_line sk_path_is_line_delegate;
		internal static bool sk_path_is_line (IntPtr cpath, SKPoint* line) =>
			(sk_path_is_line_delegate ??= GetSymbol<Delegates.sk_path_is_line> ("sk_path_is_line")).Invoke (cpath, line);
		#endif

		// bool sk_path_is_oval(sk_path_t* cpath, sk_rect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_is_oval (IntPtr cpath, SKRect* bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_oval (IntPtr cpath, SKRect* bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_oval (IntPtr cpath, SKRect* bounds);
		}
		private static Delegates.sk_path_is_oval sk_path_is_oval_delegate;
		internal static bool sk_path_is_oval (IntPtr cpath, SKRect* bounds) =>
			(sk_path_is_oval_delegate ??= GetSymbol<Delegates.sk_path_is_oval> ("sk_path_is_oval")).Invoke (cpath, bounds);
		#endif

		// bool sk_path_is_rect(sk_path_t* cpath, sk_rect_t* rect, bool* isClosed, sk_path_direction_t* direction)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_is_rect (IntPtr cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_rect (IntPtr cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_rect (IntPtr cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction);
		}
		private static Delegates.sk_path_is_rect sk_path_is_rect_delegate;
		internal static bool sk_path_is_rect (IntPtr cpath, SKRect* rect, Byte* isClosed, SKPathDirection* direction) =>
			(sk_path_is_rect_delegate ??= GetSymbol<Delegates.sk_path_is_rect> ("sk_path_is_rect")).Invoke (cpath, rect, isClosed, direction);
		#endif

		// bool sk_path_is_rrect(sk_path_t* cpath, sk_rrect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_is_rrect (IntPtr cpath, IntPtr bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_is_rrect (IntPtr cpath, IntPtr bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_is_rrect (IntPtr cpath, IntPtr bounds);
		}
		private static Delegates.sk_path_is_rrect sk_path_is_rrect_delegate;
		internal static bool sk_path_is_rrect (IntPtr cpath, IntPtr bounds) =>
			(sk_path_is_rrect_delegate ??= GetSymbol<Delegates.sk_path_is_rrect> ("sk_path_is_rrect")).Invoke (cpath, bounds);
		#endif

		// float sk_path_iter_conic_weight(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_path_iter_conic_weight (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_path_iter_conic_weight (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_path_iter_conic_weight (IntPtr iterator);
		}
		private static Delegates.sk_path_iter_conic_weight sk_path_iter_conic_weight_delegate;
		internal static Single sk_path_iter_conic_weight (IntPtr iterator) =>
			(sk_path_iter_conic_weight_delegate ??= GetSymbol<Delegates.sk_path_iter_conic_weight> ("sk_path_iter_conic_weight")).Invoke (iterator);
		#endif

		// void sk_path_iter_destroy(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_iter_destroy (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_iter_destroy (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_iter_destroy (IntPtr iterator);
		}
		private static Delegates.sk_path_iter_destroy sk_path_iter_destroy_delegate;
		internal static void sk_path_iter_destroy (IntPtr iterator) =>
			(sk_path_iter_destroy_delegate ??= GetSymbol<Delegates.sk_path_iter_destroy> ("sk_path_iter_destroy")).Invoke (iterator);
		#endif

		// int sk_path_iter_is_close_line(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_path_iter_is_close_line (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_iter_is_close_line (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_iter_is_close_line (IntPtr iterator);
		}
		private static Delegates.sk_path_iter_is_close_line sk_path_iter_is_close_line_delegate;
		internal static Int32 sk_path_iter_is_close_line (IntPtr iterator) =>
			(sk_path_iter_is_close_line_delegate ??= GetSymbol<Delegates.sk_path_iter_is_close_line> ("sk_path_iter_is_close_line")).Invoke (iterator);
		#endif

		// int sk_path_iter_is_closed_contour(sk_path_iterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_path_iter_is_closed_contour (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_path_iter_is_closed_contour (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_path_iter_is_closed_contour (IntPtr iterator);
		}
		private static Delegates.sk_path_iter_is_closed_contour sk_path_iter_is_closed_contour_delegate;
		internal static Int32 sk_path_iter_is_closed_contour (IntPtr iterator) =>
			(sk_path_iter_is_closed_contour_delegate ??= GetSymbol<Delegates.sk_path_iter_is_closed_contour> ("sk_path_iter_is_closed_contour")).Invoke (iterator);
		#endif

		// sk_path_verb_t sk_path_iter_next(sk_path_iterator_t* iterator, sk_point_t[4] points = 4)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPathVerb sk_path_iter_next (IntPtr iterator, SKPoint* points);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_iter_next (IntPtr iterator, SKPoint* points);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathVerb sk_path_iter_next (IntPtr iterator, SKPoint* points);
		}
		private static Delegates.sk_path_iter_next sk_path_iter_next_delegate;
		internal static SKPathVerb sk_path_iter_next (IntPtr iterator, SKPoint* points) =>
			(sk_path_iter_next_delegate ??= GetSymbol<Delegates.sk_path_iter_next> ("sk_path_iter_next")).Invoke (iterator, points);
		#endif

		// sk_path_t* sk_path_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_path_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_path_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_path_new ();
		}
		private static Delegates.sk_path_new sk_path_new_delegate;
		internal static IntPtr sk_path_new () =>
			(sk_path_new_delegate ??= GetSymbol<Delegates.sk_path_new> ("sk_path_new")).Invoke ();
		#endif

		// bool sk_path_parse_svg_string(sk_path_t* cpath, const char* str)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_path_parse_svg_string (IntPtr cpath, [MarshalAs (UnmanagedType.LPStr)] String str);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_path_parse_svg_string (IntPtr cpath, [MarshalAs (UnmanagedType.LPStr)] String str);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_path_parse_svg_string (IntPtr cpath, [MarshalAs (UnmanagedType.LPStr)] String str);
		}
		private static Delegates.sk_path_parse_svg_string sk_path_parse_svg_string_delegate;
		internal static bool sk_path_parse_svg_string (IntPtr cpath, [MarshalAs (UnmanagedType.LPStr)] String str) =>
			(sk_path_parse_svg_string_delegate ??= GetSymbol<Delegates.sk_path_parse_svg_string> ("sk_path_parse_svg_string")).Invoke (cpath, str);
		#endif

		// float sk_path_rawiter_conic_weight(sk_path_rawiterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_path_rawiter_conic_weight (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_path_rawiter_conic_weight (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_path_rawiter_conic_weight (IntPtr iterator);
		}
		private static Delegates.sk_path_rawiter_conic_weight sk_path_rawiter_conic_weight_delegate;
		internal static Single sk_path_rawiter_conic_weight (IntPtr iterator) =>
			(sk_path_rawiter_conic_weight_delegate ??= GetSymbol<Delegates.sk_path_rawiter_conic_weight> ("sk_path_rawiter_conic_weight")).Invoke (iterator);
		#endif

		// void sk_path_rawiter_destroy(sk_path_rawiterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_rawiter_destroy (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rawiter_destroy (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rawiter_destroy (IntPtr iterator);
		}
		private static Delegates.sk_path_rawiter_destroy sk_path_rawiter_destroy_delegate;
		internal static void sk_path_rawiter_destroy (IntPtr iterator) =>
			(sk_path_rawiter_destroy_delegate ??= GetSymbol<Delegates.sk_path_rawiter_destroy> ("sk_path_rawiter_destroy")).Invoke (iterator);
		#endif

		// sk_path_verb_t sk_path_rawiter_next(sk_path_rawiterator_t* iterator, sk_point_t[4] points = 4)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPathVerb sk_path_rawiter_next (IntPtr iterator, SKPoint* points);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_rawiter_next (IntPtr iterator, SKPoint* points);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathVerb sk_path_rawiter_next (IntPtr iterator, SKPoint* points);
		}
		private static Delegates.sk_path_rawiter_next sk_path_rawiter_next_delegate;
		internal static SKPathVerb sk_path_rawiter_next (IntPtr iterator, SKPoint* points) =>
			(sk_path_rawiter_next_delegate ??= GetSymbol<Delegates.sk_path_rawiter_next> ("sk_path_rawiter_next")).Invoke (iterator, points);
		#endif

		// sk_path_verb_t sk_path_rawiter_peek(sk_path_rawiterator_t* iterator)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPathVerb sk_path_rawiter_peek (IntPtr iterator);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathVerb sk_path_rawiter_peek (IntPtr iterator);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathVerb sk_path_rawiter_peek (IntPtr iterator);
		}
		private static Delegates.sk_path_rawiter_peek sk_path_rawiter_peek_delegate;
		internal static SKPathVerb sk_path_rawiter_peek (IntPtr iterator) =>
			(sk_path_rawiter_peek_delegate ??= GetSymbol<Delegates.sk_path_rawiter_peek> ("sk_path_rawiter_peek")).Invoke (iterator);
		#endif

		// void sk_path_reset(sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_reset (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_reset (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_reset (IntPtr cpath);
		}
		private static Delegates.sk_path_reset sk_path_reset_delegate;
		internal static void sk_path_reset (IntPtr cpath) =>
			(sk_path_reset_delegate ??= GetSymbol<Delegates.sk_path_reset> ("sk_path_reset")).Invoke (cpath);
		#endif

		// void sk_path_rewind(sk_path_t* cpath)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_rewind (IntPtr cpath);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_rewind (IntPtr cpath);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_rewind (IntPtr cpath);
		}
		private static Delegates.sk_path_rewind sk_path_rewind_delegate;
		internal static void sk_path_rewind (IntPtr cpath) =>
			(sk_path_rewind_delegate ??= GetSymbol<Delegates.sk_path_rewind> ("sk_path_rewind")).Invoke (cpath);
		#endif

		// void sk_path_set_filltype(sk_path_t*, sk_path_filltype_t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_set_filltype (IntPtr param0, SKPathFillType param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_set_filltype (IntPtr param0, SKPathFillType param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_set_filltype (IntPtr param0, SKPathFillType param1);
		}
		private static Delegates.sk_path_set_filltype sk_path_set_filltype_delegate;
		internal static void sk_path_set_filltype (IntPtr param0, SKPathFillType param1) =>
			(sk_path_set_filltype_delegate ??= GetSymbol<Delegates.sk_path_set_filltype> ("sk_path_set_filltype")).Invoke (param0, param1);
		#endif

		// void sk_path_to_svg_string(const sk_path_t* cpath, sk_string_t* str)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_to_svg_string (IntPtr cpath, IntPtr str);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_to_svg_string (IntPtr cpath, IntPtr str);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_to_svg_string (IntPtr cpath, IntPtr str);
		}
		private static Delegates.sk_path_to_svg_string sk_path_to_svg_string_delegate;
		internal static void sk_path_to_svg_string (IntPtr cpath, IntPtr str) =>
			(sk_path_to_svg_string_delegate ??= GetSymbol<Delegates.sk_path_to_svg_string> ("sk_path_to_svg_string")).Invoke (cpath, str);
		#endif

		// void sk_path_transform(sk_path_t* cpath, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_transform (IntPtr cpath, SKMatrix* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_transform (IntPtr cpath, SKMatrix* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_transform (IntPtr cpath, SKMatrix* cmatrix);
		}
		private static Delegates.sk_path_transform sk_path_transform_delegate;
		internal static void sk_path_transform (IntPtr cpath, SKMatrix* cmatrix) =>
			(sk_path_transform_delegate ??= GetSymbol<Delegates.sk_path_transform> ("sk_path_transform")).Invoke (cpath, cmatrix);
		#endif

		// void sk_path_transform_to_dest(const sk_path_t* cpath, const sk_matrix_t* cmatrix, sk_path_t* destination)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_transform_to_dest (IntPtr cpath, SKMatrix* cmatrix, IntPtr destination);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_transform_to_dest (IntPtr cpath, SKMatrix* cmatrix, IntPtr destination);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_transform_to_dest (IntPtr cpath, SKMatrix* cmatrix, IntPtr destination);
		}
		private static Delegates.sk_path_transform_to_dest sk_path_transform_to_dest_delegate;
		internal static void sk_path_transform_to_dest (IntPtr cpath, SKMatrix* cmatrix, IntPtr destination) =>
			(sk_path_transform_to_dest_delegate ??= GetSymbol<Delegates.sk_path_transform_to_dest> ("sk_path_transform_to_dest")).Invoke (cpath, cmatrix, destination);
		#endif

		// void sk_pathmeasure_destroy(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathmeasure_destroy (IntPtr pathMeasure);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathmeasure_destroy (IntPtr pathMeasure);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathmeasure_destroy (IntPtr pathMeasure);
		}
		private static Delegates.sk_pathmeasure_destroy sk_pathmeasure_destroy_delegate;
		internal static void sk_pathmeasure_destroy (IntPtr pathMeasure) =>
			(sk_pathmeasure_destroy_delegate ??= GetSymbol<Delegates.sk_pathmeasure_destroy> ("sk_pathmeasure_destroy")).Invoke (pathMeasure);
		#endif

		// float sk_pathmeasure_get_length(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_pathmeasure_get_length (IntPtr pathMeasure);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_pathmeasure_get_length (IntPtr pathMeasure);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_pathmeasure_get_length (IntPtr pathMeasure);
		}
		private static Delegates.sk_pathmeasure_get_length sk_pathmeasure_get_length_delegate;
		internal static Single sk_pathmeasure_get_length (IntPtr pathMeasure) =>
			(sk_pathmeasure_get_length_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_length> ("sk_pathmeasure_get_length")).Invoke (pathMeasure);
		#endif

		// bool sk_pathmeasure_get_matrix(sk_pathmeasure_t* pathMeasure, float distance, sk_matrix_t* matrix, sk_pathmeasure_matrixflags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathmeasure_get_matrix (IntPtr pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_matrix (IntPtr pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_get_matrix (IntPtr pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags);
		}
		private static Delegates.sk_pathmeasure_get_matrix sk_pathmeasure_get_matrix_delegate;
		internal static bool sk_pathmeasure_get_matrix (IntPtr pathMeasure, Single distance, SKMatrix* matrix, SKPathMeasureMatrixFlags flags) =>
			(sk_pathmeasure_get_matrix_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_matrix> ("sk_pathmeasure_get_matrix")).Invoke (pathMeasure, distance, matrix, flags);
		#endif

		// bool sk_pathmeasure_get_pos_tan(sk_pathmeasure_t* pathMeasure, float distance, sk_point_t* position, sk_vector_t* tangent)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathmeasure_get_pos_tan (IntPtr pathMeasure, Single distance, SKPoint* position, SKPoint* tangent);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_pos_tan (IntPtr pathMeasure, Single distance, SKPoint* position, SKPoint* tangent);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_get_pos_tan (IntPtr pathMeasure, Single distance, SKPoint* position, SKPoint* tangent);
		}
		private static Delegates.sk_pathmeasure_get_pos_tan sk_pathmeasure_get_pos_tan_delegate;
		internal static bool sk_pathmeasure_get_pos_tan (IntPtr pathMeasure, Single distance, SKPoint* position, SKPoint* tangent) =>
			(sk_pathmeasure_get_pos_tan_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_pos_tan> ("sk_pathmeasure_get_pos_tan")).Invoke (pathMeasure, distance, position, tangent);
		#endif

		// bool sk_pathmeasure_get_segment(sk_pathmeasure_t* pathMeasure, float start, float stop, sk_pathbuilder_t* dst, bool startWithMoveTo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathmeasure_get_segment (IntPtr pathMeasure, Single start, Single stop, IntPtr dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_get_segment (IntPtr pathMeasure, Single start, Single stop, IntPtr dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_get_segment (IntPtr pathMeasure, Single start, Single stop, IntPtr dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo);
		}
		private static Delegates.sk_pathmeasure_get_segment sk_pathmeasure_get_segment_delegate;
		internal static bool sk_pathmeasure_get_segment (IntPtr pathMeasure, Single start, Single stop, IntPtr dst, [MarshalAs (UnmanagedType.I1)] bool startWithMoveTo) =>
			(sk_pathmeasure_get_segment_delegate ??= GetSymbol<Delegates.sk_pathmeasure_get_segment> ("sk_pathmeasure_get_segment")).Invoke (pathMeasure, start, stop, dst, startWithMoveTo);
		#endif

		// bool sk_pathmeasure_is_closed(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathmeasure_is_closed (IntPtr pathMeasure);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_is_closed (IntPtr pathMeasure);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_is_closed (IntPtr pathMeasure);
		}
		private static Delegates.sk_pathmeasure_is_closed sk_pathmeasure_is_closed_delegate;
		internal static bool sk_pathmeasure_is_closed (IntPtr pathMeasure) =>
			(sk_pathmeasure_is_closed_delegate ??= GetSymbol<Delegates.sk_pathmeasure_is_closed> ("sk_pathmeasure_is_closed")).Invoke (pathMeasure);
		#endif

		// sk_pathmeasure_t* sk_pathmeasure_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_pathmeasure_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_pathmeasure_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_pathmeasure_new ();
		}
		private static Delegates.sk_pathmeasure_new sk_pathmeasure_new_delegate;
		internal static IntPtr sk_pathmeasure_new () =>
			(sk_pathmeasure_new_delegate ??= GetSymbol<Delegates.sk_pathmeasure_new> ("sk_pathmeasure_new")).Invoke ();
		#endif

		// sk_pathmeasure_t* sk_pathmeasure_new_with_path(const sk_path_t* path, bool forceClosed, float resScale)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_pathmeasure_new_with_path (IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_pathmeasure_new_with_path (IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_pathmeasure_new_with_path (IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale);
		}
		private static Delegates.sk_pathmeasure_new_with_path sk_pathmeasure_new_with_path_delegate;
		internal static IntPtr sk_pathmeasure_new_with_path (IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed, Single resScale) =>
			(sk_pathmeasure_new_with_path_delegate ??= GetSymbol<Delegates.sk_pathmeasure_new_with_path> ("sk_pathmeasure_new_with_path")).Invoke (path, forceClosed, resScale);
		#endif

		// bool sk_pathmeasure_next_contour(sk_pathmeasure_t* pathMeasure)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathmeasure_next_contour (IntPtr pathMeasure);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathmeasure_next_contour (IntPtr pathMeasure);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathmeasure_next_contour (IntPtr pathMeasure);
		}
		private static Delegates.sk_pathmeasure_next_contour sk_pathmeasure_next_contour_delegate;
		internal static bool sk_pathmeasure_next_contour (IntPtr pathMeasure) =>
			(sk_pathmeasure_next_contour_delegate ??= GetSymbol<Delegates.sk_pathmeasure_next_contour> ("sk_pathmeasure_next_contour")).Invoke (pathMeasure);
		#endif

		// void sk_pathmeasure_set_path(sk_pathmeasure_t* pathMeasure, const sk_path_t* path, bool forceClosed)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathmeasure_set_path (IntPtr pathMeasure, IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathmeasure_set_path (IntPtr pathMeasure, IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathmeasure_set_path (IntPtr pathMeasure, IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed);
		}
		private static Delegates.sk_pathmeasure_set_path sk_pathmeasure_set_path_delegate;
		internal static void sk_pathmeasure_set_path (IntPtr pathMeasure, IntPtr path, [MarshalAs (UnmanagedType.I1)] bool forceClosed) =>
			(sk_pathmeasure_set_path_delegate ??= GetSymbol<Delegates.sk_pathmeasure_set_path> ("sk_pathmeasure_set_path")).Invoke (pathMeasure, path, forceClosed);
		#endif

		// bool sk_pathop_as_winding(const sk_path_t* path, sk_path_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathop_as_winding (IntPtr path, IntPtr result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_as_winding (IntPtr path, IntPtr result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_as_winding (IntPtr path, IntPtr result);
		}
		private static Delegates.sk_pathop_as_winding sk_pathop_as_winding_delegate;
		internal static bool sk_pathop_as_winding (IntPtr path, IntPtr result) =>
			(sk_pathop_as_winding_delegate ??= GetSymbol<Delegates.sk_pathop_as_winding> ("sk_pathop_as_winding")).Invoke (path, result);
		#endif

		// bool sk_pathop_op(const sk_path_t* one, const sk_path_t* two, sk_pathop_t op, sk_path_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathop_op (IntPtr one, IntPtr two, SKPathOp op, IntPtr result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_op (IntPtr one, IntPtr two, SKPathOp op, IntPtr result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_op (IntPtr one, IntPtr two, SKPathOp op, IntPtr result);
		}
		private static Delegates.sk_pathop_op sk_pathop_op_delegate;
		internal static bool sk_pathop_op (IntPtr one, IntPtr two, SKPathOp op, IntPtr result) =>
			(sk_pathop_op_delegate ??= GetSymbol<Delegates.sk_pathop_op> ("sk_pathop_op")).Invoke (one, two, op, result);
		#endif

		// bool sk_pathop_simplify(const sk_path_t* path, sk_path_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathop_simplify (IntPtr path, IntPtr result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_simplify (IntPtr path, IntPtr result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_simplify (IntPtr path, IntPtr result);
		}
		private static Delegates.sk_pathop_simplify sk_pathop_simplify_delegate;
		internal static bool sk_pathop_simplify (IntPtr path, IntPtr result) =>
			(sk_pathop_simplify_delegate ??= GetSymbol<Delegates.sk_pathop_simplify> ("sk_pathop_simplify")).Invoke (path, result);
		#endif

		// bool sk_pathop_tight_bounds(const sk_path_t* path, sk_rect_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pathop_tight_bounds (IntPtr path, SKRect* result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pathop_tight_bounds (IntPtr path, SKRect* result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pathop_tight_bounds (IntPtr path, SKRect* result);
		}
		private static Delegates.sk_pathop_tight_bounds sk_pathop_tight_bounds_delegate;
		internal static bool sk_pathop_tight_bounds (IntPtr path, SKRect* result) =>
			(sk_pathop_tight_bounds_delegate ??= GetSymbol<Delegates.sk_pathop_tight_bounds> ("sk_pathop_tight_bounds")).Invoke (path, result);
		#endif

		#endregion

	}
}
