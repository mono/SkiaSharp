using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_matrix.h

		// void sk_matrix_concat(sk_matrix_t* result, sk_matrix_t* first, sk_matrix_t* second)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second);
		}
		private static Delegates.sk_matrix_concat sk_matrix_concat_delegate;
		internal static void sk_matrix_concat (SKMatrix* result, SKMatrix* first, SKMatrix* second) =>
			(sk_matrix_concat_delegate ??= GetSymbol<Delegates.sk_matrix_concat> ("sk_matrix_concat")).Invoke (result, first, second);
		#endif

		// void sk_matrix_map_points(sk_matrix_t* matrix, sk_point_t* dst, sk_point_t* src, int count)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		}
		private static Delegates.sk_matrix_map_points sk_matrix_map_points_delegate;
		internal static void sk_matrix_map_points (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count) =>
			(sk_matrix_map_points_delegate ??= GetSymbol<Delegates.sk_matrix_map_points> ("sk_matrix_map_points")).Invoke (matrix, dst, src, count);
		#endif

		// float sk_matrix_map_radius(sk_matrix_t* matrix, float radius)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_matrix_map_radius (SKMatrix* matrix, Single radius);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_matrix_map_radius (SKMatrix* matrix, Single radius);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_matrix_map_radius (SKMatrix* matrix, Single radius);
		}
		private static Delegates.sk_matrix_map_radius sk_matrix_map_radius_delegate;
		internal static Single sk_matrix_map_radius (SKMatrix* matrix, Single radius) =>
			(sk_matrix_map_radius_delegate ??= GetSymbol<Delegates.sk_matrix_map_radius> ("sk_matrix_map_radius")).Invoke (matrix, radius);
		#endif

		// void sk_matrix_map_rect(sk_matrix_t* matrix, sk_rect_t* dest, sk_rect_t* source)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source);
		}
		private static Delegates.sk_matrix_map_rect sk_matrix_map_rect_delegate;
		internal static void sk_matrix_map_rect (SKMatrix* matrix, SKRect* dest, SKRect* source) =>
			(sk_matrix_map_rect_delegate ??= GetSymbol<Delegates.sk_matrix_map_rect> ("sk_matrix_map_rect")).Invoke (matrix, dest, source);
		#endif

		// void sk_matrix_map_vector(sk_matrix_t* matrix, float x, float y, sk_point_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		}
		private static Delegates.sk_matrix_map_vector sk_matrix_map_vector_delegate;
		internal static void sk_matrix_map_vector (SKMatrix* matrix, Single x, Single y, SKPoint* result) =>
			(sk_matrix_map_vector_delegate ??= GetSymbol<Delegates.sk_matrix_map_vector> ("sk_matrix_map_vector")).Invoke (matrix, x, y, result);
		#endif

		// void sk_matrix_map_vectors(sk_matrix_t* matrix, sk_point_t* dst, sk_point_t* src, int count)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count);
		}
		private static Delegates.sk_matrix_map_vectors sk_matrix_map_vectors_delegate;
		internal static void sk_matrix_map_vectors (SKMatrix* matrix, SKPoint* dst, SKPoint* src, Int32 count) =>
			(sk_matrix_map_vectors_delegate ??= GetSymbol<Delegates.sk_matrix_map_vectors> ("sk_matrix_map_vectors")).Invoke (matrix, dst, src, count);
		#endif

		// void sk_matrix_map_xy(sk_matrix_t* matrix, float x, float y, sk_point_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result);
		}
		private static Delegates.sk_matrix_map_xy sk_matrix_map_xy_delegate;
		internal static void sk_matrix_map_xy (SKMatrix* matrix, Single x, Single y, SKPoint* result) =>
			(sk_matrix_map_xy_delegate ??= GetSymbol<Delegates.sk_matrix_map_xy> ("sk_matrix_map_xy")).Invoke (matrix, x, y, result);
		#endif

		// void sk_matrix_post_concat(sk_matrix_t* result, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix);
		}
		private static Delegates.sk_matrix_post_concat sk_matrix_post_concat_delegate;
		internal static void sk_matrix_post_concat (SKMatrix* result, SKMatrix* matrix) =>
			(sk_matrix_post_concat_delegate ??= GetSymbol<Delegates.sk_matrix_post_concat> ("sk_matrix_post_concat")).Invoke (result, matrix);
		#endif

		// void sk_matrix_pre_concat(sk_matrix_t* result, sk_matrix_t* matrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix);
		}
		private static Delegates.sk_matrix_pre_concat sk_matrix_pre_concat_delegate;
		internal static void sk_matrix_pre_concat (SKMatrix* result, SKMatrix* matrix) =>
			(sk_matrix_pre_concat_delegate ??= GetSymbol<Delegates.sk_matrix_pre_concat> ("sk_matrix_pre_concat")).Invoke (result, matrix);
		#endif

		// bool sk_matrix_try_invert(sk_matrix_t* matrix, sk_matrix_t* result)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result);
		}
		private static Delegates.sk_matrix_try_invert sk_matrix_try_invert_delegate;
		internal static bool sk_matrix_try_invert (SKMatrix* matrix, SKMatrix* result) =>
			(sk_matrix_try_invert_delegate ??= GetSymbol<Delegates.sk_matrix_try_invert> ("sk_matrix_try_invert")).Invoke (matrix, result);
		#endif

		#endregion

	}
}
