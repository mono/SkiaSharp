using System.IO;

using HarfBuzzSharp;
using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFaceTest : SKTest
	{
		public static readonly Blob Blob;

		static HbFaceTest()
		{
			Blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			Blob.MakeImmutable();
		}

		[SkippableFact]
		public void ShouldHaveGlyphCount()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(1147, face.GlyphCount);
			}
		}

		[SkippableFact]
		public void ShouldBeImmutable()
		{
			using (var face = new Face(Blob, 0))
			{
				face.MakeImmutable();

				Assert.True(face.IsImmutable);
			}
		}

		[SkippableFact]
		public void ShouldHaveIndex()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(0, face.Index);
			}
		}

		[SkippableFact]
		public void ShouldHaveUnitsPerEm()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(2048, face.UnitsPerEm);
			}
		}

		[SkippableFact]
		public void ShouldHaveTableTags()
		{
			using (var face = new Face(Blob, 0))
			{
				Assert.Equal(20, face.Tables.Length);
			}
		}

		[SkippableFact]
		public void ShouldReferenceTable()
		{
			using (var face = new Face(Blob, 0))
			using (var tableBlob = face.ReferenceTable(new Tag("post")))
			{
				Assert.Equal(13378, tableBlob.Length);
			}
		}
	}
}
