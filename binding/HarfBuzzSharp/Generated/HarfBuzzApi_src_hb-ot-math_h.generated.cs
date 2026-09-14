using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-math.h

		// extern hb_position_t hb_ot_math_get_constant(hb_font_t* font, hb_ot_math_constant_t constant)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_math_get_constant (hb_font_t font, OpenTypeMathConstant constant);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_constant (hb_font_t font, OpenTypeMathConstant constant);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_math_get_constant (hb_font_t font, OpenTypeMathConstant constant);
		}
		private static Delegates.hb_ot_math_get_constant hb_ot_math_get_constant_delegate;
		internal static Int32 hb_ot_math_get_constant (hb_font_t font, OpenTypeMathConstant constant) =>
			(hb_ot_math_get_constant_delegate ??= GetSymbol<Delegates.hb_ot_math_get_constant> ("hb_ot_math_get_constant")).Invoke (font, constant);
		#endif

		// extern unsigned int hb_ot_math_get_glyph_assembly(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, unsigned int start_offset, unsigned int* parts_count, hb_ot_math_glyph_part_t* parts, hb_position_t* italics_correction)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_math_get_glyph_assembly (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* parts_count, OpenTypeMathGlyphPart* parts, Int32* italics_correction);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_math_get_glyph_assembly (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* parts_count, OpenTypeMathGlyphPart* parts, Int32* italics_correction);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_math_get_glyph_assembly (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* parts_count, OpenTypeMathGlyphPart* parts, Int32* italics_correction);
		}
		private static Delegates.hb_ot_math_get_glyph_assembly hb_ot_math_get_glyph_assembly_delegate;
		internal static UInt32 hb_ot_math_get_glyph_assembly (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* parts_count, OpenTypeMathGlyphPart* parts, Int32* italics_correction) =>
			(hb_ot_math_get_glyph_assembly_delegate ??= GetSymbol<Delegates.hb_ot_math_get_glyph_assembly> ("hb_ot_math_get_glyph_assembly")).Invoke (font, glyph, direction, start_offset, parts_count, parts, italics_correction);
		#endif

		// extern hb_position_t hb_ot_math_get_glyph_italics_correction(hb_font_t* font, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_math_get_glyph_italics_correction (hb_font_t font, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_glyph_italics_correction (hb_font_t font, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_math_get_glyph_italics_correction (hb_font_t font, UInt32 glyph);
		}
		private static Delegates.hb_ot_math_get_glyph_italics_correction hb_ot_math_get_glyph_italics_correction_delegate;
		internal static Int32 hb_ot_math_get_glyph_italics_correction (hb_font_t font, UInt32 glyph) =>
			(hb_ot_math_get_glyph_italics_correction_delegate ??= GetSymbol<Delegates.hb_ot_math_get_glyph_italics_correction> ("hb_ot_math_get_glyph_italics_correction")).Invoke (font, glyph);
		#endif

		// extern hb_position_t hb_ot_math_get_glyph_kerning(hb_font_t* font, hb_codepoint_t glyph, hb_ot_math_kern_t kern, hb_position_t correction_height)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_math_get_glyph_kerning (hb_font_t font, UInt32 glyph, OpenTypeMathKern kern, Int32 correction_height);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_glyph_kerning (hb_font_t font, UInt32 glyph, OpenTypeMathKern kern, Int32 correction_height);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_math_get_glyph_kerning (hb_font_t font, UInt32 glyph, OpenTypeMathKern kern, Int32 correction_height);
		}
		private static Delegates.hb_ot_math_get_glyph_kerning hb_ot_math_get_glyph_kerning_delegate;
		internal static Int32 hb_ot_math_get_glyph_kerning (hb_font_t font, UInt32 glyph, OpenTypeMathKern kern, Int32 correction_height) =>
			(hb_ot_math_get_glyph_kerning_delegate ??= GetSymbol<Delegates.hb_ot_math_get_glyph_kerning> ("hb_ot_math_get_glyph_kerning")).Invoke (font, glyph, kern, correction_height);
		#endif

		// extern hb_position_t hb_ot_math_get_glyph_top_accent_attachment(hb_font_t* font, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_math_get_glyph_top_accent_attachment (hb_font_t font, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_glyph_top_accent_attachment (hb_font_t font, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_math_get_glyph_top_accent_attachment (hb_font_t font, UInt32 glyph);
		}
		private static Delegates.hb_ot_math_get_glyph_top_accent_attachment hb_ot_math_get_glyph_top_accent_attachment_delegate;
		internal static Int32 hb_ot_math_get_glyph_top_accent_attachment (hb_font_t font, UInt32 glyph) =>
			(hb_ot_math_get_glyph_top_accent_attachment_delegate ??= GetSymbol<Delegates.hb_ot_math_get_glyph_top_accent_attachment> ("hb_ot_math_get_glyph_top_accent_attachment")).Invoke (font, glyph);
		#endif

		// extern unsigned int hb_ot_math_get_glyph_variants(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, unsigned int start_offset, unsigned int* variants_count, hb_ot_math_glyph_variant_t* variants)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_math_get_glyph_variants (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* variants_count, OpenTypeMathGlyphVariant* variants);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_math_get_glyph_variants (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* variants_count, OpenTypeMathGlyphVariant* variants);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_math_get_glyph_variants (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* variants_count, OpenTypeMathGlyphVariant* variants);
		}
		private static Delegates.hb_ot_math_get_glyph_variants hb_ot_math_get_glyph_variants_delegate;
		internal static UInt32 hb_ot_math_get_glyph_variants (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* variants_count, OpenTypeMathGlyphVariant* variants) =>
			(hb_ot_math_get_glyph_variants_delegate ??= GetSymbol<Delegates.hb_ot_math_get_glyph_variants> ("hb_ot_math_get_glyph_variants")).Invoke (font, glyph, direction, start_offset, variants_count, variants);
		#endif

		// extern hb_position_t hb_ot_math_get_min_connector_overlap(hb_font_t* font, hb_direction_t direction)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_math_get_min_connector_overlap (hb_font_t font, Direction direction);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_min_connector_overlap (hb_font_t font, Direction direction);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_math_get_min_connector_overlap (hb_font_t font, Direction direction);
		}
		private static Delegates.hb_ot_math_get_min_connector_overlap hb_ot_math_get_min_connector_overlap_delegate;
		internal static Int32 hb_ot_math_get_min_connector_overlap (hb_font_t font, Direction direction) =>
			(hb_ot_math_get_min_connector_overlap_delegate ??= GetSymbol<Delegates.hb_ot_math_get_min_connector_overlap> ("hb_ot_math_get_min_connector_overlap")).Invoke (font, direction);
		#endif

		// extern hb_bool_t hb_ot_math_has_data(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_math_has_data (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_math_has_data (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_math_has_data (hb_face_t face);
		}
		private static Delegates.hb_ot_math_has_data hb_ot_math_has_data_delegate;
		internal static bool hb_ot_math_has_data (hb_face_t face) =>
			(hb_ot_math_has_data_delegate ??= GetSymbol<Delegates.hb_ot_math_has_data> ("hb_ot_math_has_data")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_math_is_glyph_extended_shape(hb_face_t* face, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_math_is_glyph_extended_shape (hb_face_t face, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_math_is_glyph_extended_shape (hb_face_t face, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_math_is_glyph_extended_shape (hb_face_t face, UInt32 glyph);
		}
		private static Delegates.hb_ot_math_is_glyph_extended_shape hb_ot_math_is_glyph_extended_shape_delegate;
		internal static bool hb_ot_math_is_glyph_extended_shape (hb_face_t face, UInt32 glyph) =>
			(hb_ot_math_is_glyph_extended_shape_delegate ??= GetSymbol<Delegates.hb_ot_math_is_glyph_extended_shape> ("hb_ot_math_is_glyph_extended_shape")).Invoke (face, glyph);
		#endif

		#endregion

	}
}
