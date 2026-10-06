using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_blurstyle_t
	/// <summary>Blur types for the <see cref="M:SkiaSharp.SKMaskFilter.CreateBlur(SkiaSharp.SKBlurStyle,System.Single)" /> method and its overloads.</summary>
	/// <remarks />
	public enum SKBlurStyle {
		// NORMAL_SK_BLUR_STYLE = 0
		/// <summary>Fuzzy inside and outside.</summary>
		Normal = 0,
		// SOLID_SK_BLUR_STYLE = 1
		/// <summary>Solid inside; fuzzy outside.</summary>
		Solid = 1,
		// OUTER_SK_BLUR_STYLE = 2
		/// <summary>Nothing inside; fuzzy outside.</summary>
		Outer = 2,
		// INNER_SK_BLUR_STYLE = 3
		/// <summary>Fuzzy inside; nothing outside.</summary>
		Inner = 3,
	}
}
