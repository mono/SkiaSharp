using System;
using System.ComponentModel;
using SkiaSharp.Views.Desktop;

#nullable enable

namespace SkiaSharp.Views.Gtk
{
	public class SKDrawingArea : global::Gtk.DrawingArea
	{
		private Cairo.ImageSurface? pix;
		private SKSurface? surface;

		public SKDrawingArea()
			: base(new GObject.ConstructArgument[] { })
		{
			SetDrawFunc(OnDrawFunc);
		}

		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		private void OnDrawFunc(global::Gtk.DrawingArea area, Cairo.Context cr, int width, int height)
		{
			if (width <= 0 || height <= 0)
				return;

			// get the drawing objects
			var imgInfo = CreateDrawingObjects(width, height);

			if (imgInfo.Width == 0 || imgInfo.Height == 0 || surface == null || pix == null)
				return;

			// start drawing
			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, imgInfo));
			}

			surface.Canvas.Flush();

			pix.MarkDirty();

			// swap R and B
			if (imgInfo.ColorType == SKColorType.Rgba8888)
			{
				using (var pixmap = surface.PeekPixels())
				{
					SKSwizzle.SwapRedBlue(pixmap.GetPixels(), imgInfo.Width * imgInfo.Height);
				}
			}

			// write the surface to the cairo context
			cr.SetSourceSurface(pix, 0, 0);
			cr.Paint();
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		public override void Dispose()
		{
			FreeDrawingObjects();
			base.Dispose();
		}

		private SKImageInfo CreateDrawingObjects(int width, int height)
		{
			var imgInfo = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			if (pix == null || pix.Width != imgInfo.Width || pix.Height != imgInfo.Height)
			{
				FreeDrawingObjects();

				if (imgInfo.Width != 0 && imgInfo.Height != 0)
				{
					pix = new Cairo.ImageSurface(Cairo.Format.Argb32, imgInfo.Width, imgInfo.Height);

					// get the data pointer from the Cairo surface via the internal API
					var dataPtr = Cairo.Internal.ImageSurface.GetData(pix.Handle);

					// (re)create the SkiaSharp drawing objects using the Cairo stride
					surface = SKSurface.Create(imgInfo, dataPtr, pix.Stride);
				}
			}

			return imgInfo;
		}

		private void FreeDrawingObjects()
		{
			pix?.Dispose();
			pix = null;

			// SkiaSharp objects should only exist if the surface is set as well
			surface?.Dispose();
			surface = null;
		}
	}
}
