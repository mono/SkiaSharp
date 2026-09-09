using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_bitmap.h

		// void sk_bitmap_destructor(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_destructor (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_destructor (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_destructor (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_destructor sk_bitmap_destructor_delegate;
		internal static void sk_bitmap_destructor (IntPtr cbitmap) =>
			(sk_bitmap_destructor_delegate ??= GetSymbol<Delegates.sk_bitmap_destructor> ("sk_bitmap_destructor")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_erase(sk_bitmap_t* cbitmap, sk_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_erase (IntPtr cbitmap, UInt32 color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_erase (IntPtr cbitmap, UInt32 color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_erase (IntPtr cbitmap, UInt32 color);
		}
		private static Delegates.sk_bitmap_erase sk_bitmap_erase_delegate;
		internal static void sk_bitmap_erase (IntPtr cbitmap, UInt32 color) =>
			(sk_bitmap_erase_delegate ??= GetSymbol<Delegates.sk_bitmap_erase> ("sk_bitmap_erase")).Invoke (cbitmap, color);
		#endif

		// void sk_bitmap_erase_rect(sk_bitmap_t* cbitmap, sk_color_t color, sk_irect_t* rect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_erase_rect (IntPtr cbitmap, UInt32 color, SKRectI* rect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_erase_rect (IntPtr cbitmap, UInt32 color, SKRectI* rect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_erase_rect (IntPtr cbitmap, UInt32 color, SKRectI* rect);
		}
		private static Delegates.sk_bitmap_erase_rect sk_bitmap_erase_rect_delegate;
		internal static void sk_bitmap_erase_rect (IntPtr cbitmap, UInt32 color, SKRectI* rect) =>
			(sk_bitmap_erase_rect_delegate ??= GetSymbol<Delegates.sk_bitmap_erase_rect> ("sk_bitmap_erase_rect")).Invoke (cbitmap, color, rect);
		#endif

		// bool sk_bitmap_extract_alpha(sk_bitmap_t* cbitmap, sk_bitmap_t* dst, const sk_paint_t* paint, sk_ipoint_t* offset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_extract_alpha (IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_extract_alpha (IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_extract_alpha (IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset);
		}
		private static Delegates.sk_bitmap_extract_alpha sk_bitmap_extract_alpha_delegate;
		internal static bool sk_bitmap_extract_alpha (IntPtr cbitmap, IntPtr dst, IntPtr paint, SKPointI* offset) =>
			(sk_bitmap_extract_alpha_delegate ??= GetSymbol<Delegates.sk_bitmap_extract_alpha> ("sk_bitmap_extract_alpha")).Invoke (cbitmap, dst, paint, offset);
		#endif

		// bool sk_bitmap_extract_subset(sk_bitmap_t* cbitmap, sk_bitmap_t* dst, sk_irect_t* subset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_extract_subset (IntPtr cbitmap, IntPtr dst, SKRectI* subset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_extract_subset (IntPtr cbitmap, IntPtr dst, SKRectI* subset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_extract_subset (IntPtr cbitmap, IntPtr dst, SKRectI* subset);
		}
		private static Delegates.sk_bitmap_extract_subset sk_bitmap_extract_subset_delegate;
		internal static bool sk_bitmap_extract_subset (IntPtr cbitmap, IntPtr dst, SKRectI* subset) =>
			(sk_bitmap_extract_subset_delegate ??= GetSymbol<Delegates.sk_bitmap_extract_subset> ("sk_bitmap_extract_subset")).Invoke (cbitmap, dst, subset);
		#endif

		// void* sk_bitmap_get_addr(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_bitmap_get_addr (IntPtr cbitmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_bitmap_get_addr (IntPtr cbitmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_bitmap_get_addr (IntPtr cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr sk_bitmap_get_addr_delegate;
		internal static void* sk_bitmap_get_addr (IntPtr cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr> ("sk_bitmap_get_addr")).Invoke (cbitmap, x, y);
		#endif

		// uint16_t* sk_bitmap_get_addr_16(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt16* sk_bitmap_get_addr_16 (IntPtr cbitmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16* sk_bitmap_get_addr_16 (IntPtr cbitmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16* sk_bitmap_get_addr_16 (IntPtr cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr_16 sk_bitmap_get_addr_16_delegate;
		internal static UInt16* sk_bitmap_get_addr_16 (IntPtr cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_16_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr_16> ("sk_bitmap_get_addr_16")).Invoke (cbitmap, x, y);
		#endif

		// uint32_t* sk_bitmap_get_addr_32(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32* sk_bitmap_get_addr_32 (IntPtr cbitmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32* sk_bitmap_get_addr_32 (IntPtr cbitmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32* sk_bitmap_get_addr_32 (IntPtr cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr_32 sk_bitmap_get_addr_32_delegate;
		internal static UInt32* sk_bitmap_get_addr_32 (IntPtr cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_32_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr_32> ("sk_bitmap_get_addr_32")).Invoke (cbitmap, x, y);
		#endif

		// uint8_t* sk_bitmap_get_addr_8(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Byte* sk_bitmap_get_addr_8 (IntPtr cbitmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_bitmap_get_addr_8 (IntPtr cbitmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_bitmap_get_addr_8 (IntPtr cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_addr_8 sk_bitmap_get_addr_8_delegate;
		internal static Byte* sk_bitmap_get_addr_8 (IntPtr cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_addr_8_delegate ??= GetSymbol<Delegates.sk_bitmap_get_addr_8> ("sk_bitmap_get_addr_8")).Invoke (cbitmap, x, y);
		#endif

		// size_t sk_bitmap_get_byte_count(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_bitmap_get_byte_count (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_bitmap_get_byte_count (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_bitmap_get_byte_count (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_get_byte_count sk_bitmap_get_byte_count_delegate;
		internal static /* size_t */ IntPtr sk_bitmap_get_byte_count (IntPtr cbitmap) =>
			(sk_bitmap_get_byte_count_delegate ??= GetSymbol<Delegates.sk_bitmap_get_byte_count> ("sk_bitmap_get_byte_count")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_get_info(sk_bitmap_t* cbitmap, sk_imageinfo_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_get_info (IntPtr cbitmap, SKImageInfoNative* info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_get_info (IntPtr cbitmap, SKImageInfoNative* info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_get_info (IntPtr cbitmap, SKImageInfoNative* info);
		}
		private static Delegates.sk_bitmap_get_info sk_bitmap_get_info_delegate;
		internal static void sk_bitmap_get_info (IntPtr cbitmap, SKImageInfoNative* info) =>
			(sk_bitmap_get_info_delegate ??= GetSymbol<Delegates.sk_bitmap_get_info> ("sk_bitmap_get_info")).Invoke (cbitmap, info);
		#endif

		// sk_color_t sk_bitmap_get_pixel_color(sk_bitmap_t* cbitmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_bitmap_get_pixel_color (IntPtr cbitmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_bitmap_get_pixel_color (IntPtr cbitmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_bitmap_get_pixel_color (IntPtr cbitmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_bitmap_get_pixel_color sk_bitmap_get_pixel_color_delegate;
		internal static UInt32 sk_bitmap_get_pixel_color (IntPtr cbitmap, Int32 x, Int32 y) =>
			(sk_bitmap_get_pixel_color_delegate ??= GetSymbol<Delegates.sk_bitmap_get_pixel_color> ("sk_bitmap_get_pixel_color")).Invoke (cbitmap, x, y);
		#endif

		// void sk_bitmap_get_pixel_colors(sk_bitmap_t* cbitmap, sk_color_t* colors)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_get_pixel_colors (IntPtr cbitmap, UInt32* colors);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_get_pixel_colors (IntPtr cbitmap, UInt32* colors);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_get_pixel_colors (IntPtr cbitmap, UInt32* colors);
		}
		private static Delegates.sk_bitmap_get_pixel_colors sk_bitmap_get_pixel_colors_delegate;
		internal static void sk_bitmap_get_pixel_colors (IntPtr cbitmap, UInt32* colors) =>
			(sk_bitmap_get_pixel_colors_delegate ??= GetSymbol<Delegates.sk_bitmap_get_pixel_colors> ("sk_bitmap_get_pixel_colors")).Invoke (cbitmap, colors);
		#endif

		// void* sk_bitmap_get_pixels(sk_bitmap_t* cbitmap, size_t* length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_bitmap_get_pixels (IntPtr cbitmap, /* size_t */ IntPtr* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_bitmap_get_pixels (IntPtr cbitmap, /* size_t */ IntPtr* length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_bitmap_get_pixels (IntPtr cbitmap, /* size_t */ IntPtr* length);
		}
		private static Delegates.sk_bitmap_get_pixels sk_bitmap_get_pixels_delegate;
		internal static void* sk_bitmap_get_pixels (IntPtr cbitmap, /* size_t */ IntPtr* length) =>
			(sk_bitmap_get_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_get_pixels> ("sk_bitmap_get_pixels")).Invoke (cbitmap, length);
		#endif

		// size_t sk_bitmap_get_row_bytes(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_bitmap_get_row_bytes (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_bitmap_get_row_bytes (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_bitmap_get_row_bytes (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_get_row_bytes sk_bitmap_get_row_bytes_delegate;
		internal static /* size_t */ IntPtr sk_bitmap_get_row_bytes (IntPtr cbitmap) =>
			(sk_bitmap_get_row_bytes_delegate ??= GetSymbol<Delegates.sk_bitmap_get_row_bytes> ("sk_bitmap_get_row_bytes")).Invoke (cbitmap);
		#endif

		// bool sk_bitmap_install_pixels(sk_bitmap_t* cbitmap, const sk_imageinfo_t* cinfo, void* pixels, size_t rowBytes, const sk_bitmap_release_proc releaseProc, void* context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_install_pixels (IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, void* releaseProc, void* context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_pixels (IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_install_pixels (IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context);
		}
		private static Delegates.sk_bitmap_install_pixels sk_bitmap_install_pixels_delegate;
		internal static bool sk_bitmap_install_pixels (IntPtr cbitmap, SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, SKBitmapReleaseProxyDelegate releaseProc, void* context) =>
			(sk_bitmap_install_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_install_pixels> ("sk_bitmap_install_pixels")).Invoke (cbitmap, cinfo, pixels, rowBytes, releaseProc, context);
		#endif

		// bool sk_bitmap_install_pixels_with_pixmap(sk_bitmap_t* cbitmap, const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_install_pixels_with_pixmap (IntPtr cbitmap, IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_install_pixels_with_pixmap (IntPtr cbitmap, IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_install_pixels_with_pixmap (IntPtr cbitmap, IntPtr cpixmap);
		}
		private static Delegates.sk_bitmap_install_pixels_with_pixmap sk_bitmap_install_pixels_with_pixmap_delegate;
		internal static bool sk_bitmap_install_pixels_with_pixmap (IntPtr cbitmap, IntPtr cpixmap) =>
			(sk_bitmap_install_pixels_with_pixmap_delegate ??= GetSymbol<Delegates.sk_bitmap_install_pixels_with_pixmap> ("sk_bitmap_install_pixels_with_pixmap")).Invoke (cbitmap, cpixmap);
		#endif

		// bool sk_bitmap_is_immutable(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_is_immutable (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_immutable (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_is_immutable (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_is_immutable sk_bitmap_is_immutable_delegate;
		internal static bool sk_bitmap_is_immutable (IntPtr cbitmap) =>
			(sk_bitmap_is_immutable_delegate ??= GetSymbol<Delegates.sk_bitmap_is_immutable> ("sk_bitmap_is_immutable")).Invoke (cbitmap);
		#endif

		// bool sk_bitmap_is_null(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_is_null (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_is_null (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_is_null (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_is_null sk_bitmap_is_null_delegate;
		internal static bool sk_bitmap_is_null (IntPtr cbitmap) =>
			(sk_bitmap_is_null_delegate ??= GetSymbol<Delegates.sk_bitmap_is_null> ("sk_bitmap_is_null")).Invoke (cbitmap);
		#endif

		// sk_shader_t* sk_bitmap_make_shader(sk_bitmap_t* cbitmap, sk_shader_tilemode_t tmx, sk_shader_tilemode_t tmy, sk_sampling_options_t* sampling, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_bitmap_make_shader (IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_bitmap_make_shader (IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_bitmap_make_shader (IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions* sampling, SKMatrix* cmatrix);
		}
		private static Delegates.sk_bitmap_make_shader sk_bitmap_make_shader_delegate;
		internal static IntPtr sk_bitmap_make_shader (IntPtr cbitmap, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions* sampling, SKMatrix* cmatrix) =>
			(sk_bitmap_make_shader_delegate ??= GetSymbol<Delegates.sk_bitmap_make_shader> ("sk_bitmap_make_shader")).Invoke (cbitmap, tmx, tmy, sampling, cmatrix);
		#endif

		// sk_bitmap_t* sk_bitmap_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_bitmap_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_bitmap_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_bitmap_new ();
		}
		private static Delegates.sk_bitmap_new sk_bitmap_new_delegate;
		internal static IntPtr sk_bitmap_new () =>
			(sk_bitmap_new_delegate ??= GetSymbol<Delegates.sk_bitmap_new> ("sk_bitmap_new")).Invoke ();
		#endif

		// void sk_bitmap_notify_pixels_changed(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_notify_pixels_changed (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_notify_pixels_changed (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_notify_pixels_changed (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_notify_pixels_changed sk_bitmap_notify_pixels_changed_delegate;
		internal static void sk_bitmap_notify_pixels_changed (IntPtr cbitmap) =>
			(sk_bitmap_notify_pixels_changed_delegate ??= GetSymbol<Delegates.sk_bitmap_notify_pixels_changed> ("sk_bitmap_notify_pixels_changed")).Invoke (cbitmap);
		#endif

		// bool sk_bitmap_peek_pixels(sk_bitmap_t* cbitmap, sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_peek_pixels (IntPtr cbitmap, IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_peek_pixels (IntPtr cbitmap, IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_peek_pixels (IntPtr cbitmap, IntPtr cpixmap);
		}
		private static Delegates.sk_bitmap_peek_pixels sk_bitmap_peek_pixels_delegate;
		internal static bool sk_bitmap_peek_pixels (IntPtr cbitmap, IntPtr cpixmap) =>
			(sk_bitmap_peek_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_peek_pixels> ("sk_bitmap_peek_pixels")).Invoke (cbitmap, cpixmap);
		#endif

		// bool sk_bitmap_ready_to_draw(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_ready_to_draw (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_ready_to_draw (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_ready_to_draw (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_ready_to_draw sk_bitmap_ready_to_draw_delegate;
		internal static bool sk_bitmap_ready_to_draw (IntPtr cbitmap) =>
			(sk_bitmap_ready_to_draw_delegate ??= GetSymbol<Delegates.sk_bitmap_ready_to_draw> ("sk_bitmap_ready_to_draw")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_reset(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_reset (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_reset (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_reset (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_reset sk_bitmap_reset_delegate;
		internal static void sk_bitmap_reset (IntPtr cbitmap) =>
			(sk_bitmap_reset_delegate ??= GetSymbol<Delegates.sk_bitmap_reset> ("sk_bitmap_reset")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_set_immutable(sk_bitmap_t* cbitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_set_immutable (IntPtr cbitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_immutable (IntPtr cbitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_set_immutable (IntPtr cbitmap);
		}
		private static Delegates.sk_bitmap_set_immutable sk_bitmap_set_immutable_delegate;
		internal static void sk_bitmap_set_immutable (IntPtr cbitmap) =>
			(sk_bitmap_set_immutable_delegate ??= GetSymbol<Delegates.sk_bitmap_set_immutable> ("sk_bitmap_set_immutable")).Invoke (cbitmap);
		#endif

		// void sk_bitmap_set_pixels(sk_bitmap_t* cbitmap, void* pixels)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_set_pixels (IntPtr cbitmap, void* pixels);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_set_pixels (IntPtr cbitmap, void* pixels);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_set_pixels (IntPtr cbitmap, void* pixels);
		}
		private static Delegates.sk_bitmap_set_pixels sk_bitmap_set_pixels_delegate;
		internal static void sk_bitmap_set_pixels (IntPtr cbitmap, void* pixels) =>
			(sk_bitmap_set_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_set_pixels> ("sk_bitmap_set_pixels")).Invoke (cbitmap, pixels);
		#endif

		// void sk_bitmap_swap(sk_bitmap_t* cbitmap, sk_bitmap_t* cother)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_bitmap_swap (IntPtr cbitmap, IntPtr cother);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_bitmap_swap (IntPtr cbitmap, IntPtr cother);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_bitmap_swap (IntPtr cbitmap, IntPtr cother);
		}
		private static Delegates.sk_bitmap_swap sk_bitmap_swap_delegate;
		internal static void sk_bitmap_swap (IntPtr cbitmap, IntPtr cother) =>
			(sk_bitmap_swap_delegate ??= GetSymbol<Delegates.sk_bitmap_swap> ("sk_bitmap_swap")).Invoke (cbitmap, cother);
		#endif

		// bool sk_bitmap_try_alloc_pixels(sk_bitmap_t* cbitmap, const sk_imageinfo_t* requestedInfo, size_t rowBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_try_alloc_pixels (IntPtr cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_try_alloc_pixels (IntPtr cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_try_alloc_pixels (IntPtr cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_bitmap_try_alloc_pixels sk_bitmap_try_alloc_pixels_delegate;
		internal static bool sk_bitmap_try_alloc_pixels (IntPtr cbitmap, SKImageInfoNative* requestedInfo, /* size_t */ IntPtr rowBytes) =>
			(sk_bitmap_try_alloc_pixels_delegate ??= GetSymbol<Delegates.sk_bitmap_try_alloc_pixels> ("sk_bitmap_try_alloc_pixels")).Invoke (cbitmap, requestedInfo, rowBytes);
		#endif

		// bool sk_bitmap_try_alloc_pixels_with_flags(sk_bitmap_t* cbitmap, const sk_imageinfo_t* requestedInfo, uint32_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_bitmap_try_alloc_pixels_with_flags (IntPtr cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_bitmap_try_alloc_pixels_with_flags (IntPtr cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_bitmap_try_alloc_pixels_with_flags (IntPtr cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags);
		}
		private static Delegates.sk_bitmap_try_alloc_pixels_with_flags sk_bitmap_try_alloc_pixels_with_flags_delegate;
		internal static bool sk_bitmap_try_alloc_pixels_with_flags (IntPtr cbitmap, SKImageInfoNative* requestedInfo, UInt32 flags) =>
			(sk_bitmap_try_alloc_pixels_with_flags_delegate ??= GetSymbol<Delegates.sk_bitmap_try_alloc_pixels_with_flags> ("sk_bitmap_try_alloc_pixels_with_flags")).Invoke (cbitmap, requestedInfo, flags);
		#endif

		#endregion

	}
}
