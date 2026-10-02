#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>Represents a 4-character OpenType tag used to identify tables, scripts, languages, and features.</summary>
	/// <remarks />
	public struct Tag : IEquatable<Tag>
	{
		/// <summary>Represents an empty or unset tag.</summary>
		/// <remarks />
		public static readonly Tag None = new Tag (0, 0, 0, 0);
		/// <summary>Represents the maximum tag value.</summary>
		/// <remarks />
		public static readonly Tag Max = new Tag (byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		/// <summary>Represents the maximum signed tag value.</summary>
		/// <remarks />
		public static readonly Tag MaxSigned = new Tag ((byte)sbyte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		private readonly uint value;

		private Tag (uint value)
		{
			this.value = value;
		}

		private Tag (byte c1, byte c2, byte c3, byte c4)
		{
			value = (uint)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.Tag" /> struct from four characters.</summary>
		/// <param name="c1">The first character of the tag.</param>
		/// <param name="c2">The second character of the tag.</param>
		/// <param name="c3">The third character of the tag.</param>
		/// <param name="c4">The fourth character of the tag.</param>
		/// <remarks />
		public Tag (char c1, char c2, char c3, char c4)
		{
			value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
		}

		/// <summary>Parses the ISO 15924 tag into the corresponding <see cref="T:HarfBuzzSharp.Tag" />.</summary>
		/// <param name="tag">The ISO 15924 tag to parse.</param>
		/// <returns>Returns the <see cref="T:HarfBuzzSharp.Tag" /> that corresponds the tag that was parsed.</returns>
		/// <remarks />
		public static Tag Parse (string tag) =>
			Parse (tag.AsSpan ());

		/// <summary>Parses the ISO 15924 tag into the corresponding <see cref="T:HarfBuzzSharp.Tag" />.</summary>
		/// <param name="tag">The ISO 15924 tag to parse.</param>
		/// <returns>Returns the <see cref="T:HarfBuzzSharp.Tag" /> that corresponds the tag that was parsed.</returns>
		/// <remarks />
		public static Tag Parse (ReadOnlySpan<char> tag)
		{
			if (tag.IsEmpty)
				return None;

			// Take up to the first four characters, padding any missing trailing
			// slots with spaces — matching the original char[4]-scratch behaviour
			// without allocating the scratch array. The first character always
			// exists here because empty input was handled by the guard above.
			var c1 = tag[0];
			var c2 = tag.Length > 1 ? tag[1] : ' ';
			var c3 = tag.Length > 2 ? tag[2] : ' ';
			var c4 = tag.Length > 3 ? tag[3] : ' ';

			return new Tag (c1, c2, c3, c4);
		}

		/// <summary>Returns a string representation of the value of this instance of the <see cref="T:HarfBuzzSharp.Tag" />.</summary>
		/// <returns>Returns a string representation.</returns>
		/// <remarks />
		public override unsafe string ToString ()
		{
			if (value == None) {
				return nameof (None);
			}
			if (value == Max) {
				return nameof (Max);
			}
			if (value == MaxSigned) {
				return nameof (MaxSigned);
			}

			// Build the 4-character string directly from a stack buffer. Passing four
			// chars to string.Concat binds to Concat(object, object, object, object),
			// which boxes every char (four extra allocations per call); writing into a
			// stackalloc'd buffer avoids that while producing the identical string.
			char* chars = stackalloc char[4];
			chars[0] = (char)(byte)(value >> 24);
			chars[1] = (char)(byte)(value >> 16);
			chars[2] = (char)(byte)(value >> 8);
			chars[3] = (char)(byte)value;
			return new string (chars, 0, 4);
		}

		/// <summary>Implicitly converts a <see cref="T:HarfBuzzSharp.Tag" /> to a <see cref="T:System.UInt32" />.</summary>
		/// <param name="tag">The tag to convert.</param>
		/// <returns>The unsigned integer representation of the tag.</returns>
		/// <remarks />
		public static implicit operator uint (Tag tag) => tag.value;

		/// <summary>Implicitly converts a <see cref="T:System.UInt32" /> to a <see cref="T:HarfBuzzSharp.Tag" />.</summary>
		/// <param name="tag">The unsigned integer to convert.</param>
		/// <returns>The <see cref="T:HarfBuzzSharp.Tag" /> representation of the unsigned integer.</returns>
		/// <remarks />
		public static implicit operator Tag (uint tag) => new Tag (tag);

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override bool Equals (object obj) =>
			obj is Tag tag && value.Equals (tag.value);

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.Tag" /> is equal to this instance.</summary>
		/// <param name="other">The <see cref="T:HarfBuzzSharp.Tag" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.Tag" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Equals (Tag other) => value == other.value;

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public override int GetHashCode () => (int)value;
	}
}
