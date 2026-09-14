using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_glyph_variant_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeMathGlyphVariant : IEquatable<OpenTypeMathGlyphVariant> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public hb_position_t advance
		private Int32 advance;
		public Int32 Advance {
			readonly get => advance;
			set => advance = value;
		}

		public readonly bool Equals (OpenTypeMathGlyphVariant obj) =>
#pragma warning disable CS8909
			glyph == obj.glyph && advance == obj.advance;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeMathGlyphVariant f && Equals (f);

		public static bool operator == (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (advance);
			return hash.ToHashCode ();
		}

	}
}
