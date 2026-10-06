using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_effect_trim_mode_t
	/// <summary>Represents the type of trimming to perform.</summary>
	/// <remarks />
	public enum SKTrimPathEffectMode {
		// NORMAL_SK_PATH_EFFECT_TRIM_MODE = 0
		/// <summary>Trim the path around the start and stop, preserving [start, stop].</summary>
		Normal = 0,
		// INVERTED_SK_PATH_EFFECT_TRIM_MODE = 1
		/// <summary>Remove the path between the start and stop, preserving [0, start] and [stop, 1].</summary>
		Inverted = 1,
	}
}
