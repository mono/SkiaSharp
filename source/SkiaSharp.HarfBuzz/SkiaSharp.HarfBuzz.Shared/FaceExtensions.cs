using System;
using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public static class FaceExtensions
	{
		public static Face ToHarfBuzzFace(this SKTypeface typeface)
		{
			var loader = new TypefaceTableLoader(typeface);

			return new Face(loader.LoadTable);
		}

		private struct TypefaceTableLoader
		{
			private readonly SKTypeface typeface;

			public TypefaceTableLoader(SKTypeface typeface)
			{
				this.typeface = typeface;
			}

			public unsafe Blob LoadTable(Face face, Tag tag)
			{
				if (typeface.TryGetTableData(tag, out var table))
				{
					fixed (byte* tablePtr = table)
					{
						return new Blob((IntPtr)tablePtr, table.Length, MemoryMode.Writeable);
					}
				}

				return null;
			}
		}
	}
}
