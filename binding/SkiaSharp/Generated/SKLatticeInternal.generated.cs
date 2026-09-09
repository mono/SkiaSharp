using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_lattice_t
	[StructLayout (LayoutKind.Sequential)]
	internal unsafe partial struct SKLatticeInternal : IEquatable<SKLatticeInternal> {
		// public const int* fXDivs
		public Int32* fXDivs;

		// public const int* fYDivs
		public Int32* fYDivs;

		// public const sk_lattice_recttype_t* fRectTypes
		public SKLatticeRectType* fRectTypes;

		// public int fXCount
		public Int32 fXCount;

		// public int fYCount
		public Int32 fYCount;

		// public const sk_irect_t* fBounds
		public SKRectI* fBounds;

		// public const sk_color_t* fColors
		public UInt32* fColors;

		public readonly bool Equals (SKLatticeInternal obj) =>
#pragma warning disable CS8909
			fXDivs == obj.fXDivs && fYDivs == obj.fYDivs && fRectTypes == obj.fRectTypes && fXCount == obj.fXCount && fYCount == obj.fYCount && fBounds == obj.fBounds && fColors == obj.fColors;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKLatticeInternal f && Equals (f);

		public static bool operator == (SKLatticeInternal left, SKLatticeInternal right) =>
			left.Equals (right);

		public static bool operator != (SKLatticeInternal left, SKLatticeInternal right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fXDivs);
			hash.Add (fYDivs);
			hash.Add (fRectTypes);
			hash.Add (fXCount);
			hash.Add (fYCount);
			hash.Add (fBounds);
			hash.Add (fColors);
			return hash.ToHashCode ();
		}

	}
}
