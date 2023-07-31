using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPaintTest : SKTest
	{
		[SkippableFact]
		public void StrokePropertyValuesAreCorrect()
		{
			var paint = new SKPaint();

			paint.IsStroke = true;
			Assert.True(paint.IsStroke);
			Assert.Equal(SKPaintStyle.Stroke, paint.Style);

			paint.IsStroke = false;
			Assert.False(paint.IsStroke);
			Assert.Equal(SKPaintStyle.Fill, paint.Style);
		}

		[SkippableFact]
		public void GetFillPathIsWorking()
		{
			var paint = new SKPaint();

			var rect = new SKRect(10, 10, 30, 30);

			var path = new SKPath();
			path.AddRect(rect);

			var fillPath = new SKPath();
			var isFilled = paint.GetFillPath(path, fillPath);

			Assert.True(isFilled);
			Assert.Equal(rect, fillPath.Bounds);
			Assert.Equal(4, fillPath.PointCount);
		}

		[SkippableFact]
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
			Assert.Equal(thinRect, fillPath.Bounds);
			Assert.Equal(2, fillPath.PointCount);

			paint.StrokeWidth = 20;
			paint.IsStroke = true;
			isFilled = paint.GetFillPath(path, fillPath);

			Assert.True(isFilled);
			Assert.Equal(rect, fillPath.Bounds);
			Assert.Equal(4 + 1, fillPath.PointCount); // +1 becuase the last point is the same as the first
			Assert.Equal(4, fillPath.Points.Distinct().Count());
		}

		// Test for issue #276
		[SkippableFact]
		public void NonAntiAliasedTextOnScaledCanvasIsCorrect()
		{
			using (var bitmap = new SKBitmap(new SKImageInfo(200, 200)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFamilyName(DefaultFontFamily))
			using (var paint = new SKPaint { IsAntialias = true })
			using (var font = new SKFont { Size = 50, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, font, paint);

				Assert.Equal(SKColors.Black, bitmap.GetPixel(49, 92));
				Assert.Equal(SKColors.White, bitmap.GetPixel(73, 63));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(100, 89));
			}

			using (var bitmap = new SKBitmap(new SKImageInfo(200, 200)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFamilyName(DefaultFontFamily))
			using (var paint = new SKPaint { })
			using (var font = new SKFont { Size = 50, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, font, paint);

				Assert.Equal(SKColors.Black, bitmap.GetPixel(49, 92));
				Assert.Equal(SKColors.White, bitmap.GetPixel(73, 63));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(100, 89));
			}
		}

		// Test for issue #282
		[SkippableFact(Skip = "Known to fail, see: https://github.com/mono/SkiaSharp/issues/282")]
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
				Assert.Equal(oceanColor, bitmap.GetPixel(30, 30));
				Assert.Equal(landColor, bitmap.GetPixel(270, 270));
			}
		}

		// Test for the "workaround" for issue #282
		[SkippableFact]
		public void DrawTransparentImageWithHighFilterQualityWithPremul()
		{
			var oceanColor = (SKColor)0xFF9EB4D6;
			var landColor = (SKColor)0xFFADB69C;

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
				Assert.Equal(oceanColor, bitmap.GetPixel(30, 30));
				Assert.Equal(landColor, bitmap.GetPixel(270, 270));
			}
		}
	}
}
