using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_color4f_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKColorF : IEquatable<SKColorF> {
		// public float fR
		private readonly Single fR;
		public readonly Single Red => fR;

		// public float fG
		private readonly Single fG;
		public readonly Single Green => fG;

		// public float fB
		private readonly Single fB;
		public readonly Single Blue => fB;

		// public float fA
		private readonly Single fA;
		public readonly Single Alpha => fA;

		public readonly bool Equals (SKColorF obj) =>
#pragma warning disable CS8909
			fR == obj.fR && fG == obj.fG && fB == obj.fB && fA == obj.fA;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKColorF f && Equals (f);

		public static bool operator == (SKColorF left, SKColorF right) =>
			left.Equals (right);

		public static bool operator != (SKColorF left, SKColorF right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fR);
			hash.Add (fG);
			hash.Add (fB);
			hash.Add (fA);
			return hash.ToHashCode ();
		}

	}
}
