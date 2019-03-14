using System.IO;

using HarfBuzzSharp;

using SkiaSharp.HarfBuzz;

using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFaceTests : SKTest
	{
		[SkippableFact]
		public void ShouldHaveGlyphCount()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			{
				Assert.Equal(1147, face.GlyphCount);
			}
		}

		[SkippableFact]
		public void ShouldBeImmutable()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			{
				face.MakeImmutable();

				Assert.True(face.IsImmutable);
			}
		}

		[SkippableFact]
		public void ShouldHaveIndex()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			{
				Assert.Equal(0, face.Index);
			}
		}

		[SkippableFact]
		public void ShouldHaveUnitsPerEm()
		{
			using (var tf = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var blob = tf.OpenStream(out var index).ToHarfBuzzBlob())
			using (var face = new Face(blob, index))
			{
				Assert.Equal(2048, face.UnitsPerEm);
			}
		}
	}
}
