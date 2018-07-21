using System;

namespace SkiaSharp
{
	public class SKOverdrawCanvas : SKNWayCanvas
	{
		[Preserve]
		internal SKOverdrawCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKOverdrawCanvas (SKCanvas canvas)
			: this (IntPtr.Zero, true)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			Handle = SkiaApi.sk_overdraw_canvas_new (canvas.Handle);
		}
	}
}
