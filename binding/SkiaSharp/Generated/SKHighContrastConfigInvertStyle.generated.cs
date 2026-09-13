using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_highcontrastconfig_invertstyle_t
	/// <summary>Various invert styles for high contrast calculations.</summary>
	/// <remarks />
	public enum SKHighContrastConfigInvertStyle {
		// NO_INVERT_SK_HIGH_CONTRAST_CONFIG_INVERT_STYLE = 0
		/// <summary>Do not invert.</summary>
		NoInvert = 0,
		// INVERT_BRIGHTNESS_SK_HIGH_CONTRAST_CONFIG_INVERT_STYLE = 1
		/// <summary>Invert the brightness.</summary>
		InvertBrightness = 1,
		// INVERT_LIGHTNESS_SK_HIGH_CONTRAST_CONFIG_INVERT_STYLE = 2
		/// <summary>Invert the lightness.</summary>
		InvertLightness = 2,
	}
}
