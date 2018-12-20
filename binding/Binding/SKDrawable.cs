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

		public SKDrawable() : base ()
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
				return SkiaApi.sk_manageddrawable_get_bounds (Handle);
			}
		}

		public void Draw (SKCanvas canvas, ref SKMatrix matrix)
		{
			SkiaApi.sk_manageddrawable_draw (Handle, canvas.Handle, ref matrix);
		}

		public void Draw (SKCanvas canvas, float x, float y)
		{
			var matrix = SKMatrix.MakeTranslation (x, y);
			SkiaApi.sk_manageddrawable_draw (Handle, canvas.Handle, ref matrix);
		}

		public SKPicture NewPictureSnapshot ()
		{
			return GetObject<SKPicture> (SkiaApi.sk_manageddrawable_new_picture_snapshot (Handle));
		}

		public void NotifyDrawingChanged ()
		{
			SkiaApi.sk_manageddrawable_notify_drawing_changed (Handle);
		}

		protected override SKPicture OnNewPictureSnapshot ()
		{
			var recorder = new SKPictureRecorder ();
			var canvas = recorder.BeginRecording (Bounds);
			Draw (canvas, 0, 0);
			return recorder.EndRecording ();
		}
	}
}
