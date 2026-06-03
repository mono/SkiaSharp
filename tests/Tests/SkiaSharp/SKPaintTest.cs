using System;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests
{
	public class SKPaintTest : SKTest
	{
		public SKPaintTest(ITestOutputHelper output)
			: base(output)
		{
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
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

			using var builder = new SKPathBuilder();
			builder.AddRect(rect);
			var path = builder.Detach();

			var fillPath = paint.GetFillPath(path);

			Assert.NotNull(fillPath);
			Assert.Equal(rect, fillPath.Bounds);
			Assert.Equal(4, fillPath.PointCount);
		}

		[SkippableFact]
		public void GetFillPathIsWorkingWithLine()
		{
			var paint = new SKPaint();

			var thinRect = SKRect.Create(20, 10, 0, 20);
			var rect = SKRect.Create(10, 10, 20, 20);

			using var builder = new SKPathBuilder();
			builder.MoveTo(20, 10);
			builder.LineTo(20, 30);
			var path = builder.Detach();

			var fillPath = paint.GetFillPath(path);

			Assert.NotNull(fillPath);
			Assert.Equal(thinRect, fillPath.Bounds);
			Assert.Equal(2, fillPath.PointCount);

			paint.StrokeWidth = 20;
			paint.IsStroke = true;
			var fillPath2 = paint.GetFillPath(path);

			Assert.NotNull(fillPath2);
			Assert.Equal(rect, fillPath2.Bounds);
			Assert.Equal(4 + 1, fillPath2.PointCount); // +1 because the last point is the same as the first
			Assert.Equal(4, fillPath2.Points.Distinct().Count());
		}

		// Test for issue #276
		[SkippableFact]
		public void NonAntiAliasedTextOnScaledCanvasIsCorrect()
		{
			SkipOnPlatform(IsAndroid, "TODO: figure out why the font has changed");
			SkipOnPlatform(IsBrowser, "WASM text rendering produces slightly different pixel values");

			using (var bitmapAA = new SKBitmap(new SKImageInfo(200, 200)))
			using (var bitmapNoAA = new SKBitmap(new SKImageInfo(200, 200)))
			{
				using (var canvas = new SKCanvas(bitmapAA))
				using (var tf = SKTypeface.FromFamilyName(DefaultFontFamily))
				using (var paint = new SKPaint { IsAntialias = true })
				using (var font = new SKFont { Size = 50, Typeface = tf })
				{
					canvas.Clear(SKColors.White);
					canvas.Scale(1, 2);
						canvas.DrawText("Skia", 10, 60, SKTextAlign.Left, font, paint);

					try
					{
						Assert.Equal(SKColors.Black, bitmapAA.GetPixel(49, 92));
						Assert.Equal(SKColors.White, bitmapAA.GetPixel(73, 63));
						Assert.Equal(SKColors.Black, bitmapAA.GetPixel(100, 89));
					}
					catch
					{
						WriteOutput(bitmapAA, "Bitmap with AA");

						throw;
					}
				}

				using (var canvas = new SKCanvas(bitmapNoAA))
				using (var tf = SKTypeface.FromFamilyName(DefaultFontFamily))
				using (var paint = new SKPaint { })
				using (var font = new SKFont { Size = 50, Typeface = tf })
				{
					canvas.Clear(SKColors.White);
					canvas.Scale(1, 2);
					canvas.DrawText("Skia", 10, 60, SKTextAlign.Left, font, paint);

					try
					{
						Assert.Equal(SKColors.Black, bitmapNoAA.GetPixel(49, 92));
						Assert.Equal(SKColors.White, bitmapNoAA.GetPixel(73, 63));
						Assert.Equal(SKColors.Black, bitmapNoAA.GetPixel(100, 89));
					}
					catch
					{
						WriteOutput(bitmapAA, "Bitmap with AA");
						WriteOutput(bitmapNoAA, "Bitmap WITHOUT AA");

						throw;
					}
				}
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
					using (var paint = new SKPaint())
					{
						canvas.DrawImage(mapImage, bounds, new SKSamplingOptions(SKCubicResampler.Mitchell), paint);
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
						using (var paint = new SKPaint())
						{
							canvas.DrawImage(mapImage, bounds, new SKSamplingOptions(SKCubicResampler.Mitchell), paint);
						}
					}
				}

				// check values
				Assert.Equal(oceanColor, bitmap.GetPixel(30, 30));
				Assert.Equal(landColor, bitmap.GetPixel(270, 270));
			}
		}
		[SkippableFact]
		public void Clone()
		{
			using var paint = new SKPaint();
			using var clonedPaint = paint.Clone();
			using var clonedPaint2 = paint.Clone();
		}
	}
}
