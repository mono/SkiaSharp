using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_rrect_corner_t
	/// <summary>Represents the corners of a rounded rectangle.</summary>
	/// <remarks />
	public enum SKRoundRectCorner {
		// UPPER_LEFT_SK_RRECT_CORNER = 0
		/// <summary>The upper-left or top-left corner.</summary>
		UpperLeft = 0,
		// UPPER_RIGHT_SK_RRECT_CORNER = 1
		/// <summary>The upper-right or top-right corner.</summary>
		UpperRight = 1,
		// LOWER_RIGHT_SK_RRECT_CORNER = 2
		/// <summary>The lower-right or bottom-right corner.</summary>
		LowerRight = 2,
		// LOWER_LEFT_SK_RRECT_CORNER = 3
		/// <summary>The lower-left or bottom-left corner.</summary>
		LowerLeft = 3,
	}
}
