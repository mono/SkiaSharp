﻿using System;
using System.Collections.Generic;
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

			Assert.True(srcPix.ScalePixels(dstPix, new SKSamplingOptions(SKCubicResampler.Mitchell)));

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

				Assert.Equal(SKColors.Red, bmp.PeekPixels().GetPixelColor(0, 0));

				SKSwizzle.SwapRedBlue(bmp.GetPixels(out var length), info.Width * info.Height);

				Assert.Equal(SKColors.Blue, bmp.PeekPixels().GetPixelColor(0, 0));
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

		[SkippableFact]
		public void MismatchingColorTypesThrow()
		{
			var info = new SKImageInfo(1, 1, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			Assert.Throws<ArgumentException>(() => pixmap.GetPixelSpan<ushort>());
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllColorTypes))]
		public void ByteWorksForEverything(SKColorType colortype)
		{
			var info = new SKImageInfo(1, 1, colortype);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			Assert.Equal(info.BytesSize, pixmap?.GetPixelSpan<byte>().Length ?? 0);
		}

		[SkippableTheory]
		[InlineData(0x00000000)]
		[InlineData(0xFF000000)]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		[InlineData(0xFFFFFFFF)]
		public void GetPixelSpanReadsValuesCorrectly(uint color)
		{
			var rgb888 = (SKColor)color;

			var info = new SKImageInfo(1, 1, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(rgb888);

			// no need for swizzle
			Assert.Equal(rgb888, pixmap.GetPixelColor(0, 0));

			// swizzle for some CPU endianness
			if (BitConverter.IsLittleEndian)
				rgb888 = new SKColor(rgb888.Blue, rgb888.Green, rgb888.Red, rgb888.Alpha);

			Assert.Equal(rgb888, pixmap.GetPixelSpan<SKColor>()[0]);
			Assert.Equal(rgb888, pixmap.GetPixelSpan<uint>()[0]);
		}

		[SkippableTheory]
		[InlineData(0x00000000, 0x0000)]
		[InlineData(0xFF000000, 0x0000)]
		[InlineData(0xFFFF0000, 0xF800)]
		[InlineData(0xFF00FF00, 0x07E0)]
		[InlineData(0xFF0000FF, 0x001F)]
		[InlineData(0xFFFFFFFF, 0xFFFF)]
		public void GetPixelSpanReads565Correctly(uint rgb888, ushort rgb565)
		{
			var info = new SKImageInfo(1, 1, SKColorType.Rgb565);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(rgb888);

			Assert.Equal(rgb565, pixmap.GetPixelSpan<ushort>()[0]);
		}

		[SkippableTheory]
		[InlineData(0x00000000, 0)]
		[InlineData(0xFF000000, 0)]
		[InlineData(0xFFFF0000, 54)]
		[InlineData(0xFF00FF00, 182)]
		[InlineData(0xFF0000FF, 18)]
		[InlineData(0xFFFFFFFF, 255)]
		public void GetPixelSpanReadsGray8Correctly(uint rgb888, byte gray8)
		{
			var info = new SKImageInfo(1, 1, SKColorType.Gray8);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(rgb888);

			Assert.Equal(gray8, pixmap.GetPixelSpan<byte>()[0]);
		}

		[SkippableTheory]
		// Rgb565 => 2 bytes per pixel
		[InlineData(SKColorType.Rgb565, 1, 1, 0, 0, 0)]
		[InlineData(SKColorType.Rgb565, 2, 2, 0, 0, 0)]
		[InlineData(SKColorType.Rgb565, 2, 2, 1, 0, 2)]
		[InlineData(SKColorType.Rgb565, 2, 2, 1, 0, 2)]
		[InlineData(SKColorType.Rgb565, 2, 2, 0, 1, 4)]
		[InlineData(SKColorType.Rgb565, 2, 2, 0, 1, 4)]
		[InlineData(SKColorType.Rgb565, 2, 2, 1, 1, 6)]
		[InlineData(SKColorType.Rgb565, 2, 2, 1, 1, 6)]
		// Rgba8888 => 4 bytes per pixel
		[InlineData(SKColorType.Rgba8888, 1, 1, 0, 0, 0)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 0, 0, 0)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 1, 0, 4)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 1, 0, 4)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 0, 1, 8)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 0, 1, 8)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 1, 1, 12)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 1, 1, 12)]
		public void GetPixelBytesOffsetIsCorrect(SKColorType ct, int w, int h, int x, int y, int offset)
		{
			var info = new SKImageInfo(w, h, ct);
			Assert.Equal(offset, info.GetPixelBytesOffset(x, y));
		}

		[SkippableTheory]
		[InlineData(0x00000000)]
		[InlineData(0xFF000000)]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		[InlineData(0xFFFFFFFF)]
		public void GetPixelSpanWithOffsetReadsValuesCorrectly(uint color)
		{
			var rgb888 = (SKColor)color;

			// swizzle for some CPU endianness
			var rawRgb888 = rgb888;
			if (BitConverter.IsLittleEndian)
				rawRgb888 = new SKColor(rgb888.Blue, rgb888.Green, rgb888.Red, rgb888.Alpha);

			var info = new SKImageInfo(3, 3, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(rgb888);

			for (var x = 0; x < info.Width; x++)
			{
				for (var y = 0; y < info.Height; y++)
				{
					var expectedLength = (info.Width * info.Height) - (y * info.Height + x);

					// no need for swizzle
					Assert.Equal(rgb888, pixmap.GetPixelColor(x, y));

					// may need a swizzle
					var colors = pixmap.GetPixelSpan<SKColor>(x, y);
					Assert.Equal(expectedLength, colors.Length);
					Assert.Equal(rawRgb888, colors[0]);

					var uints = pixmap.GetPixelSpan<uint>(x, y);
					Assert.Equal(expectedLength, uints.Length);
					Assert.Equal(rawRgb888, uints[0]);
				}
			}
		}

		[SkippableTheory]
		[InlineData(0x00000000, 0x0000)]
		[InlineData(0xFF000000, 0x0000)]
		[InlineData(0xFFFF0000, 0xF800)]
		[InlineData(0xFF00FF00, 0x07E0)]
		[InlineData(0xFF0000FF, 0x001F)]
		[InlineData(0xFFFFFFFF, 0xFFFF)]
		public void GetPixelSpanWithOffsetReads565Correctly(uint rgb888, ushort rgb565)
		{
			var info = new SKImageInfo(3, 3, SKColorType.Rgb565);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(rgb888);

			for (var x = 0; x < info.Width; x++)
			{
				for (var y = 0; y < info.Height; y++)
				{
					var expectedLength = (info.Width * info.Height) - (y * info.Height + x);

					var ushorts = pixmap.GetPixelSpan<ushort>(x, y);
					Assert.Equal(expectedLength, ushorts.Length);
					Assert.Equal(rgb565, ushorts[0]);
				}
			}
		}

		[SkippableTheory]
		[InlineData(0x00000000, 0)]
		[InlineData(0xFF000000, 0)]
		[InlineData(0xFFFF0000, 54)]
		[InlineData(0xFF00FF00, 182)]
		[InlineData(0xFF0000FF, 18)]
		[InlineData(0xFFFFFFFF, 255)]
		public void GetPixelSpanWithOffsetReadsGray8Correctly(uint rgb888, byte gray8)
		{
			var info = new SKImageInfo(3, 3, SKColorType.Gray8);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(rgb888);

			for (var x = 0; x < info.Width; x++)
			{
				for (var y = 0; y < info.Height; y++)
				{
					var expectedLength = (info.Width * info.Height) - (y * info.Height + x);

					var bytes = pixmap.GetPixelSpan<byte>(x, y);
					Assert.Equal(expectedLength, bytes.Length);
					Assert.Equal(gray8, bytes[0]);
				}
			}
		}
	}
}
