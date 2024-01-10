using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests
{
	public class SKBitmapTest : SKTest
	{
		public SKBitmapTest(ITestOutputHelper output)
			: base(output)
		{
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllColorTypes))]
		public void CanCopyToIsCorrect(SKColorType colorType)
		{
			using var bmp = CreateTestBitmap();

			var canCopy = bmp.CanCopyTo(colorType);

			if (colorType == SKColorType.Unknown)
				Assert.False(canCopy);
			else
				Assert.True(canCopy);
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllColorTypes))]
		public void CopyToSucceeds(SKColorType colorType)
		{
			if (colorType == SKColorType.Bgr101010xXR)
				throw new SkipException("The Bgr101010xXR does not support getting pixel colors.");

			var alphaType = colorType.GetAlphaType();

			using var bmp = CreateTestBitmap();

			using var copy = bmp.Copy(colorType);

			if (colorType == SKColorType.Unknown)
			{
				Assert.Null(copy);
			}
			else
			{
				Assert.NotNull(copy);
				Assert.Equal(colorType, copy.ColorType);
				Assert.Equal(alphaType, copy.AlphaType);

				var color = copy.GetPixel(10, 10);
				Assert.NotEqual(SKColors.Empty, color);
				if (colorType == SKColorType.Gray8)
					Assert.Equal(0xFF353535, color);
				else if (colorType == SKColorType.Alpha8 || colorType == SKColorType.AlphaF16 || colorType == SKColorType.Alpha16)
					Assert.Equal(0xFF000000, color);
				else
					Assert.Equal(0xFFFF0000, color);
			}
		}

		[SkippableTheory]
		[MemberData(nameof(GetAllColorTypes))]
		public void CopyWithAlphaToSucceeds(SKColorType colorType)
		{
			if (colorType == SKColorType.Bgr101010xXR)
				throw new SkipException("The Bgr101010xXR does not support getting pixel colors.");

			var alphaType = colorType.GetAlphaType();

			using var bmp = CreateTestBitmap(170);

			using var copy = bmp.Copy(colorType);

			if (colorType == SKColorType.Unknown)
			{
				Assert.Null(copy);
			}
			else
			{
				Assert.NotNull(copy);
				Assert.Equal(colorType, copy.ColorType);
				Assert.Equal(alphaType, copy.AlphaType);

				var color = copy.GetPixel(10, 10);
				Assert.NotEqual(SKColors.Empty, color);

				if (colorType == SKColorType.Gray8)
				{
					Assert.Equal((SKColor)0xFF232323, color);
				}
				else if (alphaType == SKAlphaType.Opaque)
				{
					Assert.True(color.Red > color.Green);
					Assert.True(color.Red > color.Blue);
					Assert.Equal(255, color.Alpha);
				}
				else if (colorType == SKColorType.Alpha8 || colorType == SKColorType.Alpha16)
				{
					Assert.Equal((SKColor)0xAA000000, color);
				}
				else if (colorType == SKColorType.AlphaF16)
				{
					// rounding
					Assert.Equal((SKColor)0xA9000000, color);
				}
				else if (colorType == SKColorType.Srgba8888)
				{
					// SRGB processing
					Assert.Equal((SKColor)0xAAFE0000, color);
				}
				else
				{
					Assert.Equal((SKColor)0xAAFF0000, color);
				}
			}
		}

		[SkippableFact]
		public void DoesNotCrashWhenDecodingInvalidPath()
		{
			var path = Path.Combine(PathToImages, "file-does-not-exist.png");

			Assert.Null(SKBitmap.Decode(path));
		}

		[SkippableFact]
		public void CopyIndex8ToPlatformPreservesData()
		{
			var path = Path.Combine(PathToImages, "index8.png");
			var bmp = SKBitmap.Decode(path);

			var platform = bmp.Copy(SKImageInfo.PlatformColorType);

			Assert.Equal((SKColor)0x7EA4C639, platform.GetPixel(182, 348));
			Assert.Equal(SKImageInfo.PlatformColorType, platform.ColorType);
		}

		[SkippableFact]
		public void OverwriteIndex8ToPlatformPreservesData()
		{
			var path = Path.Combine(PathToImages, "index8.png");
			var bmp = SKBitmap.Decode(path);

			bmp.CopyTo(bmp, SKImageInfo.PlatformColorType);

			Assert.Equal((SKColor)0x7EA4C639, bmp.GetPixel(182, 348));
			Assert.Equal(SKImageInfo.PlatformColorType, bmp.ColorType);
		}

		[SkippableFact]
		public void BitmapCopyToAlpha8PreservesData()
		{
			var bmp = CreateTestBitmap();

			var alpha8 = bmp.Copy(SKColorType.Alpha8);

			Assert.Equal(SKColors.Black, alpha8.GetPixel(10, 10));
			Assert.Equal(SKColors.Black, alpha8.GetPixel(30, 10));
			Assert.Equal(SKColors.Black, alpha8.GetPixel(10, 30));
			Assert.Equal(SKColors.Black, alpha8.GetPixel(30, 30));
			Assert.Equal(SKColorType.Alpha8, alpha8.ColorType);
		}

		[SkippableFact]
		public void BitmapWithAlphaCopyToAlpha8PreservesData()
		{
			var bmp = CreateTestBitmap(127);

			var alpha8 = bmp.Copy(SKColorType.Alpha8);

			Assert.Equal(SKColors.Black.WithAlpha(127), alpha8.GetPixel(10, 10));
			Assert.Equal(SKColors.Black.WithAlpha(127), alpha8.GetPixel(30, 10));
			Assert.Equal(SKColors.Black.WithAlpha(127), alpha8.GetPixel(10, 30));
			Assert.Equal(SKColors.Black.WithAlpha(127), alpha8.GetPixel(30, 30));
			Assert.Equal(SKColorType.Alpha8, alpha8.ColorType);
		}

		[SkippableFact]
		public void BitmapCopyToArgb4444PreservesData()
		{
			var bmp = CreateTestBitmap();

			var argb4444 = bmp.Copy(SKColorType.Argb4444);

			Assert.Equal((SKColor)0xffff0000, argb4444.GetPixel(10, 10));
			Assert.Equal((SKColor)0xff008800, argb4444.GetPixel(30, 10));
			Assert.Equal((SKColor)0xff0000ff, argb4444.GetPixel(10, 30));
			Assert.Equal((SKColor)0xffffff00, argb4444.GetPixel(30, 30));
			Assert.Equal(SKColorType.Argb4444, argb4444.ColorType);
		}

		[SkippableFact]
		public void BitmapWithAlphaCopyToArgb4444PreservesData()
		{
			var bmp = CreateTestBitmap(127);

			var argb4444 = bmp.Copy(SKColorType.Argb4444);

			Assert.Equal((SKColor)0x77ff0000, argb4444.GetPixel(10, 10));
			Assert.Equal((SKColor)0x77009200, argb4444.GetPixel(30, 10));
			Assert.Equal((SKColor)0x770000ff, argb4444.GetPixel(10, 30));
			Assert.Equal((SKColor)0x77ffff00, argb4444.GetPixel(30, 30));
			Assert.Equal(SKColorType.Argb4444, argb4444.ColorType);
		}

		[SkippableFact]
		public void BitmapCopyToRgb565PreservesData()
		{
			var bmp = CreateTestBitmap();

			var rgb565 = bmp.Copy(SKColorType.Rgb565);

			Assert.Equal((SKColor)0xffff0000, rgb565.GetPixel(10, 10));
			Assert.Equal((SKColor)0xff008200, rgb565.GetPixel(31, 10));
			Assert.Equal((SKColor)0xff0000ff, rgb565.GetPixel(10, 30));
			Assert.Equal((SKColor)0xffffff00, rgb565.GetPixel(30, 30));
			Assert.Equal(SKColorType.Rgb565, rgb565.ColorType);
		}

		[SkippableFact]
		public void BitmapWithAlphaCopyToRgb565PreservesData()
		{
			var bmp = CreateTestBitmap(127);

			var rgb565 = bmp.Copy(SKColorType.Rgb565);

			Assert.Equal((SKColor)0xff7b0000, rgb565.GetPixel(10, 10));
			Assert.Equal((SKColor)0xff004100, rgb565.GetPixel(31, 10));
			Assert.Equal((SKColor)0xff00007b, rgb565.GetPixel(10, 30));
			Assert.Equal((SKColor)0xff7b7d00, rgb565.GetPixel(30, 30));
			Assert.Equal(SKColorType.Rgb565, rgb565.ColorType);
		}

		[SkippableFact]
		public void BitmapCopyToRgbaF16PreservesData()
		{
			var bmp = CreateTestBitmap();

			var rgbaF16 = bmp.Copy(SKColorType.RgbaF16);

			Assert.Equal((SKColor)0xffff0000, rgbaF16.GetPixel(10, 10));
			Assert.Equal((SKColor)0xff008000, rgbaF16.GetPixel(30, 10));
			Assert.Equal((SKColor)0xff0000ff, rgbaF16.GetPixel(10, 30));
			Assert.Equal((SKColor)0xffffff00, rgbaF16.GetPixel(30, 30));
			Assert.Equal(SKColorType.RgbaF16, rgbaF16.ColorType);
		}

		[SkippableFact]
		public void BitmapWithAlphaCopyToRgbaF16PreservesData()
		{
			var bmp = CreateTestBitmap(127);

			var rgbaF16 = bmp.Copy(SKColorType.RgbaF16);

			Assert.Equal((SKColor)0x7fff0000, rgbaF16.GetPixel(10, 10));
			Assert.Equal((SKColor)0x7f008100, rgbaF16.GetPixel(30, 10));
			Assert.Equal((SKColor)0x7f0000ff, rgbaF16.GetPixel(10, 30));
			Assert.Equal((SKColor)0x7fffff00, rgbaF16.GetPixel(30, 30));
			Assert.Equal(SKColorType.RgbaF16, rgbaF16.ColorType);
		}

		[SkippableFact]
		public void BitmapCopyToInvalidIsNull()
		{
			var bmp = CreateTestBitmap();

			ValidateTestBitmap(bmp);
			ValidateTestBitmap(bmp.Copy(SKImageInfo.PlatformColorType));

			Assert.Null(bmp.Copy(SKColorType.Unknown));
		}

		[SkippableFact]
		public void ReleaseBitmapPixelsWasInvoked()
		{
			bool released = false;

			var onRelease = new SKBitmapReleaseDelegate((addr, ctx) =>
			{
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.Equal("RELEASING!", ctx);
			});

			using (var bitmap = new SKBitmap())
			{
				var info = new SKImageInfo(1, 1);
				var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

				bitmap.InstallPixels(info, pixels, info.RowBytes, onRelease, "RELEASING!");
			}

			Assert.True(released, "The SKBitmapReleaseDelegate was not called.");
		}

		[SkippableFact]
		public void ImageCreateDoesNotThrow()
		{
			var info = new SKImageInfo(1, 1);
			using (var image = SKImage.Create(info))
			{
				Assert.False(image.IsTextureBacked);
				Assert.Equal(image, image.ToRasterImage());
			}
		}

		[SkippableFact]
		public void ReleaseBitmapPixelsWithNullDelegate()
		{
			var info = new SKImageInfo(1, 1);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var bitmap = new SKBitmap())
			{
				bitmap.InstallPixels(info, pixels, info.RowBytes);
			}

			Marshal.FreeCoTaskMem(pixels);
		}

		[SkippableFact]
		public void TestExtractAlpha()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var bitmap = SKBitmap.Decode(path);

			var alpha = new SKBitmap();

			SKPointI offset;
			SKPaint paint = new SKPaint
			{
				MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5.0f)
			};

			Assert.True(bitmap.ExtractAlpha(alpha, paint, out offset));

			Assert.Equal(new SKPointI(-12, -12), offset);
		}

		[SkippableFact]
		public void TestBitmapDecodeDrawsCorrectly()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");

			using (var bitmap = SKBitmap.Decode(path))
			using (var surface = SKSurface.Create(new SKImageInfo(200, 200)))
			{
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.White);
				canvas.DrawBitmap(bitmap, 0, 0);

				using (var img = surface.Snapshot())
				using (var bmp = SKBitmap.FromImage(img))
				{
					Assert.Equal(new SKColor(2, 255, 42), bmp.GetPixel(20, 20));
					Assert.Equal(new SKColor(1, 83, 255), bmp.GetPixel(108, 20));
					Assert.Equal(new SKColor(255, 166, 1), bmp.GetPixel(20, 108));
					Assert.Equal(new SKColor(255, 1, 214), bmp.GetPixel(108, 108));
				}
			}
		}

		[SkippableFact]
		public void BitmapAndPixmapAreValid()
		{
			var info = new SKImageInfo(10, 10);
			using (var bitmap = new SKBitmap(info))
			{
				Assert.Equal(10, bitmap.Width);
				Assert.Equal(10, bitmap.Height);

				var pixmap = bitmap.PeekPixels();
				Assert.NotNull(pixmap);

				Assert.Equal(10, pixmap.Width);
				Assert.Equal(10, pixmap.Height);

				Assert.True(bitmap.GetPixels() != IntPtr.Zero);
				Assert.True(pixmap.GetPixels() != IntPtr.Zero);
				Assert.Equal(bitmap.GetPixels(), pixmap.GetPixels());
			}
		}


		public static IEnumerable<object[]> GetSamplingData()
		{
			yield return new object[] { new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None) };
			yield return new object[] { new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None) };
			yield return new object[] { new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear) };
			yield return new object[] { new SKSamplingOptions(SKCubicResampler.CatmullRom) };
			yield return new object[] { new SKSamplingOptions(SKCubicResampler.Mitchell) };
		}

		[SkippableTheory]
		[MemberData(nameof(GetSamplingData))]
		public void BitmapResizes(SKSamplingOptions sampling)
		{
			var srcInfo = new SKImageInfo(200, 200);
			var dstInfo = new SKImageInfo(100, 100);

			var srcBmp = new SKBitmap(srcInfo);

			using (var canvas = new SKCanvas(srcBmp))
			using (var paint = new SKPaint { Color = SKColors.Green })
			{
				canvas.Clear(SKColors.Blue);
				canvas.DrawRect(new SKRect(0, 0, 100, 200), paint);
			}

			Assert.Equal(SKColors.Green, srcBmp.GetPixel(75, 75));
			Assert.Equal(SKColors.Blue, srcBmp.GetPixel(175, 175));

			var dstBmp = srcBmp.Resize(dstInfo, sampling);
			Assert.NotNull(dstBmp);

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[SkippableTheory]
		[InlineData(-1, -1)]
		[InlineData(0, 0)]
		[InlineData(-1, 10)]
		[InlineData(10, -1)]
		[InlineData(0, 10)]
		[InlineData(10, 0)]
		public void BitmapDoesNotCrashOnInvalidResizes(int width, int hight)
		{
			using var bitmap = CreateTestBitmap();

			var newInfo = bitmap.Info;
			newInfo.Width = width;
			newInfo.Height = hight;

			using var newBitmap = bitmap.Resize(newInfo, new SKSamplingOptions(SKCubicResampler.Mitchell));

			Assert.Null(newBitmap);
		}

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

			Assert.True(srcBmp.ScalePixels(dstBmp, new SKSamplingOptions(SKCubicResampler.Mitchell)));

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[SkippableFact]
		public void SwizzleRedBlueTest()
		{
			var info = new SKImageInfo(1, 1);

			using (var bmp = new SKBitmap(info))
			{
				bmp.Erase((uint)0xFACEB004);

				Assert.Equal((uint)0xFACEB004, (uint)bmp.GetPixel(0, 0));

				SKSwizzle.SwapRedBlue(bmp.GetPixels(), bmp.GetPixels(), 1);

				Assert.Equal((uint)0xFA04B0CE, (uint)bmp.GetPixel(0, 0));
			}
		}

		[SkippableFact]
		public void SupportsNonASCIICharactersInPath()
		{
			var fileName = Path.Combine(PathToImages, "上田雅美.jpg");

			using (var bitmap = SKBitmap.Decode(fileName))
			{
				Assert.NotNull(bitmap);
			}
		}

		[SkippableFact]
		public void CanCreateUsingRowBytes()
		{
			using var src = CreateTestBitmap();

			var xOffset = 19 * src.BytesPerPixel;
			var yOffset = 19 * src.RowBytes;
			var offset = yOffset + xOffset;

			using var bmp = new SKBitmap();
			bmp.InstallPixels(new SKImageInfo(2, 2), src.GetPixels() + offset, src.RowBytes);

			Assert.Equal(2, bmp.Width);
			Assert.Equal(2, bmp.Height);

			Assert.Equal(SKColors.Red, bmp.GetPixel(0, 0));
			Assert.Equal(SKColors.Green, bmp.GetPixel(1, 0));
			Assert.Equal(SKColors.Blue, bmp.GetPixel(0, 1));
			Assert.Equal(SKColors.Yellow, bmp.GetPixel(1, 1));
		}

		[SkippableFact]
		public void PixelsPropertyReadsTheColors()
		{
			using var bitmap = CreateTestBitmap();
			var pixels = bitmap.Pixels;

			for (var i = 0; i < pixels.Length; i++)
			{
				var x = i % 40;
				var y = i / 40;

				if (x < 20)
				{
					if (y < 20)
						Assert.Equal(SKColors.Red, pixels[i]);
					else
						Assert.Equal(SKColors.Blue, pixels[i]);
				}
				else
				{
					if (y < 20)
						Assert.Equal(SKColors.Green, pixels[i]);
					else
						Assert.Equal(SKColors.Yellow, pixels[i]);
				}
			}
		}

		[SkippableFact]
		public void IncorrectPixelSizeThrowsWhenWriting()
		{
			SKColor[] sourcePixels;
			using (var sourceBitmap = CreateTestBitmap())
			{
				sourcePixels = sourceBitmap.Pixels;
			}

			using var bitmap = new SKBitmap(20, 40);
			var ex = Assert.Throws<ArgumentException>(() => bitmap.Pixels = sourcePixels);
			Assert.Equal("value", ex.ParamName);
			Assert.Contains("800", ex.Message);
		}

		[SkippableFact]
		public void PixelsPropertyWritesTheColors()
		{
			SKColor[] sourcePixels;
			using (var sourceBitmap = CreateTestBitmap())
			{
				sourcePixels = sourceBitmap.Pixels;
			}

			using var bitmap = new SKBitmap(40, 40);
			bitmap.Pixels = sourcePixels;
			var pixels = bitmap.Pixels;

			Assert.Equal(sourcePixels, pixels);
		}

		[SkippableFact]
		public void IncorrectPixelLocationThrowsWhenWritingPixel()
		{
			using var bitmap = new SKBitmap(20, 40);
			var ex = Assert.Throws<ArgumentOutOfRangeException>(() => bitmap.SetPixel(10, 100, SKColors.Red));
			Assert.Equal("y", ex.ParamName);
		}

		[SkippableFact]
		public void SetPixelWritesTheColor()
		{
			SKColor[] expectedPixels =
			{
				SKColors.Red, SKColors.Red, SKColors.Red, SKColors.Red,
				SKColors.Red, SKColors.Red, SKColors.Green, SKColors.Red,
				SKColors.Red, SKColors.Blue, SKColors.Red, SKColors.Red,
				SKColors.Red, SKColors.Red, SKColors.Red, SKColors.Red
			};

			using var bitmap = new SKBitmap(4, 4);
			bitmap.Erase(SKColors.Red);

			bitmap.SetPixel(1, 1, SKColors.Red);
			bitmap.SetPixel(2, 1, SKColors.Green);
			bitmap.SetPixel(1, 2, SKColors.Blue);

			Assert.Equal(expectedPixels, bitmap.Pixels);
		}

		[SkippableTheory]
		[InlineData("osm-liberty.png")]
		[InlineData("testimage.png")]
		public void CanDecodePotentiallyCorruptPngFiles(string filename)
		{
			var path = Path.Combine(PathToImages, filename);

			var bytes = File.ReadAllBytes(path);
			using var data = SKData.CreateCopy(bytes);
			using var bitmap = SKBitmap.Decode(data);

			Assert.NotNull(bitmap);
		}

		[Theory]
		[InlineData("tomato.bmp")]
		[InlineData("baboon.jpg")]
		[InlineData("baboon.png")]
		[InlineData("animated-heart.gif")]
		public void CanDecodeImageStreams(string filename)
		{
			var path = Path.Combine(PathToImages, filename);

			using var stream = File.OpenRead(path);
			using var bitmap = SKBitmap.Decode(stream);

			Assert.NotNull(bitmap);
		}

		[SkippableFact]
		public void GetPixelSpanHasCorrectLength()
		{
			using var bmp = CreateTestBitmap();

			var totalBytes = bmp.Info.BytesSize;

			Assert.Equal(totalBytes, bmp.GetPixelSpan().Length);
		}

		[SkippableTheory]
		[InlineData(0, 0, 40 * 40 * 4)]
		[InlineData(0, 20, 40 * 20 * 4)]
		[InlineData(39, 39, 4)]
		public void GetPixelSpanXYHasCorrectLength(int x, int y, int expectedLength)
		{
			using var bmp = CreateTestBitmap();
			var span = bmp.GetPixelSpan(x, y);
			Assert.Equal(expectedLength, span.Length);
		}

		[SkippableTheory]
		[InlineData("baboon.jpg", "baboon-reencoded.jpg")]
		public void CanEncodeImageStreams(string filename, string encodedFilename)
		{
			var path = Path.Combine(PathToImages, filename);
			using var stream = File.OpenRead(path);
			using var bitmap = SKBitmap.Decode(stream);

			using var ouputStream = new MemoryStream();
			bitmap.Encode(ouputStream, SKEncodedImageFormat.Jpeg, 100);
			ouputStream.Position = 0;
			using var outputBitmp = SKBitmap.Decode(ouputStream);

			var encodedPath = Path.Combine(PathToImages, encodedFilename);
			using var encodedBitmap = SKBitmap.Decode(encodedPath);

			AssertSimilar(encodedBitmap, outputBitmp);
		}
	}
}
