﻿#pragma warning disable IDE1006 // Naming Styles
using System;
using System.Runtime.InteropServices;
using System.Text;
using hb_blob_t = System.IntPtr;
using hb_bool_t = System.Boolean;
using hb_buffer_t = System.IntPtr;
using hb_codepoint_t = System.UInt32;
using hb_destroy_func_t = HarfBuzzSharp.ReleaseDelegateProxyDelegate;
using hb_direction_t = HarfBuzzSharp.Direction;
using hb_face_t = System.IntPtr;
using hb_font_extents_t = HarfBuzzSharp.FontExtents;
using hb_font_funcs_t = System.IntPtr;
using hb_font_t = System.IntPtr;
using hb_position_t = System.Int32;
using hb_script_t = System.UInt32;
using hb_unicode_funcs_t = System.IntPtr;

namespace HarfBuzzSharp
{
	internal unsafe class HarfBuzzApi
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
		public extern static hb_blob_t hb_blob_create (IntPtr data, int length, MemoryMode mode, IntPtr user_data, ReleaseDelegateProxyDelegate destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_blob_destroy (hb_blob_t blob);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_blob_make_immutable (hb_blob_t blob);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_blob_is_immutable (hb_blob_t blob);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_blob_get_length (hb_blob_t blob);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static byte* hb_blob_get_data (hb_blob_t blob, out int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_blob_t hb_blob_create_from_file ([MarshalAs (UnmanagedType.LPStr)] string file_name);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_blob_t hb_blob_get_empty ();

		// hb_face_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_face_count (hb_blob_t blob);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_face_t hb_face_create (hb_blob_t blob, int index);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_face_t hb_face_create_for_tables (GetTableDelegateProxyDelegate reference_table_func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_face_t hb_face_get_empty ();
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_face_make_immutable (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_face_is_immutable (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_blob_t hb_face_reference_table (hb_face_t face, Tag tag);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_face_get_table_tags (hb_face_t face, int start_offset, ref int table_count, IntPtr table_tags);

		// hb_font_funcs_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_font_funcs_t hb_font_funcs_create ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_font_funcs_t hb_font_funcs_get_empty ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_destroy (hb_font_funcs_t ffuncs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_make_immutable (hb_font_funcs_t ffuncs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_funcs_is_immutable (hb_font_funcs_t ffuncs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_font_h_extents_func (hb_font_funcs_t ffuncs, FontExtentsProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_font_v_extents_func (hb_font_funcs_t ffuncs, FontExtentsProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_nominal_glyph_func (hb_font_funcs_t ffuncs, NominalGlyphProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_nominal_glyphs_func (hb_font_funcs_t ffuncs, NominalGlyphsProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_variation_glyph_func (hb_font_funcs_t ffuncs, VariationGlyphProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_h_advance_func (hb_font_funcs_t ffuncs, GlyphAdvanceProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_v_advance_func (hb_font_funcs_t ffuncs, GlyphAdvanceProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_h_advances_func (hb_font_funcs_t ffuncs, GlyphAdvancesProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_v_advances_func (hb_font_funcs_t ffuncs, GlyphAdvancesProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_h_origin_func (hb_font_funcs_t ffuncs, GlyphOriginProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_v_origin_func (hb_font_funcs_t ffuncs, GlyphOriginProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_h_kerning_func (hb_font_funcs_t ffuncs, GlyphKerningProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_extents_func (hb_font_funcs_t ffuncs, GlyphExtentsProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_contour_point_func (hb_font_funcs_t ffuncs, GlyphContourPointProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_name_func (hb_font_funcs_t ffuncs, GlyphNameProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_funcs_set_glyph_from_name_func (hb_font_funcs_t ffuncs, GlyphFromNameProxyDelegate func, IntPtr user_data, hb_destroy_func_t destroy);

		// hb_font_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_font_t hb_font_create (hb_face_t face);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_font_t hb_font_create_sub_font (hb_font_t parent);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_destroy (hb_font_t font);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_set_funcs (hb_font_t font, hb_font_funcs_t klass, IntPtr font_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_set_scale (hb_font_t font, int x_scale, int y_scale);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_scale (hb_font_t font, out int x_scale, out int y_scale);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_h_extents (hb_font_t font, out hb_font_extents_t extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_v_extents (hb_font_t font, out hb_font_extents_t extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_nominal_glyph (hb_font_t font, hb_codepoint_t unicode, out hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_variation_glyph (hb_font_t font, hb_codepoint_t unicode, hb_codepoint_t variation_selector, out hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_font_get_glyph_h_advance (hb_font_t font, hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_font_get_glyph_v_advance (hb_font_t font, hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_glyph_h_advances (hb_font_t font, int count, IntPtr first_glyph, uint glyph_stride, IntPtr first_advance, uint advance_stride);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_glyph_v_advances (hb_font_t font, int count, IntPtr first_glyph, uint glyph_stride, IntPtr first_advance, uint advance_stride);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_h_origin (hb_font_t font, hb_codepoint_t glyph, out hb_position_t x, out hb_position_t y);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_v_origin (hb_font_t font, hb_codepoint_t glyph, out hb_position_t x, out hb_position_t y);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_font_get_glyph_h_kerning (hb_font_t font, hb_codepoint_t left_glyph, hb_codepoint_t right_glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_extents (hb_font_t font, hb_codepoint_t glyph, out GlyphExtents extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_contour_point (hb_font_t font, hb_codepoint_t glyph, uint point_index, out hb_position_t x, out hb_position_t y);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_name (hb_font_t font, hb_codepoint_t glyph, byte* nameBuffer, int size);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_from_name (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)]string name, int len, out hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph (hb_font_t font, hb_codepoint_t unicode, hb_codepoint_t variation_selector, out hb_codepoint_t glyph);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_extents_for_direction (hb_font_t font, hb_direction_t direction, out hb_font_extents_t extents);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_glyph_advance_for_direction (hb_font_t font, hb_codepoint_t glyph, hb_direction_t direction, out hb_position_t x, out hb_position_t y);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_get_glyph_advances_for_direction (hb_font_t font, hb_direction_t direction, int count, IntPtr first_glyph, uint glyph_stride, IntPtr first_advance, uint advance_stride);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_get_glyph_contour_point_for_origin (hb_font_t font, hb_codepoint_t glyph, uint point_index, hb_direction_t direction, out hb_position_t x, out hb_position_t y);
		/* Generates gidDDD if glyph has no name. */
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_font_glyph_to_string (hb_font_t font, hb_codepoint_t glyph, byte* s, int size);
		/* Parses gidDDD and uniUUUU strings automatically. */
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_font_glyph_from_string (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)]string s, int len, /* -1 means nul-terminated */ out hb_codepoint_t glyph);

		// hb_font_t (OT)

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_ot_font_set_funcs (hb_font_t font);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_ot_metrics_get_position (hb_font_t font, OpenTypeMetricsTag metrics_tag, out hb_position_t position     /* OUT.  May be NULL. */);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static float hb_ot_metrics_get_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_ot_metrics_get_x_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_position_t hb_ot_metrics_get_y_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);

		// hb_buffer_t

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static hb_buffer_t hb_buffer_create ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_destroy (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_reset (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_append (hb_buffer_t buffer, hb_buffer_t source, int start, int end);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_add (hb_buffer_t buffer, hb_codepoint_t codepoint, hb_codepoint_t cluster);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf8 (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf16 (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_utf32 (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_add_codepoints (hb_buffer_t buffer, IntPtr text, int text_length, int item_offset, int item_length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_guess_segment_properties (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_length (hb_buffer_t buffer, int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static int hb_buffer_get_length (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_clear_contents (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void* hb_buffer_get_glyph_infos (hb_buffer_t buffer, out int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void* hb_buffer_get_glyph_positions (hb_buffer_t buffer, out int length);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_script (hb_buffer_t buffer, hb_script_t script);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_script_t hb_buffer_get_script (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static void hb_buffer_set_direction (hb_buffer_t buffer, hb_direction_t direction);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static Direction hb_buffer_get_direction (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_language (hb_buffer_t buffer, IntPtr language);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_buffer_get_language (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_content_type (hb_buffer_t buffer, ContentType content_type);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern ContentType hb_buffer_get_content_type (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_replacement_codepoint (hb_buffer_t buffer, hb_codepoint_t replacement);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_codepoint_t hb_buffer_get_replacement_codepoint (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_invisible_glyph (hb_buffer_t buffer, hb_codepoint_t invisible);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_codepoint_t hb_buffer_get_invisible_glyph (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_flags (hb_buffer_t buffer, BufferFlags flags);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern BufferFlags hb_buffer_get_flags (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_cluster_level (hb_buffer_t buffer, ClusterLevel cluster_level);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern ClusterLevel hb_buffer_get_cluster_level (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_normalize_glyphs (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_reverse (hb_buffer_t buffer);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_reverse_range (hb_buffer_t buffer, int start, int end);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_reverse_clusters (hb_buffer_t buffer);

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern int hb_buffer_serialize_glyphs (hb_buffer_t buffer, int start, int end, IntPtr buf, int buf_size, out int buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags);

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public static extern hb_bool_t hb_buffer_deserialize_glyphs (IntPtr buffer, [MarshalAs (UnmanagedType.LPStr)] string buf, int buf_len, out IntPtr end_ptr, hb_font_t font, SerializeFormat format);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_buffer_set_unicode_funcs (hb_buffer_t buffer, hb_unicode_funcs_t unicode_funcs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_unicode_funcs_t hb_buffer_get_unicode_funcs (hb_buffer_t buffer);

		// hb_shape

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public extern static hb_bool_t hb_shape_full (hb_font_t font, hb_buffer_t buffer, IntPtr features, int num_features, IntPtr shaper_list);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr hb_shape_list_shapers ();

		// hb_language

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] string str, int len);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_language_to_string (IntPtr language);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr hb_language_get_default ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern GlyphFlags hb_glyph_info_get_glyph_flags (ref GlyphInfo info);

		// hb_feature

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_feature_to_string (ref Feature feature, [MarshalAs (UnmanagedType.LPStr)] StringBuilder buf, uint size);

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public static extern hb_bool_t hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] string str, int len, out Feature feature);

		// hb_script

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_direction_t hb_script_get_horizontal_direction (hb_script_t script);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_script_t hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] string str, int len);

		// hb_unicode

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_unicode_funcs_t hb_unicode_funcs_get_default ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_unicode_funcs_t hb_unicode_funcs_get_empty ();
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_unicode_funcs_t hb_unicode_funcs_create (hb_unicode_funcs_t parent);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_destroy (hb_unicode_funcs_t ufuncs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_make_immutable (hb_unicode_funcs_t ufuncs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public static extern hb_bool_t hb_unicode_funcs_is_immutable (hb_unicode_funcs_t ufuncs);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern UnicodeCombiningClass hb_unicode_combining_class (hb_unicode_funcs_t ufuncs, hb_codepoint_t unicode);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern UnicodeGeneralCategory hb_unicode_general_category (hb_unicode_funcs_t ufuncs, hb_codepoint_t unicode);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_codepoint_t hb_unicode_mirroring (hb_unicode_funcs_t ufuncs, hb_codepoint_t unicode);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern hb_script_t hb_unicode_script (hb_unicode_funcs_t ufuncs, hb_codepoint_t unicode);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public static extern hb_bool_t hb_unicode_compose (hb_unicode_funcs_t ufuncs, hb_codepoint_t a, hb_codepoint_t b, out hb_codepoint_t ab);

		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		public static extern hb_bool_t hb_unicode_decompose (hb_unicode_funcs_t ufuncs, hb_codepoint_t ab, out hb_codepoint_t a, out hb_codepoint_t b);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_set_combining_class_func (hb_unicode_funcs_t ufuncs, hb_unicode_combining_class_func_t func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_set_general_category_func (hb_unicode_funcs_t ufuncs, hb_unicode_general_category_func_t func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_set_mirroring_func (hb_unicode_funcs_t ufuncs, hb_unicode_mirroring_func_t func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_set_script_func (hb_unicode_funcs_t ufuncs, hb_unicode_script_func_t func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_set_compose_func (hb_unicode_funcs_t ufuncs, hb_unicode_compose_func_t func, IntPtr user_data, hb_destroy_func_t destroy);
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		public static extern void hb_unicode_funcs_set_decompose_func (hb_unicode_funcs_t ufuncs, hb_unicode_decompose_func_t func, IntPtr user_data, hb_destroy_func_t destroy);
	}
}
#pragma warning restore IDE1006 // Naming Styles
