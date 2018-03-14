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
	internal class HarfBuzzApi
	{
#if __TVOS__ && __UNIFIED__
		private const string HARFBUZZ = "__Internal";
#elif __WATCHOS__ && __UNIFIED__
		private const string HARFBUZZ = "__Internal";
#elif __IOS__ && __UNIFIED__
		private const string HARFBUZZ = "__Internal";
#elif __ANDROID__
		private const string HARFBUZZ = "libharfbuzz.so";
#elif __MACOS__
		private const string HARFBUZZ = "libharfbuzz.dylib";
#elif DESKTOP
		private const string HARFBUZZ = "harfbuzz";
#elif WINDOWS_UWP
		private const string HARFBUZZ = "harfbuzz.dll";
#elif NET_STANDARD
		private const string HARFBUZZ = "harfbuzz";
#else
		private const string HARFBUZZ = "harfbuzz";
#endif

		// hb_blob_t

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_blob_t hb_blob_create(IntPtr data, uint length, MemoryMode mode, IntPtr user_data, IntPtr destroy);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_blob_destroy(hb_blob_t blob);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_blob_make_immutable(hb_blob_t blob);

		// hb_face_t

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_face_t hb_face_create(hb_blob_t blob, uint index);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_destroy(hb_face_t face);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_set_index(hb_face_t face, uint index);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static uint hb_face_get_index(hb_face_t face);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_set_upem(hb_face_t face, uint upem);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static uint hb_face_get_upem(hb_face_t face);

		// hb_font_t

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_font_t hb_font_create(hb_face_t face);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_set_scale(hb_font_t font, int x_scale, int y_scale);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_scale(hb_font_t font, out int x_scale, out int y_scale);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_destroy(hb_font_t font);

		// hb_font_t (OT)

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_ot_font_set_funcs(hb_font_t font);

		// hb_buffer_t

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_buffer_t hb_buffer_create();

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_destroy(hb_buffer_t buffer);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf8(hb_buffer_t buffer, byte[] text, int text_length, uint item_offset, int item_length);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf16(hb_buffer_t buffer, byte[] text, int text_length, uint item_offset, int item_length);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf32(hb_buffer_t buffer, byte[] text, int text_length, uint item_offset, int item_length);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_guess_segment_properties(hb_buffer_t buffer);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_length(hb_buffer_t buffer, uint length);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static uint hb_buffer_get_length(hb_buffer_t buffer);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_clear_contents(hb_buffer_t buffer);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr hb_buffer_get_glyph_infos(hb_buffer_t buffer, out uint length);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr hb_buffer_get_glyph_positions(hb_buffer_t buffer, out uint length);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_script(hb_buffer_t buffer, hb_tag_t script); // hb_script_t

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_direction(hb_buffer_t buffer, Direction direction);

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static Direction hb_buffer_get_direction(hb_buffer_t buffer);

		// hb_shape

		[DllImport(HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_shape(hb_font_t font, hb_buffer_t buffer, IntPtr features, uint num_features);
	}
}
