using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_filltype_t
	public enum SKPathFillType {
		// WINDING_SK_PATH_FILLTYPE = 0
		Winding = 0,
		// EVENODD_SK_PATH_FILLTYPE = 1
		EvenOdd = 1,
		// INVERSE_WINDING_SK_PATH_FILLTYPE = 2
		InverseWinding = 2,
		// INVERSE_EVENODD_SK_PATH_FILLTYPE = 3
		InverseEvenOdd = 3,
	}
}
