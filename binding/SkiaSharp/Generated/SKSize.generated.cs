using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_size_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKSize : IEquatable<SKSize> {
		// public float w
		private Single w;
		/// <summary>Gets or sets the horizontal component of this <see cref="T:SkiaSharp.SKSize" /> structure.</summary>
		/// <value>The horizontal component of this <see cref="T:SkiaSharp.SKSize" /> structure.</value>
		/// <remarks />
		public Single Width {
			readonly get => w;
			set => w = value;
		}

		// public float h
		private Single h;
		/// <summary>Gets or sets the vertical component of this <see cref="T:SkiaSharp.SKSize" /> structure.</summary>
		/// <value>The vertical component of this <see cref="T:SkiaSharp.SKSize" /> structure.</value>
		/// <remarks />
		public Single Height {
			readonly get => h;
			set => h = value;
		}

		/// <summary>Tests to see whether the specified object is a <see cref="T:SkiaSharp.SKSize" /> structure with the same dimensions as this <see cref="T:SkiaSharp.SKSize" /> structure.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKSize" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> has the same dimensions as this <see cref="T:SkiaSharp.SKSize" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKSize obj) =>
#pragma warning disable CS8909
			w == obj.w && h == obj.h;
#pragma warning restore CS8909

		/// <summary>Tests to see whether the specified object is a <see cref="T:SkiaSharp.SKSize" /> structure with the same dimensions as this <see cref="T:SkiaSharp.SKSize" /> structure.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKSize" /> and has the same dimensions as this <see cref="T:SkiaSharp.SKSize" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKSize f && Equals (f);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKSize" /> structures are equal.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKSize" /> structure on the left side of the equality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKSize" /> structure on the right of the equality operator.</param>
		/// <returns><see langword="true" /> if both <see cref="T:SkiaSharp.SKSize" /> structures have equal <see cref="P:SkiaSharp.SKSize.Width" /> and <see cref="P:SkiaSharp.SKSize.Height" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKSize left, SKSize right) =>
			left.Equals (right);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKSize" /> structures are different.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKSize" /> structure that is to the left of the inequality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKSize" /> structure that is to the right of the inequality operator.</param>
		/// <returns><see langword="true" /> if either of the <see cref="P:SkiaSharp.SKSize.Width" /> and <see cref="P:SkiaSharp.SKSize.Height" /> properties of the two <see cref="T:SkiaSharp.SKSize" /> structures are unequal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKSize left, SKSize right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this <see cref="T:SkiaSharp.SKSize" /> structure.</summary>
		/// <returns>An integer value that specifies a hash value for this <see cref="T:SkiaSharp.SKSize" /> structure.</returns>
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
