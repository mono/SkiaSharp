using System;
using System.IO;
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
			using (var tf = SKTypeface.FromFamilyName("Arial"))
			using (var paint = new SKPaint { TextSize = 50, IsAntialias = true, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, paint);

				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(49, 92), "Antialias (1)");
				Assert.AreEqual(SKColors.White, bitmap.GetPixel(73, 63), "Antialias (2)");
				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(100, 89), "Antialias (3)");
			}

			using (var bitmap = new SKBitmap(new SKImageInfo(200, 200)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFamilyName("Arial"))
			using (var paint = new SKPaint { TextSize = 50, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, paint);

				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(49, 92), "Non-Antialias (1)");
				Assert.AreEqual(SKColors.White, bitmap.GetPixel(73, 63), "Non-Antialias (2)");
				Assert.AreEqual(SKColors.Black, bitmap.GetPixel(100, 89), "Non-Antialias (3)");
			}
		}

		// Test for issue #282
		[Ignore("Known to fail, see: https://github.com/mono/SkiaSharp/issues/282")]
		[Test]
		public void DrawTransparentImageWithHighFilterQualityWithUnpremul()
		{
			var oceanColor = (SKColor)0xFF9EB4D6;
			var landColor = (SKColor)0xFFACB69B;

			using (var bitmap = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Clear(oceanColor);

				// decode the bitmap
				var path = Path.Combine(PathToImages, "map.png");

				using (var mapBitmap = SKBitmap.Decode(path))
				using (var mapImage = SKImage.FromBitmap(mapBitmap))
				{
					var bounds = SKRect.Create(-259.9664f, -260.4489f, 1221.1876f, 1020.23273f);

					// draw the bitmap
					using (var paint = new SKPaint { FilterQuality = SKFilterQuality.High })
					{
						canvas.DrawImage(mapImage, bounds, paint);
					}
				}

				// check values
				Assert.AreEqual(oceanColor, bitmap.GetPixel(30, 30), "Ocean color");
				Assert.AreEqual(landColor, bitmap.GetPixel(270, 270), "Land color");
			}
		}

		// Test for the "workaround" for issue #282
		[Test]
		public void DrawTransparentImageWithHighFilterQualityWithPremul()
		{
			var oceanColor = (SKColor)0xFF9EB4D6;
			var landColor = (SKColor)0xFFACB69B;

			using (var bitmap = new SKBitmap(new SKImageInfo(300, 300)))
			using (var canvas = new SKCanvas(bitmap))
			{
				canvas.Clear(oceanColor);

				// decode the bitmap
				var path = Path.Combine(PathToImages, "map.png");
				using (var codec = SKCodec.Create(new SKFileStream(path)))
				{
					var info = new SKImageInfo(codec.Info.Width, codec.Info.Height);

					using (var mapBitmap = SKBitmap.Decode(codec, info))
					using (var mapImage = SKImage.FromBitmap(mapBitmap))
					{
						var bounds = SKRect.Create(-259.9664f, -260.4489f, 1221.1876f, 1020.23273f);

						// draw the bitmap
						using (var paint = new SKPaint { FilterQuality = SKFilterQuality.High })
						{
							canvas.DrawImage(mapImage, bounds, paint);
						}
					}
				}

				// check values
				Assert.AreEqual(oceanColor, bitmap.GetPixel(30, 30), "Ocean color");
				Assert.AreEqual(landColor, bitmap.GetPixel(270, 270), "Land color");
			}
		}
	}
}
