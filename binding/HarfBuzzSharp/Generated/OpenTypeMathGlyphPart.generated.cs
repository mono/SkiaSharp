using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{

	// hb_ot_math_glyph_part_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeMathGlyphPart : IEquatable<OpenTypeMathGlyphPart> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public hb_position_t start_connector_length
		private Int32 start_connector_length;
		public Int32 StartConnectorLength {
			readonly get => start_connector_length;
			set => start_connector_length = value;
		}

		// public hb_position_t end_connector_length
		private Int32 end_connector_length;
		public Int32 EndConnectorLength {
			readonly get => end_connector_length;
			set => end_connector_length = value;
		}

		// public hb_position_t full_advance
		private Int32 full_advance;
		public Int32 FullAdvance {
			readonly get => full_advance;
			set => full_advance = value;
		}

		// public hb_ot_math_glyph_part_flags_t flags
		private OpenTypeMathGlyphPartFlags flags;
		public OpenTypeMathGlyphPartFlags Flags {
			readonly get => flags;
			set => flags = value;
		}

		public readonly bool Equals (OpenTypeMathGlyphPart obj) =>
#pragma warning disable CS8909
			glyph == obj.glyph && start_connector_length == obj.start_connector_length && end_connector_length == obj.end_connector_length && full_advance == obj.full_advance && flags == obj.flags;
#pragma warning restore CS8909

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeMathGlyphPart f && Equals (f);

		public static bool operator == (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right) =>
			!left.Equals (right);

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
