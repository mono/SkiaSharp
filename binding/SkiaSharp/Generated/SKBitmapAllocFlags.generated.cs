using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_bitmap_allocflags_t
	/// <summary>Flags to use with <see cref="M:SkiaSharp.SKBitmap.#ctor" />.</summary>
	/// <remarks />
	[Flags]
	public enum SKBitmapAllocFlags {
		// NONE_SK_BITMAP_ALLOC_FLAGS = 0
		/// <summary>Default bitmap allocation flag.</summary>
		None = 0,
		// ZERO_PIXELS_SK_BITMAP_ALLOC_FLAGS = 1 << 0
		/// <summary>Initialize the bitmap with zeroed data.</summary>
		ZeroPixels = 1,
	}
}
