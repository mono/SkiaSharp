using System;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFontFuncsTest : HBTest
	{
		[SkippableFact]
		public void ShouldSetGlyphContourPointDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				var expected = 1337;

				fontFuncs.SetGlyphContourPointDelegate((Font f, object fd, uint g, uint p, out int px, out int py) =>
				{
					px = expected;
					py = expected;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetGlyphContourPoint('H', 0, out var x, out var y);

				Assert.Equal(expected, x);
			}
		}

		[SkippableFact]
		public void ShouldSetHorizontalFontExtentsDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				var expected = new FontExtents { Ascender = 1337 };

				fontFuncs.SetHorizontalFontExtentsDelegate((Font f, object fd, out FontExtents e) =>
				{
					e = expected;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetHorizontalFontExtents(out var extents);

				Assert.Equal(expected, extents);
			}
		}

		[SkippableFact]
		public void ShouldSetGlyphExtentsDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				var expected = new GlyphExtents { Height = 1337 };

				fontFuncs.SetGlyphExtentsDelegate((Font f, object fd, uint g, out GlyphExtents e) =>
				{
					e = expected;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetGlyphExtents('H', out var extents);

				Assert.Equal(expected, extents);
			}
		}

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

				font.TryGetGlyphFromName("H", out var glyph);

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

				font.TryGetGlyphName('H', out var name);

				Assert.Equal("H", name);
			}
		}

		[SkippableFact]
		public void ShouldSetHorizontalGlyphAdvanceDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetHorizontalGlyphAdvanceDelegate((f, fd, g) => 1337);

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var advance = font.GetHorizontalGlyphAdvance(49);

				Assert.Equal(1337, advance);
			}
		}

		[SkippableFact]
		public void ShouldSetNominalGlyphDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetNominalGlyphDelegate((Font f, object fd, uint u, out uint g) =>
				{
					g = 1337u;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetNominalGlyph(49, out var glyph);

				Assert.Equal(1337u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldSetNominalGlyphsDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetNominalGlyphsDelegate((_, __, c, u, g) =>
				{
					g[0] = 1337;
					return 1;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetNominalGlyph(49, out var glyph);

				Assert.Equal(1337u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldSetHorizontalGlyphAdvancesDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetHorizontalGlyphAdvancesDelegate((f, fd, c, g, a) =>
				{
					a[0] = 1337;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var advance = font.GetHorizontalGlyphAdvance(49u);

				Assert.Equal(1337, advance);
			}
		}

		[SkippableFact]
		public void ShouldSetHorizontalGlyphOriginDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetHorizontalGlyphOriginDelegate((Font f, object fd, uint g, out int px, out int py) =>
				{
					px = 1337;
					py = 1337;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetHorizontalGlyphOrigin(49, out var x, out var y);

				Assert.Equal(1337, x);
			}
		}

		[SkippableFact]
		public void ShouldSetVariationGlyphDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetVariationGlyphDelegate((Font _, object __, uint u, uint v, out uint g) =>
				{
					g = 1337;
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				font.TryGetVariationGlyph(49, 0, out var glyph);

				Assert.Equal(1337u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldSetHorizontalGlyphKerningDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetHorizontalGlyphKerningDelegate((_, __, f, s) => 1337);

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var kerning = font.GetHorizontalGlyphKerning(49, 50);

				Assert.Equal(1337, kerning);
			}
		}
	}
}
