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
	}
}
