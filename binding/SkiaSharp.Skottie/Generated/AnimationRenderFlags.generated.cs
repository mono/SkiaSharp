using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces

using SkiaSharp.Skottie;

#endregion

namespace SkiaSharp.Skottie
{

	// skottie_animation_renderflags_t
	[Flags]
	public enum AnimationRenderFlags {
		// SKIP_TOP_LEVEL_ISOLATION = 0x01
		SkipTopLevelIsolation = 1,
		// DISABLE_TOP_LEVEL_CLIPPING = 0x02
		DisableTopLevelClipping = 2,
	}
}
