using System;
using Windows.Foundation;
using Windows.UI;

#if WINDOWS || WINUI
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml.Media.Imaging;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
#if WINDOWS
	public static class WindowsExtensions
#else
	public static class UWPExtensions
#endif
	{
		// Point

		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		public static Point ToPoint(this SKPoint point)
		{
			return new Point(point.X, point.Y);
		}

		// Rect

		public static SKRect ToSKRect(this Rect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		public static Rect ToRect(this SKRect rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Size

		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		public static Size ToSize(this SKSize size)
		{
			return new Size(size.Width, size.Height);
		}

		// Color

		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor(color.R, color.G, color.B, color.A);
		}

		public static Color ToColor(this SKColor color)
		{
			return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
		}

#if !HAS_UNO
		// WriteableBitmap

		public static WriteableBitmap ToWriteableBitmap(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image?.ToWriteableBitmap();
			}
		}

		public static WriteableBitmap ToWriteableBitmap(this SKImage skiaImage)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			// TODO: remove this as it is old/default logic
			//using (var tempImage = SKImage.Create(info))
			//using (var pixmap = tempImage.PeekPixels())
			//using (var data = SKData.Create(pixmap.GetPixels(), info.BytesSize))
			//{
			//	skiaImage.ReadPixels(pixmap, 0, 0);
			//	using (var stream = bitmap.PixelBuffer.AsStream())
			//	{
			//		data.SaveTo(stream);
			//	}
			//}

			var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
			var bitmap = new WriteableBitmap(info.Width, info.Height);
			using (var pixmap = new SKPixmap(info, bitmap.GetPixels()))
			{
				skiaImage.ReadPixels(pixmap, 0, 0);
			}
			bitmap.Invalidate();
			return bitmap;
		}

		public static WriteableBitmap ToWriteableBitmap(this SKBitmap skiaBitmap)
		{
			using (var pixmap = skiaBitmap.PeekPixels())
			using (var image = SKImage.FromPixels(pixmap))
			{
				var wb = image.ToWriteableBitmap();
				GC.KeepAlive(skiaBitmap);
				return wb;
			}
		}

		public static WriteableBitmap ToWriteableBitmap(this SKPixmap pixmap)
		{
			using (var image = SKImage.FromPixels(pixmap))
			{
				return image.ToWriteableBitmap();
			}
		}

		public static SKBitmap ToSKBitmap(this WriteableBitmap bitmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight);
			var skiaBitmap = new SKBitmap(info);
			using (var pixmap = skiaBitmap.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
			}
			return skiaBitmap;
		}

		public static SKImage ToSKImage(this WriteableBitmap bitmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight);
			var image = SKImage.Create(info);
			using (var pixmap = image.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
			}
			return image;
		}

		public static bool ToSKPixmap(this WriteableBitmap bitmap, SKPixmap pixmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			if (pixmap.ColorType == SKImageInfo.PlatformColorType)
			{
				using (var image = SKImage.FromPixels(pixmap.Info, bitmap.GetPixels()))
				{
					return image.ReadPixels(pixmap, 0, 0);
				}
			}
			else
			{
				// we have to copy the pixels into a format that we understand
				// and then into a desired format
				// TODO: we can still do a bit more for other cases where the color types are the same
				using (var tempImage = bitmap.ToSKImage())
				{
					return tempImage.ReadPixels(pixmap, 0, 0);
				}
			}
		}

		internal static IntPtr GetPixels(this WriteableBitmap bitmap) =>
			bitmap.PixelBuffer.GetByteBuffer();
#endif
	}
}
