#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// To be added.
	/// </summary>
	/// <remarks>
	/// To be added.
	/// </remarks>
	public struct Tag : IEquatable<Tag>
	{
		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public static readonly Tag None = new Tag (0, 0, 0, 0);
		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public static readonly Tag Max = new Tag (byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		/// <summary>
		/// To be added.
		/// </summary>
		/// <remarks>
		/// To be added.
		/// </remarks>
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

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="c1">To be added.</param>
		/// <param name="c2">To be added.</param>
		/// <param name="c3">To be added.</param>
		/// <param name="c4">To be added.</param>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public Tag (char c1, char c2, char c3, char c4)
		{
			value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
		}

		/// <summary>
		/// Parses the ISO 15924 tag into the corresponding <see cref="T:HarfBuzzSharp.Tag" />.
		/// </summary>
		/// <param name="tag">The ISO 15924 tag to parse.</param>
		/// <returns>Returns the <see cref="T:HarfBuzzSharp.Tag" /> that corresponds the tag that was parsed.</returns>
		/// <remarks></remarks>
		public static Tag Parse (string tag)
		{
			if (string.IsNullOrEmpty (tag))
				return None;

			var realTag = new char[4];

			var len = Math.Min (4, tag.Length);
			var i = 0;
			for (; i < len; i++)
				realTag[i] = tag[i];
			for (; i < 4; i++)
				realTag[i] = ' ';

			return new Tag (realTag[0], realTag[1], realTag[2], realTag[3]);
		}

		/// <summary>
		/// Returns a string representation of the value of this instance of the <see cref="T:HarfBuzzSharp.Tag" />.
		/// </summary>
		/// <returns>Returns a string representation.</returns>
		/// <remarks></remarks>
		public override string ToString ()
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

			return string.Concat (
				(char)(byte)(value >> 24),
				(char)(byte)(value >> 16),
				(char)(byte)(value >> 8),
				(char)(byte)value);
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="tag">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public static implicit operator uint (Tag tag) => tag.value;

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="tag">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public static implicit operator Tag (uint tag) => new Tag (tag);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="obj">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public override bool Equals (object obj) =>
			obj is Tag tag && value.Equals (tag.value);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="other">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public bool Equals (Tag other) => value == other.value;

		/// <summary>
		/// To be added.
		/// </summary>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// To be added.
		/// </remarks>
		public override int GetHashCode () => (int)value;
	}
}
