using System;

namespace SkiaSharp
{
	public class SKOverdrawCanvas : SKNWayCanvas
	{
		public SKOverdrawCanvas (SKCanvas canvas)
			: base (IntPtr.Zero, true, false)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			RegisterHandle (SkiaApi.sk_overdraw_canvas_new (canvas.Handle));
		}
	}
}
