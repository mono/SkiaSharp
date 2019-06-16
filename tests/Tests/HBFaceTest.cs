using System.Linq.Expressions;
using Xunit;

namespace HarfBuzzSharp.Tests
{
	public class HBFaceTest : HBTest
	{
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

		[SkippableFact]
		public void ShouldCreateWithTableFunc()
		{
			var tag = new Tag("kern");

			using (var face = new Face((f, t, u) => Face.ReferenceTable(t).Handle))
			{
				var blob = face.ReferenceTable(tag);

				Assert.Equal(Face.ReferenceTable(tag).Handle, blob.Handle);
			}
		}
	}
}
