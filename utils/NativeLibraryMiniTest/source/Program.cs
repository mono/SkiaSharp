using System;
using System.Runtime.InteropServices;

using sk_bitmap_t = System.IntPtr;
using sk_colorspace_t = System.IntPtr;
using sk_pixmap_t = System.IntPtr;
using sk_wstream_t = System.IntPtr;
using sk_wstream_filestream_t = System.IntPtr;

namespace NativeLibraryMiniTest {
    unsafe class Program {
        const string SKIA = "libSkiaSharp";

        static int Main() {
            Console.WriteLine("Starting test...");
            Console.WriteLine($"OS = {RuntimeInformation.OSDescription}");
            Console.WriteLine($"OS Arch = {RuntimeInformation.OSArchitecture}");
            Console.WriteLine($"Proc Arch = {RuntimeInformation.ProcessArchitecture}");

            Console.WriteLine("Version test...");
            Console.WriteLine($"sk_version_get_milestone() = {sk_version_get_milestone()}");
            var str = Marshal.PtrToStringAnsi((IntPtr)sk_version_get_string());
            Console.WriteLine($"sk_version_get_string() = {str}");

            Console.WriteLine("Color type test...");
            Console.WriteLine($"sk_colortype_get_default_8888() = {sk_colortype_get_default_8888()}");

            Console.WriteLine("Bitmap create and save test...");
            var bmp = sk_bitmap_new();
            var info = new sk_imageinfo_t {
                width = 100,
                height = 100,
                colorType = sk_colortype_get_default_8888(),
                alphaType = sk_alphatype_t.Premul,
            };
            sk_bitmap_try_alloc_pixels_with_flags(bmp, &info, 0);
            sk_bitmap_erase(bmp, 0xFFFF0000);
            var pix = sk_pixmap_new();
            sk_bitmap_peek_pixels(bmp, pix);
            var stream = sk_filewstream_new("output.png");
            var opt = new sk_pngencoder_options_t {
                fFilterFlags = 248,
                fZLibLevel = 6,
            };
            sk_pngencoder_encode(stream, pix, &opt);
            sk_filewstream_destroy(stream);
            sk_pixmap_destructor(pix);
            sk_bitmap_destructor(bmp);

            Console.WriteLine("Test complete.");
            return 0;
        }

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern void* sk_version_get_string();

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern int sk_version_get_milestone();

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern sk_colortype_t sk_colortype_get_default_8888();

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern sk_bitmap_t sk_bitmap_new();

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool sk_bitmap_try_alloc_pixels_with_flags(sk_bitmap_t cbitmap, sk_imageinfo_t* requestedInfo, uint flags);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern void sk_bitmap_erase(sk_bitmap_t cbitmap, uint color);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool sk_bitmap_peek_pixels(sk_bitmap_t cbitmap, sk_pixmap_t cpixmap);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        static extern bool sk_pngencoder_encode(sk_wstream_t dst, sk_pixmap_t src, void* options);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern void sk_bitmap_destructor(sk_bitmap_t cbitmap);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern sk_pixmap_t sk_pixmap_new();

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern void sk_pixmap_destructor(sk_pixmap_t cpixmap);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern sk_wstream_filestream_t sk_filewstream_new([MarshalAs(UnmanagedType.LPStr)] string path);

        [DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
        static extern void sk_filewstream_destroy(sk_wstream_filestream_t cstream);

        [StructLayout(LayoutKind.Sequential)]
        unsafe partial struct sk_imageinfo_t {
            public sk_colorspace_t colorspace;
            public int width;
            public int height;
            public sk_colortype_t colorType;
            public sk_alphatype_t alphaType;
        }

        enum sk_colortype_t {
            Unknown = 0,
            Rgba8888 = 4,
            Bgra8888 = 6,
        }

        enum sk_alphatype_t {
            Unknown = 0,
            Opaque = 1,
            Premul = 2,
            Unpremul = 3,
        }

        [StructLayout (LayoutKind.Sequential)]
        struct sk_pngencoder_options_t {
            public int fFilterFlags;
            public int fZLibLevel;
            public void* fComments;
        }
    }
}
