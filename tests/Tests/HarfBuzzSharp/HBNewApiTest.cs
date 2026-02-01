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

		// Tests for APIs added in HarfBuzz 3.4.0+ (Font synthetic slant)

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
	}
}
