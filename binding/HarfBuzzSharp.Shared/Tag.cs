using System;

namespace HarfBuzzSharp
{
	public struct Tag
	{
		public uint Value;

		private Tag (byte c1, byte c2, byte c3, byte c4)
		{
			Value = (uint)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
		}

		public Tag (char c1, char c2, char c3, char c4)
		{
			Value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
		}

		public Tag (string tag)
		{
			if (tag.Length != 4) {
				throw new ArgumentException (nameof (tag));
			}

			Value = (uint)(((byte)tag[0] << 24) | ((byte)tag[1] << 16) | ((byte)tag[2] << 8) | (byte)tag[3]);
		}

		public override string ToString ()
		{
			return string.Concat ((char)(byte)(Value >> 24), (char)(byte)(Value >> 16),
				(char)(byte)(Value >> 8), (char)(byte)Value);
		}

		public static readonly Tag None = new Tag (0, 0, 0, 0);

		public static readonly Tag Max = new Tag (byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static readonly Tag MaxSigned = new Tag ((byte)sbyte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static implicit operator uint (Tag tag) => tag.Value;

		public static implicit operator Tag (uint tag) => new Tag { Value = tag };
	}
}
