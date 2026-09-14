using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_color_palette_flags_t
	/// <summary>Flags for OpenType color palettes in CPAL table.</summary>
	/// <remarks />
	public enum OpenTypeColorPaletteFlags {
		// HB_OT_COLOR_PALETTE_FLAG_DEFAULT = 0x00000000u
		/// <summary>No special flags.</summary>
		Default = 0,
		// HB_OT_COLOR_PALETTE_FLAG_USABLE_WITH_LIGHT_BACKGROUND = 0x00000001u
		/// <summary>Palette is suitable for light backgrounds.</summary>
		UsableWithLightBackground = 1,
		// HB_OT_COLOR_PALETTE_FLAG_USABLE_WITH_DARK_BACKGROUND = 0x00000002u
		/// <summary>Palette is suitable for dark backgrounds.</summary>
		UsableWithDarkBackground = 2,
	}
}
