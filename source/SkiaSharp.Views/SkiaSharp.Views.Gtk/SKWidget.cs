using System;
using System.ComponentModel;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.Gtk
{
	[ToolboxItem(true)]
	public class SKWidget : global::Gtk.DrawingArea
	{
		private Gdk.Pixbuf pix;
		private SKSurface surface;

		public SKWidget()
		{
		}

		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override bool OnExposeEvent(Gdk.EventExpose evnt)
		{
			var window = evnt.Window;

			// get the pixbuf
			var imgInfo = CreateDrawingObjects();

			if (imgInfo.Width == 0 || imgInfo.Height == 0)
				return true;

			// start drawing
			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, imgInfo));
			}

			surface.Canvas.Flush();

			// swap R and B
			if (imgInfo.ColorType == SKColorType.Bgra8888)
			{
				using (var pixmap = surface.PeekPixels())
				{
					SKSwizzle.SwapRedBlue(pixmap.GetPixels(), imgInfo.Width * imgInfo.Height);
				}
			}

			// write the pixbuf to the graphics
			window.Clear();
			window.DrawPixbuf(null, pix, 0, 0, 0, 0, imgInfo.Width, imgInfo.Height, Gdk.RgbDither.None, 0, 0);

			return true;
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		~SKWidget()
		{
			Dispose(false);
		}

		public override void Destroy()
		{
			GC.SuppressFinalize(this);
			Dispose(true);

			base.Destroy();
		}

		public override void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(true);

			base.Dispose();
		}

		public virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				FreeDrawingObjects();
			}
		}

		private SKImageInfo CreateDrawingObjects()
		{
			var alloc = Allocation;
			var w = alloc.Width;
			var h = alloc.Height;
			var imgInfo = new SKImageInfo(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			if (pix == null || pix.Width != imgInfo.Width || pix.Height != imgInfo.Height)
			{
				FreeDrawingObjects();

				if (imgInfo.Width != 0 && imgInfo.Height != 0)
				{
					pix = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, imgInfo.Width, imgInfo.Height);

					// (re)create the SkiaSharp drawing objects
					surface = SKSurface.Create(imgInfo, pix.Pixels, imgInfo.RowBytes);
				}
			}

			return imgInfo;
		}

		private void FreeDrawingObjects()
		{
			pix?.Dispose();
			pix = null;

			// SkiaSharp objects should only exist if the Pixbuf is set as well
			surface?.Dispose();
			surface = null;
		}
	}
}
