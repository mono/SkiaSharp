using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_isize_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKSizeI : IEquatable<SKSizeI> {
		// public int32_t w
		private Int32 w;
		/// <summary>Gets or sets the horizontal component of this <see cref="T:SkiaSharp.SKSizeI" /> structure.</summary>
		/// <value>The horizontal component of this <see cref="T:SkiaSharp.SKSizeI" /> structure.</value>
		/// <remarks />
		public Int32 Width {
			readonly get => w;
			set => w = value;
		}

		// public int32_t h
		private Int32 h;
		/// <summary>Gets or sets the vertical component of this <see cref="T:SkiaSharp.SKSizeI" /> structure.</summary>
		/// <value>The vertical component of this <see cref="T:SkiaSharp.SKSizeI" /> structure.</value>
		/// <remarks />
		public Int32 Height {
			readonly get => h;
			set => h = value;
		}

		/// <summary>Tests to see whether the specified object is a <see cref="T:SkiaSharp.SKSizeI" /> structure with the same dimensions as this <see cref="T:SkiaSharp.SKSizeI" /> structure.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKSizeI" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> has the same dimensions as this <see cref="T:SkiaSharp.SKSizeI" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKSizeI obj) =>
#pragma warning disable CS8909
			w == obj.w && h == obj.h;
#pragma warning restore CS8909

		/// <summary>Tests to see whether the specified object is a <see cref="T:SkiaSharp.SKSizeI" /> structure with the same dimensions as this <see cref="T:SkiaSharp.SKSizeI" /> structure.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKSizeI" /> and has the same dimensions as this <see cref="T:SkiaSharp.SKSizeI" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKSizeI f && Equals (f);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKSizeI" /> structures are equal.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKSizeI" /> structure on the left side of the equality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKSizeI" /> structure on the right of the equality operator.</param>
		/// <returns><see langword="true" /> if both <see cref="T:SkiaSharp.SKSizeI" /> structures have equal <see cref="P:SkiaSharp.SKSizeI.Width" /> and <see cref="P:SkiaSharp.SKSizeI.Height" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKSizeI left, SKSizeI right) =>
			left.Equals (right);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKSizeI" /> structures are different.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKSizeI" /> structure that is to the left of the inequality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKSizeI" /> structure that is to the right of the inequality operator.</param>
		/// <returns><see langword="true" /> if either of the <see cref="P:SkiaSharp.SKSizeI.Width" /> and <see cref="P:SkiaSharp.SKSizeI.Height" /> properties of the two <see cref="T:SkiaSharp.SKSizeI" /> structures are unequal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKSizeI left, SKSizeI right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this <see cref="T:SkiaSharp.SKSizeI" /> structure.</summary>
		/// <returns>An integer value that specifies a hash value for this <see cref="T:SkiaSharp.SKSizeI" /> structure.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (w);
			hash.Add (h);
			return hash.ToHashCode ();
		}

	}
}
