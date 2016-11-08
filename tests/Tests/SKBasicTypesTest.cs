using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKBasicTypesTest : SKTest
	{
		private const float EPSILON = 0.0001f;

		[Test]
		public void RectanlgeHasCorrectProperties()
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
		public void RectanlgeOffsetsCorrectly()
		{
			var expected = new SKRect(25, 30, 65, 80);

			var rect1 = new SKRect(15, 25, 55, 75);
			rect1.Location = new SKPoint(25, 30);

			var rect2 = new SKRect(15, 25, 55, 75);
			rect2.Offset (10, 5);

			Assert.AreEqual(expected, rect1, "SKRect.Location");
			Assert.AreEqual(expected, rect2, "SKRect.Offset");
		}

		[Test]
		public void RectanlgeInflatesCorrectly()
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
		public void RectanlgeStandardizeCorrectly()
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
		public void RectanlgeAspectFitIsCorrect()
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
		public void RectanlgeAspectFillIsCorrect()
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
	}
}
