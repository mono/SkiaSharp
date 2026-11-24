using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_font_hinting_t
	/// <summary>Specifies the level of hinting to be performed when rendering glyphs.</summary>
	/// <remarks />
	public enum SKFontHinting {
		// NONE_SK_FONT_HINTING = 0
		/// <summary>Do not apply any hinting to glyph outlines.</summary>
		None = 0,
		// SLIGHT_SK_FONT_HINTING = 1
		/// <summary>Use slight hinting to improve contrast without changing glyph shapes.</summary>
		Slight = 1,
		// NORMAL_SK_FONT_HINTING = 2
		/// <summary>Use a normal amount of hinting to improve glyph rendering at small sizes.</summary>
		Normal = 2,
		// FULL_SK_FONT_HINTING = 3
		/// <summary>Use the full amount of hinting for maximum adjustment of glyph outlines to the pixel grid.</summary>
		Full = 3,
	}
}
