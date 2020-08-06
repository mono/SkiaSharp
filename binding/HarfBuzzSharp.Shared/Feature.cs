using System;
using System.Runtime.InteropServices;

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

		public override string ToString ()
		{
			fixed (Feature* f = &this) {
				var buffer = Marshal.AllocHGlobal (MaxFeatureStringSize);
				HarfBuzzApi.hb_feature_to_string (f, (void*)buffer, MaxFeatureStringSize);
				var str = Marshal.PtrToStringAnsi (buffer);
				Marshal.FreeHGlobal (buffer);
				return str;
			}
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
