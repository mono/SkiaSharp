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
		/// <summary>Gets or sets the G coefficient.</summary>
		/// <value>The G coefficient value.</value>
		/// <remarks />
		public Single G {
			readonly get => fG;
			set => fG = value;
		}

		// public float fA
		private Single fA;
		/// <summary>Gets or sets the A coefficient.</summary>
		/// <value>The A coefficient value.</value>
		/// <remarks />
		public Single A {
			readonly get => fA;
			set => fA = value;
		}

		// public float fB
		private Single fB;
		/// <summary>Gets or sets the B coefficient.</summary>
		/// <value>The B coefficient value.</value>
		/// <remarks />
		public Single B {
			readonly get => fB;
			set => fB = value;
		}

		// public float fC
		private Single fC;
		/// <summary>Gets or sets the C coefficient.</summary>
		/// <value>The C coefficient value.</value>
		/// <remarks />
		public Single C {
			readonly get => fC;
			set => fC = value;
		}

		// public float fD
		private Single fD;
		/// <summary>Gets or sets the D coefficient.</summary>
		/// <value>The D coefficient value.</value>
		/// <remarks />
		public Single D {
			readonly get => fD;
			set => fD = value;
		}

		// public float fE
		private Single fE;
		/// <summary>Gets or sets the E coefficient.</summary>
		/// <value>The E coefficient value.</value>
		/// <remarks />
		public Single E {
			readonly get => fE;
			set => fE = value;
		}

		// public float fF
		private Single fF;
		/// <summary>Gets or sets the F coefficient.</summary>
		/// <value>The F coefficient value.</value>
		/// <remarks />
		public Single F {
			readonly get => fF;
			set => fF = value;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKColorSpaceTransferFn obj) =>
#pragma warning disable CS8909
			fG == obj.fG && fA == obj.fA && fB == obj.fB && fC == obj.fC && fD == obj.fD && fE == obj.fE && fF == obj.fF;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is a <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> and is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKColorSpaceTransferFn f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKColorSpaceTransferFn" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for the current <see cref="T:SkiaSharp.SKColorSpaceTransferFn" />.</returns>
		/// <remarks />
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
