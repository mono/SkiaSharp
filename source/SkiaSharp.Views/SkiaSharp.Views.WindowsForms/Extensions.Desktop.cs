using System;

namespace SkiaSharp.Views.Desktop
{
	/// <summary>
	/// Various extension methods to convert between SkiaSharp types and System.Drawing types.
	/// </summary>
	public static class Extensions
	{
		// System.Drawing.Point*

		/// <summary>
		/// Converts a System.Drawing point into a SkiaSharp point.
		/// </summary>
		/// <param name="point">The System.Drawing point.</param>
		/// <returns>Returns a SkiaSharp point.</returns>
		public static SKPoint ToSKPoint(this System.Drawing.PointF point)
		{
			return new SKPoint(point.X, point.Y);
		}

		/// <summary>
		/// Converts a System.Drawing point into a SkiaSharp point.
		/// </summary>
		/// <param name="point">The System.Drawing point.</param>
		/// <returns>Returns a SkiaSharp point.</returns>
		public static SKPointI ToSKPoint(this System.Drawing.Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		/// <summary>
		/// Converts a SkiaSharp point into a System.Drawing point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a System.Drawing point.</returns>
		public static System.Drawing.PointF ToDrawingPoint(this SKPoint point)
		{
			return new System.Drawing.PointF(point.X, point.Y);
		}

		/// <summary>
		/// Converts a SkiaSharp point into a System.Drawing point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a System.Drawing point.</returns>
		public static System.Drawing.Point ToDrawingPoint(this SKPointI point)
		{
			return new System.Drawing.Point(point.X, point.Y);
		}

		// System.Drawing.Rectangle*

		/// <summary>
		/// Converts a System.Drawing rectangle into a SkiaSharp rectangle.
		/// </summary>
		/// <param name="rect">The System.Drawing rectangle.</param>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		public static SKRect ToSKRect(this System.Drawing.RectangleF rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// Converts a System.Drawing rectangle into a SkiaSharp rectangle.
		/// </summary>
		/// <param name="rect">The System.Drawing rectangle.</param>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		public static SKRectI ToSKRect(this System.Drawing.Rectangle rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a System.Drawing rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a System.Drawing rectangle.</returns>
		public static System.Drawing.RectangleF ToDrawingRect(this SKRect rect)
		{
			return System.Drawing.RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a System.Drawing rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a System.Drawing rectangle.</returns>
		public static System.Drawing.Rectangle ToDrawingRect(this SKRectI rect)
		{
			return System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// System.Drawing.Size*

		/// <summary>
		/// Converts a System.Drawing size into a SkiaSharp size.
		/// </summary>
		/// <param name="size">The System.Drawing size.</param>
		/// <returns>Returns a SkiaSharp size.</returns>
		public static SKSize ToSKSize(this System.Drawing.SizeF size)
		{
			return new SKSize(size.Width, size.Height);
		}

		/// <summary>
		/// Converts a System.Drawing size into a SkiaSharp size.
		/// </summary>
		/// <param name="size">The System.Drawing size.</param>
		/// <returns>Returns a SkiaSharp size.</returns>
		public static SKSizeI ToSKSize(this System.Drawing.Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp size into a System.Drawing size.
		/// </summary>
		/// <param name="size">The SkiaSharp size.</param>
		/// <returns>Returns a System.Drawing size.</returns>
		public static System.Drawing.SizeF ToDrawingSize(this SKSize size)
		{
			return new System.Drawing.SizeF(size.Width, size.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp size into a System.Drawing size.
		/// </summary>
		/// <param name="size">The SkiaSharp size.</param>
		/// <returns>Returns a System.Drawing size.</returns>
		public static System.Drawing.Size ToDrawingSize(this SKSizeI size)
		{
			return new System.Drawing.Size(size.Width, size.Height);
		}

		// System.Drawing.Bitmap

		/// <summary>
		/// Converts a SkiaSharp picture into a System.Drawing bitmap.
		/// </summary>
		/// <param name="picture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <returns>Returns a copy of the picture as a System.Drawing bitmap.</returns>
		public static System.Drawing.Bitmap ToBitmap(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image.ToBitmap();
			}
		}

		/// <summary>
		/// Converts a SkiaSharp image into a System.Drawing bitmap.
		/// </summary>
		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <returns>Returns a copy of the image data as a System.Drawing bitmap.</returns>
		public static System.Drawing.Bitmap ToBitmap(this SKImage skiaImage)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var bitmap = new System.Drawing.Bitmap(skiaImage.Width, skiaImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
			var data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

			// copy
			using (var pixmap = new SKPixmap(new SKImageInfo(data.Width, data.Height), data.Scan0, data.Stride))
			{
				skiaImage.ReadPixels(pixmap, 0, 0);
			}

			bitmap.UnlockBits(data);
			return bitmap;
		}

		/// <summary>
		/// Converts a SkiaSharp bitmap into a System.Drawing bitmap.
		/// </summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a System.Drawing bitmap.</returns>
		public static System.Drawing.Bitmap ToBitmap(this SKBitmap skiaBitmap)
		{
			using (var pixmap = skiaBitmap.PeekPixels())
			using (var image = SKImage.FromPixels(pixmap))
			{
				var bmp = image.ToBitmap();
				GC.KeepAlive(skiaBitmap);
				return bmp;
			}
		}

		/// <summary>
		/// Converts a SkiaSharp pixmap into a System.Drawing bitmap.
		/// </summary>
		/// <param name="pixmap">The SkiaSharp pixmap.</param>
		/// <returns>Returns a copy of the pixel data as a System.Drawing bitmap.</returns>
		public static System.Drawing.Bitmap ToBitmap(this SKPixmap pixmap)
		{
			using (var image = SKImage.FromPixels(pixmap))
			{
				return image.ToBitmap();
			}
		}

		/// <summary>
		/// Converts a System.Drawing bitmap into a SkiaSharp bitmap.
		/// </summary>
		/// <param name="bitmap">The System.Drawing bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a SkiaSharp bitmap.</returns>
		public static SKBitmap ToSKBitmap(this System.Drawing.Bitmap bitmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(bitmap.Width, bitmap.Height);
			var skiaBitmap = new SKBitmap(info);
			using (var pixmap = skiaBitmap.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
			}
			return skiaBitmap;
		}

		/// <summary>
		/// Converts a System.Drawing bitmap into a SkiaSharp image.
		/// </summary>
		/// <param name="bitmap">The System.Drawing bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a SkiaSharp image.</returns>
		public static SKImage ToSKImage(this System.Drawing.Bitmap bitmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(bitmap.Width, bitmap.Height);
			var image = SKImage.Create(info);
			using (var pixmap = image.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
			}
			return image;
		}

		/// <summary>
		/// Converts a System.Drawing bitmap into a SkiaSharp pixmap.
		/// </summary>
		/// <param name="bitmap">The System.Drawing bitmap.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the bitmap data.</param>
		public static void ToSKPixmap(this System.Drawing.Bitmap bitmap, SKPixmap pixmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			if (pixmap.ColorType == SKImageInfo.PlatformColorType)
			{
				var info = pixmap.Info;
				using (var tempBitmap = new System.Drawing.Bitmap(info.Width, info.Height, info.RowBytes, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, pixmap.GetPixels()))
				using (var gr = System.Drawing.Graphics.FromImage(tempBitmap))
				{
					// Clear graphic to prevent display artifacts with transparent bitmaps					
					gr.Clear(System.Drawing.Color.Transparent);
					
					gr.DrawImageUnscaled(bitmap, 0, 0);
				}
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

		// System.Drawing.Color

		/// <summary>
		/// Converts a System.Drawing color into a SkiaSharp color.
		/// </summary>
		/// <param name="color">The System.Drawing color.</param>
		/// <returns>Returns a SkiaSharp color.</returns>
		public static SKColor ToSKColor(this System.Drawing.Color color)
		{
			return (SKColor)(uint)color.ToArgb();
		}

		/// <summary>
		/// Converts a SkiaSharp color into a System.Drawing color.
		/// </summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Returns a System.Drawing color.</returns>
		public static System.Drawing.Color ToDrawingColor(this SKColor color)
		{
			return System.Drawing.Color.FromArgb((int)(uint)color);
		}
	}
}
