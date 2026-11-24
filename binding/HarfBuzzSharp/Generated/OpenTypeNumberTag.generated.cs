using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_number_tag_t
	/// <summary>Specifies numeric values stored in OpenType font tables.</summary>
	/// <remarks />
	public enum OpenTypeNumberTag {
		// HB_OT_NUMBER_TAG_FONT_X_MIN = 2020436334
		/// <summary>The minimum horizontal coordinate from the font bounding box in the <c>head</c> table.</summary>
		FontXMin = 2020436334,
		// HB_OT_NUMBER_TAG_FONT_Y_MIN = 2037213550
		/// <summary>The minimum vertical coordinate from the font bounding box in the <c>head</c> table.</summary>
		FontYMin = 2037213550,
		// HB_OT_NUMBER_TAG_FONT_X_MAX = 2020434296
		/// <summary>The maximum horizontal coordinate from the font bounding box in the <c>head</c> table.</summary>
		FontXMax = 2020434296,
		// HB_OT_NUMBER_TAG_FONT_Y_MAX = 2037211512
		/// <summary>The maximum vertical coordinate from the font bounding box in the <c>head</c> table.</summary>
		FontYMax = 2037211512,
	}
}
