using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_pixmap.h

		// void sk_color_get_bit_shift(int* a, int* r, int* g, int* b)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b);
		}
		private static Delegates.sk_color_get_bit_shift sk_color_get_bit_shift_delegate;
		internal static void sk_color_get_bit_shift (Int32* a, Int32* r, Int32* g, Int32* b) =>
			(sk_color_get_bit_shift_delegate ??= GetSymbol<Delegates.sk_color_get_bit_shift> ("sk_color_get_bit_shift")).Invoke (a, r, g, b);
		#endif

		// sk_pmcolor_t sk_color_premultiply(const sk_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_color_premultiply (UInt32 color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color_premultiply (UInt32 color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_color_premultiply (UInt32 color);
		}
		private static Delegates.sk_color_premultiply sk_color_premultiply_delegate;
		internal static UInt32 sk_color_premultiply (UInt32 color) =>
			(sk_color_premultiply_delegate ??= GetSymbol<Delegates.sk_color_premultiply> ("sk_color_premultiply")).Invoke (color);
		#endif

		// void sk_color_premultiply_array(const sk_color_t* colors, int size, sk_pmcolor_t* pmcolors)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors);
		}
		private static Delegates.sk_color_premultiply_array sk_color_premultiply_array_delegate;
		internal static void sk_color_premultiply_array (UInt32* colors, Int32 size, UInt32* pmcolors) =>
			(sk_color_premultiply_array_delegate ??= GetSymbol<Delegates.sk_color_premultiply_array> ("sk_color_premultiply_array")).Invoke (colors, size, pmcolors);
		#endif

		// sk_color_t sk_color_unpremultiply(const sk_pmcolor_t pmcolor)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_color_unpremultiply (UInt32 pmcolor);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_color_unpremultiply (UInt32 pmcolor);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_color_unpremultiply (UInt32 pmcolor);
		}
		private static Delegates.sk_color_unpremultiply sk_color_unpremultiply_delegate;
		internal static UInt32 sk_color_unpremultiply (UInt32 pmcolor) =>
			(sk_color_unpremultiply_delegate ??= GetSymbol<Delegates.sk_color_unpremultiply> ("sk_color_unpremultiply")).Invoke (pmcolor);
		#endif

		// void sk_color_unpremultiply_array(const sk_pmcolor_t* pmcolors, int size, sk_color_t* colors)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors);
		}
		private static Delegates.sk_color_unpremultiply_array sk_color_unpremultiply_array_delegate;
		internal static void sk_color_unpremultiply_array (UInt32* pmcolors, Int32 size, UInt32* colors) =>
			(sk_color_unpremultiply_array_delegate ??= GetSymbol<Delegates.sk_color_unpremultiply_array> ("sk_color_unpremultiply_array")).Invoke (pmcolors, size, colors);
		#endif

		// bool sk_jpegencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_jpegencoder_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_jpegencoder_encode (IntPtr dst, IntPtr src, SKJpegEncoderOptions* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_jpegencoder_encode (IntPtr dst, IntPtr src, SKJpegEncoderOptions* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_jpegencoder_encode (IntPtr dst, IntPtr src, SKJpegEncoderOptions* options);
		}
		private static Delegates.sk_jpegencoder_encode sk_jpegencoder_encode_delegate;
		internal static bool sk_jpegencoder_encode (IntPtr dst, IntPtr src, SKJpegEncoderOptions* options) =>
			(sk_jpegencoder_encode_delegate ??= GetSymbol<Delegates.sk_jpegencoder_encode> ("sk_jpegencoder_encode")).Invoke (dst, src, options);
		#endif

		// bool sk_pixmap_compute_is_opaque(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pixmap_compute_is_opaque (IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_compute_is_opaque (IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_compute_is_opaque (IntPtr cpixmap);
		}
		private static Delegates.sk_pixmap_compute_is_opaque sk_pixmap_compute_is_opaque_delegate;
		internal static bool sk_pixmap_compute_is_opaque (IntPtr cpixmap) =>
			(sk_pixmap_compute_is_opaque_delegate ??= GetSymbol<Delegates.sk_pixmap_compute_is_opaque> ("sk_pixmap_compute_is_opaque")).Invoke (cpixmap);
		#endif

		// void sk_pixmap_destructor(sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pixmap_destructor (IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_destructor (IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_destructor (IntPtr cpixmap);
		}
		private static Delegates.sk_pixmap_destructor sk_pixmap_destructor_delegate;
		internal static void sk_pixmap_destructor (IntPtr cpixmap) =>
			(sk_pixmap_destructor_delegate ??= GetSymbol<Delegates.sk_pixmap_destructor> ("sk_pixmap_destructor")).Invoke (cpixmap);
		#endif

		// bool sk_pixmap_erase_color(const sk_pixmap_t* cpixmap, sk_color_t color, const sk_irect_t* subset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pixmap_erase_color (IntPtr cpixmap, UInt32 color, SKRectI* subset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_erase_color (IntPtr cpixmap, UInt32 color, SKRectI* subset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_erase_color (IntPtr cpixmap, UInt32 color, SKRectI* subset);
		}
		private static Delegates.sk_pixmap_erase_color sk_pixmap_erase_color_delegate;
		internal static bool sk_pixmap_erase_color (IntPtr cpixmap, UInt32 color, SKRectI* subset) =>
			(sk_pixmap_erase_color_delegate ??= GetSymbol<Delegates.sk_pixmap_erase_color> ("sk_pixmap_erase_color")).Invoke (cpixmap, color, subset);
		#endif

		// bool sk_pixmap_erase_color4f(const sk_pixmap_t* cpixmap, const sk_color4f_t* color, const sk_irect_t* subset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pixmap_erase_color4f (IntPtr cpixmap, SKColorF* color, SKRectI* subset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_erase_color4f (IntPtr cpixmap, SKColorF* color, SKRectI* subset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_erase_color4f (IntPtr cpixmap, SKColorF* color, SKRectI* subset);
		}
		private static Delegates.sk_pixmap_erase_color4f sk_pixmap_erase_color4f_delegate;
		internal static bool sk_pixmap_erase_color4f (IntPtr cpixmap, SKColorF* color, SKRectI* subset) =>
			(sk_pixmap_erase_color4f_delegate ??= GetSymbol<Delegates.sk_pixmap_erase_color4f> ("sk_pixmap_erase_color4f")).Invoke (cpixmap, color, subset);
		#endif

		// bool sk_pixmap_extract_subset(const sk_pixmap_t* cpixmap, sk_pixmap_t* result, const sk_irect_t* subset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pixmap_extract_subset (IntPtr cpixmap, IntPtr result, SKRectI* subset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_extract_subset (IntPtr cpixmap, IntPtr result, SKRectI* subset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_extract_subset (IntPtr cpixmap, IntPtr result, SKRectI* subset);
		}
		private static Delegates.sk_pixmap_extract_subset sk_pixmap_extract_subset_delegate;
		internal static bool sk_pixmap_extract_subset (IntPtr cpixmap, IntPtr result, SKRectI* subset) =>
			(sk_pixmap_extract_subset_delegate ??= GetSymbol<Delegates.sk_pixmap_extract_subset> ("sk_pixmap_extract_subset")).Invoke (cpixmap, result, subset);
		#endif

		// sk_colorspace_t* sk_pixmap_get_colorspace(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_pixmap_get_colorspace (IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_pixmap_get_colorspace (IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_pixmap_get_colorspace (IntPtr cpixmap);
		}
		private static Delegates.sk_pixmap_get_colorspace sk_pixmap_get_colorspace_delegate;
		internal static IntPtr sk_pixmap_get_colorspace (IntPtr cpixmap) =>
			(sk_pixmap_get_colorspace_delegate ??= GetSymbol<Delegates.sk_pixmap_get_colorspace> ("sk_pixmap_get_colorspace")).Invoke (cpixmap);
		#endif

		// void sk_pixmap_get_info(const sk_pixmap_t* cpixmap, sk_imageinfo_t* cinfo)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pixmap_get_info (IntPtr cpixmap, SKImageInfoNative* cinfo);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_get_info (IntPtr cpixmap, SKImageInfoNative* cinfo);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_get_info (IntPtr cpixmap, SKImageInfoNative* cinfo);
		}
		private static Delegates.sk_pixmap_get_info sk_pixmap_get_info_delegate;
		internal static void sk_pixmap_get_info (IntPtr cpixmap, SKImageInfoNative* cinfo) =>
			(sk_pixmap_get_info_delegate ??= GetSymbol<Delegates.sk_pixmap_get_info> ("sk_pixmap_get_info")).Invoke (cpixmap, cinfo);
		#endif

		// float sk_pixmap_get_pixel_alphaf(const sk_pixmap_t* cpixmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Single sk_pixmap_get_pixel_alphaf (IntPtr cpixmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single sk_pixmap_get_pixel_alphaf (IntPtr cpixmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single sk_pixmap_get_pixel_alphaf (IntPtr cpixmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_pixmap_get_pixel_alphaf sk_pixmap_get_pixel_alphaf_delegate;
		internal static Single sk_pixmap_get_pixel_alphaf (IntPtr cpixmap, Int32 x, Int32 y) =>
			(sk_pixmap_get_pixel_alphaf_delegate ??= GetSymbol<Delegates.sk_pixmap_get_pixel_alphaf> ("sk_pixmap_get_pixel_alphaf")).Invoke (cpixmap, x, y);
		#endif

		// sk_color_t sk_pixmap_get_pixel_color(const sk_pixmap_t* cpixmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt32 sk_pixmap_get_pixel_color (IntPtr cpixmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 sk_pixmap_get_pixel_color (IntPtr cpixmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 sk_pixmap_get_pixel_color (IntPtr cpixmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_pixmap_get_pixel_color sk_pixmap_get_pixel_color_delegate;
		internal static UInt32 sk_pixmap_get_pixel_color (IntPtr cpixmap, Int32 x, Int32 y) =>
			(sk_pixmap_get_pixel_color_delegate ??= GetSymbol<Delegates.sk_pixmap_get_pixel_color> ("sk_pixmap_get_pixel_color")).Invoke (cpixmap, x, y);
		#endif

		// void sk_pixmap_get_pixel_color4f(const sk_pixmap_t* cpixmap, int x, int y, sk_color4f_t* color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pixmap_get_pixel_color4f (IntPtr cpixmap, Int32 x, Int32 y, SKColorF* color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_get_pixel_color4f (IntPtr cpixmap, Int32 x, Int32 y, SKColorF* color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_get_pixel_color4f (IntPtr cpixmap, Int32 x, Int32 y, SKColorF* color);
		}
		private static Delegates.sk_pixmap_get_pixel_color4f sk_pixmap_get_pixel_color4f_delegate;
		internal static void sk_pixmap_get_pixel_color4f (IntPtr cpixmap, Int32 x, Int32 y, SKColorF* color) =>
			(sk_pixmap_get_pixel_color4f_delegate ??= GetSymbol<Delegates.sk_pixmap_get_pixel_color4f> ("sk_pixmap_get_pixel_color4f")).Invoke (cpixmap, x, y, color);
		#endif

		// size_t sk_pixmap_get_row_bytes(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_pixmap_get_row_bytes (IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_pixmap_get_row_bytes (IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_pixmap_get_row_bytes (IntPtr cpixmap);
		}
		private static Delegates.sk_pixmap_get_row_bytes sk_pixmap_get_row_bytes_delegate;
		internal static /* size_t */ IntPtr sk_pixmap_get_row_bytes (IntPtr cpixmap) =>
			(sk_pixmap_get_row_bytes_delegate ??= GetSymbol<Delegates.sk_pixmap_get_row_bytes> ("sk_pixmap_get_row_bytes")).Invoke (cpixmap);
		#endif

		// void* sk_pixmap_get_writable_addr(const sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_pixmap_get_writable_addr (IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_writable_addr (IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_pixmap_get_writable_addr (IntPtr cpixmap);
		}
		private static Delegates.sk_pixmap_get_writable_addr sk_pixmap_get_writable_addr_delegate;
		internal static void* sk_pixmap_get_writable_addr (IntPtr cpixmap) =>
			(sk_pixmap_get_writable_addr_delegate ??= GetSymbol<Delegates.sk_pixmap_get_writable_addr> ("sk_pixmap_get_writable_addr")).Invoke (cpixmap);
		#endif

		// void* sk_pixmap_get_writeable_addr_with_xy(const sk_pixmap_t* cpixmap, int x, int y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_pixmap_get_writeable_addr_with_xy (IntPtr cpixmap, Int32 x, Int32 y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_pixmap_get_writeable_addr_with_xy (IntPtr cpixmap, Int32 x, Int32 y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_pixmap_get_writeable_addr_with_xy (IntPtr cpixmap, Int32 x, Int32 y);
		}
		private static Delegates.sk_pixmap_get_writeable_addr_with_xy sk_pixmap_get_writeable_addr_with_xy_delegate;
		internal static void* sk_pixmap_get_writeable_addr_with_xy (IntPtr cpixmap, Int32 x, Int32 y) =>
			(sk_pixmap_get_writeable_addr_with_xy_delegate ??= GetSymbol<Delegates.sk_pixmap_get_writeable_addr_with_xy> ("sk_pixmap_get_writeable_addr_with_xy")).Invoke (cpixmap, x, y);
		#endif

		// sk_pixmap_t* sk_pixmap_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_pixmap_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_pixmap_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_pixmap_new ();
		}
		private static Delegates.sk_pixmap_new sk_pixmap_new_delegate;
		internal static IntPtr sk_pixmap_new () =>
			(sk_pixmap_new_delegate ??= GetSymbol<Delegates.sk_pixmap_new> ("sk_pixmap_new")).Invoke ();
		#endif

		// sk_pixmap_t* sk_pixmap_new_with_params(const sk_imageinfo_t* cinfo, const void* addr, size_t rowBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_pixmap_new_with_params sk_pixmap_new_with_params_delegate;
		internal static IntPtr sk_pixmap_new_with_params (SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes) =>
			(sk_pixmap_new_with_params_delegate ??= GetSymbol<Delegates.sk_pixmap_new_with_params> ("sk_pixmap_new_with_params")).Invoke (cinfo, addr, rowBytes);
		#endif

		// bool sk_pixmap_read_pixels(const sk_pixmap_t* cpixmap, const sk_imageinfo_t* dstInfo, void* dstPixels, size_t dstRowBytes, int srcX, int srcY)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pixmap_read_pixels (IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_read_pixels (IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_read_pixels (IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY);
		}
		private static Delegates.sk_pixmap_read_pixels sk_pixmap_read_pixels_delegate;
		internal static bool sk_pixmap_read_pixels (IntPtr cpixmap, SKImageInfoNative* dstInfo, void* dstPixels, /* size_t */ IntPtr dstRowBytes, Int32 srcX, Int32 srcY) =>
			(sk_pixmap_read_pixels_delegate ??= GetSymbol<Delegates.sk_pixmap_read_pixels> ("sk_pixmap_read_pixels")).Invoke (cpixmap, dstInfo, dstPixels, dstRowBytes, srcX, srcY);
		#endif

		// void sk_pixmap_reset(sk_pixmap_t* cpixmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pixmap_reset (IntPtr cpixmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_reset (IntPtr cpixmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_reset (IntPtr cpixmap);
		}
		private static Delegates.sk_pixmap_reset sk_pixmap_reset_delegate;
		internal static void sk_pixmap_reset (IntPtr cpixmap) =>
			(sk_pixmap_reset_delegate ??= GetSymbol<Delegates.sk_pixmap_reset> ("sk_pixmap_reset")).Invoke (cpixmap);
		#endif

		// void sk_pixmap_reset_with_params(sk_pixmap_t* cpixmap, const sk_imageinfo_t* cinfo, const void* addr, size_t rowBytes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pixmap_reset_with_params (IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_reset_with_params (IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_reset_with_params (IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes);
		}
		private static Delegates.sk_pixmap_reset_with_params sk_pixmap_reset_with_params_delegate;
		internal static void sk_pixmap_reset_with_params (IntPtr cpixmap, SKImageInfoNative* cinfo, void* addr, /* size_t */ IntPtr rowBytes) =>
			(sk_pixmap_reset_with_params_delegate ??= GetSymbol<Delegates.sk_pixmap_reset_with_params> ("sk_pixmap_reset_with_params")).Invoke (cpixmap, cinfo, addr, rowBytes);
		#endif

		// bool sk_pixmap_scale_pixels(const sk_pixmap_t* cpixmap, const sk_pixmap_t* dst, const sk_sampling_options_t* sampling)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pixmap_scale_pixels (IntPtr cpixmap, IntPtr dst, SKSamplingOptions* sampling);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pixmap_scale_pixels (IntPtr cpixmap, IntPtr dst, SKSamplingOptions* sampling);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pixmap_scale_pixels (IntPtr cpixmap, IntPtr dst, SKSamplingOptions* sampling);
		}
		private static Delegates.sk_pixmap_scale_pixels sk_pixmap_scale_pixels_delegate;
		internal static bool sk_pixmap_scale_pixels (IntPtr cpixmap, IntPtr dst, SKSamplingOptions* sampling) =>
			(sk_pixmap_scale_pixels_delegate ??= GetSymbol<Delegates.sk_pixmap_scale_pixels> ("sk_pixmap_scale_pixels")).Invoke (cpixmap, dst, sampling);
		#endif

		// void sk_pixmap_set_colorspace(sk_pixmap_t* cpixmap, sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_pixmap_set_colorspace (IntPtr cpixmap, IntPtr colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_pixmap_set_colorspace (IntPtr cpixmap, IntPtr colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_pixmap_set_colorspace (IntPtr cpixmap, IntPtr colorspace);
		}
		private static Delegates.sk_pixmap_set_colorspace sk_pixmap_set_colorspace_delegate;
		internal static void sk_pixmap_set_colorspace (IntPtr cpixmap, IntPtr colorspace) =>
			(sk_pixmap_set_colorspace_delegate ??= GetSymbol<Delegates.sk_pixmap_set_colorspace> ("sk_pixmap_set_colorspace")).Invoke (cpixmap, colorspace);
		#endif

		// bool sk_pngencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_pngencoder_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_pngencoder_encode (IntPtr dst, IntPtr src, SKPngEncoderOptions* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_pngencoder_encode (IntPtr dst, IntPtr src, SKPngEncoderOptions* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_pngencoder_encode (IntPtr dst, IntPtr src, SKPngEncoderOptions* options);
		}
		private static Delegates.sk_pngencoder_encode sk_pngencoder_encode_delegate;
		internal static bool sk_pngencoder_encode (IntPtr dst, IntPtr src, SKPngEncoderOptions* options) =>
			(sk_pngencoder_encode_delegate ??= GetSymbol<Delegates.sk_pngencoder_encode> ("sk_pngencoder_encode")).Invoke (dst, src, options);
		#endif

		// void sk_swizzle_swap_rb(uint32_t* dest, const uint32_t* src, int count)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count);
		}
		private static Delegates.sk_swizzle_swap_rb sk_swizzle_swap_rb_delegate;
		internal static void sk_swizzle_swap_rb (UInt32* dest, UInt32* src, Int32 count) =>
			(sk_swizzle_swap_rb_delegate ??= GetSymbol<Delegates.sk_swizzle_swap_rb> ("sk_swizzle_swap_rb")).Invoke (dest, src, count);
		#endif

		// bool sk_webpencoder_encode(sk_wstream_t* dst, const sk_pixmap_t* src, const sk_webpencoder_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_webpencoder_encode (IntPtr dst, IntPtr src, SKWebpEncoderOptions* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_webpencoder_encode (IntPtr dst, IntPtr src, SKWebpEncoderOptions* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_webpencoder_encode (IntPtr dst, IntPtr src, SKWebpEncoderOptions* options);
		}
		private static Delegates.sk_webpencoder_encode sk_webpencoder_encode_delegate;
		internal static bool sk_webpencoder_encode (IntPtr dst, IntPtr src, SKWebpEncoderOptions* options) =>
			(sk_webpencoder_encode_delegate ??= GetSymbol<Delegates.sk_webpencoder_encode> ("sk_webpencoder_encode")).Invoke (dst, src, options);
		#endif

		// bool sk_webpencoder_encode_animated(sk_wstream_t* dst, const sk_webpencoder_frame_t* src, int count, const sk_webpencoder_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_webpencoder_encode_animated (IntPtr dst, SKWebpEncoderFrameNative* src, Int32 count, SKWebpEncoderOptions* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_webpencoder_encode_animated (IntPtr dst, SKWebpEncoderFrameNative* src, Int32 count, SKWebpEncoderOptions* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_webpencoder_encode_animated (IntPtr dst, SKWebpEncoderFrameNative* src, Int32 count, SKWebpEncoderOptions* options);
		}
		private static Delegates.sk_webpencoder_encode_animated sk_webpencoder_encode_animated_delegate;
		internal static bool sk_webpencoder_encode_animated (IntPtr dst, SKWebpEncoderFrameNative* src, Int32 count, SKWebpEncoderOptions* options) =>
			(sk_webpencoder_encode_animated_delegate ??= GetSymbol<Delegates.sk_webpencoder_encode_animated> ("sk_webpencoder_encode_animated")).Invoke (dst, src, count, options);
		#endif

		#endregion

	}
}
