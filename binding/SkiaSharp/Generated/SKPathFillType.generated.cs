using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_filltype_t
	/// <summary>Possible path fill type values.</summary>
	/// <remarks />
	public enum SKPathFillType {
		// WINDING_SK_PATH_FILLTYPE = 0
		/// <summary>Specifies that "inside" is computed by a non-zero sum of signed edge crossings.</summary>
		Winding = 0,
		// EVENODD_SK_PATH_FILLTYPE = 1
		/// <summary>Specifies that "inside" is computed by an odd number of edge crossings.</summary>
		EvenOdd = 1,
		// INVERSE_WINDING_SK_PATH_FILLTYPE = 2
		/// <summary>Same as <see cref="F:SkiaSharp.SKPathFillType.Winding" />, but draws outside of the path, rather than inside.</summary>
		InverseWinding = 2,
		// INVERSE_EVENODD_SK_PATH_FILLTYPE = 3
		/// <summary>Same as <see cref="F:SkiaSharp.SKPathFillType.EvenOdd" />, but draws outside of the path, rather than inside.</summary>
		InverseEvenOdd = 3,
	}
}
