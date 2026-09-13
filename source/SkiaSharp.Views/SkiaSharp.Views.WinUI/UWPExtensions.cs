using System;
using Windows.Foundation;
using Windows.UI;
using Windows.Storage.Streams;
#if !HAS_UNO
using SkiaSharp.Views.WinUI.Native;
#endif

#if WINDOWS || WINUI
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml.Media.Imaging;
#endif

#if WINDOWS || WINUI
namespace SkiaSharp.Views.Windows
#else
namespace SkiaSharp.Views.UWP
#endif
{
#if WINDOWS
	/// <summary>Various extension methods to convert between SkiaSharp types and Windows types.</summary>
	/// <remarks />
	public static class WindowsExtensions
#else
	/// <summary>Provides extension methods for converting between SkiaSharp types and UWP types.</summary>
	/// <remarks />
	public static class UWPExtensions
#endif
	{
		// Point

		/// <summary>Converts a Windows point into a SkiaSharp point.</summary>
		/// <param name="point">The Windows point.</param>
		/// <returns>Returns a SkiaSharp point.</returns>
		/// <remarks />
		public static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint((float)point.X, (float)point.Y);
		}

		/// <summary>Converts a SkiaSharp point into a Windows point.</summary>
		/// <param name="point">The SkiaSharp point.</param>
		/// <returns>Returns a Windows point.</returns>
		/// <remarks />
		public static Point ToPoint(this SKPoint point)
		{
			return new Point(point.X, point.Y);
		}

		// Rect

		/// <summary>Converts a Windows rectangle into a SkiaSharp rectangle.</summary>
		/// <param name="rect">The Windows rectangle.</param>
		/// <returns>Returns a SkiaSharp rectangle.</returns>
		/// <remarks />
		public static SKRect ToSKRect(this Rect rect)
		{
			return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
		}

		/// <summary>Converts a SkiaSharp rectangle into a Windows rectangle.</summary>
		/// <param name="rect">The SkiaSharp rectangle.</param>
		/// <returns>Returns a Windows rectangle.</returns>
		/// <remarks />
		public static Rect ToRect(this SKRect rect)
		{
			return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
		}

		// Size

		/// <summary>Converts a Windows size into a SkiaSharp size.</summary>
		/// <param name="size">The Windows size.</param>
		/// <returns>Returns a SkiaSharp size.</returns>
		/// <remarks />
		public static SKSize ToSKSize(this Size size)
		{
			return new SKSize((float)size.Width, (float)size.Height);
		}

		/// <summary>Converts a SkiaSharp size into a Windows size.</summary>
		/// <param name="size">The SkiaSharp size.</param>
		/// <returns>Returns a Windows size.</returns>
		/// <remarks />
		public static Size ToSize(this SKSize size)
		{
			return new Size(size.Width, size.Height);
		}

		// Color

		/// <summary>Converts a Windows color into a SkiaSharp color.</summary>
		/// <param name="color">The Windows color.</param>
		/// <returns>Returns a SkiaSharp color.</returns>
		/// <remarks />
		public static SKColor ToSKColor(this Color color)
		{
			return new SKColor(color.R, color.G, color.B, color.A);
		}

		/// <summary>Converts a SkiaSharp color into a Windows color.</summary>
		/// <param name="color">The SkiaSharp color.</param>
		/// <returns>Returns a Windows color.</returns>
		/// <remarks />
		public static Color ToColor(this SKColor color)
		{
			return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
		}

#if !HAS_UNO
		// WriteableBitmap

		/// <summary>Converts a SkiaSharp picture into a Windows WriteableBitmap.</summary>
		/// <param name="picture">The SkiaSharp picture.</param>
		/// <param name="dimensions">The dimensions of the picture.</param>
		/// <returns>Returns a copy of the picture as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKPicture picture, SKSizeI dimensions)
		{
			using (var image = SKImage.FromPicture(picture, dimensions))
			{
				return image?.ToWriteableBitmap();
			}
		}

		/// <summary>Converts a SkiaSharp image into a Windows WriteableBitmap.</summary>
		/// <param name="skiaImage">The SkiaSharp image.</param>
		/// <returns>Returns a copy of the image data as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKImage skiaImage)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			// TODO: remove this as it is old/default logic
			//using (var tempImage = SKImage.Create(info))
			//using (var pixmap = tempImage.PeekPixels())
			//using (var data = SKData.Create(pixmap.GetPixels(), info.BytesSize))
			//{
			//	skiaImage.ReadPixels(pixmap, 0, 0);
			//	using (var stream = bitmap.PixelBuffer.AsStream())
			//	{
			//		data.SaveTo(stream);
			//	}
			//}

			var info = new SKImageInfo(skiaImage.Width, skiaImage.Height);
			var bitmap = new WriteableBitmap(info.Width, info.Height);
			using (var pixmap = new SKPixmap(info, bitmap.GetPixels()))
			{
				skiaImage.ReadPixels(pixmap, 0, 0);
			}
			bitmap.Invalidate();
			return bitmap;
		}

		/// <summary>Converts a SkiaSharp bitmap into a Windows WriteableBitmap.</summary>
		/// <param name="skiaBitmap">The SkiaSharp bitmap.</param>
		/// <returns>Returns a copy of the bitmap data as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKBitmap skiaBitmap)
		{
			using var pixmap = skiaBitmap.PeekPixels();
			using var image = SKImage.FromPixels(pixmap);
			var wb = image.ToWriteableBitmap();
			GC.KeepAlive(skiaBitmap);
			return wb;
		}

		/// <summary>Converts a SkiaSharp pixmap into a Windows WriteableBitmap.</summary>
		/// <param name="pixmap">The SkiaSharp pixmap.</param>
		/// <returns>Returns a copy of the pixel data as a Windows WriteableBitmap.</returns>
		/// <remarks />
		public static WriteableBitmap ToWriteableBitmap(this SKPixmap pixmap)
		{
			using (var image = SKImage.FromPixels(pixmap))
			{
				return image.ToWriteableBitmap();
			}
		}

		/// <summary>Converts a Windows WriteableBitmap into a SkiaSharp bitmap.</summary>
		/// <param name="bitmap">The Windows WriteableBitmap to convert.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKBitmap" /> containing a copy of the bitmap data.</returns>
		/// <remarks />
		public static SKBitmap ToSKBitmap(this WriteableBitmap bitmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight);
			var skiaBitmap = new SKBitmap(info);
			using (var pixmap = skiaBitmap.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
			}
			return skiaBitmap;
		}

		/// <summary>Converts a Windows WriteableBitmap into a SkiaSharp image.</summary>
		/// <param name="bitmap">The Windows WriteableBitmap to convert.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKImage" /> containing a copy of the image data.</returns>
		/// <remarks />
		public static SKImage ToSKImage(this WriteableBitmap bitmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			var info = new SKImageInfo(bitmap.PixelWidth, bitmap.PixelHeight);
			var image = SKImage.Create(info);
			using (var pixmap = image.PeekPixels())
			{
				bitmap.ToSKPixmap(pixmap);
			}
			return image;
		}

		/// <summary>Copies the pixel data from a Windows WriteableBitmap into an existing SkiaSharp pixmap.</summary>
		/// <param name="bitmap">The Windows WriteableBitmap to convert.</param>
		/// <param name="pixmap">The destination pixmap to copy the pixel data into.</param>
		/// <returns><see langword="true" /> if the copy was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool ToSKPixmap(this WriteableBitmap bitmap, SKPixmap pixmap)
		{
			// TODO: maybe keep the same color types where we can, instead of just going to the platform default

			if (pixmap.ColorType == SKImageInfo.PlatformColorType)
			{
				using var image = SKImage.FromPixels(pixmap.Info, bitmap.GetPixels());
				return image.ReadPixels(pixmap, 0, 0);
			}
			else
			{
				// we have to copy the pixels into a format that we understand
				// and then into a desired format
				// TODO: we can still do a bit more for other cases where the color types are the same
				using (var tempImage = bitmap.ToSKImage())
				{
					return tempImage.ReadPixels(pixmap, 0, 0);
				}
			}
		}

		internal static IntPtr GetPixels(this WriteableBitmap bitmap) =>
			bitmap.PixelBuffer.GetByteBuffer();

		internal static IntPtr GetByteBuffer(this IBuffer buffer) =>
			(IntPtr)BufferExtensions.GetByteBuffer(buffer);
#endif
	}
}
