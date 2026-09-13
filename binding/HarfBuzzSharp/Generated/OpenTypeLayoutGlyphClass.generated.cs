using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_layout_glyph_class_t
	/// <summary>Specifies the glyph class for OpenType layout operations.</summary>
	/// <remarks />
	public enum OpenTypeLayoutGlyphClass {
		// HB_OT_LAYOUT_GLYPH_CLASS_UNCLASSIFIED = 0
		/// <summary>The glyph has no class definition.</summary>
		Unclassified = 0,
		// HB_OT_LAYOUT_GLYPH_CLASS_BASE_GLYPH = 1
		/// <summary>A base glyph that acts as a single character.</summary>
		BaseGlyph = 1,
		// HB_OT_LAYOUT_GLYPH_CLASS_LIGATURE = 2
		/// <summary>A ligature glyph composed of multiple characters.</summary>
		Ligature = 2,
		// HB_OT_LAYOUT_GLYPH_CLASS_MARK = 3
		/// <summary>A combining mark glyph that attaches to a base glyph.</summary>
		Mark = 3,
		// HB_OT_LAYOUT_GLYPH_CLASS_COMPONENT = 4
		/// <summary>A glyph that is a component of a ligature.</summary>
		Component = 4,
	}
}
