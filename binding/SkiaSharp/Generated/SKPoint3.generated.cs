using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_point3_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKPoint3 : IEquatable<SKPoint3> {
		// public float x
		private Single x;
		/// <summary>Gets or sets the x-coordinate of this <see cref="T:SkiaSharp.SKPoint3" />.</summary>
		/// <value>The x-coordinate.</value>
		/// <remarks />
		public Single X {
			readonly get => x;
			set => x = value;
		}

		// public float y
		private Single y;
		/// <summary>Gets or sets the y-coordinate of this <see cref="T:SkiaSharp.SKPoint3" />.</summary>
		/// <value>The y-coordinate.</value>
		/// <remarks />
		public Single Y {
			readonly get => y;
			set => y = value;
		}

		// public float z
		private Single z;
		/// <summary>Gets or sets the z-coordinate of this <see cref="T:SkiaSharp.SKPoint3" />.</summary>
		/// <value>The z-coordinate.</value>
		/// <remarks />
		public Single Z {
			readonly get => z;
			set => z = value;
		}

		/// <summary>Specifies whether this <see cref="T:SkiaSharp.SKPoint3" /> contains the same coordinates as the specified <see cref="T:SkiaSharp.SKPoint3" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKPoint3" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> has the same coordinates as this <see cref="T:SkiaSharp.SKPoint3" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKPoint3 obj) =>
#pragma warning disable CS8909
			x == obj.x && y == obj.y && z == obj.z;
#pragma warning restore CS8909

		/// <summary>Specifies whether this <see cref="T:SkiaSharp.SKPoint3" /> contains the same coordinates as the specified <see cref="T:System.Object" />.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKPoint3" /> and has the same coordinates as this <see cref="T:SkiaSharp.SKPoint3" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKPoint3 f && Equals (f);

		/// <summary>Compares two <see cref="T:SkiaSharp.SKPoint3" /> structures. The result specifies whether the values of the <see cref="P:SkiaSharp.SKPoint3.X" /> and <see cref="P:SkiaSharp.SKPoint3.Y" /> properties of the two <see cref="T:SkiaSharp.SKPoint3" /> structures are equal.</summary>
		/// <param name="left">A <see cref="T:SkiaSharp.SKPoint3" /> to compare.</param>
		/// <param name="right">A <see cref="T:SkiaSharp.SKPoint3" /> to compare.</param>
		/// <returns><see langword="true" /> if the <see cref="P:SkiaSharp.SKPoint3.X" />, <see cref="P:SkiaSharp.SKPoint3.Y" /> and <see cref="P:SkiaSharp.SKPoint3.Z" /> values of the left and right <see cref="T:SkiaSharp.SKPoint3" /> structures are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKPoint3 left, SKPoint3 right) =>
			left.Equals (right);

		/// <summary>Determines whether the coordinates of the specified points are not equal.</summary>
		/// <param name="left">A <see cref="T:SkiaSharp.SKPoint3" /> to compare.</param>
		/// <param name="right">A <see cref="T:SkiaSharp.SKPoint3" /> to compare.</param>
		/// <returns><see langword="true" /> if the <see cref="P:SkiaSharp.SKPoint3.X" />, <see cref="P:SkiaSharp.SKPoint3.Y" /> and <see cref="P:SkiaSharp.SKPoint3.Z" /> values of the left and right <see cref="T:SkiaSharp.SKPoint3" /> structures differ; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKPoint3 left, SKPoint3 right) =>
			!left.Equals (right);

		/// <summary>Calculates the hashcode for this point.</summary>
		/// <returns>Returns the hashcode for this point.</returns>
		/// <remarks>You should avoid depending on GetHashCode for unique values, as two <see cref="T:SkiaSharp.SKPoint3" /> objects with the same values for their X, Y and Z properties may return the same hash code. This behavior could change in a future release.</remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x);
			hash.Add (y);
			hash.Add (z);
			return hash.ToHashCode ();
		}

	}
}
