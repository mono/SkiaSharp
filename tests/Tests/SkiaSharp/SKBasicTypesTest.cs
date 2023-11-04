using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKImageInfoTest : SKTest
	{
		[SkippableFact]
		public void MethodsDoNotModifySource()
		{
			var info = new SKImageInfo(100, 30, SKColorType.Rgb565, SKAlphaType.Unpremul);

			Assert.Equal(SKColorType.Rgb565, info.ColorType);

			var copy = info.WithColorType(SKColorType.Gray8);

			Assert.Equal(SKColorType.Rgb565, info.ColorType);
			Assert.Equal(SKColorType.Gray8, copy.ColorType);
		}
	}

	public class SKRectTest : SKTest
	{
		[SkippableFact]
		public void HasCorrectProperties()
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

		[SkippableFact]
		public void OffsetsCorrectly()
		{
			var expected = new SKRect(25, 30, 65, 80);

			var rect1 = new SKRect(15, 25, 55, 75);
			rect1.Location = new SKPoint(25, 30);

			var rect2 = new SKRect(15, 25, 55, 75);
			rect2.Offset (10, 5);

			Assert.Equal(expected, rect1);
			Assert.Equal(expected, rect2);
		}

		[SkippableFact]
		public void InflatesCorrectly()
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

		[SkippableFact]
		public void StandardizeCorrectly()
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

		[SkippableTheory]
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 5, 10, /*result:*/ 5 + 5, 5 + 0, 10, 20)] // tall image in a square frame
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 10, 5, /*result:*/ 5 + 0, 5 + 5, 20, 10)] // wide image in a square frame
		public void AspectFitIsCorrect(float rX, float rY, float rW, float rH, float sW, float sH, float eX, float eY, float eW, float eH)
		{
			var expected = SKRect.Create(eX, eY, eW, eH);

			var bigRect = SKRect.Create(rX, rY, rW, rH);
			var imageSize = new SKSize(sW, sH);

			var fit = bigRect.AspectFit(imageSize);

			Assert.Equal(expected, fit);
		}

		[SkippableTheory]
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 5, 10, /*result:*/ 5 + 0, 5 - 10, 20, 40)] // tall image in a square frame
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 10, 5, /*result:*/ 5 - 10, 5 + 0, 40, 20)] // wide image in a square frame
		[InlineData(/*frame:*/ 0, 0, 1024, 767, /*size:*/ 1024, 1024, /*result:*/ 0, -128.5f, 1024, 1024)] // #2562
		public void AspectFillIsCorrect(float rX, float rY, float rW, float rH, float sW, float sH, float eX, float eY, float eW, float eH)
		{
			var expected = SKRect.Create(eX, eY, eW, eH);

			var bigRect = SKRect.Create(rX, rY, rW, rH);
			var imageSize = new SKSize(sW, sH);

			var fit = bigRect.AspectFill(imageSize);

			Assert.Equal(expected, fit);
		}
	}

	public class SKRectITest : SKTest
	{
		[SkippableTheory]
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 5, 10, /*result:*/ 5 + 5, 5 + 0, 10, 20)] // tall image in a square frame
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 10, 5, /*result:*/ 5 + 0, 5 + 5, 20, 10)] // wide image in a square frame
		public void AspectFitIsCorrect(int rX, int rY, int rW, int rH, int sW, int sH, int eX, int eY, int eW, int eH)
		{
			var expected = SKRectI.Create(eX, eY, eW, eH);

			var bigRect = SKRectI.Create(rX, rY, rW, rH);
			var imageSize = new SKSizeI(sW, sH);

			var fit = bigRect.AspectFit(imageSize);

			Assert.Equal(expected, fit);
		}

		[SkippableTheory]
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 5, 10, /*result:*/ 5 + 0, 5 - 10, 20, 40)] // tall image in a square frame
		[InlineData(/*frame:*/ 5, 5, 20, 20, /*size:*/ 10, 5, /*result:*/ 5 - 10, 5 + 0, 40, 20)] // wide image in a square frame
		[InlineData(/*frame:*/ 0, 0, 1024, 767, /*size:*/ 1024, 1024, /*result:*/ 0, -129f, 1024, 1024)] // #2562
		public void AspectFillIsCorrect(int rX, int rY, int rW, int rH, int sW, int sH, int eX, int eY, int eW, int eH)
		{
			var expected = SKRectI.Create(eX, eY, eW, eH);

			var bigRect = SKRectI.Create(rX, rY, rW, rH);
			var imageSize = new SKSizeI(sW, sH);

			var fit = bigRect.AspectFill(imageSize);

			Assert.Equal(expected, fit);
		}

		[SkippableFact]
		public void CeilingWorksAsExpected()
		{
			Assert.Equal(new SKRectI(6, 6, 21, 21), SKRectI.Ceiling(new SKRect(5.5f, 5.5f, 20.5f, 20.5f)));
			Assert.Equal(new SKRectI(5, 5, 21, 21), SKRectI.Ceiling(new SKRect(5.5f, 5.5f, 20.5f, 20.5f), true));
			Assert.Equal(new SKRectI(6, 6, 21, 21), SKRectI.Ceiling(new SKRect(5.4f, 5.6f, 20.4f, 20.6f)));
			Assert.Equal(new SKRectI(5, 5, 21, 21), SKRectI.Ceiling(new SKRect(5.4f, 5.6f, 20.4f, 20.6f), true));
			Assert.Equal(new SKRectI(21, 21, 6, 6), SKRectI.Ceiling(new SKRect(20.4f, 20.6f, 5.4f, 5.6f)));
			Assert.Equal(new SKRectI(21, 21, 5, 5), SKRectI.Ceiling(new SKRect(20.4f, 20.6f, 5.4f, 5.6f), true));
		}

		[SkippableFact]
		public void FloorWorksAsExpected()
		{
			Assert.Equal(new SKRectI(5, 5, 20, 20), SKRectI.Floor(new SKRect(5.5f, 5.5f, 20.5f, 20.5f)));
			Assert.Equal(new SKRectI(6, 6, 20, 20), SKRectI.Floor(new SKRect(5.5f, 5.5f, 20.5f, 20.5f), true));
			Assert.Equal(new SKRectI(5, 5, 20, 20), SKRectI.Floor(new SKRect(5.4f, 5.6f, 20.4f, 20.6f)));
			Assert.Equal(new SKRectI(6, 6, 20, 20), SKRectI.Floor(new SKRect(5.4f, 5.6f, 20.4f, 20.6f), true));
			Assert.Equal(new SKRectI(20, 20, 5, 5), SKRectI.Floor(new SKRect(20.4f, 20.6f, 5.4f, 5.6f)));
			Assert.Equal(new SKRectI(20, 20, 6, 6), SKRectI.Floor(new SKRect(20.4f, 20.6f, 5.4f, 5.6f), true));
		}

		[SkippableFact]
		public void RoundWorksAsExpected()
		{
			Assert.Equal(new SKRectI(6, 6, 21, 21), SKRectI.Round(new SKRect(5.51f, 5.51f, 20.51f, 20.51f)));
			Assert.Equal(new SKRectI(5, 6, 20, 21), SKRectI.Round(new SKRect(5.41f, 5.61f, 20.41f, 20.61f)));
			Assert.Equal(new SKRectI(20, 21, 5, 6), SKRectI.Round(new SKRect(20.41f, 20.61f, 5.41f, 5.61f)));
		}
	}
}
