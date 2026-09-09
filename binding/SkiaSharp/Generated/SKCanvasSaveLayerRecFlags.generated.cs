using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_canvas_savelayerrec_flags_t
	public enum SKCanvasSaveLayerRecFlags {
		// NONE_SK_CANVAS_SAVELAYERREC_FLAGS = 0
		None = 0,
		// PRESERVE_LCD_TEXT_SK_CANVAS_SAVELAYERREC_FLAGS = 1 << 1
		PreserveLcdText = 2,
		// INITIALIZE_WITH_PREVIOUS_SK_CANVAS_SAVELAYERREC_FLAGS = 1 << 2
		InitializeWithPrevious = 4,
		// F16_COLOR_TYPE_SK_CANVAS_SAVELAYERREC_FLAGS = 1 << 4
		F16ColorType = 16,
	}
}
