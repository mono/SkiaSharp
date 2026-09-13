using System;
using Tizen.NUI;

namespace SkiaSharp.Views.Tizen.NUI
{
	/// <summary>A software-rendered canvas view for drawing SkiaSharp content in a Tizen NUI application.</summary>
	/// <remarks>This view uses CPU-based rendering. For GPU-accelerated rendering, use <see cref="T:SkiaSharp.Views.Tizen.NUI.SKGLSurfaceView" /> instead.</remarks>
	public class SKCanvasView : CustomRenderingView
	{
		private bool ignorePixelScaling;

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.Views.Tizen.NUI.SKCanvasView" /> class.</summary>
		/// <remarks />
		public SKCanvasView()
		{
			OnResized();
		}

		/// <summary>Gets or sets a value indicating whether the drawing canvas should ignore the device pixel scaling.</summary>
		/// <value><see langword="true" /> to use the view size as the canvas size; <see langword="false" /> to scale the canvas to match the device pixel density. The default is <see langword="false" />.</value>
		/// <remarks>When set to <see langword="true" />, the canvas size will match the view size in points rather than pixels. This can simplify drawing code but may result in lower quality output on high-DPI displays.</remarks>
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

		/// <summary>Called when the view needs to render a frame.</summary>
		/// <remarks>This method creates the drawing surface and raises the <see cref="E:SkiaSharp.Views.Tizen.NUI.CustomRenderingView.PaintSurface" /> event.</remarks>
		protected override void OnDrawFrame()
		{
			if (Size.Width <= 0 || Size.Height <= 0)
				return;

			int width = (int)Size.Width;
			int height = (int)Size.Height;
			int stride = 4 * (int)Size.Width;
			SKSizeI canvasSize = default(SKSizeI);

			using var pixelBuffer = new PixelBuffer((uint)width, (uint)height, PixelFormat.BGRA8888);
			var buffer = pixelBuffer.GetBuffer();

			var info = new SKImageInfo(width, height, SKColorType.Bgra8888);
			using (var surface = SKSurface.Create(info, buffer, stride))
			{
				if (surface == null)
				{
					Invalidate();
					return;
				}

				if (IgnorePixelScaling)
				{
					var skiaCanvas = surface.Canvas;
					skiaCanvas.Scale((float)ScalingInfo.ScalingFactor);
					skiaCanvas.Save();

					canvasSize.Width = (int)Math.Round(ScalingInfo.FromPixel(width));
					canvasSize.Height = (int)Math.Round(ScalingInfo.FromPixel(height));
				}
				else
				{
					canvasSize.Width = width;
					canvasSize.Height = height;
				}

				// draw using SkiaSharp
				SendPaintSurface(new SKPaintSurfaceEventArgs(surface, info.WithSize(canvasSize), info));
				surface.Canvas.Flush();
			}

			using var pixelData = PixelBuffer.Convert(pixelBuffer);
			using var url = pixelData.GenerateUrl();
			SetImage(url.ToString());
		}

		/// <summary>Called when the view has been resized.</summary>
		/// <remarks>This method updates the canvas size and reallocates the drawing surface to match the new view dimensions.</remarks>
		protected override void OnResized()
		{
			if (Size.Width <= 0 || Size.Height <= 0)
				return;
			OnDrawFrame();
		}
	}
}
