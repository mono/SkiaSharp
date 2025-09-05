#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Various utility methods for when swizzling pixels.
	/// </summary>
	public static unsafe class SKSwizzle
	{
		/// <summary>
		/// Swizzles the byte order of 32-bit pixels, swapping R and B. (RGBA &lt;-&gt; BGRA)
		/// </summary>
		/// <param name="pixels">The pixel buffer to swizzle.</param>
		/// <param name="count">The size of the pixel buffers.</param>
		public static void SwapRedBlue (IntPtr pixels, int count) =>
			SwapRedBlue (pixels, pixels, count);

		/// <summary>
		/// Swizzles the byte order of 32-bit pixels, swapping R and B. (RGBA &lt;-&gt; BGRA)
		/// </summary>
		/// <param name="dest">The destination pixel buffer.</param>
		/// <param name="src">The source pixel buffer.</param>
		/// <param name="count">The size of the pixel buffers.</param>
		public static void SwapRedBlue (IntPtr dest, IntPtr src, int count)
		{
			if (dest == IntPtr.Zero) {
				throw new ArgumentException (nameof (dest));
			}
			if (src == IntPtr.Zero) {
				throw new ArgumentException (nameof (src));
			}

			SkiaApi.sk_swizzle_swap_rb ((uint*)dest, (uint*)src, count);
		}

		/// <summary>
		/// Swizzles the byte order of 32-bit pixels, swapping R and B. (RGBA &lt;-&gt; BGRA)
		/// </summary>
		/// <param name="pixels">The pixel buffer to swizzle.</param>
		public static void SwapRedBlue (Span<byte> pixels) =>
			SwapRedBlue (pixels, pixels, pixels.Length);

		/// <summary>
		/// Swizzles the byte order of 32-bit pixels, swapping R and B. (RGBA &lt;-&gt; BGRA)
		/// </summary>
		/// <param name="pixels">The pixel buffer to swizzle.</param>
		/// <param name="count">The size of the pixel buffers.</param>
		public static void SwapRedBlue (ReadOnlySpan<byte> pixels, int count) =>
			SwapRedBlue (pixels, pixels, count);

		/// <summary>
		/// Swizzles the byte order of 32-bit pixels, swapping R and B. (RGBA &lt;-&gt; BGRA)
		/// </summary>
		/// <param name="dest">The destination pixel buffer.</param>
		/// <param name="src">The source pixel buffer.</param>
		/// <param name="count">The size of the pixel buffers.</param>
		public static void SwapRedBlue (ReadOnlySpan<byte> dest, ReadOnlySpan<byte> src, int count)
		{
			if (dest == null) {
				throw new ArgumentNullException (nameof (dest));
			}
			if (src == null) {
				throw new ArgumentNullException (nameof (src));
			}

			fixed (byte* d = dest)
			fixed (byte* s = src) {
				SkiaApi.sk_swizzle_swap_rb ((uint*)d, (uint*)s, count);
			}
		}
	}
}
