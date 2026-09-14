using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codecanimation_disposalmethod_t
	/// <summary>Represents how the next frame in the image is based on the current frame.</summary>
	/// <remarks />
	public enum SKCodecAnimationDisposalMethod {
		// KEEP_SK_CODEC_ANIMATION_DISPOSAL_METHOD = 1
		/// <summary>The next frame should be drawn on top of this one.</summary>
		Keep = 1,
		// RESTORE_BG_COLOR_SK_CODEC_ANIMATION_DISPOSAL_METHOD = 2
		/// <summary>The area inside this frame's rectangle should be cleared to the background color before drawing the next frame.</summary>
		RestoreBackgroundColor = 2,
		// RESTORE_PREVIOUS_SK_CODEC_ANIMATION_DISPOSAL_METHOD = 3
		/// <summary>The next frame should be drawn on top of the previous frame - i.e. disregarding this one.</summary>
		RestorePrevious = 3,
	}
}
