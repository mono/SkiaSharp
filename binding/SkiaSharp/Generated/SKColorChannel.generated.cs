using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_color_channel_t
	/// <summary>Identifies a specific color channel in an RGBA color.</summary>
	/// <remarks />
	public enum SKColorChannel {
		// R_SK_COLOR_CHANNEL = 0
		/// <summary>The red channel.</summary>
		R = 0,
		// G_SK_COLOR_CHANNEL = 1
		/// <summary>The green channel.</summary>
		G = 1,
		// B_SK_COLOR_CHANNEL = 2
		/// <summary>The blue channel.</summary>
		B = 2,
		// A_SK_COLOR_CHANNEL = 3
		/// <summary>The alpha (transparency) channel.</summary>
		A = 3,
	}
}
