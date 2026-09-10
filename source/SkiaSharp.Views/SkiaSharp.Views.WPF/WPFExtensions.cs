using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SkiaSharp.Views.WPF
{
	/// <summary>Various extension methods to convert between SkiaSharp types and Windows types.</summary>
	/// <remarks />
	public static class WPFExtensions
	{
		// Point

		/// <param name="point">The Windows point.</param>
		/// <summary>Converts a Windows point into a SkiaSharp point.</summary>
		/// <returns>Returns a SkiaSharp point.</returns>
		/// <remarks />
		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		/// <param name="point">The SkiaSharp point.</param>
		/// <summary>Converts a SkiaSharp point into a Windows point.</summary>
		/// <returns>Returns a Windows point.</returns>
		/// <remarks />
		public static Point ToPoint(this SKPoint point)
		{
			return new Point(point.X, point.Y);
		}

		// Rect

		/// <param name="rect">The Windows rectangle.</param>
		/// <summary>Converts a Windows rectangle into a SkiaSharp rectangle.</summary>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRect ToSKRect(this Rect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <summary>Converts a SkiaSharp rectangle into a Windows rectangle.</summary>
		/// <returns>Returns a Windows rectangle.</returns>
		/// <remarks />
		public static Rect ToRect(this SKRect rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
		}

		// Size

		/// <param name="size">The Windows size.</param>
		/// <summary>Converts a Windows size into a SkiaSharp size.</summary>
		/// <returns>Returns a SkiaSharp size.</returns>
		/// <remarks />
		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		/// <param name="size">The SkiaSharp size.</param>
		/// <summary>Converts a SkiaSharp size into a Windows size.</summary>
		/// <returns>Returns a Windows size.</returns>
		/// <remarks />
		public static Size ToSize(this SKSize size)
		{
			return new Size(size.Width, size.Height);
		}

		// Color

		/// <param name="color">The Windows color.</param>
		/// <summary>Converts a Windows color into a SkiaSharp color.</summary>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor(color.R, color.G, color.B, color.A);
		}

		/// <param name="color">The SkiaSharp color.</param>
		/// <summary>Converts a SkiaSharp color into a Windows color.</summary>
		/// <returns>Returns a Windows color.</returns>
		/// <remarks />
		public static Color ToColor(this SKColor color)
		{
			return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
		}

		// WriteableBitmap

		/// <param name="picture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <summary>Converts a SkiaSharp picture into a Windows WriteableBitmap.</summary>
		/// <returns>Returns a copy of the picture as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKPicture picture, SKSizeI dimensions)
		{
			using var image = SKImage.FromPicture(picture, dimensions);
			return image.ToWriteableBitmap();
		}

		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <summary>Converts a SkiaSharp image into a Windows WriteableBitmap.</summary>
		/// <returns>Returns a copy of the image data as a Windows WriteableBitmap.</returns>
		/// <remarks />
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

		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <summary>Converts a SkiaSharp bitmap into a Windows WriteableBitmap.</summary>
		/// <returns>Returns a copy of the bitmap data as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKBitmap skiaBitmap)
		{
			using var pixmap = skiaBitmap.PeekPixels();
			using var image = SKImage.FromPixels(pixmap);
			var wb = image.ToWriteableBitmap();
			GC.KeepAlive(skiaBitmap);
			return wb;
		}

		/// <param name="pixmap">The SkiaSharp pixmap.</param>
		/// <summary>Converts a SkiaSharp pixmap into a Windows WriteableBitmap.</summary>
		/// <returns>Returns a copy of the pixel data as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKPixmap pixmap)
		{
			using var image = SKImage.FromPixels(pixmap);
			return image.ToWriteableBitmap();
		}

		/// <param name="bitmap">The Windows BitmapSource to convert.</param>
		/// <summary>Converts a Windows BitmapSource into a SkiaSharp bitmap.</summary>
		/// <returns>Returns a copy of the bitmap data as a SkiaSharp bitmap.</returns>
		/// <remarks />
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

		/// <param name="bitmap">The Windows BitmapSource to convert.</param>
		/// <summary>Converts a Windows BitmapSource into a SkiaSharp image.</summary>
		/// <returns>Returns a copy of the bitmap data as a SkiaSharp image.</returns>
		/// <remarks />
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

		/// <param name="bitmap">The Windows BitmapSource to convert.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the bitmap data.</param>
		/// <summary>Converts a Windows BitmapSource into a SkiaSharp pixmap.</summary>
		/// <remarks />
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
				using var tempImage = bitmap.ToSKImage();
				tempImage.ReadPixels(pixmap, 0, 0);
			}
		}
	}
}
