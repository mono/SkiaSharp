#nullable disable

using System;

namespace HarfBuzzSharp
{
	public struct Tag : IEquatable<Tag>
	{
		public static readonly Tag None = new Tag (0, 0, 0, 0);
		public static readonly Tag Max = new Tag (byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
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

		public Tag (char c1, char c2, char c3, char c4)
		{
			value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
		}

		public static Tag Parse (string tag) =>
			Parse (tag.AsSpan ());

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

		public static implicit operator uint (Tag tag) => tag.value;

		public static implicit operator Tag (uint tag) => new Tag (tag);

		public override bool Equals (object obj) =>
			obj is Tag tag && value.Equals (tag.value);

		public bool Equals (Tag other) => value == other.value;

		public override int GetHashCode () => (int)value;
	}
}
