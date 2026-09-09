using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_cubic_resampler_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKCubicResampler : IEquatable<SKCubicResampler> {
		// public float fB
		private readonly Single fB;
		public readonly Single B => fB;

		// public float fC
		private readonly Single fC;
		public readonly Single C => fC;

		public readonly bool Equals (SKCubicResampler obj) =>
#pragma warning disable CS8909
			fB == obj.fB && fC == obj.fC;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKCubicResampler f && Equals (f);

		public static bool operator == (SKCubicResampler left, SKCubicResampler right) =>
			left.Equals (right);

		public static bool operator != (SKCubicResampler left, SKCubicResampler right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fB);
			hash.Add (fC);
			return hash.ToHashCode ();
		}

	}
}
