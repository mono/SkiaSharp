using System;

namespace SkiaSharp
{
	internal static class ImageResamplerExt
	{
		public static unsafe Span<T> GetWritableRowSpanOf<T> (this SKPixmap map, int row, int count = 1)
			where T : unmanaged
		{
			if (map.RowBytes % sizeof (T) != 0)
				throw new ArgumentException ();

			return new Span<T> (
				map.GetPixels (0, row).ToPointer (),
				(map.RowBytes / sizeof (T)) * count);
		}

		public static Span<T> GetWritableSpanOf<T> (this SKPixmap map) where T : unmanaged
			=> GetWritableRowSpanOf<T> (map, 0, map.Height);
	}
}
