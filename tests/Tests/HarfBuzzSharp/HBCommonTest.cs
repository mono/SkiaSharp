using System;
using System.Globalization;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBCommonTest : HBTest
	{
		[SkippableFact]
		public void ShouldCreateLanguageByCulture()
		{
			var language = new Language(new CultureInfo("en"));

			Assert.NotEqual(IntPtr.Zero, language.Handle);
		}

		[SkippableFact]
		public void ShouldCreateLanguageByString()
		{
			var language = new Language("en");

			Assert.NotEqual(IntPtr.Zero, language.Handle);
		}

		[SkippableFact]
		public void LanguageShouldBeEqual()
		{
			var langA = new Language("en");

			var langB = new Language("en");

			Assert.Equal(langA, langB);
		}

		[SkippableFact]
		public void LanguageMatchesWorksForExactMatch()
		{
			var langEn = new Language("en");

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

		// Tag tests

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

		// Variation tests

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
	}
}
