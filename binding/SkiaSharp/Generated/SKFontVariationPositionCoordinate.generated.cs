using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontarguments_variation_position_coordinate_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontVariationPositionCoordinate : IEquatable<SKFontVariationPositionCoordinate> {
		// public sk_fourbytetag_t axis
		private SKFourByteTag axis;
		public SKFourByteTag Axis {
			readonly get => axis;
			set => axis = value;
		}

		// public float value
		private Single value;
		public Single Value {
			readonly get => this.value;
			set => this.value = value;
		}

		public readonly bool Equals (SKFontVariationPositionCoordinate obj) =>
#pragma warning disable CS8909
			axis == obj.axis && value == obj.value;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKFontVariationPositionCoordinate f && Equals (f);

		public static bool operator == (SKFontVariationPositionCoordinate left, SKFontVariationPositionCoordinate right) =>
			left.Equals (right);

		public static bool operator != (SKFontVariationPositionCoordinate left, SKFontVariationPositionCoordinate right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (axis);
			hash.Add (value);
			return hash.ToHashCode ();
		}

	}
}
