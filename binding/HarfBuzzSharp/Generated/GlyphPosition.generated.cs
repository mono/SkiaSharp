using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_glyph_position_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphPosition : IEquatable<GlyphPosition> {
		// public hb_position_t x_advance
		private Int32 x_advance;
		public Int32 XAdvance {
			readonly get => x_advance;
			set => x_advance = value;
		}

		// public hb_position_t y_advance
		private Int32 y_advance;
		public Int32 YAdvance {
			readonly get => y_advance;
			set => y_advance = value;
		}

		// public hb_position_t x_offset
		private Int32 x_offset;
		public Int32 XOffset {
			readonly get => x_offset;
			set => x_offset = value;
		}

		// public hb_position_t y_offset
		private Int32 y_offset;
		public Int32 YOffset {
			readonly get => y_offset;
			set => y_offset = value;
		}

		// public hb_var_int_t var
		private Int32 var;

		public readonly bool Equals (GlyphPosition obj) =>
#pragma warning disable CS8909
			x_advance == obj.x_advance && y_advance == obj.y_advance && x_offset == obj.x_offset && y_offset == obj.y_offset && var == obj.var;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is GlyphPosition f && Equals (f);

		public static bool operator == (GlyphPosition left, GlyphPosition right) =>
			left.Equals (right);

		public static bool operator != (GlyphPosition left, GlyphPosition right) =>
			!left.Equals (right);

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
