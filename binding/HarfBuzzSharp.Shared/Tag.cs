using System;

namespace HarfBuzzSharp
{
	public struct Tag
	{
		private uint _value;

		private Tag (byte c1, byte c2, byte c3, byte c4)
		{
			_value = (uint)((c1 << 24) | (c2 << 16) | (c3 << 8) | c4);
		}

		public Tag (char c1, char c2, char c3, char c4)
		{
			_value = (uint)(((byte)c1 << 24) | ((byte)c2 << 16) | ((byte)c3 << 8) | (byte)c4);
		}

		public Tag (string tag)
		{
			if (tag.Length != 4) {
				throw new ArgumentException (nameof (tag));
			}

			_value = (uint)(((byte)tag[0] << 24) | ((byte)tag[1] << 16) | ((byte)tag[2] << 8) | (byte)tag[3]);
		}

		public override string ToString ()
		{
			if (_value == None) {
				return "None";
			}

			if (_value == Max) {
				return "Max";
			}

			if (_value == MaxSigned) {
				return "MaxSigned";
			}

			return string.Concat ((char)(byte)(_value >> 24), (char)(byte)(_value >> 16),
				(char)(byte)(_value >> 8), (char)(byte)_value);
		}

		public static readonly Tag None = new Tag (0, 0, 0, 0);

		public static readonly Tag Max = new Tag (byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static readonly Tag MaxSigned = new Tag ((byte)sbyte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		public static implicit operator uint (Tag tag) => tag._value;

		public static implicit operator Tag (uint tag) => new Tag { _value = tag };

		public override bool Equals (object obj) => base.Equals (obj);

		public bool Equals (Tag other) => _value == other._value;

		public override int GetHashCode() => (int)_value;
	}
}
