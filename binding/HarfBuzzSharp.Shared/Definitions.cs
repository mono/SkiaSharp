using System;
using System.Text;
using System.Runtime.InteropServices;

using hb_codepoint_t = System.UInt32;
using hb_mask_t = System.UInt32;
using hb_position_t = System.Int32;
using hb_tag_t = System.UInt32;
using hb_var_int_t = System.Int32;

namespace HarfBuzzSharp
{
	public enum MemoryMode
	{
		Duplicate,
		ReadOnly,
		Writeable,
		ReadOnlyMayMakeWriteable
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct Feature
	{
		private Tag tag;
		private uint val;
		private uint start;
		private uint end;

		public Feature (Tag tag, uint value, uint start = 0, uint end = uint.MaxValue)
		{
			this.tag = tag;
			this.val = value;
			this.start = start;
			this.end = end;
		}

		public Tag Tag {
			get { return tag; }
			set { tag = value; }
		}

		public uint Value {
			get { return val; }
			set { val = value; }
		}

		public uint Start {
			get { return start; }
			set { start = value; }
		}

		public uint End {
			get { return end; }
			set { end = value; }
		}

		public override string ToString ()
		{
			const int allocatedSize = 128;
			var buffer = new StringBuilder (allocatedSize);
			HarfBuzzApi.hb_feature_to_string (ref this, buffer, allocatedSize);
			return buffer.ToString ();
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GlyphInfo
	{
		private hb_codepoint_t codepoint;
		private hb_mask_t mask;
		private uint cluster;

		private hb_var_int_t var1;
		private hb_var_int_t var2;

		public hb_codepoint_t Codepoint {
			get { return codepoint; }
			set { codepoint = value; }
		}

		public hb_mask_t Mask {
			get { return mask; }
			set { mask = value; }
		}

		public uint Cluster {
			get { return cluster; }
			set { cluster = value; }
		}

		public GlyphFlags GlyphFlags
		{
			get {
				return HarfBuzzApi.hb_glyph_info_get_glyph_flags (ref this);
			}
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GlyphPosition
	{
		private hb_position_t x_advance;
		private hb_position_t y_advance;
		private hb_position_t x_offset;
		private hb_position_t y_offset;

		/*< private >*/
		private hb_var_int_t var;

		public hb_position_t XAdvance {
			get { return x_advance; }
			set { x_advance = value; }
		}

		public hb_position_t YAdvance {
			get { return y_advance; }
			set { y_advance = value; }
		}

		public hb_position_t XOffset {
			get { return x_offset; }
			set { x_offset = value; }
		}

		public hb_position_t YOffset {
			get { return y_offset; }
			set { y_offset = value; }
		}
	}

	public enum Direction
	{
		Invalid = 0,

		LeftToRight = 4,
		RightToLeft,
		TopToBottom,
		BottomToTop
	}

	public enum ClusterLevel
	{
		MonotoneGraphemes = 0,
		MonotoneCharacters = 1,
		Characters = 2,
		Default = MonotoneGraphemes
	}

	[Flags]
	public enum Flags : uint
	{
		Default = 0x00000000u,
		Bot = 0x00000001u,
		Eot = 0x00000002u,
		PreserveDefaultIgnorables = 0x00000004u,
		RemoveDefaultIgnorables = 0x00000008u
	}

	public enum ContentType
	{
		Invalid = 0,
		Unicode,
		Glyphs
	}

	[Flags]
	public enum GlyphFlags
	{
		UnsafeToBreak = 0x00000001,
		Defined = 0x00000001
	}

	[Flags]
	public enum SerializeFlag : uint
	{
		Default = 0x00000000u,
		NoClusters = 0x00000001u,
		NoPositions = 0x00000002u,
		NoGlyphNames = 0x00000004u,
		GlyphExtents = 0x00000008u,
		GlyphFlags = 0x00000010u,
		NoAdvances = 0x00000020u
	}

	public static class SerializeFormat
	{
		public static readonly Tag Text = new Tag ('T', 'E', 'X', 'T');

		public static readonly Tag Json = new Tag ('J', 'S', 'O', 'N');

		public static readonly Tag Invalid = Tag.None;
	}

	public struct GlyphExtents
	{
		hb_position_t x_bearing; /* left side of glyph from origin. */
		hb_position_t y_bearing; /* top side of glyph from origin. */
		hb_position_t width; /* distance from left to right side. */
		hb_position_t height; /* distance from top to bottom side. */

		public int XBearing {
			get => x_bearing;
			set => x_bearing = value;
		}
		public int YBearing {
			get => y_bearing;
			set => y_bearing = value;
		}
		public int Width {
			get => width;
			set => width = value;
		}
		public int Height {
			get => height;
			set => height = value;
		}
	}

	public struct FontExtents
	{
		private hb_position_t ascender; /* typographic ascender. */

		private hb_position_t descender; /* typographic descender. */

		private hb_position_t line_gap; /* suggested line spacing gap. */

		/*< private >*/

		private hb_position_t reserved9;

		private hb_position_t reserved8;

		private hb_position_t reserved7;

		private hb_position_t reserved6;

		private hb_position_t reserved5;

		private hb_position_t reserved4;

		private hb_position_t reserved3;

		private hb_position_t reserved2;

		private hb_position_t reserved1;

		public int Ascender {
			get => ascender;
			set => ascender = value;
		}

		public int Descender {
			get => descender;
			set => descender = value;
		}

		public int LineGap {
			get => line_gap;
			set => line_gap = value;
		}
	}
}
