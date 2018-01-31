using System;
using System.ComponentModel;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.Gtk
{
	[ToolboxItem(true)]
	public class SKWidget : global::Gtk.DrawingArea
	{
		private Gdk.Pixbuf pix;

		public SKWidget()
		{
			DoubleBuffered = false;
		}

		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override bool OnExposeEvent(Gdk.EventExpose evnt)
		{
			var result = base.OnExposeEvent(evnt);

			var window = evnt.Window;
			var area = evnt.Area;

			// get the pixbuf
			CreatePixbuf();
			var info = new SKImageInfo(pix.Width, pix.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			// create the surface
			using (var surface = SKSurface.Create(info, pix.Pixels, info.RowBytes))
			{
				// start drawing
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));

				surface.Canvas.Flush();

				// swap R and B
				if (info.ColorType == SKColorType.Bgra8888)
				{
					using (var pixmap = surface.PeekPixels())
					{
						SKSwizzle.SwapRedBlue(pixmap.GetPixels(), pixmap.GetPixels(), info.BytesSize);
					}
				}
			}

			// write the pixbuf to the graphics
			window.Clear();
			window.DrawPixbuf(null, pix, 0, 0, 0, 0, -1, -1, Gdk.RgbDither.None, 0, 0);

			return result;
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
				FreePixbuf();
			}
		}

		private void CreatePixbuf()
		{
			var alloc = Allocation;
			var w = alloc.Width;
			var h = alloc.Height;
			if (pix == null || pix.Width != w || pix.Height != h)
			{
				FreePixbuf();

				pix = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, true, 8, w, h);
			}
		}

		private void FreePixbuf()
		{
			if (pix != null)
			{
				pix.Dispose();
				pix = null;
			}
		}
	}
}
