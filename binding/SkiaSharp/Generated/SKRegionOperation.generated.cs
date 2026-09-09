using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_region_op_t
	public enum SKRegionOperation {
		// DIFFERENCE_SK_REGION_OP = 0
		Difference = 0,
		// INTERSECT_SK_REGION_OP = 1
		Intersect = 1,
		// UNION_SK_REGION_OP = 2
		Union = 2,
		// XOR_SK_REGION_OP = 3
		XOR = 3,
		// REVERSE_DIFFERENCE_SK_REGION_OP = 4
		ReverseDifference = 4,
		// REPLACE_SK_REGION_OP = 5
		Replace = 5,
	}
}
