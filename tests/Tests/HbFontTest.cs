using System;

using HarfBuzzSharp;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFontTest : SKTest
	{
		[SkippableFact]
		public void ShouldHaveDefaultSupportedShapers()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(new[] { "ot", "fallback" }, font.SupportedShapers);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphByUnicode()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				var glyph = font.GetGlyph('A');

				Assert.Equal(42u, glyph);
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => font.GetHorizontalGlyphAdvance(-1));
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnGetGlyphExtents()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => font.GetGlyphExtents(-1));
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultScale()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
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
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				font.GetHorizontalGlyphOrigin(49, out var xOrigin, out var yOrigin);
				Assert.Equal(0, xOrigin);
				Assert.Equal(0, yOrigin);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphOrigin()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				font.GetVerticalGlyphOrigin(49, out var xOrigin, out var yOrigin);
				Assert.Equal(557, xOrigin);
				Assert.Equal(1022, yOrigin);
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvance()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(1114, font.GetHorizontalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvance()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				Assert.Equal(-2048, font.GetVerticalGlyphAdvance(49));
			}
		}

		[SkippableFact]
		public void ShouldGetHorizontalGlyphAdvances()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetHorizontalGlyphAdvances(new[] { 49, 50, 51 });

				Assert.Equal(1114, advances[0]);
				Assert.Equal(514, advances[1]);
				Assert.Equal(602, advances[2]);
			}
		}

		[SkippableFact]
		public void ShouldGetVerticalGlyphAdvances()
		{
			using (var face = new Face(HbFaceTest.Blob, 0))
			using (var font = new Font(face))
			{
				var advances = font.GetVerticalGlyphAdvances(new[] { 49, 50, 51 });

				Assert.Equal(-2048, advances[0]);
				Assert.Equal(-2048, advances[1]);
				Assert.Equal(-2048, advances[2]);
			}
		}
	}
}
