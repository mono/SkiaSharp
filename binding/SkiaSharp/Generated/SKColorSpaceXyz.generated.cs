using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_colorspace_xyz_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpaceXyz : IEquatable<SKColorSpaceXyz> {
		// public float fM00
		private Single fM00;

		// public float fM01
		private Single fM01;

		// public float fM02
		private Single fM02;

		// public float fM10
		private Single fM10;

		// public float fM11
		private Single fM11;

		// public float fM12
		private Single fM12;

		// public float fM20
		private Single fM20;

		// public float fM21
		private Single fM21;

		// public float fM22
		private Single fM22;

		public readonly bool Equals (SKColorSpaceXyz obj) =>
#pragma warning disable CS8909
			fM00 == obj.fM00 && fM01 == obj.fM01 && fM02 == obj.fM02 && fM10 == obj.fM10 && fM11 == obj.fM11 && fM12 == obj.fM12 && fM20 == obj.fM20 && fM21 == obj.fM21 && fM22 == obj.fM22;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKColorSpaceXyz f && Equals (f);

		public static bool operator == (SKColorSpaceXyz left, SKColorSpaceXyz right) =>
			left.Equals (right);

		public static bool operator != (SKColorSpaceXyz left, SKColorSpaceXyz right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fM00);
			hash.Add (fM01);
			hash.Add (fM02);
			hash.Add (fM10);
			hash.Add (fM11);
			hash.Add (fM12);
			hash.Add (fM20);
			hash.Add (fM21);
			hash.Add (fM22);
			return hash.ToHashCode ();
		}

	}
}
