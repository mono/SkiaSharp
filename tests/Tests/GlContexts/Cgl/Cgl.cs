using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	internal class Cgl
	{
		private const string libGL = "/System/Library/Frameworks/OpenGL.framework/Versions/A/OpenGL";

		[DllImport(libGL)]
		public extern static void CGLGetVersion(out int majorvers, out int minorvers);
		[DllImport(libGL)]
		public extern static CGLError CGLChoosePixelFormat([In] CGLPixelFormatAttribute[] attribs, out IntPtr pix, out int npix);
		[DllImport(libGL)]
		public extern static CGLError CGLCreateContext(IntPtr pix, IntPtr share, out IntPtr ctx);
		[DllImport(libGL)]
		public extern static CGLError CGLReleasePixelFormat(IntPtr pix);
		[DllImport(libGL)]
		public extern static CGLError CGLSetCurrentContext(IntPtr ctx);
		[DllImport(libGL)]
		public extern static void CGLReleaseContext(IntPtr ctx);
		[DllImport(libGL)]
		public extern static CGLError CGLFlushDrawable(IntPtr ctx);
	}
}
