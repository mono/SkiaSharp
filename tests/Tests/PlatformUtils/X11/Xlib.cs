using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class Xlib
	{
		private const string libX11 = "libX11";

		public const int None = 0;
		public const int True = 1;
		public const int False = 0;

		[DllImport(libX11)]
		public extern static IntPtr XOpenDisplay(string display_name);
		[DllImport(libX11)]
		public extern static int XFree(IntPtr data);
		[DllImport(libX11)]
		public extern static int XDefaultScreen(IntPtr display);
		[DllImport(libX11)]
		public extern static IntPtr XRootWindow(IntPtr display, int screen);
		[DllImport(libX11)]
		public extern static IntPtr XCreatePixmap(IntPtr display, IntPtr d, uint width, uint height, uint depth);
		[DllImport(libX11)]
		public extern static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap);
	}
}
