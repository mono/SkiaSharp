using System;
using System.ComponentModel;
using SkiaSharp.Views.Desktop;

#nullable enable

namespace SkiaSharp.Views.Gtk
{
	/// <summary>A GTK view that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	public class SKDrawingArea : global::Gtk.DrawingArea
	{
		private Cairo.ImageSurface? pix;
		private SKSurface? surface;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Gtk.SKDrawingArea" /> class.</summary>
		/// <remarks />
		public SKDrawingArea()
			: base(new GObject.ConstructArgument[] { })
		{
			SetDrawFunc(OnDrawFunc);
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size may be different to the view size as a result of the current device's pixel density.</remarks>
		public SKSize CanvasSize => pix == null ? SKSize.Empty : new SKSize(pix.Width, pix.Height);

		/// <summary>Occurs when the canvas needs to be redrawn.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
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
		/// ]]></format>
		///         </remarks>
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

			// Flush any existing Cairo snapshots before marking dirty
			pix.Flush();
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

		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <summary>Implement this to draw on the canvas.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
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
		/// ]]></format>
		///         </remarks>
		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Releases the resources used by the current instance of the <see cref="T:SkiaSharp.Views.Gtk.SKDrawingArea" /> class.</summary>
		/// <remarks></remarks>
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
