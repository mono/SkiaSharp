using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_irect_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRectI : IEquatable<SKRectI> {
		// public int32_t left
		private Int32 left;
		/// <summary>Gets or sets the x-coordinate of the left edge of this <see cref="T:SkiaSharp.SKRectI" /> structure.</summary>
		/// <value>The x-coordinate of the left edge.</value>
		/// <remarks />
		public Int32 Left {
			readonly get => left;
			set => left = value;
		}

		// public int32_t top
		private Int32 top;
		/// <summary>Gets or sets the y-coordinate of the top edge of this <see cref="T:SkiaSharp.SKRectI" /> structure.</summary>
		/// <value>The y-coordinate of the top edge.</value>
		/// <remarks />
		public Int32 Top {
			readonly get => top;
			set => top = value;
		}

		// public int32_t right
		private Int32 right;
		/// <summary>Gets or sets the x-coordinate of the right edge of this <see cref="T:SkiaSharp.SKRectI" /> structure.</summary>
		/// <value>The x-coordinate of the right edge.</value>
		/// <remarks />
		public Int32 Right {
			readonly get => right;
			set => right = value;
		}

		// public int32_t bottom
		private Int32 bottom;
		/// <summary>Gets or sets the y-coordinate of the bottom edge of this <see cref="T:SkiaSharp.SKRectI" /> structure.</summary>
		/// <value>The y-coordinate of the bottom edge.</value>
		/// <remarks />
		public Int32 Bottom {
			readonly get => bottom;
			set => bottom = value;
		}

		/// <summary>Specifies whether this rectangle contains the same coordinates as the specified <see cref="T:SkiaSharp.SKRectI" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKRectI" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> has the same coordinates as this <see cref="T:SkiaSharp.SKRectI" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKRectI obj) =>
#pragma warning disable CS8909
			left == obj.left && top == obj.top && right == obj.right && bottom == obj.bottom;
#pragma warning restore CS8909

		/// <summary>Specifies whether this rectangle contains the same coordinates as the specified <see cref="T:System.Object" />.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKRectI" /> and has the same coordinates as this <see cref="T:SkiaSharp.SKRectI" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKRectI f && Equals (f);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKRectI" /> structures have equal location and size.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKRectI" /> structure that is to the left of the equality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKRectI" /> structure that is to the right of the equality operator.</param>
		/// <returns><see langword="true" /> if the two specified <see cref="T:SkiaSharp.SKRectI" /> structures have equal <see cref="P:SkiaSharp.SKRectI.Left" />, <see cref="P:SkiaSharp.SKRectI.Top" />, <see cref="P:SkiaSharp.SKRectI.Right" />, or <see cref="P:SkiaSharp.SKRectI.Bottom" /> properties.</returns>
		/// <remarks />
		public static bool operator == (SKRectI left, SKRectI right) =>
			left.Equals (right);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKRectI" /> structures differ in location or size.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKRectI" /> structure that is to the left of the inequality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKRectI" /> structure that is to the right of the inequality operator.</param>
		/// <returns><see langword="true" /> if any of the <see cref="P:SkiaSharp.SKRectI.Left" />, <see cref="P:SkiaSharp.SKRectI.Top" />, <see cref="P:SkiaSharp.SKRectI.Right" />, or <see cref="P:SkiaSharp.SKRectI.Bottom" /> properties of the two <see cref="T:SkiaSharp.SKRectI" /> structures are unequal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKRectI left, SKRectI right) =>
			!left.Equals (right);

		/// <summary>Calculates the hashcode for this rectangle.</summary>
		/// <returns>Returns the hashcode for this rectangle.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (left);
			hash.Add (top);
			hash.Add (right);
			hash.Add (bottom);
			return hash.ToHashCode ();
		}

	}
}
