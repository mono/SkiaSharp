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
		/// <summary>Gets the B parameter of the cubic resampler.</summary>
		/// <value>The B parameter value.</value>
		/// <remarks />
		public readonly Single B => fB;

		// public float fC
		private readonly Single fC;
		/// <summary>Gets the C parameter of the cubic resampler.</summary>
		/// <value>The C parameter value.</value>
		/// <remarks />
		public readonly Single C => fC;

		/// <summary>Determines whether the specified <see cref="T:SkiaSharp.SKCubicResampler" /> is equal to the current instance.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKCubicResampler" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKCubicResampler obj) =>
#pragma warning disable CS8909
			fB == obj.fB && fC == obj.fC;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKCubicResampler f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKCubicResampler" /> instances are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKCubicResampler" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKCubicResampler" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKCubicResampler left, SKCubicResampler right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKCubicResampler" /> instances are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKCubicResampler" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKCubicResampler" /> to compare.</param>
		/// <returns><see langword="true" /> if the two instances are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKCubicResampler left, SKCubicResampler right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fB);
			hash.Add (fC);
			return hash.ToHashCode ();
		}

	}
}
