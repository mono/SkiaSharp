using System;
using System.IO;

using HarfBuzzSharp;
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

		private class TypefaceTableLoader : TableLoader
		{
			private readonly SKTypeface _typeface;

			public TypefaceTableLoader(SKTypeface typeface)
			{
				_typeface = typeface;
			}

			protected override unsafe Blob Load(Tag tag)
			{
				if (_typeface.TryGetTableData(tag, out var table))
				{
					fixed (byte* tablePtr = table)
					{
						return new Blob((IntPtr)tablePtr, table.Length, MemoryMode.Duplicate);
					}
				}

				return null;
			}
		}

		[SkippableFact]
		public void ShouldCreateForTables()
		{
			using (var typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf")))
			using (var face = new Face(new TypefaceTableLoader(typeface)))
			using (var font = new Font(face))
			using (var buffer = new HarfBuzzSharp.Buffer())
			{
				buffer.AddUtf16("Hello");

				font.Shape(buffer);
			}
		}
	}
}
