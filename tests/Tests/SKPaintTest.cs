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

			paint.IsStroke = false;
			Assert.False(paint.IsStroke);
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
			using (var paint = new SKPaint { TextSize = 50, IsAntialias = true, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, paint);

				Assert.Equal(SKColors.Black, bitmap.GetPixel(49, 92));
				Assert.Equal(SKColors.White, bitmap.GetPixel(73, 63));
				Assert.Equal(SKColors.Black, bitmap.GetPixel(100, 89));
			}

			using (var bitmap = new SKBitmap(new SKImageInfo(200, 200)))
			using (var canvas = new SKCanvas(bitmap))
			using (var tf = SKTypeface.FromFamilyName(DefaultFontFamily))
			using (var paint = new SKPaint { TextSize = 50, Typeface = tf })
			{
				canvas.Clear(SKColors.White);
				canvas.Scale(1, 2);
				canvas.DrawText("Skia", 10, 60, paint);

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

		[SkippableFact]
		public void BreakTextReturnsTheCorrectNumberOfBytes()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Equal(2, paint.BreakText(StringUtilities.GetEncodedText("ä", paint.TextEncoding), 50.0f));
			Assert.Equal(1, paint.BreakText(StringUtilities.GetEncodedText("a", paint.TextEncoding), 50.0f));

			paint.TextEncoding = SKTextEncoding.Utf16;
			Assert.Equal(2, paint.BreakText(StringUtilities.GetEncodedText("ä", paint.TextEncoding), 50.0f));
			Assert.Equal(2, paint.BreakText(StringUtilities.GetEncodedText("a", paint.TextEncoding), 50.0f));

			paint.TextEncoding = SKTextEncoding.Utf32;
			Assert.Equal(4, paint.BreakText(StringUtilities.GetEncodedText("ä", paint.TextEncoding), 50.0f));
			Assert.Equal(4, paint.BreakText(StringUtilities.GetEncodedText("a", paint.TextEncoding), 50.0f));
		}

		[SkippableFact]
		public void BreakTextReturnsTheCorrectNumberOfCharacters()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Equal(1, paint.BreakText("ä", 50.0f));
			Assert.Equal(1, paint.BreakText("a", 50.0f));

			paint.TextEncoding = SKTextEncoding.Utf16;
			Assert.Equal(1, paint.BreakText("ä", 50.0f));
			Assert.Equal(1, paint.BreakText("a", 50.0f));

			paint.TextEncoding = SKTextEncoding.Utf32;
			Assert.Equal(1, paint.BreakText("ä", 50.0f));
			Assert.Equal(1, paint.BreakText("a", 50.0f));
		}

		[SkippableFact]
		public void PlainGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "Hello World!";

			var paint = new SKPaint();

			Assert.Equal(text.Length, paint.CountGlyphs(text));
			Assert.Equal(text.Length, paint.GetGlyphs(text).Length);
		}

		[SkippableFact]
		public void UnicodeGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var paint = new SKPaint();
			paint.TextEncoding = SKTextEncoding.Utf32;
			paint.Typeface = SKFontManager.Default.MatchCharacter(emojiChar);

			Assert.Equal(1, paint.CountGlyphs(text));
			Assert.Single(paint.GetGlyphs(text));
		}

		[SkippableFact]
		public void ContainsTextIsCorrect()
		{
			const string text = "🚀";

			var paint = new SKPaint();
			paint.TextEncoding = SKTextEncoding.Utf32;

			// use the default typeface (which shouldn't have the emojis)
			paint.Typeface = SKTypeface.Default;

			Assert.False(paint.ContainsGlyphs(text));

			// find a font with the character
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);
			paint.Typeface = SKFontManager.Default.MatchCharacter(emojiChar);

			Assert.True(paint.ContainsGlyphs(text));
		}

		[SkippableFact]
		public void MeasureTextMeasuresTheText()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;
			var width8 = paint.MeasureText("Hello World!");

			paint.TextEncoding = SKTextEncoding.Utf16;
			var width16 = paint.MeasureText("Hello World!");

			paint.TextEncoding = SKTextEncoding.Utf32;
			var width32 = paint.MeasureText("Hello World!");

			Assert.True(width8 > 0);
			Assert.Equal(width8, width16);
			Assert.Equal(width8, width32);
		}

		[SkippableFact]
		public void MeasureTextReturnsTheBounds()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;
			var bounds8 = new SKRect();
			var width8 = paint.MeasureText("Hello World!", ref bounds8);

			paint.TextEncoding = SKTextEncoding.Utf16;
			var bounds16 = new SKRect();
			var width16 = paint.MeasureText("Hello World!", ref bounds16);

			paint.TextEncoding = SKTextEncoding.Utf32;
			var bounds32 = new SKRect();
			var width32 = paint.MeasureText("Hello World!", ref bounds32);

			Assert.True(width8 > 0);
			Assert.Equal(width8, width16);
			Assert.Equal(width8, width32);

			Assert.NotEqual(0, bounds8.Left);
			Assert.NotEqual(0, bounds8.Top);
			Assert.NotEqual(0, bounds8.Width);
			Assert.NotEqual(0, bounds8.Height);
			Assert.Equal(bounds8, bounds16);
			Assert.Equal(bounds8, bounds32);
		}

		[SkippableFact]
		public void GetGlyphWidthsReturnsTheCorrectAmount()
		{
			var paint = new SKPaint();

			var widths = paint.GetGlyphWidths("Hello World!", out var bounds);

			Assert.Equal(widths.Length, bounds.Length);
		}

		[SkippableFact]
		public void GetGlyphWidthsAreCorrect()
		{
			var paint = new SKPaint();

			var widths = paint.GetGlyphWidths("Hello World!", out var bounds);

			// make sure the 'l' glyphs are the same width
			Assert.True(widths[2] > 0);
			Assert.True(widths[2] == widths[3]);
			Assert.True(widths[2] == widths[9]);

			// make sure the 'l' bounds are the same size
			Assert.False(bounds[2].IsEmpty);
			Assert.True(bounds[2] == bounds[3]);
			Assert.True(bounds[2] == bounds[9]);

			// make sure the 'l' and 'W' glyphs are NOT the same width
			Assert.True(widths[2] != widths[6]);

			// make sure the 'l' and 'W' bounds are NOT the same width
			Assert.True(bounds[2] != bounds[6]);
		}

		[SkippableFact]
		public unsafe void TextInterceptsAreFoundCorrectly()
		{
			var paint = new SKPaint();
			paint.TextSize = 100;

			var widths = paint.GetTextIntercepts("|", 50, 100, 40, 50);
			Assert.Equal(2, widths.Length);

			var diff = Math.Round(widths[1] - widths[0]);

			var bounds = new SKRect();
			paint.MeasureText("|", ref bounds);

			Assert.Equal(Math.Round(bounds.Width), diff);
		}
	}
}
