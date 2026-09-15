using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_flags_t
	/// <summary>Represents the various glyph flags of a <see cref="T:HarfBuzzSharp.GlyphInfo" />.</summary>
	/// <remarks />
	[Flags]
	public enum GlyphFlags {
		// HB_GLYPH_FLAG_UNSAFE_TO_BREAK = 0x00000001
		/// <summary>If input text is broken at the beginning of the cluster this glyph is part of, then both sides need to be re-shaped, as the result might be different.</summary>
		UnsafeToBreak = 1,
		// HB_GLYPH_FLAG_UNSAFE_TO_CONCAT = 0x00000002
		/// <summary>Indicates that concatenating text at this glyph may require reshaping both sides.</summary>
		UnsafeToConcat = 2,
		// HB_GLYPH_FLAG_SAFE_TO_INSERT_TATWEEL = 0x00000004
		/// <summary>Indicates that a tatweel can be inserted before this glyph without reshaping.</summary>
		SafeToInsertTatweel = 4,
	}
}
