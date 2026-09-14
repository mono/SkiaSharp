using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_serialize_format_t
	public enum SerializeFormat {
		// HB_BUFFER_SERIALIZE_FORMAT_TEXT = 1413830740
		Text = 1413830740,
		// HB_BUFFER_SERIALIZE_FORMAT_JSON = 1246973774
		Json = 1246973774,
		// HB_BUFFER_SERIALIZE_FORMAT_INVALID = 0
		Invalid = 0,
	}
}
