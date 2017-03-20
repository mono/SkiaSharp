using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKMaskTest : SKTest
	{
		private const float EPSILON = 0.0001f;
		private const int PRECISION = 4;

		[Test]
		public unsafe void FixedImageMaskIsHandledCorrectly()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			fixed (void* bufferPtr = buffer)
			{
				var mask = SKMask.Create(buffer, bounds, rowBytes, format);

				Assert.AreEqual(rawMask, mask.GetAddr1(0, 0));
			}
		}

		[Test]
		public void MonochromeMaskBufferIsCopied()
		{
			byte rawMask = 1 << 7 | 0 << 6 | 0 << 5 | 1 << 4 | 1 << 3 | 0 << 2 | 1 << 1 | 1;
			var buffer = new byte[] { rawMask };
			var bounds = new SKRectI(0, 0, 8, 1);
			UInt32 rowBytes = 1;
			var format = SKMaskFormat.BW;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.AreEqual(rawMask, mask.GetAddr1(0, 0));

			mask.FreeImage();
		}

		[Test]
		public void Alpha8MaskBufferIsCopied()
		{
			var buffer = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
			var bounds = new SKRectI(0, 0, 4, 2);
			UInt32 rowBytes = 4;
			var format = SKMaskFormat.A8;

			var mask = SKMask.Create(buffer, bounds, rowBytes, format);

			Assert.AreEqual(buffer[0], mask.GetAddr8(0, 0));
			Assert.AreEqual(buffer[1], mask.GetAddr8(1, 0));
			Assert.AreEqual(buffer[2], mask.GetAddr8(2, 0));
			Assert.AreEqual(buffer[3], mask.GetAddr8(3, 0));
			Assert.AreEqual(buffer[4], mask.GetAddr8(0, 1));
			Assert.AreEqual(buffer[5], mask.GetAddr8(1, 1));
			Assert.AreEqual(buffer[6], mask.GetAddr8(2, 1));
			Assert.AreEqual(buffer[7], mask.GetAddr8(3, 1));

			mask.FreeImage();
		}

		[Test]
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
			Assert.AreEqual((uint)red, mask.GetAddr32(0, 0));
			Assert.AreEqual((uint)blue, mask.GetAddr32(1, 0));
			Assert.AreEqual((uint)red, mask.GetAddr32(2, 0));
			Assert.AreEqual((uint)blue, mask.GetAddr32(3, 0));
			Assert.AreEqual((uint)red, mask.GetAddr32(0, 1));
			Assert.AreEqual((uint)blue, mask.GetAddr32(1, 1));
			Assert.AreEqual((uint)red, mask.GetAddr32(2, 1));
			Assert.AreEqual((uint)blue, mask.GetAddr32(3, 1));

			mask.FreeImage();
		}
	}
}
