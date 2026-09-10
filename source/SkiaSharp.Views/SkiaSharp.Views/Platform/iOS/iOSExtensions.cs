using System;
using UIKit;
using ObjCRuntime;

#if __TVOS__
namespace SkiaSharp.Views.tvOS
#elif __IOS__
namespace SkiaSharp.Views.iOS
#endif
{
	/// <summary>Various extension methods to convert between SkiaSharp types and UIKit types.</summary>
	/// <remarks />
	public static class iOSExtensions
	{
		// UIColor

		/// <param name="color">The UIKit color.</param>
		/// <summary>Converts a UIKit color into a SkiaSharp color.</summary>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this UIColor color)
		{
			nfloat r, g, b, a;
			color.GetRGBA(out r, out g, out b, out a);
			return new SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		/// <param name="color">The UIKit color.</param>
		/// <summary>Converts a UIKit color into a SkiaSharp floating-point color.</summary>
		/// <returns>Returns a SkiaSharp floating-point color.</returns>
		/// <remarks />
		public static SKColorF ToSKColorF(this UIColor color)
		{
			nfloat r, g, b, a;
			color.GetRGBA(out r, out g, out b, out a);
			return new SKColorF((float)r, (float)g, (float)b, (float)a);
		}

		/// <param name="color">The SkiaSharp color.</param>
		/// <summary>Converts a SkiaSharp color into a UIKit color.</summary>
		/// <returns>Returns a UIKit color.</returns>
		/// <remarks />
		public static UIColor ToUIColor(this SKColor color) =>
			UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha);

		/// <param name="color">The SkiaSharp floating-point color.</param>
		/// <summary>Converts a SkiaSharp floating-point color into a UIKit color.</summary>
		/// <returns>Returns a UIKit color.</returns>
		/// <remarks />
		public static UIColor ToUIColor(this SKColorF color) =>
			UIColor.FromRGBA(color.Red, color.Green, color.Blue, color.Alpha);

		// UIImage

		/// <param name="uiImage">The UIKit image.</param>
		/// <summary>Converts a UIKit image into a SkiaSharp image.</summary>
		/// <returns>Returns a copy of the image data as a SkiaSharp image.</returns>
		/// <remarks />
		public static SKImage ToSKImage(this UIImage uiImage)
		{
			var cgImage = uiImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKImage();
			}
			var ciImage = uiImage.CIImage;
			if (ciImage != null)
			{
				return ciImage.ToSKImage();
			}
			return null;
		}

		/// <param name="uiImage">The UIKit image.</param>
		/// <summary>Converts a UIKit image into a SkiaSharp bitmap.</summary>
		/// <returns>Returns a copy of the image data as a SkiaSharp bitmap.</returns>
		/// <remarks />
		public static SKBitmap ToSKBitmap(this UIImage uiImage)
		{
			var cgImage = uiImage.CGImage;
			if (cgImage != null)
			{
				return cgImage.ToSKBitmap();
			}
			var ciImage = uiImage.CIImage;
			if (ciImage != null)
			{
				return ciImage.ToSKBitmap();
			}
			return null;
		}

		/// <param name="uiImage">The UIKit image.</param>
		/// <param name="pixmap">The SkiaSharp pixmap to hold the copy of the image data.</param>
		/// <summary>Converts a SkiaSharp pixmap into a UIKit image.</summary>
		/// <returns>Returns <see langword="true" /> if the copy was successful, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool ToSKPixmap(this UIImage uiImage, SKPixmap pixmap)
		{
			var cgImage = uiImage.CGImage;
			if (cgImage != null)
			{
				cgImage.ToSKPixmap(pixmap);
				return true;
			}
			var ciImage = uiImage.CIImage;
			if (ciImage != null)
			{
				ciImage.ToSKPixmap(pixmap);
				return true;
			}
			return false;
		}

		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <param name="scale">The scale factor for the image. A factor of 1.0 is the original size of the image.</param>
		/// <param name="orientation">The rotation that is to be applied to the image.</param>
		/// <summary>Converts a SkiaSharp picture into a UIKit image.</summary>
		/// <returns>Returns a copy of the picture as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKPicture skiaPicture, SKSizeI dimensions, nfloat scale, UIImageOrientation orientation)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new UIImage(cgImage, scale, orientation);
		}

		/// <param name="skiaPicture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <summary>Converts a SkiaSharp picture into a UIKit image.</summary>
		/// <returns>Returns a copy of the picture as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKPicture skiaPicture, SKSizeI dimensions)
		{
			var cgImage = skiaPicture.ToCGImage(dimensions);
			return new UIImage(cgImage);
		}

		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <summary>Converts a SkiaSharp image into a UIKit image.</summary>
		/// <returns>Returns a copy of the image data as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKImage skiaImage)
		{
			var cgImage = skiaImage.ToCGImage();
			return new UIImage(cgImage);
		}

		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <param name="scale">The scale factor for the image. A factor of 1.0 is the original size of the image.</param>
		/// <param name="orientation">The rotation that is to be applied to the image.</param>
		/// <summary>Converts a SkiaSharp pixmap into a UIKit image.</summary>
		/// <returns>Returns a copy of the pixel data as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKPixmap skiaPixmap, nfloat scale, UIImageOrientation orientation)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new UIImage(cgImage, scale, orientation);
		}

		/// <param name="skiaPixmap">The SkiaSharp pixmap.</param>
		/// <summary>Converts a SkiaSharp pixmap into a UIKit image.</summary>
		/// <returns>Returns a copy of the pixel data as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKPixmap skiaPixmap)
		{
			var cgImage = skiaPixmap.ToCGImage();
			return new UIImage(cgImage);
		}

		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <param name="scale">The scale factor for the image. A factor of 1.0 is the original size of the image.</param>
		/// <param name="orientation">The rotation that is to be applied to the image.</param>
		/// <summary>Converts a SkiaSharp bitmap into a UIKit image.</summary>
		/// <returns>Returns a copy of the bitmap data as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKBitmap skiaBitmap, nfloat scale, UIImageOrientation orientation)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new UIImage(cgImage, scale, orientation);
		}

		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <summary>Converts a SkiaSharp bitmap into a UIKit image.</summary>
		/// <returns>Returns a copy of the bitmap data as a UIKit image.</returns>
		/// <remarks />
		public static UIImage ToUIImage(this SKBitmap skiaBitmap)
		{
			var cgImage = skiaBitmap.ToCGImage();
			return new UIImage(cgImage);
		}

	}
}
