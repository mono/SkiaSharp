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
		private bool ignorePixelScaling;
		private SKSizeI canvasSize;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Gtk.SKDrawingArea" /> class.</summary>
		/// <remarks />
		public SKDrawingArea()
			: base(new GObject.ConstructArgument[] { })
		{
			SetDrawFunc(OnDrawFunc);
		}

		/// <summary>Gets the current canvas size.</summary>
		/// <value>The current canvas size in pixels.</value>
		/// <remarks>The canvas size uses backing-surface pixels unless <see cref="IgnorePixelScaling" /> is enabled.</remarks>
		public SKSize CanvasSize => canvasSize;

		/// <summary>Gets or sets whether paint coordinates use logical pixels instead of backing-surface pixels.</summary>
		/// <remarks>By default, paint coordinates use physical pixels. When enabled, paint events expose logical <c>Info</c> and physical <c>RawInfo</c>; the canvas is scaled to match logical coordinates.</remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling == value)
					return;
				ignorePixelScaling = value;
				QueueDraw();
			}
		}

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
		public event EventHandler<SKPaintSurfaceEventArgs>? PaintSurface;

		private void OnDrawFunc(global::Gtk.DrawingArea area, Cairo.Context cr, int width, int height)
		{
			if (width <= 0 || height <= 0)
				return;

			// get the drawing objects
			var scale = GetScaleFactor();
			var imgInfo = CreateDrawingObjects(checked(width * scale), checked(height * scale));

			if (imgInfo.Width == 0 || imgInfo.Height == 0 || surface == null || pix == null)
				return;

			// start drawing
			var displayInfo = ignorePixelScaling
				? imgInfo.WithSize(new SKSizeI(width, height))
				: imgInfo;
			canvasSize = displayInfo.Size;
			using (new SKAutoCanvasRestore(surface.Canvas, true))
			{
				if (ignorePixelScaling)
					surface.Canvas.Scale(scale);
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, displayInfo, imgInfo));
			}

			surface.Canvas.Flush();

			// Flush any existing Cairo snapshots before modifying the backing pixels.
			pix.Flush();

			// swap R and B
			if (imgInfo.ColorType == SKColorType.Rgba8888)
			{
				using (var pixmap = surface.PeekPixels())
				{
					SKSwizzle.SwapRedBlue(pixmap.GetPixels(), imgInfo.Width * imgInfo.Height);
				}
			}
			pix.MarkDirty();

			// write the surface to the cairo context
			cr.Save();
			cr.Scale(1.0 / scale, 1.0 / scale);
			cr.SetSourceSurface(pix, 0, 0);
			cr.Paint();
			cr.Restore();
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
					surface = SKSurface.Create(imgInfo, dataPtr, pix.Stride)
						?? throw new InvalidOperationException("Unable to create the GTK4 drawing surface.");
				}
			}

			return imgInfo;
		}

		private void FreeDrawingObjects()
		{
			surface?.Dispose();
			surface = null;

			pix?.Dispose();
			pix = null;
			canvasSize = default;
		}
	}
}
