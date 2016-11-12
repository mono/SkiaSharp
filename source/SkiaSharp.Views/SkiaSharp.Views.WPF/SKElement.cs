using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkiaSharp.Views.Desktop;

namespace SkiaSharp.Views.WPF
{
	public class SKElement : FrameworkElement
	{
		private readonly bool designMode;

		private WriteableBitmap bitmap;
		private bool ignorePixelScaling;

		public SKElement()
		{
			designMode = DesignerProperties.GetIsInDesignMode(this);
		}

		public SKSize CanvasSize => bitmap == null ? SKSize.Empty : new SKSize(bitmap.PixelWidth, bitmap.PixelHeight);

		public bool IgnorePixelScaling
		{
			get { return ignorePixelScaling; }
			set
			{
				ignorePixelScaling = value;
				InvalidateVisual();
			}
		}

		public event EventHandler<SKPaintSurfaceEventArgs> PaintSurface;

		protected override void OnRender(DrawingContext drawingContext)
		{
			if (designMode)
				return;

			base.OnRender(drawingContext);

			int width, height;
			double dpiX = 1.0;
			double dpiY = 1.0;
			if (IgnorePixelScaling)
			{
				width = (int)ActualWidth;
				height = (int)ActualHeight;
			}
			else
			{
				var m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
				dpiX = m.M11;
				dpiY = m.M22;
				width = (int)(ActualWidth * dpiX);
				height = (int)(ActualHeight * dpiY);
			}

			var info = new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			// reset the bitmap if the size has changed
			if (bitmap == null || info.Width != bitmap.PixelWidth || info.Height != bitmap.PixelHeight)
			{
				bitmap = new WriteableBitmap(width, height, dpiX, dpiY, PixelFormats.Pbgra32, null);
			}

			// draw on the bitmap
			bitmap.Lock();
			using (var surface = SKSurface.Create(info, bitmap.BackBuffer, bitmap.BackBufferStride))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			// draw the bitmap to the screen
			bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
			bitmap.Unlock();
			drawingContext.DrawImage(bitmap, new Rect(0, 0, ActualWidth, ActualHeight));
		}

		protected virtual void OnPaintSurface(SKPaintSurfaceEventArgs e)
		{
			// invoke the event
			PaintSurface?.Invoke(this, e);
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			InvalidateVisual();
		}
	}
}
