using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_lattice_recttype_t
	/// <summary>Optional setting per rectangular grid entry to make it transparent, or to fill the grid entry with a color.</summary>
	/// <remarks />
	public enum SKLatticeRectType {
		// DEFAULT_SK_LATTICE_RECT_TYPE = 0
		/// <summary>Draw the bitmap into the lattice rectangle.</summary>
		Default = 0,
		// TRANSPARENT_SK_LATTICE_RECT_TYPE = 1
		/// <summary>Skip the lattice rectangle (make it transparent).</summary>
		Transparent = 1,
		// FIXED_COLOR_SK_LATTICE_RECT_TYPE = 2
		/// <summary>Draw the associated <see cref="P:SkiaSharp.SKLattice.Colors" /> entry into the lattice rectangle.</summary>
		FixedColor = 2,
	}
}
