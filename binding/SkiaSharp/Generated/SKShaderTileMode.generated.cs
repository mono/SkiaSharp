using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_shader_tilemode_t
	/// <summary>Indications on how the shader should handle drawing outside the original bounds.</summary>
	/// <remarks />
	public enum SKShaderTileMode {
		// CLAMP_SK_SHADER_TILEMODE = 0
		/// <summary>Replicate the edge color.</summary>
		Clamp = 0,
		// REPEAT_SK_SHADER_TILEMODE = 1
		/// <summary>Repeat the shader's image horizontally and vertically.</summary>
		Repeat = 1,
		// MIRROR_SK_SHADER_TILEMODE = 2
		/// <summary>Repeat the shader's image horizontally and vertically, alternating mirror images so that adjacent images always seam.</summary>
		Mirror = 2,
		// DECAL_SK_SHADER_TILEMODE = 3
		/// <summary>Only draw the shader's image in the original domain, returning transparent black elsewhere.</summary>
		Decal = 3,
	}
}
