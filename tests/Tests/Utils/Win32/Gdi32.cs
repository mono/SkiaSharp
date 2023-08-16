using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class Gdi32
	{
		private const string gdi32 = "gdi32.dll";

		public const byte PFD_TYPE_RGBA = 0;

		public const byte PFD_MAIN_PLANE = 0;

		public const uint PFD_DRAW_TO_WINDOW = 0x00000004;
		public const uint PFD_SUPPORT_OPENGL = 0x00000020;

		[DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetPixelFormat(IntPtr hdc, int iPixelFormat, [In] ref PIXELFORMATDESCRIPTOR ppfd);

		[DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		public static extern int ChoosePixelFormat(IntPtr hdc, [In] ref PIXELFORMATDESCRIPTOR ppfd);

		[DllImport(gdi32, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SwapBuffers(IntPtr hdc);
	}
}
