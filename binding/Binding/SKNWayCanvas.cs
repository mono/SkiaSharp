using System;

namespace SkiaSharp
{
	public class SKNWayCanvas : SKNoDrawCanvas
	{
		private protected SKNWayCanvas (IntPtr handle, bool owns = true, bool registerHandle = true)
			: base (handle, owns, registerHandle)
		{
		}

		public SKNWayCanvas (int width, int height)
			: base (SkiaApi.sk_nway_canvas_new (width, height))
		{
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
