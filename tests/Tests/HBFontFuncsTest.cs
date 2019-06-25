using System;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFontFuncsTest : HBTest
	{
		[SkippableFact]
		public void ShouldSetGlyphFromNameDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetGlyphFromNameDelegate((Font f, object fd, string n, out uint g) =>
				{
					g = n[0];
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var glyph = font.GetGlyphFromName("H");

				Assert.Equal('H', glyph);
			}
		}

		[SkippableFact]
		public void ShouldSetGlyphNameDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				unsafe
				{
					fontFuncs.SetGlyphNameDelegate((f, fd, g, nb, s) =>
					{
						var nameSpan = new Span<char>(nb, s);

						nameSpan[0] = (char)g;

						return true;
					});
				}

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var glyphName = font.GetGlyphName('H');

				Assert.Equal("H", glyphName);
			}
		}
	}
}
