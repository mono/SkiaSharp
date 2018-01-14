//
// Bindings for Skia's swizzle features
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public static class SKSwizzle
	{
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
