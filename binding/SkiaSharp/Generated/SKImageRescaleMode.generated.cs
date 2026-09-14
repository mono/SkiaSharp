using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_image_rescale_mode_t
	public enum SKImageRescaleMode {
		// NEAREST_SK_IMAGE_RESCALE_MODE = 0
		Nearest = 0,
		// LINEAR_SK_IMAGE_RESCALE_MODE = 1
		Linear = 1,
		// REPEATED_LINEAR_SK_IMAGE_RESCALE_MODE = 2
		RepeatedLinear = 2,
		// REPEATED_CUBIC_SK_IMAGE_RESCALE_MODE = 3
		RepeatedCubic = 3,
	}
}
