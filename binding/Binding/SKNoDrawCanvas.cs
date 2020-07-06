using System;

namespace SkiaSharp
{
	public class SKNoDrawCanvas : SKCanvas
	{
		private protected SKNoDrawCanvas (IntPtr handle, bool owns = true, bool registerHandle = true)
			: base (handle, owns, registerHandle)
		{
		}

		public SKNoDrawCanvas (int width, int height)
			: base (SkiaApi.sk_nodraw_canvas_new (width, height))
		{
		}
	}
}
