using System;
using System.Collections.Generic;
using System.IO;

using HarfBuzzSharp;
using SkiaSharp.HarfBuzz;
using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFaceTest : SKTest
	{
		private static readonly Blob s_blob;

		static HbFaceTest()
		{
			s_blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			s_blob.MakeImmutable();
		}


		[SkippableFact]
		public void ShouldHaveGlyphCount()
		{
			using (var face = new Face(s_blob, 0))
			{
				Assert.Equal(1147, face.GlyphCount);
			}
		}

		[SkippableFact]
		public void ShouldBeImmutable()
		{
			using (var face = new Face(s_blob, 0))
			{
				face.MakeImmutable();

				Assert.True(face.IsImmutable);
			}
		}

		[SkippableFact]
		public void ShouldHaveIndex()
		{
			using (var face = new Face(s_blob, 0))
			{
				Assert.Equal(0, face.Index);
			}
		}

		[SkippableFact]
		public void ShouldHaveUnitsPerEm()
		{
			using (var face = new Face(s_blob, 0))
			{
				Assert.Equal(2048, face.UnitsPerEm);
			}
		}

		[SkippableFact]
		public void ShouldHaveTableTags()
		{
			using (var face = new Face(s_blob, 0))
			{
				Assert.Equal(20, face.Tables.Length);
			}
		}

		[SkippableFact]
		public void ShouldReferenceTable()
		{
			using (var face = new Face(s_blob, 0))
			using (var tableBlob = face.ReferenceTable(new Tag("post")))
			{
				Assert.Equal(13378, tableBlob.Length);
			}
		}
	}
}
