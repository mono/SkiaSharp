using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_bitmap_allocflags_t
	[Flags]
	public enum SKBitmapAllocFlags {
		// NONE_SK_BITMAP_ALLOC_FLAGS = 0
		None = 0,
		// ZERO_PIXELS_SK_BITMAP_ALLOC_FLAGS = 1 << 0
		ZeroPixels = 1,
	}
}
