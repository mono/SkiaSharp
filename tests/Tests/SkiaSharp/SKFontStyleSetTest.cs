using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFontStyleSetTest : SKTest
	{
		[SkippableFact]
		public void TestCanCreateEmpty()
		{
			var set = new SKFontStyleSet();

			Assert.Empty(set);
		}

		[SkippableFact]
		public void TestFindsNothing()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles("Missing Font");
			Assert.NotNull(set);

			Assert.Empty(set);
		}

		[SkippableFact]
		public void TestSetHasAtLeastOne()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles(DefaultFontFamily);
			Assert.NotNull(set);

			Assert.True(set.Count > 0);
		}

		[SkippableFact]
		public void TestCanGetStyles()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			for (var i = 0; i < set.Count; i++)
			{
				Assert.NotNull(set[i]);
				Assert.NotNull(set.GetStyleName(i));
			}
		}

		[SkippableFact]
		public void TestCanCreateBoldFromIndex()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			int idx;
			for (idx = 0; idx < set.Count; idx++)
			{
				if (set[idx].Weight == (int)SKFontStyleWeight.Bold)
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
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var typeface = set.CreateTypeface(SKFontStyle.Bold);

			Assert.NotNull(typeface);
			Assert.Equal((int)SKFontStyleWeight.Bold, typeface.FontStyle.Weight);
		}

		[SkippableFact]
		public void TestCanIterate()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var count = 0;
			foreach (var style in set)
			{
				count++;
			}

			Assert.Equal(set.Count, count);
		}

		[SkippableFact]
		public void CreateTypefaceReturnsSameTypeface()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var tf1 = set.CreateTypeface(SKFontStyle.Normal);
			var tf2 = set.CreateTypeface(SKFontStyle.Normal);

			Assert.Same(tf1, tf2);
		}

		[SkippableFact]
		public unsafe void CreateTypefaceDisposeDoesNotDispose()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var tf1 = set.CreateTypeface(SKFontStyle.Normal);
			var tf2 = set.CreateTypeface(SKFontStyle.Normal);

			Assert.Same(tf1, tf2);

			tf1.Dispose();

			Assert.NotEqual(IntPtr.Zero, tf1.Handle);
			Assert.False(tf1.IsDisposed);
		}

		[SkippableFact]
		public void StyleReturnsSameTypeface()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles(DefaultFontFamily);
			var tf1 = set.CreateTypeface(SKFontStyle.Normal);

			var tf2 = fonts.MatchFamily(DefaultFontFamily, SKFontStyle.Normal);

			Assert.Same(tf1, tf2);
		}
	}
}
