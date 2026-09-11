using System;
using System.ComponentModel;
using Cairo;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.Gtk
{
	/// <summary>A GTK# view that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	[ToolboxItem(true)]
	public class SKDrawingArea : global::Gtk.DrawingArea
	{
		private ImageSurface pix;
		private SKSurface surface;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Gtk.SKDrawingArea" /> class.</summary>
		/// <remarks />
		public SKDrawingArea()
		{
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Gtk.SKDrawingArea.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Gtk.SKDrawingArea.PaintSurface>
		/// event.
		///
		/// ## Examples
		///
		/// ```csharp
		/// myView.PaintSurface += (sender, e) => {
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///     canvas.Flush ();
		/// };
		/// ```
		/// ]]></format></remarks>
		[Category("Appearance")]
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		/// <summary>Default handler for the Gtk.Widget.Drawn event.</summary>
		/// <param name="cr">The <see cref="T:Cairo.Context" /> to be used to paint the widget.</param>
		/// <returns>Returns <see langword="true" /> to stop other handlers from being invoked for the event, or <see langword="false" /> to continue the event propagation.</returns>
		/// <remarks>Override this method in a subclass to provide a default handler for the Gtk.Widget.Drawn event. The <see cref="T:Cairo.Context" /> will be disposed after this method returns, so you should not keep a reference to it outside of the scope of this method.</remarks>
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

		/// <summary>Implement this to draw on the canvas.</summary>
		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Gtk.SKDrawingArea.OnPaintSurface(SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Gtk.SKDrawingArea.PaintSurface>
		/// event.
		///
		/// > [!IMPORTANT]
		/// > If this method is overridden, then the base must be called, otherwise the
		/// > event will not be fired.
		///
		/// ## Examples
		///
		/// ```csharp
		/// protected override void OnPaintSurface (SKPaintSurfaceEventArgs e)
		/// {
		///     // call the base method
		///     base.OnPaintSurface (e);
		///
		///     var surface = e.Surface;
		///     var surfaceWidth = e.Info.Width;
		///     var surfaceHeight = e.Info.Height;
		///
		///     var canvas = surface.Canvas;
		///
		///     // draw on the canvas
		///
		///     canvas.Flush ();
		/// }
		/// ```
		/// ]]></format></remarks>
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Releases the resources used by this <see cref="T:SkiaSharp.Views.Gtk.SKDrawingArea" />.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and native resources; <see langword="false" /> to release only native resources.</param>
		/// <remarks></remarks>
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
