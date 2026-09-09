using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_compatpaint.h

		// sk_compatpaint_t* sk_compatpaint_clone(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_compatpaint_clone (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_compatpaint_clone (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_compatpaint_clone (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_clone sk_compatpaint_clone_delegate;
		internal static IntPtr sk_compatpaint_clone (IntPtr paint) =>
			(sk_compatpaint_clone_delegate ??= GetSymbol<Delegates.sk_compatpaint_clone> ("sk_compatpaint_clone")).Invoke (paint);
		#endif

		// void sk_compatpaint_delete(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_delete (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_delete (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_delete (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_delete sk_compatpaint_delete_delegate;
		internal static void sk_compatpaint_delete (IntPtr paint) =>
			(sk_compatpaint_delete_delegate ??= GetSymbol<Delegates.sk_compatpaint_delete> ("sk_compatpaint_delete")).Invoke (paint);
		#endif

		// int sk_compatpaint_get_filter_quality(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_compatpaint_get_filter_quality (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_compatpaint_get_filter_quality (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_compatpaint_get_filter_quality (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_get_filter_quality sk_compatpaint_get_filter_quality_delegate;
		internal static Int32 sk_compatpaint_get_filter_quality (IntPtr paint) =>
			(sk_compatpaint_get_filter_quality_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_filter_quality> ("sk_compatpaint_get_filter_quality")).Invoke (paint);
		#endif

		// sk_font_t* sk_compatpaint_get_font(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_compatpaint_get_font (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_compatpaint_get_font (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_compatpaint_get_font (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_get_font sk_compatpaint_get_font_delegate;
		internal static IntPtr sk_compatpaint_get_font (IntPtr paint) =>
			(sk_compatpaint_get_font_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_font> ("sk_compatpaint_get_font")).Invoke (paint);
		#endif

		// bool sk_compatpaint_get_lcd_render_text(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_compatpaint_get_lcd_render_text (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_compatpaint_get_lcd_render_text (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_compatpaint_get_lcd_render_text (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_get_lcd_render_text sk_compatpaint_get_lcd_render_text_delegate;
		internal static bool sk_compatpaint_get_lcd_render_text (IntPtr paint) =>
			(sk_compatpaint_get_lcd_render_text_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_lcd_render_text> ("sk_compatpaint_get_lcd_render_text")).Invoke (paint);
		#endif

		// sk_text_align_t sk_compatpaint_get_text_align(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKTextAlign sk_compatpaint_get_text_align (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKTextAlign sk_compatpaint_get_text_align (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKTextAlign sk_compatpaint_get_text_align (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_get_text_align sk_compatpaint_get_text_align_delegate;
		internal static SKTextAlign sk_compatpaint_get_text_align (IntPtr paint) =>
			(sk_compatpaint_get_text_align_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_text_align> ("sk_compatpaint_get_text_align")).Invoke (paint);
		#endif

		// sk_text_encoding_t sk_compatpaint_get_text_encoding(const sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKTextEncoding sk_compatpaint_get_text_encoding (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKTextEncoding sk_compatpaint_get_text_encoding (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKTextEncoding sk_compatpaint_get_text_encoding (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_get_text_encoding sk_compatpaint_get_text_encoding_delegate;
		internal static SKTextEncoding sk_compatpaint_get_text_encoding (IntPtr paint) =>
			(sk_compatpaint_get_text_encoding_delegate ??= GetSymbol<Delegates.sk_compatpaint_get_text_encoding> ("sk_compatpaint_get_text_encoding")).Invoke (paint);
		#endif

		// sk_font_t* sk_compatpaint_make_font(sk_compatpaint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_compatpaint_make_font (IntPtr paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_compatpaint_make_font (IntPtr paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_compatpaint_make_font (IntPtr paint);
		}
		private static Delegates.sk_compatpaint_make_font sk_compatpaint_make_font_delegate;
		internal static IntPtr sk_compatpaint_make_font (IntPtr paint) =>
			(sk_compatpaint_make_font_delegate ??= GetSymbol<Delegates.sk_compatpaint_make_font> ("sk_compatpaint_make_font")).Invoke (paint);
		#endif

		// sk_compatpaint_t* sk_compatpaint_new_with_font(const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_compatpaint_new_with_font (IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_compatpaint_new_with_font (IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_compatpaint_new_with_font (IntPtr font);
		}
		private static Delegates.sk_compatpaint_new_with_font sk_compatpaint_new_with_font_delegate;
		internal static IntPtr sk_compatpaint_new_with_font (IntPtr font) =>
			(sk_compatpaint_new_with_font_delegate ??= GetSymbol<Delegates.sk_compatpaint_new_with_font> ("sk_compatpaint_new_with_font")).Invoke (font);
		#endif

		// void sk_compatpaint_reset(sk_compatpaint_t* paint, const sk_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_reset (IntPtr paint, IntPtr font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_reset (IntPtr paint, IntPtr font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_reset (IntPtr paint, IntPtr font);
		}
		private static Delegates.sk_compatpaint_reset sk_compatpaint_reset_delegate;
		internal static void sk_compatpaint_reset (IntPtr paint, IntPtr font) =>
			(sk_compatpaint_reset_delegate ??= GetSymbol<Delegates.sk_compatpaint_reset> ("sk_compatpaint_reset")).Invoke (paint, font);
		#endif

		// void sk_compatpaint_set_filter_quality(sk_compatpaint_t* paint, int quality)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_set_filter_quality (IntPtr paint, Int32 quality);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_filter_quality (IntPtr paint, Int32 quality);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_filter_quality (IntPtr paint, Int32 quality);
		}
		private static Delegates.sk_compatpaint_set_filter_quality sk_compatpaint_set_filter_quality_delegate;
		internal static void sk_compatpaint_set_filter_quality (IntPtr paint, Int32 quality) =>
			(sk_compatpaint_set_filter_quality_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_filter_quality> ("sk_compatpaint_set_filter_quality")).Invoke (paint, quality);
		#endif

		// void sk_compatpaint_set_is_antialias(sk_compatpaint_t* paint, bool antialias)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_set_is_antialias (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool antialias);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_is_antialias (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool antialias);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_is_antialias (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool antialias);
		}
		private static Delegates.sk_compatpaint_set_is_antialias sk_compatpaint_set_is_antialias_delegate;
		internal static void sk_compatpaint_set_is_antialias (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool antialias) =>
			(sk_compatpaint_set_is_antialias_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_is_antialias> ("sk_compatpaint_set_is_antialias")).Invoke (paint, antialias);
		#endif

		// void sk_compatpaint_set_lcd_render_text(sk_compatpaint_t* paint, bool lcdRenderText)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_set_lcd_render_text (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool lcdRenderText);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_lcd_render_text (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool lcdRenderText);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_lcd_render_text (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool lcdRenderText);
		}
		private static Delegates.sk_compatpaint_set_lcd_render_text sk_compatpaint_set_lcd_render_text_delegate;
		internal static void sk_compatpaint_set_lcd_render_text (IntPtr paint, [MarshalAs (UnmanagedType.I1)] bool lcdRenderText) =>
			(sk_compatpaint_set_lcd_render_text_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_lcd_render_text> ("sk_compatpaint_set_lcd_render_text")).Invoke (paint, lcdRenderText);
		#endif

		// void sk_compatpaint_set_text_align(sk_compatpaint_t* paint, sk_text_align_t align)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_set_text_align (IntPtr paint, SKTextAlign align);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_text_align (IntPtr paint, SKTextAlign align);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_text_align (IntPtr paint, SKTextAlign align);
		}
		private static Delegates.sk_compatpaint_set_text_align sk_compatpaint_set_text_align_delegate;
		internal static void sk_compatpaint_set_text_align (IntPtr paint, SKTextAlign align) =>
			(sk_compatpaint_set_text_align_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_text_align> ("sk_compatpaint_set_text_align")).Invoke (paint, align);
		#endif

		// void sk_compatpaint_set_text_encoding(sk_compatpaint_t* paint, sk_text_encoding_t encoding)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_compatpaint_set_text_encoding (IntPtr paint, SKTextEncoding encoding);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_compatpaint_set_text_encoding (IntPtr paint, SKTextEncoding encoding);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_compatpaint_set_text_encoding (IntPtr paint, SKTextEncoding encoding);
		}
		private static Delegates.sk_compatpaint_set_text_encoding sk_compatpaint_set_text_encoding_delegate;
		internal static void sk_compatpaint_set_text_encoding (IntPtr paint, SKTextEncoding encoding) =>
			(sk_compatpaint_set_text_encoding_delegate ??= GetSymbol<Delegates.sk_compatpaint_set_text_encoding> ("sk_compatpaint_set_text_encoding")).Invoke (paint, encoding);
		#endif

		#endregion

	}
}
