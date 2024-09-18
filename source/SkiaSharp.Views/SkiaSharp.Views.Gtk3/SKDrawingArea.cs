using System;
using System.ComponentModel;
using Cairo;
using Gtk;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.Gtk
{
	[ToolboxItem(true)]
	public class SKDrawingArea : global::Gtk.DrawingArea
	{
		private ImageSurface pix;
		private SKSurface surface;
		private bool ignorePixelScaling = true; // Enable by default to keep previous behavior

		public SKDrawingArea()
		{
		}

		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				ignorePixelScaling = value;
				QueueDraw();
			}
		}

		protected override bool OnDrawn(Context cr)
		{
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

			pix.MarkDirty();

			// swap R and B
			if (imgInfo.ColorType == SKColorType.Rgba8888)
			{
				using (var pixmap = surface.PeekPixels())
				{
					SKSwizzle.SwapRedBlue(pixmap.GetPixels(), imgInfo.Width * imgInfo.Height);
				}
			}

			// write the pixbuf to the graphics
			if (!IgnorePixelScaling)
			{
				cr.Scale(1.0f / ScaleFactor, 1.0f / ScaleFactor);
			}
			cr.SetSourceSurface(pix, 0, 0);
			cr.Paint();

			return true;
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void Dispose(bool disposing)
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

			if (!IgnorePixelScaling)
			{
				w *= ScaleFactor;
				h *= ScaleFactor;
			}

			var imgInfo = new SKImageInfo(w, h, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			if (pix == null || pix.Width != imgInfo.Width || pix.Height != imgInfo.Height)
			{
				FreeDrawingObjects();

				if (imgInfo.Width != 0 && imgInfo.Height != 0)
				{
					pix = new ImageSurface(Format.Argb32, imgInfo.Width, imgInfo.Height);

					// (re)create the SkiaSharp drawing objects
					surface = SKSurface.Create(imgInfo, pix.DataPtr, imgInfo.RowBytes);
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
