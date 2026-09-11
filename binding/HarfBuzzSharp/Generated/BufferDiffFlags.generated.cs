using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_diff_flags_t
	public enum BufferDiffFlags {
		// HB_BUFFER_DIFF_FLAG_EQUAL = 0x0000
		Equal = 0,
		// HB_BUFFER_DIFF_FLAG_CONTENT_TYPE_MISMATCH = 0x0001
		ContentTypeMismatch = 1,
		// HB_BUFFER_DIFF_FLAG_LENGTH_MISMATCH = 0x0002
		LengthMismatch = 2,
		// HB_BUFFER_DIFF_FLAG_NOTDEF_PRESENT = 0x0004
		NotdefPresent = 4,
		// HB_BUFFER_DIFF_FLAG_DOTTED_CIRCLE_PRESENT = 0x0008
		DottedCirclePresent = 8,
		// HB_BUFFER_DIFF_FLAG_CODEPOINT_MISMATCH = 0x0010
		CodepointMismatch = 16,
		// HB_BUFFER_DIFF_FLAG_CLUSTER_MISMATCH = 0x0020
		ClusterMismatch = 32,
		// HB_BUFFER_DIFF_FLAG_GLYPH_FLAGS_MISMATCH = 0x0040
		GlyphFlagsMismatch = 64,
		// HB_BUFFER_DIFF_FLAG_POSITION_MISMATCH = 0x0080
		PositionMismatch = 128,
	}
}
