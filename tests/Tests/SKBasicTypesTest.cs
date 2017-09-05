using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKBasicTypesTest : SKTest
	{
		private const float EPSILON = 0.0001f;
		private const int PRECISION = 4;

		[Test]
		public void RectangleHasCorrectProperties()
		{
			var rect = new SKRect(15, 25, 55, 75);

			Assert.AreEqual(15f, rect.Left);
			Assert.AreEqual(25f, rect.Top);
			Assert.AreEqual(55f, rect.Right);
			Assert.AreEqual(75f, rect.Bottom);

			Assert.AreEqual(40f, rect.Width);
			Assert.AreEqual(50f, rect.Height);

			Assert.AreEqual(35f, rect.MidX);
			Assert.AreEqual(50f, rect.MidY);
		}

		[Test]
		public void RectangleOffsetsCorrectly()
		{
			var expected = new SKRect(25, 30, 65, 80);

			var rect1 = new SKRect(15, 25, 55, 75);
			rect1.Location = new SKPoint(25, 30);

			var rect2 = new SKRect(15, 25, 55, 75);
			rect2.Offset (10, 5);

			Assert.AreEqual(expected, rect1);
			Assert.AreEqual(expected, rect2);
		}

		[Test]
		public void RectangleInflatesCorrectly()
		{
			var rect = new SKRect(15, 25, 55, 75);

			Assert.AreEqual(15f, rect.Left);
			Assert.AreEqual(25f, rect.Top);
			Assert.AreEqual(55f, rect.Right);
			Assert.AreEqual(75f, rect.Bottom);

			rect.Inflate(10, 20);

			Assert.AreEqual(5f, rect.Left);
			Assert.AreEqual(5f, rect.Top);
			Assert.AreEqual(65f, rect.Right);
			Assert.AreEqual(95f, rect.Bottom);
		}

		[Test]
		public void RectangleStandardizeCorrectly()
		{
			var rect = new SKRect(5, 5, 15, 15);
			Assert.AreEqual(10, rect.Width);
			Assert.AreEqual(10, rect.Height);

			Assert.AreEqual(rect, rect.Standardized);

			var negW = new SKRect(15, 5, 5, 15);
			Assert.AreEqual(-10, negW.Width);
			Assert.AreEqual(10, negW.Height);
			Assert.AreEqual(rect, negW.Standardized);

			var negH = new SKRect(5, 15, 15, 5);
			Assert.AreEqual(10, negH.Width);
			Assert.AreEqual(-10, negH.Height);
			Assert.AreEqual(rect, negH.Standardized);

			var negWH = new SKRect(15, 15, 5, 5);
			Assert.AreEqual(-10, negWH.Width);
			Assert.AreEqual(-10, negWH.Height);
			Assert.AreEqual(rect, negWH.Standardized);
		}

		[Test]
		public void RectangleAspectFitIsCorrect()
		{
			var bigRect = SKRect.Create(5, 5, 20, 20);
			var tallSize = new SKSize(5, 10);
			var wideSize = new SKSize(10, 5);

			var fitTall = bigRect.AspectFit(tallSize);
			Assert.AreEqual(5 + 5, fitTall.Left);
			Assert.AreEqual(5 + 0, fitTall.Top);
			Assert.AreEqual(10, fitTall.Width);
			Assert.AreEqual(20, fitTall.Height);

			var fitWide = bigRect.AspectFit(wideSize);
			Assert.AreEqual(5 + 0, fitWide.Left);
			Assert.AreEqual(5 + 5, fitWide.Top);
			Assert.AreEqual(20, fitWide.Width);
			Assert.AreEqual(10, fitWide.Height);
		}

		[Test]
		public void RectangleAspectFillIsCorrect()
		{
			var bigRect = SKRect.Create(5, 5, 20, 20);
			var tallSize = new SKSize(5, 10);
			var wideSize = new SKSize(10, 5);

			var fitTall = bigRect.AspectFill(tallSize);
			Assert.AreEqual(5 + 0, fitTall.Left);
			Assert.AreEqual(5 - 10, fitTall.Top);
			Assert.AreEqual(20, fitTall.Width);
			Assert.AreEqual(40, fitTall.Height);

			var fitWide = bigRect.AspectFill(wideSize);
			Assert.AreEqual(5 - 10, fitWide.Left);
			Assert.AreEqual(5 + 0, fitWide.Top);
			Assert.AreEqual(40, fitWide.Width);
			Assert.AreEqual(20, fitWide.Height);
		}

		[Test]
		public void SKRectICeilingWorksAsExpected()
		{
			Assert.AreEqual(new SKRectI(6, 6, 21, 21), SKRectI.Ceiling(new SKRect(5.5f, 5.5f, 20.5f, 20.5f)));
			Assert.AreEqual(new SKRectI(5, 5, 21, 21), SKRectI.Ceiling(new SKRect(5.5f, 5.5f, 20.5f, 20.5f), true));
			Assert.AreEqual(new SKRectI(6, 6, 21, 21), SKRectI.Ceiling(new SKRect(5.4f, 5.6f, 20.4f, 20.6f)));
			Assert.AreEqual(new SKRectI(5, 5, 21, 21), SKRectI.Ceiling(new SKRect(5.4f, 5.6f, 20.4f, 20.6f), true));
			Assert.AreEqual(new SKRectI(21, 21, 6, 6), SKRectI.Ceiling(new SKRect(20.4f, 20.6f, 5.4f, 5.6f)));
			Assert.AreEqual(new SKRectI(21, 21, 5, 5), SKRectI.Ceiling(new SKRect(20.4f, 20.6f, 5.4f, 5.6f), true));
		}

		[Test]
		public void SKRectIFloorWorksAsExpected()
		{
			Assert.AreEqual(new SKRectI(5, 5, 20, 20), SKRectI.Floor(new SKRect(5.5f, 5.5f, 20.5f, 20.5f)));
			Assert.AreEqual(new SKRectI(6, 6, 20, 20), SKRectI.Floor(new SKRect(5.5f, 5.5f, 20.5f, 20.5f), true));
			Assert.AreEqual(new SKRectI(5, 5, 20, 20), SKRectI.Floor(new SKRect(5.4f, 5.6f, 20.4f, 20.6f)));
			Assert.AreEqual(new SKRectI(6, 6, 20, 20), SKRectI.Floor(new SKRect(5.4f, 5.6f, 20.4f, 20.6f), true));
			Assert.AreEqual(new SKRectI(20, 20, 5, 5), SKRectI.Floor(new SKRect(20.4f, 20.6f, 5.4f, 5.6f)));
			Assert.AreEqual(new SKRectI(20, 20, 6, 6), SKRectI.Floor(new SKRect(20.4f, 20.6f, 5.4f, 5.6f), true));
		}

		[Test]
		public void SKRectIRoundWorksAsExpected()
		{
			Assert.AreEqual(new SKRectI(6, 6, 21, 21), SKRectI.Round(new SKRect(5.51f, 5.51f, 20.51f, 20.51f)));
			Assert.AreEqual(new SKRectI(5, 6, 20, 21), SKRectI.Round(new SKRect(5.41f, 5.61f, 20.41f, 20.61f)));
			Assert.AreEqual(new SKRectI(20, 21, 5, 6), SKRectI.Round(new SKRect(20.41f, 20.61f, 5.41f, 5.61f)));
		}
	}
}
