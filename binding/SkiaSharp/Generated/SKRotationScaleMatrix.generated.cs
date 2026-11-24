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
		/// <summary>Gets or sets the scaled cosine component (scale * cos(angle)).</summary>
		/// <value>The scaled cosine value representing the combined scale and rotation.</value>
		/// <remarks />
		public Single SCos {
			readonly get => fSCos;
			set => fSCos = value;
		}

		// public float fSSin
		private Single fSSin;
		/// <summary>Gets or sets the scaled sine component (scale * sin(angle)).</summary>
		/// <value>The scaled sine value representing the combined scale and rotation.</value>
		/// <remarks />
		public Single SSin {
			readonly get => fSSin;
			set => fSSin = value;
		}

		// public float fTX
		private Single fTX;
		/// <summary>Gets or sets the x-axis translation component.</summary>
		/// <value>The horizontal translation value.</value>
		/// <remarks />
		public Single TX {
			readonly get => fTX;
			set => fTX = value;
		}

		// public float fTY
		private Single fTY;
		/// <summary>Gets or sets the y-axis translation component.</summary>
		/// <value>The vertical translation value.</value>
		/// <remarks />
		public Single TY {
			readonly get => fTY;
			set => fTY = value;
		}

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified matrix is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKRotationScaleMatrix obj) =>
#pragma warning disable CS8909
			fSCos == obj.fSCos && fSSin == obj.fSSin && fTX == obj.fTX && fTY == obj.fTY;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is a <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> and is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKRotationScaleMatrix f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> to compare.</param>
		/// <returns><see langword="true" /> if the two matrices are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKRotationScaleMatrix left, SKRotationScaleMatrix right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKRotationScaleMatrix" /> to compare.</param>
		/// <returns><see langword="true" /> if the two matrices are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKRotationScaleMatrix left, SKRotationScaleMatrix right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
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
