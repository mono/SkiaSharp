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
	}
}
