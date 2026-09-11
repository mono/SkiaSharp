using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_rrect_type_t
	public enum SKRoundRectType {
		// EMPTY_SK_RRECT_TYPE = 0
		Empty = 0,
		// RECT_SK_RRECT_TYPE = 1
		Rect = 1,
		// OVAL_SK_RRECT_TYPE = 2
		Oval = 2,
		// SIMPLE_SK_RRECT_TYPE = 3
		Simple = 3,
		// NINE_PATCH_SK_RRECT_TYPE = 4
		NinePatch = 4,
		// COMPLEX_SK_RRECT_TYPE = 5
		Complex = 5,
	}
}
