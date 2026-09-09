using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-font.h

		// extern void hb_font_add_glyph_origin_for_direction(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_add_glyph_origin_for_direction hb_font_add_glyph_origin_for_direction_delegate;
		internal static void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y) =>
			(hb_font_add_glyph_origin_for_direction_delegate ??= GetSymbol<Delegates.hb_font_add_glyph_origin_for_direction> ("hb_font_add_glyph_origin_for_direction")).Invoke (font, glyph, direction, x, y);
		#endif

		// extern void hb_font_changed(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_changed (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_changed (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_changed (hb_font_t font);
		}
		private static Delegates.hb_font_changed hb_font_changed_delegate;
		internal static void hb_font_changed (hb_font_t font) =>
			(hb_font_changed_delegate ??= GetSymbol<Delegates.hb_font_changed> ("hb_font_changed")).Invoke (font);
		#endif

		// extern hb_font_t* hb_font_create(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_t hb_font_create (hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_create (hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_t hb_font_create (hb_face_t face);
		}
		private static Delegates.hb_font_create hb_font_create_delegate;
		internal static hb_font_t hb_font_create (hb_face_t face) =>
			(hb_font_create_delegate ??= GetSymbol<Delegates.hb_font_create> ("hb_font_create")).Invoke (face);
		#endif

		// extern hb_font_t* hb_font_create_sub_font(hb_font_t* parent)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_t hb_font_create_sub_font (hb_font_t parent);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_create_sub_font (hb_font_t parent);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_t hb_font_create_sub_font (hb_font_t parent);
		}
		private static Delegates.hb_font_create_sub_font hb_font_create_sub_font_delegate;
		internal static hb_font_t hb_font_create_sub_font (hb_font_t parent) =>
			(hb_font_create_sub_font_delegate ??= GetSymbol<Delegates.hb_font_create_sub_font> ("hb_font_create_sub_font")).Invoke (parent);
		#endif

		// extern void hb_font_destroy(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_destroy (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_destroy (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_destroy (hb_font_t font);
		}
		private static Delegates.hb_font_destroy hb_font_destroy_delegate;
		internal static void hb_font_destroy (hb_font_t font) =>
			(hb_font_destroy_delegate ??= GetSymbol<Delegates.hb_font_destroy> ("hb_font_destroy")).Invoke (font);
		#endif

		// extern hb_font_funcs_t* hb_font_funcs_create()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_funcs_t hb_font_funcs_create ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_funcs_t hb_font_funcs_create ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_funcs_t hb_font_funcs_create ();
		}
		private static Delegates.hb_font_funcs_create hb_font_funcs_create_delegate;
		internal static hb_font_funcs_t hb_font_funcs_create () =>
			(hb_font_funcs_create_delegate ??= GetSymbol<Delegates.hb_font_funcs_create> ("hb_font_funcs_create")).Invoke ();
		#endif

		// extern void hb_font_funcs_destroy(hb_font_funcs_t* ffuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_destroy (hb_font_funcs_t ffuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_destroy (hb_font_funcs_t ffuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_destroy (hb_font_funcs_t ffuncs);
		}
		private static Delegates.hb_font_funcs_destroy hb_font_funcs_destroy_delegate;
		internal static void hb_font_funcs_destroy (hb_font_funcs_t ffuncs) =>
			(hb_font_funcs_destroy_delegate ??= GetSymbol<Delegates.hb_font_funcs_destroy> ("hb_font_funcs_destroy")).Invoke (ffuncs);
		#endif

		// extern hb_font_funcs_t* hb_font_funcs_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_funcs_t hb_font_funcs_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_funcs_t hb_font_funcs_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_funcs_t hb_font_funcs_get_empty ();
		}
		private static Delegates.hb_font_funcs_get_empty hb_font_funcs_get_empty_delegate;
		internal static hb_font_funcs_t hb_font_funcs_get_empty () =>
			(hb_font_funcs_get_empty_delegate ??= GetSymbol<Delegates.hb_font_funcs_get_empty> ("hb_font_funcs_get_empty")).Invoke ();
		#endif

		// extern hb_bool_t hb_font_funcs_is_immutable(hb_font_funcs_t* ffuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_funcs_is_immutable (hb_font_funcs_t ffuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_funcs_is_immutable (hb_font_funcs_t ffuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_funcs_is_immutable (hb_font_funcs_t ffuncs);
		}
		private static Delegates.hb_font_funcs_is_immutable hb_font_funcs_is_immutable_delegate;
		internal static bool hb_font_funcs_is_immutable (hb_font_funcs_t ffuncs) =>
			(hb_font_funcs_is_immutable_delegate ??= GetSymbol<Delegates.hb_font_funcs_is_immutable> ("hb_font_funcs_is_immutable")).Invoke (ffuncs);
		#endif

		// extern void hb_font_funcs_make_immutable(hb_font_funcs_t* ffuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_make_immutable (hb_font_funcs_t ffuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_make_immutable (hb_font_funcs_t ffuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_make_immutable (hb_font_funcs_t ffuncs);
		}
		private static Delegates.hb_font_funcs_make_immutable hb_font_funcs_make_immutable_delegate;
		internal static void hb_font_funcs_make_immutable (hb_font_funcs_t ffuncs) =>
			(hb_font_funcs_make_immutable_delegate ??= GetSymbol<Delegates.hb_font_funcs_make_immutable> ("hb_font_funcs_make_immutable")).Invoke (ffuncs);
		#endif

		// extern hb_font_funcs_t* hb_font_funcs_reference(hb_font_funcs_t* ffuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_funcs_t hb_font_funcs_reference (hb_font_funcs_t ffuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_funcs_t hb_font_funcs_reference (hb_font_funcs_t ffuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_funcs_t hb_font_funcs_reference (hb_font_funcs_t ffuncs);
		}
		private static Delegates.hb_font_funcs_reference hb_font_funcs_reference_delegate;
		internal static hb_font_funcs_t hb_font_funcs_reference (hb_font_funcs_t ffuncs) =>
			(hb_font_funcs_reference_delegate ??= GetSymbol<Delegates.hb_font_funcs_reference> ("hb_font_funcs_reference")).Invoke (ffuncs);
		#endif

		// extern void hb_font_funcs_set_font_h_extents_func(hb_font_funcs_t* ffuncs, hb_font_get_font_h_extents_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_font_h_extents_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_font_h_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_font_h_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_font_h_extents_func hb_font_funcs_set_font_h_extents_func_delegate;
		internal static void hb_font_funcs_set_font_h_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_font_h_extents_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_font_h_extents_func> ("hb_font_funcs_set_font_h_extents_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_font_v_extents_func(hb_font_funcs_t* ffuncs, hb_font_get_font_v_extents_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_font_v_extents_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_font_v_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_font_v_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_font_v_extents_func hb_font_funcs_set_font_v_extents_func_delegate;
		internal static void hb_font_funcs_set_font_v_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_font_v_extents_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_font_v_extents_func> ("hb_font_funcs_set_font_v_extents_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_contour_point_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_contour_point_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_contour_point_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_contour_point_func (hb_font_funcs_t ffuncs, FontGetGlyphContourPointProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_contour_point_func (hb_font_funcs_t ffuncs, FontGetGlyphContourPointProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_contour_point_func hb_font_funcs_set_glyph_contour_point_func_delegate;
		internal static void hb_font_funcs_set_glyph_contour_point_func (hb_font_funcs_t ffuncs, FontGetGlyphContourPointProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_contour_point_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_contour_point_func> ("hb_font_funcs_set_glyph_contour_point_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_extents_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_extents_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_extents_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_extents_func (hb_font_funcs_t ffuncs, FontGetGlyphExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_extents_func (hb_font_funcs_t ffuncs, FontGetGlyphExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_extents_func hb_font_funcs_set_glyph_extents_func_delegate;
		internal static void hb_font_funcs_set_glyph_extents_func (hb_font_funcs_t ffuncs, FontGetGlyphExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_extents_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_extents_func> ("hb_font_funcs_set_glyph_extents_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_from_name_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_from_name_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_from_name_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_from_name_func (hb_font_funcs_t ffuncs, FontGetGlyphFromNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_from_name_func (hb_font_funcs_t ffuncs, FontGetGlyphFromNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_from_name_func hb_font_funcs_set_glyph_from_name_func_delegate;
		internal static void hb_font_funcs_set_glyph_from_name_func (hb_font_funcs_t ffuncs, FontGetGlyphFromNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_from_name_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_from_name_func> ("hb_font_funcs_set_glyph_from_name_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_h_advance_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_h_advance_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_h_advance_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_h_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_h_advance_func hb_font_funcs_set_glyph_h_advance_func_delegate;
		internal static void hb_font_funcs_set_glyph_h_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_h_advance_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_h_advance_func> ("hb_font_funcs_set_glyph_h_advance_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_h_advances_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_h_advances_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_h_advances_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_h_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_h_advances_func hb_font_funcs_set_glyph_h_advances_func_delegate;
		internal static void hb_font_funcs_set_glyph_h_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_h_advances_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_h_advances_func> ("hb_font_funcs_set_glyph_h_advances_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_h_kerning_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_h_kerning_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_h_kerning_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_kerning_func (hb_font_funcs_t ffuncs, FontGetGlyphKerningProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_h_kerning_func (hb_font_funcs_t ffuncs, FontGetGlyphKerningProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_h_kerning_func hb_font_funcs_set_glyph_h_kerning_func_delegate;
		internal static void hb_font_funcs_set_glyph_h_kerning_func (hb_font_funcs_t ffuncs, FontGetGlyphKerningProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_h_kerning_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_h_kerning_func> ("hb_font_funcs_set_glyph_h_kerning_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_h_origin_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_h_origin_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_h_origin_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_h_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_h_origin_func hb_font_funcs_set_glyph_h_origin_func_delegate;
		internal static void hb_font_funcs_set_glyph_h_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_h_origin_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_h_origin_func> ("hb_font_funcs_set_glyph_h_origin_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_name_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_name_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_name_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_name_func (hb_font_funcs_t ffuncs, FontGetGlyphNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_name_func (hb_font_funcs_t ffuncs, FontGetGlyphNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_name_func hb_font_funcs_set_glyph_name_func_delegate;
		internal static void hb_font_funcs_set_glyph_name_func (hb_font_funcs_t ffuncs, FontGetGlyphNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_name_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_name_func> ("hb_font_funcs_set_glyph_name_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_v_advance_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_v_advance_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_v_advance_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_v_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_v_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_v_advance_func hb_font_funcs_set_glyph_v_advance_func_delegate;
		internal static void hb_font_funcs_set_glyph_v_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_v_advance_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_v_advance_func> ("hb_font_funcs_set_glyph_v_advance_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_v_advances_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_v_advances_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_v_advances_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_v_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_v_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_v_advances_func hb_font_funcs_set_glyph_v_advances_func_delegate;
		internal static void hb_font_funcs_set_glyph_v_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_v_advances_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_v_advances_func> ("hb_font_funcs_set_glyph_v_advances_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_glyph_v_origin_func(hb_font_funcs_t* ffuncs, hb_font_get_glyph_v_origin_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_glyph_v_origin_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_v_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_glyph_v_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_glyph_v_origin_func hb_font_funcs_set_glyph_v_origin_func_delegate;
		internal static void hb_font_funcs_set_glyph_v_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_glyph_v_origin_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_glyph_v_origin_func> ("hb_font_funcs_set_glyph_v_origin_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_nominal_glyph_func(hb_font_funcs_t* ffuncs, hb_font_get_nominal_glyph_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_nominal_glyph_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_nominal_glyph_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_nominal_glyph_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_nominal_glyph_func hb_font_funcs_set_nominal_glyph_func_delegate;
		internal static void hb_font_funcs_set_nominal_glyph_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_nominal_glyph_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_nominal_glyph_func> ("hb_font_funcs_set_nominal_glyph_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_nominal_glyphs_func(hb_font_funcs_t* ffuncs, hb_font_get_nominal_glyphs_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_nominal_glyphs_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_nominal_glyphs_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_nominal_glyphs_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_nominal_glyphs_func hb_font_funcs_set_nominal_glyphs_func_delegate;
		internal static void hb_font_funcs_set_nominal_glyphs_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_nominal_glyphs_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_nominal_glyphs_func> ("hb_font_funcs_set_nominal_glyphs_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern void hb_font_funcs_set_variation_glyph_func(hb_font_funcs_t* ffuncs, hb_font_get_variation_glyph_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_funcs_set_variation_glyph_func (hb_font_funcs_t ffuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_variation_glyph_func (hb_font_funcs_t ffuncs, FontGetVariationGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_funcs_set_variation_glyph_func (hb_font_funcs_t ffuncs, FontGetVariationGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_funcs_set_variation_glyph_func hb_font_funcs_set_variation_glyph_func_delegate;
		internal static void hb_font_funcs_set_variation_glyph_func (hb_font_funcs_t ffuncs, FontGetVariationGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_font_funcs_set_variation_glyph_func_delegate ??= GetSymbol<Delegates.hb_font_funcs_set_variation_glyph_func> ("hb_font_funcs_set_variation_glyph_func")).Invoke (ffuncs, func, user_data, destroy);
		#endif

		// extern hb_font_t* hb_font_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_t hb_font_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_t hb_font_get_empty ();
		}
		private static Delegates.hb_font_get_empty hb_font_get_empty_delegate;
		internal static hb_font_t hb_font_get_empty () =>
			(hb_font_get_empty_delegate ??= GetSymbol<Delegates.hb_font_get_empty> ("hb_font_get_empty")).Invoke ();
		#endif

		// extern void hb_font_get_extents_for_direction(hb_font_t* font, hb_direction_t direction, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_extents_for_direction (hb_font_t font, Direction direction, FontExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_extents_for_direction (hb_font_t font, Direction direction, FontExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_extents_for_direction (hb_font_t font, Direction direction, FontExtents* extents);
		}
		private static Delegates.hb_font_get_extents_for_direction hb_font_get_extents_for_direction_delegate;
		internal static void hb_font_get_extents_for_direction (hb_font_t font, Direction direction, FontExtents* extents) =>
			(hb_font_get_extents_for_direction_delegate ??= GetSymbol<Delegates.hb_font_get_extents_for_direction> ("hb_font_get_extents_for_direction")).Invoke (font, direction, extents);
		#endif

		// extern hb_face_t* hb_font_get_face(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_face_t hb_font_get_face (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_font_get_face (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_face_t hb_font_get_face (hb_font_t font);
		}
		private static Delegates.hb_font_get_face hb_font_get_face_delegate;
		internal static hb_face_t hb_font_get_face (hb_font_t font) =>
			(hb_font_get_face_delegate ??= GetSymbol<Delegates.hb_font_get_face> ("hb_font_get_face")).Invoke (font);
		#endif

		// extern hb_bool_t hb_font_get_glyph(hb_font_t* font, hb_codepoint_t unicode, hb_codepoint_t variation_selector, hb_codepoint_t* glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
		}
		private static Delegates.hb_font_get_glyph hb_font_get_glyph_delegate;
		internal static bool hb_font_get_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph) =>
			(hb_font_get_glyph_delegate ??= GetSymbol<Delegates.hb_font_get_glyph> ("hb_font_get_glyph")).Invoke (font, unicode, variation_selector, glyph);
		#endif

		// extern void hb_font_get_glyph_advance_for_direction(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_glyph_advance_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_advance_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_glyph_advance_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_advance_for_direction hb_font_get_glyph_advance_for_direction_delegate;
		internal static void hb_font_get_glyph_advance_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y) =>
			(hb_font_get_glyph_advance_for_direction_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_advance_for_direction> ("hb_font_get_glyph_advance_for_direction")).Invoke (font, glyph, direction, x, y);
		#endif

		// extern void hb_font_get_glyph_advances_for_direction(hb_font_t* font, hb_direction_t direction, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_advance, unsigned int advance_stride)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_glyph_advances_for_direction (hb_font_t font, Direction direction, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_advances_for_direction (hb_font_t font, Direction direction, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_glyph_advances_for_direction (hb_font_t font, Direction direction, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		}
		private static Delegates.hb_font_get_glyph_advances_for_direction hb_font_get_glyph_advances_for_direction_delegate;
		internal static void hb_font_get_glyph_advances_for_direction (hb_font_t font, Direction direction, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride) =>
			(hb_font_get_glyph_advances_for_direction_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_advances_for_direction> ("hb_font_get_glyph_advances_for_direction")).Invoke (font, direction, count, first_glyph, glyph_stride, first_advance, advance_stride);
		#endif

		// extern hb_bool_t hb_font_get_glyph_contour_point(hb_font_t* font, hb_codepoint_t glyph, unsigned int point_index, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_contour_point (hb_font_t font, UInt32 glyph, UInt32 point_index, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_contour_point (hb_font_t font, UInt32 glyph, UInt32 point_index, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_contour_point (hb_font_t font, UInt32 glyph, UInt32 point_index, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_contour_point hb_font_get_glyph_contour_point_delegate;
		internal static bool hb_font_get_glyph_contour_point (hb_font_t font, UInt32 glyph, UInt32 point_index, Int32* x, Int32* y) =>
			(hb_font_get_glyph_contour_point_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_contour_point> ("hb_font_get_glyph_contour_point")).Invoke (font, glyph, point_index, x, y);
		#endif

		// extern hb_bool_t hb_font_get_glyph_contour_point_for_origin(hb_font_t* font, hb_codepoint_t glyph, unsigned int point_index, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_contour_point_for_origin (hb_font_t font, UInt32 glyph, UInt32 point_index, Direction direction, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_contour_point_for_origin (hb_font_t font, UInt32 glyph, UInt32 point_index, Direction direction, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_contour_point_for_origin (hb_font_t font, UInt32 glyph, UInt32 point_index, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_contour_point_for_origin hb_font_get_glyph_contour_point_for_origin_delegate;
		internal static bool hb_font_get_glyph_contour_point_for_origin (hb_font_t font, UInt32 glyph, UInt32 point_index, Direction direction, Int32* x, Int32* y) =>
			(hb_font_get_glyph_contour_point_for_origin_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_contour_point_for_origin> ("hb_font_get_glyph_contour_point_for_origin")).Invoke (font, glyph, point_index, direction, x, y);
		#endif

		// extern hb_bool_t hb_font_get_glyph_extents(hb_font_t* font, hb_codepoint_t glyph, hb_glyph_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_extents (hb_font_t font, UInt32 glyph, GlyphExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_extents (hb_font_t font, UInt32 glyph, GlyphExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_extents (hb_font_t font, UInt32 glyph, GlyphExtents* extents);
		}
		private static Delegates.hb_font_get_glyph_extents hb_font_get_glyph_extents_delegate;
		internal static bool hb_font_get_glyph_extents (hb_font_t font, UInt32 glyph, GlyphExtents* extents) =>
			(hb_font_get_glyph_extents_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_extents> ("hb_font_get_glyph_extents")).Invoke (font, glyph, extents);
		#endif

		// extern hb_bool_t hb_font_get_glyph_extents_for_origin(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, hb_glyph_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_extents_for_origin (hb_font_t font, UInt32 glyph, Direction direction, GlyphExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_extents_for_origin (hb_font_t font, UInt32 glyph, Direction direction, GlyphExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_extents_for_origin (hb_font_t font, UInt32 glyph, Direction direction, GlyphExtents* extents);
		}
		private static Delegates.hb_font_get_glyph_extents_for_origin hb_font_get_glyph_extents_for_origin_delegate;
		internal static bool hb_font_get_glyph_extents_for_origin (hb_font_t font, UInt32 glyph, Direction direction, GlyphExtents* extents) =>
			(hb_font_get_glyph_extents_for_origin_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_extents_for_origin> ("hb_font_get_glyph_extents_for_origin")).Invoke (font, glyph, direction, extents);
		#endif

		// extern hb_bool_t hb_font_get_glyph_from_name(hb_font_t* font, const char* name, int len, hb_codepoint_t* glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_from_name (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String name, Int32 len, UInt32* glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_from_name (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String name, Int32 len, UInt32* glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_from_name (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String name, Int32 len, UInt32* glyph);
		}
		private static Delegates.hb_font_get_glyph_from_name hb_font_get_glyph_from_name_delegate;
		internal static bool hb_font_get_glyph_from_name (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String name, Int32 len, UInt32* glyph) =>
			(hb_font_get_glyph_from_name_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_from_name> ("hb_font_get_glyph_from_name")).Invoke (font, name, len, glyph);
		#endif

		// extern hb_position_t hb_font_get_glyph_h_advance(hb_font_t* font, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_font_get_glyph_h_advance (hb_font_t font, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_font_get_glyph_h_advance (hb_font_t font, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_font_get_glyph_h_advance (hb_font_t font, UInt32 glyph);
		}
		private static Delegates.hb_font_get_glyph_h_advance hb_font_get_glyph_h_advance_delegate;
		internal static Int32 hb_font_get_glyph_h_advance (hb_font_t font, UInt32 glyph) =>
			(hb_font_get_glyph_h_advance_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_h_advance> ("hb_font_get_glyph_h_advance")).Invoke (font, glyph);
		#endif

		// extern void hb_font_get_glyph_h_advances(hb_font_t* font, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_advance, unsigned int advance_stride)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_glyph_h_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_h_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_glyph_h_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		}
		private static Delegates.hb_font_get_glyph_h_advances hb_font_get_glyph_h_advances_delegate;
		internal static void hb_font_get_glyph_h_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride) =>
			(hb_font_get_glyph_h_advances_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_h_advances> ("hb_font_get_glyph_h_advances")).Invoke (font, count, first_glyph, glyph_stride, first_advance, advance_stride);
		#endif

		// extern hb_position_t hb_font_get_glyph_h_kerning(hb_font_t* font, hb_codepoint_t left_glyph, hb_codepoint_t right_glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_font_get_glyph_h_kerning (hb_font_t font, UInt32 left_glyph, UInt32 right_glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_font_get_glyph_h_kerning (hb_font_t font, UInt32 left_glyph, UInt32 right_glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_font_get_glyph_h_kerning (hb_font_t font, UInt32 left_glyph, UInt32 right_glyph);
		}
		private static Delegates.hb_font_get_glyph_h_kerning hb_font_get_glyph_h_kerning_delegate;
		internal static Int32 hb_font_get_glyph_h_kerning (hb_font_t font, UInt32 left_glyph, UInt32 right_glyph) =>
			(hb_font_get_glyph_h_kerning_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_h_kerning> ("hb_font_get_glyph_h_kerning")).Invoke (font, left_glyph, right_glyph);
		#endif

		// extern hb_bool_t hb_font_get_glyph_h_origin(hb_font_t* font, hb_codepoint_t glyph, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_h_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_h_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_h_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_h_origin hb_font_get_glyph_h_origin_delegate;
		internal static bool hb_font_get_glyph_h_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y) =>
			(hb_font_get_glyph_h_origin_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_h_origin> ("hb_font_get_glyph_h_origin")).Invoke (font, glyph, x, y);
		#endif

		// extern hb_bool_t hb_font_get_glyph_h_origins(hb_font_t* font, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_x, unsigned int x_stride, hb_position_t* first_y, unsigned int y_stride)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_h_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_h_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_h_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride);
		}
		private static Delegates.hb_font_get_glyph_h_origins hb_font_get_glyph_h_origins_delegate;
		internal static bool hb_font_get_glyph_h_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride) =>
			(hb_font_get_glyph_h_origins_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_h_origins> ("hb_font_get_glyph_h_origins")).Invoke (font, count, first_glyph, glyph_stride, first_x, x_stride, first_y, y_stride);
		#endif

		// extern void hb_font_get_glyph_kerning_for_direction(hb_font_t* font, hb_codepoint_t first_glyph, hb_codepoint_t second_glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_glyph_kerning_for_direction (hb_font_t font, UInt32 first_glyph, UInt32 second_glyph, Direction direction, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_kerning_for_direction (hb_font_t font, UInt32 first_glyph, UInt32 second_glyph, Direction direction, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_glyph_kerning_for_direction (hb_font_t font, UInt32 first_glyph, UInt32 second_glyph, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_kerning_for_direction hb_font_get_glyph_kerning_for_direction_delegate;
		internal static void hb_font_get_glyph_kerning_for_direction (hb_font_t font, UInt32 first_glyph, UInt32 second_glyph, Direction direction, Int32* x, Int32* y) =>
			(hb_font_get_glyph_kerning_for_direction_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_kerning_for_direction> ("hb_font_get_glyph_kerning_for_direction")).Invoke (font, first_glyph, second_glyph, direction, x, y);
		#endif

		// extern hb_bool_t hb_font_get_glyph_name(hb_font_t* font, hb_codepoint_t glyph, char* name, unsigned int size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_name (hb_font_t font, UInt32 glyph, /* char */ void* name, UInt32 size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_name (hb_font_t font, UInt32 glyph, /* char */ void* name, UInt32 size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_name (hb_font_t font, UInt32 glyph, /* char */ void* name, UInt32 size);
		}
		private static Delegates.hb_font_get_glyph_name hb_font_get_glyph_name_delegate;
		internal static bool hb_font_get_glyph_name (hb_font_t font, UInt32 glyph, /* char */ void* name, UInt32 size) =>
			(hb_font_get_glyph_name_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_name> ("hb_font_get_glyph_name")).Invoke (font, glyph, name, size);
		#endif

		// extern void hb_font_get_glyph_origin_for_direction(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_origin_for_direction hb_font_get_glyph_origin_for_direction_delegate;
		internal static void hb_font_get_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y) =>
			(hb_font_get_glyph_origin_for_direction_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_origin_for_direction> ("hb_font_get_glyph_origin_for_direction")).Invoke (font, glyph, direction, x, y);
		#endif

		// extern hb_position_t hb_font_get_glyph_v_advance(hb_font_t* font, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_font_get_glyph_v_advance (hb_font_t font, UInt32 glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_font_get_glyph_v_advance (hb_font_t font, UInt32 glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_font_get_glyph_v_advance (hb_font_t font, UInt32 glyph);
		}
		private static Delegates.hb_font_get_glyph_v_advance hb_font_get_glyph_v_advance_delegate;
		internal static Int32 hb_font_get_glyph_v_advance (hb_font_t font, UInt32 glyph) =>
			(hb_font_get_glyph_v_advance_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_v_advance> ("hb_font_get_glyph_v_advance")).Invoke (font, glyph);
		#endif

		// extern void hb_font_get_glyph_v_advances(hb_font_t* font, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_advance, unsigned int advance_stride)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_glyph_v_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_v_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_glyph_v_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
		}
		private static Delegates.hb_font_get_glyph_v_advances hb_font_get_glyph_v_advances_delegate;
		internal static void hb_font_get_glyph_v_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride) =>
			(hb_font_get_glyph_v_advances_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_v_advances> ("hb_font_get_glyph_v_advances")).Invoke (font, count, first_glyph, glyph_stride, first_advance, advance_stride);
		#endif

		// extern hb_bool_t hb_font_get_glyph_v_origin(hb_font_t* font, hb_codepoint_t glyph, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_v_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_v_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_v_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_get_glyph_v_origin hb_font_get_glyph_v_origin_delegate;
		internal static bool hb_font_get_glyph_v_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y) =>
			(hb_font_get_glyph_v_origin_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_v_origin> ("hb_font_get_glyph_v_origin")).Invoke (font, glyph, x, y);
		#endif

		// extern hb_bool_t hb_font_get_glyph_v_origins(hb_font_t* font, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_x, unsigned int x_stride, hb_position_t* first_y, unsigned int y_stride)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_glyph_v_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_v_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_glyph_v_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride);
		}
		private static Delegates.hb_font_get_glyph_v_origins hb_font_get_glyph_v_origins_delegate;
		internal static bool hb_font_get_glyph_v_origins (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_x, UInt32 x_stride, Int32* first_y, UInt32 y_stride) =>
			(hb_font_get_glyph_v_origins_delegate ??= GetSymbol<Delegates.hb_font_get_glyph_v_origins> ("hb_font_get_glyph_v_origins")).Invoke (font, count, first_glyph, glyph_stride, first_x, x_stride, first_y, y_stride);
		#endif

		// extern hb_bool_t hb_font_get_h_extents(hb_font_t* font, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_h_extents (hb_font_t font, FontExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_h_extents (hb_font_t font, FontExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_h_extents (hb_font_t font, FontExtents* extents);
		}
		private static Delegates.hb_font_get_h_extents hb_font_get_h_extents_delegate;
		internal static bool hb_font_get_h_extents (hb_font_t font, FontExtents* extents) =>
			(hb_font_get_h_extents_delegate ??= GetSymbol<Delegates.hb_font_get_h_extents> ("hb_font_get_h_extents")).Invoke (font, extents);
		#endif

		// extern hb_bool_t hb_font_get_nominal_glyph(hb_font_t* font, hb_codepoint_t unicode, hb_codepoint_t* glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_nominal_glyph (hb_font_t font, UInt32 unicode, UInt32* glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_nominal_glyph (hb_font_t font, UInt32 unicode, UInt32* glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_nominal_glyph (hb_font_t font, UInt32 unicode, UInt32* glyph);
		}
		private static Delegates.hb_font_get_nominal_glyph hb_font_get_nominal_glyph_delegate;
		internal static bool hb_font_get_nominal_glyph (hb_font_t font, UInt32 unicode, UInt32* glyph) =>
			(hb_font_get_nominal_glyph_delegate ??= GetSymbol<Delegates.hb_font_get_nominal_glyph> ("hb_font_get_nominal_glyph")).Invoke (font, unicode, glyph);
		#endif

		// extern unsigned int hb_font_get_nominal_glyphs(hb_font_t* font, unsigned int count, const hb_codepoint_t* first_unicode, unsigned int unicode_stride, hb_codepoint_t* first_glyph, unsigned int glyph_stride)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_font_get_nominal_glyphs (hb_font_t font, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_font_get_nominal_glyphs (hb_font_t font, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_font_get_nominal_glyphs (hb_font_t font, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride);
		}
		private static Delegates.hb_font_get_nominal_glyphs hb_font_get_nominal_glyphs_delegate;
		internal static UInt32 hb_font_get_nominal_glyphs (hb_font_t font, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride) =>
			(hb_font_get_nominal_glyphs_delegate ??= GetSymbol<Delegates.hb_font_get_nominal_glyphs> ("hb_font_get_nominal_glyphs")).Invoke (font, count, first_unicode, unicode_stride, first_glyph, glyph_stride);
		#endif

		// extern hb_font_t* hb_font_get_parent(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_t hb_font_get_parent (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_get_parent (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_t hb_font_get_parent (hb_font_t font);
		}
		private static Delegates.hb_font_get_parent hb_font_get_parent_delegate;
		internal static hb_font_t hb_font_get_parent (hb_font_t font) =>
			(hb_font_get_parent_delegate ??= GetSymbol<Delegates.hb_font_get_parent> ("hb_font_get_parent")).Invoke (font);
		#endif

		// extern void hb_font_get_ppem(hb_font_t* font, unsigned int* x_ppem, unsigned int* y_ppem)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_ppem (hb_font_t font, UInt32* x_ppem, UInt32* y_ppem);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_ppem (hb_font_t font, UInt32* x_ppem, UInt32* y_ppem);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_ppem (hb_font_t font, UInt32* x_ppem, UInt32* y_ppem);
		}
		private static Delegates.hb_font_get_ppem hb_font_get_ppem_delegate;
		internal static void hb_font_get_ppem (hb_font_t font, UInt32* x_ppem, UInt32* y_ppem) =>
			(hb_font_get_ppem_delegate ??= GetSymbol<Delegates.hb_font_get_ppem> ("hb_font_get_ppem")).Invoke (font, x_ppem, y_ppem);
		#endif

		// extern float hb_font_get_ptem(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Single hb_font_get_ptem (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single hb_font_get_ptem (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single hb_font_get_ptem (hb_font_t font);
		}
		private static Delegates.hb_font_get_ptem hb_font_get_ptem_delegate;
		internal static Single hb_font_get_ptem (hb_font_t font) =>
			(hb_font_get_ptem_delegate ??= GetSymbol<Delegates.hb_font_get_ptem> ("hb_font_get_ptem")).Invoke (font);
		#endif

		// extern void hb_font_get_scale(hb_font_t* font, int* x_scale, int* y_scale)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale);
		}
		private static Delegates.hb_font_get_scale hb_font_get_scale_delegate;
		internal static void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale) =>
			(hb_font_get_scale_delegate ??= GetSymbol<Delegates.hb_font_get_scale> ("hb_font_get_scale")).Invoke (font, x_scale, y_scale);
		#endif

		// extern unsigned int hb_font_get_serial(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_font_get_serial (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_font_get_serial (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_font_get_serial (hb_font_t font);
		}
		private static Delegates.hb_font_get_serial hb_font_get_serial_delegate;
		internal static UInt32 hb_font_get_serial (hb_font_t font) =>
			(hb_font_get_serial_delegate ??= GetSymbol<Delegates.hb_font_get_serial> ("hb_font_get_serial")).Invoke (font);
		#endif

		// extern void hb_font_get_synthetic_bold(hb_font_t* font, float* x_embolden, float* y_embolden, hb_bool_t* in_place)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_get_synthetic_bold (hb_font_t font, Single* x_embolden, Single* y_embolden, Boolean* in_place);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_synthetic_bold (hb_font_t font, Single* x_embolden, Single* y_embolden, Boolean* in_place);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_synthetic_bold (hb_font_t font, Single* x_embolden, Single* y_embolden, Boolean* in_place);
		}
		private static Delegates.hb_font_get_synthetic_bold hb_font_get_synthetic_bold_delegate;
		internal static void hb_font_get_synthetic_bold (hb_font_t font, Single* x_embolden, Single* y_embolden, Boolean* in_place) =>
			(hb_font_get_synthetic_bold_delegate ??= GetSymbol<Delegates.hb_font_get_synthetic_bold> ("hb_font_get_synthetic_bold")).Invoke (font, x_embolden, y_embolden, in_place);
		#endif

		// extern float hb_font_get_synthetic_slant(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Single hb_font_get_synthetic_slant (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single hb_font_get_synthetic_slant (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single hb_font_get_synthetic_slant (hb_font_t font);
		}
		private static Delegates.hb_font_get_synthetic_slant hb_font_get_synthetic_slant_delegate;
		internal static Single hb_font_get_synthetic_slant (hb_font_t font) =>
			(hb_font_get_synthetic_slant_delegate ??= GetSymbol<Delegates.hb_font_get_synthetic_slant> ("hb_font_get_synthetic_slant")).Invoke (font);
		#endif

		// extern hb_bool_t hb_font_get_v_extents(hb_font_t* font, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_v_extents (hb_font_t font, FontExtents* extents);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_v_extents (hb_font_t font, FontExtents* extents);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_v_extents (hb_font_t font, FontExtents* extents);
		}
		private static Delegates.hb_font_get_v_extents hb_font_get_v_extents_delegate;
		internal static bool hb_font_get_v_extents (hb_font_t font, FontExtents* extents) =>
			(hb_font_get_v_extents_delegate ??= GetSymbol<Delegates.hb_font_get_v_extents> ("hb_font_get_v_extents")).Invoke (font, extents);
		#endif

		// extern const float* hb_font_get_var_coords_design(hb_font_t* font, unsigned int* length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Single* hb_font_get_var_coords_design (hb_font_t font, UInt32* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single* hb_font_get_var_coords_design (hb_font_t font, UInt32* length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single* hb_font_get_var_coords_design (hb_font_t font, UInt32* length);
		}
		private static Delegates.hb_font_get_var_coords_design hb_font_get_var_coords_design_delegate;
		internal static Single* hb_font_get_var_coords_design (hb_font_t font, UInt32* length) =>
			(hb_font_get_var_coords_design_delegate ??= GetSymbol<Delegates.hb_font_get_var_coords_design> ("hb_font_get_var_coords_design")).Invoke (font, length);
		#endif

		// extern const int* hb_font_get_var_coords_normalized(hb_font_t* font, unsigned int* length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length);
		}
		private static Delegates.hb_font_get_var_coords_normalized hb_font_get_var_coords_normalized_delegate;
		internal static Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length) =>
			(hb_font_get_var_coords_normalized_delegate ??= GetSymbol<Delegates.hb_font_get_var_coords_normalized> ("hb_font_get_var_coords_normalized")).Invoke (font, length);
		#endif

		// extern unsigned int hb_font_get_var_named_instance(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_font_get_var_named_instance (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_font_get_var_named_instance (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_font_get_var_named_instance (hb_font_t font);
		}
		private static Delegates.hb_font_get_var_named_instance hb_font_get_var_named_instance_delegate;
		internal static UInt32 hb_font_get_var_named_instance (hb_font_t font) =>
			(hb_font_get_var_named_instance_delegate ??= GetSymbol<Delegates.hb_font_get_var_named_instance> ("hb_font_get_var_named_instance")).Invoke (font);
		#endif

		// extern hb_bool_t hb_font_get_variation_glyph(hb_font_t* font, hb_codepoint_t unicode, hb_codepoint_t variation_selector, hb_codepoint_t* glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_get_variation_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_variation_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_get_variation_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
		}
		private static Delegates.hb_font_get_variation_glyph hb_font_get_variation_glyph_delegate;
		internal static bool hb_font_get_variation_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph) =>
			(hb_font_get_variation_glyph_delegate ??= GetSymbol<Delegates.hb_font_get_variation_glyph> ("hb_font_get_variation_glyph")).Invoke (font, unicode, variation_selector, glyph);
		#endif

		// extern hb_bool_t hb_font_glyph_from_string(hb_font_t* font, const char* s, int len, hb_codepoint_t* glyph)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_glyph_from_string (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String s, Int32 len, UInt32* glyph);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_glyph_from_string (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String s, Int32 len, UInt32* glyph);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_glyph_from_string (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String s, Int32 len, UInt32* glyph);
		}
		private static Delegates.hb_font_glyph_from_string hb_font_glyph_from_string_delegate;
		internal static bool hb_font_glyph_from_string (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String s, Int32 len, UInt32* glyph) =>
			(hb_font_glyph_from_string_delegate ??= GetSymbol<Delegates.hb_font_glyph_from_string> ("hb_font_glyph_from_string")).Invoke (font, s, len, glyph);
		#endif

		// extern void hb_font_glyph_to_string(hb_font_t* font, hb_codepoint_t glyph, char* s, unsigned int size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_glyph_to_string (hb_font_t font, UInt32 glyph, /* char */ void* s, UInt32 size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_glyph_to_string (hb_font_t font, UInt32 glyph, /* char */ void* s, UInt32 size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_glyph_to_string (hb_font_t font, UInt32 glyph, /* char */ void* s, UInt32 size);
		}
		private static Delegates.hb_font_glyph_to_string hb_font_glyph_to_string_delegate;
		internal static void hb_font_glyph_to_string (hb_font_t font, UInt32 glyph, /* char */ void* s, UInt32 size) =>
			(hb_font_glyph_to_string_delegate ??= GetSymbol<Delegates.hb_font_glyph_to_string> ("hb_font_glyph_to_string")).Invoke (font, glyph, s, size);
		#endif

		// extern hb_bool_t hb_font_is_immutable(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_is_immutable (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_is_immutable (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_is_immutable (hb_font_t font);
		}
		private static Delegates.hb_font_is_immutable hb_font_is_immutable_delegate;
		internal static bool hb_font_is_immutable (hb_font_t font) =>
			(hb_font_is_immutable_delegate ??= GetSymbol<Delegates.hb_font_is_immutable> ("hb_font_is_immutable")).Invoke (font);
		#endif

		// extern hb_bool_t hb_font_is_synthetic(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_is_synthetic (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_is_synthetic (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_is_synthetic (hb_font_t font);
		}
		private static Delegates.hb_font_is_synthetic hb_font_is_synthetic_delegate;
		internal static bool hb_font_is_synthetic (hb_font_t font) =>
			(hb_font_is_synthetic_delegate ??= GetSymbol<Delegates.hb_font_is_synthetic> ("hb_font_is_synthetic")).Invoke (font);
		#endif

		// extern const char** hb_font_list_funcs()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void** hb_font_list_funcs ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void** hb_font_list_funcs ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void** hb_font_list_funcs ();
		}
		private static Delegates.hb_font_list_funcs hb_font_list_funcs_delegate;
		internal static /* char */ void** hb_font_list_funcs () =>
			(hb_font_list_funcs_delegate ??= GetSymbol<Delegates.hb_font_list_funcs> ("hb_font_list_funcs")).Invoke ();
		#endif

		// extern void hb_font_make_immutable(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_make_immutable (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_make_immutable (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_make_immutable (hb_font_t font);
		}
		private static Delegates.hb_font_make_immutable hb_font_make_immutable_delegate;
		internal static void hb_font_make_immutable (hb_font_t font) =>
			(hb_font_make_immutable_delegate ??= GetSymbol<Delegates.hb_font_make_immutable> ("hb_font_make_immutable")).Invoke (font);
		#endif

		// extern hb_font_t* hb_font_reference(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_font_t hb_font_reference (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_reference (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_font_t hb_font_reference (hb_font_t font);
		}
		private static Delegates.hb_font_reference hb_font_reference_delegate;
		internal static hb_font_t hb_font_reference (hb_font_t font) =>
			(hb_font_reference_delegate ??= GetSymbol<Delegates.hb_font_reference> ("hb_font_reference")).Invoke (font);
		#endif

		// extern void hb_font_set_face(hb_font_t* font, hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_face (hb_font_t font, hb_face_t face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_face (hb_font_t font, hb_face_t face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_face (hb_font_t font, hb_face_t face);
		}
		private static Delegates.hb_font_set_face hb_font_set_face_delegate;
		internal static void hb_font_set_face (hb_font_t font, hb_face_t face) =>
			(hb_font_set_face_delegate ??= GetSymbol<Delegates.hb_font_set_face> ("hb_font_set_face")).Invoke (font, face);
		#endif

		// extern void hb_font_set_funcs(hb_font_t* font, hb_font_funcs_t* klass, void* font_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_funcs (hb_font_t font, hb_font_funcs_t klass, void* font_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_funcs (hb_font_t font, hb_font_funcs_t klass, void* font_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_funcs (hb_font_t font, hb_font_funcs_t klass, void* font_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_set_funcs hb_font_set_funcs_delegate;
		internal static void hb_font_set_funcs (hb_font_t font, hb_font_funcs_t klass, void* font_data, DestroyProxyDelegate destroy) =>
			(hb_font_set_funcs_delegate ??= GetSymbol<Delegates.hb_font_set_funcs> ("hb_font_set_funcs")).Invoke (font, klass, font_data, destroy);
		#endif

		// extern void hb_font_set_funcs_data(hb_font_t* font, void* font_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_funcs_data (hb_font_t font, void* font_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_funcs_data (hb_font_t font, void* font_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_funcs_data (hb_font_t font, void* font_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_set_funcs_data hb_font_set_funcs_data_delegate;
		internal static void hb_font_set_funcs_data (hb_font_t font, void* font_data, DestroyProxyDelegate destroy) =>
			(hb_font_set_funcs_data_delegate ??= GetSymbol<Delegates.hb_font_set_funcs_data> ("hb_font_set_funcs_data")).Invoke (font, font_data, destroy);
		#endif

		// extern hb_bool_t hb_font_set_funcs_using(hb_font_t* font, const char* name)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_font_set_funcs_using (hb_font_t font, /* char */ void* name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_set_funcs_using (hb_font_t font, /* char */ void* name);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_font_set_funcs_using (hb_font_t font, /* char */ void* name);
		}
		private static Delegates.hb_font_set_funcs_using hb_font_set_funcs_using_delegate;
		internal static bool hb_font_set_funcs_using (hb_font_t font, /* char */ void* name) =>
			(hb_font_set_funcs_using_delegate ??= GetSymbol<Delegates.hb_font_set_funcs_using> ("hb_font_set_funcs_using")).Invoke (font, name);
		#endif

		// extern void hb_font_set_parent(hb_font_t* font, hb_font_t* parent)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_parent (hb_font_t font, hb_font_t parent);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_parent (hb_font_t font, hb_font_t parent);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_parent (hb_font_t font, hb_font_t parent);
		}
		private static Delegates.hb_font_set_parent hb_font_set_parent_delegate;
		internal static void hb_font_set_parent (hb_font_t font, hb_font_t parent) =>
			(hb_font_set_parent_delegate ??= GetSymbol<Delegates.hb_font_set_parent> ("hb_font_set_parent")).Invoke (font, parent);
		#endif

		// extern void hb_font_set_ppem(hb_font_t* font, unsigned int x_ppem, unsigned int y_ppem)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_ppem (hb_font_t font, UInt32 x_ppem, UInt32 y_ppem);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_ppem (hb_font_t font, UInt32 x_ppem, UInt32 y_ppem);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_ppem (hb_font_t font, UInt32 x_ppem, UInt32 y_ppem);
		}
		private static Delegates.hb_font_set_ppem hb_font_set_ppem_delegate;
		internal static void hb_font_set_ppem (hb_font_t font, UInt32 x_ppem, UInt32 y_ppem) =>
			(hb_font_set_ppem_delegate ??= GetSymbol<Delegates.hb_font_set_ppem> ("hb_font_set_ppem")).Invoke (font, x_ppem, y_ppem);
		#endif

		// extern void hb_font_set_ptem(hb_font_t* font, float ptem)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_ptem (hb_font_t font, Single ptem);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_ptem (hb_font_t font, Single ptem);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_ptem (hb_font_t font, Single ptem);
		}
		private static Delegates.hb_font_set_ptem hb_font_set_ptem_delegate;
		internal static void hb_font_set_ptem (hb_font_t font, Single ptem) =>
			(hb_font_set_ptem_delegate ??= GetSymbol<Delegates.hb_font_set_ptem> ("hb_font_set_ptem")).Invoke (font, ptem);
		#endif

		// extern void hb_font_set_scale(hb_font_t* font, int x_scale, int y_scale)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale);
		}
		private static Delegates.hb_font_set_scale hb_font_set_scale_delegate;
		internal static void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale) =>
			(hb_font_set_scale_delegate ??= GetSymbol<Delegates.hb_font_set_scale> ("hb_font_set_scale")).Invoke (font, x_scale, y_scale);
		#endif

		// extern void hb_font_set_synthetic_bold(hb_font_t* font, float x_embolden, float y_embolden, hb_bool_t in_place)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_synthetic_bold (hb_font_t font, Single x_embolden, Single y_embolden, [MarshalAs (UnmanagedType.I1)] bool in_place);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_synthetic_bold (hb_font_t font, Single x_embolden, Single y_embolden, [MarshalAs (UnmanagedType.I1)] bool in_place);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_synthetic_bold (hb_font_t font, Single x_embolden, Single y_embolden, [MarshalAs (UnmanagedType.I1)] bool in_place);
		}
		private static Delegates.hb_font_set_synthetic_bold hb_font_set_synthetic_bold_delegate;
		internal static void hb_font_set_synthetic_bold (hb_font_t font, Single x_embolden, Single y_embolden, [MarshalAs (UnmanagedType.I1)] bool in_place) =>
			(hb_font_set_synthetic_bold_delegate ??= GetSymbol<Delegates.hb_font_set_synthetic_bold> ("hb_font_set_synthetic_bold")).Invoke (font, x_embolden, y_embolden, in_place);
		#endif

		// extern void hb_font_set_synthetic_slant(hb_font_t* font, float slant)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_synthetic_slant (hb_font_t font, Single slant);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_synthetic_slant (hb_font_t font, Single slant);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_synthetic_slant (hb_font_t font, Single slant);
		}
		private static Delegates.hb_font_set_synthetic_slant hb_font_set_synthetic_slant_delegate;
		internal static void hb_font_set_synthetic_slant (hb_font_t font, Single slant) =>
			(hb_font_set_synthetic_slant_delegate ??= GetSymbol<Delegates.hb_font_set_synthetic_slant> ("hb_font_set_synthetic_slant")).Invoke (font, slant);
		#endif

		// extern void hb_font_set_var_coords_design(hb_font_t* font, const float* coords, unsigned int coords_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_var_coords_design (hb_font_t font, Single* coords, UInt32 coords_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_var_coords_design (hb_font_t font, Single* coords, UInt32 coords_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_var_coords_design (hb_font_t font, Single* coords, UInt32 coords_length);
		}
		private static Delegates.hb_font_set_var_coords_design hb_font_set_var_coords_design_delegate;
		internal static void hb_font_set_var_coords_design (hb_font_t font, Single* coords, UInt32 coords_length) =>
			(hb_font_set_var_coords_design_delegate ??= GetSymbol<Delegates.hb_font_set_var_coords_design> ("hb_font_set_var_coords_design")).Invoke (font, coords, coords_length);
		#endif

		// extern void hb_font_set_var_coords_normalized(hb_font_t* font, const int* coords, unsigned int coords_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_var_coords_normalized (hb_font_t font, Int32* coords, UInt32 coords_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_var_coords_normalized (hb_font_t font, Int32* coords, UInt32 coords_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_var_coords_normalized (hb_font_t font, Int32* coords, UInt32 coords_length);
		}
		private static Delegates.hb_font_set_var_coords_normalized hb_font_set_var_coords_normalized_delegate;
		internal static void hb_font_set_var_coords_normalized (hb_font_t font, Int32* coords, UInt32 coords_length) =>
			(hb_font_set_var_coords_normalized_delegate ??= GetSymbol<Delegates.hb_font_set_var_coords_normalized> ("hb_font_set_var_coords_normalized")).Invoke (font, coords, coords_length);
		#endif

		// extern void hb_font_set_var_named_instance(hb_font_t* font, unsigned int instance_index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index);
		}
		private static Delegates.hb_font_set_var_named_instance hb_font_set_var_named_instance_delegate;
		internal static void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index) =>
			(hb_font_set_var_named_instance_delegate ??= GetSymbol<Delegates.hb_font_set_var_named_instance> ("hb_font_set_var_named_instance")).Invoke (font, instance_index);
		#endif

		// extern void hb_font_set_variation(hb_font_t* font, hb_tag_t tag, float value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_variation (hb_font_t font, UInt32 tag, Single value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_variation (hb_font_t font, UInt32 tag, Single value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_variation (hb_font_t font, UInt32 tag, Single value);
		}
		private static Delegates.hb_font_set_variation hb_font_set_variation_delegate;
		internal static void hb_font_set_variation (hb_font_t font, UInt32 tag, Single value) =>
			(hb_font_set_variation_delegate ??= GetSymbol<Delegates.hb_font_set_variation> ("hb_font_set_variation")).Invoke (font, tag, value);
		#endif

		// extern void hb_font_set_variations(hb_font_t* font, const hb_variation_t* variations, unsigned int variations_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_set_variations (hb_font_t font, Variation* variations, UInt32 variations_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_variations (hb_font_t font, Variation* variations, UInt32 variations_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_variations (hb_font_t font, Variation* variations, UInt32 variations_length);
		}
		private static Delegates.hb_font_set_variations hb_font_set_variations_delegate;
		internal static void hb_font_set_variations (hb_font_t font, Variation* variations, UInt32 variations_length) =>
			(hb_font_set_variations_delegate ??= GetSymbol<Delegates.hb_font_set_variations> ("hb_font_set_variations")).Invoke (font, variations, variations_length);
		#endif

		// extern void hb_font_subtract_glyph_origin_for_direction(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_font_subtract_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_subtract_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_subtract_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_subtract_glyph_origin_for_direction hb_font_subtract_glyph_origin_for_direction_delegate;
		internal static void hb_font_subtract_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y) =>
			(hb_font_subtract_glyph_origin_for_direction_delegate ??= GetSymbol<Delegates.hb_font_subtract_glyph_origin_for_direction> ("hb_font_subtract_glyph_origin_for_direction")).Invoke (font, glyph, direction, x, y);
		#endif

		#endregion

	}
}
