using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_image_rescale_gamma_t
	/// <summary>Specifies the gamma space in which rescaling is performed during an asynchronous read-pixels operation.</summary>
	/// <remarks />
	public enum SKImageRescaleGamma {
		// SRC_SK_IMAGE_RESCALE_GAMMA = 0
		/// <summary>Rescaling is performed in the color space of the source pixels.</summary>
		Src = 0,
		// LINEAR_SK_IMAGE_RESCALE_GAMMA = 1
		/// <summary>Rescaling is performed in a linear gamma space.</summary>
		Linear = 1,
	}
}
