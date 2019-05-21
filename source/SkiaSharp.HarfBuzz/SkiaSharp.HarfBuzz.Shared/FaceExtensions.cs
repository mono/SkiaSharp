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

			public unsafe IntPtr LoadTable(IntPtr face, Tag tag, IntPtr user_data)
			{
				if (typeface.TryGetTableData(tag, out var table))
				{
					fixed (byte* tablePtr = table)
					{
						var blob = new Blob((IntPtr)tablePtr, table.Length, MemoryMode.Writeable);

						return blob.Handle;
					}
				}

				return IntPtr.Zero;
			}
		}
	}
}
