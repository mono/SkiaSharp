using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFontFuncsTest : HBTest
	{
		[SkippableFact]
		public void ShouldSetGlyphFromNameDelegate()
		{
			var expected = 1337u;

			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetGlyphFromNameDelegate((Font f, object fd, string n, out uint g) =>
				{
					g = expected;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var glyph = font.GetGlyphFromName("0");

				Assert.Equal(expected, glyph);
			}
		}
	}
}
