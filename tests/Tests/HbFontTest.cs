using System;
using System.IO;

using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFontTest : SKTest
	{
		private static readonly Blob s_blob;

		static HbFontTest()
		{
			s_blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			s_blob.MakeImmutable();
		}

		[SkippableFact]
		public void ShouldHaveDefaultSupportedShapers()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new[] { "ot", "fallback" }, font.SupportedShapers);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphByUnicode()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				var glyph = font.GetGlyph('A');

				Assert.Equal(42u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => font.GetHorizontalGlyphAdvance(-1));
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnGetGlyphExtents()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => font.GetGlyphExtents(-1));
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultScale()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new Point(2048, 2048), font.Scale);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphOrigin()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new Point(), font.GetHorizontalGlyphOrigin(49));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphOrigin()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new Point(557, 1022), font.GetVerticalGlyphOrigin(49));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(1114, font.GetHorizontalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvance()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(-2048, font.GetVerticalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvances()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetHorizontalGlyphAdvances(new[] { 49, 50, 51 });

				Assert.Equal(1114, advances[0]);
				Assert.Equal(0, advances[1]);
				Assert.Equal(0, advances[2]);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvances()
		{
			using (var face = new Face(s_blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetVerticalGlyphAdvances(new[] { 49, 50, 51 });

				Assert.Equal(-2048, advances[0]);
				Assert.Equal(0, advances[1]);
				Assert.Equal(0, advances[2]);
			}
		}
	}
}
