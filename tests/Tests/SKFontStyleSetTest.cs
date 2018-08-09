using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFontStyleSetTest : SKTest
	{
		[SkippableFact]
		public void TestCanCreateEmpty()
		{
			var set = new SKFontStyleSet();

			Assert.Equal(0, set.Count);
		}

		[SkippableFact]
		public void TestFindsNothing()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.MatchFamily("Missing Font");
			Assert.NotNull(set);

			Assert.Equal(0, set.Count);
		}

		[SkippableFact]
		public void TestSetHasAtLeastOne()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.MatchFamily(DefaultFontFamily);
			Assert.NotNull(set);

			Assert.True(set.Count > 0);
		}

		[SkippableFact]
		public void TestCanGetStyles()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.MatchFamily(DefaultFontFamily);

			for (var i = 0; i < set.Count; i++)
			{
				set.GetStyle(i, out var style, out var name);

				Assert.NotNull(style);
				Assert.NotNull(name);
			}
		}

		[SkippableFact]
		public void TestCanCreateBoldFromIndex()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.MatchFamily(DefaultFontFamily);

			int idx;
			for (idx = 0; idx < set.Count; idx++)
			{
				set.GetStyle(idx, out var style, out var name);
				if (style.Weight == (int)SKFontStyleWeight.Bold)
				{
					// flip the sign so we can confirm that we found it
					idx *= -1;
					break;
				}
			}

			// check that we found something
			Assert.True(idx <= 0);

			// flip the sign and get the typeface
			var typeface = set.CreateTypeface(-idx);

			Assert.NotNull(typeface);
			Assert.Equal((int)SKFontStyleWeight.Bold, typeface.FontStyle.Weight);
		}

		[SkippableFact]
		public void TestCanCreateBold()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.MatchFamily(DefaultFontFamily);

			var typeface = set.CreateTypeface(SKFontStyle.Bold);

			Assert.NotNull(typeface);
			Assert.Equal((int)SKFontStyleWeight.Bold, typeface.FontStyle.Weight);
		}
	}
}
