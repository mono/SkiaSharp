using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_flags_t
	[Flags]
	public enum GlyphFlags {
		// HB_GLYPH_FLAG_UNSAFE_TO_BREAK = 0x00000001
		UnsafeToBreak = 1,
		// HB_GLYPH_FLAG_UNSAFE_TO_CONCAT = 0x00000002
		UnsafeToConcat = 2,
		// HB_GLYPH_FLAG_SAFE_TO_INSERT_TATWEEL = 0x00000004
		SafeToInsertTatweel = 4,
	}
}
