using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_matrix_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKMatrix : IEquatable<SKMatrix> {
		// public float scaleX
		private Single scaleX;
		public Single ScaleX {
			readonly get => scaleX;
			set => scaleX = value;
		}

		// public float skewX
		private Single skewX;
		public Single SkewX {
			readonly get => skewX;
			set => skewX = value;
		}

		// public float transX
		private Single transX;
		public Single TransX {
			readonly get => transX;
			set => transX = value;
		}

		// public float skewY
		private Single skewY;
		public Single SkewY {
			readonly get => skewY;
			set => skewY = value;
		}

		// public float scaleY
		private Single scaleY;
		public Single ScaleY {
			readonly get => scaleY;
			set => scaleY = value;
		}

		// public float transY
		private Single transY;
		public Single TransY {
			readonly get => transY;
			set => transY = value;
		}

		// public float persp0
		private Single persp0;
		public Single Persp0 {
			readonly get => persp0;
			set => persp0 = value;
		}

		// public float persp1
		private Single persp1;
		public Single Persp1 {
			readonly get => persp1;
			set => persp1 = value;
		}

		// public float persp2
		private Single persp2;
		public Single Persp2 {
			readonly get => persp2;
			set => persp2 = value;
		}

		public readonly bool Equals (SKMatrix obj) =>
#pragma warning disable CS8909
			scaleX == obj.scaleX && skewX == obj.skewX && transX == obj.transX && skewY == obj.skewY && scaleY == obj.scaleY && transY == obj.transY && persp0 == obj.persp0 && persp1 == obj.persp1 && persp2 == obj.persp2;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is SKMatrix f && Equals (f);

		public static bool operator == (SKMatrix left, SKMatrix right) =>
			left.Equals (right);

		public static bool operator != (SKMatrix left, SKMatrix right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (scaleX);
			hash.Add (skewX);
			hash.Add (transX);
			hash.Add (skewY);
			hash.Add (scaleY);
			hash.Add (transY);
			hash.Add (persp0);
			hash.Add (persp1);
			hash.Add (persp2);
			return hash.ToHashCode ();
		}

	}
}
