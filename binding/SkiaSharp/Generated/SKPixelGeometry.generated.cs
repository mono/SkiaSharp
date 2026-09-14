using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pixelgeometry_t
	public enum SKPixelGeometry {
		// UNKNOWN_SK_PIXELGEOMETRY = 0
		Unknown = 0,
		// RGB_H_SK_PIXELGEOMETRY = 1
		RgbHorizontal = 1,
		// BGR_H_SK_PIXELGEOMETRY = 2
		BgrHorizontal = 2,
		// RGB_V_SK_PIXELGEOMETRY = 3
		RgbVertical = 3,
		// BGR_V_SK_PIXELGEOMETRY = 4
		BgrVertical = 4,
	}
}
