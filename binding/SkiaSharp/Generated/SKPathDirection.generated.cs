using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_direction_t
	/// <summary>Direction for path contours.</summary>
	/// <remarks />
	public enum SKPathDirection {
		// CW_SK_PATH_DIRECTION = 0
		/// <summary>Clockwise direction for adding closed contours.</summary>
		Clockwise = 0,
		// CCW_SK_PATH_DIRECTION = 1
		/// <summary>Counter-clockwise direction for adding closed contours.</summary>
		CounterClockwise = 1,
	}
}
