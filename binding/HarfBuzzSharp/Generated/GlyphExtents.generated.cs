using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_extents_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphExtents : IEquatable<GlyphExtents> {
		// public hb_position_t x_bearing
		private Int32 x_bearing;
		public Int32 XBearing {
			readonly get => x_bearing;
			set => x_bearing = value;
		}

		// public hb_position_t y_bearing
		private Int32 y_bearing;
		public Int32 YBearing {
			readonly get => y_bearing;
			set => y_bearing = value;
		}

		// public hb_position_t width
		private Int32 width;
		public Int32 Width {
			readonly get => width;
			set => width = value;
		}

		// public hb_position_t height
		private Int32 height;
		public Int32 Height {
			readonly get => height;
			set => height = value;
		}

		public readonly bool Equals (GlyphExtents obj) =>
#pragma warning disable CS8909
			x_bearing == obj.x_bearing && y_bearing == obj.y_bearing && width == obj.width && height == obj.height;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GlyphExtents f && Equals (f);

		public static bool operator == (GlyphExtents left, GlyphExtents right) =>
			left.Equals (right);

		public static bool operator != (GlyphExtents left, GlyphExtents right) =>
			!left.Equals (right);

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
