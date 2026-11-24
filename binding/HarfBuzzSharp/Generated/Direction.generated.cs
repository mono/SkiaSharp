using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_direction_t
	/// <summary>Various text directions that can be set via <see cref="P:HarfBuzzSharp.Buffer.Direction" />.</summary>
	/// <remarks />
	public enum Direction {
		// HB_DIRECTION_INVALID = 0
		/// <summary>Initial, unset direction.</summary>
		Invalid = 0,
		// HB_DIRECTION_LTR = 4
		/// <summary>Text is set horizontally from left to right.</summary>
		LeftToRight = 4,
		// HB_DIRECTION_RTL = 5
		/// <summary>Text is set horizontally from right to left.</summary>
		RightToLeft = 5,
		// HB_DIRECTION_TTB = 6
		/// <summary>Text is set vertically from top to bottom.</summary>
		TopToBottom = 6,
		// HB_DIRECTION_BTT = 7
		/// <summary>Text is set vertically from bottom to top.</summary>
		BottomToTop = 7,
	}
}
