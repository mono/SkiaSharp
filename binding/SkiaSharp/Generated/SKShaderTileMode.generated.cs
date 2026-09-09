using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_shader_tilemode_t
	public enum SKShaderTileMode {
		// CLAMP_SK_SHADER_TILEMODE = 0
		Clamp = 0,
		// REPEAT_SK_SHADER_TILEMODE = 1
		Repeat = 1,
		// MIRROR_SK_SHADER_TILEMODE = 2
		Mirror = 2,
		// DECAL_SK_SHADER_TILEMODE = 3
		Decal = 3,
	}
}
