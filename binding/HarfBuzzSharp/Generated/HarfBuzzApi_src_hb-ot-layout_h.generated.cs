using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-layout.h

		// extern void hb_ot_layout_collect_features(hb_face_t* face, hb_tag_t table_tag, const hb_tag_t* scripts, const hb_tag_t* languages, const hb_tag_t* features, hb_set_t* feature_indexes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes);
		}
		private static Delegates.hb_ot_layout_collect_features hb_ot_layout_collect_features_delegate;
		internal static void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes) =>
			(hb_ot_layout_collect_features_delegate ??= GetSymbol<Delegates.hb_ot_layout_collect_features> ("hb_ot_layout_collect_features")).Invoke (face, table_tag, scripts, languages, features, feature_indexes);
		#endif

		// extern void hb_ot_layout_collect_features_map(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_index, hb_map_t* feature_map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_collect_features_map (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, hb_map_t feature_map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_collect_features_map (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, hb_map_t feature_map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_collect_features_map (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, hb_map_t feature_map);
		}
		private static Delegates.hb_ot_layout_collect_features_map hb_ot_layout_collect_features_map_delegate;
		internal static void hb_ot_layout_collect_features_map (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, hb_map_t feature_map) =>
			(hb_ot_layout_collect_features_map_delegate ??= GetSymbol<Delegates.hb_ot_layout_collect_features_map> ("hb_ot_layout_collect_features_map")).Invoke (face, table_tag, script_index, language_index, feature_map);
		#endif

		// extern void hb_ot_layout_collect_lookups(hb_face_t* face, hb_tag_t table_tag, const hb_tag_t* scripts, const hb_tag_t* languages, const hb_tag_t* features, hb_set_t* lookup_indexes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_collect_lookups (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t lookup_indexes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_collect_lookups (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t lookup_indexes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_collect_lookups (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t lookup_indexes);
		}
		private static Delegates.hb_ot_layout_collect_lookups hb_ot_layout_collect_lookups_delegate;
		internal static void hb_ot_layout_collect_lookups (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t lookup_indexes) =>
			(hb_ot_layout_collect_lookups_delegate ??= GetSymbol<Delegates.hb_ot_layout_collect_lookups> ("hb_ot_layout_collect_lookups")).Invoke (face, table_tag, scripts, languages, features, lookup_indexes);
		#endif

		// extern unsigned int hb_ot_layout_feature_get_characters(hb_face_t* face, hb_tag_t table_tag, unsigned int feature_index, unsigned int start_offset, unsigned int* char_count, hb_codepoint_t* characters)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_feature_get_characters (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* char_count, UInt32* characters);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_feature_get_characters (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* char_count, UInt32* characters);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_feature_get_characters (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* char_count, UInt32* characters);
		}
		private static Delegates.hb_ot_layout_feature_get_characters hb_ot_layout_feature_get_characters_delegate;
		internal static UInt32 hb_ot_layout_feature_get_characters (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* char_count, UInt32* characters) =>
			(hb_ot_layout_feature_get_characters_delegate ??= GetSymbol<Delegates.hb_ot_layout_feature_get_characters> ("hb_ot_layout_feature_get_characters")).Invoke (face, table_tag, feature_index, start_offset, char_count, characters);
		#endif

		// extern unsigned int hb_ot_layout_feature_get_lookups(hb_face_t* face, hb_tag_t table_tag, unsigned int feature_index, unsigned int start_offset, unsigned int* lookup_count, unsigned int* lookup_indexes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_feature_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_feature_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_feature_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
		}
		private static Delegates.hb_ot_layout_feature_get_lookups hb_ot_layout_feature_get_lookups_delegate;
		internal static UInt32 hb_ot_layout_feature_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes) =>
			(hb_ot_layout_feature_get_lookups_delegate ??= GetSymbol<Delegates.hb_ot_layout_feature_get_lookups> ("hb_ot_layout_feature_get_lookups")).Invoke (face, table_tag, feature_index, start_offset, lookup_count, lookup_indexes);
		#endif

		// extern hb_bool_t hb_ot_layout_feature_get_name_ids(hb_face_t* face, hb_tag_t table_tag, unsigned int feature_index, hb_ot_name_id_t* label_id, hb_ot_name_id_t* tooltip_id, hb_ot_name_id_t* sample_id, unsigned int* num_named_parameters, hb_ot_name_id_t* first_param_id)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_feature_get_name_ids (hb_face_t face, UInt32 table_tag, UInt32 feature_index, OpenTypeNameId* label_id, OpenTypeNameId* tooltip_id, OpenTypeNameId* sample_id, UInt32* num_named_parameters, OpenTypeNameId* first_param_id);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_feature_get_name_ids (hb_face_t face, UInt32 table_tag, UInt32 feature_index, OpenTypeNameId* label_id, OpenTypeNameId* tooltip_id, OpenTypeNameId* sample_id, UInt32* num_named_parameters, OpenTypeNameId* first_param_id);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_feature_get_name_ids (hb_face_t face, UInt32 table_tag, UInt32 feature_index, OpenTypeNameId* label_id, OpenTypeNameId* tooltip_id, OpenTypeNameId* sample_id, UInt32* num_named_parameters, OpenTypeNameId* first_param_id);
		}
		private static Delegates.hb_ot_layout_feature_get_name_ids hb_ot_layout_feature_get_name_ids_delegate;
		internal static bool hb_ot_layout_feature_get_name_ids (hb_face_t face, UInt32 table_tag, UInt32 feature_index, OpenTypeNameId* label_id, OpenTypeNameId* tooltip_id, OpenTypeNameId* sample_id, UInt32* num_named_parameters, OpenTypeNameId* first_param_id) =>
			(hb_ot_layout_feature_get_name_ids_delegate ??= GetSymbol<Delegates.hb_ot_layout_feature_get_name_ids> ("hb_ot_layout_feature_get_name_ids")).Invoke (face, table_tag, feature_index, label_id, tooltip_id, sample_id, num_named_parameters, first_param_id);
		#endif

		// extern unsigned int hb_ot_layout_feature_with_variations_get_lookups(hb_face_t* face, hb_tag_t table_tag, unsigned int feature_index, unsigned int variations_index, unsigned int start_offset, unsigned int* lookup_count, unsigned int* lookup_indexes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_feature_with_variations_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 variations_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_feature_with_variations_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 variations_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_feature_with_variations_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 variations_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
		}
		private static Delegates.hb_ot_layout_feature_with_variations_get_lookups hb_ot_layout_feature_with_variations_get_lookups_delegate;
		internal static UInt32 hb_ot_layout_feature_with_variations_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 variations_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes) =>
			(hb_ot_layout_feature_with_variations_get_lookups_delegate ??= GetSymbol<Delegates.hb_ot_layout_feature_with_variations_get_lookups> ("hb_ot_layout_feature_with_variations_get_lookups")).Invoke (face, table_tag, feature_index, variations_index, start_offset, lookup_count, lookup_indexes);
		#endif

		// extern unsigned int hb_ot_layout_get_attach_points(hb_face_t* face, hb_codepoint_t glyph, unsigned int start_offset, unsigned int* point_count, unsigned int* point_array)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_get_attach_points (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* point_count, UInt32* point_array);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_get_attach_points (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* point_count, UInt32* point_array);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_get_attach_points (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* point_count, UInt32* point_array);
		}
		private static Delegates.hb_ot_layout_get_attach_points hb_ot_layout_get_attach_points_delegate;
		internal static UInt32 hb_ot_layout_get_attach_points (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* point_count, UInt32* point_array) =>
			(hb_ot_layout_get_attach_points_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_attach_points> ("hb_ot_layout_get_attach_points")).Invoke (face, glyph, start_offset, point_count, point_array);
		#endif

		// extern hb_bool_t hb_ot_layout_get_baseline(hb_font_t* font, hb_ot_layout_baseline_tag_t baseline_tag, hb_direction_t direction, hb_tag_t script_tag, hb_tag_t language_tag, hb_position_t* coord)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_get_baseline (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_baseline (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_get_baseline (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
		}
		private static Delegates.hb_ot_layout_get_baseline hb_ot_layout_get_baseline_delegate;
		internal static bool hb_ot_layout_get_baseline (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord) =>
			(hb_ot_layout_get_baseline_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_baseline> ("hb_ot_layout_get_baseline")).Invoke (font, baseline_tag, direction, script_tag, language_tag, coord);
		#endif

		// extern hb_bool_t hb_ot_layout_get_baseline2(hb_font_t* font, hb_ot_layout_baseline_tag_t baseline_tag, hb_direction_t direction, hb_script_t script, hb_language_t language, hb_position_t* coord)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_get_baseline2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_baseline2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_get_baseline2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord);
		}
		private static Delegates.hb_ot_layout_get_baseline2 hb_ot_layout_get_baseline2_delegate;
		internal static bool hb_ot_layout_get_baseline2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord) =>
			(hb_ot_layout_get_baseline2_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_baseline2> ("hb_ot_layout_get_baseline2")).Invoke (font, baseline_tag, direction, script, language, coord);
		#endif

		// extern void hb_ot_layout_get_baseline_with_fallback(hb_font_t* font, hb_ot_layout_baseline_tag_t baseline_tag, hb_direction_t direction, hb_tag_t script_tag, hb_tag_t language_tag, hb_position_t* coord)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_get_baseline_with_fallback (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_get_baseline_with_fallback (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_get_baseline_with_fallback (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
		}
		private static Delegates.hb_ot_layout_get_baseline_with_fallback hb_ot_layout_get_baseline_with_fallback_delegate;
		internal static void hb_ot_layout_get_baseline_with_fallback (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord) =>
			(hb_ot_layout_get_baseline_with_fallback_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_baseline_with_fallback> ("hb_ot_layout_get_baseline_with_fallback")).Invoke (font, baseline_tag, direction, script_tag, language_tag, coord);
		#endif

		// extern void hb_ot_layout_get_baseline_with_fallback2(hb_font_t* font, hb_ot_layout_baseline_tag_t baseline_tag, hb_direction_t direction, hb_script_t script, hb_language_t language, hb_position_t* coord)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_get_baseline_with_fallback2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_get_baseline_with_fallback2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_get_baseline_with_fallback2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord);
		}
		private static Delegates.hb_ot_layout_get_baseline_with_fallback2 hb_ot_layout_get_baseline_with_fallback2_delegate;
		internal static void hb_ot_layout_get_baseline_with_fallback2 (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script, IntPtr language, Int32* coord) =>
			(hb_ot_layout_get_baseline_with_fallback2_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_baseline_with_fallback2> ("hb_ot_layout_get_baseline_with_fallback2")).Invoke (font, baseline_tag, direction, script, language, coord);
		#endif

		// extern hb_bool_t hb_ot_layout_get_font_extents(hb_font_t* font, hb_direction_t direction, hb_tag_t script_tag, hb_tag_t language_tag, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_get_font_extents (hb_font_t font, Direction direction, UInt32 script_tag, UInt32 language_tag, FontExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_font_extents (hb_font_t font, Direction direction, UInt32 script_tag, UInt32 language_tag, FontExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_get_font_extents (hb_font_t font, Direction direction, UInt32 script_tag, UInt32 language_tag, FontExtents* extents);
		}
		private static Delegates.hb_ot_layout_get_font_extents hb_ot_layout_get_font_extents_delegate;
		internal static bool hb_ot_layout_get_font_extents (hb_font_t font, Direction direction, UInt32 script_tag, UInt32 language_tag, FontExtents* extents) =>
			(hb_ot_layout_get_font_extents_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_font_extents> ("hb_ot_layout_get_font_extents")).Invoke (font, direction, script_tag, language_tag, extents);
		#endif

		// extern hb_bool_t hb_ot_layout_get_font_extents2(hb_font_t* font, hb_direction_t direction, hb_script_t script, hb_language_t language, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_get_font_extents2 (hb_font_t font, Direction direction, UInt32 script, IntPtr language, FontExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_font_extents2 (hb_font_t font, Direction direction, UInt32 script, IntPtr language, FontExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_get_font_extents2 (hb_font_t font, Direction direction, UInt32 script, IntPtr language, FontExtents* extents);
		}
		private static Delegates.hb_ot_layout_get_font_extents2 hb_ot_layout_get_font_extents2_delegate;
		internal static bool hb_ot_layout_get_font_extents2 (hb_font_t font, Direction direction, UInt32 script, IntPtr language, FontExtents* extents) =>
			(hb_ot_layout_get_font_extents2_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_font_extents2> ("hb_ot_layout_get_font_extents2")).Invoke (font, direction, script, language, extents);
		#endif

		// extern hb_ot_layout_glyph_class_t hb_ot_layout_get_glyph_class(hb_face_t* face, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeLayoutGlyphClass hb_ot_layout_get_glyph_class (hb_face_t face, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeLayoutGlyphClass hb_ot_layout_get_glyph_class (hb_face_t face, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeLayoutGlyphClass hb_ot_layout_get_glyph_class (hb_face_t face, UInt32 glyph);
		}
		private static Delegates.hb_ot_layout_get_glyph_class hb_ot_layout_get_glyph_class_delegate;
		internal static OpenTypeLayoutGlyphClass hb_ot_layout_get_glyph_class (hb_face_t face, UInt32 glyph) =>
			(hb_ot_layout_get_glyph_class_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_glyph_class> ("hb_ot_layout_get_glyph_class")).Invoke (face, glyph);
		#endif

		// extern void hb_ot_layout_get_glyphs_in_class(hb_face_t* face, hb_ot_layout_glyph_class_t klass, hb_set_t* glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs);
		}
		private static Delegates.hb_ot_layout_get_glyphs_in_class hb_ot_layout_get_glyphs_in_class_delegate;
		internal static void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs) =>
			(hb_ot_layout_get_glyphs_in_class_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_glyphs_in_class> ("hb_ot_layout_get_glyphs_in_class")).Invoke (face, klass, glyphs);
		#endif

		// extern hb_ot_layout_baseline_tag_t hb_ot_layout_get_horizontal_baseline_tag_for_script(hb_script_t script)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeLayoutBaselineTag hb_ot_layout_get_horizontal_baseline_tag_for_script (UInt32 script);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeLayoutBaselineTag hb_ot_layout_get_horizontal_baseline_tag_for_script (UInt32 script);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeLayoutBaselineTag hb_ot_layout_get_horizontal_baseline_tag_for_script (UInt32 script);
		}
		private static Delegates.hb_ot_layout_get_horizontal_baseline_tag_for_script hb_ot_layout_get_horizontal_baseline_tag_for_script_delegate;
		internal static OpenTypeLayoutBaselineTag hb_ot_layout_get_horizontal_baseline_tag_for_script (UInt32 script) =>
			(hb_ot_layout_get_horizontal_baseline_tag_for_script_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_horizontal_baseline_tag_for_script> ("hb_ot_layout_get_horizontal_baseline_tag_for_script")).Invoke (script);
		#endif

		// extern unsigned int hb_ot_layout_get_ligature_carets(hb_font_t* font, hb_direction_t direction, hb_codepoint_t glyph, unsigned int start_offset, unsigned int* caret_count, hb_position_t* caret_array)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_get_ligature_carets (hb_font_t font, Direction direction, UInt32 glyph, UInt32 start_offset, UInt32* caret_count, Int32* caret_array);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_get_ligature_carets (hb_font_t font, Direction direction, UInt32 glyph, UInt32 start_offset, UInt32* caret_count, Int32* caret_array);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_get_ligature_carets (hb_font_t font, Direction direction, UInt32 glyph, UInt32 start_offset, UInt32* caret_count, Int32* caret_array);
		}
		private static Delegates.hb_ot_layout_get_ligature_carets hb_ot_layout_get_ligature_carets_delegate;
		internal static UInt32 hb_ot_layout_get_ligature_carets (hb_font_t font, Direction direction, UInt32 glyph, UInt32 start_offset, UInt32* caret_count, Int32* caret_array) =>
			(hb_ot_layout_get_ligature_carets_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_ligature_carets> ("hb_ot_layout_get_ligature_carets")).Invoke (font, direction, glyph, start_offset, caret_count, caret_array);
		#endif

		// extern hb_bool_t hb_ot_layout_get_size_params(hb_face_t* face, unsigned int* design_size, unsigned int* subfamily_id, hb_ot_name_id_t* subfamily_name_id, unsigned int* range_start, unsigned int* range_end)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_get_size_params (hb_face_t face, UInt32* design_size, UInt32* subfamily_id, OpenTypeNameId* subfamily_name_id, UInt32* range_start, UInt32* range_end);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_size_params (hb_face_t face, UInt32* design_size, UInt32* subfamily_id, OpenTypeNameId* subfamily_name_id, UInt32* range_start, UInt32* range_end);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_get_size_params (hb_face_t face, UInt32* design_size, UInt32* subfamily_id, OpenTypeNameId* subfamily_name_id, UInt32* range_start, UInt32* range_end);
		}
		private static Delegates.hb_ot_layout_get_size_params hb_ot_layout_get_size_params_delegate;
		internal static bool hb_ot_layout_get_size_params (hb_face_t face, UInt32* design_size, UInt32* subfamily_id, OpenTypeNameId* subfamily_name_id, UInt32* range_start, UInt32* range_end) =>
			(hb_ot_layout_get_size_params_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_size_params> ("hb_ot_layout_get_size_params")).Invoke (face, design_size, subfamily_id, subfamily_name_id, range_start, range_end);
		#endif

		// extern hb_bool_t hb_ot_layout_has_glyph_classes(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_has_glyph_classes (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_has_glyph_classes (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_has_glyph_classes (hb_face_t face);
		}
		private static Delegates.hb_ot_layout_has_glyph_classes hb_ot_layout_has_glyph_classes_delegate;
		internal static bool hb_ot_layout_has_glyph_classes (hb_face_t face) =>
			(hb_ot_layout_has_glyph_classes_delegate ??= GetSymbol<Delegates.hb_ot_layout_has_glyph_classes> ("hb_ot_layout_has_glyph_classes")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_layout_has_positioning(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_has_positioning (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_has_positioning (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_has_positioning (hb_face_t face);
		}
		private static Delegates.hb_ot_layout_has_positioning hb_ot_layout_has_positioning_delegate;
		internal static bool hb_ot_layout_has_positioning (hb_face_t face) =>
			(hb_ot_layout_has_positioning_delegate ??= GetSymbol<Delegates.hb_ot_layout_has_positioning> ("hb_ot_layout_has_positioning")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_layout_has_substitution(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_has_substitution (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_has_substitution (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_has_substitution (hb_face_t face);
		}
		private static Delegates.hb_ot_layout_has_substitution hb_ot_layout_has_substitution_delegate;
		internal static bool hb_ot_layout_has_substitution (hb_face_t face) =>
			(hb_ot_layout_has_substitution_delegate ??= GetSymbol<Delegates.hb_ot_layout_has_substitution> ("hb_ot_layout_has_substitution")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_layout_language_find_feature(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_index, hb_tag_t feature_tag, unsigned int* feature_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_language_find_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 feature_tag, UInt32* feature_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_language_find_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 feature_tag, UInt32* feature_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_language_find_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 feature_tag, UInt32* feature_index);
		}
		private static Delegates.hb_ot_layout_language_find_feature hb_ot_layout_language_find_feature_delegate;
		internal static bool hb_ot_layout_language_find_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 feature_tag, UInt32* feature_index) =>
			(hb_ot_layout_language_find_feature_delegate ??= GetSymbol<Delegates.hb_ot_layout_language_find_feature> ("hb_ot_layout_language_find_feature")).Invoke (face, table_tag, script_index, language_index, feature_tag, feature_index);
		#endif

		// extern unsigned int hb_ot_layout_language_get_feature_indexes(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_index, unsigned int start_offset, unsigned int* feature_count, unsigned int* feature_indexes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_language_get_feature_indexes (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_indexes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_language_get_feature_indexes (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_indexes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_language_get_feature_indexes (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_indexes);
		}
		private static Delegates.hb_ot_layout_language_get_feature_indexes hb_ot_layout_language_get_feature_indexes_delegate;
		internal static UInt32 hb_ot_layout_language_get_feature_indexes (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_indexes) =>
			(hb_ot_layout_language_get_feature_indexes_delegate ??= GetSymbol<Delegates.hb_ot_layout_language_get_feature_indexes> ("hb_ot_layout_language_get_feature_indexes")).Invoke (face, table_tag, script_index, language_index, start_offset, feature_count, feature_indexes);
		#endif

		// extern unsigned int hb_ot_layout_language_get_feature_tags(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_index, unsigned int start_offset, unsigned int* feature_count, hb_tag_t* feature_tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_language_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_language_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_language_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
		}
		private static Delegates.hb_ot_layout_language_get_feature_tags hb_ot_layout_language_get_feature_tags_delegate;
		internal static UInt32 hb_ot_layout_language_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags) =>
			(hb_ot_layout_language_get_feature_tags_delegate ??= GetSymbol<Delegates.hb_ot_layout_language_get_feature_tags> ("hb_ot_layout_language_get_feature_tags")).Invoke (face, table_tag, script_index, language_index, start_offset, feature_count, feature_tags);
		#endif

		// extern hb_bool_t hb_ot_layout_language_get_required_feature(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_index, unsigned int* feature_index, hb_tag_t* feature_tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_language_get_required_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index, UInt32* feature_tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_language_get_required_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index, UInt32* feature_tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_language_get_required_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index, UInt32* feature_tag);
		}
		private static Delegates.hb_ot_layout_language_get_required_feature hb_ot_layout_language_get_required_feature_delegate;
		internal static bool hb_ot_layout_language_get_required_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index, UInt32* feature_tag) =>
			(hb_ot_layout_language_get_required_feature_delegate ??= GetSymbol<Delegates.hb_ot_layout_language_get_required_feature> ("hb_ot_layout_language_get_required_feature")).Invoke (face, table_tag, script_index, language_index, feature_index, feature_tag);
		#endif

		// extern hb_bool_t hb_ot_layout_language_get_required_feature_index(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_index, unsigned int* feature_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_language_get_required_feature_index (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_language_get_required_feature_index (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_language_get_required_feature_index (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index);
		}
		private static Delegates.hb_ot_layout_language_get_required_feature_index hb_ot_layout_language_get_required_feature_index_delegate;
		internal static bool hb_ot_layout_language_get_required_feature_index (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index) =>
			(hb_ot_layout_language_get_required_feature_index_delegate ??= GetSymbol<Delegates.hb_ot_layout_language_get_required_feature_index> ("hb_ot_layout_language_get_required_feature_index")).Invoke (face, table_tag, script_index, language_index, feature_index);
		#endif

		// extern hb_bool_t hb_ot_layout_lookup_collect_glyph_alternates(hb_face_t* face, unsigned int lookup_index, hb_map_t* alternate_count, hb_map_t* alternate_glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_lookup_collect_glyph_alternates (hb_face_t face, UInt32 lookup_index, hb_map_t alternate_count, hb_map_t alternate_glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_lookup_collect_glyph_alternates (hb_face_t face, UInt32 lookup_index, hb_map_t alternate_count, hb_map_t alternate_glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_lookup_collect_glyph_alternates (hb_face_t face, UInt32 lookup_index, hb_map_t alternate_count, hb_map_t alternate_glyphs);
		}
		private static Delegates.hb_ot_layout_lookup_collect_glyph_alternates hb_ot_layout_lookup_collect_glyph_alternates_delegate;
		internal static bool hb_ot_layout_lookup_collect_glyph_alternates (hb_face_t face, UInt32 lookup_index, hb_map_t alternate_count, hb_map_t alternate_glyphs) =>
			(hb_ot_layout_lookup_collect_glyph_alternates_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_collect_glyph_alternates> ("hb_ot_layout_lookup_collect_glyph_alternates")).Invoke (face, lookup_index, alternate_count, alternate_glyphs);
		#endif

		// extern void hb_ot_layout_lookup_collect_glyphs(hb_face_t* face, hb_tag_t table_tag, unsigned int lookup_index, hb_set_t* glyphs_before, hb_set_t* glyphs_input, hb_set_t* glyphs_after, hb_set_t* glyphs_output)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_lookup_collect_glyphs (hb_face_t face, UInt32 table_tag, UInt32 lookup_index, hb_set_t glyphs_before, hb_set_t glyphs_input, hb_set_t glyphs_after, hb_set_t glyphs_output);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_lookup_collect_glyphs (hb_face_t face, UInt32 table_tag, UInt32 lookup_index, hb_set_t glyphs_before, hb_set_t glyphs_input, hb_set_t glyphs_after, hb_set_t glyphs_output);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_lookup_collect_glyphs (hb_face_t face, UInt32 table_tag, UInt32 lookup_index, hb_set_t glyphs_before, hb_set_t glyphs_input, hb_set_t glyphs_after, hb_set_t glyphs_output);
		}
		private static Delegates.hb_ot_layout_lookup_collect_glyphs hb_ot_layout_lookup_collect_glyphs_delegate;
		internal static void hb_ot_layout_lookup_collect_glyphs (hb_face_t face, UInt32 table_tag, UInt32 lookup_index, hb_set_t glyphs_before, hb_set_t glyphs_input, hb_set_t glyphs_after, hb_set_t glyphs_output) =>
			(hb_ot_layout_lookup_collect_glyphs_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_collect_glyphs> ("hb_ot_layout_lookup_collect_glyphs")).Invoke (face, table_tag, lookup_index, glyphs_before, glyphs_input, glyphs_after, glyphs_output);
		#endif

		// extern unsigned int hb_ot_layout_lookup_get_glyph_alternates(hb_face_t* face, unsigned int lookup_index, hb_codepoint_t glyph, unsigned int start_offset, unsigned int* alternate_count, hb_codepoint_t* alternate_glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs);
		}
		private static Delegates.hb_ot_layout_lookup_get_glyph_alternates hb_ot_layout_lookup_get_glyph_alternates_delegate;
		internal static UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs) =>
			(hb_ot_layout_lookup_get_glyph_alternates_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_get_glyph_alternates> ("hb_ot_layout_lookup_get_glyph_alternates")).Invoke (face, lookup_index, glyph, start_offset, alternate_count, alternate_glyphs);
		#endif

		// extern hb_position_t hb_ot_layout_lookup_get_optical_bound(hb_font_t* font, unsigned int lookup_index, hb_direction_t direction, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_layout_lookup_get_optical_bound (hb_font_t font, UInt32 lookup_index, Direction direction, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_layout_lookup_get_optical_bound (hb_font_t font, UInt32 lookup_index, Direction direction, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_layout_lookup_get_optical_bound (hb_font_t font, UInt32 lookup_index, Direction direction, UInt32 glyph);
		}
		private static Delegates.hb_ot_layout_lookup_get_optical_bound hb_ot_layout_lookup_get_optical_bound_delegate;
		internal static Int32 hb_ot_layout_lookup_get_optical_bound (hb_font_t font, UInt32 lookup_index, Direction direction, UInt32 glyph) =>
			(hb_ot_layout_lookup_get_optical_bound_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_get_optical_bound> ("hb_ot_layout_lookup_get_optical_bound")).Invoke (font, lookup_index, direction, glyph);
		#endif

		// extern void hb_ot_layout_lookup_substitute_closure(hb_face_t* face, unsigned int lookup_index, hb_set_t* glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_lookup_substitute_closure (hb_face_t face, UInt32 lookup_index, hb_set_t glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_lookup_substitute_closure (hb_face_t face, UInt32 lookup_index, hb_set_t glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_lookup_substitute_closure (hb_face_t face, UInt32 lookup_index, hb_set_t glyphs);
		}
		private static Delegates.hb_ot_layout_lookup_substitute_closure hb_ot_layout_lookup_substitute_closure_delegate;
		internal static void hb_ot_layout_lookup_substitute_closure (hb_face_t face, UInt32 lookup_index, hb_set_t glyphs) =>
			(hb_ot_layout_lookup_substitute_closure_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_substitute_closure> ("hb_ot_layout_lookup_substitute_closure")).Invoke (face, lookup_index, glyphs);
		#endif

		// extern hb_bool_t hb_ot_layout_lookup_would_substitute(hb_face_t* face, unsigned int lookup_index, const hb_codepoint_t* glyphs, unsigned int glyphs_length, hb_bool_t zero_context)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_lookup_would_substitute (hb_face_t face, UInt32 lookup_index, UInt32* glyphs, UInt32 glyphs_length, [MarshalAs (UnmanagedType.I1)] bool zero_context);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_lookup_would_substitute (hb_face_t face, UInt32 lookup_index, UInt32* glyphs, UInt32 glyphs_length, [MarshalAs (UnmanagedType.I1)] bool zero_context);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_lookup_would_substitute (hb_face_t face, UInt32 lookup_index, UInt32* glyphs, UInt32 glyphs_length, [MarshalAs (UnmanagedType.I1)] bool zero_context);
		}
		private static Delegates.hb_ot_layout_lookup_would_substitute hb_ot_layout_lookup_would_substitute_delegate;
		internal static bool hb_ot_layout_lookup_would_substitute (hb_face_t face, UInt32 lookup_index, UInt32* glyphs, UInt32 glyphs_length, [MarshalAs (UnmanagedType.I1)] bool zero_context) =>
			(hb_ot_layout_lookup_would_substitute_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_would_substitute> ("hb_ot_layout_lookup_would_substitute")).Invoke (face, lookup_index, glyphs, glyphs_length, zero_context);
		#endif

		// extern void hb_ot_layout_lookups_substitute_closure(hb_face_t* face, const hb_set_t* lookups, hb_set_t* glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_layout_lookups_substitute_closure (hb_face_t face, hb_set_t lookups, hb_set_t glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_lookups_substitute_closure (hb_face_t face, hb_set_t lookups, hb_set_t glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_lookups_substitute_closure (hb_face_t face, hb_set_t lookups, hb_set_t glyphs);
		}
		private static Delegates.hb_ot_layout_lookups_substitute_closure hb_ot_layout_lookups_substitute_closure_delegate;
		internal static void hb_ot_layout_lookups_substitute_closure (hb_face_t face, hb_set_t lookups, hb_set_t glyphs) =>
			(hb_ot_layout_lookups_substitute_closure_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookups_substitute_closure> ("hb_ot_layout_lookups_substitute_closure")).Invoke (face, lookups, glyphs);
		#endif

		// extern unsigned int hb_ot_layout_script_get_language_tags(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int start_offset, unsigned int* language_count, hb_tag_t* language_tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_script_get_language_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 start_offset, UInt32* language_count, UInt32* language_tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_script_get_language_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 start_offset, UInt32* language_count, UInt32* language_tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_script_get_language_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 start_offset, UInt32* language_count, UInt32* language_tags);
		}
		private static Delegates.hb_ot_layout_script_get_language_tags hb_ot_layout_script_get_language_tags_delegate;
		internal static UInt32 hb_ot_layout_script_get_language_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 start_offset, UInt32* language_count, UInt32* language_tags) =>
			(hb_ot_layout_script_get_language_tags_delegate ??= GetSymbol<Delegates.hb_ot_layout_script_get_language_tags> ("hb_ot_layout_script_get_language_tags")).Invoke (face, table_tag, script_index, start_offset, language_count, language_tags);
		#endif

		// extern hb_bool_t hb_ot_layout_script_select_language(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_count, const hb_tag_t* language_tags, unsigned int* language_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_script_select_language (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_script_select_language (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_script_select_language (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index);
		}
		private static Delegates.hb_ot_layout_script_select_language hb_ot_layout_script_select_language_delegate;
		internal static bool hb_ot_layout_script_select_language (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index) =>
			(hb_ot_layout_script_select_language_delegate ??= GetSymbol<Delegates.hb_ot_layout_script_select_language> ("hb_ot_layout_script_select_language")).Invoke (face, table_tag, script_index, language_count, language_tags, language_index);
		#endif

		// extern hb_bool_t hb_ot_layout_script_select_language2(hb_face_t* face, hb_tag_t table_tag, unsigned int script_index, unsigned int language_count, const hb_tag_t* language_tags, unsigned int* language_index, hb_tag_t* chosen_language)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_script_select_language2 (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index, UInt32* chosen_language);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_script_select_language2 (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index, UInt32* chosen_language);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_script_select_language2 (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index, UInt32* chosen_language);
		}
		private static Delegates.hb_ot_layout_script_select_language2 hb_ot_layout_script_select_language2_delegate;
		internal static bool hb_ot_layout_script_select_language2 (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index, UInt32* chosen_language) =>
			(hb_ot_layout_script_select_language2_delegate ??= GetSymbol<Delegates.hb_ot_layout_script_select_language2> ("hb_ot_layout_script_select_language2")).Invoke (face, table_tag, script_index, language_count, language_tags, language_index, chosen_language);
		#endif

		// extern hb_bool_t hb_ot_layout_table_find_feature_variations(hb_face_t* face, hb_tag_t table_tag, const int* coords, unsigned int num_coords, unsigned int* variations_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_table_find_feature_variations (hb_face_t face, UInt32 table_tag, Int32* coords, UInt32 num_coords, UInt32* variations_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_table_find_feature_variations (hb_face_t face, UInt32 table_tag, Int32* coords, UInt32 num_coords, UInt32* variations_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_table_find_feature_variations (hb_face_t face, UInt32 table_tag, Int32* coords, UInt32 num_coords, UInt32* variations_index);
		}
		private static Delegates.hb_ot_layout_table_find_feature_variations hb_ot_layout_table_find_feature_variations_delegate;
		internal static bool hb_ot_layout_table_find_feature_variations (hb_face_t face, UInt32 table_tag, Int32* coords, UInt32 num_coords, UInt32* variations_index) =>
			(hb_ot_layout_table_find_feature_variations_delegate ??= GetSymbol<Delegates.hb_ot_layout_table_find_feature_variations> ("hb_ot_layout_table_find_feature_variations")).Invoke (face, table_tag, coords, num_coords, variations_index);
		#endif

		// extern hb_bool_t hb_ot_layout_table_find_script(hb_face_t* face, hb_tag_t table_tag, hb_tag_t script_tag, unsigned int* script_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_table_find_script (hb_face_t face, UInt32 table_tag, UInt32 script_tag, UInt32* script_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_table_find_script (hb_face_t face, UInt32 table_tag, UInt32 script_tag, UInt32* script_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_table_find_script (hb_face_t face, UInt32 table_tag, UInt32 script_tag, UInt32* script_index);
		}
		private static Delegates.hb_ot_layout_table_find_script hb_ot_layout_table_find_script_delegate;
		internal static bool hb_ot_layout_table_find_script (hb_face_t face, UInt32 table_tag, UInt32 script_tag, UInt32* script_index) =>
			(hb_ot_layout_table_find_script_delegate ??= GetSymbol<Delegates.hb_ot_layout_table_find_script> ("hb_ot_layout_table_find_script")).Invoke (face, table_tag, script_tag, script_index);
		#endif

		// extern unsigned int hb_ot_layout_table_get_feature_tags(hb_face_t* face, hb_tag_t table_tag, unsigned int start_offset, unsigned int* feature_count, hb_tag_t* feature_tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_table_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_table_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_table_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
		}
		private static Delegates.hb_ot_layout_table_get_feature_tags hb_ot_layout_table_get_feature_tags_delegate;
		internal static UInt32 hb_ot_layout_table_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags) =>
			(hb_ot_layout_table_get_feature_tags_delegate ??= GetSymbol<Delegates.hb_ot_layout_table_get_feature_tags> ("hb_ot_layout_table_get_feature_tags")).Invoke (face, table_tag, start_offset, feature_count, feature_tags);
		#endif

		// extern unsigned int hb_ot_layout_table_get_lookup_count(hb_face_t* face, hb_tag_t table_tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_table_get_lookup_count (hb_face_t face, UInt32 table_tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_table_get_lookup_count (hb_face_t face, UInt32 table_tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_table_get_lookup_count (hb_face_t face, UInt32 table_tag);
		}
		private static Delegates.hb_ot_layout_table_get_lookup_count hb_ot_layout_table_get_lookup_count_delegate;
		internal static UInt32 hb_ot_layout_table_get_lookup_count (hb_face_t face, UInt32 table_tag) =>
			(hb_ot_layout_table_get_lookup_count_delegate ??= GetSymbol<Delegates.hb_ot_layout_table_get_lookup_count> ("hb_ot_layout_table_get_lookup_count")).Invoke (face, table_tag);
		#endif

		// extern unsigned int hb_ot_layout_table_get_script_tags(hb_face_t* face, hb_tag_t table_tag, unsigned int start_offset, unsigned int* script_count, hb_tag_t* script_tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_layout_table_get_script_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* script_count, UInt32* script_tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_table_get_script_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* script_count, UInt32* script_tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_table_get_script_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* script_count, UInt32* script_tags);
		}
		private static Delegates.hb_ot_layout_table_get_script_tags hb_ot_layout_table_get_script_tags_delegate;
		internal static UInt32 hb_ot_layout_table_get_script_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* script_count, UInt32* script_tags) =>
			(hb_ot_layout_table_get_script_tags_delegate ??= GetSymbol<Delegates.hb_ot_layout_table_get_script_tags> ("hb_ot_layout_table_get_script_tags")).Invoke (face, table_tag, start_offset, script_count, script_tags);
		#endif

		// extern hb_bool_t hb_ot_layout_table_select_script(hb_face_t* face, hb_tag_t table_tag, unsigned int script_count, const hb_tag_t* script_tags, unsigned int* script_index, hb_tag_t* chosen_script)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_layout_table_select_script (hb_face_t face, UInt32 table_tag, UInt32 script_count, UInt32* script_tags, UInt32* script_index, UInt32* chosen_script);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_table_select_script (hb_face_t face, UInt32 table_tag, UInt32 script_count, UInt32* script_tags, UInt32* script_index, UInt32* chosen_script);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_layout_table_select_script (hb_face_t face, UInt32 table_tag, UInt32 script_count, UInt32* script_tags, UInt32* script_index, UInt32* chosen_script);
		}
		private static Delegates.hb_ot_layout_table_select_script hb_ot_layout_table_select_script_delegate;
		internal static bool hb_ot_layout_table_select_script (hb_face_t face, UInt32 table_tag, UInt32 script_count, UInt32* script_tags, UInt32* script_index, UInt32* chosen_script) =>
			(hb_ot_layout_table_select_script_delegate ??= GetSymbol<Delegates.hb_ot_layout_table_select_script> ("hb_ot_layout_table_select_script")).Invoke (face, table_tag, script_count, script_tags, script_index, chosen_script);
		#endif

		// extern hb_language_t hb_ot_tag_to_language(hb_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_ot_tag_to_language (UInt32 tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_ot_tag_to_language (UInt32 tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_ot_tag_to_language (UInt32 tag);
		}
		private static Delegates.hb_ot_tag_to_language hb_ot_tag_to_language_delegate;
		internal static IntPtr hb_ot_tag_to_language (UInt32 tag) =>
			(hb_ot_tag_to_language_delegate ??= GetSymbol<Delegates.hb_ot_tag_to_language> ("hb_ot_tag_to_language")).Invoke (tag);
		#endif

		// extern hb_script_t hb_ot_tag_to_script(hb_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_tag_to_script (UInt32 tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_tag_to_script (UInt32 tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_tag_to_script (UInt32 tag);
		}
		private static Delegates.hb_ot_tag_to_script hb_ot_tag_to_script_delegate;
		internal static UInt32 hb_ot_tag_to_script (UInt32 tag) =>
			(hb_ot_tag_to_script_delegate ??= GetSymbol<Delegates.hb_ot_tag_to_script> ("hb_ot_tag_to_script")).Invoke (tag);
		#endif

		// extern void hb_ot_tags_from_script_and_language(hb_script_t script, hb_language_t language, unsigned int* script_count, hb_tag_t* script_tags, unsigned int* language_count, hb_tag_t* language_tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_tags_from_script_and_language (UInt32 script, IntPtr language, UInt32* script_count, UInt32* script_tags, UInt32* language_count, UInt32* language_tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_tags_from_script_and_language (UInt32 script, IntPtr language, UInt32* script_count, UInt32* script_tags, UInt32* language_count, UInt32* language_tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_tags_from_script_and_language (UInt32 script, IntPtr language, UInt32* script_count, UInt32* script_tags, UInt32* language_count, UInt32* language_tags);
		}
		private static Delegates.hb_ot_tags_from_script_and_language hb_ot_tags_from_script_and_language_delegate;
		internal static void hb_ot_tags_from_script_and_language (UInt32 script, IntPtr language, UInt32* script_count, UInt32* script_tags, UInt32* language_count, UInt32* language_tags) =>
			(hb_ot_tags_from_script_and_language_delegate ??= GetSymbol<Delegates.hb_ot_tags_from_script_and_language> ("hb_ot_tags_from_script_and_language")).Invoke (script, language, script_count, script_tags, language_count, language_tags);
		#endif

		// extern void hb_ot_tags_to_script_and_language(hb_tag_t script_tag, hb_tag_t language_tag, hb_script_t* script, hb_language_t* language)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_tags_to_script_and_language (UInt32 script_tag, UInt32 language_tag, UInt32* script, IntPtr* language);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_tags_to_script_and_language (UInt32 script_tag, UInt32 language_tag, UInt32* script, IntPtr* language);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_tags_to_script_and_language (UInt32 script_tag, UInt32 language_tag, UInt32* script, IntPtr* language);
		}
		private static Delegates.hb_ot_tags_to_script_and_language hb_ot_tags_to_script_and_language_delegate;
		internal static void hb_ot_tags_to_script_and_language (UInt32 script_tag, UInt32 language_tag, UInt32* script, IntPtr* language) =>
			(hb_ot_tags_to_script_and_language_delegate ??= GetSymbol<Delegates.hb_ot_tags_to_script_and_language> ("hb_ot_tags_to_script_and_language")).Invoke (script_tag, language_tag, script, language);
		#endif

		#endregion

	}
}
