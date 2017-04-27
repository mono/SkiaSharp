using System;
using System.Collections.Generic;
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

		public void AddUtf8(string utf8text)
		{
			var bytes = Encoding.UTF8.GetBytes(utf8text);
			AddUtf8(bytes);
		}

		public void AddUtf8(byte[] bytes)
		{
			HarfBuzzApi.hb_buffer_add_utf8(Handle, bytes, bytes.Length, 0, bytes.Length);
		}

		public void GuessSegmentProperties() => HarfBuzzApi.hb_buffer_guess_segment_properties(Handle);

		public void ClearContents() => HarfBuzzApi.hb_buffer_clear_contents(Handle);

		public GlyphInfo[] GlyphInfos
		{
			get
			{
				uint length;
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_infos(Handle, out length);
				return PtrToStructureArray<GlyphInfo>(infoPtrs, (int)length);
			}
		}

		public GlyphPosition[] GlyphPositions
		{
			get
			{
				uint length;
				var infoPtrs = HarfBuzzApi.hb_buffer_get_glyph_positions(Handle, out length);
				return PtrToStructureArray<GlyphPosition>(infoPtrs, (int)length);
			}
		}
	}
}
