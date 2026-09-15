using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_image_rescale_mode_t
	/// <summary>Specifies the sampling algorithm used to rescale pixels during an asynchronous read-pixels operation.</summary>
	/// <remarks />
	public enum SKImageRescaleMode {
		// NEAREST_SK_IMAGE_RESCALE_MODE = 0
		/// <summary>Uses nearest-neighbor sampling.</summary>
		Nearest = 0,
		// LINEAR_SK_IMAGE_RESCALE_MODE = 1
		/// <summary>Uses a single bilinear sampling step.</summary>
		Linear = 1,
		// REPEATED_LINEAR_SK_IMAGE_RESCALE_MODE = 2
		/// <summary>Uses repeated bilinear sampling steps, halving the size each pass, for higher-quality downscaling.</summary>
		RepeatedLinear = 2,
		// REPEATED_CUBIC_SK_IMAGE_RESCALE_MODE = 3
		/// <summary>Uses repeated bicubic sampling steps, halving the size each pass, for the highest-quality downscaling.</summary>
		RepeatedCubic = 3,
	}
}
