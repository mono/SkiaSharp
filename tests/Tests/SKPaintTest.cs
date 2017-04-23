using System;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKPaintTest : SKTest
	{
		[Test]
		public void StrokePropertyValuesAreCorrect()
		{
			var paint = new SKPaint();

			paint.IsStroke = true;
			Assert.True(paint.IsStroke);

			paint.IsStroke = false;
			Assert.False(paint.IsStroke);
		}

		[Test]
		public void GetFillPathIsWorking()
		{
			var paint = new SKPaint();

			var rect = new SKRect(10, 10, 30, 30);

			var path = new SKPath();
			path.AddRect(rect);

			var fillPath = new SKPath();
			var isFilled = paint.GetFillPath(path, fillPath);

			Assert.True(isFilled);
			Assert.AreEqual(rect, fillPath.Bounds);
			Assert.AreEqual(4, fillPath.PointCount);
		}

		[Test]
		public void GetFillPathIsWorkingWithLine()
		{
			var paint = new SKPaint();

			var thinRect = SKRect.Create(20, 10, 0, 20);
			var rect = SKRect.Create(10, 10, 20, 20);

			var path = new SKPath();
			path.MoveTo(20, 10);
			path.LineTo(20, 30);

			var fillPath = new SKPath();
			var isFilled = paint.GetFillPath(path, fillPath);

			Assert.True(isFilled);
			Assert.AreEqual(thinRect, fillPath.Bounds);
			Assert.AreEqual(2, fillPath.PointCount);

			paint.StrokeWidth = 20;
			paint.IsStroke = true;
			isFilled = paint.GetFillPath(path, fillPath);

			Assert.True(isFilled);
			Assert.AreEqual(rect, fillPath.Bounds);
			Assert.AreEqual(4 + 1, fillPath.PointCount); // +1 becuase the last point is the same as the first
			Assert.AreEqual(4, fillPath.Points.Distinct().Count());
		}

		// Test for issue #276
		[Test]
		public void NonAntiAliasedTextOnScaledCanvasIsCorrect()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(200, 200)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFamilyName("Arial Black"))
			using (var paint = new SKPaint { TextSize = 50, IsAntialias = true, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, paint);

				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(53, 86), "Antialias (1)");
				Assert.AreEqual(SKColors.White, bitmap.GetPixel(87, 64), "Antialias (2)");
				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(121, 86), "Antialias (3)");
			}

			using (var bitmap = new SKBitmap(new SKImageInfo(200, 200)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFamilyName("Arial Black"))
			using (var paint = new SKPaint { TextSize = 50, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, paint);

				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(53, 86), "Non-Antialias (1)");
				Assert.AreEqual(SKColors.White, bitmap.GetPixel(87, 64), "Non-Antialias (2)");
				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(121, 86), "Non-Antialias (3)");
			}
		}
	}
}
