using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKBitmapTest : SKTest
	{
		[SkippableFact]
		public void BitmapCanCopyIsCorrect()
		{
			var bmp = CreateTestBitmap();

			Assert.True(bmp.CanCopyTo(SKColorType.Alpha8));
			Assert.True(bmp.CanCopyTo(SKColorType.Rgb565));
			Assert.True(bmp.CanCopyTo(SKColorType.Argb4444));
			Assert.True(bmp.CanCopyTo(SKColorType.Bgra8888));
			Assert.True(bmp.CanCopyTo(SKColorType.Rgba8888));
			Assert.True(bmp.CanCopyTo(SKColorType.RgbaF16));

			Assert.False(bmp.CanCopyTo(SKColorType.Unknown));
			Assert.False(bmp.CanCopyTo(SKColorType.Gray8));
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

			bmp = CreateTestBitmap(127);

			alpha8 = bmp.Copy(SKColorType.Alpha8);
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
			Assert.Equal((SKColor)0xff007700, argb4444.GetPixel(30, 10));
			Assert.Equal((SKColor)0xff0000ff, argb4444.GetPixel(10, 30));
			Assert.Equal((SKColor)0xffffff00, argb4444.GetPixel(30, 30));
			Assert.Equal(SKColorType.Argb4444, argb4444.ColorType);

			bmp = CreateTestBitmap(127);

			argb4444 = bmp.Copy(SKColorType.Argb4444);
			Assert.Equal((SKColor)0x77ff0000, argb4444.GetPixel(10, 10));
			Assert.Equal((SKColor)0x77006d00, argb4444.GetPixel(30, 10));
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

			bmp = CreateTestBitmap(127);

			rgb565 = bmp.Copy(SKColorType.Rgb565);
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
			Assert.Equal((SKColor)0xff003700, rgbaF16.GetPixel(30, 10));
			Assert.Equal((SKColor)0xff0000ff, rgbaF16.GetPixel(10, 30));
			Assert.Equal((SKColor)0xffffff00, rgbaF16.GetPixel(30, 30));
			Assert.Equal(SKColorType.RgbaF16, rgbaF16.ColorType);

			bmp = CreateTestBitmap(127);

			rgbaF16 = bmp.Copy(SKColorType.RgbaF16);
			Assert.Equal((SKColor)0x7f6d0000, rgbaF16.GetPixel(10, 10));
			Assert.Equal((SKColor)0x7f001a00, rgbaF16.GetPixel(30, 10));
			Assert.Equal((SKColor)0x7f00006d, rgbaF16.GetPixel(10, 30));
			Assert.Equal((SKColor)0x7f6d6d00, rgbaF16.GetPixel(30, 30));
			Assert.Equal(SKColorType.RgbaF16, rgbaF16.ColorType);
		}

		[SkippableFact]
		public void BitmapCopyToInvalidIsNull()
		{
			var bmp = CreateTestBitmap();

			ValidateTestBitmap(bmp);
			ValidateTestBitmap(bmp.Copy(SKImageInfo.PlatformColorType));

			Assert.Null(bmp.Copy(SKColorType.Unknown));
			Assert.Null(bmp.Copy(SKColorType.Gray8));

			// alpha to non-alpha is not supported
			Assert.Null(bmp
				.Copy(SKColorType.Alpha8)
				.Copy(SKImageInfo.PlatformColorType));
		}

		[SkippableFact]
		public void ReleaseBitmapPixelsWasInvoked()
		{
			bool released = false;

			var onRelease = new SKBitmapReleaseDelegate((addr, ctx) => {
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.Equal("RELEASING!", ctx);
			});

			using (var bitmap = new SKBitmap()) {
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
			using (var image = SKImage.Create(info)) {
				Assert.False(image.IsTextureBacked);
				Assert.Equal(image, image.ToRasterImage());
			}
		}

		[SkippableFact]
		public void ReleaseBitmapPixelsWithNullDelegate()
		{
			var info = new SKImageInfo(1, 1);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var bitmap = new SKBitmap()) {
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
			SKPaint paint = new SKPaint {
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
			using (var surface = SKSurface.Create(new SKImageInfo(200, 200))) {
				var canvas = surface.Canvas;
				canvas.Clear(SKColors.White);
				canvas.DrawBitmap(bitmap, 0, 0);

				using (var img = surface.Snapshot())
				using (var bmp = SKBitmap.FromImage(img)) {
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
			using (var bitmap = new SKBitmap(info)) {
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

		[Obsolete]
		[SkippableFact]
		public void BitmapResizesObsolete()
		{
			var srcInfo = new SKImageInfo(200, 200);
			var dstInfo = new SKImageInfo(100, 100);

			var srcBmp = new SKBitmap(srcInfo);

			using (var canvas = new SKCanvas(srcBmp))
			using (var paint = new SKPaint { Color = SKColors.Green }) {
				canvas.Clear(SKColors.Blue);
				canvas.DrawRect(new SKRect(0, 0, 100, 200), paint);
			}

			Assert.Equal(SKColors.Green, srcBmp.GetPixel(75, 75));
			Assert.Equal(SKColors.Blue, srcBmp.GetPixel(175, 175));

			var dstBmp = srcBmp.Resize(dstInfo, SKBitmapResizeMethod.Mitchell);
			Assert.NotNull(dstBmp);

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[SkippableFact]
		public void BitmapResizes()
		{
			var srcInfo = new SKImageInfo(200, 200);
			var dstInfo = new SKImageInfo(100, 100);

			var srcBmp = new SKBitmap(srcInfo);

			using (var canvas = new SKCanvas(srcBmp))
			using (var paint = new SKPaint { Color = SKColors.Green }) {
				canvas.Clear(SKColors.Blue);
				canvas.DrawRect(new SKRect(0, 0, 100, 200), paint);
			}

			Assert.Equal(SKColors.Green, srcBmp.GetPixel(75, 75));
			Assert.Equal(SKColors.Blue, srcBmp.GetPixel(175, 175));

			var dstBmp = srcBmp.Resize(dstInfo, SKFilterQuality.High);
			Assert.NotNull(dstBmp);

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
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

			Assert.True(srcBmp.ScalePixels(dstBmp, SKFilterQuality.High));

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[SkippableFact]
		public void AlphaMaskIsApplied()
		{
			var srcInfo = new SKImageInfo(4, 4);
			var srcBmp = new SKBitmap(srcInfo);
			srcBmp.Erase(SKColors.Red);
			var pixels = srcBmp.Pixels;

			foreach (var pixel in pixels)
			{
				Assert.Equal(255, pixel.Alpha);
			}
			
			var maskBuffer = new byte[]
			{
				128, 127, 126, 125,
				101, 102, 103, 104,
				96, 95, 94, 93,
				72, 73, 74, 75
			};
			var bounds = new SKRectI(0, 0, 4, 4);
			uint rowBytes = 4;
			var format = SKMaskFormat.A8;
			var mask = SKMask.Create(maskBuffer, bounds, rowBytes, format);

			srcBmp.InstallMaskPixels(mask);

			pixels = srcBmp.Pixels;
			Assert.Equal(128, pixels[0].Alpha);
			Assert.Equal(127, pixels[1].Alpha);
			Assert.Equal(126, pixels[2].Alpha);
			Assert.Equal(125, pixels[3].Alpha);
			Assert.Equal(101, pixels[4].Alpha);
			Assert.Equal(102, pixels[5].Alpha);
			Assert.Equal(103, pixels[6].Alpha);
			Assert.Equal(104, pixels[7].Alpha);
			Assert.Equal(96, pixels[8].Alpha);
			Assert.Equal(95, pixels[9].Alpha);
			Assert.Equal(94, pixels[10].Alpha);
			Assert.Equal(93, pixels[11].Alpha);
			Assert.Equal(72, pixels[12].Alpha);
			Assert.Equal(73, pixels[13].Alpha);
			Assert.Equal(74, pixels[14].Alpha);
			Assert.Equal(75, pixels[15].Alpha);

			mask.FreeImage();
		}

		[Obsolete]
		[SkippableFact(Skip = "This test takes a long time (~3mins), so ignore this most of the time.")]
		public static void ImageScalingMultipleThreadsTest()
		{
			const int numThreads = 100;
			const int numIterationsPerThread = 1000;

			var referenceFile = Path.Combine(PathToImages, "baboon.jpg");

			var tasks = new List<Task>();

			for (int i = 0; i < numThreads; i++)
			{
				var task = Task.Run(() =>
				{
					for (int j = 0; j < numIterationsPerThread; j++)
					{
						var imageData = ComputeThumbnail(referenceFile);
					}
				});
				tasks.Add(task);
			}

			Task.WaitAll(tasks.ToArray());

			Console.WriteLine($"Test completed for {numThreads} tasks, {numIterationsPerThread} each.");
		}

		[Obsolete]
		private static byte[] ComputeThumbnail(string fileName)
		{
			using (var ms = new MemoryStream())
			using (var bitmap = SKBitmap.Decode(fileName))
			using (var scaledBitmap = new SKBitmap(60, 40, bitmap.ColorType, bitmap.AlphaType))
			{
				SKBitmap.Resize(scaledBitmap, bitmap, SKBitmapResizeMethod.Hamming);

				using (var image = SKImage.FromBitmap(scaledBitmap))
				using (var data = image.Encode(SKEncodedImageFormat.Png, 80))
				{
					data.SaveTo(ms);

					return ms.ToArray();
				}
			}
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
	}
}
