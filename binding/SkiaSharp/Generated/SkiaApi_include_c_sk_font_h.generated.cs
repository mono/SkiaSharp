using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_font.h

		// size_t sk_font_break_text(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, float maxWidth, float* measuredWidth, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_font_break_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_font_break_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_font_break_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, IntPtr paint);
		}
		private static Delegates.sk_font_break_text sk_font_break_text_delegate;
		internal static /* size_t */ IntPtr sk_font_break_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, Single maxWidth, Single* measuredWidth, IntPtr paint) =>
			(sk_font_break_text_delegate ??= GetSymbol<Delegates.sk_font_break_text> ("sk_font_break_text")).Invoke (font, text, byteLength, encoding, maxWidth, measuredWidth, paint);
		#endif

		// void sk_font_delete(sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_delete (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_delete (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_delete (IntPtr font);
		}
		private static Delegates.sk_font_delete sk_font_delete_delegate;
		internal static void sk_font_delete (IntPtr font) =>
			(sk_font_delete_delegate ??= GetSymbol<Delegates.sk_font_delete> ("sk_font_delete")).Invoke (font);
		#endif

		// sk_font_edging_t sk_font_get_edging(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKFontEdging sk_font_get_edging (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontEdging sk_font_get_edging (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontEdging sk_font_get_edging (IntPtr font);
		}
		private static Delegates.sk_font_get_edging sk_font_get_edging_delegate;
		internal static SKFontEdging sk_font_get_edging (IntPtr font) =>
			(sk_font_get_edging_delegate ??= GetSymbol<Delegates.sk_font_get_edging> ("sk_font_get_edging")).Invoke (font);
		#endif

		// sk_font_hinting_t sk_font_get_hinting(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKFontHinting sk_font_get_hinting (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontHinting sk_font_get_hinting (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontHinting sk_font_get_hinting (IntPtr font);
		}
		private static Delegates.sk_font_get_hinting sk_font_get_hinting_delegate;
		internal static SKFontHinting sk_font_get_hinting (IntPtr font) =>
			(sk_font_get_hinting_delegate ??= GetSymbol<Delegates.sk_font_get_hinting> ("sk_font_get_hinting")).Invoke (font);
		#endif

		// float sk_font_get_metrics(const sk_font_t* font, sk_fontmetrics_t* metrics)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_font_get_metrics (IntPtr font, SKFontMetrics* metrics);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_metrics (IntPtr font, SKFontMetrics* metrics);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_metrics (IntPtr font, SKFontMetrics* metrics);
		}
		private static Delegates.sk_font_get_metrics sk_font_get_metrics_delegate;
		internal static Single sk_font_get_metrics (IntPtr font, SKFontMetrics* metrics) =>
			(sk_font_get_metrics_delegate ??= GetSymbol<Delegates.sk_font_get_metrics> ("sk_font_get_metrics")).Invoke (font, metrics);
		#endif

		// bool sk_font_get_path(const sk_font_t* font, uint16_t glyph, sk_path_t* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_get_path (IntPtr font, UInt16 glyph, IntPtr path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_get_path (IntPtr font, UInt16 glyph, IntPtr path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_get_path (IntPtr font, UInt16 glyph, IntPtr path);
		}
		private static Delegates.sk_font_get_path sk_font_get_path_delegate;
		internal static bool sk_font_get_path (IntPtr font, UInt16 glyph, IntPtr path) =>
			(sk_font_get_path_delegate ??= GetSymbol<Delegates.sk_font_get_path> ("sk_font_get_path")).Invoke (font, glyph, path);
		#endif

		// void sk_font_get_paths(const sk_font_t* font, uint16_t[-1] glyphs, int count, const sk_glyph_path_proc glyphPathProc, void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_get_paths (IntPtr font, UInt16* glyphs, Int32 count, void* glyphPathProc, void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_paths (IntPtr font, UInt16* glyphs, Int32 count, SKGlyphPathProxyDelegate glyphPathProc, void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_paths (IntPtr font, UInt16* glyphs, Int32 count, SKGlyphPathProxyDelegate glyphPathProc, void* context);
		}
		private static Delegates.sk_font_get_paths sk_font_get_paths_delegate;
		internal static void sk_font_get_paths (IntPtr font, UInt16* glyphs, Int32 count, SKGlyphPathProxyDelegate glyphPathProc, void* context) =>
			(sk_font_get_paths_delegate ??= GetSymbol<Delegates.sk_font_get_paths> ("sk_font_get_paths")).Invoke (font, glyphs, count, glyphPathProc, context);
		#endif

		// void sk_font_get_pos(const sk_font_t* font, const uint16_t[-1] glyphs, int count, sk_point_t[-1] pos, sk_point_t* origin)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_get_pos (IntPtr font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_pos (IntPtr font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_pos (IntPtr font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin);
		}
		private static Delegates.sk_font_get_pos sk_font_get_pos_delegate;
		internal static void sk_font_get_pos (IntPtr font, UInt16* glyphs, Int32 count, SKPoint* pos, SKPoint* origin) =>
			(sk_font_get_pos_delegate ??= GetSymbol<Delegates.sk_font_get_pos> ("sk_font_get_pos")).Invoke (font, glyphs, count, pos, origin);
		#endif

		// float sk_font_get_scale_x(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_font_get_scale_x (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_scale_x (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_scale_x (IntPtr font);
		}
		private static Delegates.sk_font_get_scale_x sk_font_get_scale_x_delegate;
		internal static Single sk_font_get_scale_x (IntPtr font) =>
			(sk_font_get_scale_x_delegate ??= GetSymbol<Delegates.sk_font_get_scale_x> ("sk_font_get_scale_x")).Invoke (font);
		#endif

		// float sk_font_get_size(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_font_get_size (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_size (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_size (IntPtr font);
		}
		private static Delegates.sk_font_get_size sk_font_get_size_delegate;
		internal static Single sk_font_get_size (IntPtr font) =>
			(sk_font_get_size_delegate ??= GetSymbol<Delegates.sk_font_get_size> ("sk_font_get_size")).Invoke (font);
		#endif

		// float sk_font_get_skew_x(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_font_get_skew_x (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_get_skew_x (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_get_skew_x (IntPtr font);
		}
		private static Delegates.sk_font_get_skew_x sk_font_get_skew_x_delegate;
		internal static Single sk_font_get_skew_x (IntPtr font) =>
			(sk_font_get_skew_x_delegate ??= GetSymbol<Delegates.sk_font_get_skew_x> ("sk_font_get_skew_x")).Invoke (font);
		#endif

		// sk_typeface_t* sk_font_get_typeface(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_font_get_typeface (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_font_get_typeface (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_font_get_typeface (IntPtr font);
		}
		private static Delegates.sk_font_get_typeface sk_font_get_typeface_delegate;
		internal static IntPtr sk_font_get_typeface (IntPtr font) =>
			(sk_font_get_typeface_delegate ??= GetSymbol<Delegates.sk_font_get_typeface> ("sk_font_get_typeface")).Invoke (font);
		#endif

		// void sk_font_get_widths_bounds(const sk_font_t* font, const uint16_t[-1] glyphs, int count, float[-1] widths, sk_rect_t[-1] bounds, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_get_widths_bounds (IntPtr font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_widths_bounds (IntPtr font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_widths_bounds (IntPtr font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, IntPtr paint);
		}
		private static Delegates.sk_font_get_widths_bounds sk_font_get_widths_bounds_delegate;
		internal static void sk_font_get_widths_bounds (IntPtr font, UInt16* glyphs, Int32 count, Single* widths, SKRect* bounds, IntPtr paint) =>
			(sk_font_get_widths_bounds_delegate ??= GetSymbol<Delegates.sk_font_get_widths_bounds> ("sk_font_get_widths_bounds")).Invoke (font, glyphs, count, widths, bounds, paint);
		#endif

		// void sk_font_get_xpos(const sk_font_t* font, const uint16_t[-1] glyphs, int count, float[-1] xpos, float origin)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_get_xpos (IntPtr font, UInt16* glyphs, Int32 count, Single* xpos, Single origin);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_get_xpos (IntPtr font, UInt16* glyphs, Int32 count, Single* xpos, Single origin);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_get_xpos (IntPtr font, UInt16* glyphs, Int32 count, Single* xpos, Single origin);
		}
		private static Delegates.sk_font_get_xpos sk_font_get_xpos_delegate;
		internal static void sk_font_get_xpos (IntPtr font, UInt16* glyphs, Int32 count, Single* xpos, Single origin) =>
			(sk_font_get_xpos_delegate ??= GetSymbol<Delegates.sk_font_get_xpos> ("sk_font_get_xpos")).Invoke (font, glyphs, count, xpos, origin);
		#endif

		// bool sk_font_is_baseline_snap(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_is_baseline_snap (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_baseline_snap (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_baseline_snap (IntPtr font);
		}
		private static Delegates.sk_font_is_baseline_snap sk_font_is_baseline_snap_delegate;
		internal static bool sk_font_is_baseline_snap (IntPtr font) =>
			(sk_font_is_baseline_snap_delegate ??= GetSymbol<Delegates.sk_font_is_baseline_snap> ("sk_font_is_baseline_snap")).Invoke (font);
		#endif

		// bool sk_font_is_embedded_bitmaps(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_is_embedded_bitmaps (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_embedded_bitmaps (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_embedded_bitmaps (IntPtr font);
		}
		private static Delegates.sk_font_is_embedded_bitmaps sk_font_is_embedded_bitmaps_delegate;
		internal static bool sk_font_is_embedded_bitmaps (IntPtr font) =>
			(sk_font_is_embedded_bitmaps_delegate ??= GetSymbol<Delegates.sk_font_is_embedded_bitmaps> ("sk_font_is_embedded_bitmaps")).Invoke (font);
		#endif

		// bool sk_font_is_embolden(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_is_embolden (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_embolden (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_embolden (IntPtr font);
		}
		private static Delegates.sk_font_is_embolden sk_font_is_embolden_delegate;
		internal static bool sk_font_is_embolden (IntPtr font) =>
			(sk_font_is_embolden_delegate ??= GetSymbol<Delegates.sk_font_is_embolden> ("sk_font_is_embolden")).Invoke (font);
		#endif

		// bool sk_font_is_force_auto_hinting(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_is_force_auto_hinting (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_force_auto_hinting (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_force_auto_hinting (IntPtr font);
		}
		private static Delegates.sk_font_is_force_auto_hinting sk_font_is_force_auto_hinting_delegate;
		internal static bool sk_font_is_force_auto_hinting (IntPtr font) =>
			(sk_font_is_force_auto_hinting_delegate ??= GetSymbol<Delegates.sk_font_is_force_auto_hinting> ("sk_font_is_force_auto_hinting")).Invoke (font);
		#endif

		// bool sk_font_is_linear_metrics(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_is_linear_metrics (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_linear_metrics (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_linear_metrics (IntPtr font);
		}
		private static Delegates.sk_font_is_linear_metrics sk_font_is_linear_metrics_delegate;
		internal static bool sk_font_is_linear_metrics (IntPtr font) =>
			(sk_font_is_linear_metrics_delegate ??= GetSymbol<Delegates.sk_font_is_linear_metrics> ("sk_font_is_linear_metrics")).Invoke (font);
		#endif

		// bool sk_font_is_subpixel(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_font_is_subpixel (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_font_is_subpixel (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_font_is_subpixel (IntPtr font);
		}
		private static Delegates.sk_font_is_subpixel sk_font_is_subpixel_delegate;
		internal static bool sk_font_is_subpixel (IntPtr font) =>
			(sk_font_is_subpixel_delegate ??= GetSymbol<Delegates.sk_font_is_subpixel> ("sk_font_is_subpixel")).Invoke (font);
		#endif

		// float sk_font_measure_text(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, sk_rect_t* bounds, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_font_measure_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_font_measure_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_font_measure_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint);
		}
		private static Delegates.sk_font_measure_text sk_font_measure_text_delegate;
		internal static Single sk_font_measure_text (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint) =>
			(sk_font_measure_text_delegate ??= GetSymbol<Delegates.sk_font_measure_text> ("sk_font_measure_text")).Invoke (font, text, byteLength, encoding, bounds, paint);
		#endif

		// void sk_font_measure_text_no_return(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, sk_rect_t* bounds, const sk_paint_t* paint, float* measuredWidth)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_measure_text_no_return (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, Single* measuredWidth);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_measure_text_no_return (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, Single* measuredWidth);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_measure_text_no_return (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, Single* measuredWidth);
		}
		private static Delegates.sk_font_measure_text_no_return sk_font_measure_text_no_return_delegate;
		internal static void sk_font_measure_text_no_return (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, SKRect* bounds, IntPtr paint, Single* measuredWidth) =>
			(sk_font_measure_text_no_return_delegate ??= GetSymbol<Delegates.sk_font_measure_text_no_return> ("sk_font_measure_text_no_return")).Invoke (font, text, byteLength, encoding, bounds, paint, measuredWidth);
		#endif

		// sk_font_t* sk_font_new_with_values(sk_typeface_t* typeface, float size, float scaleX, float skewX)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_font_new_with_values (IntPtr typeface, Single size, Single scaleX, Single skewX);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_font_new_with_values (IntPtr typeface, Single size, Single scaleX, Single skewX);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_font_new_with_values (IntPtr typeface, Single size, Single scaleX, Single skewX);
		}
		private static Delegates.sk_font_new_with_values sk_font_new_with_values_delegate;
		internal static IntPtr sk_font_new_with_values (IntPtr typeface, Single size, Single scaleX, Single skewX) =>
			(sk_font_new_with_values_delegate ??= GetSymbol<Delegates.sk_font_new_with_values> ("sk_font_new_with_values")).Invoke (typeface, size, scaleX, skewX);
		#endif

		// void sk_font_set_baseline_snap(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_baseline_snap (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_baseline_snap (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_baseline_snap (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_baseline_snap sk_font_set_baseline_snap_delegate;
		internal static void sk_font_set_baseline_snap (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_baseline_snap_delegate ??= GetSymbol<Delegates.sk_font_set_baseline_snap> ("sk_font_set_baseline_snap")).Invoke (font, value);
		#endif

		// void sk_font_set_edging(sk_font_t* font, sk_font_edging_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_edging (IntPtr font, SKFontEdging value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_edging (IntPtr font, SKFontEdging value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_edging (IntPtr font, SKFontEdging value);
		}
		private static Delegates.sk_font_set_edging sk_font_set_edging_delegate;
		internal static void sk_font_set_edging (IntPtr font, SKFontEdging value) =>
			(sk_font_set_edging_delegate ??= GetSymbol<Delegates.sk_font_set_edging> ("sk_font_set_edging")).Invoke (font, value);
		#endif

		// void sk_font_set_embedded_bitmaps(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_embedded_bitmaps (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_embedded_bitmaps (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_embedded_bitmaps (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_embedded_bitmaps sk_font_set_embedded_bitmaps_delegate;
		internal static void sk_font_set_embedded_bitmaps (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_embedded_bitmaps_delegate ??= GetSymbol<Delegates.sk_font_set_embedded_bitmaps> ("sk_font_set_embedded_bitmaps")).Invoke (font, value);
		#endif

		// void sk_font_set_embolden(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_embolden (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_embolden (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_embolden (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_embolden sk_font_set_embolden_delegate;
		internal static void sk_font_set_embolden (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_embolden_delegate ??= GetSymbol<Delegates.sk_font_set_embolden> ("sk_font_set_embolden")).Invoke (font, value);
		#endif

		// void sk_font_set_force_auto_hinting(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_force_auto_hinting (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_force_auto_hinting (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_force_auto_hinting (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_force_auto_hinting sk_font_set_force_auto_hinting_delegate;
		internal static void sk_font_set_force_auto_hinting (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_force_auto_hinting_delegate ??= GetSymbol<Delegates.sk_font_set_force_auto_hinting> ("sk_font_set_force_auto_hinting")).Invoke (font, value);
		#endif

		// void sk_font_set_hinting(sk_font_t* font, sk_font_hinting_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_hinting (IntPtr font, SKFontHinting value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_hinting (IntPtr font, SKFontHinting value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_hinting (IntPtr font, SKFontHinting value);
		}
		private static Delegates.sk_font_set_hinting sk_font_set_hinting_delegate;
		internal static void sk_font_set_hinting (IntPtr font, SKFontHinting value) =>
			(sk_font_set_hinting_delegate ??= GetSymbol<Delegates.sk_font_set_hinting> ("sk_font_set_hinting")).Invoke (font, value);
		#endif

		// void sk_font_set_linear_metrics(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_linear_metrics (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_linear_metrics (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_linear_metrics (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_linear_metrics sk_font_set_linear_metrics_delegate;
		internal static void sk_font_set_linear_metrics (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_linear_metrics_delegate ??= GetSymbol<Delegates.sk_font_set_linear_metrics> ("sk_font_set_linear_metrics")).Invoke (font, value);
		#endif

		// void sk_font_set_scale_x(sk_font_t* font, float value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_scale_x (IntPtr font, Single value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_scale_x (IntPtr font, Single value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_scale_x (IntPtr font, Single value);
		}
		private static Delegates.sk_font_set_scale_x sk_font_set_scale_x_delegate;
		internal static void sk_font_set_scale_x (IntPtr font, Single value) =>
			(sk_font_set_scale_x_delegate ??= GetSymbol<Delegates.sk_font_set_scale_x> ("sk_font_set_scale_x")).Invoke (font, value);
		#endif

		// void sk_font_set_size(sk_font_t* font, float value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_size (IntPtr font, Single value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_size (IntPtr font, Single value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_size (IntPtr font, Single value);
		}
		private static Delegates.sk_font_set_size sk_font_set_size_delegate;
		internal static void sk_font_set_size (IntPtr font, Single value) =>
			(sk_font_set_size_delegate ??= GetSymbol<Delegates.sk_font_set_size> ("sk_font_set_size")).Invoke (font, value);
		#endif

		// void sk_font_set_skew_x(sk_font_t* font, float value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_skew_x (IntPtr font, Single value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_skew_x (IntPtr font, Single value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_skew_x (IntPtr font, Single value);
		}
		private static Delegates.sk_font_set_skew_x sk_font_set_skew_x_delegate;
		internal static void sk_font_set_skew_x (IntPtr font, Single value) =>
			(sk_font_set_skew_x_delegate ??= GetSymbol<Delegates.sk_font_set_skew_x> ("sk_font_set_skew_x")).Invoke (font, value);
		#endif

		// void sk_font_set_subpixel(sk_font_t* font, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_subpixel (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_subpixel (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_subpixel (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_font_set_subpixel sk_font_set_subpixel_delegate;
		internal static void sk_font_set_subpixel (IntPtr font, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_font_set_subpixel_delegate ??= GetSymbol<Delegates.sk_font_set_subpixel> ("sk_font_set_subpixel")).Invoke (font, value);
		#endif

		// void sk_font_set_typeface(sk_font_t* font, sk_typeface_t* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_set_typeface (IntPtr font, IntPtr value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_set_typeface (IntPtr font, IntPtr value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_set_typeface (IntPtr font, IntPtr value);
		}
		private static Delegates.sk_font_set_typeface sk_font_set_typeface_delegate;
		internal static void sk_font_set_typeface (IntPtr font, IntPtr value) =>
			(sk_font_set_typeface_delegate ??= GetSymbol<Delegates.sk_font_set_typeface> ("sk_font_set_typeface")).Invoke (font, value);
		#endif

		// int sk_font_text_to_glyphs(const sk_font_t* font, const void* text, size_t byteLength, sk_text_encoding_t encoding, uint16_t[-1] glyphs, int maxGlyphCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_font_text_to_glyphs (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_font_text_to_glyphs (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_font_text_to_glyphs (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount);
		}
		private static Delegates.sk_font_text_to_glyphs sk_font_text_to_glyphs_delegate;
		internal static Int32 sk_font_text_to_glyphs (IntPtr font, void* text, /* size_t */ IntPtr byteLength, SKTextEncoding encoding, UInt16* glyphs, Int32 maxGlyphCount) =>
			(sk_font_text_to_glyphs_delegate ??= GetSymbol<Delegates.sk_font_text_to_glyphs> ("sk_font_text_to_glyphs")).Invoke (font, text, byteLength, encoding, glyphs, maxGlyphCount);
		#endif

		// uint16_t sk_font_unichar_to_glyph(const sk_font_t* font, int32_t uni)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt16 sk_font_unichar_to_glyph (IntPtr font, Int32 uni);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16 sk_font_unichar_to_glyph (IntPtr font, Int32 uni);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16 sk_font_unichar_to_glyph (IntPtr font, Int32 uni);
		}
		private static Delegates.sk_font_unichar_to_glyph sk_font_unichar_to_glyph_delegate;
		internal static UInt16 sk_font_unichar_to_glyph (IntPtr font, Int32 uni) =>
			(sk_font_unichar_to_glyph_delegate ??= GetSymbol<Delegates.sk_font_unichar_to_glyph> ("sk_font_unichar_to_glyph")).Invoke (font, uni);
		#endif

		// void sk_font_unichars_to_glyphs(const sk_font_t* font, const int32_t[-1] uni, int count, uint16_t[-1] glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_font_unichars_to_glyphs (IntPtr font, Int32* uni, Int32 count, UInt16* glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_font_unichars_to_glyphs (IntPtr font, Int32* uni, Int32 count, UInt16* glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_font_unichars_to_glyphs (IntPtr font, Int32* uni, Int32 count, UInt16* glyphs);
		}
		private static Delegates.sk_font_unichars_to_glyphs sk_font_unichars_to_glyphs_delegate;
		internal static void sk_font_unichars_to_glyphs (IntPtr font, Int32* uni, Int32 count, UInt16* glyphs) =>
			(sk_font_unichars_to_glyphs_delegate ??= GetSymbol<Delegates.sk_font_unichars_to_glyphs> ("sk_font_unichars_to_glyphs")).Invoke (font, uni, count, glyphs);
		#endif

		// void sk_text_utils_get_path(const void* text, size_t length, sk_text_encoding_t encoding, float x, float y, const sk_font_t* font, sk_path_t* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, IntPtr font, IntPtr path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, IntPtr font, IntPtr path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, IntPtr font, IntPtr path);
		}
		private static Delegates.sk_text_utils_get_path sk_text_utils_get_path_delegate;
		internal static void sk_text_utils_get_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, Single x, Single y, IntPtr font, IntPtr path) =>
			(sk_text_utils_get_path_delegate ??= GetSymbol<Delegates.sk_text_utils_get_path> ("sk_text_utils_get_path")).Invoke (text, length, encoding, x, y, font, path);
		#endif

		// void sk_text_utils_get_pos_path(const void* text, size_t length, sk_text_encoding_t encoding, const sk_point_t[-1] pos, const sk_font_t* font, sk_path_t* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path);
		}
		private static Delegates.sk_text_utils_get_pos_path sk_text_utils_get_pos_path_delegate;
		internal static void sk_text_utils_get_pos_path (void* text, /* size_t */ IntPtr length, SKTextEncoding encoding, SKPoint* pos, IntPtr font, IntPtr path) =>
			(sk_text_utils_get_pos_path_delegate ??= GetSymbol<Delegates.sk_text_utils_get_pos_path> ("sk_text_utils_get_pos_path")).Invoke (text, length, encoding, pos, font, path);
		#endif

		#endregion

	}
}
