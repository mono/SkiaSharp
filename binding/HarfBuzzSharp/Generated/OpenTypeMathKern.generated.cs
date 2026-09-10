using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_kern_t
	/// <summary>Specifies math kerning positions in the OpenType MATH table.</summary>
	/// <remarks />
	public enum OpenTypeMathKern {
		// HB_OT_MATH_KERN_TOP_RIGHT = 0
		/// <summary>Top-right kerning position.</summary>
		TopRight = 0,
		// HB_OT_MATH_KERN_TOP_LEFT = 1
		/// <summary>Top-left kerning position.</summary>
		TopLeft = 1,
		// HB_OT_MATH_KERN_BOTTOM_RIGHT = 2
		/// <summary>Bottom-right kerning position.</summary>
		BottomRight = 2,
		// HB_OT_MATH_KERN_BOTTOM_LEFT = 3
		/// <summary>Bottom-left kerning position.</summary>
		BottomLeft = 3,
	}
}
