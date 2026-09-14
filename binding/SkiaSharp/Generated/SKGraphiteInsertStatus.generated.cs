using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_insert_status_t
	public enum SKGraphiteInsertStatus {
		// SUCCESS_SK_GRAPHITE_INSERT_STATUS = 0
		Success = 0,
		// INVALID_RECORDING_SK_GRAPHITE_INSERT_STATUS = 1
		InvalidRecording = 1,
		// PROMISE_INSTANTIATION_FAILED_SK_GRAPHITE_INSERT_STATUS = 2
		PromiseInstantiationFailed = 2,
		// ADD_COMMANDS_FAILED_SK_GRAPHITE_INSERT_STATUS = 3
		AddCommandsFailed = 3,
		// ASYNC_SHADER_COMPILES_FAILED_SK_GRAPHITE_INSERT_STATUS = 4
		AsyncShaderCompilesFailed = 4,
		// OUT_OF_ORDER_RECORDING_SK_GRAPHITE_INSERT_STATUS = 5
		OutOfOrderRecording = 5,
	}
}
