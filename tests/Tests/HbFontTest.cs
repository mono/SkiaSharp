using System;
using System.IO;

using HarfBuzzSharp;

using SkiaSharp.HarfBuzz;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFontTest : SKTest
	{
		[SkippableFact]
		public void ShouldHaveDefaultSupportedShapers()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			using (var font = new Font(face))
			{
				Assert.Equal(new[] { "ot", "fallback" }, font.SupportedShapers);
			}
		}

		[SkippableFact]
		public void ShouldGetGlyphByUnicode()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			using (var font = new Font(face))
			{
				Assert.Equal(42, font.GetGlyph('A'));
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnGetHorizontalGlyphAdvance()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => font.GetHorizontalGlyphAdvance(-1));
			}
		}

		[SkippableFact]
		public void ShouldThrowInvalidOperationExceptionOnGetGlyphExtents()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			using (var font = new Font(face))
			{
				Assert.Throws<ArgumentOutOfRangeException>(() => font.GetGlyphExtents(-1));
			}
		}

		[SkippableFact]
		public void ShouldHaveDefaultScale()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			using (var font = new Font(face))
			{
				Assert.Equal(new Scale(2048, 2048), font.Scale);
			}
		}
	}
}
