using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPixmapTest : SKTest
	{
		[Fact]
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

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void ReadPixelSucceeds()
		{
			var info = new SKImageInfo(10, 10);

			var ptr1 = Marshal.AllocCoTaskMem(info.BytesSize);
			var pix1 = new SKPixmap(info, ptr1);

			var ptr2 = Marshal.AllocCoTaskMem(info.BytesSize);

			var result = pix1.ReadPixels(info, ptr2, info.RowBytes);

			Assert.True(result);
		}

		[Fact]
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

		[Fact]
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

		[Fact]
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

		[Fact]
		public void EraseWithColor()
		{
			var info = new SKImageInfo(1, 1);

			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(SKColors.Red);

			Assert.Equal(SKColors.Red, pixmap.GetPixelColor(0, 0));
		}

		[Fact]
		public void EraseWithColorF()
		{
			var info = new SKImageInfo(1, 1);

			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			pixmap.Erase(new SKColorF(1, 0, 0));

			Assert.Equal(SKColors.Red, pixmap.GetPixelColor(0, 0));
		}

		[Fact]
		public void EncodeWithPngEncoder()
		{
			var bitmap = CreateTestBitmap();
			var pixmap = bitmap.PeekPixels();

			var data = pixmap.Encode(SKPngEncoderOptions.Default);

			Assert.NotNull(data);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Png, codec.EncodedFormat);
		}

		[Fact]
		public void EncodeWithJpegEncoder()
		{
			var bitmap = CreateTestBitmap();
			var pixmap = bitmap.PeekPixels();

			var data = pixmap.Encode(SKJpegEncoderOptions.Default);

			Assert.NotNull(data);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Jpeg, codec.EncodedFormat);
		}

		[Fact]
		public void EncodeWithWebpEncoder()
		{
			var bitmap = CreateTestBitmap();
			var pixmap = bitmap.PeekPixels();

			var data = pixmap.Encode(SKWebpEncoderOptions.Default);

			Assert.NotNull(data);

			var codec = SKCodec.Create(data);

			Assert.Equal(SKEncodedImageFormat.Webp, codec.EncodedFormat);
		}

		[Fact]
		public void MismatchingColorTypesThrow()
		{
			var info = new SKImageInfo(1, 1, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			Assert.Throws<ArgumentException>(() => pixmap.GetPixelSpan<ushort>());
		}

		[Theory]
		[MemberData(nameof(GetAllColorTypes))]
		public void ByteWorksForEverything(SKColorType colortype)
		{
			var info = new SKImageInfo(1, 1, colortype);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			Assert.Equal(info.BytesSize, pixmap?.GetPixelSpan<byte>().Length ?? 0);
		}

		[Theory]
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

		[Theory]
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

		[Theory]
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

		[Theory]
		// Rgb565 => 2 bytes per pixel
		[InlineData(SKColorType.Rgb565, 1, 1, 0, 0, 0)]
		[InlineData(SKColorType.Rgb565, 2, 2, 0, 0, 0)]
		[InlineData(SKColorType.Rgb565, 2, 2, 1, 0, 2)]
		[InlineData(SKColorType.Rgb565, 2, 2, 0, 1, 4)]
		[InlineData(SKColorType.Rgb565, 2, 2, 1, 1, 6)]
		// Rgba8888 => 4 bytes per pixel
		[InlineData(SKColorType.Rgba8888, 1, 1, 0, 0, 0)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 0, 0, 0)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 1, 0, 4)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 0, 1, 8)]
		[InlineData(SKColorType.Rgba8888, 2, 2, 1, 1, 12)]
		public void GetPixelBytesOffsetIsCorrect(SKColorType ct, int w, int h, int x, int y, int offset)
		{
			var info = new SKImageInfo(w, h, ct);
			Assert.Equal(offset, info.GetPixelBytesOffset(x, y, info.RowBytes));
		}

		[Theory]
		// the offset must honor a stride that is larger than the packed width
		// Rgba8888 (4 bpp), 2px wide, but a 16-byte stride (8 bytes of padding)
		[InlineData(SKColorType.Rgba8888, 2, 16, 0, 0, 0)]
		[InlineData(SKColorType.Rgba8888, 2, 16, 1, 0, 4)]
		[InlineData(SKColorType.Rgba8888, 2, 16, 0, 1, 16)]
		[InlineData(SKColorType.Rgba8888, 2, 16, 1, 1, 20)]
		public void GetPixelBytesOffsetHonorsStride(SKColorType ct, int w, int rowBytes, int x, int y, int offset)
		{
			var info = new SKImageInfo(w, 4, ct);
			Assert.Equal(offset, info.GetPixelBytesOffset(x, y, rowBytes));
		}

		[Theory]
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
					var expectedLength = (info.Width * info.Height) - (y * info.Width + x);

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

		[Theory]
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
					var expectedLength = (info.Width * info.Height) - (y * info.Width + x);

					var ushorts = pixmap.GetPixelSpan<ushort>(x, y);
					Assert.Equal(expectedLength, ushorts.Length);
					Assert.Equal(rgb565, ushorts[0]);
				}
			}
		}

		[Theory]
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
					var expectedLength = (info.Width * info.Height) - (y * info.Width + x);

					var bytes = pixmap.GetPixelSpan<byte>(x, y);
					Assert.Equal(expectedLength, bytes.Length);
					Assert.Equal(gray8, bytes[0]);
				}
			}
		}

		[Fact]
		public void GetPixelSpanWithOffsetWorksForNonSquarePixmaps()
		{
			// Use a non-square pixmap to expose the bug where Width and Height were swapped
			var info = new SKImageInfo(10, 5);
			using var bmp = new SKBitmap(info);
			using var canvas = new SKCanvas(bmp);

			canvas.Clear(SKColors.Red);

			using var pixmap = bmp.PeekPixels();

			// Verify offset calculation produces correct span lengths for all positions
			for (var y = 0; y < info.Height; y++)
			{
				for (var x = 0; x < info.Width; x++)
				{
					var span = pixmap.GetPixelSpan<SKColor>(x, y);

					// The span should start at pixel (x, y) and extend to end
					var expectedLength = info.Width * info.Height - (y * info.Width + x);
					Assert.Equal(expectedLength, span.Length);
				}
			}
		}

		[Fact]
		public void GetPixelSpanWithOffsetReturnsCorrectPixelForNonSquare()
		{
			// Create a wide pixmap and draw distinct colors in known positions
			var info = new SKImageInfo(8, 3);
			using var bmp = new SKBitmap(info);
			using var canvas = new SKCanvas(bmp);
			canvas.Clear(SKColors.Transparent);

			// Draw colored rectangles in specific rows
			using var paint = new SKPaint();
			paint.Color = SKColors.Red;
			canvas.DrawRect(0, 0, 8, 1, paint);
			paint.Color = SKColors.Green;
			canvas.DrawRect(0, 1, 8, 1, paint);
			paint.Color = SKColors.Blue;
			canvas.DrawRect(0, 2, 8, 1, paint);

			using var pixmap = bmp.PeekPixels();

			// Verify that GetPixelSpan at different rows returns data matching that row's color
			// by checking that span[0] matches what GetPixelColor reports at that position
			var spanRow0 = pixmap.GetPixelSpan<SKColor>(0, 0);
			var spanRow1 = pixmap.GetPixelSpan<SKColor>(0, 1);
			var spanRow2 = pixmap.GetPixelSpan<SKColor>(0, 2);

			// All pixels in each row should have the same raw value
			// Compare within the same span to verify offset is pointing to the right row
			Assert.Equal(spanRow0[0], spanRow0[3]); // same row, same color
			Assert.Equal(spanRow1[0], spanRow1[5]); // same row, same color
			Assert.Equal(spanRow2[0], spanRow2[7]); // same row, same color

			// Different rows should have different colors
			Assert.NotEqual(spanRow0[0], spanRow1[0]);
			Assert.NotEqual(spanRow1[0], spanRow2[0]);
			Assert.NotEqual(spanRow0[0], spanRow2[0]);

			// Verify mid-row access: GetPixelSpan(4, 1) should give same first pixel as
			// GetPixelSpan(0, 1) offset by 4 (same row)
			var spanMidRow1 = pixmap.GetPixelSpan<SKColor>(4, 1);
			Assert.Equal(spanRow1[4], spanMidRow1[0]);
		}

		[Fact]
		public void GetPixelSpanHandlesStrideCorrectly()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			for (int y = 0; y < 4; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					bmp.SetPixel(x, y, x < 2 && y < 2 ? SKColors.White : SKColors.Black);
				}
			}

			using var pixmap = bmp.PeekPixels();
			var span = pixmap.GetPixelSpan<SKColor>();
			Assert.Equal(16, span.Length);

			using SKBitmap roi = new();
			bool ok = bmp.ExtractSubset(roi, new SKRectI(0, 0, 2, 2));
			Assert.True(ok);

			using var roiPixmap = roi.PeekPixels();

			// the subset shares the parent buffer, so its stride is the parent's
			// row bytes; the span must reach the last valid pixel of the last row
			Assert.True(roiPixmap.RowBytes > roiPixmap.Info.Width * roiPixmap.BytesPerPixel);

			var roiSpan = roiPixmap.GetPixelSpan<SKColor>();
			Assert.Equal(6, roiSpan.Length);

			// both pixels in the subset's first row are white
			var white = roiPixmap.GetPixelColor(0, 0);
			Assert.Equal(white, roiSpan[0]);
			Assert.Equal(white, roiSpan[1]);

			// row 1 of the subset starts at the stride offset
			var stridePixels = roiPixmap.RowBytes / roiPixmap.BytesPerPixel;
			var roiSpanRow1 = roiPixmap.GetPixelSpan<SKColor>(0, 1);
			Assert.Equal(roiSpan[stridePixels], roiSpanRow1[0]);
		}

		// Bgra8888 stores bytes as B, G, R, A; reinterpreting those bytes as a
		// little-endian SKColor (0xAARRGGBB) reproduces the logical color exactly,
		// so the span contents can be compared without any channel swizzle.
		private static SKColor UniqueColor(int x, int y) =>
			new SKColor((byte)(x + 1), (byte)(y + 1), 0xAB, 0xFF);

		private static SKBitmap CreateUniquePixelBitmap(int width, int height)
		{
			var bmp = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
			for (var y = 0; y < height; y++)
				for (var x = 0; x < width; x++)
					bmp.SetPixel(x, y, UniqueColor(x, y));
			return bmp;
		}

		[Theory]
		[InlineData(0, 0, 3, 2)]   // origin (0, 0)
		[InlineData(3, 2, 7, 5)]   // interior origin, non-square
		[InlineData(2, 0, 5, 3)]   // top edge, non-zero x
		[InlineData(0, 3, 3, 6)]   // left edge, non-zero y
		[InlineData(5, 4, 8, 6)]   // bottom-right, touches the parent's last row/col
		public void GetPixelSpanReturnsExactSubsetPixels(int left, int top, int right, int bottom)
		{
			// every parent pixel has a unique colour encoding its (x, y), so reading
			// the wrong row or column (the original bug) produces a wrong colour
			using var bmp = CreateUniquePixelBitmap(8, 6);

			using SKBitmap roi = new();
			Assert.True(bmp.ExtractSubset(roi, new SKRectI(left, top, right, bottom)));

			using var pixmap = roi.PeekPixels();

			// the subset is narrower than the parent, so its stride must differ
			Assert.True(pixmap.RowBytes > pixmap.Width * pixmap.BytesPerPixel);

			for (var y = 0; y < pixmap.Height; y++)
			{
				for (var x = 0; x < pixmap.Width; x++)
				{
					var expected = UniqueColor(left + x, top + y);

					// the typed span at (x, y) must point at exactly the parent pixel
					var fromSpan = pixmap.GetPixelSpan<SKColor>(x, y)[0];
					Assert.Equal(expected, fromSpan);

					// cross-check against the independent logical accessor
					Assert.Equal(expected, pixmap.GetPixelColor(x, y));
				}
			}

			// the full span's first element is the subset's top-left pixel
			Assert.Equal(UniqueColor(left, top), pixmap.GetPixelSpan<SKColor>()[0]);
		}

		[Fact]
		public void GetPixelSpanHandlesNonSquareSubset()
		{
			// distinct dimensions so a Width/Height swap would be caught
			var info = new SKImageInfo(6, 4, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using (var canvas = new SKCanvas(bmp))
				canvas.Clear(SKColors.Red);

			// non-square, non-square subset
			using SKBitmap roi = new();
			Assert.True(bmp.ExtractSubset(roi, new SKRectI(0, 0, 3, 2)));

			using var pixmap = roi.PeekPixels();
			Assert.Equal(3, pixmap.Width);
			Assert.Equal(2, pixmap.Height);
			Assert.True(pixmap.RowBytes > pixmap.Width * pixmap.BytesPerPixel);

			var rowLength = pixmap.RowBytes / pixmap.BytesPerPixel;

			// length = (Height - 1) * rowLength + Width
			var span = pixmap.GetPixelSpan<SKColor>();
			Assert.Equal((2 - 1) * rowLength + 3, span.Length);

			// offset of (2, 1) = 1 * rowLength + 2
			var spanX2Y1 = pixmap.GetPixelSpan<SKColor>(2, 1);
			Assert.Equal(span.Length - (rowLength + 2), spanX2Y1.Length);
		}

		[Fact]
		public void GetPixelSpanHandlesBottomRightSubset()
		{
			// a subset whose last row is the parent's last row: a naively padded
			// last row would read past the end of the parent buffer
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using (var canvas = new SKCanvas(bmp))
				canvas.Clear(SKColors.Blue);
			bmp.SetPixel(3, 3, SKColors.Green);

			using SKBitmap roi = new();
			Assert.True(bmp.ExtractSubset(roi, new SKRectI(2, 2, 4, 4)));

			using var pixmap = roi.PeekPixels();
			var rowLength = pixmap.RowBytes / pixmap.BytesPerPixel;

			// length stops at the last valid pixel of the last row
			var span = pixmap.GetPixelSpan<SKColor>();
			Assert.Equal((2 - 1) * rowLength + 2, span.Length);

			// the last valid pixel (1, 1) is the green parent pixel (3, 3)
			var green = pixmap.GetPixelColor(1, 1);
			Assert.Equal(green, span[span.Length - 1]);
			Assert.Equal(SKColors.Green, green);
		}

		[Fact]
		public void GetPixelSpanByteBranchHandlesSubsetStride()
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);

			using SKBitmap roi = new();
			Assert.True(bmp.ExtractSubset(roi, new SKRectI(0, 0, 2, 2)));

			using var pixmap = roi.PeekPixels();

			// (Height - 1) * RowBytes + Width * BytesPerPixel = 1 * 16 + 2 * 4
			var span = pixmap.GetPixelSpan<byte>();
			Assert.Equal(24, span.Length);

			// offset for row 1 is one full stride
			var row1 = pixmap.GetPixelSpan<byte>(0, 1);
			Assert.Equal(span.Length - pixmap.RowBytes, row1.Length);

			// the last valid pixel must reduce to exactly one pixel of bytes,
			// proving the byte-branch offset never reads past the parent buffer
			var lastByte = pixmap.GetPixelSpan<byte>(pixmap.Width - 1, pixmap.Height - 1);
			Assert.Equal(pixmap.BytesPerPixel, lastByte.Length);
		}

		[Fact]
		public void GetPixelSpanHandlesSubsetForHighBitShiftColorType()
		{
			// RgbaF32 is 16 bytes per pixel (shift 4) - the largest multiplier
			// and the most relevant to the offset overflow surface
			var info = new SKImageInfo(4, 4, SKColorType.RgbaF32);
			using var bmp = new SKBitmap(info);

			using SKBitmap roi = new();
			Assert.True(bmp.ExtractSubset(roi, new SKRectI(0, 0, 2, 2)));

			using var pixmap = roi.PeekPixels();
			Assert.Equal(16, pixmap.BytesPerPixel);
			Assert.True(pixmap.RowBytes > pixmap.Width * pixmap.BytesPerPixel);

			// typed: the last valid pixel reduces to exactly one element
			var rowLength = pixmap.RowBytes / pixmap.BytesPerPixel;
			var span = pixmap.GetPixelSpan<SKColorF>();
			Assert.Equal((2 - 1) * rowLength + 2, span.Length);
			var lastTyped = pixmap.GetPixelSpan<SKColorF>(pixmap.Width - 1, pixmap.Height - 1);
			Assert.Equal(1, lastTyped.Length);

			// byte: the last valid pixel reduces to exactly BytesPerPixel
			var lastByte = pixmap.GetPixelSpan<byte>(pixmap.Width - 1, pixmap.Height - 1);
			Assert.Equal(pixmap.BytesPerPixel, lastByte.Length);
		}

		[Fact]
		public void GetPixelSpanHandlesSubsetForMultiByteColorType()
		{
			// Rgb565 is 2 bytes per pixel, exercising a different bpp/shift
			var info = new SKImageInfo(4, 4, SKColorType.Rgb565);
			using var bmp = new SKBitmap(info);
			using (var canvas = new SKCanvas(bmp))
				canvas.Clear(SKColors.Red);

			using SKBitmap roi = new();
			Assert.True(bmp.ExtractSubset(roi, new SKRectI(0, 0, 2, 2)));

			using var pixmap = roi.PeekPixels();
			Assert.True(pixmap.RowBytes > pixmap.Width * pixmap.BytesPerPixel);

			var rowLength = pixmap.RowBytes / pixmap.BytesPerPixel;
			var span = pixmap.GetPixelSpan<ushort>();
			Assert.Equal((2 - 1) * rowLength + 2, span.Length);
		}

		[Theory]
		[InlineData(-1, 0)]
		[InlineData(0, -1)]
		[InlineData(0, 4)]
		[InlineData(4, 0)]
		public void GetPixelSpanThrowsForOutOfRangeCoordinates(int x, int y)
		{
			var info = new SKImageInfo(4, 4, SKColorType.Rgba8888);
			using var bmp = new SKBitmap(info);
			using var pixmap = bmp.PeekPixels();

			Assert.Throws<ArgumentOutOfRangeException>(() => pixmap.GetPixelSpan<SKColor>(x, y));
			Assert.Throws<ArgumentOutOfRangeException>(() => pixmap.GetPixelSpan(x, y));
		}

		[Fact]
		public unsafe void GetPixelSpanTypedThrowsForStrideNotMultipleOfElement()
		{
			// a stride that is not a whole number of pixels cannot be represented
			// by a typed span, but the byte overload must still work
			var info = new SKImageInfo(1, 2, SKColorType.Rgba8888);
			var rowBytes = info.Width * info.BytesPerPixel + 1; // 5
			var buffer = Marshal.AllocCoTaskMem(rowBytes * info.Height);
			try
			{
				using var pixmap = new SKPixmap(info, buffer, rowBytes);

				// byte overload works with any stride
				var bytes = pixmap.GetPixelSpan<byte>(0, 1);
				Assert.Equal(rowBytes, pixmap.GetPixelSpan<byte>().Length - bytes.Length);

				// typed overload cannot represent the fractional stride
				Assert.Throws<ArgumentException>(() => pixmap.GetPixelSpan<SKColor>());
			}
			finally
			{
				Marshal.FreeCoTaskMem(buffer);
			}
		}

		[Fact]
		public unsafe void GetPixelSpanTypedAllowsStrideNotMultipleOfElementForSingleRow()
		{
			// a single-row pixmap never crosses the stride padding, so even a
			// stride that is not a whole number of pixels is representable as a
			// typed span of exactly Width elements
			var info = new SKImageInfo(1, 1, SKColorType.Rgba8888);
			var rowBytes = info.Width * info.BytesPerPixel + 1; // 5
			var buffer = Marshal.AllocCoTaskMem(rowBytes * info.Height);
			try
			{
				using var pixmap = new SKPixmap(info, buffer, rowBytes);

				var span = pixmap.GetPixelSpan<SKColor>();
				Assert.Equal(info.Width, span.Length);

				var atOrigin = pixmap.GetPixelSpan<SKColor>(0, 0);
				Assert.Equal(info.Width, atOrigin.Length);
			}
			finally
			{
				Marshal.FreeCoTaskMem(buffer);
			}
		}

		[Fact]
		public void GetPixelSpanReturnsEmptyForEmptyPixmap()
		{
			using var pixmap = new SKPixmap();
			Assert.True(pixmap.Info.IsEmpty);

			// an empty pixmap returns an empty span rather than throwing, and the
			// empty short-circuit wins even over out-of-range coordinates
			Assert.True(pixmap.GetPixelSpan<byte>().IsEmpty);
			Assert.True(pixmap.GetPixelSpan<byte>(0, 0).IsEmpty);
			Assert.True(pixmap.GetPixelSpan<byte>(-1, 5).IsEmpty);
			Assert.True(pixmap.GetPixelSpan<SKColor>(0, 0).IsEmpty);
			Assert.True(pixmap.GetPixelSpan<SKColor>(-1, 5).IsEmpty);
		}

		[Fact]
		public unsafe void GetPixelSpanReturnsEmptyForUnknownColorType()
		{
			// a non-empty buffer with an unknown color type has no pixel size, so
			// it returns an empty span rather than throwing
			var info = new SKImageInfo(4, 4, SKColorType.Unknown);
			Assert.False(info.IsEmpty);
			Assert.Equal(0, info.BytesPerPixel);

			var buffer = Marshal.AllocCoTaskMem(64);
			try
			{
				using var pixmap = new SKPixmap(info, buffer);

				Assert.True(pixmap.GetPixelSpan<byte>(0, 0).IsEmpty);
				Assert.True(pixmap.GetPixelSpan<byte>(2, 3).IsEmpty);
				Assert.True(pixmap.GetPixelSpan<byte>(-1, 99).IsEmpty);
			}
			finally
			{
				Marshal.FreeCoTaskMem(buffer);
			}
		}
	}
}
