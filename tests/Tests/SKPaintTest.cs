using System;
using System.Drawing;
using System.Drawing.Imaging;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	[TestFixture]
	public class SKPaintTest : SKTest
	{
		[Test]
		public void StrokePropertyValuesAreCorrect()
		{
			var paint = new SKPaint();

			paint.IsStroke = true;
			Assert.IsTrue(paint.IsStroke);

			paint.IsStroke = false;
			Assert.IsFalse(paint.IsStroke);
		}
	}
}
