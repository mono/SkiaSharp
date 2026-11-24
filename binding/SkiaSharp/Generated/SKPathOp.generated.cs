using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pathop_t
	/// <summary>The logical operations that can be performed when combining two paths using <see cref="M:SkiaSharp.SKPath.Op(SkiaSharp.SKPath,SkiaSharp.SKPathOp)" />.</summary>
	/// <remarks />
	public enum SKPathOp {
		// DIFFERENCE_SK_PATHOP = 0
		/// <summary>Subtract the op path from the current path.</summary>
		Difference = 0,
		// INTERSECT_SK_PATHOP = 1
		/// <summary>Intersect the two paths.</summary>
		Intersect = 1,
		// UNION_SK_PATHOP = 2
		/// <summary>Union (inclusive-or) the two paths.</summary>
		Union = 2,
		// XOR_SK_PATHOP = 3
		/// <summary>Exclusive-or the two paths.</summary>
		Xor = 3,
		// REVERSE_DIFFERENCE_SK_PATHOP = 4
		/// <summary>Subtract the current path from the op path.</summary>
		ReverseDifference = 4,
	}
}
