using System;
using System.Text;

namespace HarfBuzzSharp
{
	public class Buffer : NativeObject
	{
		internal Buffer(IntPtr handle)
			: base(handle)
		{
		}

		public Buffer()
			: this(HarfBuzzApi.hb_buffer_create())
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero)
			{
				HarfBuzzApi.hb_buffer_destroy(Handle);
			}

			base.Dispose(disposing);
		}

		public Direction Direction
		{
			set { HarfBuzzApi.hb_buffer_set_direction(Handle, value); }
			get { return HarfBuzzApi.hb_buffer_get_direction(Handle); }
		}

		//public Script Script
		//{
		//	set { HarfBuzzApi.hb_buffer_set_script(Handle, value); }
		//	get { return HarfBuzzApi.hb_buffer_get_script(Handle); }
		//}

		public uint Length
		{
			set { HarfBuzzApi.hb_buffer_set_length(Handle, value); }
			get { return HarfBuzzApi.hb_buffer_get_length(Handle); }
		}

		public void AddUtf8(string text)
		{
			var bytes = Encoding.UTF8.GetBytes(text);
			AddUtf8(bytes);
		}

		public void AddUtf8(byte[] text, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf8(Handle, text, text.Length, itemOffset, itemLength);
		}

		public void AddUtf16(string text)
		{
			var bytes = Encoding.Unicode.GetBytes(text);
			AddUtf16(bytes);
		}

		public void AddUtf16(byte[] text, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf16(Handle, text, text.Length, itemOffset, itemLength);
		}

		public void AddUtf32(string text)
		{
			var bytes = Encoding.UTF32.GetBytes(text);
			AddUtf32(bytes);
		}

		public void AddUtf32(byte[] text, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf32(Handle, text, text.Length, itemOffset, itemLength);
		}

		public void GuessSegmentProperties() => HarfBuzzApi.hb_buffer_guess_segment_properties(Handle);

		public void ClearContents() => HarfBuzzApi.hb_buffer_clear_contents(Handle);

		public GlyphInfo[] GlyphInfos
		{
			get
			{
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos(Handle, out var length);
				return PtrToStructureArray<GlyphInfo>(infoPtrs, (int)length);
			}
		}

		public GlyphPosition[] GlyphPositions
		{
			get
			{
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions(Handle, out var length);
				return PtrToStructureArray<GlyphPosition>(infoPtrs, (int)length);
			}
		}
	}
}
