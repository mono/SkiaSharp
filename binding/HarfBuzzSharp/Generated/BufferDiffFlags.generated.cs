using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_diff_flags_t
	/// <summary>Flags indicating differences between two buffers when compared.</summary>
	/// <remarks />
	public enum BufferDiffFlags {
		// HB_BUFFER_DIFF_FLAG_EQUAL = 0x0000
		/// <summary>The buffers are equal.</summary>
		Equal = 0,
		// HB_BUFFER_DIFF_FLAG_CONTENT_TYPE_MISMATCH = 0x0001
		/// <summary>Content types differ between the buffers.</summary>
		ContentTypeMismatch = 1,
		// HB_BUFFER_DIFF_FLAG_LENGTH_MISMATCH = 0x0002
		/// <summary>Buffer lengths differ.</summary>
		LengthMismatch = 2,
		// HB_BUFFER_DIFF_FLAG_NOTDEF_PRESENT = 0x0004
		/// <summary>A .notdef glyph is present in one of the buffers.</summary>
		NotdefPresent = 4,
		// HB_BUFFER_DIFF_FLAG_DOTTED_CIRCLE_PRESENT = 0x0008
		/// <summary>A dotted circle glyph is present in one of the buffers.</summary>
		DottedCirclePresent = 8,
		// HB_BUFFER_DIFF_FLAG_CODEPOINT_MISMATCH = 0x0010
		/// <summary>Codepoints or glyph indices differ between the buffers.</summary>
		CodepointMismatch = 16,
		// HB_BUFFER_DIFF_FLAG_CLUSTER_MISMATCH = 0x0020
		/// <summary>Cluster values differ between the buffers.</summary>
		ClusterMismatch = 32,
		// HB_BUFFER_DIFF_FLAG_GLYPH_FLAGS_MISMATCH = 0x0040
		/// <summary>Glyph flags differ between the buffers.</summary>
		GlyphFlagsMismatch = 64,
		// HB_BUFFER_DIFF_FLAG_POSITION_MISMATCH = 0x0080
		/// <summary>Glyph positions differ between the buffers.</summary>
		PositionMismatch = 128,
	}
}
