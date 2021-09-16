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

		[SkippableTheory]
		[InlineData(SKTextEncoding.Utf8, "ä", 2)]
		[InlineData(SKTextEncoding.Utf8, "a", 1)]
		[InlineData(SKTextEncoding.Utf16, "ä", 2)]
		[InlineData(SKTextEncoding.Utf16, "a", 2)]
		[InlineData(SKTextEncoding.Utf32, "ä", 4)]
		[InlineData(SKTextEncoding.Utf32, "a", 4)]
		[InlineData(SKTextEncoding.GlyphId, "ä", 2)]
		[InlineData(SKTextEncoding.GlyphId, "a", 2)]
		public void BreakTextReturnsTheCorrectNumberOfBytes(SKTextEncoding encoding, string text, int extectedRead)
		{
			var paint = new SKPaint();
			paint.TextEncoding = encoding;

			// get bytes
			var bytes = encoding == SKTextEncoding.GlyphId
				? GetGlyphBytes(text)
				: StringUtilities.GetEncodedText(text, encoding);

			var read = paint.BreakText(bytes, 50.0f, out var measured);
			Assert.Equal(extectedRead, read);
			Assert.True(measured > 0);

			byte[] GetGlyphBytes(string text)
			{
				var glyphs = paint.GetGlyphs(text);
				var bytes = new byte[Buffer.ByteLength(glyphs)];
				Buffer.BlockCopy(glyphs, 0, bytes, 0, bytes.Length);
				return bytes;
			}
		}

		[SkippableFact]
		public void BreakTextSucceedsForEmtptyString()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Equal(0, paint.BreakText("", 50.0f));
		}

		[SkippableFact]
		public void BreakTextSucceedsForNullPointerZeroLength()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Equal(0, paint.BreakText(IntPtr.Zero, IntPtr.Zero, 50.0f));
			Assert.Equal(0, paint.BreakText(IntPtr.Zero, 0, 50.0f));
		}

		[SkippableFact]
		public void BreakTextThrowsForNullPointer()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Throws<ArgumentNullException>(() => paint.BreakText(IntPtr.Zero, (IntPtr)123, 50.0f));
			Assert.Throws<ArgumentNullException>(() => paint.BreakText(IntPtr.Zero, 123, 50.0f));
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

		[SkippableTheory]
		[InlineData(-1)]
		[InlineData(1 << 17)]
		public void BreakTextWidthIsEqualToMeasureTextWidth(int textSize)
		{
			var font = new SKPaint();

			if (textSize >= 0)
				font.TextSize = textSize;

			var text =
				"The ultimate measure of a man is not where he stands in moments of comfort " +
				"and convenience, but where he stands at times of challenge and controversy.";
			var length = text.Length;

			var width = font.MeasureText(text);

			var length2 = font.BreakText(text, width, out var mm);

			Assert.Equal(length, length2);
			Assert.Equal(width, mm);
		}

		[SkippableTheory]
		[InlineData(-1)]
		[InlineData(1 << 17)]
		public void BreakTextHandlesLongText(int textSize)
		{
			var font = new SKPaint();

			if (textSize >= 0)
				font.TextSize = textSize;

			var text = string.Concat(Enumerable.Repeat('a', 1024));

			var width = font.MeasureText(text);

			var length = font.BreakText(text, width, out var mm);

			Assert.Equal(1024, length);
			Assert.Equal(width, mm);
		}

		[SkippableTheory]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(1 << 17)]
		public void BreatTextHasCorrectLogic(int textSize)
		{
			var font = new SKPaint();

			if (textSize >= 0)
				font.TextSize = textSize;

			var text = "sdfkljAKLDFJKEWkldfjlk#$%&sdfs.dsj";
			var length = text.Length;
			var width = font.MeasureText(text);

			var mm = 0f;
			var nn = 0L;
			var step = Math.Max(width / 10f, 1f);
			for (float w = 0; w <= width; w += step)
			{
				var n = font.BreakText(text, w, out var m);

				Assert.True(n <= length);
				Assert.True(m <= width);

				if (n == 0)
				{
					Assert.Equal(0, m);
				}
				else if (n == nn)
				{
					Assert.Equal(mm, m);
				}
				else
				{
					Assert.True(n > nn);
					Assert.True(m > mm);
				}
				nn = n;
				mm = m;
			}
		}

		[SkippableFact]
		public void PlainGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "Hello World!";

			var paint = new SKPaint();

			Assert.Equal(text.Length, paint.CountGlyphs(text));
			Assert.Equal(text.Length, paint.GetGlyphs(text).Length);
		}

		[Trait(CategoryKey, MatchCharacterCategory)]
		[SkippableFact]
		public void UnicodeGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);

			var paint = new SKPaint();
			paint.TextEncoding = SKTextEncoding.Utf32;
			paint.Typeface = typeface;

			Assert.Equal(1, paint.CountGlyphs(text));
			Assert.Single(paint.GetGlyphs(text));
			Assert.NotEqual(0, paint.GetGlyphs(text)[0]);
		}

		[SkippableFact]
		public void ContainsTextIsCorrect()
		{
			const string text = "A";

			var paint = new SKPaint();
			paint.TextEncoding = SKTextEncoding.Utf32;
			paint.Typeface = SKTypeface.Default;

			Assert.True(paint.ContainsGlyphs(text));
		}

		[Trait(CategoryKey, MatchCharacterCategory)]
		[SkippableFact]
		public void ContainsUnicodeTextIsCorrect()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var paint = new SKPaint();
			paint.TextEncoding = SKTextEncoding.Utf32;

			// use the default typeface (which shouldn't have the emojis)
			paint.Typeface = SKTypeface.Default;

			Assert.False(paint.ContainsGlyphs(text));

			// find a font with the character
			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);
			paint.Typeface = typeface;

			Assert.True(paint.ContainsGlyphs(text));
		}

		[SkippableFact]
		public void CanMeasureBadUnicodeText()
		{
			using var paint = new SKPaint();

			var rect = SKRect.Empty;
			var width = paint.MeasureText("\ud83c", ref rect);

			Assert.Equal(0, width);
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

			Assert.NotEqual(SKRect.Empty, bounds8);
			Assert.Equal(bounds8, bounds16);
			Assert.Equal(bounds8, bounds32);
		}

		[SkippableFact]
		public void MeasureTextSucceedsForEmtptyString()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Equal(0, paint.MeasureText(""));
		}

		[SkippableFact]
		public void MeasureTextSucceedsForNullPointerZeroLength()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Equal(0, paint.MeasureText(IntPtr.Zero, IntPtr.Zero));
			Assert.Equal(0, paint.MeasureText(IntPtr.Zero, 0));
		}

		[SkippableFact]
		public void MeasureTextThrowsForNullPointer()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.Throws<ArgumentNullException>(() => paint.MeasureText(IntPtr.Zero, (IntPtr)123));
			Assert.Throws<ArgumentNullException>(() => paint.MeasureText(IntPtr.Zero, 123));
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
			var text = "|";

			var paint = new SKPaint();
			paint.TextSize = 100;

			var widths = paint.GetTextIntercepts(text, 50, 100, 0, 100);
			Assert.Equal(2, widths.Length);

			var diff = widths[1] - widths[0];

			var textPath = paint.GetTextPath(text, 0, 0);
			var pathWidth = textPath.TightBounds.Width;

			Assert.Equal(pathWidth, diff, 2);
		}

		[SkippableFact]
		public void GetTextPathSucceedsForEmtptyString()
		{
			var paint = new SKPaint();

			paint.TextEncoding = SKTextEncoding.Utf8;

			Assert.NotNull(paint.GetTextPath("", 0, 0));
		}

		[SkippableTheory]
		[InlineData(true, true, SKFontEdging.SubpixelAntialias)]
		[InlineData(false, true, SKFontEdging.Alias)]
		[InlineData(true, false, SKFontEdging.Antialias)]
		[InlineData(false, false, SKFontEdging.Alias)]
		public void UpdatingPropertiesIsAntialiasLcdRenderText(bool isAntialias, bool lcd, SKFontEdging newEdging)
		{
			var paint = new SKPaint();

			paint.IsAntialias = isAntialias;
			paint.LcdRenderText = lcd;

			Assert.Equal(newEdging, paint.GetFont().Edging);
		}

		[SkippableTheory]
		[InlineData(true, true, SKFontEdging.SubpixelAntialias)]
		[InlineData(false, true, SKFontEdging.Alias)]
		[InlineData(true, false, SKFontEdging.Antialias)]
		[InlineData(false, false, SKFontEdging.Alias)]
		public void UpdatingPropertiesLcdRenderTextIsAntialias(bool isAntialias, bool lcd, SKFontEdging newEdging)
		{
			var paint = new SKPaint();

			paint.LcdRenderText = lcd;
			paint.IsAntialias = isAntialias;

			Assert.Equal(newEdging, paint.GetFont().Edging);
		}

		[SkippableFact]
		public void PaintWithSubpixelEdgingIsPreserved()
		{
			var font = new SKFont();
			font.Edging = SKFontEdging.SubpixelAntialias;

			var paint = new SKPaint(font);

			Assert.True(paint.LcdRenderText);
			Assert.False(paint.IsAntialias);
			Assert.Equal(SKFontEdging.Alias, paint.GetFont().Edging);

			paint.IsAntialias = true;

			Assert.True(paint.LcdRenderText);
			Assert.True(paint.IsAntialias);
			Assert.Equal(SKFontEdging.SubpixelAntialias, paint.GetFont().Edging);
		}

		[SkippableFact]
		public void PaintWithAntialiasEdgingIsPreserved()
		{
			var font = new SKFont();
			font.Edging = SKFontEdging.Antialias;

			var paint = new SKPaint(font);

			Assert.False(paint.LcdRenderText);
			Assert.False(paint.IsAntialias);
			Assert.Equal(SKFontEdging.Alias, paint.GetFont().Edging);

			paint.IsAntialias = true;

			Assert.False(paint.LcdRenderText);
			Assert.True(paint.IsAntialias);
			Assert.Equal(SKFontEdging.Antialias, paint.GetFont().Edging);
		}

		[SkippableFact]
		public void PaintWithAliasEdgingIsPreserved()
		{
			var font = new SKFont();
			font.Edging = SKFontEdging.Alias;

			var paint = new SKPaint(font);

			Assert.False(paint.LcdRenderText);
			Assert.False(paint.IsAntialias);
			Assert.Equal(SKFontEdging.Alias, paint.GetFont().Edging);

			paint.IsAntialias = true;

			Assert.False(paint.LcdRenderText);
			Assert.True(paint.IsAntialias);
			Assert.Equal(SKFontEdging.Antialias, paint.GetFont().Edging);
		}
	}
}
