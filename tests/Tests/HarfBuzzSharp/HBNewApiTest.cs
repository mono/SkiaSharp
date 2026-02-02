using System;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBNewApiTest : HBTest
	{
		// Tests for APIs added in HarfBuzz 2.9.1+

		[SkippableFact]
		public void LanguageMatchesWorksForExactMatch()
		{
			var langEn = new Language("en");
			var langEnUs = new Language("en-us");
			var langFr = new Language("fr");

			// Exact match should work
			Assert.True(langEn.Matches(langEn));
		}

		[SkippableFact]
		public void LanguageMatchesWorksForPrefix()
		{
			var langEn = new Language("en");
			var langEnUs = new Language("en-us");

			// "en" matches "en-us" because "en" is a prefix
			Assert.True(langEn.Matches(langEnUs));
		}

		[SkippableFact]
		public void LanguageMatchesReturnsFalseForDifferentLanguages()
		{
			var langEn = new Language("en");
			var langFr = new Language("fr");

			Assert.False(langEn.Matches(langFr));
		}

		[SkippableFact]
		public void LanguageMatchesThrowsOnNull()
		{
			var langEn = new Language("en");
			Assert.Throws<ArgumentNullException>(() => langEn.Matches(null));
		}

		// Tests for APIs added in HarfBuzz 3.4.0+

		[SkippableFact]
		public void BufferCreateSimilarCreatesNewBuffer()
		{
			using (var original = new Buffer())
			{
				// Set properties that should be copied
				original.Flags = BufferFlags.BeginningOfText;
				original.ClusterLevel = ClusterLevel.MonotoneCharacters;
				original.ReplacementCodepoint = 0xFFFE;

				using (var similar = Buffer.CreateSimilar(original))
				{
					Assert.NotNull(similar);
					Assert.NotEqual(original.Handle, similar.Handle);
					
					// These properties should be copied
					Assert.Equal(original.Flags, similar.Flags);
					Assert.Equal(original.ClusterLevel, similar.ClusterLevel);
					Assert.Equal(original.ReplacementCodepoint, similar.ReplacementCodepoint);
				}
			}
		}

		[SkippableFact]
		public void BufferCreateSimilarDoesNotCopyContent()
		{
			using (var original = new Buffer())
			{
				original.Direction = Direction.LeftToRight;
				original.AddUtf8("Hello");

				using (var similar = Buffer.CreateSimilar(original))
				{
					// Content should NOT be copied
					Assert.Equal(0, similar.Length);
				}
			}
		}

		[SkippableFact]
		public void BufferCreateSimilarThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => Buffer.CreateSimilar(null));
		}

		// Tests for Font synthetic slant (HarfBuzz 3.4.0+)

		[SkippableFact]
		public void FontSyntheticSlantDefaultsToZero()
		{
			Assert.Equal(0f, Font.SyntheticSlant);
		}

		[SkippableFact]
		public void FontSyntheticSlantCanBeSet()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SyntheticSlant = 0.2f;
				Assert.Equal(0.2f, font.SyntheticSlant, 0.001f);

				font.SyntheticSlant = -0.1f;
				Assert.Equal(-0.1f, font.SyntheticSlant, 0.001f);

				font.SyntheticSlant = 0f;
				Assert.Equal(0f, font.SyntheticSlant);
			}
		}

		// Tests for Font synthetic bold (HarfBuzz 7.0.0+)

		[SkippableFact]
		public void FontSyntheticBoldDefaultsToZeroButInPlaceIsTrue()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetSyntheticBold(out var x, out var y, out var inPlace);
				Assert.Equal(0f, x);
				Assert.Equal(0f, y);
				// Default is true in HarfBuzz (useful for font grading simulation)
				Assert.True(inPlace);
			}
		}

		[SkippableFact]
		public void FontSyntheticBoldCanBeSet()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetSyntheticBold(40f, 20f, false);
				font.GetSyntheticBold(out var x, out var y, out var inPlace);
				Assert.Equal(40f, x, 0.001f);
				Assert.Equal(20f, y, 0.001f);
				Assert.False(inPlace);

				font.SetSyntheticBold(30f, 0f, true);
				font.GetSyntheticBold(out x, out y, out inPlace);
				Assert.Equal(30f, x, 0.001f);
				Assert.Equal(0f, y);
				Assert.True(inPlace);
			}
		}

		// Tests for Font variations (HarfBuzz - variable font support)

		[SkippableFact]
		public void FontSetVariationWithTagWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				// Even for non-variable fonts, setting variations shouldn't throw
				font.SetVariation("wght", 700f);
			}
		}

		[SkippableFact]
		public void FontSetVariationWithStringTagWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetVariation("wght", 700f);
				font.SetVariation("wdth", 100f);
				font.SetVariation("slnt", -12f);
			}
		}

		[SkippableFact]
		public void FontSetVariationThrowsOnInvalidTag()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentException>(() => font.SetVariation("wt", 700f));
				Assert.Throws<ArgumentException>(() => font.SetVariation("weight", 700f));
				Assert.Throws<ArgumentNullException>(() => font.SetVariation((string)null, 700f));
			}
		}

		[SkippableFact]
		public void FontSetVariationsArrayWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var variations = new Variation[]
				{
					new Variation { Tag = Tag.Parse("wght"), Value = 700f },
					new Variation { Tag = Tag.Parse("wdth"), Value = 100f }
				};
				font.SetVariations(variations);
			}
		}

		[SkippableFact]
		public void FontSetVariationsEmptyArrayWorks()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.SetVariations(Array.Empty<Variation>());
			}
		}

		[SkippableFact]
		public void FontSetVariationsThrowsOnNull()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentNullException>(() => font.SetVariations(null));
			}
		}

		// Tests for Font named instance (HarfBuzz 7.0.0+)

		[SkippableFact]
		public void FontNamedInstanceDefaultsToMaxValue()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				// HB_FONT_NO_VAR_NAMED_INSTANCE = 0xFFFFFFFF means no named instance
				Assert.Equal(0xFFFFFFFF, font.NamedInstance);
			}
		}

		[SkippableFact]
		public void FontNamedInstanceCanBeSet()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.NamedInstance = 0;
				Assert.Equal(0u, font.NamedInstance);

				font.NamedInstance = 0xFFFFFFFF;
				Assert.Equal(0xFFFFFFFF, font.NamedInstance);
			}
		}

		// Tests for Variation struct

		[SkippableFact]
		public void VariationStructCanBeCreated()
		{
			var variation = new Variation
			{
				Tag = Tag.Parse("wght"),
				Value = 700f
			};

			Assert.Equal((uint)Tag.Parse("wght"), variation.Tag);
			Assert.Equal(700f, variation.Value);
		}

		[SkippableFact]
		public void VariationStructEquality()
		{
			var v1 = new Variation { Tag = Tag.Parse("wght"), Value = 700f };
			var v2 = new Variation { Tag = Tag.Parse("wght"), Value = 700f };
			var v3 = new Variation { Tag = Tag.Parse("wdth"), Value = 700f };
			var v4 = new Variation { Tag = Tag.Parse("wght"), Value = 400f };

			Assert.Equal(v1, v2);
			Assert.NotEqual(v1, v3);
			Assert.NotEqual(v1, v4);
		}

		// Tests for Tag struct

		[SkippableFact]
		public void TagParseWorks()
		{
			var tag = Tag.Parse("wght");
			Assert.Equal("wght", tag.ToString());
		}

		[SkippableFact]
		public void TagParsePadsShortStrings()
		{
			var tag = Tag.Parse("ab");
			Assert.Equal("ab  ", tag.ToString());
		}

		[SkippableFact]
		public void TagParseTruncatesLongStrings()
		{
			var tag = Tag.Parse("weight");
			Assert.Equal("weig", tag.ToString());
		}
	}
}
