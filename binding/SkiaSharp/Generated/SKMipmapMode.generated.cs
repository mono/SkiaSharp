using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_mipmap_mode_t
	/// <summary>Specifies the mipmap mode used when sampling images.</summary>
	/// <remarks />
	public enum SKMipmapMode {
		// NONE_SK_MIPMAP_MODE = 0
		/// <summary>Disables mipmap filtering.</summary>
		None = 0,
		// NEAREST_SK_MIPMAP_MODE = 1
		/// <summary>Selects the nearest mipmap level.</summary>
		Nearest = 1,
		// LINEAR_SK_MIPMAP_MODE = 2
		/// <summary>Interpolates between two mipmap levels using linear filtering.</summary>
		Linear = 2,
	}
}
