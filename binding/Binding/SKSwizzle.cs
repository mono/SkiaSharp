using System;

namespace SkiaSharp
{
	public static unsafe class SKSwizzle
	{
		public static void SwapRedBlue (IntPtr pixels, int count) =>
			SwapRedBlue (pixels, pixels, count);

		public static void SwapRedBlue (IntPtr dest, IntPtr src, int count)
		{
			if (dest == IntPtr.Zero)
				throw new ArgumentException (nameof (dest));
			if (src == IntPtr.Zero)
				throw new ArgumentException (nameof (src));

			SkiaApi.sk_swizzle_swap_rb ((uint*)dest, (uint*)src, count);
		}

		public static void SwapRedBlue (Span<byte> pixels) =>
			SwapRedBlue (pixels, pixels, pixels.Length);

		public static void SwapRedBlue (Span<byte> pixels, int count) =>
			SwapRedBlue (pixels, pixels, count);

		public static void SwapRedBlue (Span<byte> dest, ReadOnlySpan<byte> src, int count)
		{
			if (dest.Length != count)
				throw new ArgumentException ("The number of pixels in the destination does not match the number of pixels being swapped.", nameof (dest));
			if (src.Length != count)
				throw new ArgumentException ("The number of pixels in the source does not match the number of pixels being swapped.", nameof (src));

			fixed (byte* d = dest)
			fixed (byte* s = src) {
				SkiaApi.sk_swizzle_swap_rb ((uint*)d, (uint*)s, count);
			}
		}
	}
}
