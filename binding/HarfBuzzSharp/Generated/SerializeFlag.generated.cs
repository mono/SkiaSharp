using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_serialize_flags_t
	/// <summary>The various flags that control what glyph information are serialized by <see cref="M:HarfBuzzSharp.Buffer.SerializeGlyphs(System.Int32,System.Int32,HarfBuzzSharp.Font,HarfBuzzSharp.SerializeFormat,HarfBuzzSharp.SerializeFlag)" />.</summary>
	/// <remarks />
	[Flags]
	public enum SerializeFlag {
		// HB_BUFFER_SERIALIZE_FLAG_DEFAULT = 0x00000000u
		/// <summary>Serialize glyph names, clusters and position information.</summary>
		Default = 0,
		// HB_BUFFER_SERIALIZE_FLAG_NO_CLUSTERS = 0x00000001u
		/// <summary>Do not serialize glyph clusters.</summary>
		NoClusters = 1,
		// HB_BUFFER_SERIALIZE_FLAG_NO_POSITIONS = 0x00000002u
		/// <summary>Do not serialize glyph position information.</summary>
		NoPositions = 2,
		// HB_BUFFER_SERIALIZE_FLAG_NO_GLYPH_NAMES = 0x00000004u
		/// <summary>Do not serialize glyph names.</summary>
		NoGlyphNames = 4,
		// HB_BUFFER_SERIALIZE_FLAG_GLYPH_EXTENTS = 0x00000008u
		/// <summary>Serialize glyph extents.</summary>
		GlyphExtents = 8,
		// HB_BUFFER_SERIALIZE_FLAG_GLYPH_FLAGS = 0x00000010u
		/// <summary>Serialize glyph flags.</summary>
		GlyphFlags = 16,
		// HB_BUFFER_SERIALIZE_FLAG_NO_ADVANCES = 0x00000020u
		/// <summary>Do not serialize glyph advances (glyph offsets will reflect absolute glyph positions).</summary>
		NoAdvances = 32,
	}
}
