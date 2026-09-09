using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codec_result_t
	public enum SKCodecResult {
		// SUCCESS_SK_CODEC_RESULT = 0
		Success = 0,
		// INCOMPLETE_INPUT_SK_CODEC_RESULT = 1
		IncompleteInput = 1,
		// ERROR_IN_INPUT_SK_CODEC_RESULT = 2
		ErrorInInput = 2,
		// INVALID_CONVERSION_SK_CODEC_RESULT = 3
		InvalidConversion = 3,
		// INVALID_SCALE_SK_CODEC_RESULT = 4
		InvalidScale = 4,
		// INVALID_PARAMETERS_SK_CODEC_RESULT = 5
		InvalidParameters = 5,
		// INVALID_INPUT_SK_CODEC_RESULT = 6
		InvalidInput = 6,
		// COULD_NOT_REWIND_SK_CODEC_RESULT = 7
		CouldNotRewind = 7,
		// INTERNAL_ERROR_SK_CODEC_RESULT = 8
		InternalError = 8,
		// UNIMPLEMENTED_SK_CODEC_RESULT = 9
		Unimplemented = 9,
	}
}
