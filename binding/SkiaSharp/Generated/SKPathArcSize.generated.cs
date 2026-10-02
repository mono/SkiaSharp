using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_arc_size_t
	/// <summary>Indication for whether the smaller or larger of possible two arcs is drawn.</summary>
	/// <remarks />
	public enum SKPathArcSize {
		// SMALL_SK_PATH_ARC_SIZE = 0
		/// <summary>The smaller of the two possible arcs.</summary>
		Small = 0,
		// LARGE_SK_PATH_ARC_SIZE = 1
		/// <summary>The larger of the two possible arcs.</summary>
		Large = 1,
	}
}
