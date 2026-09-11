using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pathop_t
	public enum SKPathOp {
		// DIFFERENCE_SK_PATHOP = 0
		Difference = 0,
		// INTERSECT_SK_PATHOP = 1
		Intersect = 1,
		// UNION_SK_PATHOP = 2
		Union = 2,
		// XOR_SK_PATHOP = 3
		Xor = 3,
		// REVERSE_DIFFERENCE_SK_PATHOP = 4
		ReverseDifference = 4,
	}
}
