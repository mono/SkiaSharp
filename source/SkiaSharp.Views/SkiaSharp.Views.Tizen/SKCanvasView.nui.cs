using Tizen.NUI;

namespace SkiaSharp.Views.Tizen.NUI
{
    public class SKCanvasView : CustomRenderingView
    {
		private bool ignorePixelScaling;

		public SKCanvasView()
        {
            OnResized();
        }

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

					canvasSize.Width = (int)ScalingInfo.FromPixel(width);
					canvasSize.Height = (int)ScalingInfo.FromPixel(height);
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

        protected override void OnResized()
        {
            if (Size.Width <= 0 || Size.Height <= 0)
                return;
            OnDrawFrame();
        }
    }
}
