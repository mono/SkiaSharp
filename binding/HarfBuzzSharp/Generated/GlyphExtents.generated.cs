using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_extents_t
	/// <summary>Represents the bounding box of a glyph.</summary>
	/// <remarks />
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphExtents : IEquatable<GlyphExtents> {
		// public hb_position_t x_bearing
		private Int32 x_bearing;
		/// <summary>Gets or sets the horizontal bearing (left side bearing).</summary>
		/// <value>The horizontal distance from the origin to the left edge of the glyph in font units.</value>
		/// <remarks />
		public Int32 XBearing {
			readonly get => x_bearing;
			set => x_bearing = value;
		}

		// public hb_position_t y_bearing
		private Int32 y_bearing;
		/// <summary>Gets or sets the vertical bearing (top side bearing).</summary>
		/// <value>The vertical distance from the origin to the top edge of the glyph in font units.</value>
		/// <remarks />
		public Int32 YBearing {
			readonly get => y_bearing;
			set => y_bearing = value;
		}

		// public hb_position_t width
		private Int32 width;
		/// <summary>Gets or sets the width of the glyph extents.</summary>
		/// <value>The width in font units.</value>
		/// <remarks />
		public Int32 Width {
			readonly get => width;
			set => width = value;
		}

		// public hb_position_t height
		private Int32 height;
		/// <summary>Gets or sets the height of the glyph extents.</summary>
		/// <value>The height in font units.</value>
		/// <remarks>Typically negative in coordinate systems where Y grows upward.</remarks>
		public Int32 Height {
			readonly get => height;
			set => height = value;
		}

		/// <summary>Determines whether the specified <see cref="T:HarfBuzzSharp.GlyphExtents" /> is equal to this instance.</summary>
		/// <param name="obj">The <see cref="T:HarfBuzzSharp.GlyphExtents" /> to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified <see cref="T:HarfBuzzSharp.GlyphExtents" /> is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (GlyphExtents obj) =>
#pragma warning disable CS8909
			x_bearing == obj.x_bearing && y_bearing == obj.y_bearing && width == obj.width && height == obj.height;
#pragma warning restore CS8909

		/// <summary>Determines whether the specified object is equal to this instance.</summary>
		/// <param name="obj">The object to compare with.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to this instance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object obj) =>
			obj is GlyphExtents f && Equals (f);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.GlyphExtents" /> objects are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.GlyphExtents" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.GlyphExtents" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.GlyphExtents" /> objects are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (GlyphExtents left, GlyphExtents right) =>
			left.Equals (right);

		/// <summary>Determines whether two specified <see cref="T:HarfBuzzSharp.GlyphExtents" /> objects are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.GlyphExtents" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.GlyphExtents" /> to compare.</param>
		/// <returns>Returns <see langword="true" /> if the two <see cref="T:HarfBuzzSharp.GlyphExtents" /> objects are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (GlyphExtents left, GlyphExtents right) =>
			!left.Equals (right);

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		/// <remarks />
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x_bearing);
			hash.Add (y_bearing);
			hash.Add (width);
			hash.Add (height);
			return hash.ToHashCode ();
		}

	}
}
