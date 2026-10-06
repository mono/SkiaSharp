using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_rrect_type_t
	/// <summary>Represents the various sub-types of rounded rectangles.</summary>
	/// <remarks />
	public enum SKRoundRectType {
		// EMPTY_SK_RRECT_TYPE = 0
		/// <summary>An empty (all zero) rounded rectangle.</summary>
		Empty = 0,
		// RECT_SK_RRECT_TYPE = 1
		/// <summary>A non-empty rounded rectangle with zero radii at all corners.</summary>
		Rect = 1,
		// OVAL_SK_RRECT_TYPE = 2
		/// <summary>A non-empty rounded rectangle with the x-radii equal to half the width and the y-radii equal to half the height.</summary>
		Oval = 2,
		// SIMPLE_SK_RRECT_TYPE = 3
		/// <summary>A non-empty rounded rectangle with equal x-radii and equal y-radii.</summary>
		Simple = 3,
		// NINE_PATCH_SK_RRECT_TYPE = 4
		/// <summary>A non-empty rounded rectangle where the left x-radii are equal, the top y-radii are equal, the right x-radii are equal and the bottom y-radii are equal.</summary>
		NinePatch = 4,
		// COMPLEX_SK_RRECT_TYPE = 5
		/// <summary>A non-empty rounded rectangle with at least one corner non-zero.</summary>
		Complex = 5,
	}
}
