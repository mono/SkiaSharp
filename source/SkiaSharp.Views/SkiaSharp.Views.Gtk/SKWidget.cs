using System;
using System.ComponentModel;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.Gtk
{
	[ToolboxItem(true)]
	public class SKWidget : global::Gtk.DrawingArea
	{
		private Gdk.Pixbuf pix;
		private SKImageInfo imgInfo;
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
			var area = evnt.Area;

			// get the pixbuf
			CreateDrawingObjects();

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
			window.DrawPixbuf(null, pix, 0, 0, 0, 0, -1, -1, Gdk.RgbDither.None, 0, 0);

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

		private void CreateDrawingObjects()
		{
			var alloc = Allocation;
			var w = alloc.Width;
			var h = alloc.Height;
			if (pix == null || pix.Width != w || pix.Height != h)
			{
				FreeDrawingObjects();

				pix = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, w, h);

				// (Re)create the Skia Drawing objects
				imgInfo = new SKImageInfo(pix.Width, pix.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
				surface = SKSurface.Create(imgInfo, pix.Pixels, imgInfo.RowBytes);
			}
		}

		private void FreeDrawingObjects()
		{
			if (pix != null)
			{
				pix.Dispose();
				pix = null;

				// Skia objects should only exist if the Pixbuf is set as well
				surface?.Dispose();
				surface = null;
			}
		}
	}
}
