using System;
using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public static class SKTypefaceExtensions
	{
		public static Face ToHarfBuzzFace(this SKTypeface typeface)
		{
			return new Face(new SKTypefaceTableLoader(typeface));
		}

		private class SKTypefaceTableLoader : TableLoader
		{
			private readonly SKTypeface typeface;

			public SKTypefaceTableLoader(SKTypeface typeface)
			{
				this.typeface = typeface;
			}

			protected override unsafe Blob Load(Tag tag)
			{
				if (typeface.TryGetTableData(tag, out var table))
				{
					fixed (byte* tablePtr = table)
					{
						return new Blob((IntPtr)tablePtr, table.Length, MemoryMode.Duplicate);
					}
				}

				return null;
			}
		}
	}
}
