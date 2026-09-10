using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_region_op_t
	/// <summary>The logical operations that can be performed when combining two regions.</summary>
	/// <remarks />
	public enum SKRegionOperation {
		// DIFFERENCE_SK_REGION_OP = 0
		/// <summary>Subtract the op region from the first region.</summary>
		Difference = 0,
		// INTERSECT_SK_REGION_OP = 1
		/// <summary>Intersect the two regions.</summary>
		Intersect = 1,
		// UNION_SK_REGION_OP = 2
		/// <summary>Union (inclusive-or) the two regions.</summary>
		Union = 2,
		// XOR_SK_REGION_OP = 3
		/// <summary>Exclusive-or the two regions.</summary>
		XOR = 3,
		// REVERSE_DIFFERENCE_SK_REGION_OP = 4
		/// <summary>Subtract the first region from the op region.</summary>
		ReverseDifference = 4,
		// REPLACE_SK_REGION_OP = 5
		/// <summary>Replace the destination region with the op region.</summary>
		Replace = 5,
	}
}
