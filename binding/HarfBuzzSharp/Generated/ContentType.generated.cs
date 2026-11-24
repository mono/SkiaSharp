using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_content_type_t
	/// <summary>The various types of buffer contents.</summary>
	/// <remarks />
	public enum ContentType {
		// HB_BUFFER_CONTENT_TYPE_INVALID = 0
		/// <summary>Initial value for new buffer.</summary>
		Invalid = 0,
		// HB_BUFFER_CONTENT_TYPE_UNICODE = 1
		/// <summary>The buffer contains input characters (before shaping).</summary>
		Unicode = 1,
		// HB_BUFFER_CONTENT_TYPE_GLYPHS = 2
		/// <summary>The buffer contains output glyphs (after shaping).</summary>
		Glyphs = 2,
	}
}
