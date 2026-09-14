using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_color_palette_flags_t
	public enum OpenTypeColorPaletteFlags {
		// HB_OT_COLOR_PALETTE_FLAG_DEFAULT = 0x00000000u
		Default = 0,
		// HB_OT_COLOR_PALETTE_FLAG_USABLE_WITH_LIGHT_BACKGROUND = 0x00000001u
		UsableWithLightBackground = 1,
		// HB_OT_COLOR_PALETTE_FLAG_USABLE_WITH_DARK_BACKGROUND = 0x00000002u
		UsableWithDarkBackground = 2,
	}
}
