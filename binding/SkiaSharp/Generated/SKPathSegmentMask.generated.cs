using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_segment_mask_t
	/// <summary>The flags used by <see cref="P:SkiaSharp.SKPath.SegmentMasks" />.</summary>
	/// <remarks />
	[Flags]
	public enum SKPathSegmentMask {
		// LINE_SK_PATH_SEGMENT_MASK = 1 << 0
		/// <summary>The path contains one or more line segments.</summary>
		Line = 1,
		// QUAD_SK_PATH_SEGMENT_MASK = 1 << 1
		/// <summary>The path contains one or more quad segments.</summary>
		Quad = 2,
		// CONIC_SK_PATH_SEGMENT_MASK = 1 << 2
		/// <summary>The path contains one or more conic segments.</summary>
		Conic = 4,
		// CUBIC_SK_PATH_SEGMENT_MASK = 1 << 3
		/// <summary>The path contains one or more cubic segments.</summary>
		Cubic = 8,
	}
}
