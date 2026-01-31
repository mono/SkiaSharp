using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFontTest : SKTest
	{
		[SkippableTheory]
		[InlineData(SKTextEncoding.Utf8)]
		[InlineData(SKTextEncoding.Utf16)]
		[InlineData(SKTextEncoding.Utf32)]
		public void CountGlyphsReturnsTheCorrectNumberOfGlyphsForAscii(SKTextEncoding encoding)
		{
			using var font = new SKFont();

			var bytes = StringUtilities.GetEncodedText("a", encoding);
			var count = font.CountGlyphs(bytes, encoding);
			Assert.Equal(1, count);
		}

		[SkippableTheory]
		[InlineData(SKTextEncoding.Utf8)]
		[InlineData(SKTextEncoding.Utf16)]
		[InlineData(SKTextEncoding.Utf32)]
		public void CountGlyphsReturnsTheCorrectNumberOfGlyphsForUnicode(SKTextEncoding encoding)
		{
			using var font = new SKFont();

			var bytes = StringUtilities.GetEncodedText("ä", encoding);
			var count = font.CountGlyphs(bytes, encoding);
			Assert.Equal(1, count);
		}

		[SkippableFact]
		public unsafe void UnicharCountReturnsCorrectCount()
		{
			var text = new uint[] { 79 };
			var count = text.Length * sizeof(uint);

			using var font = new SKFont();

			fixed (uint* t = text)
			{
				Assert.Equal(1, font.CountGlyphs((IntPtr)t, count, SKTextEncoding.Utf32));

				var glyphs = font.GetGlyphs((IntPtr)t, count, SKTextEncoding.Utf32);
				Assert.Single(glyphs);
			}
		}

		[SkippableFact]
		public void CountGlyphsReturnsTheCorrectNumberOfGlyphsForEmtptyString()
		{
			using var font = new SKFont();

			Assert.Equal(0, font.CountGlyphs(""));
		}

		[SkippableFact]
		public void CountGlyphsSucceedsForNullPointerZeroLength()
		{
			using var font = new SKFont();

			Assert.Equal(0, font.CountGlyphs(IntPtr.Zero, 0, SKTextEncoding.Utf16));
		}

		[SkippableFact]
		public void CountGlyphsThrowsForNullPointer()
		{
			using var font = new SKFont();

			Assert.Throws<ArgumentNullException>(() => font.CountGlyphs(IntPtr.Zero, 123, SKTextEncoding.Utf16));
		}

		[SkippableFact]
		public void PlainGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "Hello World!";

			var font = new SKFont();

			Assert.Equal(text.Length, font.CountGlyphs(text));
			Assert.Equal(text.Length, font.GetGlyphs(text).Length);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.MatchCharacter)]
		[SkippableFact]
		public void UnicodeGlyphsReturnsTheCorrectNumberOfCharacters()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);

			using var font = new SKFont();
			font.Typeface = typeface;

			Assert.Equal(1, font.CountGlyphs(text));
			Assert.Single(font.GetGlyphs(text));
			Assert.NotEqual(0, font.GetGlyphs(text)[0]);
		}

		[SkippableFact]
		public void ContainsTextIsCorrect()
		{
			const string text = "A";

			var font = new SKFont();
			font.Typeface = SKTypeface.Default;

			Assert.True(font.ContainsGlyphs(text));
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.MatchCharacter)]
		[SkippableFact]
		public void ContainsUnicodeTextIsCorrect()
		{
			const string text = "🚀";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			var font = new SKFont();

			// use the default typeface (which shouldn't have the emojis)
			font.Typeface = SKTypeface.Default;

			Assert.False(font.ContainsGlyphs(text));

			// find a font with the character
			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);
			font.Typeface = typeface;

			Assert.True(font.ContainsGlyphs(text));
		}

		[SkippableFact]
		public void CanMeasureBadUnicodeText()
		{
			var font = new SKFont();

			var width = font.MeasureText("\ud83c", out var rect);

			Assert.Equal(0, width);
			Assert.Equal(SKRect.Empty, rect);
		}

		[SkippableFact]
		public void MeasureTextMeasuresTheText()
		{
			var font = new SKFont();

			var width = font.MeasureText("Hello World!");

			Assert.True(width > 0);
		}

		[SkippableFact]
		public void MeasureTextMeasuresTheTextForBytes()
		{
			var font = new SKFont();

			var text8 = StringUtilities.GetEncodedText("Hello World!", SKTextEncoding.Utf8);
			var width8 = font.MeasureText(text8, SKTextEncoding.Utf8);

			var text16 = StringUtilities.GetEncodedText("Hello World!", SKTextEncoding.Utf16);
			var width16 = font.MeasureText(text16, SKTextEncoding.Utf16);

			var text32 = StringUtilities.GetEncodedText("Hello World!", SKTextEncoding.Utf32);
			var width32 = font.MeasureText(text32, SKTextEncoding.Utf32);

			Assert.True(width8 > 0);
			Assert.Equal(width8, width16);
			Assert.Equal(width8, width32);
		}

		[SkippableFact]
		public void MeasureTextReturnsTheBounds()
		{
			var font = new SKFont();

			var width = font.MeasureText("Hello World!", out var bounds);

			Assert.True(width > 0);
			Assert.NotEqual(SKRect.Empty, bounds);
		}

		[SkippableFact]
		public void MeasureTextReturnsTheBoundsForBytes()
		{
			var font = new SKFont();

			var text8 = StringUtilities.GetEncodedText("Hello World!", SKTextEncoding.Utf8);
			var width8 = font.MeasureText(text8, SKTextEncoding.Utf8, out var bounds8);

			var text16 = StringUtilities.GetEncodedText("Hello World!", SKTextEncoding.Utf16);
			var width16 = font.MeasureText(text16, SKTextEncoding.Utf16, out var bounds16);

			var text32 = StringUtilities.GetEncodedText("Hello World!", SKTextEncoding.Utf32);
			var width32 = font.MeasureText(text32, SKTextEncoding.Utf32, out var bounds32);

			Assert.True(width8 > 0);
			Assert.Equal(width8, width16);
			Assert.Equal(width8, width32);

			Assert.NotEqual(SKRect.Empty, bounds8);
			Assert.Equal(bounds8, bounds16);
			Assert.Equal(bounds8, bounds32);
		}

		[SkippableFact]
		public void MeasureTextMeasuresTheTextForGlyphs()
		{
			var font = new SKFont();
			var expectedWidth = font.MeasureText("Hello World!");

			var glyphs = font.GetGlyphs("Hello World!");
			var width = font.MeasureText(glyphs);

			Assert.Equal(expectedWidth, width);
		}

		[SkippableFact]
		public void MeasureTextReturnsTheBoundsForGlyphs()
		{
			var font = new SKFont();
			var expectedWidth = font.MeasureText("Hello World!", out var expectedBounds);

			var glyphs = font.GetGlyphs("Hello World!");
			var width = font.MeasureText(glyphs, out var bounds);

			Assert.Equal(expectedWidth, width);
			Assert.Equal(expectedBounds, bounds);
		}

		[SkippableFact]
		public void MeasureTextSucceedsForEmptyString()
		{
			var font = new SKFont();

			Assert.Equal(0, font.MeasureText(""));
		}

		[SkippableFact]
		public void MeasureTextSucceedsForNullPointerZeroLength()
		{
			var font = new SKFont();

			Assert.Equal(0, font.MeasureText(IntPtr.Zero, 0, SKTextEncoding.Utf16));
		}

		[SkippableFact]
		public void MeasureTextThrowsForNullPointer()
		{
			var font = new SKFont();

			Assert.Throws<ArgumentNullException>(() => font.MeasureText(IntPtr.Zero, 123, SKTextEncoding.Utf16));
		}

		[SkippableFact]
		public void GetGlyphWidthsReturnsTheCorrectAmount()
		{
			var font = new SKFont();

			var widths = font.GetGlyphWidths("Hello World!", out var bounds);

			Assert.Equal(widths.Length, bounds.Length);
		}

		[SkippableFact]
		public void GetGlyphWidthsAreCorrect()
		{
			var font = new SKFont();

			var widths = font.GetGlyphWidths("Hello World!", out var bounds);

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

			var font = new SKFont();
			font.Size = 100;

			var blob = SKTextBlob.Create(text, font, new SKPoint(50, 100));

			var widths = blob.GetIntercepts(0, 100);
			Assert.Equal(2, widths.Length);

			var diff = widths[1] - widths[0];

			var textPath = font.GetTextPath(text, SKPoint.Empty);
			var pathWidth = textPath.TightBounds.Width;

			Assert.Equal((double)pathWidth, (double)diff, 2);
		}

		[SkippableFact]
		public void GetTextPathSucceedsForEmptyString()
		{
			var font = new SKFont();

			var path = font.GetTextPath("");

			Assert.NotNull(path);
			Assert.Equal(0, path.PointCount);
		}

		[Trait(Traits.Category.Key, Traits.Category.Values.MatchCharacter)]
		[SkippableFact]
		public void GetTextPathReturnsPathForEmoji()
		{
			const string text = "😊";
			var emojiChar = StringUtilities.GetUnicodeCharacterCode(text, SKTextEncoding.Utf32);

			// Find a font that supports this emoji
			var typeface = SKFontManager.Default.MatchCharacter(emojiChar);
			Assert.NotNull(typeface);

			using var font = new SKFont(typeface, 48);
			using var path = font.GetTextPath(text, new SKPoint(0, 0));

			Assert.NotNull(path);
			// For emojis, we should get a bounding box path even if no vector outline exists
			var bounds = path.Bounds;
			Assert.True(bounds.Width > 0, $"Expected non-zero width, got {bounds.Width}");
			Assert.True(bounds.Height > 0, $"Expected non-zero height, got {bounds.Height}");
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
		public void BreakTextReturnsTheCorrectNumberOfBytes(SKTextEncoding encoding, string text, int expectedRead)
		{
			var font = new SKFont();

			// get bytes
			var bytes = encoding == SKTextEncoding.GlyphId
				? GetGlyphBytes(text)
				: StringUtilities.GetEncodedText(text, encoding);

			var read = font.BreakText(bytes, encoding, 50.0f, out var measured);
			Assert.Equal(expectedRead, read);
			Assert.True(measured > 0);

			byte[] GetGlyphBytes(string text)
			{
				var glyphs = font.GetGlyphs(text);
				var bytes = new byte[Buffer.ByteLength(glyphs)];
				Buffer.BlockCopy(glyphs, 0, bytes, 0, bytes.Length);
				return bytes;
			}
		}

		[SkippableFact]
		public void BreakTextSucceedsForEmptyString()
		{
			var font = new SKFont();

			Assert.Equal(0, font.BreakText("", 50.0f));
		}

		[SkippableFact]
		public void BreakTextSucceedsForNullPointerZeroLength()
		{
			var font = new SKFont();

			Assert.Equal(0, font.BreakText(IntPtr.Zero, 0, SKTextEncoding.Utf8, 50.0f));
		}

		[SkippableFact]
		public void BreakTextThrowsForNullPointer()
		{
			var font = new SKFont();

			Assert.Throws<ArgumentNullException>(() => font.BreakText(IntPtr.Zero, 123, SKTextEncoding.Utf8, 50.0f));
		}

		[SkippableFact]
		public void BreakTextReturnsTheCorrectNumberOfCharacters()
		{
			var font = new SKFont();

			Assert.Equal(1, font.BreakText("ä", 50.0f));
			Assert.Equal(1, font.BreakText("ä", 50.0f));
		}

		[SkippableTheory]
		[InlineData(-1)]
		[InlineData(1 << 17)]
		public void BreakTextWidthIsEqualToMeasureTextWidth(int textSize)
		{
			var font = new SKFont();

			if (textSize >= 0)
				font.Size = textSize;

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
			var font = new SKFont();

			if (textSize >= 0)
				font.Size = textSize;

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
		public void BreakTextHasCorrectLogic(int textSize)
		{
			var font = new SKFont();

			if (textSize >= 0)
				font.Size = textSize;

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

		[SkippableTheory]
		[InlineData("CourierNew.ttf")]
		[InlineData("Distortable.ttf")]
		[InlineData("Funkster.ttf")]
		[InlineData("HangingS.ttf")]
		[InlineData("ReallyBigA.ttf")]
		[InlineData("Roboto.woff2")]
		[InlineData("RobotoMono.woff2")]
		[InlineData("Roboto2-Regular_NoEmbed.ttf")]
		[InlineData("segoeui.ttf")]
		[InlineData("上田雅美.ttf")]
		public void CanSetTypefacesWithoutCrashing(string fontfile)
		{
			using var font = new SKFont();

			using var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, fontfile));
			font.Typeface = typeface;

			Assert.Same(typeface, font.Typeface);
		}
	}
}
