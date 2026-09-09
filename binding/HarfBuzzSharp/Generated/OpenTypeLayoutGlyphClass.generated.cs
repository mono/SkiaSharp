using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_layout_glyph_class_t
	public enum OpenTypeLayoutGlyphClass {
		// HB_OT_LAYOUT_GLYPH_CLASS_UNCLASSIFIED = 0
		Unclassified = 0,
		// HB_OT_LAYOUT_GLYPH_CLASS_BASE_GLYPH = 1
		BaseGlyph = 1,
		// HB_OT_LAYOUT_GLYPH_CLASS_LIGATURE = 2
		Ligature = 2,
		// HB_OT_LAYOUT_GLYPH_CLASS_MARK = 3
		Mark = 3,
		// HB_OT_LAYOUT_GLYPH_CLASS_COMPONENT = 4
		Component = 4,
	}
}
