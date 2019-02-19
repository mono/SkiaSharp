using System;
using System.Runtime.InteropServices;
using System.Text;
using hb_blob_t = System.IntPtr;
using hb_buffer_t = System.IntPtr;
using hb_codepoint_t = System.Int32;
using hb_face_t = System.IntPtr;
using hb_font_t = System.IntPtr;
using hb_position_t = System.Int32;
using hb_script_t = System.UInt32;

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
		private const string HARFBUZZ = "libHarfBuzzSharp.so";
#elif __MACOS__
		private const string HARFBUZZ = "libHarfBuzzSharp.dylib";
#elif __DESKTOP__
		private const string HARFBUZZ = "libHarfBuzzSharp";
#elif WINDOWS_UWP
		private const string HARFBUZZ = "libHarfBuzzSharp.dll";
#elif NET_STANDARD
		private const string HARFBUZZ = "libHarfBuzzSharp";
#else
		private const string HARFBUZZ = "libHarfBuzzSharp";
#endif

		// hb_blob_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_blob_t hb_blob_create (IntPtr data, int length, MemoryMode mode, IntPtr user_data, IntPtr destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_blob_destroy (hb_blob_t blob);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_blob_make_immutable (hb_blob_t blob);

		// hb_face_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_face_t hb_face_create (hb_blob_t blob, int index);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_destroy (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_set_index (hb_face_t face, int index);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_face_get_index (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_set_upem (hb_face_t face, int upem);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_face_get_upem (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_set_glyph_count (hb_face_t face, int glyph_count);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_face_get_glyph_count (hb_face_t face);

		// hb_font_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_font_t hb_font_create (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_set_scale (hb_font_t font, int x_scale, int y_scale);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_scale (hb_font_t font, out int x_scale, out int y_scale);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_destroy (hb_font_t font);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool hb_font_get_h_extents (hb_font_t font, out FontExtents extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool hb_font_get_v_extents (hb_font_t font, out FontExtents extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_font_get_glyph_h_advance (hb_font_t font, hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_font_get_glyph_v_advance (hb_font_t font, hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool hb_font_get_glyph_extents (hb_font_t font, hb_codepoint_t glyph, out GlyphExtents extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool hb_font_get_glyph_name (hb_font_t font, hb_codepoint_t glyph, StringBuilder name, uint size);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool hb_font_get_glyph (hb_font_t font, hb_codepoint_t unicode, hb_codepoint_t variation_selector, out hb_codepoint_t glyph);

		// hb_font_t (OT)

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_ot_font_set_funcs (hb_font_t font);

		// hb_buffer_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_buffer_t hb_buffer_create ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_destroy (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_reset (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add (hb_buffer_t buffer, int codepoint, int cluster);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf8 (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf16 (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf32 (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_guess_segment_properties (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_length (hb_buffer_t buffer, int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_buffer_get_length (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_clear_contents (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr hb_buffer_get_glyph_infos (hb_buffer_t buffer, out int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr hb_buffer_get_glyph_positions (hb_buffer_t buffer, out int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_script (hb_buffer_t buffer, hb_script_t script);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_script_t hb_buffer_get_script (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_direction (hb_buffer_t buffer, Direction direction);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static Direction hb_buffer_get_direction (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_language (IntPtr buffer, IntPtr language);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_buffer_get_language (IntPtr buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_language_from_string (byte[] str, int len);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_language_to_string (IntPtr language);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_content_type (IntPtr buffer, ContentType content_type);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern ContentType hb_buffer_get_content_type (IntPtr buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_replacement_codepoint (IntPtr buffer, hb_codepoint_t replacement);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_codepoint_t hb_buffer_get_replacement_codepoint (IntPtr buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_flags (IntPtr buffer, Flags flags);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern Flags hb_buffer_get_flags (IntPtr buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_cluster_level (IntPtr buffer, ClusterLevel cluster_level);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern ClusterLevel hb_buffer_get_cluster_level (IntPtr buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hb_buffer_serialize_glyphs (
			IntPtr buffer,
			int start,
			int end,
			IntPtr buf,
			uint buf_size,
			out int buf_consumed,
			IntPtr font,
			SerializeFormat format,
			SerializeFlag flags);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool hb_buffer_deserialize_glyphs (
			IntPtr buffer,
			byte[] buf,
			int buf_len,
			out IntPtr end_ptr,
			IntPtr font,
			SerializeFormat format);

		// hb_shape

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_shape (hb_font_t font, hb_buffer_t buffer, IntPtr features, int num_features);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool hb_shape_full (hb_font_t font, hb_buffer_t buffer, IntPtr features, int num_features, IntPtr shaper_list);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr hb_shape_list_shapers();

		//hb_language

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_language_get_default ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern GlyphFlags hb_glyph_info_get_glyph_flags (ref GlyphInfo info);

		//hb_feature

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_feature_to_string (ref Feature feature,
														[MarshalAs (UnmanagedType.LPStr)] StringBuilder buf, uint size);

		//hb_script

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern Direction hb_script_get_horizontal_direction (hb_script_t script);
	}
}
