using System;
using ElmSharp;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
	/// <summary>A view that can be drawn on using SkiaSharp drawing commands.</summary>
	/// <remarks />
	public class SKCanvasView : CustomRenderingView
	{
		private bool ignorePixelScaling;
		private SKImageInfo info;
		private SKSizeI canvasSize;

		/// <param name="parent">The parent object.</param>
		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Tizen.SKCanvasView" /> class.</summary>
		/// <remarks>Use this constructor when creating the view programmatically from code.</remarks>
		public SKCanvasView(EvasObject parent)
			: base(parent)
		{
			info = new SKImageInfo(0, 0, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		}

		/// <summary>Gets or sets a value indicating whether the drawing canvas should be resized on high resolution displays.</summary>
		/// <value>
		///           <see langword="true" /> to ignore pixel scaling; otherwise, <see langword="false" />.</value>
		/// <remarks>By default, when <see langword="false" />, the canvas is resized to 1 canvas pixel per display pixel. When <see langword="true" />, the canvas is resized to device independent pixels, and then stretched to fill the view. Although performance is improved and all objects are the same size on different display densities, blurring and pixelation may occur.</remarks>
		public bool IgnorePixelScaling
		{
			get => ignorePixelScaling;
			set
			{
				if (ignorePixelScaling != value)
				{
					ignorePixelScaling = value;
					OnResized();
					Invalidate();
				}
			}
		}

		/// <summary>Occurs when the surface needs to be redrawn.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Tizen.SKCanvasView.OnDrawFrame(SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Tizen.SKCanvasView.PaintSurface>
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
		///
		///     canvas.Flush ();
		/// };
		/// ```
		/// ]]></format>
		///         </remarks>
		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to provide the dimensions of the current drawing surface.</summary>
		/// <returns>Returns the current drawing surface dimensions.</returns>
		/// <remarks />
		protected override SKSizeI GetSurfaceSize() => canvasSize;

		/// <summary>Gets the raw pixel size of the drawing surface.</summary>
		/// <returns>Returns the raw pixel size of the drawing surface.</returns>
		/// <remarks />
		protected override SKSizeI GetRawSurfaceSize() => info.Size;

		/// <param name="e">The event arguments that contain the drawing surface and information.</param>
		/// <summary>Implement this to draw on the canvas.</summary>
		/// <remarks>
		///           <format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// There are two ways to draw on this surface: by overriding the
		/// <xref:SkiaSharp.Views.Tizen.SKCanvasView.OnDrawFrame(SkiaSharp.Views.Tizen.SKPaintSurfaceEventArgs)>
		/// method, or by attaching a handler to the
		/// <xref:SkiaSharp.Views.Tizen.SKCanvasView.PaintSurface>
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
		protected virtual void OnDrawFrame(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to draw the next frame or paint the control.</summary>
		/// <remarks />
		protected sealed override void OnDrawFrame()
		{
			if (info.Width == 0 || info.Height == 0)
				return;

			// draw directly into the EFL image data
			using var surface = SKSurface.Create(info, Evas.evas_object_image_data_get(evasImage, true), info.RowBytes);

			if (IgnorePixelScaling)
			{
				var skiaCanvas = surface.Canvas;
				skiaCanvas.Scale((float)ScalingInfo.ScalingFactor);
				skiaCanvas.Save();
			}

			// draw using SkiaSharp
			OnDrawFrame(new SKPaintSurfaceEventArgs(surface, info.WithSize(canvasSize), info));
			surface.Canvas.Flush();
		}

		/// <param name="geometry">The current geometry of the control.</param>
		/// <summary>Implemented by derived <see cref="T:SkiaSharp.Views.Tizen.CustomRenderingView" /> types to update the drawing surface dimensions.</summary>
		/// <returns>Returns <see langword="true" /> if the size has changed, otherwise <see langword="false" />.</returns>
		/// <remarks />
		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			var w = info.Width;
			var h = info.Height;

			info.Width = geometry.Width;
			info.Height = geometry.Height;

			if (IgnorePixelScaling)
			{
				canvasSize.Width = (int)Math.Round(ScalingInfo.FromPixel(geometry.Width));
				canvasSize.Height = (int)Math.Round(ScalingInfo.FromPixel(geometry.Height));
			}
			else
			{
				canvasSize.Width = geometry.Width;
				canvasSize.Height = geometry.Height;
			}

			return (w != info.Width || h != info.Height);
		}
	}
}
