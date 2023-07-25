using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct XVisualInfo
	{
		public IntPtr visual;
		public IntPtr visualid;
		public int screen;
		public int depth;
		public XVisualClass c_class;
		public ulong red_mask;
		public ulong green_mask;
		public ulong blue_mask;
		public int colormap_size;
		public int bits_per_rgb;
	}
}
