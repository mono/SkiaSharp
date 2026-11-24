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
		/// <summary>Gets or sets the scaling in the x-direction.</summary>
		/// <value>The scaling in the x-direction.</value>
		/// <remarks />
		public Single ScaleX {
			readonly get => scaleX;
			set => scaleX = value;
		}

		// public float skewX
		private Single skewX;
		/// <summary>Gets or sets the skew in the x-direction.</summary>
		/// <value>The skew in the x-direction.</value>
		/// <remarks />
		public Single SkewX {
			readonly get => skewX;
			set => skewX = value;
		}

		// public float transX
		private Single transX;
		/// <summary>Gets or sets the translation in the x-direction.</summary>
		/// <value>The translation in the x-direction.</value>
		/// <remarks />
		public Single TransX {
			readonly get => transX;
			set => transX = value;
		}

		// public float skewY
		private Single skewY;
		/// <summary>Gets or sets the skew in the y-direction.</summary>
		/// <value>The skew in the y-direction.</value>
		/// <remarks />
		public Single SkewY {
			readonly get => skewY;
			set => skewY = value;
		}

		// public float scaleY
		private Single scaleY;
		/// <summary>Gets or sets the scaling in the y-direction.</summary>
		/// <value>The scaling in the y-direction.</value>
		/// <remarks />
		public Single ScaleY {
			readonly get => scaleY;
			set => scaleY = value;
		}

		// public float transY
		private Single transY;
		/// <summary>Gets or sets the translation in the y-direction.</summary>
		/// <value>The translation in the y-direction.</value>
		/// <remarks />
		public Single TransY {
			readonly get => transY;
			set => transY = value;
		}

		// public float persp0
		private Single persp0;
		/// <summary>Gets or sets the x-perspective.</summary>
		/// <value>The x-perspective.</value>
		/// <remarks />
		public Single Persp0 {
			readonly get => persp0;
			set => persp0 = value;
		}

		// public float persp1
		private Single persp1;
		/// <summary>Gets or sets the y-perspective.</summary>
		/// <value>The y-perspective.</value>
		/// <remarks />
		public Single Persp1 {
			readonly get => persp1;
			set => persp1 = value;
		}

		// public float persp2
		private Single persp2;
		/// <summary>Gets or sets the z-perspective.</summary>
		/// <value>The z-perspective.</value>
		/// <remarks />
		public Single Persp2 {
			readonly get => persp2;
			set => persp2 = value;
		}

		/// <summary>Determines whether this matrix is equal to the specified matrix.</summary>
		/// <param name="obj">The matrix to compare with this instance.</param>
		/// <returns>Returns <see langword="true" /> if the matrices are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKMatrix obj) =>
#pragma warning disable CS8909
			scaleX == obj.scaleX && skewX == obj.skewX && transX == obj.transX && skewY == obj.skewY && scaleY == obj.scaleY && transY == obj.transY && persp0 == obj.persp0 && persp1 == obj.persp1 && persp2 == obj.persp2;
#pragma warning restore CS8909

		/// <summary>Determines whether this matrix is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns>Returns <see langword="true" /> if the object is an <see cref="T:SkiaSharp.SKMatrix" /> and is equal to this matrix; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKMatrix f && Equals (f);

		/// <summary>Compares two matrices for equality.</summary>
		/// <param name="left">The first matrix to compare.</param>
		/// <param name="right">The second matrix to compare.</param>
		/// <returns>Returns <see langword="true" /> if the matrices are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKMatrix left, SKMatrix right) =>
			left.Equals (right);

		/// <summary>Compares two matrices for inequality.</summary>
		/// <param name="left">The first matrix to compare.</param>
		/// <param name="right">The second matrix to compare.</param>
		/// <returns>Returns <see langword="true" /> if the matrices are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKMatrix left, SKMatrix right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this matrix.</summary>
		/// <returns>Returns the hash code.</returns>
		/// <remarks />
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
