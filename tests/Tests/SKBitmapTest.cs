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
	}
}
