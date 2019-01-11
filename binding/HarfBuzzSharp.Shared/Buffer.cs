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

		public void AddUtf8(string utf8text)
		{
			var bytes = Encoding.UTF8.GetBytes(utf8text);
			AddUtf8(bytes);
		}

		public unsafe void AddUtf8(byte[] bytes, uint itemOffset = 0, int itemLength = -1)
		{
			fixed (byte* b = bytes)
			{
				AddUtf8((IntPtr)b, bytes.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf8(IntPtr buffer, int textLength, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf8(Handle, buffer, textLength, itemOffset, itemLength);
		}

		public void AddUtf16(string utf16text)
		{
			var bytes = Encoding.Unicode.GetBytes(utf16text);
			AddUtf16(bytes, 0, bytes.Length / 2);
		}

		public unsafe void AddUtf16(byte[] bytes, uint itemOffset = 0, int itemLength = -1)
		{
			fixed (byte* b = bytes)
			{
				AddUtf16((IntPtr)b, bytes.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf16(IntPtr buffer, int textLength, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf16(Handle, buffer, textLength, itemOffset, itemLength);
		}

		public void AddUtf32(string utf32text)
		{
			var bytes = Encoding.UTF32.GetBytes(utf32text);
			AddUtf32(bytes, 0, bytes.Length / 4);
		}

		public unsafe void AddUtf32(byte[] bytes, uint itemOffset = 0, int itemLength = -1)
		{
			fixed (byte* b = bytes)
			{
				AddUtf32((IntPtr)b, bytes.Length, itemOffset, itemLength);
			}
		}

		public void AddUtf32(IntPtr buffer, int textLength, uint itemOffset = 0, int itemLength = -1)
		{
			HarfBuzzApi.hb_buffer_add_utf32(Handle, buffer, textLength, itemOffset, itemLength);
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
