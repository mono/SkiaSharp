using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

#endregion

namespace SkiaSharp.Skottie
{

	// skottie_animation_renderflags_t
	/// <summary>Specifies flags that control animation rendering behavior.</summary>
	/// <remarks />
	[Flags]
	public enum AnimationRenderFlags {
		// SKIP_TOP_LEVEL_ISOLATION = 0x01
		/// <summary>When rendering into a transparent canvas, skip the implicit layer isolation. This can be used to improve performance when it is known that no animation blend modes or layer effects require isolation.</summary>
		SkipTopLevelIsolation = 1,
		// DISABLE_TOP_LEVEL_CLIPPING = 0x02
		/// <summary>Disable the implicit top-level clipping to the animation bounds.</summary>
		DisableTopLevelClipping = 2,
	}
}
