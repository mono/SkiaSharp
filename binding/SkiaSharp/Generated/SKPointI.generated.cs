using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_ipoint_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPointI : IEquatable<SKPointI> {
		// public int32_t x
		private Int32 x;
		/// <summary>Gets or sets the x-coordinate of this <see cref="T:SkiaSharp.SKPointI" />.</summary>
		/// <value>The x-coordinate of this point.</value>
		/// <remarks />
		public Int32 X {
			readonly get => x;
			set => x = value;
		}

		// public int32_t y
		private Int32 y;
		/// <summary>Gets or sets the y-coordinate of this <see cref="T:SkiaSharp.SKPointI" />.</summary>
		/// <value>The y-coordinate of this point.</value>
		/// <remarks />
		public Int32 Y {
			readonly get => y;
			set => y = value;
		}

		/// <summary>Specifies whether this <see cref="T:SkiaSharp.SKPointI" /> contains the same coordinates as the specified <see cref="T:SkiaSharp.SKPointI" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKPointI" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> has the same coordinates as this <see cref="T:SkiaSharp.SKPointI" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKPointI obj) =>
#pragma warning disable CS8909
			x == obj.x && y == obj.y;
#pragma warning restore CS8909

		/// <summary>Specifies whether this <see cref="T:SkiaSharp.SKPointI" /> contains the same coordinates as the specified <see cref="T:System.Object" />.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKPointI" /> and has the same coordinates as this <see cref="T:SkiaSharp.SKPointI" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKPointI f && Equals (f);

		/// <summary>Determines whether the coordinates of the specified points are equal.</summary>
		/// <param name="left">A <see cref="T:SkiaSharp.SKPointI" /> to compare.</param>
		/// <param name="right">A <see cref="T:SkiaSharp.SKPointI" /> to compare.</param>
		/// <returns><see langword="true" /> if the <see cref="P:SkiaSharp.SKPointI.X" /> and <see cref="P:SkiaSharp.SKPointI.Y" /> values of <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKPointI left, SKPointI right) =>
			left.Equals (right);

		/// <summary>Determines whether the coordinates of the specified points are not equal.</summary>
		/// <param name="left">A <see cref="T:SkiaSharp.SKPointI" /> to compare.</param>
		/// <param name="right">A <see cref="T:SkiaSharp.SKPointI" /> to compare.</param>
		/// <returns><see langword="true" /> if the <see cref="P:SkiaSharp.SKPointI.X" /> and <see cref="P:SkiaSharp.SKPointI.Y" /> values of <paramref name="left" /> and <paramref name="right" /> differ; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKPointI left, SKPointI right) =>
			!left.Equals (right);

		/// <summary>Calculates the hashcode for this point.</summary>
		/// <returns>Returns the hashcode for this point.</returns>
		/// <remarks>You should avoid depending on GetHashCode for unique values, as two point objects with the same values for their X and Y properties may return the same hash code. This behavior could change in a future release.</remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			return hash.ToHashCode ();
		}

	}
}
