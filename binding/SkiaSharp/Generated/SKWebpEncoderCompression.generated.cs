using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_webpencoder_compression_t
	/// <summary>The various types of compression for WEBP files.</summary>
	/// <remarks />
	public enum SKWebpEncoderCompression {
		// LOSSY_SK_WEBPENCODER_COMPTRESSION = 0
		/// <summary>Compress the files by reducing image quality.</summary>
		Lossy = 0,
		// LOSSLESS_SK_WEBPENCODER_COMPTRESSION = 1
		/// <summary>Compress the file without loosing data.</summary>
		Lossless = 1,
	}
}
