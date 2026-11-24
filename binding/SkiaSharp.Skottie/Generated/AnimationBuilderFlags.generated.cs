using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

#endregion

namespace SkiaSharp.Skottie
{

	// skottie_animation_builder_flags_t
	/// <summary>Specifies flags that control animation builder behavior.</summary>
	/// <remarks />
	public enum AnimationBuilderFlags {
		// NONE_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0
		/// <summary>No flags. Use default behavior.</summary>
		None = 0,
		// DEFER_IMAGE_LOADING_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0x01
		/// <summary>Defer image loading in the animation builder. Images will be loaded on demand instead of at build time.</summary>
		DeferImageLoading = 1,
		// PREFER_EMBEDDED_FONTS_SKOTTIE_ANIMATION_BUILDER_FLAGS = 0x02
		/// <summary>Prefer fonts embedded in the Lottie JSON file over externally loaded fonts.</summary>
		PreferEmbeddedFonts = 2,
	}
}
