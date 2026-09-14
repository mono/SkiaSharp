using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphInfo : IEquatable<GlyphInfo> {
		// public hb_codepoint_t codepoint
		private UInt32 codepoint;
		public UInt32 Codepoint {
			readonly get => codepoint;
			set => codepoint = value;
		}

		// public hb_mask_t mask
		private UInt32 mask;
		public UInt32 Mask {
			readonly get => mask;
			set => mask = value;
		}

		// public uint32_t cluster
		private UInt32 cluster;
		public UInt32 Cluster {
			readonly get => cluster;
			set => cluster = value;
		}

		// public hb_var_int_t var1
		private Int32 var1;

		// public hb_var_int_t var2
		private Int32 var2;

		public readonly bool Equals (GlyphInfo obj) =>
#pragma warning disable CS8909
			codepoint == obj.codepoint && mask == obj.mask && cluster == obj.cluster && var1 == obj.var1 && var2 == obj.var2;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GlyphInfo f && Equals (f);

		public static bool operator == (GlyphInfo left, GlyphInfo right) =>
			left.Equals (right);

		public static bool operator != (GlyphInfo left, GlyphInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (codepoint);
			hash.Add (mask);
			hash.Add (cluster);
			hash.Add (var1);
			hash.Add (var2);
			return hash.ToHashCode ();
		}

	}
}
