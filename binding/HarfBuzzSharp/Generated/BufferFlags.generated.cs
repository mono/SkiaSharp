using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_buffer_flags_t
	/// <summary>Flags for configuring buffer behavior during text shaping.</summary>
	/// <remarks />
	[Flags]
	public enum BufferFlags {
		// HB_BUFFER_FLAG_DEFAULT = 0x00000000u
		/// <summary>The default buffer flags with no special behavior.</summary>
		Default = 0,
		// HB_BUFFER_FLAG_BOT = 0x00000001u
		/// <summary>Indicates that the buffer represents the beginning of the text.</summary>
		BeginningOfText = 1,
		// HB_BUFFER_FLAG_EOT = 0x00000002u
		/// <summary>Indicates that the buffer represents the end of the text.</summary>
		EndOfText = 2,
		// HB_BUFFER_FLAG_PRESERVE_DEFAULT_IGNORABLES = 0x00000004u
		/// <summary>Preserve default ignorable characters in the output.</summary>
		PreserveDefaultIgnorables = 4,
		// HB_BUFFER_FLAG_REMOVE_DEFAULT_IGNORABLES = 0x00000008u
		/// <summary>Remove default ignorable characters from the output.</summary>
		RemoveDefaultIgnorables = 8,
		// HB_BUFFER_FLAG_DO_NOT_INSERT_DOTTED_CIRCLE = 0x00000010u
		/// <summary>Do not insert dotted circle glyph for invalid character sequences.</summary>
		DoNotInsertDottedCircle = 16,
		// HB_BUFFER_FLAG_VERIFY = 0x00000020u
		/// <summary>Verifies buffer contents before text shaping.</summary>
		Verify = 32,
		// HB_BUFFER_FLAG_PRODUCE_UNSAFE_TO_CONCAT = 0x00000040u
		/// <summary>Produces flags that identify glyphs unsafe to concatenate.</summary>
		ProduceUnsafeToConcat = 64,
		// HB_BUFFER_FLAG_PRODUCE_SAFE_TO_INSERT_TATWEEL = 0x00000080u
		/// <summary>Produces flags that identify positions safe for tatweel insertion.</summary>
		ProduceSafeToInsertTatweel = 128,
	}
}
