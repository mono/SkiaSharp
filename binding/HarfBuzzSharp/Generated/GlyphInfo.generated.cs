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
		/// <summary>Gets or sets the Unicode code point (or the glyph index after shaping).</summary>
		/// <value>The Unicode code point or glyph index.</value>
		/// <remarks>This represents either a Unicode code point (before shaping) or a glyph index (after shaping).</remarks>
		public UInt32 Codepoint {
			readonly get => codepoint;
			set => codepoint = value;
		}

		// public hb_mask_t mask
		private UInt32 mask;
		/// <summary>Gets or sets the glyph mask.</summary>
		/// <value>The glyph mask.</value>
		/// <remarks />
		public UInt32 Mask {
			readonly get => mask;
			set => mask = value;
		}

		// public uint32_t cluster
		private UInt32 cluster;
		/// <summary>Gets or sets the index of the character in the original text.</summary>
		/// <value>The index of the character in the original text.</value>
		/// <remarks />
		public UInt32 Cluster {
			readonly get => cluster;
			set => cluster = value;
		}

		// public hb_var_int_t var1
		private Int32 var1;

		// public hb_var_int_t var2
		private Int32 var2;

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.GlyphInfo" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:HarfBuzzSharp.GlyphInfo" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.GlyphInfo" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GlyphInfo obj) =>
#pragma warning disable CS8909
			codepoint == obj.codepoint && mask == obj.mask && cluster == obj.cluster && var1 == obj.var1 && var2 == obj.var2;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GlyphInfo f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.GlyphInfo" /> objects are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.GlyphInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.GlyphInfo" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.GlyphInfo" /> objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GlyphInfo left, GlyphInfo right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.GlyphInfo" /> objects are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.GlyphInfo" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.GlyphInfo" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.GlyphInfo" /> objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GlyphInfo left, GlyphInfo right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
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
