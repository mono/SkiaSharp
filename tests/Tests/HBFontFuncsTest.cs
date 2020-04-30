using System;
using System.Buffers;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFontFuncsTest : HBTest
	{
		[SkippableFact]
		public void ImmutableFunctionsShouldNotChange()
		{
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.MakeImmutable();
				Assert.Throws<InvalidOperationException>(() => fontFuncs.SetHorizontalGlyphAdvanceDelegate((a, b, c) => 1337));
			}
		}

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

				var result = font.TryGetGlyphContourPoint('H', 0, out var x, out var y);

				Assert.True(result);
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

				var result = font.TryGetHorizontalFontExtents(out var extents);

				Assert.True(result);
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

				var result = font.TryGetGlyphExtents('H', out var extents);

				Assert.True(result);
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

				var result = font.TryGetGlyphFromName("H", out var glyph);

				Assert.True(result);
				Assert.Equal('H', glyph);
			}
		}

		[SkippableFact]
		public void ShouldSetGlyphNameDelegate()
		{
			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetGlyphNameDelegate((Font f, object fd, uint g, out string n) =>
				{
					n = ((char)g).ToString();
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var result = font.TryGetGlyphName('H', out var name);

				Assert.True(result);
				Assert.Equal("H", name);
			}
		}

		[SkippableFact]
		public void TryGetGlyphNameIsCorrectWithDelegate()
		{
			// get an array and fill it with things
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent(Font.NameBufferLength);
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte)i;
			pool.Return(buffer);

			using (var font = new Font(Font))
			using (var fontFuncs = new FontFunctions())
			{
				fontFuncs.SetGlyphNameDelegate((Font f, object fd, uint g, out string n) =>
				{
					n = ((char)g).ToString();
					return true;
				});

				fontFuncs.MakeImmutable();

				font.SetFontFunctions(fontFuncs, "FontData");

				var result = font.TryGetGlyphName('H', out var name);

				Assert.True(result);
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

				var result = font.TryGetNominalGlyph(49, out var glyph);

				Assert.True(result);
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

				var result = font.TryGetNominalGlyph(49, out var glyph);

				Assert.True(result);
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

				var result = font.TryGetHorizontalGlyphOrigin(49, out var x, out _);

				Assert.True(result);
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

				var result = font.TryGetVariationGlyph(49, 0, out var glyph);

				Assert.True(result);
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
