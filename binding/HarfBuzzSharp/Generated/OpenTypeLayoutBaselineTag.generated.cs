using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_layout_baseline_tag_t
	/// <summary>Baseline tags from the OpenType BASE table.</summary>
	/// <remarks />
	public enum OpenTypeLayoutBaselineTag {
		// HB_OT_LAYOUT_BASELINE_TAG_ROMAN = 1919905134
		/// <summary>Roman baseline.</summary>
		Roman = 1919905134,
		// HB_OT_LAYOUT_BASELINE_TAG_HANGING = 1751215719
		/// <summary>Hanging baseline for Indic scripts.</summary>
		Hanging = 1751215719,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_FACE_BOTTOM_OR_LEFT = 1768121954
		/// <summary>Ideographic face bottom or left baseline.</summary>
		IdeoFaceBottomOrLeft = 1768121954,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_FACE_TOP_OR_RIGHT = 1768121972
		/// <summary>Ideographic face top or right baseline.</summary>
		IdeoFaceTopOrRight = 1768121972,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_FACE_CENTRAL = 1231251043
		/// <summary>Ideographic face central baseline.</summary>
		IdeoFaceCentral = 1231251043,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_EMBOX_BOTTOM_OR_LEFT = 1768187247
		/// <summary>Ideographic em-box bottom or left baseline.</summary>
		IdeoEmboxBottomOrLeft = 1768187247,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_EMBOX_TOP_OR_RIGHT = 1768191088
		/// <summary>Ideographic em-box top or right baseline.</summary>
		IdeoEmboxTopOrRight = 1768191088,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_EMBOX_CENTRAL = 1231315813
		/// <summary>Ideographic em-box central baseline.</summary>
		IdeoEmboxCentral = 1231315813,
		// HB_OT_LAYOUT_BASELINE_TAG_MATH = 1835103336
		/// <summary>Math baseline.</summary>
		Math = 1835103336,
	}
}
