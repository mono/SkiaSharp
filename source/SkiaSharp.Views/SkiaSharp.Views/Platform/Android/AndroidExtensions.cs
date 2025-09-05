using System;
using Android.Graphics;

namespace SkiaSharp.Views.Android
{
	/// <summary>
	/// Various extension methods to convert between SkiaSharp types and Xamarin.Android types.
	/// </summary>
	public static class AndroidExtensions
	{
		// Point*

		/// <summary>
		/// Converts a Xamarin.Android point into a SkiaSharp point.
		/// </summary>
		/// <param name="point">The Xamarin.Android point.</param>
		/// <returns>Returns a SkiaSharp point.</returns>
		public static SKPoint ToSKPoint(this PointF point)
		{
			return new SKPoint(point.X, point.Y);
		}

		/// <summary>
		/// Converts a Xamarin.Android point into a SkiaSharp point.
		/// </summary>
		/// <param name="point">The Xamarin.Android point.</param>
		/// <returns>Returns a SkiaSharp point.</returns>
		public static SKPointI ToSKPoint(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		/// <summary>
		/// Converts a SkiaSharp point into a Xamarin.Android point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a Xamarin.Android point.</returns>
		public static PointF ToPoint(this SKPoint point)
		{
			return new PointF(point.X, point.Y);
		}

		/// <summary>
		/// Converts a SkiaSharp point into a Xamarin.Android point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a Xamarin.Android point.</returns>
		public static Point ToPoint(this SKPointI point)
		{
			return new Point(point.X, point.Y);
		}

		// Rectangle*

		/// <summary>
		/// Converts a Xamarin.Android rectangle into a SkiaSharp rectangle.
		/// </summary>
		/// <param name="rect">The Xamarin.Android rectangle.</param>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		public static SKRect ToSKRect(this RectF rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// Converts a Xamarin.Android rectangle into a SkiaSharp rectangle.
		/// </summary>
		/// <param name="rect">The Xamarin.Android rectangle.</param>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		public static SKRectI ToSKRect(this Rect rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a Xamarin.Android rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a Xamarin.Android rectangle.</returns>
		public static RectF ToRect(this SKRect rect)
		{
			return new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a Xamarin.Android rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a Xamarin.Android rectangle.</returns>
		public static Rect ToRect(this SKRectI rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Color

		/// <summary>
		/// Converts a Xamarin.Android color into a SkiaSharp color.
		/// </summary>
		/// <param name="color">The Xamarin.Android color.</param>
		/// <returns>Returns a SkiaSharp color.</returns>
		public static SKColor ToSKColor(this Color color)
		{
			return (SKColor)(uint)(int)color;
		}

		/// <summary>
		/// Converts a SkiaSharp color into a Xamarin.Android color.
		/// </summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Returns a Xamarin.Android color.</returns>
		public static Color ToColor(this SKColor color)
		{
			return new Color((int)(uint)color);
		}

		// Matrix

		/// <summary>
		/// Converts a Xamarin.Android matrix into a SkiaSharp matrix.
		/// </summary>
		/// <param name="matrix">The Xamarin.Android matrix.</param>
		/// <returns>Returns a SkiaSharp matrix.</returns>
		public static SKMatrix ToSKMatrix(this Matrix matrix)
		{
			var values = new float[9];
			matrix.GetValues(values);
			return new SKMatrix { Values = values };
		}

		/// <summary>
		/// Converts a SkiaSharp matrix into a Xamarin.Android matrix.
		/// </summary>
		/// <param name="matrix">The SkiaSharp matrix.</param>
		/// <returns>Returns a Xamarin.Android matrix.</returns>
		public static Matrix ToMatrix(this SKMatrix matrix)
		{
			var native = new Matrix();
			native.SetValues(matrix.Values);
			return native;
		}

		// Image types

		private static SKImageInfo GetInfo(Bitmap bitmap)
		{
			var config = bitmap.GetConfig();
			var colorType = SKColorType.Rgba8888;
			if (config == Bitmap.Config.Alpha8)
			{
				colorType = SKColorType.Alpha8;
			}
#pragma warning disable CS0618 // Type or member is obsolete
			else if (config == Bitmap.Config.Argb4444)
			{
				colorType = SKColorType.Argb4444;
			}
#pragma warning restore CS0618 // Type or member is obsolete
			else if (config == Bitmap.Config.Rgb565)
			{
				colorType = SKColorType.Rgb565;
			}
			return new SKImageInfo(bitmap.Width, bitmap.Height, colorType);
		}

		/// <summary>
		/// Converts a Xamarin.Android bitmap into a SkiaSharp image.
		/// </summary>
		/// <param name="bitmap">The Xamarin.Android bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a SkiaSharp image.</returns>
		public static SKImage ToSKImage(this Bitmap bitmap)
		{
			var info = GetInfo(bitmap);
			var ptr = bitmap.LockPixels();
			var image = SKImage.FromPixelCopy(info, ptr);
			bitmap.UnlockPixels();
			return image;
		}

		/// <summary>
		/// Converts a Xamarin.Android bitmap into a SkiaSharp pixmap.
		/// </summary>
		/// <param name="bitmap">The Xamarin.Android bitmap.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the bitmap data.</param>
		public static void ToSKPixmap(this Bitmap bitmap, SKPixmap pixmap)
		{
			// create an image that wraps the existing pixels
			var info = GetInfo(bitmap);
			var ptr = bitmap.LockPixels();
			var image = SKImage.FromPixels(info, ptr);

			// read into pixmap (converting if necessary)
			image.ReadPixels(pixmap, 0, 0);
			bitmap.UnlockPixels();
		}

		/// <summary>
		/// Converts a Xamarin.Android bitmap into a SkiaSharp bitmap.
		/// </summary>
		/// <param name="bitmap">The Xamarin.Android bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a SkiaSharp bitmap.</returns>
		public static SKBitmap ToSKBitmap(this Bitmap bitmap)
		{
			var info = GetInfo(bitmap);
			var bmp = new SKBitmap(info);
			bitmap.ToSKPixmap(bmp.PeekPixels());
			return bmp;
		}

		/// <summary>
		/// Converts a SkiaSharp bitmap into a Xamarin.Android bitmap.
		/// </summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a Xamarin.Android bitmap.</returns>
		public static Bitmap ToBitmap(this SKBitmap skiaBitmap)
		{
			using var pixmap = skiaBitmap.PeekPixels();
			var bmp = pixmap.ToBitmap();
			GC.KeepAlive(skiaBitmap);
			return bmp;
		}

		/// <summary>
		/// Converts a SkiaSharp image into a Xamarin.Android bitmap.
		/// </summary>
		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <returns>Returns a copy of the image data as a Xamarin.Android bitmap.</returns>
		public static Bitmap ToBitmap(this SKImage skiaImage)
		{
			var info = skiaImage.Info;

			// destination values
			var config = Bitmap.Config.Argb8888;
			var dstInfo = new SKImageInfo(info.Width, info.Height);

			// try keep the pixel format if we can
			switch (info.ColorType)
			{
				case SKColorType.Alpha8:
					config = Bitmap.Config.Alpha8;
					dstInfo.ColorType = SKColorType.Alpha8;
					break;
				case SKColorType.Rgb565:
					config = Bitmap.Config.Rgb565;
					dstInfo.ColorType = SKColorType.Rgb565;
					dstInfo.AlphaType = SKAlphaType.Opaque;
					break;
#pragma warning disable CS0618 // Type or member is obsolete
				case SKColorType.Argb4444:
					config = Bitmap.Config.Argb4444;
					dstInfo.ColorType = SKColorType.Argb4444;
					break;
#pragma warning restore CS0618 // Type or member is obsolete
			}

			// destination bitmap
			var bmp = Bitmap.CreateBitmap(info.Width, info.Height, config);
			var ptr = bmp.LockPixels();

			// copy
			var success = skiaImage.ReadPixels(dstInfo, ptr, dstInfo.RowBytes);

			// confirm
			bmp.UnlockPixels();
			if (!success)
			{
				bmp.Recycle();
				bmp.Dispose();
				bmp = null;
			}

			GC.KeepAlive(skiaImage);
			return bmp;
		}

		/// <summary>
		/// Converts a SkiaSharp pixmap into a Xamarin.Android bitmap.
		/// </summary>
		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <returns>Returns a copy of the pixel data as a Xamarin.Android bitmap.</returns>
		public static Bitmap ToBitmap(this SKPixmap skiaPixmap)
		{
			using var image = SKImage.FromPixels(skiaPixmap);
			var bmp = image.ToBitmap();
			return bmp;
		}

		/// <summary>
		/// Converts a SkiaSharp picture into a Xamarin.Android bitmap.
		/// </summary>
		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <returns>Returns a copy of the picture as a Xamarin.Android bitmap.</returns>
		public static Bitmap ToBitmap(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			using (var img = SKImage.FromPicture(skiaPicture, dimensions))
			{
				return img.ToBitmap();
			}
		}
	}
}
