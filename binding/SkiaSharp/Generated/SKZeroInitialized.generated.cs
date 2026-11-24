using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_zero_initialized_t
	/// <summary>Specifies whether the memory passed to <see cref="M:SkiaSharp.SKCodec.GetPixels(SkiaSharp.SKImageInfo,System.IntPtr,SkiaSharp.SKCodecOptions)" /> (or one of the overloads that accepts a <see cref="T:SkiaSharp.SKCodecOptions" />) is zero initialized.</summary>
	/// <remarks />
	public enum SKZeroInitialized {
		// YES_SK_CODEC_ZERO_INITIALIZED = 0
		/// <summary>The memory passed is zero initialized, so the codec may take advantage of this by skipping writing zeroes.</summary>
		Yes = 0,
		// NO_SK_CODEC_ZERO_INITIALIZED = 1
		/// <summary>The memory passed has not been initialized to zero, so the codec must write all zeros to memory.</summary>
		No = 1,
	}
}
