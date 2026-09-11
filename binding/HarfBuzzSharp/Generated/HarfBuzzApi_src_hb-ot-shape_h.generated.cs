using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-shape.h

		// extern unsigned int hb_ot_shape_get_buffer_format_serial()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_shape_get_buffer_format_serial ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_shape_get_buffer_format_serial ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_shape_get_buffer_format_serial ();
		}
		private static Delegates.hb_ot_shape_get_buffer_format_serial hb_ot_shape_get_buffer_format_serial_delegate;
		internal static UInt32 hb_ot_shape_get_buffer_format_serial () =>
			(hb_ot_shape_get_buffer_format_serial_delegate ??= GetSymbol<Delegates.hb_ot_shape_get_buffer_format_serial> ("hb_ot_shape_get_buffer_format_serial")).Invoke ();
		#endif

		// extern void hb_ot_shape_glyphs_closure(hb_font_t* font, hb_buffer_t* buffer, const hb_feature_t* features, unsigned int num_features, hb_set_t* glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_shape_glyphs_closure (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, hb_set_t glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_shape_glyphs_closure (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, hb_set_t glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_shape_glyphs_closure (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, hb_set_t glyphs);
		}
		private static Delegates.hb_ot_shape_glyphs_closure hb_ot_shape_glyphs_closure_delegate;
		internal static void hb_ot_shape_glyphs_closure (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, hb_set_t glyphs) =>
			(hb_ot_shape_glyphs_closure_delegate ??= GetSymbol<Delegates.hb_ot_shape_glyphs_closure> ("hb_ot_shape_glyphs_closure")).Invoke (font, buffer, features, num_features, glyphs);
		#endif

		// extern void hb_ot_shape_plan_collect_lookups(hb_shape_plan_t* shape_plan, hb_tag_t table_tag, hb_set_t* lookup_indexes)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes);
		}
		private static Delegates.hb_ot_shape_plan_collect_lookups hb_ot_shape_plan_collect_lookups_delegate;
		internal static void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes) =>
			(hb_ot_shape_plan_collect_lookups_delegate ??= GetSymbol<Delegates.hb_ot_shape_plan_collect_lookups> ("hb_ot_shape_plan_collect_lookups")).Invoke (shape_plan, table_tag, lookup_indexes);
		#endif

		// extern unsigned int hb_ot_shape_plan_get_feature_tags(hb_shape_plan_t* shape_plan, unsigned int start_offset, unsigned int* tag_count, hb_tag_t* tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_shape_plan_get_feature_tags (hb_shape_plan_t shape_plan, UInt32 start_offset, UInt32* tag_count, UInt32* tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_shape_plan_get_feature_tags (hb_shape_plan_t shape_plan, UInt32 start_offset, UInt32* tag_count, UInt32* tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_shape_plan_get_feature_tags (hb_shape_plan_t shape_plan, UInt32 start_offset, UInt32* tag_count, UInt32* tags);
		}
		private static Delegates.hb_ot_shape_plan_get_feature_tags hb_ot_shape_plan_get_feature_tags_delegate;
		internal static UInt32 hb_ot_shape_plan_get_feature_tags (hb_shape_plan_t shape_plan, UInt32 start_offset, UInt32* tag_count, UInt32* tags) =>
			(hb_ot_shape_plan_get_feature_tags_delegate ??= GetSymbol<Delegates.hb_ot_shape_plan_get_feature_tags> ("hb_ot_shape_plan_get_feature_tags")).Invoke (shape_plan, start_offset, tag_count, tags);
		#endif

		#endregion

	}
}
