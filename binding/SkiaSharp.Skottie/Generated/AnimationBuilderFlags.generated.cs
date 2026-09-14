using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

#endregion

namespace SkiaSharp.Skottie
{

	// skottie_animation_builder_flags_t
	public enum AnimationBuilderFlags {
		// NONE_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0
		None = 0,
		// DEFER_IMAGE_LOADING_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0x01
		DeferImageLoading = 1,
		// PREFER_EMBEDDED_FONTS_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0x02
		PreferEmbeddedFonts = 2,
	}
}
