using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_encodedorigin_t
	/// <summary>Represents various origin values returned by <see cref="P:SkiaSharp.SKCodec.EncodedOrigin" />.</summary>
	/// <remarks />
	public enum SKEncodedOrigin {
		// TOP_LEFT_SK_ENCODED_ORIGIN = 1
		/// <summary>Default.</summary>
		TopLeft = 1,
		// TOP_RIGHT_SK_ENCODED_ORIGIN = 2
		/// <summary>Reflected across y-axis.</summary>
		TopRight = 2,
		// BOTTOM_RIGHT_SK_ENCODED_ORIGIN = 3
		/// <summary>Rotated 180°.</summary>
		BottomRight = 3,
		// BOTTOM_LEFT_SK_ENCODED_ORIGIN = 4
		/// <summary>Reflected across x-axis.</summary>
		BottomLeft = 4,
		// LEFT_TOP_SK_ENCODED_ORIGIN = 5
		/// <summary>Reflected across x-axis. Rotated 90° counter-clockwise.</summary>
		LeftTop = 5,
		// RIGHT_TOP_SK_ENCODED_ORIGIN = 6
		/// <summary>Rotated 90° clockwise.</summary>
		RightTop = 6,
		// RIGHT_BOTTOM_SK_ENCODED_ORIGIN = 7
		/// <summary>Reflected across x-axis. Rotated 90° clockwise.</summary>
		RightBottom = 7,
		// LEFT_BOTTOM_SK_ENCODED_ORIGIN = 8
		/// <summary>Rotated 90° counter-clockwise.</summary>
		LeftBottom = 8,
		// DEFAULT_SK_ENCODED_ORIGIN = TOP_LEFT_SK_ENCODED_ORIGIN
		/// <summary>This is equivalent to <see cref="F:SkiaSharp.SKEncodedOrigin.TopLeft" />.</summary>
		Default = 1,
	}
}
