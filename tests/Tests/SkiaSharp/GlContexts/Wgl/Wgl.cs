using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class Wgl
	{
		private const string opengl32 = "opengl32.dll";

		public const int NONE = 0;
		public const int FALSE = 0;
		public const int TRUE = 1;

		public const int GL_VERSION = 0x1F02;
		public const int GL_EXTENSIONS = 0x1F03;
		public const int GL_TEXTURE_2D = 0x0DE1;
		public const int GL_UNSIGNED_BYTE = 0x1401;
		public const int GL_RGBA = 0x1908;
		public const int GL_RGBA8 = 0x8058;

		public const int WGL_NUMBER_PIXEL_FORMATS_ARB = 0x2000;
		public const int WGL_DRAW_TO_WINDOW_ARB = 0x2001;
		public const int WGL_DRAW_TO_BITMAP_ARB = 0x2002;
		public const int WGL_ACCELERATION_ARB = 0x2003;
		public const int WGL_NEED_PALETTE_ARB = 0x2004;
		public const int WGL_NEED_SYSTEM_PALETTE_ARB = 0x2005;
		public const int WGL_SWAP_LAYER_BUFFERS_ARB = 0x2006;
		public const int WGL_SWAP_METHOD_ARB = 0x2007;
		public const int WGL_NUMBER_OVERLAYS_ARB = 0x2008;
		public const int WGL_NUMBER_UNDERLAYS_ARB = 0x2009;
		public const int WGL_TRANSPARENT_ARB = 0x200A;
		public const int WGL_TRANSPARENT_RED_VALUE_ARB = 0x2037;
		public const int WGL_TRANSPARENT_GREEN_VALUE_ARB = 0x2038;
		public const int WGL_TRANSPARENT_BLUE_VALUE_ARB = 0x2039;
		public const int WGL_TRANSPARENT_ALPHA_VALUE_ARB = 0x203A;
		public const int WGL_TRANSPARENT_INDEX_VALUE_ARB = 0x203B;
		public const int WGL_SHARE_DEPTH_ARB = 0x200C;
		public const int WGL_SHARE_STENCIL_ARB = 0x200D;
		public const int WGL_SHARE_ACCUM_ARB = 0x200E;
		public const int WGL_SUPPORT_GDI_ARB = 0x200F;
		public const int WGL_SUPPORT_OPENGL_ARB = 0x2010;
		public const int WGL_DOUBLE_BUFFER_ARB = 0x2011;
		public const int WGL_STEREO_ARB = 0x2012;
		public const int WGL_PIXEL_TYPE_ARB = 0x2013;
		public const int WGL_COLOR_BITS_ARB = 0x2014;
		public const int WGL_RED_BITS_ARB = 0x2015;
		public const int WGL_RED_SHIFT_ARB = 0x2016;
		public const int WGL_GREEN_BITS_ARB = 0x2017;
		public const int WGL_GREEN_SHIFT_ARB = 0x2018;
		public const int WGL_BLUE_BITS_ARB = 0x2019;
		public const int WGL_BLUE_SHIFT_ARB = 0x201A;
		public const int WGL_ALPHA_BITS_ARB = 0x201B;
		public const int WGL_ALPHA_SHIFT_ARB = 0x201C;
		public const int WGL_ACCUM_BITS_ARB = 0x201D;
		public const int WGL_ACCUM_RED_BITS_ARB = 0x201E;
		public const int WGL_ACCUM_GREEN_BITS_ARB = 0x201F;
		public const int WGL_ACCUM_BLUE_BITS_ARB = 0x2020;
		public const int WGL_ACCUM_ALPHA_BITS_ARB = 0x2021;
		public const int WGL_DEPTH_BITS_ARB = 0x2022;
		public const int WGL_STENCIL_BITS_ARB = 0x2023;
		public const int WGL_AUX_BUFFERS_ARB = 0x2024;
		public const int WGL_NO_ACCELERATION_ARB = 0x2025;
		public const int WGL_GENERIC_ACCELERATION_ARB = 0x2026;
		public const int WGL_FULL_ACCELERATION_ARB = 0x2027;
		public const int WGL_SWAP_EXCHANGE_ARB = 0x2028;
		public const int WGL_SWAP_COPY_ARB = 0x2029;
		public const int WGL_SWAP_UNDEFINED_ARB = 0x202A;
		public const int WGL_TYPE_RGBA_ARB = 0x202B;
		public const int WGL_TYPE_COLORINDEX_ARB = 0x202C;

		static Wgl()
		{
			// save the current GL context
			var prevDC = Wgl.wglGetCurrentDC();
			var prevGLRC = Wgl.wglGetCurrentContext();

			// register the dummy window class
			var wc = new WNDCLASS
			{
				style = (User32.CS_HREDRAW | User32.CS_VREDRAW | User32.CS_OWNDC),
				lpfnWndProc = (WNDPROC)User32.DefWindowProc,
				cbClsExtra = 0,
				cbWndExtra = 0,
				hInstance = Kernel32.CurrentModuleHandle,
				hCursor = User32.LoadCursor(IntPtr.Zero, (int)User32.IDC_ARROW),
				hIcon = User32.LoadIcon(IntPtr.Zero, (IntPtr)User32.IDI_WINLOGO),
				hbrBackground = IntPtr.Zero,
				lpszMenuName = null,
				lpszClassName = "DummyClass"
			};
			if (User32.RegisterClass(ref wc) == 0)
			{
				throw new Exception("Could not register dummy class.");
			}

			// get the the dummy window bounds
			var windowRect = new RECT { left = 0, right = 8, top = 0, bottom = 8 };
			User32.AdjustWindowRectEx(ref windowRect, WindowStyles.WS_SYSMENU, false, User32.WS_EX_CLIENTEDGE);

			// create the dummy window
			var dummyWND = User32.CreateWindowEx(
				User32.WS_EX_CLIENTEDGE,
				"DummyClass",
				"DummyWindow",
				WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_SYSMENU,
				0, 0,
				windowRect.right - windowRect.left, windowRect.bottom - windowRect.top,
				IntPtr.Zero, IntPtr.Zero, Kernel32.CurrentModuleHandle, IntPtr.Zero);
			if (dummyWND == IntPtr.Zero)
			{
				User32.UnregisterClass("DummyClass", Kernel32.CurrentModuleHandle);
				throw new Exception("Could not create dummy window.");
			}

			// get the dummy DC
			var dummyDC = User32.GetDC(dummyWND);

			// get the dummy pixel format
			var dummyPFD = new PIXELFORMATDESCRIPTOR();
			dummyPFD.nSize = (ushort)Marshal.SizeOf(dummyPFD);
			dummyPFD.nVersion = 1;
			dummyPFD.dwFlags = Gdi32.PFD_DRAW_TO_WINDOW | Gdi32.PFD_SUPPORT_OPENGL;
			dummyPFD.iPixelType = Gdi32.PFD_TYPE_RGBA;
			dummyPFD.cColorBits = 32;
			dummyPFD.cDepthBits = 24;
			dummyPFD.cStencilBits = 8;
			dummyPFD.iLayerType = Gdi32.PFD_MAIN_PLANE;
			var dummyFormat = Gdi32.ChoosePixelFormat(dummyDC, ref dummyPFD);
			Gdi32.SetPixelFormat(dummyDC, dummyFormat, ref dummyPFD);

			// get the dummy GL context
			var dummyGLRC = Wgl.wglCreateContext(dummyDC);
			if (dummyGLRC == IntPtr.Zero)
			{
				throw new Exception("Could not create dummy GL context.");
			}
			Wgl.wglMakeCurrent(dummyDC, dummyGLRC);

			VersionString = Wgl.GetString(Wgl.GL_VERSION);

			// get the extension methods using the dummy context
			wglGetExtensionsStringARB = Wgl.wglGetProcAddress<wglGetExtensionsStringARBDelegate>("wglGetExtensionsStringARB");
			wglChoosePixelFormatARB = Wgl.wglGetProcAddress<wglChoosePixelFormatARBDelegate>("wglChoosePixelFormatARB");
			wglCreatePbufferARB = Wgl.wglGetProcAddress<wglCreatePbufferARBDelegate>("wglCreatePbufferARB");
			wglDestroyPbufferARB = Wgl.wglGetProcAddress<wglDestroyPbufferARBDelegate>("wglDestroyPbufferARB");
			wglGetPbufferDCARB = Wgl.wglGetProcAddress<wglGetPbufferDCARBDelegate>("wglGetPbufferDCARB");
			wglReleasePbufferDCARB = Wgl.wglGetProcAddress<wglReleasePbufferDCARBDelegate>("wglReleasePbufferDCARB");
			wglSwapIntervalEXT = Wgl.wglGetProcAddress<wglSwapIntervalEXTDelegate>("wglSwapIntervalEXT");

			// destroy the dummy GL context 
			Wgl.wglMakeCurrent(dummyDC, IntPtr.Zero);
			Wgl.wglDeleteContext(dummyGLRC);

			// destroy the dummy window
			User32.DestroyWindow(dummyWND);
			User32.UnregisterClass("DummyClass", Kernel32.CurrentModuleHandle);

			// reset the initial GL context
			Wgl.wglMakeCurrent(prevDC, prevGLRC);
		}

		public static string VersionString { get; }

		public static bool HasExtension(IntPtr dc, string ext)
		{
			if (wglGetExtensionsStringARB == null)
			{
				return false;
			}

			if (ext == "WGL_ARB_extensions_string")
			{
				return true;
			}

			return Array.IndexOf(GetExtensionsARB(dc), ext) != -1;
		}

		public static string GetExtensionsStringARB(IntPtr dc)
		{
			return Marshal.PtrToStringAnsi(wglGetExtensionsStringARB(dc));
		}

		public static string[] GetExtensionsARB(IntPtr dc)
		{
			var str = GetExtensionsStringARB(dc);
			if (string.IsNullOrEmpty(str))
			{
				return new string[0];
			}
			return str.Split(' ');
		}

		public static readonly wglGetExtensionsStringARBDelegate wglGetExtensionsStringARB;
		public static readonly wglChoosePixelFormatARBDelegate wglChoosePixelFormatARB;
		public static readonly wglCreatePbufferARBDelegate wglCreatePbufferARB;
		public static readonly wglDestroyPbufferARBDelegate wglDestroyPbufferARB;
		public static readonly wglGetPbufferDCARBDelegate wglGetPbufferDCARB;
		public static readonly wglReleasePbufferDCARBDelegate wglReleasePbufferDCARB;
		public static readonly wglSwapIntervalEXTDelegate wglSwapIntervalEXT;

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglGetCurrentDC();

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglGetCurrentContext();

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglCreateContext(IntPtr hDC);

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern bool wglMakeCurrent(IntPtr hDC, IntPtr hRC);

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern bool wglDeleteContext(IntPtr hRC);

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr wglGetProcAddress([MarshalAs(UnmanagedType.LPStr)] string lpszProc);

		public static T wglGetProcAddress<T>(string lpszProc)
		{
			var ptr = wglGetProcAddress(lpszProc);
			if (ptr == IntPtr.Zero)
			{
				return default(T);
			}
			return (T)(object)Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
		}

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern IntPtr glGetString(uint value);

		public static string GetString(uint value)
		{
			var intPtr = glGetString(value);
			return Marshal.PtrToStringAnsi(intPtr);
		}

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern void glGenTextures(int n, uint[] textures);

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern void glDeleteTextures(int n, uint[] textures);

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern void glBindTexture(uint target, uint texture);

		[DllImport(opengl32, CallingConvention = CallingConvention.Winapi)]
		public static extern void glTexImage2D(uint target, int level, int internalformat, int width, int height, int border, uint format, uint type, IntPtr pixels);
	}

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate IntPtr wglGetExtensionsStringARBDelegate(IntPtr dc);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public delegate bool wglChoosePixelFormatARBDelegate(
		IntPtr dc,
		[In] int[] attribIList,
		[In] float[] attribFList,
		uint maxFormats,
		[Out] int[] pixelFormats,
		out uint numFormats);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate IntPtr wglCreatePbufferARBDelegate(IntPtr dc, int pixelFormat, int width, int height, [In] int[] attribList);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public delegate bool wglDestroyPbufferARBDelegate(IntPtr pbuffer);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate IntPtr wglGetPbufferDCARBDelegate(IntPtr pbuffer);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	public delegate int wglReleasePbufferDCARBDelegate(IntPtr pbuffer, IntPtr dc);

	[UnmanagedFunctionPointer(CallingConvention.Winapi)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public delegate bool wglSwapIntervalEXTDelegate(int interval);
}
