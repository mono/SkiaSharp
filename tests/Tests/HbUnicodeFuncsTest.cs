namespace SkiaSharp.Tests
{
	using HarfBuzzSharp;

	using Xunit;

	public class HbUnicodeFuncsTest : SKTest
	{
		[SkippableFact]
		public void ShouldGetScript()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var script = unicodeFuncs.GetScript('A');

				Assert.Equal(Script.LATIN, script);
			}
		}

		[SkippableFact]
		public void ShouldGetCombiningClass()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var combiningClass = unicodeFuncs.GetCombiningClass('A');

				Assert.Equal(UnicodeCombiningClass.NotReordered, combiningClass);
			}
		}

		[SkippableFact]
		public void ShouldGetGeneralCategory()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var generalCategory = unicodeFuncs.GetGeneralCategory('A');

				Assert.Equal(UnicodeGeneralCategory.UppercaseLetter, generalCategory);
			}
		}
	}
}
