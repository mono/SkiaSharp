using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_font_extents_t
	/// <summary>Represents font-wide extent values for horizontal or vertical text layout.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct FontExtents : IEquatable<FontExtents> {
		// public hb_position_t ascender
		private Int32 ascender;
		/// <summary>Gets or sets the typographic ascender.</summary>
		/// <value>The distance from the baseline to the top of the highest glyph in font units.</value>
		/// <remarks />
		public Int32 Ascender {
			readonly get => ascender;
			set => ascender = value;
		}

		// public hb_position_t descender
		private Int32 descender;
		/// <summary>Gets or sets the typographic descender.</summary>
		/// <value>The distance from the baseline to the bottom of the lowest glyph in font units.</value>
		/// <remarks />
		public Int32 Descender {
			readonly get => descender;
			set => descender = value;
		}

		// public hb_position_t line_gap
		private Int32 line_gap;
		/// <summary>Gets or sets the suggested line gap between lines of text.</summary>
		/// <value>The recommended spacing between consecutive lines of text in font units.</value>
		/// <remarks />
		public Int32 LineGap {
			readonly get => line_gap;
			set => line_gap = value;
		}

		// public hb_position_t reserved9
		private Int32 reserved9;

		// public hb_position_t reserved8
		private Int32 reserved8;

		// public hb_position_t reserved7
		private Int32 reserved7;

		// public hb_position_t reserved6
		private Int32 reserved6;

		// public hb_position_t reserved5
		private Int32 reserved5;

		// public hb_position_t reserved4
		private Int32 reserved4;

		// public hb_position_t reserved3
		private Int32 reserved3;

		// public hb_position_t reserved2
		private Int32 reserved2;

		// public hb_position_t reserved1
		private Int32 reserved1;

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.FontExtents" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:HarfBuzzSharp.FontExtents" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.FontExtents" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (FontExtents obj) =>
#pragma warning disable CS8909
			ascender == obj.ascender && descender == obj.descender && line_gap == obj.line_gap && reserved9 == obj.reserved9 && reserved8 == obj.reserved8 && reserved7 == obj.reserved7 && reserved6 == obj.reserved6 && reserved5 == obj.reserved5 && reserved4 == obj.reserved4 && reserved3 == obj.reserved3 && reserved2 == obj.reserved2 && reserved1 == obj.reserved1;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is FontExtents f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.FontExtents" /> objects are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.FontExtents" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.FontExtents" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.FontExtents" /> objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (FontExtents left, FontExtents right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.FontExtents" /> objects are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.FontExtents" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.FontExtents" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.FontExtents" /> objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (FontExtents left, FontExtents right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (ascender);
			hash.Add (descender);
			hash.Add (line_gap);
			hash.Add (reserved9);
			hash.Add (reserved8);
			hash.Add (reserved7);
			hash.Add (reserved6);
			hash.Add (reserved5);
			hash.Add (reserved4);
			hash.Add (reserved3);
			hash.Add (reserved2);
			hash.Add (reserved1);
			return hash.ToHashCode ();
		}

	}
}
