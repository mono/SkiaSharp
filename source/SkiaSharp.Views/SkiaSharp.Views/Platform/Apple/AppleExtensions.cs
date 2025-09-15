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
	/// <summary>
	/// Various extension methods to convert between SkiaSharp types and CoreGraphics types.
	/// </summary>
	public static class AppleExtensions
	{
		// CGPoint

		/// <summary>
		/// Converts a CoreGraphics point into a SkiaSharp point.
		/// </summary>
		/// <param name="point">The CoreGraphics point.</param>
		/// <returns>Returns a SkiaSharp point.</returns>
		public static SKPoint ToSKPoint(this CGPoint point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		/// <summary>
		/// Converts a SkiaSharp point into a CoreGraphics point.
		/// </summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a CoreGraphics point.</returns>
		public static CGPoint ToPoint(this SKPoint point)
		{
			return new CGPoint(point.X, point.Y);
		}

		// CGRect

		/// <summary>
		/// Converts a CoreGraphics rectangle into a SkiaSharp rectangle.
		/// </summary>
		/// <param name="rect">The CoreGraphics rectangle.</param>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		public static SKRect ToSKRect(this CGRect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		/// <summary>
		/// Converts a SkiaSharp rectangle into a CoreGraphics rectangle.
		/// </summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a CoreGraphics rectangle.</returns>
		public static CGRect ToRect(this SKRect rect)
		{
			return new CGRect(rect.Left, rect.Top, rect.Width, rect.Height);
		}

		// CGSize

		/// <summary>
		/// Converts a CoreGraphics size into a SkiaSharp size.
		/// </summary>
		/// <param name="size">The CoreGraphics size.</param>
		/// <returns>Returns a SkiaSharp size.</returns>
		public static SKSize ToSKSize(this CGSize size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		/// <summary>
		/// Converts a SkiaSharp size into a CoreGraphics size.
		/// </summary>
		/// <param name="size">The SkiaSharp size.</param>
		/// <returns>Returns a CoreGraphics size.</returns>
		public static CGSize ToSize(this SKSize size)
		{
			return new CGSize(size.Width, size.Height);
		}

		// CGColor

		/// <summary>
		/// Converts a CoreGraphics color into a SkiaSharp color.
		/// </summary>
		/// <param name="color">The CoreGraphics color.</param>
		/// <returns>Returns a SkiaSharp color.</returns>
		public static SKColor ToSKColor(this CGColor color) =>
			UIColor.FromCGColor(color).ToSKColor();

		public static SKColorF ToSKColorF(this CGColor color) =>
			UIColor.FromCGColor(color).ToSKColorF();

		/// <summary>
		/// Converts a SkiaSharp color into a CoreGraphics color.
		/// </summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Returns a CoreGraphics color.</returns>
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

		/// <summary>
		/// Converts a CoreImage color into a SkiaSharp color.
		/// </summary>
		/// <param name="color">The CoreImage color.</param>
		/// <returns>Returns a SkiaSharp color.</returns>
		public static SKColor ToSKColor(this CIColor color) =>
			UIColor.FromCIColor(color).ToSKColor();

		public static SKColorF ToSKColorF(this CIColor color) =>
			UIColor.FromCIColor(color).ToSKColorF();

		/// <summary>
		/// Converts a SkiaSharp color into a CoreImage color.
		/// </summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Returns a CoreImage color.</returns>
		public static CIColor ToCIColor(this SKColor color) =>
			new CIColor(color.Red / 255f, color.Green / 255f, color.Blue / 255f, color.Alpha / 255f);

		public static CIColor ToCIColor(this SKColorF color) =>
			new CIColor(color.Red, color.Green, color.Blue, color.Alpha);

		// CGImage

		/// <summary>
		/// Converts a CoreGraphics image into a SkiaSharp pixmap.
		/// </summary>
		/// <param name="cgImage">The CoreGraphics image.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the image data.</param>
		public static void ToSKPixmap(this CGImage cgImage, SKPixmap pixmap)
		{
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using var context = new CGBitmapContext(pixmap.GetPixels(), pixmap.Width, pixmap.Height, 8, pixmap.RowBytes, colorSpace, CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);
			CGRect rect = new CGRect(0, 0, cgImage.Width, cgImage.Height);
			context.ClearRect(rect);
			context.DrawImage(rect, cgImage);
		}

		/// <summary>
		/// Converts a CoreGraphics image into a SkiaSharp image.
		/// </summary>
		/// <param name="cgImage">The CoreGraphics image.</param>
		/// <returns>Returns a copy of the image data as a SkiaSharp image.</returns>
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

		/// <summary>
		/// Converts a CoreGraphics image into a SkiaSharp bitmap.
		/// </summary>
		/// <param name="cgImage">The CoreGraphics image.</param>
		/// <returns>Returns a copy of the image data as a SkiaSharp bitmap.</returns>
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

		/// <summary>
		/// Converts a SkiaSharp picture into a CoreGraphics image.
		/// </summary>
		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <returns>Returns a copy of the picture as a CoreGraphics image.</returns>
		public static CGImage ToCGImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var img = SKImage.FromPicture(skiaPicture, dimensions);
			return img.ToCGImage();
		}

		/// <summary>
		/// Converts a SkiaSharp image into a CoreGraphics image.
		/// </summary>
		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <returns>Returns a copy of the image data as a CoreGraphics image.</returns>
		public static CGImage ToCGImage(this SKImage skiaImage)
		{
			var bmp = SKBitmap.FromImage(skiaImage);
			return bmp.ToCGImage();
		}

		/// <summary>
		/// Converts a SkiaSharp pixmap into a CoreGraphics image.
		/// </summary>
		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <returns>Returns a copy of the pixel data as a CoreGraphics image.</returns>
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

		/// <summary>
		/// Converts a SkiaSharp bitmap into a CoreGraphics image.
		/// </summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a CoreGraphics image.</returns>
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

		/// <summary>
		/// Converts a CoreImage image into a SkiaSharp pixmap.
		/// </summary>
		/// <param name="ciImage">The CoreImage image.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the image data.</param>
		public static void ToSKPixmap(this CIImage ciImage, SKPixmap pixmap)
		{
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			using var context = new CIContext(null);
			context.RenderToBitmap(ciImage, pixmap.GetPixels(), pixmap.RowBytes, ciImage.Extent, (int)CIFormat.kRGBA8, colorSpace);
		}

		/// <summary>
		/// Converts a CoreImage image into a SkiaSharp image.
		/// </summary>
		/// <param name="ciImage">The CoreImage image.</param>
		/// <returns>Returns a copy of the image data as a SkiaSharp image.</returns>
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

		/// <summary>
		/// Converts a CoreImage image into a SkiaSharp bitmap.
		/// </summary>
		/// <param name="ciImage">The CoreImage image.</param>
		/// <returns>Returns a copy of the image data as a SkiaSharp bitmap.</returns>
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

		/// <summary>
		/// Converts a SkiaSharp picture into a CoreImage image.
		/// </summary>
		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <returns>Returns a copy of the picture as a CoreImage image.</returns>
		public static CIImage ToCIImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			return skiaPicture.ToCGImage(dimensions);
		}

		/// <summary>
		/// Converts a SkiaSharp image into a CoreImage image.
		/// </summary>
		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <returns>Returns a copy of the image data as a CoreImage image.</returns>
		public static CIImage ToCIImage(this SKImage skiaImage)
		{
			return skiaImage.ToCGImage();
		}

		/// <summary>
		/// Converts a SkiaSharp pixmap into a CoreImage image.
		/// </summary>
		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <returns>Returns a copy of the pixel data as a CoreImage image.</returns>
		public static CIImage ToCIImage(this SKPixmap skiaPixmap)
		{
			return skiaPixmap.ToCGImage();
		}

		/// <summary>
		/// Converts a SkiaSharp bitmap into a CoreImage image.
		/// </summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a CoreImage image.</returns>
		public static CIImage ToCIImage(this SKBitmap skiaBitmap)
		{
			return skiaBitmap.ToCGImage();
		}

		// NSData

		/// <summary>
		/// Converts a SkiaSharp data object into a NSData.
		/// </summary>
		/// <param name="skiaData">The SkiaSharp data object.</param>
		/// <returns>Returns a copy of the data as a NSData.</returns>
		public static NSData ToNSData(this SKData skiaData)
		{
			return NSData.FromBytes(skiaData.Data, (nuint)skiaData.Size);
		}

		/// <summary>
		/// Converts a NSData into a SkiaSharp data object.
		/// </summary>
		/// <param name="nsData">The NSData.</param>
		/// <returns>Returns a copy of the data as a SkiaSharp data object.</returns>
		public static SKData ToSKData(this NSData nsData)
		{
			return SKData.CreateCopy(nsData.Bytes, nsData.Length);
		}

	}
}
