using System;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKBitmapTest : SKTest
	{
		[Test]
		public void ReleaseBitmapPixelsWasInvoked()
		{
			bool released = false;

			var onRelease = new SKBitmapReleaseDelegate((addr, ctx) => {
				Marshal.FreeCoTaskMem(addr);
				released = true;
			});

			using (var bitmap = new SKBitmap()) {
				var info = new SKImageInfo(1, 1);
				var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

				bitmap.InstallPixels(info, pixels, info.RowBytes, null, onRelease, "RELEASING!");
			}

			Assert.IsTrue(released, "The SKBitmapReleaseDelegate was not called.");
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
		public void BitmapAndPixmapAreValid()
		{
			var info = new SKImageInfo(10, 10);
			using (var bitmap = new SKBitmap(info)) {
				Assert.AreEqual(10, bitmap.Width);
				Assert.AreEqual(10, bitmap.Height);

				var pixmap = bitmap.PeekPixels();
				Assert.IsNotNull(pixmap);

				Assert.AreEqual(10, pixmap.Width);
				Assert.AreEqual(10, pixmap.Height);

				Assert.IsTrue(bitmap.GetPixels() != IntPtr.Zero);
				Assert.IsTrue(pixmap.GetPixels() != IntPtr.Zero);
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
			Assert.IsNotNull(dstBmp);

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
			var mask = new SKMask(maskBuffer, bounds, rowBytes, format);

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
		}
	}
}
