using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_variation_t
	/// <summary>Represents a font variation axis setting for variable fonts.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct Variation : IEquatable<Variation> {
		// public hb_tag_t tag
		private UInt32 tag;
		/// <summary>Gets or sets the variation axis tag.</summary>
		/// <value>A 4-byte tag identifying the variation axis (e.g., 'wght' for weight, 'wdth' for width).</value>
		/// <remarks />
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public float value
		private Single value;
		/// <summary>Gets or sets the variation axis value.</summary>
		/// <value>The value for the variation axis within its defined range.</value>
		/// <remarks />
		public Single Value {
			readonly get => this.value;
			set => this.value = value;
		}

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.Variation" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:HarfBuzzSharp.Variation" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.Variation" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (Variation obj) =>
#pragma warning disable CS8909
			tag == obj.tag && value == obj.value;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is Variation f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.Variation" /> objects are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.Variation" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.Variation" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.Variation" /> objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (Variation left, Variation right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.Variation" /> objects are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.Variation" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.Variation" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.Variation" /> objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (Variation left, Variation right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (value);
			return hash.ToHashCode ();
		}

	}
}
