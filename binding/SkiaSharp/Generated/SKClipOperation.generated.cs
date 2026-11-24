using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_clipop_t
	/// <summary>The logical operations that can be performed when combining two regions.</summary>
	/// <remarks />
	public enum SKClipOperation {
		// DIFFERENCE_SK_CLIPOP = 0
		/// <summary>Subtract the op region from the first region.</summary>
		Difference = 0,
		// INTERSECT_SK_CLIPOP = 1
		/// <summary>Intersect the two regions.</summary>
		Intersect = 1,
	}
}
