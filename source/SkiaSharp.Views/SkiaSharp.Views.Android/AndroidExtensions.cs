using System;
using Android.Graphics;

namespace SkiaSharp.Views.Android
{
	public static class AndroidExtensions
	{
		// Point*

		public static SKPoint ToSKPoint(this PointF point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static SKPointI ToSKPoint(this Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		public static PointF ToPoint(this SKPoint point)
		{
			return new PointF(point.X, point.Y);
		}

		public static Point ToPoint(this SKPointI point)
		{
			return new Point(point.X, point.Y);
		}

		// Rectangle*

		public static SKRect ToSKRect(this RectF rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static SKRectI ToSKRect(this Rect rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static RectF ToRect(this SKRect rect)
		{
			return new RectF(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static Rect ToRect(this SKRectI rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Color

		public static SKColor ToSKColor(this Color color)
		{
			return (SKColor)(uint)(int)color;
		}

		public static Color ToColor(this SKColor color)
		{
			return new Color((int)(uint)color);
		}

		// Matrix

		public static SKMatrix ToSKMatrix(this Matrix matrix)
		{
			var values = new float[9];
			matrix.GetValues(values);
			return new SKMatrix { Values = values };
		}

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
			else if (config == Bitmap.Config.Argb4444)
			{
				colorType = SKColorType.Argb4444;
			}
			else if (config == Bitmap.Config.Rgb565)
			{
				colorType = SKColorType.Rgb565;
			}
			return new SKImageInfo(bitmap.Width, bitmap.Height, colorType);
		}

		public static SKImage ToSKImage(this Bitmap bitmap)
		{
			var info = GetInfo(bitmap);
			var ptr = bitmap.LockPixels();
			var image = SKImage.FromPixelCopy(info, ptr);
			bitmap.UnlockPixels();
			return image;
		}

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

		public static SKBitmap ToSKBitmap(this Bitmap bitmap)
		{
			var info = GetInfo(bitmap);
			var bmp = new SKBitmap(info);
			bitmap.ToSKPixmap(bmp.PeekPixels());
			return bmp;
		}

		public static Bitmap ToBitmap(this SKBitmap skiaBitmap)
		{
			using (var pixmap = skiaBitmap.PeekPixels())
			{
				var bmp = pixmap.ToBitmap();
				GC.KeepAlive(skiaBitmap);
				return bmp;
			}
		}

		public static Bitmap ToBitmap(this SKPixmap skiaPixmap)
		{
			var info = skiaPixmap.Info;

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
				case SKColorType.Argb4444:
					config = Bitmap.Config.Argb4444;
					dstInfo.ColorType = SKColorType.Argb4444;
					break;
			}

			// destination bitmap
			var bmp = Bitmap.CreateBitmap(info.Width, info.Height, config);
			var ptr = bmp.LockPixels();

			// copy
			var success = skiaPixmap.ReadPixels(dstInfo, ptr, dstInfo.RowBytes);

			// confirm
			bmp.UnlockPixels();
			if (!success)
			{
				bmp.Recycle();
				bmp.Dispose();
				bmp = null;
			}

			return bmp;
		}

		public static Bitmap ToBitmap(this SKImage skiaImage)
		{
			using (var pixmap = skiaImage.PeekPixels())
			{
				var bmp = pixmap.ToBitmap();
				GC.KeepAlive(skiaImage);
				return bmp;
			}
		}

		public static Bitmap ToBitmap(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			using (var img = SKImage.FromPicture(skiaPicture, dimensions))
			{
				return img.ToBitmap();
			}
		}
	}
}
