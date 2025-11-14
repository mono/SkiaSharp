using System;
using AppKit;
using CoreGraphics;
using ObjCRuntime;

namespace SkiaSharp.Views.Mac
{
	/// <summary>
	/// Various extension methods to convert between SkiaSharp types and AppKit types.
	/// </summary>
	public static class MacExtensions
	{
		// NSColor

		/// <summary>
		/// Converts a AppKit color into an SkiaSharp color.
		/// </summary>
		/// <param name="color">The AppKit color.</param>
		/// <returns>Returns a SkiaSharp color.</returns>
		public static SKColor ToSKColor(this NSColor color)
		{
			nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		/// <param name="color"></param>
		public static SKColorF ToSKColorF(this NSColor color)
		{
			nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColorF((float)r, (float)g, (float)b, (float)a);
		}

		/// <summary>
		/// Converts a SkiaSharp color into an AppKit color.
		/// </summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Return an AppKit color.</returns>
		public static NSColor ToNSColor(this SKColor color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);

		/// <param name="color"></param>
		public static NSColor ToNSColor(this SKColorF color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);


		// NSImage

		/// <summary>
		/// Converts a AppKit image into a SkiaSharp image.
		/// </summary>
		/// <param name="nsImage">The AppKit image.</param>
		/// <returns>Returns a copy of the image data as a SkiaSharp image.</returns>
		public static SKImage ToSKImage(this NSImage nsImage)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKImage();
			}
			return null;
		}

		/// <summary>
		/// Converts a AppKit image into a SkiaSharp bitmap.
		/// </summary>
		/// <param name="nsImage">The AppKit image.</param>
		/// <returns>Returns a copy of the image data as a SkiaSharp bitmap.</returns>
		public static SKBitmap ToSKBitmap(this NSImage nsImage)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKBitmap();
			}
			return null;
		}

		/// <summary>
		/// Converts a SkiaSharp pixmap into a AppKit image.
		/// </summary>
		/// <param name="nsImage">The AppKit image.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the image data.</param>
		/// <returns>Returns <see langword="true" /> if the copy was successful, otherwise <see langword="false" />.</returns>
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

		/// <summary>
		/// Converts a SkiaSharp picture into a AppKit image.
		/// </summary>
		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <returns>Returns a copy of the picture as a AppKit image.</returns>
		public static NSImage ToNSImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new NSImage(cgImage, CGSize.Empty);
		}

		/// <summary>
		/// Converts a SkiaSharp image into a AppKit image.
		/// </summary>
		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <returns>Returns a copy of the image data as a AppKit image.</returns>
		public static NSImage ToNSImage(this SKImage skiaImage)
		{
			var cgImage = skiaImage.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

		/// <summary>
		/// Converts a SkiaSharp pixmap into a AppKit image.
		/// </summary>
		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <returns>Returns a copy of the pixel data as a AppKit image.</returns>
		public static NSImage ToNSImage(this SKPixmap skiaPixmap)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

		/// <summary>
		/// Converts a SkiaSharp bitmap into a AppKit image.
		/// </summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a AppKit image.</returns>
		public static NSImage ToNSImage(this SKBitmap skiaBitmap)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

	}
}
