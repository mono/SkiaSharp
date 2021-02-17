using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace SkiaSharp.Views.UWP
{
	public partial class SKXamlCanvas : Canvas
	{
		private byte[] pixels;
		private GCHandle pixelsHandle;
		private int pixelWidth;
		private int pixelHeight;
		private WriteableBitmap bitmap;

		public SKXamlCanvas()
		{
			Initialize();
		}

		partial void DoUnloaded() =>
			FreeBitmap();

		private SKSize GetCanvasSize() =>
			new SKSize(pixelWidth, pixelHeight);

		private void DoInvalidate()
		{
			if (designMode)
				return;

			if (!isVisible)
				return;

			if (ActualWidth <= 0 || ActualHeight <= 0)
				return;

			var info = CreateBitmap();

			using (var surface = SKSurface.Create(info, pixelsHandle.AddrOfPinnedObject(), info.RowBytes))
			{
				OnPaintSurface(new SKPaintSurfaceEventArgs(surface, info));
			}

			// This implementation is not fast enough, and providing the original pixel buffer
			// is needed, yet the internal `IBufferByteAccess` interface is not yet available in Uno.
			// Once it is, we can replace this implementation and provide the pinned array directly
			// to skia.
			using (var data = bitmap.PixelBuffer.AsStream())
			{
				data.Write(pixels, 0, pixels.Length);
				data.Flush();
			}

			bitmap.Invalidate();
		}

		private SKSizeI CreateSize()
		{
			var w = ActualWidth;
			var h = ActualHeight;

			if (!IsPositive(w) || !IsPositive(h))
				return SKSizeI.Empty;

			if (IgnorePixelScaling)
				return new SKSizeI((int)w, (int)h);

			var dpi = Dpi;
			return new SKSizeI((int)(w * dpi), (int)(h * dpi));

			static bool IsPositive(double value)
			{
				return !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
			}
		}

		private SKImageInfo CreateBitmap()
		{
			var size = CreateSize();
			var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

			if (bitmap?.PixelWidth != info.Width || bitmap?.PixelHeight != info.Height)
				FreeBitmap();

			if (bitmap == null && info.Width > 0 && info.Height > 0)
			{
				bitmap = new WriteableBitmap(info.Width, info.Height);

				var brush = new ImageBrush
				{
					ImageSource = bitmap,
					AlignmentX = AlignmentX.Left,
					AlignmentY = AlignmentY.Top,
					Stretch = Stretch.None
				};

				if (!IgnorePixelScaling)
				{
					var scale = 1.0 / Dpi;
					brush.Transform = new ScaleTransform
					{
						ScaleX = scale,
						ScaleY = scale
					};
				}

				Background = brush;
			}

			if (pixels == null || pixelWidth != info.Width || pixelHeight != info.Height)
			{
				FreeBitmap();

				pixels = new byte[info.BytesSize];
				pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

				pixelWidth = info.Width;
				pixelHeight = info.Height;
			}

			return info;
		}

		private void FreeBitmap()
		{
			if (pixels != null)
			{
				pixels = null;
			}
		}
	}
}
