using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_font_style_slant_t
	/// <summary>Various font slants for use with <see cref="T:SkiaSharp.SKTypeface" />.</summary>
	/// <remarks />
	public enum SKFontStyleSlant {
		// UPRIGHT_SK_FONT_STYLE_SLANT = 0
		/// <summary>The upright/normal font slant.</summary>
		Upright = 0,
		// ITALIC_SK_FONT_STYLE_SLANT = 1
		/// <summary>The italic font slant, in which the slanted characters appear as they were designed.</summary>
		Italic = 1,
		// OBLIQUE_SK_FONT_STYLE_SLANT = 2
		/// <summary>The oblique font slant, in which the characters are artificially slanted.</summary>
		Oblique = 2,
	}
}
