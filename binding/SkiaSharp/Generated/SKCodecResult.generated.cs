using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_result_t
	/// <summary>Used to describe the result of a call to <see cref="M:SkiaSharp.SKCodec.GetPixels(SkiaSharp.SKImageInfo,System.IntPtr,SkiaSharp.SKCodecOptions)" /> or one of the overloads that accepts a <see cref="T:SkiaSharp.SKCodecOptions" />.</summary>
	/// <remarks>Result is the union of possible results from subclasses.</remarks>
	public enum SKCodecResult {
		// SUCCESS_SK_CODEC_RESULT = 0
		/// <summary>The general return value for success.</summary>
		Success = 0,
		// INCOMPLETE_INPUT_SK_CODEC_RESULT = 1
		/// <summary>The input is incomplete. A partial image was generated.</summary>
		IncompleteInput = 1,
		// ERROR_IN_INPUT_SK_CODEC_RESULT = 2
		/// <summary>There was an error in the imput data. If returned from an incremental decode, decoding cannot continue, even with more data.</summary>
		ErrorInInput = 2,
		// INVALID_CONVERSION_SK_CODEC_RESULT = 3
		/// <summary>The codec cannot convert to match the request, ignoring dimensions.</summary>
		InvalidConversion = 3,
		// INVALID_SCALE_SK_CODEC_RESULT = 4
		/// <summary>The generator cannot scale to requested size.</summary>
		InvalidScale = 4,
		// INVALID_PARAMETERS_SK_CODEC_RESULT = 5
		/// <summary>The parameters (besides info) are invalid. e.g. null pixels, row bytes too small, etc.</summary>
		InvalidParameters = 5,
		// INVALID_INPUT_SK_CODEC_RESULT = 6
		/// <summary>The input did not contain a valid image.</summary>
		InvalidInput = 6,
		// COULD_NOT_REWIND_SK_CODEC_RESULT = 7
		/// <summary>Fulfilling this request requires rewinding the input, which is not supported for this input.</summary>
		CouldNotRewind = 7,
		// INTERNAL_ERROR_SK_CODEC_RESULT = 8
		/// <summary>An internal memory occurred, such as an out-of-memory error.</summary>
		InternalError = 8,
		// UNIMPLEMENTED_SK_CODEC_RESULT = 9
		/// <summary>This method is not supported by this codec.</summary>
		Unimplemented = 9,
	}
}
