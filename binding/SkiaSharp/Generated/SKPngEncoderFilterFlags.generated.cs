using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pngencoder_filterflags_t
	[Flags]
	public enum SKPngEncoderFilterFlags {
		// ZERO_SK_PNGENCODER_FILTER_FLAGS = 0x00
		NoFilters = 0,
		// NONE_SK_PNGENCODER_FILTER_FLAGS = 0x08
		None = 8,
		// SUB_SK_PNGENCODER_FILTER_FLAGS = 0x10
		Sub = 16,
		// UP_SK_PNGENCODER_FILTER_FLAGS = 0x20
		Up = 32,
		// AVG_SK_PNGENCODER_FILTER_FLAGS = 0x40
		Avg = 64,
		// PAETH_SK_PNGENCODER_FILTER_FLAGS = 0x80
		Paeth = 128,
		// ALL_SK_PNGENCODER_FILTER_FLAGS = NONE_SK_PNGENCODER_FILTER_FLAGS | SUB_SK_PNGENCODER_FILTER_FLAGS | UP_SK_PNGENCODER_FILTER_FLAGS | AVG_SK_PNGENCODER_FILTER_FLAGS | PAETH_SK_PNGENCODER_FILTER_FLAGS
		AllFilters = 248,
	}
}
