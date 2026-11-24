using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_jpegencoder_alphaoption_t
	/// <summary>Various options to control how alpha should be handled.</summary>
	/// <remarks />
	public enum SKJpegEncoderAlphaOption {
		// IGNORE_SK_JPEGENCODER_ALPHA_OPTION = 0
		/// <summary>Ignore the alpha channel and treat the image as opaque.</summary>
		Ignore = 0,
		// BLEND_ON_BLACK_SK_JPEGENCODER_ALPHA_OPTION = 1
		/// <summary>Blend the pixels onto a black background before encoding.</summary>
		BlendOnBlack = 1,
	}
}
