using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_stroke_cap_t
	/// <summary>Various options for <see cref="P:SkiaSharp.SKPaint.StrokeCap" />.</summary>
	/// <remarks>This is the treatment that is applied to the beginning and end of each non-closed contour (e.g. lines).</remarks>
	public enum SKStrokeCap {
		// BUTT_SK_STROKE_CAP = 0
		/// <summary>Begin/end contours with no extension.</summary>
		Butt = 0,
		// ROUND_SK_STROKE_CAP = 1
		/// <summary>Begin/end contours with a semi-circle extension.</summary>
		Round = 1,
		// SQUARE_SK_STROKE_CAP = 2
		/// <summary>Begin/end contours with a half square extension.</summary>
		Square = 2,
	}
}
