using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_pathbuilder.h

		// void sk_pathbuilder_add_arc(sk_pathbuilder_t* builder, const sk_rect_t* rect, float startAngle, float sweepAngle)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_arc (sk_pathbuilder_t builder, SKRect* rect, Single startAngle, Single sweepAngle);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_arc (sk_pathbuilder_t builder, SKRect* rect, Single startAngle, Single sweepAngle);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_arc (sk_pathbuilder_t builder, SKRect* rect, Single startAngle, Single sweepAngle);
		}
		private static Delegates.sk_pathbuilder_add_arc sk_pathbuilder_add_arc_delegate;
		internal static void sk_pathbuilder_add_arc (sk_pathbuilder_t builder, SKRect* rect, Single startAngle, Single sweepAngle) =>
			(sk_pathbuilder_add_arc_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_arc> ("sk_pathbuilder_add_arc")).Invoke (builder, rect, startAngle, sweepAngle);
		#endif

		// void sk_pathbuilder_add_circle(sk_pathbuilder_t* builder, float x, float y, float radius, sk_path_direction_t dir)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_circle (sk_pathbuilder_t builder, Single x, Single y, Single radius, SKPathDirection dir);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_circle (sk_pathbuilder_t builder, Single x, Single y, Single radius, SKPathDirection dir);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_circle (sk_pathbuilder_t builder, Single x, Single y, Single radius, SKPathDirection dir);
		}
		private static Delegates.sk_pathbuilder_add_circle sk_pathbuilder_add_circle_delegate;
		internal static void sk_pathbuilder_add_circle (sk_pathbuilder_t builder, Single x, Single y, Single radius, SKPathDirection dir) =>
			(sk_pathbuilder_add_circle_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_circle> ("sk_pathbuilder_add_circle")).Invoke (builder, x, y, radius, dir);
		#endif

		// void sk_pathbuilder_add_oval(sk_pathbuilder_t* builder, const sk_rect_t* rect, sk_path_direction_t dir)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_oval (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_oval (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_oval (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir);
		}
		private static Delegates.sk_pathbuilder_add_oval sk_pathbuilder_add_oval_delegate;
		internal static void sk_pathbuilder_add_oval (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir) =>
			(sk_pathbuilder_add_oval_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_oval> ("sk_pathbuilder_add_oval")).Invoke (builder, rect, dir);
		#endif

		// void sk_pathbuilder_add_path(sk_pathbuilder_t* builder, const sk_path_t* other, sk_path_add_mode_t add_mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_path (sk_pathbuilder_t builder, sk_path_t other, SKPathAddMode add_mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_path (sk_pathbuilder_t builder, sk_path_t other, SKPathAddMode add_mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_path (sk_pathbuilder_t builder, sk_path_t other, SKPathAddMode add_mode);
		}
		private static Delegates.sk_pathbuilder_add_path sk_pathbuilder_add_path_delegate;
		internal static void sk_pathbuilder_add_path (sk_pathbuilder_t builder, sk_path_t other, SKPathAddMode add_mode) =>
			(sk_pathbuilder_add_path_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_path> ("sk_pathbuilder_add_path")).Invoke (builder, other, add_mode);
		#endif

		// void sk_pathbuilder_add_path_matrix(sk_pathbuilder_t* builder, const sk_path_t* other, sk_matrix_t* matrix, sk_path_add_mode_t add_mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_path_matrix (sk_pathbuilder_t builder, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_path_matrix (sk_pathbuilder_t builder, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_path_matrix (sk_pathbuilder_t builder, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode);
		}
		private static Delegates.sk_pathbuilder_add_path_matrix sk_pathbuilder_add_path_matrix_delegate;
		internal static void sk_pathbuilder_add_path_matrix (sk_pathbuilder_t builder, sk_path_t other, SKMatrix* matrix, SKPathAddMode add_mode) =>
			(sk_pathbuilder_add_path_matrix_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_path_matrix> ("sk_pathbuilder_add_path_matrix")).Invoke (builder, other, matrix, add_mode);
		#endif

		// void sk_pathbuilder_add_path_offset(sk_pathbuilder_t* builder, const sk_path_t* other, float dx, float dy, sk_path_add_mode_t add_mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_path_offset (sk_pathbuilder_t builder, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_path_offset (sk_pathbuilder_t builder, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_path_offset (sk_pathbuilder_t builder, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode);
		}
		private static Delegates.sk_pathbuilder_add_path_offset sk_pathbuilder_add_path_offset_delegate;
		internal static void sk_pathbuilder_add_path_offset (sk_pathbuilder_t builder, sk_path_t other, Single dx, Single dy, SKPathAddMode add_mode) =>
			(sk_pathbuilder_add_path_offset_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_path_offset> ("sk_pathbuilder_add_path_offset")).Invoke (builder, other, dx, dy, add_mode);
		#endif

		// void sk_pathbuilder_add_poly(sk_pathbuilder_t* builder, const sk_point_t* points, int count, bool close)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_poly (sk_pathbuilder_t builder, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_poly (sk_pathbuilder_t builder, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_poly (sk_pathbuilder_t builder, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close);
		}
		private static Delegates.sk_pathbuilder_add_poly sk_pathbuilder_add_poly_delegate;
		internal static void sk_pathbuilder_add_poly (sk_pathbuilder_t builder, SKPoint* points, Int32 count, [MarshalAs (UnmanagedType.I1)] bool close) =>
			(sk_pathbuilder_add_poly_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_poly> ("sk_pathbuilder_add_poly")).Invoke (builder, points, count, close);
		#endif

		// void sk_pathbuilder_add_rect(sk_pathbuilder_t* builder, const sk_rect_t* rect, sk_path_direction_t dir)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_rect (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_rect (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_rect (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir);
		}
		private static Delegates.sk_pathbuilder_add_rect sk_pathbuilder_add_rect_delegate;
		internal static void sk_pathbuilder_add_rect (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir) =>
			(sk_pathbuilder_add_rect_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_rect> ("sk_pathbuilder_add_rect")).Invoke (builder, rect, dir);
		#endif

		// void sk_pathbuilder_add_rect_start(sk_pathbuilder_t* builder, const sk_rect_t* rect, sk_path_direction_t dir, uint32_t startIndex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_rect_start (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir, UInt32 startIndex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_rect_start (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir, UInt32 startIndex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_rect_start (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir, UInt32 startIndex);
		}
		private static Delegates.sk_pathbuilder_add_rect_start sk_pathbuilder_add_rect_start_delegate;
		internal static void sk_pathbuilder_add_rect_start (sk_pathbuilder_t builder, SKRect* rect, SKPathDirection dir, UInt32 startIndex) =>
			(sk_pathbuilder_add_rect_start_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_rect_start> ("sk_pathbuilder_add_rect_start")).Invoke (builder, rect, dir, startIndex);
		#endif

		// void sk_pathbuilder_add_rounded_rect(sk_pathbuilder_t* builder, const sk_rect_t* rect, float rx, float ry, sk_path_direction_t dir)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_rounded_rect (sk_pathbuilder_t builder, SKRect* rect, Single rx, Single ry, SKPathDirection dir);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_rounded_rect (sk_pathbuilder_t builder, SKRect* rect, Single rx, Single ry, SKPathDirection dir);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_rounded_rect (sk_pathbuilder_t builder, SKRect* rect, Single rx, Single ry, SKPathDirection dir);
		}
		private static Delegates.sk_pathbuilder_add_rounded_rect sk_pathbuilder_add_rounded_rect_delegate;
		internal static void sk_pathbuilder_add_rounded_rect (sk_pathbuilder_t builder, SKRect* rect, Single rx, Single ry, SKPathDirection dir) =>
			(sk_pathbuilder_add_rounded_rect_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_rounded_rect> ("sk_pathbuilder_add_rounded_rect")).Invoke (builder, rect, rx, ry, dir);
		#endif

		// void sk_pathbuilder_add_rrect(sk_pathbuilder_t* builder, const sk_rrect_t* rect, sk_path_direction_t dir)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_rrect (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_rrect (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_rrect (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir);
		}
		private static Delegates.sk_pathbuilder_add_rrect sk_pathbuilder_add_rrect_delegate;
		internal static void sk_pathbuilder_add_rrect (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir) =>
			(sk_pathbuilder_add_rrect_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_rrect> ("sk_pathbuilder_add_rrect")).Invoke (builder, rect, dir);
		#endif

		// void sk_pathbuilder_add_rrect_start(sk_pathbuilder_t* builder, const sk_rrect_t* rect, sk_path_direction_t dir, uint32_t start)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_add_rrect_start (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir, UInt32 start);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_add_rrect_start (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir, UInt32 start);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_add_rrect_start (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir, UInt32 start);
		}
		private static Delegates.sk_pathbuilder_add_rrect_start sk_pathbuilder_add_rrect_start_delegate;
		internal static void sk_pathbuilder_add_rrect_start (sk_pathbuilder_t builder, sk_rrect_t rect, SKPathDirection dir, UInt32 start) =>
			(sk_pathbuilder_add_rrect_start_delegate ??= GetSymbol<Delegates.sk_pathbuilder_add_rrect_start> ("sk_pathbuilder_add_rrect_start")).Invoke (builder, rect, dir, start);
		#endif

		// void sk_pathbuilder_arc_to(sk_pathbuilder_t* builder, float rx, float ry, float xAxisRotate, sk_path_arc_size_t largeArc, sk_path_direction_t sweep, float x, float y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_arc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_arc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_arc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		}
		private static Delegates.sk_pathbuilder_arc_to sk_pathbuilder_arc_to_delegate;
		internal static void sk_pathbuilder_arc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y) =>
			(sk_pathbuilder_arc_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_arc_to> ("sk_pathbuilder_arc_to")).Invoke (builder, rx, ry, xAxisRotate, largeArc, sweep, x, y);
		#endif

		// void sk_pathbuilder_arc_to_with_oval(sk_pathbuilder_t* builder, const sk_rect_t* oval, float startAngle, float sweepAngle, bool forceMoveTo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_arc_to_with_oval (sk_pathbuilder_t builder, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_arc_to_with_oval (sk_pathbuilder_t builder, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_arc_to_with_oval (sk_pathbuilder_t builder, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo);
		}
		private static Delegates.sk_pathbuilder_arc_to_with_oval sk_pathbuilder_arc_to_with_oval_delegate;
		internal static void sk_pathbuilder_arc_to_with_oval (sk_pathbuilder_t builder, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool forceMoveTo) =>
			(sk_pathbuilder_arc_to_with_oval_delegate ??= GetSymbol<Delegates.sk_pathbuilder_arc_to_with_oval> ("sk_pathbuilder_arc_to_with_oval")).Invoke (builder, oval, startAngle, sweepAngle, forceMoveTo);
		#endif

		// void sk_pathbuilder_arc_to_with_points(sk_pathbuilder_t* builder, float x1, float y1, float x2, float y2, float radius)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_arc_to_with_points (sk_pathbuilder_t builder, Single x1, Single y1, Single x2, Single y2, Single radius);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_arc_to_with_points (sk_pathbuilder_t builder, Single x1, Single y1, Single x2, Single y2, Single radius);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_arc_to_with_points (sk_pathbuilder_t builder, Single x1, Single y1, Single x2, Single y2, Single radius);
		}
		private static Delegates.sk_pathbuilder_arc_to_with_points sk_pathbuilder_arc_to_with_points_delegate;
		internal static void sk_pathbuilder_arc_to_with_points (sk_pathbuilder_t builder, Single x1, Single y1, Single x2, Single y2, Single radius) =>
			(sk_pathbuilder_arc_to_with_points_delegate ??= GetSymbol<Delegates.sk_pathbuilder_arc_to_with_points> ("sk_pathbuilder_arc_to_with_points")).Invoke (builder, x1, y1, x2, y2, radius);
		#endif

		// void sk_pathbuilder_close(sk_pathbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_close (sk_pathbuilder_t builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_close (sk_pathbuilder_t builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_close (sk_pathbuilder_t builder);
		}
		private static Delegates.sk_pathbuilder_close sk_pathbuilder_close_delegate;
		internal static void sk_pathbuilder_close (sk_pathbuilder_t builder) =>
			(sk_pathbuilder_close_delegate ??= GetSymbol<Delegates.sk_pathbuilder_close> ("sk_pathbuilder_close")).Invoke (builder);
		#endif

		// void sk_pathbuilder_conic_to(sk_pathbuilder_t* builder, float x0, float y0, float x1, float y1, float w)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_conic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single w);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_conic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single w);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_conic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single w);
		}
		private static Delegates.sk_pathbuilder_conic_to sk_pathbuilder_conic_to_delegate;
		internal static void sk_pathbuilder_conic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single w) =>
			(sk_pathbuilder_conic_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_conic_to> ("sk_pathbuilder_conic_to")).Invoke (builder, x0, y0, x1, y1, w);
		#endif

		// void sk_pathbuilder_cubic_to(sk_pathbuilder_t* builder, float x0, float y0, float x1, float y1, float x2, float y2)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_cubic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_cubic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_cubic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2);
		}
		private static Delegates.sk_pathbuilder_cubic_to sk_pathbuilder_cubic_to_delegate;
		internal static void sk_pathbuilder_cubic_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1, Single x2, Single y2) =>
			(sk_pathbuilder_cubic_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_cubic_to> ("sk_pathbuilder_cubic_to")).Invoke (builder, x0, y0, x1, y1, x2, y2);
		#endif

		// void sk_pathbuilder_delete(sk_pathbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_delete (sk_pathbuilder_t builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_delete (sk_pathbuilder_t builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_delete (sk_pathbuilder_t builder);
		}
		private static Delegates.sk_pathbuilder_delete sk_pathbuilder_delete_delegate;
		internal static void sk_pathbuilder_delete (sk_pathbuilder_t builder) =>
			(sk_pathbuilder_delete_delegate ??= GetSymbol<Delegates.sk_pathbuilder_delete> ("sk_pathbuilder_delete")).Invoke (builder);
		#endif

		// sk_path_t* sk_pathbuilder_detach_path(sk_pathbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_t sk_pathbuilder_detach_path (sk_pathbuilder_t builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_pathbuilder_detach_path (sk_pathbuilder_t builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_t sk_pathbuilder_detach_path (sk_pathbuilder_t builder);
		}
		private static Delegates.sk_pathbuilder_detach_path sk_pathbuilder_detach_path_delegate;
		internal static sk_path_t sk_pathbuilder_detach_path (sk_pathbuilder_t builder) =>
			(sk_pathbuilder_detach_path_delegate ??= GetSymbol<Delegates.sk_pathbuilder_detach_path> ("sk_pathbuilder_detach_path")).Invoke (builder);
		#endif

		// sk_path_filltype_t sk_pathbuilder_get_filltype(const sk_pathbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPathFillType sk_pathbuilder_get_filltype (sk_pathbuilder_t builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPathFillType sk_pathbuilder_get_filltype (sk_pathbuilder_t builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPathFillType sk_pathbuilder_get_filltype (sk_pathbuilder_t builder);
		}
		private static Delegates.sk_pathbuilder_get_filltype sk_pathbuilder_get_filltype_delegate;
		internal static SKPathFillType sk_pathbuilder_get_filltype (sk_pathbuilder_t builder) =>
			(sk_pathbuilder_get_filltype_delegate ??= GetSymbol<Delegates.sk_pathbuilder_get_filltype> ("sk_pathbuilder_get_filltype")).Invoke (builder);
		#endif

		// void sk_pathbuilder_line_to(sk_pathbuilder_t* builder, float x, float y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_line_to (sk_pathbuilder_t builder, Single x, Single y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_line_to (sk_pathbuilder_t builder, Single x, Single y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_line_to (sk_pathbuilder_t builder, Single x, Single y);
		}
		private static Delegates.sk_pathbuilder_line_to sk_pathbuilder_line_to_delegate;
		internal static void sk_pathbuilder_line_to (sk_pathbuilder_t builder, Single x, Single y) =>
			(sk_pathbuilder_line_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_line_to> ("sk_pathbuilder_line_to")).Invoke (builder, x, y);
		#endif

		// void sk_pathbuilder_move_to(sk_pathbuilder_t* builder, float x, float y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_move_to (sk_pathbuilder_t builder, Single x, Single y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_move_to (sk_pathbuilder_t builder, Single x, Single y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_move_to (sk_pathbuilder_t builder, Single x, Single y);
		}
		private static Delegates.sk_pathbuilder_move_to sk_pathbuilder_move_to_delegate;
		internal static void sk_pathbuilder_move_to (sk_pathbuilder_t builder, Single x, Single y) =>
			(sk_pathbuilder_move_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_move_to> ("sk_pathbuilder_move_to")).Invoke (builder, x, y);
		#endif

		// sk_pathbuilder_t* sk_pathbuilder_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_pathbuilder_t sk_pathbuilder_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pathbuilder_t sk_pathbuilder_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_pathbuilder_t sk_pathbuilder_new ();
		}
		private static Delegates.sk_pathbuilder_new sk_pathbuilder_new_delegate;
		internal static sk_pathbuilder_t sk_pathbuilder_new () =>
			(sk_pathbuilder_new_delegate ??= GetSymbol<Delegates.sk_pathbuilder_new> ("sk_pathbuilder_new")).Invoke ();
		#endif

		// sk_pathbuilder_t* sk_pathbuilder_new_from_path(const sk_path_t* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_pathbuilder_t sk_pathbuilder_new_from_path (sk_path_t path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_pathbuilder_t sk_pathbuilder_new_from_path (sk_path_t path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_pathbuilder_t sk_pathbuilder_new_from_path (sk_path_t path);
		}
		private static Delegates.sk_pathbuilder_new_from_path sk_pathbuilder_new_from_path_delegate;
		internal static sk_pathbuilder_t sk_pathbuilder_new_from_path (sk_path_t path) =>
			(sk_pathbuilder_new_from_path_delegate ??= GetSymbol<Delegates.sk_pathbuilder_new_from_path> ("sk_pathbuilder_new_from_path")).Invoke (path);
		#endif

		// void sk_pathbuilder_quad_to(sk_pathbuilder_t* builder, float x0, float y0, float x1, float y1)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_quad_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_quad_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_quad_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1);
		}
		private static Delegates.sk_pathbuilder_quad_to sk_pathbuilder_quad_to_delegate;
		internal static void sk_pathbuilder_quad_to (sk_pathbuilder_t builder, Single x0, Single y0, Single x1, Single y1) =>
			(sk_pathbuilder_quad_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_quad_to> ("sk_pathbuilder_quad_to")).Invoke (builder, x0, y0, x1, y1);
		#endif

		// void sk_pathbuilder_rarc_to(sk_pathbuilder_t* builder, float rx, float ry, float xAxisRotate, sk_path_arc_size_t largeArc, sk_path_direction_t sweep, float x, float y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_rarc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_rarc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_rarc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y);
		}
		private static Delegates.sk_pathbuilder_rarc_to sk_pathbuilder_rarc_to_delegate;
		internal static void sk_pathbuilder_rarc_to (sk_pathbuilder_t builder, Single rx, Single ry, Single xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, Single x, Single y) =>
			(sk_pathbuilder_rarc_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_rarc_to> ("sk_pathbuilder_rarc_to")).Invoke (builder, rx, ry, xAxisRotate, largeArc, sweep, x, y);
		#endif

		// void sk_pathbuilder_rconic_to(sk_pathbuilder_t* builder, float dx0, float dy0, float dx1, float dy1, float w)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_rconic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single w);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_rconic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single w);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_rconic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single w);
		}
		private static Delegates.sk_pathbuilder_rconic_to sk_pathbuilder_rconic_to_delegate;
		internal static void sk_pathbuilder_rconic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single w) =>
			(sk_pathbuilder_rconic_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_rconic_to> ("sk_pathbuilder_rconic_to")).Invoke (builder, dx0, dy0, dx1, dy1, w);
		#endif

		// void sk_pathbuilder_rcubic_to(sk_pathbuilder_t* builder, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_rcubic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_rcubic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_rcubic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2);
		}
		private static Delegates.sk_pathbuilder_rcubic_to sk_pathbuilder_rcubic_to_delegate;
		internal static void sk_pathbuilder_rcubic_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1, Single dx2, Single dy2) =>
			(sk_pathbuilder_rcubic_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_rcubic_to> ("sk_pathbuilder_rcubic_to")).Invoke (builder, dx0, dy0, dx1, dy1, dx2, dy2);
		#endif

		// void sk_pathbuilder_reset(sk_pathbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_reset (sk_pathbuilder_t builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_reset (sk_pathbuilder_t builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_reset (sk_pathbuilder_t builder);
		}
		private static Delegates.sk_pathbuilder_reset sk_pathbuilder_reset_delegate;
		internal static void sk_pathbuilder_reset (sk_pathbuilder_t builder) =>
			(sk_pathbuilder_reset_delegate ??= GetSymbol<Delegates.sk_pathbuilder_reset> ("sk_pathbuilder_reset")).Invoke (builder);
		#endif

		// void sk_pathbuilder_reverse_add_path(sk_pathbuilder_t* builder, const sk_path_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_reverse_add_path (sk_pathbuilder_t builder, sk_path_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_reverse_add_path (sk_pathbuilder_t builder, sk_path_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_reverse_add_path (sk_pathbuilder_t builder, sk_path_t other);
		}
		private static Delegates.sk_pathbuilder_reverse_add_path sk_pathbuilder_reverse_add_path_delegate;
		internal static void sk_pathbuilder_reverse_add_path (sk_pathbuilder_t builder, sk_path_t other) =>
			(sk_pathbuilder_reverse_add_path_delegate ??= GetSymbol<Delegates.sk_pathbuilder_reverse_add_path> ("sk_pathbuilder_reverse_add_path")).Invoke (builder, other);
		#endif

		// void sk_pathbuilder_rline_to(sk_pathbuilder_t* builder, float dx, float dy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_rline_to (sk_pathbuilder_t builder, Single dx, Single dy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_rline_to (sk_pathbuilder_t builder, Single dx, Single dy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_rline_to (sk_pathbuilder_t builder, Single dx, Single dy);
		}
		private static Delegates.sk_pathbuilder_rline_to sk_pathbuilder_rline_to_delegate;
		internal static void sk_pathbuilder_rline_to (sk_pathbuilder_t builder, Single dx, Single dy) =>
			(sk_pathbuilder_rline_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_rline_to> ("sk_pathbuilder_rline_to")).Invoke (builder, dx, dy);
		#endif

		// void sk_pathbuilder_rmove_to(sk_pathbuilder_t* builder, float dx, float dy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_rmove_to (sk_pathbuilder_t builder, Single dx, Single dy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_rmove_to (sk_pathbuilder_t builder, Single dx, Single dy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_rmove_to (sk_pathbuilder_t builder, Single dx, Single dy);
		}
		private static Delegates.sk_pathbuilder_rmove_to sk_pathbuilder_rmove_to_delegate;
		internal static void sk_pathbuilder_rmove_to (sk_pathbuilder_t builder, Single dx, Single dy) =>
			(sk_pathbuilder_rmove_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_rmove_to> ("sk_pathbuilder_rmove_to")).Invoke (builder, dx, dy);
		#endif

		// void sk_pathbuilder_rquad_to(sk_pathbuilder_t* builder, float dx0, float dy0, float dx1, float dy1)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_rquad_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_rquad_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_rquad_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1);
		}
		private static Delegates.sk_pathbuilder_rquad_to sk_pathbuilder_rquad_to_delegate;
		internal static void sk_pathbuilder_rquad_to (sk_pathbuilder_t builder, Single dx0, Single dy0, Single dx1, Single dy1) =>
			(sk_pathbuilder_rquad_to_delegate ??= GetSymbol<Delegates.sk_pathbuilder_rquad_to> ("sk_pathbuilder_rquad_to")).Invoke (builder, dx0, dy0, dx1, dy1);
		#endif

		// void sk_pathbuilder_set_filltype(sk_pathbuilder_t* builder, sk_path_filltype_t filltype)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pathbuilder_set_filltype (sk_pathbuilder_t builder, SKPathFillType filltype);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pathbuilder_set_filltype (sk_pathbuilder_t builder, SKPathFillType filltype);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pathbuilder_set_filltype (sk_pathbuilder_t builder, SKPathFillType filltype);
		}
		private static Delegates.sk_pathbuilder_set_filltype sk_pathbuilder_set_filltype_delegate;
		internal static void sk_pathbuilder_set_filltype (sk_pathbuilder_t builder, SKPathFillType filltype) =>
			(sk_pathbuilder_set_filltype_delegate ??= GetSymbol<Delegates.sk_pathbuilder_set_filltype> ("sk_pathbuilder_set_filltype")).Invoke (builder, filltype);
		#endif

		// sk_path_t* sk_pathbuilder_snapshot_path(sk_pathbuilder_t* builder)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_t sk_pathbuilder_snapshot_path (sk_pathbuilder_t builder);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_t sk_pathbuilder_snapshot_path (sk_pathbuilder_t builder);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_t sk_pathbuilder_snapshot_path (sk_pathbuilder_t builder);
		}
		private static Delegates.sk_pathbuilder_snapshot_path sk_pathbuilder_snapshot_path_delegate;
		internal static sk_path_t sk_pathbuilder_snapshot_path (sk_pathbuilder_t builder) =>
			(sk_pathbuilder_snapshot_path_delegate ??= GetSymbol<Delegates.sk_pathbuilder_snapshot_path> ("sk_pathbuilder_snapshot_path")).Invoke (builder);
		#endif

		#endregion

	}
}
