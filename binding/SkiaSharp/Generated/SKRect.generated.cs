using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_rect_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKRect : IEquatable<SKRect> {
		// public float left
		private Single left;
		/// <summary>Gets or sets the x-coordinate of the left edge of this <see cref="T:SkiaSharp.SKRect" /> structure.</summary>
		/// <value>The x-coordinate of the left edge.</value>
		/// <remarks />
		public Single Left {
			readonly get => left;
			set => left = value;
		}

		// public float top
		private Single top;
		/// <summary>Gets or sets the y-coordinate of the top edge of this <see cref="T:SkiaSharp.SKRect" /> structure.</summary>
		/// <value>The y-coordinate of the top edge.</value>
		/// <remarks />
		public Single Top {
			readonly get => top;
			set => top = value;
		}

		// public float right
		private Single right;
		/// <summary>Gets or sets the x-coordinate of the right edge of this <see cref="T:SkiaSharp.SKRect" /> structure.</summary>
		/// <value>The x-coordinate of the right edge.</value>
		/// <remarks />
		public Single Right {
			readonly get => right;
			set => right = value;
		}

		// public float bottom
		private Single bottom;
		/// <summary>Gets or sets the y-coordinate of the bottom edge of this <see cref="T:SkiaSharp.SKRect" /> structure.</summary>
		/// <value>The y-coordinate of the bottom edge.</value>
		/// <remarks />
		public Single Bottom {
			readonly get => bottom;
			set => bottom = value;
		}

		/// <summary>Specifies whether this rectangle contains the same coordinates as the specified <see cref="T:SkiaSharp.SKRect" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKRect" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> has the same coordinates as this <see cref="T:SkiaSharp.SKRect" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKRect obj) =>
#pragma warning disable CS8909
			left == obj.left && top == obj.top && right == obj.right && bottom == obj.bottom;
#pragma warning restore CS8909

		/// <summary>Specifies whether this rectangle contains the same coordinates as the specified <see cref="T:System.Object" />.</summary>
		/// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKRect" /> and has the same coordinates as this <see cref="T:SkiaSharp.SKRect" />; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKRect f && Equals (f);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKRect" /> structures have equal coordinates.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKRect" /> structure that is to the left of the equality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKRect" /> structure that is to the right of the equality operator.</param>
		/// <returns><see langword="true" /> if the two specified <see cref="T:SkiaSharp.SKRect" /> structures have equal <see cref="P:SkiaSharp.SKRect.Left" />, <see cref="P:SkiaSharp.SKRect.Top" />, <see cref="P:SkiaSharp.SKRect.Right" />, and <see cref="P:SkiaSharp.SKRect.Bottom" /> properties.</returns>
		/// <remarks />
		public static bool operator == (SKRect left, SKRect right) =>
			left.Equals (right);

		/// <summary>Tests whether two <see cref="T:SkiaSharp.SKRect" /> structures differ in coordinates.</summary>
		/// <param name="left">The <see cref="T:SkiaSharp.SKRect" /> structure that is to the left of the inequality operator.</param>
		/// <param name="right">The <see cref="T:SkiaSharp.SKRect" /> structure that is to the right of the inequality operator.</param>
		/// <returns><see langword="true" /> if any of the <see cref="P:SkiaSharp.SKRect.Left" />, <see cref="P:SkiaSharp.SKRect.Top" />, <see cref="P:SkiaSharp.SKRect.Right" />, or <see cref="P:SkiaSharp.SKRect.Bottom" /> properties of the two <see cref="T:SkiaSharp.SKRect" /> structures are unequal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKRect left, SKRect right) =>
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
