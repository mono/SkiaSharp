using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_jpegencoder_downsample_t
	/// <summary>Various options for the downsampling factor of the U and V components.</summary>
	/// <remarks />
	public enum SKJpegEncoderDownsample {
		// DOWNSAMPLE_420_SK_JPEGENCODER_DOWNSAMPLE = 0
		/// <summary>Reduction by a factor of two in both the horizontal and vertical directions.</summary>
		Downsample420 = 0,
		// DOWNSAMPLE_422_SK_JPEGENCODER_DOWNSAMPLE = 1
		/// <summary>Reduction by a factor of two in the horizontal direction.</summary>
		Downsample422 = 1,
		// DOWNSAMPLE_444_SK_JPEGENCODER_DOWNSAMPLE = 2
		/// <summary>No downsampling.</summary>
		Downsample444 = 2,
	}
}
