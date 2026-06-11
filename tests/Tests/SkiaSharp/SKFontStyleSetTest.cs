using System;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFontStyleSetTest : SKTest
	{
		[Fact]
		public void TestCanCreateEmpty()
		{
			var set = new SKFontStyleSet();

			Assert.Empty(set);
		}

		[Fact]
		public void TestFindsNothing()
		{
			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles("Missing Font");
			Assert.NotNull(set);

			Assert.Empty(set);
		}

		[Fact]
		public void TestSetHasAtLeastOne()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;

			var set = fonts.GetFontStyles(DefaultFontFamily);
			Assert.NotNull(set);

			Assert.True(set.Count > 0);
		}

		[Fact]
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

		[Fact]
		public void TestCanCreateBoldFromIndex()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

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

		[Fact]
		public void TestCanCreateBold()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var typeface = set.CreateTypeface(SKFontStyle.Bold);

			Assert.NotNull(typeface);
			Assert.Equal((int)SKFontStyleWeight.Bold, typeface.FontStyle.Weight);
		}

		[Fact]
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

		[Fact]
		public void CreateTypefaceReturnsSameTypeface()
		{
			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var tf1 = set.CreateTypeface(SKFontStyle.Normal);
			var tf2 = set.CreateTypeface(SKFontStyle.Normal);

			Assert.Same(tf1, tf2);
		}

		[Fact]
		public unsafe void CreateTypefaceDisposeDoesNotDispose()
		{
			SkipOnPlatform(IsBrowser, "WASM has no system font manager");

			var fonts = SKFontManager.Default;
			var set = fonts.GetFontStyles(DefaultFontFamily);

			var tf1 = set.CreateTypeface(SKFontStyle.Normal);
			var tf2 = set.CreateTypeface(SKFontStyle.Normal);

			Assert.Same(tf1, tf2);

			tf1.Dispose();

			Assert.NotEqual(IntPtr.Zero, tf1.Handle);
			Assert.False(tf1.IsDisposed);
		}

		[Fact]
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
