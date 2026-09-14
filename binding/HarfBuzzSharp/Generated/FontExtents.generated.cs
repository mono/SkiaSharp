using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_font_extents_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct FontExtents : IEquatable<FontExtents> {
		// public hb_position_t ascender
		private Int32 ascender;
		public Int32 Ascender {
			readonly get => ascender;
			set => ascender = value;
		}

		// public hb_position_t descender
		private Int32 descender;
		public Int32 Descender {
			readonly get => descender;
			set => descender = value;
		}

		// public hb_position_t line_gap
		private Int32 line_gap;
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

		public readonly bool Equals (FontExtents obj) =>
#pragma warning disable CS8909
			ascender == obj.ascender && descender == obj.descender && line_gap == obj.line_gap && reserved9 == obj.reserved9 && reserved8 == obj.reserved8 && reserved7 == obj.reserved7 && reserved6 == obj.reserved6 && reserved5 == obj.reserved5 && reserved4 == obj.reserved4 && reserved3 == obj.reserved3 && reserved2 == obj.reserved2 && reserved1 == obj.reserved1;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is FontExtents f && Equals (f);

		public static bool operator == (FontExtents left, FontExtents right) =>
			left.Equals (right);

		public static bool operator != (FontExtents left, FontExtents right) =>
			!left.Equals (right);

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
