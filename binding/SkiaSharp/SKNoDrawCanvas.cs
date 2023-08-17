#nullable disable

using System;

namespace SkiaSharp
{
	public class SKNoDrawCanvas : SKCanvas
	{
		internal SKNoDrawCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKNoDrawCanvas (int width, int height)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sk_nodraw_canvas_new (width, height);
		}
	}
}
