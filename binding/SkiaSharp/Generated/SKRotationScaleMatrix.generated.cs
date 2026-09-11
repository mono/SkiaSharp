using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_rsxform_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRotationScaleMatrix : IEquatable<SKRotationScaleMatrix> {
		// public float fSCos
		private Single fSCos;
		public Single SCos {
			readonly get => fSCos;
			set => fSCos = value;
		}

		// public float fSSin
		private Single fSSin;
		public Single SSin {
			readonly get => fSSin;
			set => fSSin = value;
		}

		// public float fTX
		private Single fTX;
		public Single TX {
			readonly get => fTX;
			set => fTX = value;
		}

		// public float fTY
		private Single fTY;
		public Single TY {
			readonly get => fTY;
			set => fTY = value;
		}

		public readonly bool Equals (SKRotationScaleMatrix obj) =>
#pragma warning disable CS8909
			fSCos == obj.fSCos && fSSin == obj.fSSin && fTX == obj.fTX && fTY == obj.fTY;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKRotationScaleMatrix f && Equals (f);

		public static bool operator == (SKRotationScaleMatrix left, SKRotationScaleMatrix right) =>
			left.Equals (right);

		public static bool operator != (SKRotationScaleMatrix left, SKRotationScaleMatrix right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fSCos);
			hash.Add (fSSin);
			hash.Add (fTX);
			hash.Add (fTY);
			return hash.ToHashCode ();
		}

	}
}
