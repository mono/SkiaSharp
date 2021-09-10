using System;
using ElmSharp;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
	public class SKCanvasView : CustomRenderingView
	{
		private bool ignorePixelScaling;
		private SKImageInfo info;
		private SKSizeI canvasSize;

		public SKCanvasView(EvasObject parent)
			: base(parent)
		{
			info = new SKImageInfo(0, 0, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
		}

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				if (ignorePixelScaling != value)
				{
					ignorePixelScaling = value;
					OnResized();
				}
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override SKSizeI GetSurfaceSize() => canvasSize;

		protected override SKSizeI GetRawSurfaceSize() => info.Size;

		protected virtual void OnDrawFrame(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected sealed override void OnDrawFrame()
		{
			if (info.Width == 0 || info.Height == 0)
				return;

			// draw directly into the EFL image data
			using (var surface = SKSurface.Create(info, Evas.evas_object_image_data_get(evasImage, true), info.RowBytes))
			{
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
		}

		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			var w = info.Width;
			var h = info.Height;

			info.Width = geometry.Width;
			info.Height = geometry.Height;

			if (IgnorePixelScaling)
			{
				canvasSize.Width = (int)ScalingInfo.FromPixel(geometry.Width);
				canvasSize.Height = (int)ScalingInfo.FromPixel(geometry.Height);
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
