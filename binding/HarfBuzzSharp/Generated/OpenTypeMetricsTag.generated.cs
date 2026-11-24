using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_metrics_tag_t
	/// <summary>Specifies OpenType font metrics tags for querying font-wide metrics.</summary>
	/// <remarks />
	public enum OpenTypeMetricsTag {
		// HB_OT_METRICS_TAG_HORIZONTAL_ASCENDER = 1751216995
		/// <summary>Font metric value.</summary>
		HorizontalAscender = 1751216995,
		// HB_OT_METRICS_TAG_HORIZONTAL_DESCENDER = 1751413603
		/// <summary>Font metric value.</summary>
		HorizontalDescender = 1751413603,
		// HB_OT_METRICS_TAG_HORIZONTAL_LINE_GAP = 1751934832
		/// <summary>Font metric value.</summary>
		HorizontalLineGap = 1751934832,
		// HB_OT_METRICS_TAG_HORIZONTAL_CLIPPING_ASCENT = 1751346273
		/// <summary>Font metric value.</summary>
		HorizontalClippingAscent = 1751346273,
		// HB_OT_METRICS_TAG_HORIZONTAL_CLIPPING_DESCENT = 1751346276
		/// <summary>Font metric value.</summary>
		HorizontalClippingDescent = 1751346276,
		// HB_OT_METRICS_TAG_VERTICAL_ASCENDER = 1986098019
		/// <summary>Font metric value.</summary>
		VerticalAscender = 1986098019,
		// HB_OT_METRICS_TAG_VERTICAL_DESCENDER = 1986294627
		/// <summary>Font metric value.</summary>
		VerticalDescender = 1986294627,
		// HB_OT_METRICS_TAG_VERTICAL_LINE_GAP = 1986815856
		/// <summary>Font metric value.</summary>
		VerticalLineGap = 1986815856,
		// HB_OT_METRICS_TAG_HORIZONTAL_CARET_RISE = 1751347827
		/// <summary>Font metric value.</summary>
		HorizontalCaretRise = 1751347827,
		// HB_OT_METRICS_TAG_HORIZONTAL_CARET_RUN = 1751347822
		/// <summary>Font metric value.</summary>
		HorizontalCaretRun = 1751347822,
		// HB_OT_METRICS_TAG_HORIZONTAL_CARET_OFFSET = 1751347046
		/// <summary>Font metric value.</summary>
		HorizontalCaretOffset = 1751347046,
		// HB_OT_METRICS_TAG_VERTICAL_CARET_RISE = 1986228851
		/// <summary>Font metric value.</summary>
		VerticalCaretRise = 1986228851,
		// HB_OT_METRICS_TAG_VERTICAL_CARET_RUN = 1986228846
		/// <summary>Font metric value.</summary>
		VerticalCaretRun = 1986228846,
		// HB_OT_METRICS_TAG_VERTICAL_CARET_OFFSET = 1986228070
		/// <summary>Font metric value.</summary>
		VerticalCaretOffset = 1986228070,
		// HB_OT_METRICS_TAG_X_HEIGHT = 2020108148
		/// <summary>Font metric value.</summary>
		XHeight = 2020108148,
		// HB_OT_METRICS_TAG_CAP_HEIGHT = 1668311156
		/// <summary>Font metric value.</summary>
		CapHeight = 1668311156,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_X_SIZE = 1935833203
		/// <summary>Font metric value.</summary>
		SubScriptEmXSize = 1935833203,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_Y_SIZE = 1935833459
		/// <summary>Font metric value.</summary>
		SubScriptEmYSize = 1935833459,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_X_OFFSET = 1935833199
		/// <summary>Font metric value.</summary>
		SubScriptEmXOffset = 1935833199,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_Y_OFFSET = 1935833455
		/// <summary>Font metric value.</summary>
		SubScriptEmYOffset = 1935833455,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_X_SIZE = 1936750707
		/// <summary>Font metric value.</summary>
		SuperScriptEmXSize = 1936750707,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_Y_SIZE = 1936750963
		/// <summary>Font metric value.</summary>
		SuperScriptEmYSize = 1936750963,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_X_OFFSET = 1936750703
		/// <summary>Font metric value.</summary>
		SuperScriptEmXOffset = 1936750703,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_Y_OFFSET = 1936750959
		/// <summary>Font metric value.</summary>
		SuperScriptEmYOffset = 1936750959,
		// HB_OT_METRICS_TAG_STRIKEOUT_SIZE = 1937011315
		/// <summary>Font metric value.</summary>
		StrikeoutSize = 1937011315,
		// HB_OT_METRICS_TAG_STRIKEOUT_OFFSET = 1937011311
		/// <summary>Font metric value.</summary>
		StrikeoutOffset = 1937011311,
		// HB_OT_METRICS_TAG_UNDERLINE_SIZE = 1970168947
		/// <summary>Font metric value.</summary>
		UnderlineSize = 1970168947,
		// HB_OT_METRICS_TAG_UNDERLINE_OFFSET = 1970168943
		/// <summary>Font metric value.</summary>
		UnderlineOffset = 1970168943,
	}
}
