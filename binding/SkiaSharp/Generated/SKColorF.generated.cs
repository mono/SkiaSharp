using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_color4f_t
	[StructLayout (LayoutKind.Sequential)]
	public readonly unsafe partial struct SKColorF : IEquatable<SKColorF> {
		// public float fR
		private readonly Single fR;
		/// <summary>Gets the red component of the color.</summary>
		/// <value>The red component of the color.</value>
		/// <remarks />
		public readonly Single Red => fR;

		// public float fG
		private readonly Single fG;
		/// <summary>Gets the green component of the color.</summary>
		/// <value>The green component of the color.</value>
		/// <remarks />
		public readonly Single Green => fG;

		// public float fB
		private readonly Single fB;
		/// <summary>Gets the blue component of the color.</summary>
		/// <value>The blue component of the color.</value>
		/// <remarks />
		public readonly Single Blue => fB;

		// public float fA
		private readonly Single fA;
		/// <summary>Gets the alpha component of the color.</summary>
		/// <value>The alpha component of the color.</value>
		/// <remarks />
		public readonly Single Alpha => fA;

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The color to compare with the current color.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKColorF obj) =>
#pragma warning disable CS8909
			fR == obj.fR && fG == obj.fG && fB == obj.fB && fA == obj.fA;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is SKColorF f && Equals (f);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKColorF" /> objects are equal.</summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKColorF left, SKColorF right) =>
			left.Equals (right);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKColorF" /> objects are different.</summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKColorF left, SKColorF right) =>
			!left.Equals (right);

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>Returns a hash code for the current object.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (fR);
			hash.Add (fG);
			hash.Add (fB);
			hash.Add (fA);
			return hash.ToHashCode ();
		}

	}
}
