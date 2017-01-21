using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKBasicTypesTest : SKTest
	{
		private const float EPSILON = 0.0001f;

		[Fact]
		public void RectanlgeHasCorrectProperties()
		{
			var rect = new SKRect(15, 25, 55, 75);

			Assert.Equal(15f, rect.Left);
			Assert.Equal(25f, rect.Top);
			Assert.Equal(55f, rect.Right);
			Assert.Equal(75f, rect.Bottom);

			Assert.Equal(40f, rect.Width);
			Assert.Equal(50f, rect.Height);

			Assert.Equal(35f, rect.MidX);
			Assert.Equal(50f, rect.MidY);
		}

		[Fact]
		public void RectanlgeOffsetsCorrectly()
		{
			var expected = new SKRect(25, 30, 65, 80);

			var rect1 = new SKRect(15, 25, 55, 75);
			rect1.Location = new SKPoint(25, 30);

			var rect2 = new SKRect(15, 25, 55, 75);
			rect2.Offset (10, 5);

			Assert.Equal(expected, rect1);
			Assert.Equal(expected, rect2);
		}

		[Fact]
		public void RectanlgeInflatesCorrectly()
		{
			var rect = new SKRect(15, 25, 55, 75);

			Assert.Equal(15f, rect.Left);
			Assert.Equal(25f, rect.Top);
			Assert.Equal(55f, rect.Right);
			Assert.Equal(75f, rect.Bottom);

			rect.Inflate(10, 20);

			Assert.Equal(5f, rect.Left);
			Assert.Equal(5f, rect.Top);
			Assert.Equal(65f, rect.Right);
			Assert.Equal(95f, rect.Bottom);
		}

		[Fact]
		public void RectanlgeStandardizeCorrectly()
		{
			var rect = new SKRect(5, 5, 15, 15);
			Assert.Equal(10, rect.Width);
			Assert.Equal(10, rect.Height);

			Assert.Equal(rect, rect.Standardized);

			var negW = new SKRect(15, 5, 5, 15);
			Assert.Equal(-10, negW.Width);
			Assert.Equal(10, negW.Height);
			Assert.Equal(rect, negW.Standardized);

			var negH = new SKRect(5, 15, 15, 5);
			Assert.Equal(10, negH.Width);
			Assert.Equal(-10, negH.Height);
			Assert.Equal(rect, negH.Standardized);

			var negWH = new SKRect(15, 15, 5, 5);
			Assert.Equal(-10, negWH.Width);
			Assert.Equal(-10, negWH.Height);
			Assert.Equal(rect, negWH.Standardized);
		}

		[Fact]
		public void RectanlgeAspectFitIsCorrect()
		{
			var bigRect = SKRect.Create(5, 5, 20, 20);
			var tallSize = new SKSize(5, 10);
			var wideSize = new SKSize(10, 5);

			var fitTall = bigRect.AspectFit(tallSize);
			Assert.Equal(5 + 5, fitTall.Left);
			Assert.Equal(5 + 0, fitTall.Top);
			Assert.Equal(10, fitTall.Width);
			Assert.Equal(20, fitTall.Height);

			var fitWide = bigRect.AspectFit(wideSize);
			Assert.Equal(5 + 0, fitWide.Left);
			Assert.Equal(5 + 5, fitWide.Top);
			Assert.Equal(20, fitWide.Width);
			Assert.Equal(10, fitWide.Height);
		}

		[Fact]
		public void RectanlgeAspectFillIsCorrect()
		{
			var bigRect = SKRect.Create(5, 5, 20, 20);
			var tallSize = new SKSize(5, 10);
			var wideSize = new SKSize(10, 5);

			var fitTall = bigRect.AspectFill(tallSize);
			Assert.Equal(5 + 0, fitTall.Left);
			Assert.Equal(5 - 10, fitTall.Top);
			Assert.Equal(20, fitTall.Width);
			Assert.Equal(40, fitTall.Height);

			var fitWide = bigRect.AspectFill(wideSize);
			Assert.Equal(5 - 10, fitWide.Left);
			Assert.Equal(5 + 0, fitWide.Top);
			Assert.Equal(40, fitWide.Width);
			Assert.Equal(20, fitWide.Height);
		}
		
		[Fact]
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

				Assert.Equal(rawMask, mask.GetAddr1(0, 0));
			}
		}

		[Fact]
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

		[Fact]
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

		[Fact]
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
