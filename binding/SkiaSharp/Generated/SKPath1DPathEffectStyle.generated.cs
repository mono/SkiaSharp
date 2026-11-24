using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_effect_1d_style_t
	/// <summary>How to transform path at each point (based on the current position and tangent).</summary>
	/// <remarks />
	public enum SKPath1DPathEffectStyle {
		// TRANSLATE_SK_PATH_EFFECT_1D_STYLE = 0
		/// <summary>Translate the shape to each position.</summary>
		Translate = 0,
		// ROTATE_SK_PATH_EFFECT_1D_STYLE = 1
		/// <summary>Rotate the shape about its center.</summary>
		Rotate = 1,
		// MORPH_SK_PATH_EFFECT_1D_STYLE = 2
		/// <summary>Transform each point, and turn lines into curves.</summary>
		Morph = 2,
	}
}
