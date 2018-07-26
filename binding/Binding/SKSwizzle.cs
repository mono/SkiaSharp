using System;

namespace SkiaSharp
{
	public static class SKSwizzle
	{
		public static void SwapRedBlue (IntPtr pixels, int count)
		{
			SwapRedBlue (pixels, pixels, count);
		}

		public static void SwapRedBlue (IntPtr dest, IntPtr src, int count)
		{
			if (dest == IntPtr.Zero) {
				throw new ArgumentException (nameof (dest));
			}
			if (src == IntPtr.Zero) {
				throw new ArgumentException (nameof (src));
			}

			SkiaApi.sk_swizzle_swap_rb (dest, src, count);
		}
	}
}
