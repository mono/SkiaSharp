using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_serialize_format_t
	/// <summary>The various serialization and de-serialization formats.</summary>
	/// <remarks />
	public enum SerializeFormat {
		// HB_BUFFER_SERIALIZE_FORMAT_TEXT = 1413830740
		/// <summary>A human-readable, plain text format.</summary>
		Text = 1413830740,
		// HB_BUFFER_SERIALIZE_FORMAT_JSON = 1246973774
		/// <summary>A machine-readable JSON format.</summary>
		Json = 1246973774,
		// HB_BUFFER_SERIALIZE_FORMAT_INVALID = 0
		/// <summary>The format is invalid.</summary>
		Invalid = 0,
	}
}
