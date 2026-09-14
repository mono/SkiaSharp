using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_canvas_savelayerrec_flags_t
	/// <summary>Specifies the options for saving a layer on an <see cref="T:SkiaSharp.SKCanvas" />.</summary>
	/// <remarks />
	public enum SKCanvasSaveLayerRecFlags {
		// NONE_SK_CANVAS_SAVELAYERREC_FLAGS = 0
		/// <summary>No special flags; use default layer behavior.</summary>
		None = 0,
		// PRESERVE_LCD_TEXT_SK_CANVAS_SAVELAYERREC_FLAGS = 1 << 1
		/// <summary>Preserves LCD text rendering quality within the layer.</summary>
		PreserveLcdText = 2,
		// INITIALIZE_WITH_PREVIOUS_SK_CANVAS_SAVELAYERREC_FLAGS = 1 << 2
		/// <summary>Initializes the layer with a copy of the previous layer content.</summary>
		InitializeWithPrevious = 4,
		// F16_COLOR_TYPE_SK_CANVAS_SAVELAYERREC_FLAGS = 1 << 4
		/// <summary>Allocates the layer in 16-bit floating-point color format for higher precision.</summary>
		F16ColorType = 16,
	}
}
