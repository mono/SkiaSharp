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
