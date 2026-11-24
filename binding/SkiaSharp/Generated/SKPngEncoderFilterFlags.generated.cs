using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pngencoder_filterflags_t
	/// <summary>Various row filters to use when encoding a PNG.</summary>
	/// <remarks />
	[Flags]
	public enum SKPngEncoderFilterFlags {
		// ZERO_SK_PNGENCODER_FILTER_FLAGS = 0x00
		/// <summary>Do not use any filters.</summary>
		NoFilters = 0,
		// NONE_SK_PNGENCODER_FILTER_FLAGS = 0x08
		/// <summary>Transmit unmodified.</summary>
		None = 8,
		// SUB_SK_PNGENCODER_FILTER_FLAGS = 0x10
		/// <summary>Transmits the difference between each byte and the value of the corresponding byte of the prior pixel: Sub(x) = Raw(x) - Raw(x-bpp).</summary>
		Sub = 16,
		// UP_SK_PNGENCODER_FILTER_FLAGS = 0x20
		/// <summary>Transmits the difference between each byte and the value of the corresponding byte of the pixel above: Up(x) = Raw(x) - Prior(x).</summary>
		Up = 32,
		// AVG_SK_PNGENCODER_FILTER_FLAGS = 0x40
		/// <summary>Uses the average of the two neighboring pixels (left and above) to predict the value of a pixel: Average(x) = Raw(x) - floor((Raw(x-bpp)+Prior(x))/2).</summary>
		Avg = 64,
		// PAETH_SK_PNGENCODER_FILTER_FLAGS = 0x80
		/// <summary>Computes a simple linear function of the three neighboring pixels (left, above, upper left), then chooses as predictor the neighboring pixel closest to the computed value: Paeth(x) = Raw(x) - PaethPredictor(Raw(x-bpp), Prior(x), Prior(x-bpp)).</summary>
		Paeth = 128,
		// ALL_SK_PNGENCODER_FILTER_FLAGS = NONE_SK_PNGENCODER_FILTER_FLAGS | SUB_SK_PNGENCODER_FILTER_FLAGS | UP_SK_PNGENCODER_FILTER_FLAGS | AVG_SK_PNGENCODER_FILTER_FLAGS | PAETH_SK_PNGENCODER_FILTER_FLAGS
		/// <summary>Try all the filters.</summary>
		AllFilters = 248,
	}
}
