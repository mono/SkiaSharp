using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pixelgeometry_t
	/// <summary>Describes how LCD strips are organized for each pixel.</summary>
	/// <remarks>Description of how the LCD strips are arranged for each pixel. If this is unknown, or the pixels are meant to be "portable" and/or transformed before showing (e.g. rotated, scaled) then use <see cref="F:SkiaSharp.SKPixelGeometry.Unknown" />.</remarks>
	public enum SKPixelGeometry {
		// UNKNOWN_SK_PIXELGEOMETRY = 0
		/// <summary>Use if the order is not known or the pixels are meant to be "portable" and/or transformed before showing (e.g. rotated, scaled).</summary>
		Unknown = 0,
		// RGB_H_SK_PIXELGEOMETRY = 1
		/// <summary>Pixels are made up horizontal red, green and blue lights.</summary>
		RgbHorizontal = 1,
		// BGR_H_SK_PIXELGEOMETRY = 2
		/// <summary>Pixels are made up horizontal blue, green and red lights.</summary>
		BgrHorizontal = 2,
		// RGB_V_SK_PIXELGEOMETRY = 3
		/// <summary>Pixels are made up vertical red, green and blue lights.</summary>
		RgbVertical = 3,
		// BGR_V_SK_PIXELGEOMETRY = 4
		/// <summary>Pixels are made up vertical blue, green and red lights.</summary>
		BgrVertical = 4,
	}
}
