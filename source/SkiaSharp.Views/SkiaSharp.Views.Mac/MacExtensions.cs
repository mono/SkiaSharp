using System;
using AppKit;
using CoreGraphics;
using ObjCRuntime;

namespace SkiaSharp.Views.Mac
{
	public static class MacExtensions
	{
		// NSColor

		public static SKColor ToSKColor(this NSColor color)
		{
			nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		public static SKColorF ToSKColorF(this NSColor color)
		{
			nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColorF((float)r, (float)g, (float)b, (float)a);
		}

		public static NSColor ToNSColor(this SKColor color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);

		public static NSColor ToNSColor(this SKColorF color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);


		// NSImage

		public static SKImage ToSKImage(this NSImage nsImage)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKImage();
			}
			return null;
		}

		public static SKBitmap ToSKBitmap(this NSImage nsImage)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKBitmap();
			}
			return null;
		}

		public static bool ToSKPixmap(this NSImage nsImage, SKPixmap pixmap)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				cgImage.ToSKPixmap(pixmap);
				return true;
			}
			return false;
		}

		public static NSImage ToNSImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new NSImage(cgImage, CGSize.Empty);
		}

		public static NSImage ToNSImage(this SKImage skiaImage)
		{
			var cgImage = skiaImage.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

		public static NSImage ToNSImage(this SKPixmap skiaPixmap)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

		public static NSImage ToNSImage(this SKBitmap skiaBitmap)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

	}
}
