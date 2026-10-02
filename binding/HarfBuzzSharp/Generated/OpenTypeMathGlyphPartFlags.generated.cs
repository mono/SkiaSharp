using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_glyph_part_flags_t
	/// <summary>Flags for OpenType math glyph parts.</summary>
	/// <remarks />
	public enum OpenTypeMathGlyphPartFlags {
		// HB_OT_MATH_GLYPH_PART_FLAG_EXTENDER = 0x00000001u
		/// <summary>Indicates the glyph part can be repeated for extension.</summary>
		Extender = 1,
	}
}
