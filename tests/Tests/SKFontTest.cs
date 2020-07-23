using System;
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

		[Trait(CategoryKey, MatchCharacterCategory)]
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

		[Trait(CategoryKey, MatchCharacterCategory)]
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
		public void MeasureTextMeasuresTheText()
		{
			var font = new SKFont();

			var width = font.MeasureText("Hello World!");

			Assert.True(width > 0);
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
		public void MeasureTextSucceedsForEmtptyString()
		{
			var font = new SKFont();

			Assert.Equal(0, font.MeasureText(""));
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

			Assert.Equal(pathWidth, diff, 2);
		}

		[SkippableFact]
		public void GetTextPathSucceedsForEmtptyString()
		{
			var font = new SKFont();

			var path = font.GetTextPath("");

			Assert.NotNull(path);
			Assert.Equal(0, path.PointCount);
		}
	}
}
