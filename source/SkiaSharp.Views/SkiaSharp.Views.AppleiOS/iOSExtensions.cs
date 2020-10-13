using System;
using UIKit;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __WATCHOS__
namespace SkiaSharp.Views.watchOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	public static class iOSExtensions
	{
		// UIColor

		public static SKColor ToSKColor(this UIColor color)
		{
			System.nfloat r, g, b, a;
			color.GetRGBA(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		public static SKColorF ToSKColorF(this UIColor color)
		{
			System.nfloat r, g, b, a;
			color.GetRGBA(out r, out g, out b, out a);
			return new SKColorF((float)r, (float)g, (float)b, (float)a);
		}

		public static UIColor ToUIColor(this SKColor color) =>
			UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha);

		public static UIColor ToUIColor(this SKColorF color) =>
			UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha);

		// UIImage

		public static SKImage ToSKImage(this UIImage uiImage)
		{
			var cgImage = uiImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKImage();
			}
#if !__WATCHOS__
			var ciImage = uiImage.CIImage;
			if (ciImage != null)
			{
				return ciImage.ToSKImage();
			}
#endif
			return null;
		}

		public static SKBitmap ToSKBitmap(this UIImage uiImage)
		{
			var cgImage = uiImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKBitmap();
			}
#if !__WATCHOS__
			var ciImage = uiImage.CIImage;
			if (ciImage != null)
			{
				return ciImage.ToSKBitmap();
			}
#endif
			return null;
		}

		public static bool ToSKPixmap(this UIImage uiImage, SKPixmap pixmap)
		{
			var cgImage = uiImage.CGImage;
			if (cgImage != null)
			{
				cgImage.ToSKPixmap(pixmap);
				return true;
			}
#if !__WATCHOS__
			var ciImage = uiImage.CIImage;
			if (ciImage != null)
			{
				ciImage.ToSKPixmap(pixmap);
				return true;
			}
#endif
			return false;
		}

		public static UIImage ToUIImage(this SKPicture skiaPicture, SKSizeI dimensions, nfloat scale, UIImageOrientation orientation)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new UIImage(cgImage, scale, orientation);
		}

		public static UIImage ToUIImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new UIImage(cgImage);
		}

		public static UIImage ToUIImage(this SKImage skiaImage)
		{
			var cgImage = skiaImage.ToCGImage();
			return new UIImage(cgImage);
		}

		public static UIImage ToUIImage(this SKPixmap skiaPixmap, nfloat scale, UIImageOrientation orientation)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new UIImage(cgImage, scale, orientation);
		}

		public static UIImage ToUIImage(this SKPixmap skiaPixmap)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new UIImage(cgImage);
		}

		public static UIImage ToUIImage(this SKBitmap skiaBitmap, nfloat scale, UIImageOrientation orientation)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new UIImage(cgImage, scale, orientation);
		}

		public static UIImage ToUIImage(this SKBitmap skiaBitmap)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new UIImage(cgImage);
		}

	}
}
