using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_stroke_join_t
	/// <summary>Join style for stroking operations.</summary>
	/// <remarks>This is the treatment that is applied to corners in paths and rectangles.</remarks>
	public enum SKStrokeJoin {
		// MITER_SK_STROKE_JOIN = 0
		/// <summary>Connect path segments with a sharp join.</summary>
		Miter = 0,
		// ROUND_SK_STROKE_JOIN = 1
		/// <summary>Connect path segments with a round join.</summary>
		Round = 1,
		// BEVEL_SK_STROKE_JOIN = 2
		/// <summary>Connect path segments with a flat bevel join.</summary>
		Bevel = 2,
	}
}
