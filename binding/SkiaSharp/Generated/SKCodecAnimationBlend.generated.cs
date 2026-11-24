using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_codecanimation_blend_t
	/// <summary>Specifies how a frame should be blended with a previous frame in an animated image.</summary>
	/// <remarks />
	public enum SKCodecAnimationBlend {
		// SRC_OVER_SK_CODEC_ANIMATION_BLEND = 0
		/// <summary>The source frame is blended over the destination using alpha compositing.</summary>
		SrcOver = 0,
		// SRC_SK_CODEC_ANIMATION_BLEND = 1
		/// <summary>The source frame replaces the destination, ignoring any previous content.</summary>
		Src = 1,
	}
}
