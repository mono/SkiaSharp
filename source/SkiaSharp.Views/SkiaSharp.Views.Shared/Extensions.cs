using System;

#if HAS_UNO_WINUI
namespace SkiaSharp.Views.Windows
#elif WINDOWS_UWP || HAS_UNO
namespace SkiaSharp.Views.UWP
#elif __ANDROID__
namespace SkiaSharp.Views.Android
#elif __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __WATCHOS__
namespace SkiaSharp.Views.watchOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __DESKTOP__
namespace SkiaSharp.Views.Desktop
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#elif __TIZEN__
namespace SkiaSharp.Views.Tizen
#elif WINDOWS
namespace SkiaSharp.Views.Windows
#endif
{
	public static class Extensions
	{
		private static readonly Lazy<bool> isValidEnvironment = new Lazy<bool>(() =>
		{
			try
			{
				// test an operation that requires the native library
				SKPMColor.PreMultiply(SKColors.Black);
				return true;
			}
			catch (DllNotFoundException)
			{
				// If we can't load the native library,
				// we may be in some designer.
				// We can make this assumption since any other member will fail
				// at some point in the draw operation.
				return false;
			}
		});

		internal static bool IsValidEnvironment => isValidEnvironment.Value;

#if !WINDOWS_UWP && !__TIZEN__
		// System.Drawing.Point*

		public static SKPoint ToSKPoint(this System.Drawing.PointF point)
		{
			return new SKPoint(point.X, point.Y);
		}

		public static SKPointI ToSKPoint(this System.Drawing.Point point)
		{
			return new SKPointI(point.X, point.Y);
		}

		public static System.Drawing.PointF ToDrawingPoint(this SKPoint point)
		{
			return new System.Drawing.PointF(point.X, point.Y);
		}

		public static System.Drawing.Point ToDrawingPoint(this SKPointI point)
		{
			return new System.Drawing.Point(point.X, point.Y);
		}

		// System.Drawing.Rectangle*

		public static SKRect ToSKRect(this System.Drawing.RectangleF rect)
		{
			return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static SKRectI ToSKRect(this System.Drawing.Rectangle rect)
		{
			return new SKRectI(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static System.Drawing.RectangleF ToDrawingRect(this SKRect rect)
		{
			return System.Drawing.RectangleF.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		public static System.Drawing.Rectangle ToDrawingRect(this SKRectI rect)
		{
			return System.Drawing.Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// System.Drawing.Size*

		public static SKSize ToSKSize(this System.Drawing.SizeF size)
		{
			return new SKSize(size.Width, size.Height);
		}

		public static SKSizeI ToSKSize(this System.Drawing.Size size)
		{
			return new SKSizeI(size.Width, size.Height);
		}

		public static System.Drawing.SizeF ToDrawingSize(this SKSize size)
		{
			return new System.Drawing.SizeF(size.Width, size.Height);
		}

		public static System.Drawing.Size ToDrawingSize(this SKSizeI size)
		{
			return new System.Drawing.Size(size.Width, size.Height);
		}

#if __DESKTOP__

		// System.Drawing.Bitmap

		public static System.Drawing.Bitmap ToBitmap(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image.ToBitmap();
			}
		}

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

		public static System.Drawing.Bitmap ToBitmap(this SKPixmap pixmap)
		{
			using (var image = SKImage.FromPixels(pixmap))
			{
				return image.ToBitmap();
			}
		}

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
#endif

#if __ANDROID__ || __DESKTOP__
		// System.Drawing.Color

		public static SKColor ToSKColor(this System.Drawing.Color color)
		{
			return (SKColor)(uint)color.ToArgb();
		}

		public static System.Drawing.Color ToDrawingColor(this SKColor color)
		{
			return System.Drawing.Color.FromArgb((int)(uint)color);
		}
#endif

#endif
	}
}
