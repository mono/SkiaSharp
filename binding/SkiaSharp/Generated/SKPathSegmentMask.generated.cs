using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_segment_mask_t
	[Flags]
	public enum SKPathSegmentMask {
		// LINE_SK_PATH_SEGMENT_MASK = 1 << 0
		Line = 1,
		// QUAD_SK_PATH_SEGMENT_MASK = 1 << 1
		Quad = 2,
		// CONIC_SK_PATH_SEGMENT_MASK = 1 << 2
		Conic = 4,
		// CUBIC_SK_PATH_SEGMENT_MASK = 1 << 3
		Cubic = 8,
	}
}
