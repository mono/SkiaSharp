using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_patheffect.h

		// sk_path_effect_t* sk_path_effect_create_1d_path(const sk_path_t* path, float advance, float phase, sk_path_effect_1d_style_t style)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style);
		}
		private static Delegates.sk_path_effect_create_1d_path sk_path_effect_create_1d_path_delegate;
		internal static sk_path_effect_t sk_path_effect_create_1d_path (sk_path_t path, Single advance, Single phase, SKPath1DPathEffectStyle style) =>
			(sk_path_effect_create_1d_path_delegate ??= GetSymbol<Delegates.sk_path_effect_create_1d_path> ("sk_path_effect_create_1d_path")).Invoke (path, advance, phase, style);
		#endif

		// sk_path_effect_t* sk_path_effect_create_2d_line(float width, const sk_matrix_t* matrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix);
		}
		private static Delegates.sk_path_effect_create_2d_line sk_path_effect_create_2d_line_delegate;
		internal static sk_path_effect_t sk_path_effect_create_2d_line (Single width, SKMatrix* matrix) =>
			(sk_path_effect_create_2d_line_delegate ??= GetSymbol<Delegates.sk_path_effect_create_2d_line> ("sk_path_effect_create_2d_line")).Invoke (width, matrix);
		#endif

		// sk_path_effect_t* sk_path_effect_create_2d_path(const sk_matrix_t* matrix, const sk_path_t* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path);
		}
		private static Delegates.sk_path_effect_create_2d_path sk_path_effect_create_2d_path_delegate;
		internal static sk_path_effect_t sk_path_effect_create_2d_path (SKMatrix* matrix, sk_path_t path) =>
			(sk_path_effect_create_2d_path_delegate ??= GetSymbol<Delegates.sk_path_effect_create_2d_path> ("sk_path_effect_create_2d_path")).Invoke (matrix, path);
		#endif

		// sk_path_effect_t* sk_path_effect_create_compose(sk_path_effect_t* outer, sk_path_effect_t* inner)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner);
		}
		private static Delegates.sk_path_effect_create_compose sk_path_effect_create_compose_delegate;
		internal static sk_path_effect_t sk_path_effect_create_compose (sk_path_effect_t outer, sk_path_effect_t inner) =>
			(sk_path_effect_create_compose_delegate ??= GetSymbol<Delegates.sk_path_effect_create_compose> ("sk_path_effect_create_compose")).Invoke (outer, inner);
		#endif

		// sk_path_effect_t* sk_path_effect_create_corner(float radius)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_corner (Single radius);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_corner (Single radius);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_corner (Single radius);
		}
		private static Delegates.sk_path_effect_create_corner sk_path_effect_create_corner_delegate;
		internal static sk_path_effect_t sk_path_effect_create_corner (Single radius) =>
			(sk_path_effect_create_corner_delegate ??= GetSymbol<Delegates.sk_path_effect_create_corner> ("sk_path_effect_create_corner")).Invoke (radius);
		#endif

		// sk_path_effect_t* sk_path_effect_create_dash(const float[-1] intervals, int count, float phase)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase);
		}
		private static Delegates.sk_path_effect_create_dash sk_path_effect_create_dash_delegate;
		internal static sk_path_effect_t sk_path_effect_create_dash (Single* intervals, Int32 count, Single phase) =>
			(sk_path_effect_create_dash_delegate ??= GetSymbol<Delegates.sk_path_effect_create_dash> ("sk_path_effect_create_dash")).Invoke (intervals, count, phase);
		#endif

		// sk_path_effect_t* sk_path_effect_create_discrete(float segLength, float deviation, uint32_t seedAssist)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist);
		}
		private static Delegates.sk_path_effect_create_discrete sk_path_effect_create_discrete_delegate;
		internal static sk_path_effect_t sk_path_effect_create_discrete (Single segLength, Single deviation, UInt32 seedAssist) =>
			(sk_path_effect_create_discrete_delegate ??= GetSymbol<Delegates.sk_path_effect_create_discrete> ("sk_path_effect_create_discrete")).Invoke (segLength, deviation, seedAssist);
		#endif

		// sk_path_effect_t* sk_path_effect_create_sum(sk_path_effect_t* first, sk_path_effect_t* second)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second);
		}
		private static Delegates.sk_path_effect_create_sum sk_path_effect_create_sum_delegate;
		internal static sk_path_effect_t sk_path_effect_create_sum (sk_path_effect_t first, sk_path_effect_t second) =>
			(sk_path_effect_create_sum_delegate ??= GetSymbol<Delegates.sk_path_effect_create_sum> ("sk_path_effect_create_sum")).Invoke (first, second);
		#endif

		// sk_path_effect_t* sk_path_effect_create_trim(float start, float stop, sk_path_effect_trim_mode_t mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode);
		}
		private static Delegates.sk_path_effect_create_trim sk_path_effect_create_trim_delegate;
		internal static sk_path_effect_t sk_path_effect_create_trim (Single start, Single stop, SKTrimPathEffectMode mode) =>
			(sk_path_effect_create_trim_delegate ??= GetSymbol<Delegates.sk_path_effect_create_trim> ("sk_path_effect_create_trim")).Invoke (start, stop, mode);
		#endif

		// void sk_path_effect_unref(sk_path_effect_t* t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_path_effect_unref (sk_path_effect_t t);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_path_effect_unref (sk_path_effect_t t);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_path_effect_unref (sk_path_effect_t t);
		}
		private static Delegates.sk_path_effect_unref sk_path_effect_unref_delegate;
		internal static void sk_path_effect_unref (sk_path_effect_t t) =>
			(sk_path_effect_unref_delegate ??= GetSymbol<Delegates.sk_path_effect_unref> ("sk_path_effect_unref")).Invoke (t);
		#endif

		#endregion

	}
}
