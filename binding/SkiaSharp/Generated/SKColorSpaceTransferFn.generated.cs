using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_colorspace_transfer_fn_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKColorSpaceTransferFn : IEquatable<SKColorSpaceTransferFn> {
		// public float fG
		private Single fG;
		public Single G {
			readonly get => fG;
			set => fG = value;
		}

		// public float fA
		private Single fA;
		public Single A {
			readonly get => fA;
			set => fA = value;
		}

		// public float fB
		private Single fB;
		public Single B {
			readonly get => fB;
			set => fB = value;
		}

		// public float fC
		private Single fC;
		public Single C {
			readonly get => fC;
			set => fC = value;
		}

		// public float fD
		private Single fD;
		public Single D {
			readonly get => fD;
			set => fD = value;
		}

		// public float fE
		private Single fE;
		public Single E {
			readonly get => fE;
			set => fE = value;
		}

		// public float fF
		private Single fF;
		public Single F {
			readonly get => fF;
			set => fF = value;
		}

		public readonly bool Equals (SKColorSpaceTransferFn obj) =>
#pragma warning disable CS8909
			fG == obj.fG && fA == obj.fA && fB == obj.fB && fC == obj.fC && fD == obj.fD && fE == obj.fE && fF == obj.fF;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKColorSpaceTransferFn f && Equals (f);

		public static bool operator == (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right) =>
			left.Equals (right);

		public static bool operator != (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fG);
			hash.Add (fA);
			hash.Add (fB);
			hash.Add (fC);
			hash.Add (fD);
			hash.Add (fE);
			hash.Add (fF);
			return hash.ToHashCode ();
		}

	}
}
