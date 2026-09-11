using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_flags_t
	[Flags]
	public enum BufferFlags {
		// HB_BUFFER_FLAG_DEFAULT = 0x00000000u
		Default = 0,
		// HB_BUFFER_FLAG_BOT = 0x00000001u
		BeginningOfText = 1,
		// HB_BUFFER_FLAG_EOT = 0x00000002u
		EndOfText = 2,
		// HB_BUFFER_FLAG_PRESERVE_DEFAULT_IGNORABLES = 0x00000004u
		PreserveDefaultIgnorables = 4,
		// HB_BUFFER_FLAG_REMOVE_DEFAULT_IGNORABLES = 0x00000008u
		RemoveDefaultIgnorables = 8,
		// HB_BUFFER_FLAG_DO_NOT_INSERT_DOTTED_CIRCLE = 0x00000010u
		DoNotInsertDottedCircle = 16,
		// HB_BUFFER_FLAG_VERIFY = 0x00000020u
		Verify = 32,
		// HB_BUFFER_FLAG_PRODUCE_UNSAFE_TO_CONCAT = 0x00000040u
		ProduceUnsafeToConcat = 64,
		// HB_BUFFER_FLAG_PRODUCE_SAFE_TO_INSERT_TATWEEL = 0x00000080u
		ProduceSafeToInsertTatweel = 128,
	}
}
