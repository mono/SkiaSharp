using System.Runtime.InteropServices;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKPixmapTest : SKTest
	{
		[Test]
		public void ReadPixelSucceeds()
		{
			var info = new SKImageInfo(10, 10);

			var ptr1 = Marshal.AllocCoTaskMem(info.BytesSize);
			var pix1 = new SKPixmap(info, ptr1);

			var ptr2 = Marshal.AllocCoTaskMem(info.BytesSize);

			var result = pix1.ReadPixels(info, ptr2, info.RowBytes);

			Assert.True(result);
		}

		[Test]
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

				Assert.AreNotEqual(Marshal.ReadInt64(pix1.GetPixels()), Marshal.ReadInt64(pix2.GetPixels()));

				var result = pix1.ReadPixels(pix2);

				Assert.True(result);

				Assert.AreEqual(Marshal.ReadInt64(pix1.GetPixels()), Marshal.ReadInt64(pix2.GetPixels()));
			}
		}
	}
}
