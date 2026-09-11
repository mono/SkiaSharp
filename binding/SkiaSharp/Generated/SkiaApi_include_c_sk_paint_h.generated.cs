using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_paint.h

		// bool sk_paint_can_compute_fast_bounds(const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_paint_can_compute_fast_bounds (sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_can_compute_fast_bounds (sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_can_compute_fast_bounds (sk_paint_t cpaint);
		}
		private static Delegates.sk_paint_can_compute_fast_bounds sk_paint_can_compute_fast_bounds_delegate;
		internal static bool sk_paint_can_compute_fast_bounds (sk_paint_t cpaint) =>
			(sk_paint_can_compute_fast_bounds_delegate ??= GetSymbol<Delegates.sk_paint_can_compute_fast_bounds> ("sk_paint_can_compute_fast_bounds")).Invoke (cpaint);
		#endif

		// sk_paint_t* sk_paint_clone(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_paint_t sk_paint_clone (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_paint_t sk_paint_clone (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_paint_t sk_paint_clone (sk_paint_t param0);
		}
		private static Delegates.sk_paint_clone sk_paint_clone_delegate;
		internal static sk_paint_t sk_paint_clone (sk_paint_t param0) =>
			(sk_paint_clone_delegate ??= GetSymbol<Delegates.sk_paint_clone> ("sk_paint_clone")).Invoke (param0);
		#endif

		// void sk_paint_compute_fast_bounds(const sk_paint_t* cpaint, const sk_rect_t* orig, sk_rect_t* storage)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_compute_fast_bounds (sk_paint_t cpaint, SKRect* orig, SKRect* storage);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_compute_fast_bounds (sk_paint_t cpaint, SKRect* orig, SKRect* storage);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_compute_fast_bounds (sk_paint_t cpaint, SKRect* orig, SKRect* storage);
		}
		private static Delegates.sk_paint_compute_fast_bounds sk_paint_compute_fast_bounds_delegate;
		internal static void sk_paint_compute_fast_bounds (sk_paint_t cpaint, SKRect* orig, SKRect* storage) =>
			(sk_paint_compute_fast_bounds_delegate ??= GetSymbol<Delegates.sk_paint_compute_fast_bounds> ("sk_paint_compute_fast_bounds")).Invoke (cpaint, orig, storage);
		#endif

		// void sk_paint_delete(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_delete (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_delete (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_delete (sk_paint_t param0);
		}
		private static Delegates.sk_paint_delete sk_paint_delete_delegate;
		internal static void sk_paint_delete (sk_paint_t param0) =>
			(sk_paint_delete_delegate ??= GetSymbol<Delegates.sk_paint_delete> ("sk_paint_delete")).Invoke (param0);
		#endif

		// sk_blender_t* sk_paint_get_blender(sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_blender_t sk_paint_get_blender (sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_blender_t sk_paint_get_blender (sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_blender_t sk_paint_get_blender (sk_paint_t cpaint);
		}
		private static Delegates.sk_paint_get_blender sk_paint_get_blender_delegate;
		internal static sk_blender_t sk_paint_get_blender (sk_paint_t cpaint) =>
			(sk_paint_get_blender_delegate ??= GetSymbol<Delegates.sk_paint_get_blender> ("sk_paint_get_blender")).Invoke (cpaint);
		#endif

		// sk_blendmode_t sk_paint_get_blendmode(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKBlendMode sk_paint_get_blendmode (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKBlendMode sk_paint_get_blendmode (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKBlendMode sk_paint_get_blendmode (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_blendmode sk_paint_get_blendmode_delegate;
		internal static SKBlendMode sk_paint_get_blendmode (sk_paint_t param0) =>
			(sk_paint_get_blendmode_delegate ??= GetSymbol<Delegates.sk_paint_get_blendmode> ("sk_paint_get_blendmode")).Invoke (param0);
		#endif

		// sk_color_t sk_paint_get_color(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_paint_get_color (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_paint_get_color (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_paint_get_color (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_color sk_paint_get_color_delegate;
		internal static UInt32 sk_paint_get_color (sk_paint_t param0) =>
			(sk_paint_get_color_delegate ??= GetSymbol<Delegates.sk_paint_get_color> ("sk_paint_get_color")).Invoke (param0);
		#endif

		// void sk_paint_get_color4f(const sk_paint_t* paint, sk_color4f_t* color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color);
		}
		private static Delegates.sk_paint_get_color4f sk_paint_get_color4f_delegate;
		internal static void sk_paint_get_color4f (sk_paint_t paint, SKColorF* color) =>
			(sk_paint_get_color4f_delegate ??= GetSymbol<Delegates.sk_paint_get_color4f> ("sk_paint_get_color4f")).Invoke (paint, color);
		#endif

		// sk_colorfilter_t* sk_paint_get_colorfilter(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_colorfilter sk_paint_get_colorfilter_delegate;
		internal static sk_colorfilter_t sk_paint_get_colorfilter (sk_paint_t param0) =>
			(sk_paint_get_colorfilter_delegate ??= GetSymbol<Delegates.sk_paint_get_colorfilter> ("sk_paint_get_colorfilter")).Invoke (param0);
		#endif

		// bool sk_paint_get_fill_path(const sk_paint_t* cpaint, const sk_path_t* src, sk_pathbuilder_t* dst, const sk_rect_t* cullRect, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_paint_get_fill_path (sk_paint_t cpaint, sk_path_t src, sk_pathbuilder_t dst, SKRect* cullRect, SKMatrix* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_get_fill_path (sk_paint_t cpaint, sk_path_t src, sk_pathbuilder_t dst, SKRect* cullRect, SKMatrix* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_get_fill_path (sk_paint_t cpaint, sk_path_t src, sk_pathbuilder_t dst, SKRect* cullRect, SKMatrix* cmatrix);
		}
		private static Delegates.sk_paint_get_fill_path sk_paint_get_fill_path_delegate;
		internal static bool sk_paint_get_fill_path (sk_paint_t cpaint, sk_path_t src, sk_pathbuilder_t dst, SKRect* cullRect, SKMatrix* cmatrix) =>
			(sk_paint_get_fill_path_delegate ??= GetSymbol<Delegates.sk_paint_get_fill_path> ("sk_paint_get_fill_path")).Invoke (cpaint, src, dst, cullRect, cmatrix);
		#endif

		// sk_imagefilter_t* sk_paint_get_imagefilter(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_imagefilter sk_paint_get_imagefilter_delegate;
		internal static sk_imagefilter_t sk_paint_get_imagefilter (sk_paint_t param0) =>
			(sk_paint_get_imagefilter_delegate ??= GetSymbol<Delegates.sk_paint_get_imagefilter> ("sk_paint_get_imagefilter")).Invoke (param0);
		#endif

		// sk_maskfilter_t* sk_paint_get_maskfilter(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_maskfilter sk_paint_get_maskfilter_delegate;
		internal static sk_maskfilter_t sk_paint_get_maskfilter (sk_paint_t param0) =>
			(sk_paint_get_maskfilter_delegate ??= GetSymbol<Delegates.sk_paint_get_maskfilter> ("sk_paint_get_maskfilter")).Invoke (param0);
		#endif

		// sk_path_effect_t* sk_paint_get_path_effect(sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint);
		}
		private static Delegates.sk_paint_get_path_effect sk_paint_get_path_effect_delegate;
		internal static sk_path_effect_t sk_paint_get_path_effect (sk_paint_t cpaint) =>
			(sk_paint_get_path_effect_delegate ??= GetSymbol<Delegates.sk_paint_get_path_effect> ("sk_paint_get_path_effect")).Invoke (cpaint);
		#endif

		// sk_shader_t* sk_paint_get_shader(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_shader_t sk_paint_get_shader (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_shader_t sk_paint_get_shader (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_shader_t sk_paint_get_shader (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_shader sk_paint_get_shader_delegate;
		internal static sk_shader_t sk_paint_get_shader (sk_paint_t param0) =>
			(sk_paint_get_shader_delegate ??= GetSymbol<Delegates.sk_paint_get_shader> ("sk_paint_get_shader")).Invoke (param0);
		#endif

		// sk_stroke_cap_t sk_paint_get_stroke_cap(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_cap sk_paint_get_stroke_cap_delegate;
		internal static SKStrokeCap sk_paint_get_stroke_cap (sk_paint_t param0) =>
			(sk_paint_get_stroke_cap_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_cap> ("sk_paint_get_stroke_cap")).Invoke (param0);
		#endif

		// sk_stroke_join_t sk_paint_get_stroke_join(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_join sk_paint_get_stroke_join_delegate;
		internal static SKStrokeJoin sk_paint_get_stroke_join (sk_paint_t param0) =>
			(sk_paint_get_stroke_join_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_join> ("sk_paint_get_stroke_join")).Invoke (param0);
		#endif

		// float sk_paint_get_stroke_miter(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_paint_get_stroke_miter (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_stroke_miter (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_paint_get_stroke_miter (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_miter sk_paint_get_stroke_miter_delegate;
		internal static Single sk_paint_get_stroke_miter (sk_paint_t param0) =>
			(sk_paint_get_stroke_miter_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_miter> ("sk_paint_get_stroke_miter")).Invoke (param0);
		#endif

		// float sk_paint_get_stroke_width(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_paint_get_stroke_width (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_paint_get_stroke_width (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_paint_get_stroke_width (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_stroke_width sk_paint_get_stroke_width_delegate;
		internal static Single sk_paint_get_stroke_width (sk_paint_t param0) =>
			(sk_paint_get_stroke_width_delegate ??= GetSymbol<Delegates.sk_paint_get_stroke_width> ("sk_paint_get_stroke_width")).Invoke (param0);
		#endif

		// sk_paint_style_t sk_paint_get_style(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKPaintStyle sk_paint_get_style (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKPaintStyle sk_paint_get_style (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKPaintStyle sk_paint_get_style (sk_paint_t param0);
		}
		private static Delegates.sk_paint_get_style sk_paint_get_style_delegate;
		internal static SKPaintStyle sk_paint_get_style (sk_paint_t param0) =>
			(sk_paint_get_style_delegate ??= GetSymbol<Delegates.sk_paint_get_style> ("sk_paint_get_style")).Invoke (param0);
		#endif

		// bool sk_paint_is_antialias(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_paint_is_antialias (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_antialias (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_is_antialias (sk_paint_t param0);
		}
		private static Delegates.sk_paint_is_antialias sk_paint_is_antialias_delegate;
		internal static bool sk_paint_is_antialias (sk_paint_t param0) =>
			(sk_paint_is_antialias_delegate ??= GetSymbol<Delegates.sk_paint_is_antialias> ("sk_paint_is_antialias")).Invoke (param0);
		#endif

		// bool sk_paint_is_dither(const sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_paint_is_dither (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_paint_is_dither (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_paint_is_dither (sk_paint_t param0);
		}
		private static Delegates.sk_paint_is_dither sk_paint_is_dither_delegate;
		internal static bool sk_paint_is_dither (sk_paint_t param0) =>
			(sk_paint_is_dither_delegate ??= GetSymbol<Delegates.sk_paint_is_dither> ("sk_paint_is_dither")).Invoke (param0);
		#endif

		// sk_paint_t* sk_paint_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_paint_t sk_paint_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_paint_t sk_paint_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_paint_t sk_paint_new ();
		}
		private static Delegates.sk_paint_new sk_paint_new_delegate;
		internal static sk_paint_t sk_paint_new () =>
			(sk_paint_new_delegate ??= GetSymbol<Delegates.sk_paint_new> ("sk_paint_new")).Invoke ();
		#endif

		// void sk_paint_reset(sk_paint_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_reset (sk_paint_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_reset (sk_paint_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_reset (sk_paint_t param0);
		}
		private static Delegates.sk_paint_reset sk_paint_reset_delegate;
		internal static void sk_paint_reset (sk_paint_t param0) =>
			(sk_paint_reset_delegate ??= GetSymbol<Delegates.sk_paint_reset> ("sk_paint_reset")).Invoke (param0);
		#endif

		// void sk_paint_set_antialias(sk_paint_t*, bool)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		}
		private static Delegates.sk_paint_set_antialias sk_paint_set_antialias_delegate;
		internal static void sk_paint_set_antialias (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1) =>
			(sk_paint_set_antialias_delegate ??= GetSymbol<Delegates.sk_paint_set_antialias> ("sk_paint_set_antialias")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_blender(sk_paint_t* paint, sk_blender_t* blender)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_blender (sk_paint_t paint, sk_blender_t blender);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_blender (sk_paint_t paint, sk_blender_t blender);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_blender (sk_paint_t paint, sk_blender_t blender);
		}
		private static Delegates.sk_paint_set_blender sk_paint_set_blender_delegate;
		internal static void sk_paint_set_blender (sk_paint_t paint, sk_blender_t blender) =>
			(sk_paint_set_blender_delegate ??= GetSymbol<Delegates.sk_paint_set_blender> ("sk_paint_set_blender")).Invoke (paint, blender);
		#endif

		// void sk_paint_set_blendmode(sk_paint_t*, sk_blendmode_t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1);
		}
		private static Delegates.sk_paint_set_blendmode sk_paint_set_blendmode_delegate;
		internal static void sk_paint_set_blendmode (sk_paint_t param0, SKBlendMode param1) =>
			(sk_paint_set_blendmode_delegate ??= GetSymbol<Delegates.sk_paint_set_blendmode> ("sk_paint_set_blendmode")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_color(sk_paint_t*, sk_color_t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_color (sk_paint_t param0, UInt32 param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_color (sk_paint_t param0, UInt32 param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_color (sk_paint_t param0, UInt32 param1);
		}
		private static Delegates.sk_paint_set_color sk_paint_set_color_delegate;
		internal static void sk_paint_set_color (sk_paint_t param0, UInt32 param1) =>
			(sk_paint_set_color_delegate ??= GetSymbol<Delegates.sk_paint_set_color> ("sk_paint_set_color")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_color4f(sk_paint_t* paint, sk_color4f_t* color, sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace);
		}
		private static Delegates.sk_paint_set_color4f sk_paint_set_color4f_delegate;
		internal static void sk_paint_set_color4f (sk_paint_t paint, SKColorF* color, sk_colorspace_t colorspace) =>
			(sk_paint_set_color4f_delegate ??= GetSymbol<Delegates.sk_paint_set_color4f> ("sk_paint_set_color4f")).Invoke (paint, color, colorspace);
		#endif

		// void sk_paint_set_colorfilter(sk_paint_t*, sk_colorfilter_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1);
		}
		private static Delegates.sk_paint_set_colorfilter sk_paint_set_colorfilter_delegate;
		internal static void sk_paint_set_colorfilter (sk_paint_t param0, sk_colorfilter_t param1) =>
			(sk_paint_set_colorfilter_delegate ??= GetSymbol<Delegates.sk_paint_set_colorfilter> ("sk_paint_set_colorfilter")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_dither(sk_paint_t*, bool)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1);
		}
		private static Delegates.sk_paint_set_dither sk_paint_set_dither_delegate;
		internal static void sk_paint_set_dither (sk_paint_t param0, [MarshalAs (UnmanagedType.I1)] bool param1) =>
			(sk_paint_set_dither_delegate ??= GetSymbol<Delegates.sk_paint_set_dither> ("sk_paint_set_dither")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_imagefilter(sk_paint_t*, sk_imagefilter_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1);
		}
		private static Delegates.sk_paint_set_imagefilter sk_paint_set_imagefilter_delegate;
		internal static void sk_paint_set_imagefilter (sk_paint_t param0, sk_imagefilter_t param1) =>
			(sk_paint_set_imagefilter_delegate ??= GetSymbol<Delegates.sk_paint_set_imagefilter> ("sk_paint_set_imagefilter")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_maskfilter(sk_paint_t*, sk_maskfilter_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1);
		}
		private static Delegates.sk_paint_set_maskfilter sk_paint_set_maskfilter_delegate;
		internal static void sk_paint_set_maskfilter (sk_paint_t param0, sk_maskfilter_t param1) =>
			(sk_paint_set_maskfilter_delegate ??= GetSymbol<Delegates.sk_paint_set_maskfilter> ("sk_paint_set_maskfilter")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_path_effect(sk_paint_t* cpaint, sk_path_effect_t* effect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect);
		}
		private static Delegates.sk_paint_set_path_effect sk_paint_set_path_effect_delegate;
		internal static void sk_paint_set_path_effect (sk_paint_t cpaint, sk_path_effect_t effect) =>
			(sk_paint_set_path_effect_delegate ??= GetSymbol<Delegates.sk_paint_set_path_effect> ("sk_paint_set_path_effect")).Invoke (cpaint, effect);
		#endif

		// void sk_paint_set_shader(sk_paint_t*, sk_shader_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1);
		}
		private static Delegates.sk_paint_set_shader sk_paint_set_shader_delegate;
		internal static void sk_paint_set_shader (sk_paint_t param0, sk_shader_t param1) =>
			(sk_paint_set_shader_delegate ??= GetSymbol<Delegates.sk_paint_set_shader> ("sk_paint_set_shader")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_stroke_cap(sk_paint_t*, sk_stroke_cap_t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1);
		}
		private static Delegates.sk_paint_set_stroke_cap sk_paint_set_stroke_cap_delegate;
		internal static void sk_paint_set_stroke_cap (sk_paint_t param0, SKStrokeCap param1) =>
			(sk_paint_set_stroke_cap_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_cap> ("sk_paint_set_stroke_cap")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_stroke_join(sk_paint_t*, sk_stroke_join_t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1);
		}
		private static Delegates.sk_paint_set_stroke_join sk_paint_set_stroke_join_delegate;
		internal static void sk_paint_set_stroke_join (sk_paint_t param0, SKStrokeJoin param1) =>
			(sk_paint_set_stroke_join_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_join> ("sk_paint_set_stroke_join")).Invoke (param0, param1);
		#endif

		// void sk_paint_set_stroke_miter(sk_paint_t*, float miter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter);
		}
		private static Delegates.sk_paint_set_stroke_miter sk_paint_set_stroke_miter_delegate;
		internal static void sk_paint_set_stroke_miter (sk_paint_t param0, Single miter) =>
			(sk_paint_set_stroke_miter_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_miter> ("sk_paint_set_stroke_miter")).Invoke (param0, miter);
		#endif

		// void sk_paint_set_stroke_width(sk_paint_t*, float width)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_stroke_width (sk_paint_t param0, Single width);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_stroke_width (sk_paint_t param0, Single width);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_stroke_width (sk_paint_t param0, Single width);
		}
		private static Delegates.sk_paint_set_stroke_width sk_paint_set_stroke_width_delegate;
		internal static void sk_paint_set_stroke_width (sk_paint_t param0, Single width) =>
			(sk_paint_set_stroke_width_delegate ??= GetSymbol<Delegates.sk_paint_set_stroke_width> ("sk_paint_set_stroke_width")).Invoke (param0, width);
		#endif

		// void sk_paint_set_style(sk_paint_t*, sk_paint_style_t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1);
		}
		private static Delegates.sk_paint_set_style sk_paint_set_style_delegate;
		internal static void sk_paint_set_style (sk_paint_t param0, SKPaintStyle param1) =>
			(sk_paint_set_style_delegate ??= GetSymbol<Delegates.sk_paint_set_style> ("sk_paint_set_style")).Invoke (param0, param1);
		#endif

		#endregion

	}
}
