using System;
using System.Globalization;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBCommonTest : HBTest
	{
		[Fact]
		public void ShouldCreateLanguageByCulture()
		{
			var language = new Language(new CultureInfo("en"));

			Assert.NotEqual(IntPtr.Zero, language.Handle);
		}

		[Fact]
		public void ShouldCreateLanguageByString()
		{
			var language = new Language("en");

			Assert.NotEqual(IntPtr.Zero, language.Handle);
		}

		[Fact]
		public void LanguageShouldBeEqual()
		{
			var langA = new Language("en");

			var langB = new Language("en");

			Assert.Equal(langA, langB);
		}
	}
}
