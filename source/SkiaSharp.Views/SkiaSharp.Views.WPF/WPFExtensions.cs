using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SkiaSharp.Views.WPF
{
	public static class WPFExtensions
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

		// WriteableBitmap

		public static WriteableBitmap ToWriteableBitmap(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image.ToWriteableBitmap();
			}
		}

		public static WriteableBitmap ToWriteableBitmap(this SKImage skiaImage)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
			var bitmap = new WriteableBitmap(info.Width, info.Height, 96, 96, PixelFormats.Pbgra32, null);
			bitmap.Lock();

			// copy
			using (var pixmap = new SKPixmap(info, bitmap.BackBuffer, bitmap.BackBufferStride))
			{
				skiaImage.ReadPixels(pixmap, 0, 0);
			}

			bitmap.AddDirtyRect(new Int32Rect(0, 0, info.Width, info.Height));
			bitmap.Unlock();
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

		public static SKBitmap ToSKBitmap(this BitmapSource bitmap)
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

		public static SKImage ToSKImage(this BitmapSource bitmap)
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

		public static void ToSKPixmap(this BitmapSource bitmap, SKPixmap pixmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			if (pixmap.ColorType == SKImageInfo.PlatformColorType)
			{
				var info = pixmap.Info;
				var converted = new FormatConvertedBitmap(bitmap, PixelFormats.Pbgra32, null, 0);
				converted.CopyPixels(new Int32Rect(0, 0, info.Width, info.Height), pixmap.GetPixels(), info.BytesSize, info.RowBytes);
			}
			else
			{
				// we have to copy the pixels into a format that we understand
				// and then into a desired format
				// TODO: we can still do a bit more for other cases where the color types are the same
				using (var tempImage = bitmap.ToSKImage())
				{
					tempImage.ReadPixels(pixmap, 0, 0);
				}
			}
		}
	}
}
