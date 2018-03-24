using System;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPixmapTest : SKTest
	{
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

			var copy = pixmap.WithColorType(SKColorType.Index8);

			Assert.Equal(SKColorType.Rgb565, pixmap.ColorType);
			Assert.Equal((IntPtr)123, pixmap.GetPixels());
			Assert.Equal(SKColorType.Index8, copy.ColorType);
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

				SKSwizzle.SwapRedBlue(bmp.GetPixels(out var length), (int)length);

				Assert.Equal(SKColors.Blue, bmp.Pixels[0]);
			}
		}
	}
}
