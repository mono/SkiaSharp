using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKBitmapTest : SKTest
	{
		[Test]
		public void ReleaseBitmapPixelsWasInvoked()
		{
			bool released = false;

			var onRelease = new SKBitmapReleaseDelegate((addr, ctx) => {
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.AreEqual("RELEASING!", ctx);
			});

			using (var bitmap = new SKBitmap()) {
				var info = new SKImageInfo(1, 1);
				var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

				bitmap.InstallPixels(info, pixels, info.RowBytes, null, onRelease, "RELEASING!");
			}

			Assert.True(released, "The SKBitmapReleaseDelegate was not called.");
		}

		[Test]
		public void ImageCreateDoesNotThrow()
		{
			var info = new SKImageInfo(1, 1);
			using (var image = SKImage.Create(info)) {
				Assert.IsFalse(image.IsTextureBacked);
				Assert.AreEqual(image, image.ToRasterImage());
			}
		}

		[Test]
		public void ReleaseImagePixelsWasInvoked()
		{
			bool released = false;

			var onRelease = new SKImageRasterReleaseDelegate((addr, ctx) => {
				Marshal.FreeCoTaskMem(addr);
				released = true;
				Assert.AreEqual("RELEASING!", ctx);
			});

			var info = new SKImageInfo(1, 1);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var pixmap = new SKPixmap(info, pixels))
			using (var image = SKImage.FromPixels(pixmap, onRelease, "RELEASING!")) {
				Assert.IsFalse(image.IsTextureBacked);
				using (var raster = image.ToRasterImage()) {
					Assert.AreEqual(image, raster);
				}
				Assert.False(released, "The SKImageRasterReleaseDelegate was called too soon.");
			}

			Assert.True(released, "The SKImageRasterReleaseDelegate was not called.");
		}

		[Test]
		public void ReleaseBitmapPixelsWithNullDelegate()
		{
			var info = new SKImageInfo(1, 1);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var bitmap = new SKBitmap()) {
				bitmap.InstallPixels(info, pixels, info.RowBytes);
			}

			Marshal.FreeCoTaskMem(pixels);
		}

		[Test]
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

			Assert.AreEqual(new SKPointI(-7, -7), offset);
		}

		[Test]
		public void BitmapAndPixmapAreValid()
		{
			var info = new SKImageInfo(10, 10);
			using (var bitmap = new SKBitmap(info)) {
				Assert.AreEqual(10, bitmap.Width);
				Assert.AreEqual(10, bitmap.Height);

				var pixmap = bitmap.PeekPixels();
				Assert.NotNull(pixmap);

				Assert.AreEqual(10, pixmap.Width);
				Assert.AreEqual(10, pixmap.Height);

				Assert.True(bitmap.GetPixels() != IntPtr.Zero);
				Assert.True(pixmap.GetPixels() != IntPtr.Zero);
				Assert.AreEqual(bitmap.GetPixels(), pixmap.GetPixels());
			}
		}

		[Test]
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

			Assert.AreEqual(SKColors.Green, srcBmp.GetPixel(75, 75));
			Assert.AreEqual(SKColors.Blue, srcBmp.GetPixel(175, 175));

			var dstBmp = srcBmp.Resize(dstInfo, SKBitmapResizeMethod.Mitchell);
			Assert.NotNull(dstBmp);

			Assert.AreEqual(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.AreEqual(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[Test]
		public void AlphaMaskIsApplied()
		{
			var srcInfo = new SKImageInfo(4, 4);
			var srcBmp = new SKBitmap(srcInfo);
			srcBmp.Erase(SKColors.Red);
			var pixels = srcBmp.Pixels;

			foreach (var pixel in pixels)
			{
				Assert.AreEqual(255, pixel.Alpha);
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
			Assert.AreEqual(128, pixels[0].Alpha);
			Assert.AreEqual(127, pixels[1].Alpha);
			Assert.AreEqual(126, pixels[2].Alpha);
			Assert.AreEqual(125, pixels[3].Alpha);
			Assert.AreEqual(101, pixels[4].Alpha);
			Assert.AreEqual(102, pixels[5].Alpha);
			Assert.AreEqual(103, pixels[6].Alpha);
			Assert.AreEqual(104, pixels[7].Alpha);
			Assert.AreEqual(96, pixels[8].Alpha);
			Assert.AreEqual(95, pixels[9].Alpha);
			Assert.AreEqual(94, pixels[10].Alpha);
			Assert.AreEqual(93, pixels[11].Alpha);
			Assert.AreEqual(72, pixels[12].Alpha);
			Assert.AreEqual(73, pixels[13].Alpha);
			Assert.AreEqual(74, pixels[14].Alpha);
			Assert.AreEqual(75, pixels[15].Alpha);

			mask.FreeImage();
		}

		[Test]
		[Ignore("This test takes a long time (~3mins), so ignore this most of the time.")]
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

	}
}
