#nullable disable

using System;

namespace SkiaSharp
{
	public class SKOverdrawCanvas : SKNWayCanvas
	{
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
			// The native SkOverdrawCanvas stores a raw, non-owning pointer to the wrapped
			// canvas, so root the managed SKCanvas for the lifetime of this wrapper to
			// prevent it being finalized (and its native canvas destroyed) too early.
			Referenced (this, canvas);
			GC.KeepAlive (canvas);
		}
	}
}
