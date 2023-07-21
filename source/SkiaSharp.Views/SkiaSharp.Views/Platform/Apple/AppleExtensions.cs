using System;
using CoreGraphics;
using Foundation;

using CoreImage;

#if __MACOS__
using UIColor = AppKit.NSColor;
#else
using UIKit;
#endif

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#elif __MACOS__
namespace SkiaSharp.Views.Mac
#endif
{
	public static class AppleExtensions
	{
		// CGPoint

		public static SKPoint ToSKPoint(this CGPoint point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		public static CGPoint ToPoint(this SKPoint point)
		{
			return new CGPoint(point.X, point.Y);
		}

		// CGRect

		public static SKRect ToSKRect(this CGRect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		public static CGRect ToRect(this SKRect rect)
		{
			return new CGRect(rect.Left, rect.Top, rect.Width, rect.Height);
		}

		// CGSize

		public static SKSize ToSKSize(this CGSize size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		public static CGSize ToSize(this SKSize size)
		{
			return new CGSize(size.Width, size.Height);
		}

		// CGColor

		public static SKColor ToSKColor(this CGColor color) =>
			UIColor.FromCGColor(color).ToSKColor();

		public static SKColorF ToSKColorF(this CGColor color) =>
			UIColor.FromCGColor(color).ToSKColorF();

		public static CGColor ToCGColor(this SKColor color)
		{
#if __TVOS__ || __IOS__
			// see https://bugzilla.xamarin.com/show_bug.cgi?id=44507
			return UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha).CGColor;
#else
			return UIColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha).CGColor;
#endif
		}

		public static CGColor ToCGColor(this SKColorF color)
		{
#if __TVOS__ || __IOS__
			// see https://bugzilla.xamarin.com/show_bug.cgi?id=44507
			return UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha).CGColor;
#else
			return UIColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha).CGColor;
#endif
		}

		// CIColor

		public static SKColor ToSKColor(this CIColor color) =>
			UIColor.FromCIColor(color).ToSKColor();

		public static SKColorF ToSKColorF(this CIColor color) =>
			UIColor.FromCIColor(color).ToSKColorF();

		public static CIColor ToCIColor(this SKColor color) =>
			new CIColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);

		public static CIColor ToCIColor(this SKColorF color) =>
			new CIColor(color.Red, color.Green, color.Blue, color.Alpha);

		// CGImage

		public static void ToSKPixmap(this CGImage cgImage, SKPixmap pixmap)
		{
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using var context = new CGBitmapContext(pixmap.GetPixels(), pixmap.Width, pixmap.Height, 8, pixmap.RowBytes, colorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);
			CGRect rect = new CGRect(0, 0, cgImage.Width, cgImage.Height);
			context.ClearRect(rect);
			context.DrawImage(rect, cgImage);
		}

		public static SKImage ToSKImage(this CGImage cgImage)
		{
			var info = new SKImageInfo((int)cgImage.Width, (int)cgImage.Height);
			var image = SKImage.Create(info);
			using (var pixmap = image.PeekPixels())
			{
				cgImage.ToSKPixmap(pixmap);
			}
			return image;
		}

		public static SKBitmap ToSKBitmap(this CGImage cgImage)
		{
			var info = new SKImageInfo((int)cgImage.Width, (int)cgImage.Height);
			var bitmap = new SKBitmap(info);
			using (var pixmap = bitmap.PeekPixels())
			{
				cgImage.ToSKPixmap(pixmap);
			}
			return bitmap;
		}

		public static CGImage ToCGImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var img = SKImage.FromPicture(skiaPicture, dimensions);
			return img.ToCGImage();
		}

		public static CGImage ToCGImage(this SKImage skiaImage)
		{
			var bmp = SKBitmap.FromImage(skiaImage);
			return bmp.ToCGImage();
		}

		public static CGImage ToCGImage(this SKPixmap skiaPixmap)
		{
			var info = skiaPixmap.Info;

			CGImage cgImage;
			using (var provider = new CGDataProvider(skiaPixmap.GetPixels(), info.BytesSize))
			using (var colorSpace = CGColorSpace.CreateDeviceRGB())
			{
				cgImage = new CGImage(
					info.Width, info.Height,
					8, info.BitsPerPixel, info.RowBytes,
					colorSpace,
					CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big,
					provider,
					null, false, CGColorRenderingIntent.Default);
			}
			return cgImage;
		}

		public static CGImage ToCGImage(this SKBitmap skiaBitmap)
		{
			var info = skiaBitmap.Info;

			CGImage cgImage;
			IntPtr size;
			using (var provider = new CGDataProvider(skiaBitmap.GetPixels(out size), size.ToInt32()))
			using (var colorSpace = CGColorSpace.CreateDeviceRGB())
			{
				cgImage = new CGImage(
					info.Width, info.Height,
					8, info.BitsPerPixel, info.RowBytes,
					colorSpace,
					CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big,
					provider,
					null, false, CGColorRenderingIntent.Default);
			}
			GC.KeepAlive(skiaBitmap);
			return cgImage;
		}

		// CIImage

		public static void ToSKPixmap(this CIImage ciImage, SKPixmap pixmap)
		{
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using var context = new CIContext(null);
			context.RenderToBitmap(ciImage, pixmap.GetPixels(), pixmap.RowBytes, ciImage.Extent, (int)CIFormat.kRGBA8, colorSpace);
		}

		public static SKImage ToSKImage(this CIImage ciImage)
		{
			var extent = ciImage.Extent;
			var info = new SKImageInfo((int)extent.Width, (int)extent.Height);
			var image = SKImage.Create(info);
			using (var pixmap = image.PeekPixels())
			{
				ciImage.ToSKPixmap(pixmap);
			}
			return image;
		}

		public static SKBitmap ToSKBitmap(this CIImage ciImage)
		{
			var extent = ciImage.Extent;
			var info = new SKImageInfo((int)extent.Width, (int)extent.Height);
			var image = new SKBitmap(info);
			using (var pixmap = image.PeekPixels())
			{
				ciImage.ToSKPixmap(image.PeekPixels());
			}
			return image;
		}

		public static CIImage ToCIImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			return skiaPicture.ToCGImage(dimensions);
		}

		public static CIImage ToCIImage(this SKImage skiaImage)
		{
			return skiaImage.ToCGImage();
		}

		public static CIImage ToCIImage(this SKPixmap skiaPixmap)
		{
			return skiaPixmap.ToCGImage();
		}

		public static CIImage ToCIImage(this SKBitmap skiaBitmap)
		{
			return skiaBitmap.ToCGImage();
		}

		// NSData

		public static NSData ToNSData(this SKData skiaData)
		{
			return NSData.FromBytes(skiaData.Data, (nuint)skiaData.Size);
		}

		public static SKData ToSKData(this NSData nsData)
		{
			return SKData.CreateCopy(nsData.Bytes, nsData.Length);
		}

	}
}
