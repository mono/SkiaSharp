using System;
using System.Buffers;
using System.Text;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HbFontTest : HBTest
	{
		[SkippableFact]
		public void ShouldHaveDefaultSupportedShapers()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new[] { "ot", "fallback" }, font.SupportedShapers);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphByUnicode()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetGlyph('A', out var glyph));
				Assert.Equal(42u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultScale()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetScale(out var xScale, out var yScale);
				Assert.Equal(2048, xScale);
				Assert.Equal(2048, yScale);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetHorizontalGlyphOrigin(49, out var xOrigin, out var yOrigin);
				Assert.Equal(0, xOrigin);
				Assert.Equal(0, yOrigin);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetVerticalGlyphOrigin(49, out var xOrigin, out var yOrigin));
				Assert.Equal(557, xOrigin);
				Assert.Equal(1022, yOrigin);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(1114, font.GetHorizontalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvance()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(-2048, font.GetVerticalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvances()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetHorizontalGlyphAdvances(new[] { 49u, 50u, 51u });

				Assert.Equal(1114, advances[0]);
				Assert.Equal(514, advances[1]);
				Assert.Equal(602, advances[2]);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvances()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetVerticalGlyphAdvances(new[] { 49u, 50u, 51u });

				Assert.Equal(-2048, advances[0]);
				Assert.Equal(-2048, advances[1]);
				Assert.Equal(-2048, advances[2]);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphName()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetGlyphName(49, out var name);

				Assert.Equal("H", name);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphFromName()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.TryGetGlyphFromName("H", out var glyph);

				Assert.Equal(49u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphFromString()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.True(font.TryGetGlyphFromString("H", out var glyph));
				Assert.Equal(49u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldConvertGlyphToString()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var s = font.GlyphToString(49);

				Assert.Equal("H", s);
			}
		}

		[SkippableFact]
		public void GlyphToStringIsCorrectWithDelegate()
		{
			// get an array and fill it with things
			var pool = ArrayPool<byte>.Shared;
			var buffer = pool.Rent(Font.NameBufferLength);
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = (byte)i;
			pool.Return(buffer);

			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var s = font.GlyphToString(49);

				Assert.Equal("H", s);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphContourPointForOrigin()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.False(font.TryGetGlyphContourPointForOrigin(49, 0, Direction.LeftToRight, out _, out _));
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphContourPoint()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				Assert.False(font.TryGetGlyphContourPoint(49, 0, out _, out _));
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphAdvanceForDirection()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				font.GetGlyphAdvanceForDirection(49, Direction.LeftToRight, out var x, out _);

				Assert.Equal(1114, x);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphKerning()
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var kerning = font.GetHorizontalGlyphKerning(49, 50);

				Assert.Equal(0, kerning);
			}
		}

		[Theory]
		[InlineData(OpenTypeMetricsTag.UnderlineOffset, -60)]
		[InlineData(OpenTypeMetricsTag.UnderlineSize, 9)]
		[InlineData(OpenTypeMetricsTag.StrikeoutOffset, 1049)]
		[InlineData(OpenTypeMetricsTag.StrikeoutSize, 209)]
		public void ShouldGetOpenTypeMetrics(OpenTypeMetricsTag tag, int expected)
		{
			using (var face = new Face(Blob, 0))
			using (var font = new Font(face))
			{
				var result = font.OpenTypeMetrics.TryGetPosition(tag, out var position);

				Assert.True(result);

				Assert.Equal(expected, position);
			}
		}
	}
}
