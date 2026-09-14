using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-color.h

		// extern unsigned int hb_ot_color_get_svg_document_count(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_color_get_svg_document_count (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_get_svg_document_count (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_color_get_svg_document_count (hb_face_t face);
		}
		private static Delegates.hb_ot_color_get_svg_document_count hb_ot_color_get_svg_document_count_delegate;
		internal static UInt32 hb_ot_color_get_svg_document_count (hb_face_t face) =>
			(hb_ot_color_get_svg_document_count_delegate ??= GetSymbol<Delegates.hb_ot_color_get_svg_document_count> ("hb_ot_color_get_svg_document_count")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_color_get_svg_document_glyph_range(hb_face_t* face, unsigned int svg_document_index, hb_codepoint_t* start_glyph_id, hb_codepoint_t* end_glyph_id)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_get_svg_document_glyph_range (hb_face_t face, UInt32 svg_document_index, UInt32* start_glyph_id, UInt32* end_glyph_id);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_get_svg_document_glyph_range (hb_face_t face, UInt32 svg_document_index, UInt32* start_glyph_id, UInt32* end_glyph_id);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_get_svg_document_glyph_range (hb_face_t face, UInt32 svg_document_index, UInt32* start_glyph_id, UInt32* end_glyph_id);
		}
		private static Delegates.hb_ot_color_get_svg_document_glyph_range hb_ot_color_get_svg_document_glyph_range_delegate;
		internal static bool hb_ot_color_get_svg_document_glyph_range (hb_face_t face, UInt32 svg_document_index, UInt32* start_glyph_id, UInt32* end_glyph_id) =>
			(hb_ot_color_get_svg_document_glyph_range_delegate ??= GetSymbol<Delegates.hb_ot_color_get_svg_document_glyph_range> ("hb_ot_color_get_svg_document_glyph_range")).Invoke (face, svg_document_index, start_glyph_id, end_glyph_id);
		#endif

		// extern unsigned int hb_ot_color_glyph_get_layers(hb_face_t* face, hb_codepoint_t glyph, unsigned int start_offset, unsigned int* layer_count, hb_ot_color_layer_t* layers)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers);
		}
		private static Delegates.hb_ot_color_glyph_get_layers hb_ot_color_glyph_get_layers_delegate;
		internal static UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers) =>
			(hb_ot_color_glyph_get_layers_delegate ??= GetSymbol<Delegates.hb_ot_color_glyph_get_layers> ("hb_ot_color_glyph_get_layers")).Invoke (face, glyph, start_offset, layer_count, layers);
		#endif

		// extern hb_bool_t hb_ot_color_glyph_get_svg_document_index(hb_face_t* face, hb_codepoint_t glyph, unsigned int* svg_document_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_glyph_get_svg_document_index (hb_face_t face, UInt32 glyph, UInt32* svg_document_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_glyph_get_svg_document_index (hb_face_t face, UInt32 glyph, UInt32* svg_document_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_glyph_get_svg_document_index (hb_face_t face, UInt32 glyph, UInt32* svg_document_index);
		}
		private static Delegates.hb_ot_color_glyph_get_svg_document_index hb_ot_color_glyph_get_svg_document_index_delegate;
		internal static bool hb_ot_color_glyph_get_svg_document_index (hb_face_t face, UInt32 glyph, UInt32* svg_document_index) =>
			(hb_ot_color_glyph_get_svg_document_index_delegate ??= GetSymbol<Delegates.hb_ot_color_glyph_get_svg_document_index> ("hb_ot_color_glyph_get_svg_document_index")).Invoke (face, glyph, svg_document_index);
		#endif

		// extern hb_bool_t hb_ot_color_glyph_has_paint(hb_face_t* face, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_glyph_has_paint (hb_face_t face, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_glyph_has_paint (hb_face_t face, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_glyph_has_paint (hb_face_t face, UInt32 glyph);
		}
		private static Delegates.hb_ot_color_glyph_has_paint hb_ot_color_glyph_has_paint_delegate;
		internal static bool hb_ot_color_glyph_has_paint (hb_face_t face, UInt32 glyph) =>
			(hb_ot_color_glyph_has_paint_delegate ??= GetSymbol<Delegates.hb_ot_color_glyph_has_paint> ("hb_ot_color_glyph_has_paint")).Invoke (face, glyph);
		#endif

		// extern hb_blob_t* hb_ot_color_glyph_reference_png(hb_font_t* font, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_ot_color_glyph_reference_png (hb_font_t font, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_ot_color_glyph_reference_png (hb_font_t font, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_ot_color_glyph_reference_png (hb_font_t font, UInt32 glyph);
		}
		private static Delegates.hb_ot_color_glyph_reference_png hb_ot_color_glyph_reference_png_delegate;
		internal static hb_blob_t hb_ot_color_glyph_reference_png (hb_font_t font, UInt32 glyph) =>
			(hb_ot_color_glyph_reference_png_delegate ??= GetSymbol<Delegates.hb_ot_color_glyph_reference_png> ("hb_ot_color_glyph_reference_png")).Invoke (font, glyph);
		#endif

		// extern hb_blob_t* hb_ot_color_glyph_reference_svg(hb_face_t* face, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_ot_color_glyph_reference_svg (hb_face_t face, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_ot_color_glyph_reference_svg (hb_face_t face, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_ot_color_glyph_reference_svg (hb_face_t face, UInt32 glyph);
		}
		private static Delegates.hb_ot_color_glyph_reference_svg hb_ot_color_glyph_reference_svg_delegate;
		internal static hb_blob_t hb_ot_color_glyph_reference_svg (hb_face_t face, UInt32 glyph) =>
			(hb_ot_color_glyph_reference_svg_delegate ??= GetSymbol<Delegates.hb_ot_color_glyph_reference_svg> ("hb_ot_color_glyph_reference_svg")).Invoke (face, glyph);
		#endif

		// extern hb_bool_t hb_ot_color_has_layers(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_has_layers (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_layers (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_has_layers (hb_face_t face);
		}
		private static Delegates.hb_ot_color_has_layers hb_ot_color_has_layers_delegate;
		internal static bool hb_ot_color_has_layers (hb_face_t face) =>
			(hb_ot_color_has_layers_delegate ??= GetSymbol<Delegates.hb_ot_color_has_layers> ("hb_ot_color_has_layers")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_color_has_paint(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_has_paint (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_paint (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_has_paint (hb_face_t face);
		}
		private static Delegates.hb_ot_color_has_paint hb_ot_color_has_paint_delegate;
		internal static bool hb_ot_color_has_paint (hb_face_t face) =>
			(hb_ot_color_has_paint_delegate ??= GetSymbol<Delegates.hb_ot_color_has_paint> ("hb_ot_color_has_paint")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_color_has_palettes(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_has_palettes (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_palettes (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_has_palettes (hb_face_t face);
		}
		private static Delegates.hb_ot_color_has_palettes hb_ot_color_has_palettes_delegate;
		internal static bool hb_ot_color_has_palettes (hb_face_t face) =>
			(hb_ot_color_has_palettes_delegate ??= GetSymbol<Delegates.hb_ot_color_has_palettes> ("hb_ot_color_has_palettes")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_color_has_png(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_has_png (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_png (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_has_png (hb_face_t face);
		}
		private static Delegates.hb_ot_color_has_png hb_ot_color_has_png_delegate;
		internal static bool hb_ot_color_has_png (hb_face_t face) =>
			(hb_ot_color_has_png_delegate ??= GetSymbol<Delegates.hb_ot_color_has_png> ("hb_ot_color_has_png")).Invoke (face);
		#endif

		// extern hb_bool_t hb_ot_color_has_svg(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_color_has_svg (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_svg (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_color_has_svg (hb_face_t face);
		}
		private static Delegates.hb_ot_color_has_svg hb_ot_color_has_svg_delegate;
		internal static bool hb_ot_color_has_svg (hb_face_t face) =>
			(hb_ot_color_has_svg_delegate ??= GetSymbol<Delegates.hb_ot_color_has_svg> ("hb_ot_color_has_svg")).Invoke (face);
		#endif

		// extern hb_ot_name_id_t hb_ot_color_palette_color_get_name_id(hb_face_t* face, unsigned int color_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeNameId hb_ot_color_palette_color_get_name_id (hb_face_t face, UInt32 color_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameId hb_ot_color_palette_color_get_name_id (hb_face_t face, UInt32 color_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeNameId hb_ot_color_palette_color_get_name_id (hb_face_t face, UInt32 color_index);
		}
		private static Delegates.hb_ot_color_palette_color_get_name_id hb_ot_color_palette_color_get_name_id_delegate;
		internal static OpenTypeNameId hb_ot_color_palette_color_get_name_id (hb_face_t face, UInt32 color_index) =>
			(hb_ot_color_palette_color_get_name_id_delegate ??= GetSymbol<Delegates.hb_ot_color_palette_color_get_name_id> ("hb_ot_color_palette_color_get_name_id")).Invoke (face, color_index);
		#endif

		// extern unsigned int hb_ot_color_palette_get_colors(hb_face_t* face, unsigned int palette_index, unsigned int start_offset, unsigned int* color_count, hb_color_t* colors)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, HBColor* colors);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, HBColor* colors);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, HBColor* colors);
		}
		private static Delegates.hb_ot_color_palette_get_colors hb_ot_color_palette_get_colors_delegate;
		internal static UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, HBColor* colors) =>
			(hb_ot_color_palette_get_colors_delegate ??= GetSymbol<Delegates.hb_ot_color_palette_get_colors> ("hb_ot_color_palette_get_colors")).Invoke (face, palette_index, start_offset, color_count, colors);
		#endif

		// extern unsigned int hb_ot_color_palette_get_count(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_color_palette_get_count (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_palette_get_count (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_color_palette_get_count (hb_face_t face);
		}
		private static Delegates.hb_ot_color_palette_get_count hb_ot_color_palette_get_count_delegate;
		internal static UInt32 hb_ot_color_palette_get_count (hb_face_t face) =>
			(hb_ot_color_palette_get_count_delegate ??= GetSymbol<Delegates.hb_ot_color_palette_get_count> ("hb_ot_color_palette_get_count")).Invoke (face);
		#endif

		// extern hb_ot_color_palette_flags_t hb_ot_color_palette_get_flags(hb_face_t* face, unsigned int palette_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeColorPaletteFlags hb_ot_color_palette_get_flags (hb_face_t face, UInt32 palette_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeColorPaletteFlags hb_ot_color_palette_get_flags (hb_face_t face, UInt32 palette_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeColorPaletteFlags hb_ot_color_palette_get_flags (hb_face_t face, UInt32 palette_index);
		}
		private static Delegates.hb_ot_color_palette_get_flags hb_ot_color_palette_get_flags_delegate;
		internal static OpenTypeColorPaletteFlags hb_ot_color_palette_get_flags (hb_face_t face, UInt32 palette_index) =>
			(hb_ot_color_palette_get_flags_delegate ??= GetSymbol<Delegates.hb_ot_color_palette_get_flags> ("hb_ot_color_palette_get_flags")).Invoke (face, palette_index);
		#endif

		// extern hb_ot_name_id_t hb_ot_color_palette_get_name_id(hb_face_t* face, unsigned int palette_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeNameId hb_ot_color_palette_get_name_id (hb_face_t face, UInt32 palette_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameId hb_ot_color_palette_get_name_id (hb_face_t face, UInt32 palette_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeNameId hb_ot_color_palette_get_name_id (hb_face_t face, UInt32 palette_index);
		}
		private static Delegates.hb_ot_color_palette_get_name_id hb_ot_color_palette_get_name_id_delegate;
		internal static OpenTypeNameId hb_ot_color_palette_get_name_id (hb_face_t face, UInt32 palette_index) =>
			(hb_ot_color_palette_get_name_id_delegate ??= GetSymbol<Delegates.hb_ot_color_palette_get_name_id> ("hb_ot_color_palette_get_name_id")).Invoke (face, palette_index);
		#endif

		#endregion

	}
}
