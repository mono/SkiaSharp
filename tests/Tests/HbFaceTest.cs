using System;
using System.IO;

using HarfBuzzSharp;
using Xunit;

namespace SkiaSharp.Tests
{
	public class HbFaceTest : SKTest
	{
		private static readonly SKTypeface Typeface;
		public static readonly Blob Blob;

		static HbFaceTest()
		{
			Blob = Blob.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
			Blob.MakeImmutable();

			Typeface = SKTypeface.FromFile(Path.Combine(PathToFonts, "content-font.ttf"));
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

		private class SKTypefaceTableLoader : TableLoader
		{
			private readonly SKTypeface _typeface;

			public SKTypefaceTableLoader(SKTypeface typeface)
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
			using (var face = new Face(new SKTypefaceTableLoader(Typeface)))
			using (var font = new Font(face))
			{
				using (var buffer = new HarfBuzzSharp.Buffer())
				{
					buffer.AddUtf16("Hello");

					font.Shape(buffer);
				}
			}
		}
	}
}
