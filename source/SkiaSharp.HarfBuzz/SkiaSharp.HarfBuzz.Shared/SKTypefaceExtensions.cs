using System;
using HarfBuzzSharp;

namespace SkiaSharp.HarfBuzz
{
	public static class SKTypefaceExtensions
	{
		public static Face ToHarfBuzzFace(this SKTypeface typeface) =>
			new Face(new SKTypefaceTableLoader(typeface).Load);

		private class SKTypefaceTableLoader
		{
			private readonly SKTypeface typeface;

			public SKTypefaceTableLoader(SKTypeface typeface)
			{
				this.typeface = typeface;
			}

			public unsafe IntPtr Load(IntPtr face, Tag tag, IntPtr userData)
			{
				if (typeface.TryGetTableData(tag, out var table))
				{
					fixed (byte* tablePtr = table)
					{
						var blob = new Blob((IntPtr)tablePtr, table.Length, MemoryMode.ReadOnly);

						return blob.Handle;
					}
				}

				return IntPtr.Zero;
			}
		}
	}
}
