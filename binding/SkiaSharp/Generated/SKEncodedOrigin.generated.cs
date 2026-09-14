using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_encodedorigin_t
	public enum SKEncodedOrigin {
		// TOP_LEFT_SK_ENCODED_ORIGIN = 1
		TopLeft = 1,
		// TOP_RIGHT_SK_ENCODED_ORIGIN = 2
		TopRight = 2,
		// BOTTOM_RIGHT_SK_ENCODED_ORIGIN = 3
		BottomRight = 3,
		// BOTTOM_LEFT_SK_ENCODED_ORIGIN = 4
		BottomLeft = 4,
		// LEFT_TOP_SK_ENCODED_ORIGIN = 5
		LeftTop = 5,
		// RIGHT_TOP_SK_ENCODED_ORIGIN = 6
		RightTop = 6,
		// RIGHT_BOTTOM_SK_ENCODED_ORIGIN = 7
		RightBottom = 7,
		// LEFT_BOTTOM_SK_ENCODED_ORIGIN = 8
		LeftBottom = 8,
		// DEFAULT_SK_ENCODED_ORIGIN = TOP_LEFT_SK_ENCODED_ORIGIN
		Default = 1,
	}
}
