using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class Xlib
	{
		// The versioned SONAME — see the note in Glx.cs. A runtime-only host ships
		// libX11.so.6 but not the unversioned libX11.so linker symlink.
		private const string libX11 = "libX11.so.6";

		public const int None = 0;
		public const int True = 1;
		public const int False = 0;

		static Xlib()
		{
			// Xlib is only thread-safe once XInitThreads has run, and it must be the
			// very first Xlib call in the process — which a static constructor
			// guarantees, since every entry point below lives on this type. The GL
			// tests are serialized by GpuRenderingCollection so they never share a
			// display concurrently, but xUnit still hands successive tests to
			// different pool threads and the driver keeps per-thread state, so make
			// the library thread-aware anyway. See #4590.
			XInitThreads();
		}

		[DllImport(libX11)]
		public extern static int XInitThreads();
		[DllImport(libX11)]
		public extern static IntPtr XOpenDisplay(string display_name);
		[DllImport(libX11)]
		public extern static int XCloseDisplay(IntPtr display);
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
