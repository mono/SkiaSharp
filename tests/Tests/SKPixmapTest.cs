using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPixmapTest : SKTest
	{
		[SkippableFact]
		public void CanScalePixels()
		{
			var srcInfo = new SKImageInfo(200, 200);
			var dstInfo = new SKImageInfo(100, 100);

			var srcBmp = new SKBitmap(srcInfo);
			var dstBmp = new SKBitmap(dstInfo);

			using (var canvas = new SKCanvas(srcBmp))
			using (var paint = new SKPaint { Color = SKColors.Green })
			{
				canvas.Clear(SKColors.Blue);
				canvas.DrawRect(new SKRect(0, 0, 100, 200), paint);
			}

			Assert.Equal(SKColors.Green, srcBmp.GetPixel(75, 75));
			Assert.Equal(SKColors.Blue, srcBmp.GetPixel(175, 175));

			var srcPix = srcBmp.PeekPixels();
			var dstPix = dstBmp.PeekPixels();

			Assert.True(srcPix.ScalePixels(dstPix, SKFilterQuality.High));

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[SkippableFact]
		public void ReadPixelSucceeds()
		{
			var info = new SKImageInfo(10, 10);

			var ptr1 = Marshal.AllocCoTaskMem(info.BytesSize);
			var pix1 = new SKPixmap(info, ptr1);

			var ptr2 = Marshal.AllocCoTaskMem(info.BytesSize);

			var result = pix1.ReadPixels(info, ptr2, info.RowBytes);

			Assert.True(result);
		}
		
		[SkippableFact]
		public void WithMethodsDoNotModifySource()
		{
			var info = new SKImageInfo(100, 30, SKColorType.Rgb565, SKAlphaType.Unpremul);
			var pixmap = new SKPixmap(info, (IntPtr)123);

			Assert.Equal(SKColorType.Rgb565, pixmap.ColorType);
			Assert.Equal((IntPtr)123, pixmap.GetPixels());

			var copy = pixmap.WithColorType(SKColorType.Gray8);

			Assert.Equal(SKColorType.Rgb565, pixmap.ColorType);
			Assert.Equal((IntPtr)123, pixmap.GetPixels());
			Assert.Equal(SKColorType.Gray8, copy.ColorType);
			Assert.Equal((IntPtr)123, copy.GetPixels());
		}

		[SkippableFact]
		public void ReadPixelCopiesData()
		{
			var info = new SKImageInfo(10, 10);

			using (var bmp1 = new SKBitmap(info))
			using (var pix1 = bmp1.PeekPixels())
			using (var bmp2 = new SKBitmap(info))
			using (var pix2 = bmp2.PeekPixels())
			{
				bmp1.Erase(SKColors.Blue);
				bmp1.Erase(SKColors.Green);

				Assert.NotEqual(Marshal.ReadInt64(pix1.GetPixels()), Marshal.ReadInt64(pix2.GetPixels()));

				var result = pix1.ReadPixels(pix2);

				Assert.True(result);

				Assert.Equal(Marshal.ReadInt64(pix1.GetPixels()), Marshal.ReadInt64(pix2.GetPixels()));
			}
		}

		[SkippableFact]
		public void SwizzleSwapsRedAndBlue()
		{
			var info = new SKImageInfo(10, 10);

			using (var bmp = new SKBitmap(info))
			{
				bmp.Erase(SKColors.Red);

				Assert.Equal(SKColors.Red, bmp.Pixels[0]);

				SKSwizzle.SwapRedBlue(bmp.GetPixels(out var length), info.Width * info.Height);

				Assert.Equal(SKColors.Blue, bmp.Pixels[0]);
			}
		}

		[SkippableFact]
		public void EraseWithColor()
		{
			var info = new SKImageInfo(1, 1);

			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(SKColors.Red);

			Assert.Equal(SKColors.Red, pixmap.GetPixelColor(0, 0));
		}

		[SkippableFact]
		public void EraseWithColorF()
		{
			var info = new SKImageInfo(1, 1);

			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(new SKColorF(1, 0, 0));

			Assert.Equal(SKColors.Red, pixmap.GetPixelColor(0, 0));
		}

		[SkippableFact]
		public void EncodeWithPngEncoder()
		{
			var bitmap = CreateTestBitmap();
			var pixmap = bitmap.PeekPixels();

			var data = pixmap.Encode(SKPngEncoderOptions.Default);

			Assert.NotNull(data);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Png, codec.EncodedFormat);
		}

		[SkippableFact]
		public void EncodeWithJpegEncoder()
		{
			var bitmap = CreateTestBitmap();
			var pixmap = bitmap.PeekPixels();

			var data = pixmap.Encode(SKJpegEncoderOptions.Default);

			Assert.NotNull(data);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);
		}

		[SkippableFact]
		public void EncodeWithWebpEncoder()
		{
			var bitmap = CreateTestBitmap();
			var pixmap = bitmap.PeekPixels();

			var data = pixmap.Encode(SKWebpEncoderOptions.Default);

			Assert.NotNull(data);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		}
	}
}
