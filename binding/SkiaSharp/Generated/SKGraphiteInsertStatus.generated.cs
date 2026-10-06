using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_insert_status_t
	/// <summary>Describes the result of inserting a recording into a Graphite context.</summary>
	/// <remarks />
	public enum SKGraphiteInsertStatus {
		// SUCCESS_SK_GRAPHITE_INSERT_STATUS = 0
		/// <summary>The recording was inserted successfully.</summary>
		Success = 0,
		// INVALID_RECORDING_SK_GRAPHITE_INSERT_STATUS = 1
		/// <summary>The recording was not valid and could not be inserted.</summary>
		InvalidRecording = 1,
		// PROMISE_INSTANTIATION_FAILED_SK_GRAPHITE_INSERT_STATUS = 2
		/// <summary>A promise image referenced by the recording could not be instantiated.</summary>
		PromiseInstantiationFailed = 2,
		// ADD_COMMANDS_FAILED_SK_GRAPHITE_INSERT_STATUS = 3
		/// <summary>The commands from the recording could not be added to the backend command buffer.</summary>
		AddCommandsFailed = 3,
		// ASYNC_SHADER_COMPILES_FAILED_SK_GRAPHITE_INSERT_STATUS = 4
		/// <summary>One or more asynchronous shader compilations required by the recording failed.</summary>
		AsyncShaderCompilesFailed = 4,
		// OUT_OF_ORDER_RECORDING_SK_GRAPHITE_INSERT_STATUS = 5
		/// <summary>The recording was inserted out of the order required when ordered recordings are enforced.</summary>
		OutOfOrderRecording = 5,
	}
}
