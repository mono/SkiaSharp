using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HbFontTest : HBTest
	{
		[SkippableFact]
		public void ShouldHaveDefaultSupportedShapers()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new[] { "ot" }, font.SupportedShapers);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphByUnicode()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetGlyph('A', out var glyph));
				Assert.Equal(42u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultScale()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetScale(out var xScale, out var yScale);
				Assert.Equal((2048, 2048), (xScale, yScale));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetHorizontalGlyphOrigin(49, out var xOrigin, out var yOrigin);
				Assert.Equal((0, 0), (xOrigin, yOrigin));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetVerticalGlyphOrigin(49, out var xOrigin, out var yOrigin));
				Assert.Equal((557, 1991), (xOrigin, yOrigin));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(1114, font.GetHorizontalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvance()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(-2961, font.GetVerticalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvances()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetHorizontalGlyphAdvances(new[] { 49u, 50u, 51u });
				var expected = new [] { 1114, 514, 602 };

				Assert.Equal(expected, advances);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvances()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetVerticalGlyphAdvances(new[] { 49u, 50u, 51u });
				var expected = new [] { -2961, -2961, -2961 };

				Assert.Equal(expected, advances);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphName()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetGlyphName(49, out var name);

				Assert.Equal("H", name);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphFromName()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetGlyphFromName("H", out var glyph);

				Assert.Equal(49u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphFromString()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetGlyphFromString("H", out var glyph));
				Assert.Equal(49u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldConvertGlyphToString()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var s = font.GlyphToString(49);

				Assert.Equal("H", s);
			}
		}

		[SkippableFact]
		public void GlyphToStringIsCorrectWithDelegate()
		{
			// get an array and fill it with things
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent(Font.NameBufferLength);
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte)i;
			pool.Return(buffer);

			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var s = font.GlyphToString(49);

				Assert.Equal("H", s);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphContourPointForOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.False(font.TryGetGlyphContourPointForOrigin(49, 0, Direction.LeftToRight, out _, out _));
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphContourPoint()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.False(font.TryGetGlyphContourPoint(49, 0, out _, out _));
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphAdvanceForDirection()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetGlyphAdvanceForDirection(49, Direction.LeftToRight, out var x, out _);

				Assert.Equal(1114, x);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphKerning()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var kerning = font.GetHorizontalGlyphKerning(49, 50);

				Assert.Equal(0, kerning);
			}
		}

		[Theory]
		[InlineData(OpenTypeMetricsTag.UnderlineOffset, -60)]
		[InlineData(OpenTypeMetricsTag.UnderlineSize, 9)]
		[InlineData(OpenTypeMetricsTag.StrikeoutOffset, 1049)]
		[InlineData(OpenTypeMetricsTag.StrikeoutSize, 209)]
		public void ShouldGetOpenTypeMetrics(OpenTypeMetricsTag tag, int expected)
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var result = font.OpenTypeMetrics.TryGetPosition(tag, out var position);

				Assert.True(result);

				Assert.Equal(expected, position);
			}
		}

		// Synthetic slant tests (added in HarfBuzz 3.3.0)

		[SkippableFact]
		public void SyntheticSlantDefaultsToZero()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(0f, font.SyntheticSlant);
			}
		}

		[SkippableFact]
		public void SyntheticSlantCanBeSet()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SyntheticSlant = 0.25f;
				Assert.Equal(0.25f, font.SyntheticSlant);
			}
		}

		[SkippableFact]
		public void SyntheticSlantCanBeSetToNegative()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SyntheticSlant = -0.5f;
				Assert.Equal(-0.5f, font.SyntheticSlant);
			}
		}

		// Synthetic bold tests (added in HarfBuzz 7.0.0)

		[SkippableFact]
		public void SyntheticBoldDefaultsToZero()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetSyntheticBold(out var xEmbolden, out var yEmbolden, out var inPlace);
				Assert.Equal(0f, xEmbolden);
				Assert.Equal(0f, yEmbolden);
				Assert.True(inPlace);
			}
		}

		[SkippableFact]
		public void SetSyntheticBoldWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetSyntheticBold(0.02f, 0.04f, false);
				font.GetSyntheticBold(out var x, out var y, out var inPlace);
				Assert.Equal(0.02, (double)x, 4);
				Assert.Equal(0.04, (double)y, 4);
				Assert.False(inPlace);
			}
		}

		[SkippableFact]
		public void SetSyntheticBoldInPlaceWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetSyntheticBold(0.05f, 0.05f, true);
				font.GetSyntheticBold(out var x, out var y, out var inPlace);
				Assert.Equal(0.05, (double)x, 4);
				Assert.Equal(0.05, (double)y, 4);
				Assert.True(inPlace);
			}
		}

		// Variable font tests (added in HarfBuzz 1.4.2+, expanded in later versions)

		[SkippableFact]
		public void SetVariationsWithEmptyArrayDoesNotThrow()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetVariations(Array.Empty<Variation>());
			}
		}

		[SkippableFact]
		public void SetVariationsWorksWithSingleVariation()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var variations = new[] { new Variation { Tag = Tag.Parse("wght"), Value = 700f } };
				font.SetVariations(variations);
			}
		}

		[SkippableFact]
		public void SetVariationsWorksWithMultipleVariations()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var variations = new[]
				{
					new Variation { Tag = Tag.Parse("wght"), Value = 700f },
					new Variation { Tag = Tag.Parse("wdth"), Value = 100f }
				};
				font.SetVariations(variations);
			}
		}

		[SkippableFact]
		public void SetVariationWithNumericTagWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetVariation(Tag.Parse("wght"), 400f);
			}
		}

		[SkippableFact]
		public void SetVariationWithStringTagWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetVariation("wght", 600f);
			}
		}

		[SkippableFact]
		public void SetVariationWithStringTagThrowsOnNull()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentNullException>(() => font.SetVariation(null, 400f));
			}
		}

		[SkippableFact]
		public void SetVariationWithStringTagThrowsOnInvalidLength()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentException>(() => font.SetVariation("", 400f));
				Assert.Throws<ArgumentException>(() => font.SetVariation("w", 400f));
				Assert.Throws<ArgumentException>(() => font.SetVariation("wg", 400f));
				Assert.Throws<ArgumentException>(() => font.SetVariation("wgh", 400f));
			}
		}

		// Named instance tests (added in HarfBuzz 7.0.0)

		[SkippableFact]
		public void NamedInstanceDefaultsToUnset()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				// Default is HB_FONT_NO_VAR_NAMED_INSTANCE (0xFFFFFFFF)
				Assert.Equal(0xFFFFFFFF, font.NamedInstance);
			}
		}

		[SkippableFact]
		public void NamedInstanceCanBeSet()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.NamedInstance = 0;
				Assert.Equal(0u, font.NamedInstance);
			}
		}

		// Ppem tests

		[SkippableFact]
		public void PpemDefaultsToZero()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetPpem(out var xPpem, out var yPpem);
				Assert.Equal(0, xPpem);
				Assert.Equal(0, yPpem);
			}
		}

		[SkippableFact]
		public void SetPpemWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetPpem(16, 16);
				font.GetPpem(out var xPpem, out var yPpem);
				Assert.Equal(16, xPpem);
				Assert.Equal(16, yPpem);
			}
		}

		[SkippableFact]
		public void SetPpemWithDifferentValues()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetPpem(12, 24);
				font.GetPpem(out var xPpem, out var yPpem);
				Assert.Equal(12, xPpem);
				Assert.Equal(24, yPpem);
			}
		}

		// Ptem tests

		[SkippableFact]
		public void PtemDefaultsToZero()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(0f, font.Ptem);
			}
		}

		[SkippableFact]
		public void PtemCanBeSet()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.Ptem = 12.0f;
				Assert.Equal(12.0f, font.Ptem);
			}
		}

		[SkippableFact]
		public void PtemCanBeSetToLargeValue()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.Ptem = 72.0f;
				Assert.Equal(72.0f, font.Ptem);
			}
		}
	}
}
