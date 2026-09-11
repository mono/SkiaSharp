using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_content_type_t
	public enum ContentType {
		// HB_BUFFER_CONTENT_TYPE_INVALID = 0
		Invalid = 0,
		// HB_BUFFER_CONTENT_TYPE_UNICODE = 1
		Unicode = 1,
		// HB_BUFFER_CONTENT_TYPE_GLYPHS = 2
		Glyphs = 2,
	}
}
