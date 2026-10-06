using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_glyph_variant_t
	/// <summary>Represents a math glyph variant used in mathematical typesetting.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeMathGlyphVariant : IEquatable<OpenTypeMathGlyphVariant> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		/// <summary>Gets or sets the glyph index of the variant.</summary>
		/// <value>The glyph index.</value>
		/// <remarks />
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public hb_position_t advance
		private Int32 advance;
		/// <summary>Gets or sets the advance measurement of the variant glyph.</summary>
		/// <value>The advance measurement in font units.</value>
		/// <remarks />
		public Int32 Advance {
			readonly get => advance;
			set => advance = value;
		}

		/// <summary>Determines whether the specified OpenTypeMathGlyphVariant is equal to this instance.</summary>
		/// <param name="obj">The OpenTypeMathGlyphVariant to compare with.</param>
		/// <returns><see langword="true" /> if the specified OpenTypeMathGlyphVariant is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (OpenTypeMathGlyphVariant obj) =>
#pragma warning disable CS8909
			glyph == obj.glyph && advance == obj.advance;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns><see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is OpenTypeMathGlyphVariant f && Equals (f);

		/// <summary>Determines whether two specified OpenTypeMathGlyphVariant objects are equal.</summary>
		/// <param name="left">The first OpenTypeMathGlyphVariant to compare.</param>
		/// <param name="right">The second OpenTypeMathGlyphVariant to compare.</param>
		/// <returns><see langword="true" /> if the two OpenTypeMathGlyphVariant objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified OpenTypeMathGlyphVariant objects are not equal.</summary>
		/// <param name="left">The first OpenTypeMathGlyphVariant to compare.</param>
		/// <param name="right">The second OpenTypeMathGlyphVariant to compare.</param>
		/// <returns><see langword="true" /> if the two OpenTypeMathGlyphVariant objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (advance);
			return hash.ToHashCode ();
		}

	}
}
