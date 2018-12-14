using System;
using System.IO;

namespace SkiaSharp
{
	public abstract class SKDrawable : SKAbstractDrawable
	{
		[Preserve]
		internal SKDrawable (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_manageddrawable_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public uint GenerationID => SkiaApi.sk_manageddrawable_get_generation_id (Handle);

		public SKRect Bounds {
			get {
				SkiaApi.sk_manageddrawable_get_bounds (Handle, out var rect);
				return rect;
			}
		}

		public void Draw (SKCanvas canvas, ref SKMatrix matrix)
		{
			SkiaApi.sk_manageddrawable_draw (canvas.Handle, ref matrix);
		}

		public void Draw (SKCanvas canvas, float x, float y)
		{
			SkiaApi.sk_manageddrawable_draw (canvas.Handle, x, y);
		}

		public SKPicture NewPictureSnapshot ()
		{
			return GetObject<SKPicture> (SkiaApi.sk_manageddrawable_new_picture_snapshot ());
		}

		public void NotifyDrawingChanged ()
		{
			SkiaApi.sk_manageddrawable_notify_drawing_changed ();
		}
	}
}
