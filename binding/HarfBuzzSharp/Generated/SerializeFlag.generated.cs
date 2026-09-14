using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_serialize_flags_t
	[Flags]
	public enum SerializeFlag {
		// HB_BUFFER_SERIALIZE_FLAG_DEFAULT = 0x00000000u
		Default = 0,
		// HB_BUFFER_SERIALIZE_FLAG_NO_CLUSTERS = 0x00000001u
		NoClusters = 1,
		// HB_BUFFER_SERIALIZE_FLAG_NO_POSITIONS = 0x00000002u
		NoPositions = 2,
		// HB_BUFFER_SERIALIZE_FLAG_NO_GLYPH_NAMES = 0x00000004u
		NoGlyphNames = 4,
		// HB_BUFFER_SERIALIZE_FLAG_GLYPH_EXTENTS = 0x00000008u
		GlyphExtents = 8,
		// HB_BUFFER_SERIALIZE_FLAG_GLYPH_FLAGS = 0x00000010u
		GlyphFlags = 16,
		// HB_BUFFER_SERIALIZE_FLAG_NO_ADVANCES = 0x00000020u
		NoAdvances = 32,
	}
}
