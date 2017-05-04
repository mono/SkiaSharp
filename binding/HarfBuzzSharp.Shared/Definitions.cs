using System;
using System.Runtime.InteropServices;

using hb_blob_t = System.IntPtr;
using hb_face_t = System.IntPtr;
using hb_font_t = System.IntPtr;
using hb_buffer_t = System.IntPtr;

using hb_tag_t = System.UInt32;
using hb_codepoint_t = System.UInt32;
using hb_mask_t = System.UInt32;
using hb_position_t = System.Int32;

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

	[StructLayout(LayoutKind.Sequential)]
	public struct Feature
	{
		private hb_tag_t tag;
		private uint val;
		private uint start;
		private uint end;

		public hb_tag_t Tag
		{
			get { return tag; }
			set { tag = value; }
		}

		public uint Value
		{
			get { return val; }
			set { val = value; }
		}

		public uint Start
		{
			get { return start; }
			set { start = value; }
		}

		public uint End
		{
			get { return end; }
			set { end = value; }
		}

	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GlyphInfo
	{
		private hb_codepoint_t codepoint;
		private hb_mask_t mask;
		private uint cluster;

		/*< private >*/
		private hb_var_int_t var1;
		private hb_var_int_t var2;

		public hb_codepoint_t Codepoint
		{
			get { return codepoint; }
			set { codepoint = value; }
		}

		public hb_mask_t Mask
		{
			get { return mask; }
			set { mask = value; }
		}

		public uint Cluster
		{
			get { return cluster; }
			set { cluster = value; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GlyphPosition
	{
		private hb_position_t x_advance;
		private hb_position_t y_advance;
		private hb_position_t x_offset;
		private hb_position_t y_offset;

		/*< private >*/
		private hb_var_int_t var;

		public hb_position_t XAdvance
		{
			get { return x_advance; }
			set { x_advance = value; }
		}

		public hb_position_t YAdvance
		{
			get { return y_advance; }
			set { y_advance = value; }
		}

		public hb_position_t XOffset
		{
			get { return x_offset; }
			set { x_offset = value; }
		}

		public hb_position_t YOffset
		{
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
}
