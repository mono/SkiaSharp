using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_filter_mode_t
	/// <summary>Specifies the filtering algorithm for scaling and transforming images.</summary>
	/// <remarks />
	public enum SKFilterMode {
		// NEAREST_SK_FILTER_MODE = 0
		/// <summary>Nearest-neighbor sampling that uses the color of the single closest pixel.</summary>
		Nearest = 0,
		// LINEAR_SK_FILTER_MODE = 1
		/// <summary>Bilinear interpolation that samples the four nearest pixels and blends them.</summary>
		Linear = 1,
	}
}
