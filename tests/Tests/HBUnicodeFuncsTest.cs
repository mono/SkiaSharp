using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HBUnicodeFuncsTest : SKTest
	{
		[SkippableFact]
		public void ShouldBeImmutable()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				Assert.True(unicodeFuncs.IsImmutable);
			}
		}

		[SkippableFact]
		public void ShouldGetScript()
		{
			using (var unicodeFuncs = UnicodeFunctions.Default)
			{
				var script = unicodeFuncs.GetScript('A');

				Assert.Equal(Script.Latin, script);
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
