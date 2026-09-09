using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-face.h

		// extern hb_bool_t hb_face_builder_add_table(hb_face_t* face, hb_tag_t tag, hb_blob_t* blob)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_face_builder_add_table (IntPtr face, UInt32 tag, IntPtr blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_face_builder_add_table (IntPtr face, UInt32 tag, IntPtr blob);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_face_builder_add_table (IntPtr face, UInt32 tag, IntPtr blob);
		}
		private static Delegates.hb_face_builder_add_table hb_face_builder_add_table_delegate;
		internal static bool hb_face_builder_add_table (IntPtr face, UInt32 tag, IntPtr blob) =>
			(hb_face_builder_add_table_delegate ??= GetSymbol<Delegates.hb_face_builder_add_table> ("hb_face_builder_add_table")).Invoke (face, tag, blob);
		#endif

		// extern hb_face_t* hb_face_builder_create()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_builder_create ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_builder_create ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_builder_create ();
		}
		private static Delegates.hb_face_builder_create hb_face_builder_create_delegate;
		internal static IntPtr hb_face_builder_create () =>
			(hb_face_builder_create_delegate ??= GetSymbol<Delegates.hb_face_builder_create> ("hb_face_builder_create")).Invoke ();
		#endif

		// extern void hb_face_collect_unicodes(hb_face_t* face, hb_set_t* out)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_collect_unicodes (IntPtr face, IntPtr @out);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_collect_unicodes (IntPtr face, IntPtr @out);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_collect_unicodes (IntPtr face, IntPtr @out);
		}
		private static Delegates.hb_face_collect_unicodes hb_face_collect_unicodes_delegate;
		internal static void hb_face_collect_unicodes (IntPtr face, IntPtr @out) =>
			(hb_face_collect_unicodes_delegate ??= GetSymbol<Delegates.hb_face_collect_unicodes> ("hb_face_collect_unicodes")).Invoke (face, @out);
		#endif

		// extern void hb_face_collect_variation_selectors(hb_face_t* face, hb_set_t* out)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_collect_variation_selectors (IntPtr face, IntPtr @out);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_collect_variation_selectors (IntPtr face, IntPtr @out);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_collect_variation_selectors (IntPtr face, IntPtr @out);
		}
		private static Delegates.hb_face_collect_variation_selectors hb_face_collect_variation_selectors_delegate;
		internal static void hb_face_collect_variation_selectors (IntPtr face, IntPtr @out) =>
			(hb_face_collect_variation_selectors_delegate ??= GetSymbol<Delegates.hb_face_collect_variation_selectors> ("hb_face_collect_variation_selectors")).Invoke (face, @out);
		#endif

		// extern void hb_face_collect_variation_unicodes(hb_face_t* face, hb_codepoint_t variation_selector, hb_set_t* out)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_collect_variation_unicodes (IntPtr face, UInt32 variation_selector, IntPtr @out);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_collect_variation_unicodes (IntPtr face, UInt32 variation_selector, IntPtr @out);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_collect_variation_unicodes (IntPtr face, UInt32 variation_selector, IntPtr @out);
		}
		private static Delegates.hb_face_collect_variation_unicodes hb_face_collect_variation_unicodes_delegate;
		internal static void hb_face_collect_variation_unicodes (IntPtr face, UInt32 variation_selector, IntPtr @out) =>
			(hb_face_collect_variation_unicodes_delegate ??= GetSymbol<Delegates.hb_face_collect_variation_unicodes> ("hb_face_collect_variation_unicodes")).Invoke (face, variation_selector, @out);
		#endif

		// extern unsigned int hb_face_count(hb_blob_t* blob)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_face_count (IntPtr blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_count (IntPtr blob);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_count (IntPtr blob);
		}
		private static Delegates.hb_face_count hb_face_count_delegate;
		internal static UInt32 hb_face_count (IntPtr blob) =>
			(hb_face_count_delegate ??= GetSymbol<Delegates.hb_face_count> ("hb_face_count")).Invoke (blob);
		#endif

		// extern hb_face_t* hb_face_create(hb_blob_t* blob, unsigned int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_create (IntPtr blob, UInt32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_create (IntPtr blob, UInt32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_create (IntPtr blob, UInt32 index);
		}
		private static Delegates.hb_face_create hb_face_create_delegate;
		internal static IntPtr hb_face_create (IntPtr blob, UInt32 index) =>
			(hb_face_create_delegate ??= GetSymbol<Delegates.hb_face_create> ("hb_face_create")).Invoke (blob, index);
		#endif

		// extern hb_face_t* hb_face_create_for_tables(hb_reference_table_func_t reference_table_func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_create_for_tables (void* reference_table_func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_create_for_tables (ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_create_for_tables (ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_face_create_for_tables hb_face_create_for_tables_delegate;
		internal static IntPtr hb_face_create_for_tables (ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_face_create_for_tables_delegate ??= GetSymbol<Delegates.hb_face_create_for_tables> ("hb_face_create_for_tables")).Invoke (reference_table_func, user_data, destroy);
		#endif

		// extern void hb_face_destroy(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_destroy (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_destroy (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_destroy (IntPtr face);
		}
		private static Delegates.hb_face_destroy hb_face_destroy_delegate;
		internal static void hb_face_destroy (IntPtr face) =>
			(hb_face_destroy_delegate ??= GetSymbol<Delegates.hb_face_destroy> ("hb_face_destroy")).Invoke (face);
		#endif

		// extern hb_face_t* hb_face_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_get_empty ();
		}
		private static Delegates.hb_face_get_empty hb_face_get_empty_delegate;
		internal static IntPtr hb_face_get_empty () =>
			(hb_face_get_empty_delegate ??= GetSymbol<Delegates.hb_face_get_empty> ("hb_face_get_empty")).Invoke ();
		#endif

		// extern unsigned int hb_face_get_glyph_count(const hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_face_get_glyph_count (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_glyph_count (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_glyph_count (IntPtr face);
		}
		private static Delegates.hb_face_get_glyph_count hb_face_get_glyph_count_delegate;
		internal static UInt32 hb_face_get_glyph_count (IntPtr face) =>
			(hb_face_get_glyph_count_delegate ??= GetSymbol<Delegates.hb_face_get_glyph_count> ("hb_face_get_glyph_count")).Invoke (face);
		#endif

		// extern unsigned int hb_face_get_index(const hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_face_get_index (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_index (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_index (IntPtr face);
		}
		private static Delegates.hb_face_get_index hb_face_get_index_delegate;
		internal static UInt32 hb_face_get_index (IntPtr face) =>
			(hb_face_get_index_delegate ??= GetSymbol<Delegates.hb_face_get_index> ("hb_face_get_index")).Invoke (face);
		#endif

		// extern unsigned int hb_face_get_table_tags(const hb_face_t* face, unsigned int start_offset, unsigned int* table_count, hb_tag_t* table_tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_face_get_table_tags (IntPtr face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_table_tags (IntPtr face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_table_tags (IntPtr face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags);
		}
		private static Delegates.hb_face_get_table_tags hb_face_get_table_tags_delegate;
		internal static UInt32 hb_face_get_table_tags (IntPtr face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags) =>
			(hb_face_get_table_tags_delegate ??= GetSymbol<Delegates.hb_face_get_table_tags> ("hb_face_get_table_tags")).Invoke (face, start_offset, table_count, table_tags);
		#endif

		// extern unsigned int hb_face_get_upem(const hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_face_get_upem (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_upem (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_upem (IntPtr face);
		}
		private static Delegates.hb_face_get_upem hb_face_get_upem_delegate;
		internal static UInt32 hb_face_get_upem (IntPtr face) =>
			(hb_face_get_upem_delegate ??= GetSymbol<Delegates.hb_face_get_upem> ("hb_face_get_upem")).Invoke (face);
		#endif

		// extern hb_bool_t hb_face_is_immutable(const hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_face_is_immutable (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_face_is_immutable (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_face_is_immutable (IntPtr face);
		}
		private static Delegates.hb_face_is_immutable hb_face_is_immutable_delegate;
		internal static bool hb_face_is_immutable (IntPtr face) =>
			(hb_face_is_immutable_delegate ??= GetSymbol<Delegates.hb_face_is_immutable> ("hb_face_is_immutable")).Invoke (face);
		#endif

		// extern void hb_face_make_immutable(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_make_immutable (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_make_immutable (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_make_immutable (IntPtr face);
		}
		private static Delegates.hb_face_make_immutable hb_face_make_immutable_delegate;
		internal static void hb_face_make_immutable (IntPtr face) =>
			(hb_face_make_immutable_delegate ??= GetSymbol<Delegates.hb_face_make_immutable> ("hb_face_make_immutable")).Invoke (face);
		#endif

		// extern hb_face_t* hb_face_reference(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_reference (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_reference (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_reference (IntPtr face);
		}
		private static Delegates.hb_face_reference hb_face_reference_delegate;
		internal static IntPtr hb_face_reference (IntPtr face) =>
			(hb_face_reference_delegate ??= GetSymbol<Delegates.hb_face_reference> ("hb_face_reference")).Invoke (face);
		#endif

		// extern hb_blob_t* hb_face_reference_blob(hb_face_t* face)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_reference_blob (IntPtr face);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_reference_blob (IntPtr face);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_reference_blob (IntPtr face);
		}
		private static Delegates.hb_face_reference_blob hb_face_reference_blob_delegate;
		internal static IntPtr hb_face_reference_blob (IntPtr face) =>
			(hb_face_reference_blob_delegate ??= GetSymbol<Delegates.hb_face_reference_blob> ("hb_face_reference_blob")).Invoke (face);
		#endif

		// extern hb_blob_t* hb_face_reference_table(const hb_face_t* face, hb_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_face_reference_table (IntPtr face, UInt32 tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_face_reference_table (IntPtr face, UInt32 tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_face_reference_table (IntPtr face, UInt32 tag);
		}
		private static Delegates.hb_face_reference_table hb_face_reference_table_delegate;
		internal static IntPtr hb_face_reference_table (IntPtr face, UInt32 tag) =>
			(hb_face_reference_table_delegate ??= GetSymbol<Delegates.hb_face_reference_table> ("hb_face_reference_table")).Invoke (face, tag);
		#endif

		// extern void hb_face_set_glyph_count(hb_face_t* face, unsigned int glyph_count)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_set_glyph_count (IntPtr face, UInt32 glyph_count);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_set_glyph_count (IntPtr face, UInt32 glyph_count);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_set_glyph_count (IntPtr face, UInt32 glyph_count);
		}
		private static Delegates.hb_face_set_glyph_count hb_face_set_glyph_count_delegate;
		internal static void hb_face_set_glyph_count (IntPtr face, UInt32 glyph_count) =>
			(hb_face_set_glyph_count_delegate ??= GetSymbol<Delegates.hb_face_set_glyph_count> ("hb_face_set_glyph_count")).Invoke (face, glyph_count);
		#endif

		// extern void hb_face_set_index(hb_face_t* face, unsigned int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_set_index (IntPtr face, UInt32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_set_index (IntPtr face, UInt32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_set_index (IntPtr face, UInt32 index);
		}
		private static Delegates.hb_face_set_index hb_face_set_index_delegate;
		internal static void hb_face_set_index (IntPtr face, UInt32 index) =>
			(hb_face_set_index_delegate ??= GetSymbol<Delegates.hb_face_set_index> ("hb_face_set_index")).Invoke (face, index);
		#endif

		// extern void hb_face_set_upem(hb_face_t* face, unsigned int upem)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_face_set_upem (IntPtr face, UInt32 upem);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_set_upem (IntPtr face, UInt32 upem);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_set_upem (IntPtr face, UInt32 upem);
		}
		private static Delegates.hb_face_set_upem hb_face_set_upem_delegate;
		internal static void hb_face_set_upem (IntPtr face, UInt32 upem) =>
			(hb_face_set_upem_delegate ??= GetSymbol<Delegates.hb_face_set_upem> ("hb_face_set_upem")).Invoke (face, upem);
		#endif

		#endregion

	}
}
