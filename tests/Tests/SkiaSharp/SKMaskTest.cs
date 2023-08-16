using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKMaskTest : SKTest
	{
		[SkippableFact]
		public unsafe void FixedImageMaskIsHandledCorrectly()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.Equal(rawMask, mask.GetAddr1(0, 0));

			mask.FreeImage();
		}

		[SkippableFact]
		public void MonochromeMaskBufferIsCopied()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.Equal(rawMask, mask.GetAddr1(0, 0));

			mask.FreeImage();
		}

		[SkippableFact]
		public void AutoMaskFreeImageReleasesMemory()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			var mask = new SKMask(bounds, rowBytes, format);

			var size = mask.ComputeTotalImageSize();
			mask.Image = SKMask.AllocateImage(size);

			Marshal.Copy(buffer, 0, mask.Image, (int)size);

			using (new SKAutoMaskFreeImage(mask.Image))
			{
				Assert.Equal(rawMask, mask.GetAddr1(0, 0));
			}
		}

		[SkippableFact]
		public void Alpha8MaskBufferIsCopied()
		{
			var buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
			var bounds = new SKRectI(0, 0, 4, 2);
			UInt32 rowBytes = 4;
			var format = SKMaskFormat.A8;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.Equal(buffer[0], mask.GetAddr8(0, 0));
			Assert.Equal(buffer[1], mask.GetAddr8(1, 0));
			Assert.Equal(buffer[2], mask.GetAddr8(2, 0));
			Assert.Equal(buffer[3], mask.GetAddr8(3, 0));
			Assert.Equal(buffer[4], mask.GetAddr8(0, 1));
			Assert.Equal(buffer[5], mask.GetAddr8(1, 1));
			Assert.Equal(buffer[6], mask.GetAddr8(2, 1));
			Assert.Equal(buffer[7], mask.GetAddr8(3, 1));

			mask.FreeImage();
		}

		[SkippableFact]
		public void ThirtyTwoBitMaskBufferIsCopied()
		{
			var buffer = new byte[]
			{
				0, 0, 255, 255,
				255, 0, 0, 255,
				0, 0, 255, 255,
				255, 0, 0, 255,
				0, 0, 255, 255,
				255, 0, 0, 255,
				0, 0, 255, 255,
				255, 0, 0, 255
			};
			var bounds = new SKRectI(0, 0, 4, 2);
			UInt32 rowBytes = 16;
			var format = SKMaskFormat.Argb32;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			var red = SKColors.Red;
			var blue = SKColors.Blue;
			Assert.Equal((uint)red, mask.GetAddr32(0, 0));
			Assert.Equal((uint)blue, mask.GetAddr32(1, 0));
			Assert.Equal((uint)red, mask.GetAddr32(2, 0));
			Assert.Equal((uint)blue, mask.GetAddr32(3, 0));
			Assert.Equal((uint)red, mask.GetAddr32(0, 1));
			Assert.Equal((uint)blue, mask.GetAddr32(1, 1));
			Assert.Equal((uint)red, mask.GetAddr32(2, 1));
			Assert.Equal((uint)blue, mask.GetAddr32(3, 1));

			mask.FreeImage();
		}
	}
}
