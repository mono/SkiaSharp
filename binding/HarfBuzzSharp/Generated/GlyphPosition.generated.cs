using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_position_t
	/// <summary>Represents the position of a glyph, relative to the current point.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphPosition : IEquatable<GlyphPosition> {
		// public hb_position_t x_advance
		private Int32 x_advance;
		/// <summary>Gets or sets how much the line advances after drawing this glyph when setting text in horizontal direction.</summary>
		/// <value>The horizontal advance, in font units.</value>
		/// <remarks />
		public Int32 XAdvance {
			readonly get => x_advance;
			set => x_advance = value;
		}

		// public hb_position_t y_advance
		private Int32 y_advance;
		/// <summary>Gets or sets how much the line advances after drawing this glyph when setting text in vertical direction.</summary>
		/// <value>The vertical advance, in font units.</value>
		/// <remarks />
		public Int32 YAdvance {
			readonly get => y_advance;
			set => y_advance = value;
		}

		// public hb_position_t x_offset
		private Int32 x_offset;
		/// <summary>Gets or sets how much the glyph moves horizontally before drawing it.</summary>
		/// <value>The horizontal offset, in font units.</value>
		/// <remarks>This should not affect how much the line advances.</remarks>
		public Int32 XOffset {
			readonly get => x_offset;
			set => x_offset = value;
		}

		// public hb_position_t y_offset
		private Int32 y_offset;
		/// <summary>Gets or sets how much the glyph moves vertically before drawing it.</summary>
		/// <value>The vertical offset, in font units.</value>
		/// <remarks>This should not affect how much the line advances.</remarks>
		public Int32 YOffset {
			readonly get => y_offset;
			set => y_offset = value;
		}

		// public hb_var_int_t var
		private Int32 var;

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.GlyphPosition" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:HarfBuzzSharp.GlyphPosition" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.GlyphPosition" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GlyphPosition obj) =>
#pragma warning disable CS8909
			x_advance == obj.x_advance && y_advance == obj.y_advance && x_offset == obj.x_offset && y_offset == obj.y_offset && var == obj.var;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GlyphPosition f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.GlyphPosition" /> objects are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.GlyphPosition" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.GlyphPosition" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.GlyphPosition" /> objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GlyphPosition left, GlyphPosition right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.GlyphPosition" /> objects are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.GlyphPosition" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.GlyphPosition" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.GlyphPosition" /> objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GlyphPosition left, GlyphPosition right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x_advance);
			hash.Add (y_advance);
			hash.Add (x_offset);
			hash.Add (y_offset);
			hash.Add (var);
			return hash.ToHashCode ();
		}

	}
}
