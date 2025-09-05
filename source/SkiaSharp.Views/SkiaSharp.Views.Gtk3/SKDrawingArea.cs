using System;
using System.ComponentModel;
using Cairo;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.Gtk
{
	/// <summary>
	/// A GTK# view that can be drawn on using SkiaSharp drawing commands.
	/// </summary>
	[ToolboxItem(true)]
	public class SKDrawingArea : global::Gtk.DrawingArea
	{
		private ImageSurface pix;
		private SKSurface surface;

		/// <summary>
		/// Default constructor that initializes a new instance of <see cref="SKDrawingArea" />.
		/// </summary>
		public SKDrawingArea()
		{
		}

		/// <summary>
		/// Gets the current canvas size.
		/// </summary>
		/// <remarks>
		/// The canvas size may be different to the view size as a result of the current device's pixel density.
		/// </remarks>
		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		/// <summary>
		/// Occurs when the canvas needs to be redrawn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There are two ways to draw on this surface: by overriding the
		/// <see cref="SKDrawingArea.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs)" />
		/// method, or by attaching a handler to the
		/// <see cref="SKDrawingArea.PaintSurface" />
		/// event.
		/// </para>
		/// </remarks>
		/// <example>
		/// <code language="csharp"><![CDATA[
		/// myView.PaintSurface += (sender, e) => 
		/// {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///     var canvas = surface.Canvas;
		/// 
		///     // draw on the canvas
		/// };
		/// ]]></code>
		/// </example>
		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

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
