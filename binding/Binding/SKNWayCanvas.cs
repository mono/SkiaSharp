using System;

namespace SkiaSharp
{
	public class SKNWayCanvas : SKNoDrawCanvas
	{
		[Preserve]
		internal SKNWayCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKNWayCanvas (int width, int height)
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sk_nway_canvas_new (width, height);
		}

		public void AddCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_add_canvas (Handle, canvas.Handle);
		}

		public void RemoveCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_nway_canvas_remove_canvas (Handle, canvas.Handle);
		}

		public void RemoveAll ()
		{
			SkiaApi.sk_nway_canvas_remove_all (Handle);
		}
	}
}
