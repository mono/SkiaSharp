using System;

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
			using (var tableBlob = face.ReferenceTable(Tag.Parse("post")))
			{
				Assert.Equal(13378, tableBlob.Length);
			}
		}

		[SkippableFact]
		public void ShouldCreateWithTableFunc()
		{
			var tag = Tag.Parse("kern");

			using (var face = new Face((f, t) => Face.ReferenceTable(t)))
			{
				var blob = face.ReferenceTable(tag);

				Assert.Equal(Face.ReferenceTable(tag).Handle, blob.Handle);
			}
		}

		[SkippableFact]
		public void DelegateBasedConstructionSucceededs()
		{
			var didReference = 0;
			var didDestroy = 0;

			var tag = Tag.Parse("kern");

			Face face = null;
			face = new Face(
				(f, t) =>
				{
					Assert.Equal(face, f);

					didReference++;
					return Face.ReferenceTable(t);
				},
				() =>
				{
					didDestroy++;
				});

			Assert.Equal(0, didReference);
			Assert.Equal(0, didDestroy);

			var blob1 = face.ReferenceTable(tag);

			Assert.Equal(1, didReference);
			Assert.Equal(0, didDestroy);
			Assert.NotNull(blob1);

			var blob2 = face.ReferenceTable(tag);

			Assert.Equal(2, didReference);
			Assert.Equal(0, didDestroy);
			Assert.NotNull(blob2);

			Assert.Equal(blob1.Handle, blob2.Handle);

			face.Dispose();

			Assert.Equal(2, didReference);
			Assert.Equal(1, didDestroy);
		}


		[SkippableFact]
		public void EmptyFacesAreExactlyTheSameInstance()
		{
			var emptyFace1 = Face.Empty;
			var emptyFace2 = Face.Empty;

			Assert.Equal(emptyFace1, emptyFace2);
			Assert.Equal(emptyFace1.Handle, emptyFace2.Handle);
			Assert.Same(emptyFace1, emptyFace2);
		}

		[SkippableFact]
		public void EmptyFacesAreNotDisposed()
		{
			var emptyFace = Face.Empty;
			emptyFace.Dispose();

			Assert.False(emptyFace.IsDisposed());
			Assert.NotEqual(IntPtr.Zero, emptyFace.Handle);
		}
	}
}
