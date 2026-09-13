using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_font_edging_t
	/// <summary>Specifies the type of edge smoothing to use when rendering glyphs.</summary>
	/// <remarks />
	public enum SKFontEdging {
		// ALIAS_SK_FONT_EDGING = 0
		/// <summary>No edge smoothing; glyphs are rendered with hard, aliased edges.</summary>
		Alias = 0,
		// ANTIALIAS_SK_FONT_EDGING = 1
		/// <summary>Grayscale antialiasing for smooth edges using alpha blending.</summary>
		Antialias = 1,
		// SUBPIXEL_ANTIALIAS_SK_FONT_EDGING = 2
		/// <summary>Subpixel antialiasing that uses LCD pixel geometry for improved clarity on LCD displays.</summary>
		SubpixelAntialias = 2,
	}
}
