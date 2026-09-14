using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_text_encoding_t
	/// <summary>Possible text encodings.</summary>
	/// <remarks />
	public enum SKTextEncoding {
		// UTF8_SK_TEXT_ENCODING = 0
		/// <summary>The buffer contains UTF-8 encoded characters.</summary>
		Utf8 = 0,
		// UTF16_SK_TEXT_ENCODING = 1
		/// <summary>The buffer contains UTF-16 encoded characters.</summary>
		Utf16 = 1,
		// UTF32_SK_TEXT_ENCODING = 2
		/// <summary>The buffer contains UTF-32 encoded characters.</summary>
		Utf32 = 2,
		// GLYPH_ID_SK_TEXT_ENCODING = 3
		/// <summary>The buffer contains glyph ids.</summary>
		GlyphId = 3,
	}
}
