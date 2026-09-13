using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_feature_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct Feature : IEquatable<Feature> {
		// public hb_tag_t tag
		private UInt32 tag;

		// public uint32_t value
		private UInt32 value;

		// public unsigned int start
		private UInt32 start;

		// public unsigned int end
		private UInt32 end;

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.Feature" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:HarfBuzzSharp.Feature" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.Feature" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (Feature obj) =>
#pragma warning disable CS8909
			tag == obj.tag && value == obj.value && start == obj.start && end == obj.end;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is Feature f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.Feature" /> objects are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.Feature" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.Feature" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.Feature" /> objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (Feature left, Feature right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.Feature" /> objects are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.Feature" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.Feature" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.Feature" /> objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (Feature left, Feature right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (value);
			hash.Add (start);
			hash.Add (end);
			return hash.ToHashCode ();
		}

	}
}
