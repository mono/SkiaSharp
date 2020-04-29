using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKBasicTypesTest : SKTest
	{
		[SkippableFact]
		public void ImageInfoMethodsDoNotModifySource()
		{
			var info = new SKImageInfo(100, 30, SKColorType.Rgb565, SKAlphaType.Unpremul);

			Assert.Equal(SKColorType.Rgb565, info.ColorType);

			var copy = info.WithColorType(SKColorType.Gray8);

			Assert.Equal(SKColorType.Rgb565, info.ColorType);
			Assert.Equal(SKColorType.Gray8, copy.ColorType);
		}

		[SkippableFact]
		public void RectangleHasCorrectProperties()
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
		public void RectangleOffsetsCorrectly()
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
		public void RectangleInflatesCorrectly()
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
		public void RectangleStandardizeCorrectly()
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

		[SkippableFact]
		public void RectangleAspectFitIsCorrect()
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

		[SkippableFact]
		public void RectangleAspectFillIsCorrect()
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

		[SkippableFact]
		public void SKRectICeilingWorksAsExpected()
		{
			Assert.Equal(new SKRectI(6, 6, 21, 21), SKRectI.Ceiling(new SKRect(5.5f, 5.5f, 20.5f, 20.5f)));
			Assert.Equal(new SKRectI(5, 5, 21, 21), SKRectI.Ceiling(new SKRect(5.5f, 5.5f, 20.5f, 20.5f), true));
			Assert.Equal(new SKRectI(6, 6, 21, 21), SKRectI.Ceiling(new SKRect(5.4f, 5.6f, 20.4f, 20.6f)));
			Assert.Equal(new SKRectI(5, 5, 21, 21), SKRectI.Ceiling(new SKRect(5.4f, 5.6f, 20.4f, 20.6f), true));
			Assert.Equal(new SKRectI(21, 21, 6, 6), SKRectI.Ceiling(new SKRect(20.4f, 20.6f, 5.4f, 5.6f)));
			Assert.Equal(new SKRectI(21, 21, 5, 5), SKRectI.Ceiling(new SKRect(20.4f, 20.6f, 5.4f, 5.6f), true));
		}

		[SkippableFact]
		public void SKRectIFloorWorksAsExpected()
		{
			Assert.Equal(new SKRectI(5, 5, 20, 20), SKRectI.Floor(new SKRect(5.5f, 5.5f, 20.5f, 20.5f)));
			Assert.Equal(new SKRectI(6, 6, 20, 20), SKRectI.Floor(new SKRect(5.5f, 5.5f, 20.5f, 20.5f), true));
			Assert.Equal(new SKRectI(5, 5, 20, 20), SKRectI.Floor(new SKRect(5.4f, 5.6f, 20.4f, 20.6f)));
			Assert.Equal(new SKRectI(6, 6, 20, 20), SKRectI.Floor(new SKRect(5.4f, 5.6f, 20.4f, 20.6f), true));
			Assert.Equal(new SKRectI(20, 20, 5, 5), SKRectI.Floor(new SKRect(20.4f, 20.6f, 5.4f, 5.6f)));
			Assert.Equal(new SKRectI(20, 20, 6, 6), SKRectI.Floor(new SKRect(20.4f, 20.6f, 5.4f, 5.6f), true));
		}

		[SkippableFact]
		public void SKRectIRoundWorksAsExpected()
		{
			Assert.Equal(new SKRectI(6, 6, 21, 21), SKRectI.Round(new SKRect(5.51f, 5.51f, 20.51f, 20.51f)));
			Assert.Equal(new SKRectI(5, 6, 20, 21), SKRectI.Round(new SKRect(5.41f, 5.61f, 20.41f, 20.61f)));
			Assert.Equal(new SKRectI(20, 21, 5, 6), SKRectI.Round(new SKRect(20.41f, 20.61f, 5.41f, 5.61f)));
		}
	}
}
