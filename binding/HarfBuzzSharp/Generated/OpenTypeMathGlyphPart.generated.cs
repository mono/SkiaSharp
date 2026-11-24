using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_glyph_part_t
	/// <summary>Represents a part of a math glyph assembly used for extensible math constructs.</summary>
	/// <remarks>Glyph parts are used to construct extensible mathematical symbols such as large parentheses, brackets, and radical signs.</remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeMathGlyphPart : IEquatable<OpenTypeMathGlyphPart> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		/// <summary>Gets or sets the glyph identifier for this part.</summary>
		/// <value>The glyph identifier.</value>
		/// <remarks />
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public hb_position_t start_connector_length
		private Int32 start_connector_length;
		/// <summary>Gets or sets the length of the start connector, in font design units.</summary>
		/// <value>The start connector length.</value>
		/// <remarks />
		public Int32 StartConnectorLength {
			readonly get => start_connector_length;
			set => start_connector_length = value;
		}

		// public hb_position_t end_connector_length
		private Int32 end_connector_length;
		/// <summary>Gets or sets the length of the end connector, in font design units.</summary>
		/// <value>The end connector length.</value>
		/// <remarks />
		public Int32 EndConnectorLength {
			readonly get => end_connector_length;
			set => end_connector_length = value;
		}

		// public hb_position_t full_advance
		private Int32 full_advance;
		/// <summary>Gets or sets the full advance of this part in the direction of the assembly, in font design units.</summary>
		/// <value>The full advance value.</value>
		/// <remarks />
		public Int32 FullAdvance {
			readonly get => full_advance;
			set => full_advance = value;
		}

		// public hb_ot_math_glyph_part_flags_t flags
		private OpenTypeMathGlyphPartFlags flags;
		/// <summary>Gets or sets the flags for this glyph part.</summary>
		/// <value>The glyph part flags.</value>
		/// <remarks />
		public OpenTypeMathGlyphPartFlags Flags {
			readonly get => flags;
			set => flags = value;
		}

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.OpenTypeMathGlyphPart" /> is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (OpenTypeMathGlyphPart obj) =>
#pragma warning disable CS8909
			glyph == obj.glyph && start_connector_length == obj.start_connector_length && end_connector_length == obj.end_connector_length && full_advance == obj.full_advance && flags == obj.flags;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to the current instance.</summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		/// <returns><see langword="true" /> if the objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is OpenTypeMathGlyphPart f && Equals (f);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.OpenTypeMathGlyphPart" /> structures are equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.OpenTypeMathGlyphPart" /> structures are not equal.</summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		/// <returns><see langword="true" /> if the values are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this instance.</summary>
		/// <returns>A hash code for the current object.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (start_connector_length);
			hash.Add (end_connector_length);
			hash.Add (full_advance);
			hash.Add (flags);
			return hash.ToHashCode ();
		}

	}
}
