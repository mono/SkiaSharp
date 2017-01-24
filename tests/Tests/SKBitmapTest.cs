using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKBitmapTest : SKTest
	{
		[Fact]
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

			Assert.True(released, "The SKBitmapReleaseDelegate was not called.");
		}

		[Fact]
		public void ReleaseBitmapPixelsWithNullDelegate()
		{
			var info = new SKImageInfo(1, 1);
			var pixels = Marshal.AllocCoTaskMem(info.BytesSize);

			using (var bitmap = new SKBitmap()) {
				bitmap.InstallPixels(info, pixels, info.RowBytes);
			}

			Marshal.FreeCoTaskMem(pixels);
		}

		[Fact]
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

		[Fact]
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

			var dstBmp = srcBmp.Resize(dstInfo, SKBitmapResizeMethod.Mitchell);
			Assert.NotNull(dstBmp);

			Assert.Equal(SKColors.Green, dstBmp.GetPixel(25, 25));
			Assert.Equal(SKColors.Blue, dstBmp.GetPixel(75, 75));
		}

		[Fact]
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
	}
}
