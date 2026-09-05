#nullable disable

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace HarfBuzzSharp
{
	public unsafe partial struct Feature
	{
		private const int MaxFeatureStringSize = 128;

		public Feature (Tag tag)
			: this (tag, 1u, 0, uint.MaxValue)
		{
		}

		public Feature (Tag tag, uint value)
			: this (tag, value, 0, uint.MaxValue)
		{
		}

		public Feature (Tag tag, uint value, uint start, uint end)
		{
			this.tag = tag;
			this.value = value;
			this.start = start;
			this.end = end;
		}

		public Tag Tag {
			readonly get => tag;
			set => tag = value;
		}

		public uint Value {
			readonly get => value;
			set => this.value = value;
		}

		public uint Start {
			readonly get => start;
			set => start = value;
		}

		public uint End {
			readonly get => end;
			set => end = value;
		}

		public override string ToString ()
		{
			Span<byte> buffer = stackalloc byte[MaxFeatureStringSize];
			fixed (Feature* f = &this)
			fixed (byte* b = buffer) {
				HarfBuzzApi.hb_feature_to_string (f, b, MaxFeatureStringSize);
			}
			var len = buffer.IndexOf ((byte)0);
			if (len < 0)
				len = MaxFeatureStringSize;
			return len == 0 ? string.Empty : Encoding.ASCII.GetString (buffer.Slice (0, len));
		}

		public static bool TryParse (string s, out Feature feature)
		{
			fixed (Feature* f = &feature) {
				return HarfBuzzApi.hb_feature_from_string (s, -1, f);
			}
		}

		public static Feature Parse (string s) =>
			TryParse (s, out var feature) ? feature : throw new FormatException ("Unrecognized feature string format.");
	}
}
