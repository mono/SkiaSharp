using System;
using AppKit;
using CoreGraphics;
using ObjCRuntime;

namespace SkiaSharp.Views.Mac
{
	/// <summary>Various extension methods to convert between SkiaSharp types and AppKit types.</summary>
	/// <remarks />
	public static class MacExtensions
	{
		// NSColor

		/// <param name="color">The AppKit color.</param>
		/// <summary>Converts an AppKit color into a SkiaSharp color.</summary>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this NSColor color)
		{
			nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		/// <param name="color">The AppKit color.</param>
		/// <summary>Converts an AppKit color into a SkiaSharp color.</summary>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColorF ToSKColorF(this NSColor color)
		{
			nfloat r, g, b, a;
			color.GetRgba(out r, out g, out b, out a);
			return new SKColorF((float)r, (float)g, (float)b, (float)a);
		}

		/// <param name="color">The SkiaSharp color.</param>
		/// <summary>Converts a SkiaSharp color into an AppKit color.</summary>
		/// <returns>Returns an AppKit color.</returns>
		/// <remarks />
		public static NSColor ToNSColor(this SKColor color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);

		/// <param name="color">The SkiaSharp color.</param>
		/// <summary>Converts a SkiaSharp color into an AppKit color.</summary>
		/// <returns>Returns an AppKit color.</returns>
		/// <remarks />
		public static NSColor ToNSColor(this SKColorF color) =>
			NSColor.FromRgba(color.Red, color.Green, color.Blue, color.Alpha);


		// NSImage

		/// <param name="nsImage">The AppKit image.</param>
		/// <summary>Converts an AppKit image into a SkiaSharp image.</summary>
		/// <returns>Returns a copy of the image data as a SkiaSharp image.</returns>
		/// <remarks />
		public static SKImage ToSKImage(this NSImage nsImage)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKImage();
			}
			return null;
		}

		/// <param name="nsImage">The AppKit image.</param>
		/// <summary>Converts an AppKit image into a SkiaSharp bitmap.</summary>
		/// <returns>Returns a copy of the image data as a SkiaSharp bitmap.</returns>
		/// <remarks />
		public static SKBitmap ToSKBitmap(this NSImage nsImage)
		{
			var cgImage = nsImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKBitmap();
			}
			return null;
		}

		/// <param name="nsImage">The AppKit image.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the image data.</param>
		/// <summary>Converts an AppKit image into a SkiaSharp pixmap.</summary>
		/// <returns>Returns <see langword="true" /> if the copy was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
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

		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <summary>Converts a SkiaSharp picture into an AppKit image.</summary>
		/// <returns>Returns a copy of the picture as an AppKit image.</returns>
		/// <remarks />
		public static NSImage ToNSImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new NSImage(cgImage, CGSize.Empty);
		}

		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <summary>Converts a SkiaSharp image into an AppKit image.</summary>
		/// <returns>Returns a copy of the image data as an AppKit image.</returns>
		/// <remarks />
		public static NSImage ToNSImage(this SKImage skiaImage)
		{
			var cgImage = skiaImage.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <summary>Converts a SkiaSharp pixmap into an AppKit image.</summary>
		/// <returns>Returns a copy of the pixel data as an AppKit image.</returns>
		/// <remarks />
		public static NSImage ToNSImage(this SKPixmap skiaPixmap)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <summary>Converts a SkiaSharp bitmap into an AppKit image.</summary>
		/// <returns>Returns a copy of the bitmap data as an AppKit image.</returns>
		/// <remarks />
		public static NSImage ToNSImage(this SKBitmap skiaBitmap)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new NSImage(cgImage, CGSize.Empty);
		}

	}
}
