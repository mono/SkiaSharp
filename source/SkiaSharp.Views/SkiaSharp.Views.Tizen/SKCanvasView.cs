using System;
using ElmSharp;
using SkiaSharp.Views.Tizen.Interop;

namespace SkiaSharp.Views.Tizen
{
	public class SKCanvasView : CustomRenderingView
	{
		private bool ignorePixelScaling;
		private SKImageInfo info;

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

		protected override SKSizeI GetSurfaceSize() => info.Size;

		protected virtual void OnDrawFrame(SKPaintSurfaceEventArgs e)
		{
			PaintSurface?.Invoke(this, e);
		}

		protected sealed override void OnDrawFrame()
		{
			// draw directly into the EFL image data
			using (var surface = SKSurface.Create(info, Evas.evas_object_image_data_get(evasImage, true), info.RowBytes))
			{
				// draw using SkiaSharp
				OnDrawFrame(new SKPaintSurfaceEventArgs(surface, info));
				surface.Canvas.Flush();
			}
		}

		protected sealed override bool UpdateSurfaceSize(Rect geometry)
		{
			var w = info.Width;
			var h = info.Height;

			if (IgnorePixelScaling)
			{
				info.Width = (int)ScalingInfo.FromPixel(geometry.Width);
				info.Height = (int)ScalingInfo.FromPixel(geometry.Height);
			}
			else
			{
				info.Width = geometry.Width;
				info.Height = geometry.Height;
			}

			return (w != info.Width || h != info.Height);
		}
	}
}
