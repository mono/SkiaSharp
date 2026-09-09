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
	}
}
