using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_direction_t
	public enum Direction {
		// HB_DIRECTION_INVALID = 0
		Invalid = 0,
		// HB_DIRECTION_LTR = 4
		LeftToRight = 4,
		// HB_DIRECTION_RTL = 5
		RightToLeft = 5,
		// HB_DIRECTION_TTB = 6
		TopToBottom = 6,
		// HB_DIRECTION_BTT = 7
		BottomToTop = 7,
	}
}
