using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKPaintTest : SKTest
	{
		public SKPaintTest(ITestOutputHelper output)
			: base(output)
		{
		}

		[Fact]
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

		[Fact]
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

		[Fact]
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
		[Fact]
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
		[Fact(Skip = "Known to fail, see: https://github.com/mono/SkiaSharp/issues/282")]
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
		[Fact]
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
		[Fact]
		public void Clone()
		{
			using var paint = new SKPaint();
			using var clonedPaint = paint.Clone();
			using var clonedPaint2 = paint.Clone();
		}

		[Fact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void GetFastBoundsWithBlurOutsetsByThreeSigma()
		{
			const float sigma = 4.5f;
			using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, sigma);
			using var paint = new SKPaint { MaskFilter = blur };

			var src = new SKRect(10, 20, 110, 120);
			Assert.True(paint.GetFastBounds(src, out var bounds));

			// A fill paint adds no stroke inflation, so the blur mask filter outsets the
			// source rect by 3 * sigma on every edge.
			var pad = 3f * sigma;
			Assert.Equal(src.Left - pad, bounds.Left, 3);
			Assert.Equal(src.Top - pad, bounds.Top, 3);
			Assert.Equal(src.Right + pad, bounds.Right, 3);
			Assert.Equal(src.Bottom + pad, bounds.Bottom, 3);
		}

		[Fact]
		public void GetFastBoundsFillWithNoEffectsReturnsSource()
		{
			using var paint = new SKPaint();

			var src = new SKRect(10, 20, 110, 120);
			Assert.True(paint.GetFastBounds(src, out var bounds));

			// Filling with no geometry-affecting effects cannot grow the bounds.
			Assert.Equal(src, bounds);
		}

		[Fact]
		public void GetFastBoundsStrokeOutsetsByHalfStrokeWidth()
		{
			const float strokeWidth = 12f;
			using var paint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = strokeWidth,
				StrokeJoin = SKStrokeJoin.Round,
				StrokeCap = SKStrokeCap.Round,
			};

			var src = new SKRect(10, 20, 110, 120);
			Assert.True(paint.GetFastBounds(src, out var bounds));

			// Round join and round cap give an inflation multiplier of 1, so the rect
			// outsets by strokeWidth / 2 on every edge.
			var pad = strokeWidth / 2f;
			Assert.Equal(src.Left - pad, bounds.Left, 3);
			Assert.Equal(src.Top - pad, bounds.Top, 3);
			Assert.Equal(src.Right + pad, bounds.Right, 3);
			Assert.Equal(src.Bottom + pad, bounds.Bottom, 3);
		}

		[Fact]
		public void GetFastBoundsComposesStrokeAndBlur()
		{
			const float sigma = 5f;
			const float strokeWidth = 8f;
			using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, sigma);
			using var paint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = strokeWidth,
				StrokeJoin = SKStrokeJoin.Round,
				StrokeCap = SKStrokeCap.Round,
				MaskFilter = blur,
			};

			var src = new SKRect(20, 20, 120, 120);
			Assert.True(paint.GetFastBounds(src, out var bounds));

			// The paint composes effects: the stroke inflates by strokeWidth / 2, then the
			// blur outsets that result by 3 * sigma.
			var pad = (strokeWidth / 2f) + (3f * sigma);
			Assert.Equal(src.Left - pad, bounds.Left, 3);
			Assert.Equal(src.Top - pad, bounds.Top, 3);
			Assert.Equal(src.Right + pad, bounds.Right, 3);
			Assert.Equal(src.Bottom + pad, bounds.Bottom, 3);
		}

		[Fact]
		public void GetFastBoundsDoesNotModifySource()
		{
			using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 5f);
			using var paint = new SKPaint { MaskFilter = blur };

			var src = new SKRect(10, 10, 60, 60);
			var original = src;

			Assert.True(paint.GetFastBounds(src, out _));

			Assert.Equal(original, src);
		}

		[Fact]
		public void GetFastBoundsReturnsTrueForSimplePaints()
		{
			var src = new SKRect(0, 0, 50, 50);

			using var fill = new SKPaint();
			Assert.True(fill.GetFastBounds(src, out _));

			using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f);
			using var blurred = new SKPaint { MaskFilter = blur };
			Assert.True(blurred.GetFastBounds(src, out _));

			using var stroked = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 4 };
			Assert.True(stroked.GetFastBounds(src, out _));
		}

		[Fact]
		public void GetFastBoundsMatchesNativeApi()
		{
			const float sigma = 3.25f;
			using var blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, sigma);
			using var paint = new SKPaint { MaskFilter = blur };

			var src = new SKRect(5, 15, 95, 135);

			Assert.True(paint.GetFastBounds(src, out var managed));

			SKRect native;
			unsafe
			{
				SkiaApi.sk_paint_compute_fast_bounds(paint.Handle, &src, &native);
			}

			Assert.Equal(native, managed);
		}
	}
}
