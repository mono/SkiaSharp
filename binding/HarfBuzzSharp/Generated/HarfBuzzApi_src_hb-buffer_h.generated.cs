using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-buffer.h

		// extern void hb_buffer_add(hb_buffer_t* buffer, hb_codepoint_t codepoint, unsigned int cluster)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_add (IntPtr buffer, UInt32 codepoint, UInt32 cluster);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add (IntPtr buffer, UInt32 codepoint, UInt32 cluster);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add (IntPtr buffer, UInt32 codepoint, UInt32 cluster);
		}
		private static Delegates.hb_buffer_add hb_buffer_add_delegate;
		internal static void hb_buffer_add (IntPtr buffer, UInt32 codepoint, UInt32 cluster) =>
			(hb_buffer_add_delegate ??= GetSymbol<Delegates.hb_buffer_add> ("hb_buffer_add")).Invoke (buffer, codepoint, cluster);
		#endif

		// extern void hb_buffer_add_codepoints(hb_buffer_t* buffer, const hb_codepoint_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_add_codepoints (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_codepoints (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_codepoints (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_codepoints hb_buffer_add_codepoints_delegate;
		internal static void hb_buffer_add_codepoints (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_codepoints_delegate ??= GetSymbol<Delegates.hb_buffer_add_codepoints> ("hb_buffer_add_codepoints")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_latin1(hb_buffer_t* buffer, const uint8_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_add_latin1 (IntPtr buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_latin1 (IntPtr buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_latin1 (IntPtr buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_latin1 hb_buffer_add_latin1_delegate;
		internal static void hb_buffer_add_latin1 (IntPtr buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_latin1_delegate ??= GetSymbol<Delegates.hb_buffer_add_latin1> ("hb_buffer_add_latin1")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_utf16(hb_buffer_t* buffer, const uint16_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_add_utf16 (IntPtr buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_utf16 (IntPtr buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_utf16 (IntPtr buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_utf16 hb_buffer_add_utf16_delegate;
		internal static void hb_buffer_add_utf16 (IntPtr buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_utf16_delegate ??= GetSymbol<Delegates.hb_buffer_add_utf16> ("hb_buffer_add_utf16")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_utf32(hb_buffer_t* buffer, const uint32_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_add_utf32 (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_utf32 (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_utf32 (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_utf32 hb_buffer_add_utf32_delegate;
		internal static void hb_buffer_add_utf32 (IntPtr buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_utf32_delegate ??= GetSymbol<Delegates.hb_buffer_add_utf32> ("hb_buffer_add_utf32")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_utf8(hb_buffer_t* buffer, const char* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_add_utf8 (IntPtr buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_utf8 (IntPtr buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_utf8 (IntPtr buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_utf8 hb_buffer_add_utf8_delegate;
		internal static void hb_buffer_add_utf8 (IntPtr buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_utf8_delegate ??= GetSymbol<Delegates.hb_buffer_add_utf8> ("hb_buffer_add_utf8")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern hb_bool_t hb_buffer_allocation_successful(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_buffer_allocation_successful (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_allocation_successful (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_allocation_successful (IntPtr buffer);
		}
		private static Delegates.hb_buffer_allocation_successful hb_buffer_allocation_successful_delegate;
		internal static bool hb_buffer_allocation_successful (IntPtr buffer) =>
			(hb_buffer_allocation_successful_delegate ??= GetSymbol<Delegates.hb_buffer_allocation_successful> ("hb_buffer_allocation_successful")).Invoke (buffer);
		#endif

		// extern void hb_buffer_append(hb_buffer_t* buffer, const hb_buffer_t* source, unsigned int start, unsigned int end)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_append (IntPtr buffer, IntPtr source, UInt32 start, UInt32 end);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_append (IntPtr buffer, IntPtr source, UInt32 start, UInt32 end);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_append (IntPtr buffer, IntPtr source, UInt32 start, UInt32 end);
		}
		private static Delegates.hb_buffer_append hb_buffer_append_delegate;
		internal static void hb_buffer_append (IntPtr buffer, IntPtr source, UInt32 start, UInt32 end) =>
			(hb_buffer_append_delegate ??= GetSymbol<Delegates.hb_buffer_append> ("hb_buffer_append")).Invoke (buffer, source, start, end);
		#endif

		// extern void hb_buffer_changed(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_changed (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_changed (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_changed (IntPtr buffer);
		}
		private static Delegates.hb_buffer_changed hb_buffer_changed_delegate;
		internal static void hb_buffer_changed (IntPtr buffer) =>
			(hb_buffer_changed_delegate ??= GetSymbol<Delegates.hb_buffer_changed> ("hb_buffer_changed")).Invoke (buffer);
		#endif

		// extern void hb_buffer_clear_contents(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_clear_contents (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_clear_contents (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_clear_contents (IntPtr buffer);
		}
		private static Delegates.hb_buffer_clear_contents hb_buffer_clear_contents_delegate;
		internal static void hb_buffer_clear_contents (IntPtr buffer) =>
			(hb_buffer_clear_contents_delegate ??= GetSymbol<Delegates.hb_buffer_clear_contents> ("hb_buffer_clear_contents")).Invoke (buffer);
		#endif

		// extern hb_buffer_t* hb_buffer_create()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_buffer_create ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_create ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_create ();
		}
		private static Delegates.hb_buffer_create hb_buffer_create_delegate;
		internal static IntPtr hb_buffer_create () =>
			(hb_buffer_create_delegate ??= GetSymbol<Delegates.hb_buffer_create> ("hb_buffer_create")).Invoke ();
		#endif

		// extern hb_buffer_t* hb_buffer_create_similar(const hb_buffer_t* src)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_buffer_create_similar (IntPtr src);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_create_similar (IntPtr src);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_create_similar (IntPtr src);
		}
		private static Delegates.hb_buffer_create_similar hb_buffer_create_similar_delegate;
		internal static IntPtr hb_buffer_create_similar (IntPtr src) =>
			(hb_buffer_create_similar_delegate ??= GetSymbol<Delegates.hb_buffer_create_similar> ("hb_buffer_create_similar")).Invoke (src);
		#endif

		// extern hb_bool_t hb_buffer_deserialize_glyphs(hb_buffer_t* buffer, const char* buf, int buf_len, const char** end_ptr, hb_font_t* font, hb_buffer_serialize_format_t format)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_buffer_deserialize_glyphs (IntPtr buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, IntPtr font, SerializeFormat format);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_deserialize_glyphs (IntPtr buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, IntPtr font, SerializeFormat format);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_deserialize_glyphs (IntPtr buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, IntPtr font, SerializeFormat format);
		}
		private static Delegates.hb_buffer_deserialize_glyphs hb_buffer_deserialize_glyphs_delegate;
		internal static bool hb_buffer_deserialize_glyphs (IntPtr buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, IntPtr font, SerializeFormat format) =>
			(hb_buffer_deserialize_glyphs_delegate ??= GetSymbol<Delegates.hb_buffer_deserialize_glyphs> ("hb_buffer_deserialize_glyphs")).Invoke (buffer, buf, buf_len, end_ptr, font, format);
		#endif

		// extern hb_bool_t hb_buffer_deserialize_unicode(hb_buffer_t* buffer, const char* buf, int buf_len, const char** end_ptr, hb_buffer_serialize_format_t format)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_buffer_deserialize_unicode (IntPtr buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_deserialize_unicode (IntPtr buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_deserialize_unicode (IntPtr buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format);
		}
		private static Delegates.hb_buffer_deserialize_unicode hb_buffer_deserialize_unicode_delegate;
		internal static bool hb_buffer_deserialize_unicode (IntPtr buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format) =>
			(hb_buffer_deserialize_unicode_delegate ??= GetSymbol<Delegates.hb_buffer_deserialize_unicode> ("hb_buffer_deserialize_unicode")).Invoke (buffer, buf, buf_len, end_ptr, format);
		#endif

		// extern void hb_buffer_destroy(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_destroy (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_destroy (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_destroy (IntPtr buffer);
		}
		private static Delegates.hb_buffer_destroy hb_buffer_destroy_delegate;
		internal static void hb_buffer_destroy (IntPtr buffer) =>
			(hb_buffer_destroy_delegate ??= GetSymbol<Delegates.hb_buffer_destroy> ("hb_buffer_destroy")).Invoke (buffer);
		#endif

		// extern hb_buffer_diff_flags_t hb_buffer_diff(hb_buffer_t* buffer, hb_buffer_t* reference, hb_codepoint_t dottedcircle_glyph, unsigned int position_fuzz)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial BufferDiffFlags hb_buffer_diff (IntPtr buffer, IntPtr reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern BufferDiffFlags hb_buffer_diff (IntPtr buffer, IntPtr reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate BufferDiffFlags hb_buffer_diff (IntPtr buffer, IntPtr reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz);
		}
		private static Delegates.hb_buffer_diff hb_buffer_diff_delegate;
		internal static BufferDiffFlags hb_buffer_diff (IntPtr buffer, IntPtr reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz) =>
			(hb_buffer_diff_delegate ??= GetSymbol<Delegates.hb_buffer_diff> ("hb_buffer_diff")).Invoke (buffer, reference, dottedcircle_glyph, position_fuzz);
		#endif

		// extern hb_buffer_cluster_level_t hb_buffer_get_cluster_level(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial ClusterLevel hb_buffer_get_cluster_level (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ClusterLevel hb_buffer_get_cluster_level (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate ClusterLevel hb_buffer_get_cluster_level (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_cluster_level hb_buffer_get_cluster_level_delegate;
		internal static ClusterLevel hb_buffer_get_cluster_level (IntPtr buffer) =>
			(hb_buffer_get_cluster_level_delegate ??= GetSymbol<Delegates.hb_buffer_get_cluster_level> ("hb_buffer_get_cluster_level")).Invoke (buffer);
		#endif

		// extern hb_buffer_content_type_t hb_buffer_get_content_type(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial ContentType hb_buffer_get_content_type (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ContentType hb_buffer_get_content_type (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate ContentType hb_buffer_get_content_type (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_content_type hb_buffer_get_content_type_delegate;
		internal static ContentType hb_buffer_get_content_type (IntPtr buffer) =>
			(hb_buffer_get_content_type_delegate ??= GetSymbol<Delegates.hb_buffer_get_content_type> ("hb_buffer_get_content_type")).Invoke (buffer);
		#endif

		// extern hb_direction_t hb_buffer_get_direction(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Direction hb_buffer_get_direction (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Direction hb_buffer_get_direction (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Direction hb_buffer_get_direction (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_direction hb_buffer_get_direction_delegate;
		internal static Direction hb_buffer_get_direction (IntPtr buffer) =>
			(hb_buffer_get_direction_delegate ??= GetSymbol<Delegates.hb_buffer_get_direction> ("hb_buffer_get_direction")).Invoke (buffer);
		#endif

		// extern hb_buffer_t* hb_buffer_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_buffer_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_get_empty ();
		}
		private static Delegates.hb_buffer_get_empty hb_buffer_get_empty_delegate;
		internal static IntPtr hb_buffer_get_empty () =>
			(hb_buffer_get_empty_delegate ??= GetSymbol<Delegates.hb_buffer_get_empty> ("hb_buffer_get_empty")).Invoke ();
		#endif

		// extern hb_buffer_flags_t hb_buffer_get_flags(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial BufferFlags hb_buffer_get_flags (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern BufferFlags hb_buffer_get_flags (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate BufferFlags hb_buffer_get_flags (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_flags hb_buffer_get_flags_delegate;
		internal static BufferFlags hb_buffer_get_flags (IntPtr buffer) =>
			(hb_buffer_get_flags_delegate ??= GetSymbol<Delegates.hb_buffer_get_flags> ("hb_buffer_get_flags")).Invoke (buffer);
		#endif

		// extern hb_glyph_info_t* hb_buffer_get_glyph_infos(hb_buffer_t* buffer, unsigned int* length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial GlyphInfo* hb_buffer_get_glyph_infos (IntPtr buffer, UInt32* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GlyphInfo* hb_buffer_get_glyph_infos (IntPtr buffer, UInt32* length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GlyphInfo* hb_buffer_get_glyph_infos (IntPtr buffer, UInt32* length);
		}
		private static Delegates.hb_buffer_get_glyph_infos hb_buffer_get_glyph_infos_delegate;
		internal static GlyphInfo* hb_buffer_get_glyph_infos (IntPtr buffer, UInt32* length) =>
			(hb_buffer_get_glyph_infos_delegate ??= GetSymbol<Delegates.hb_buffer_get_glyph_infos> ("hb_buffer_get_glyph_infos")).Invoke (buffer, length);
		#endif

		// extern hb_glyph_position_t* hb_buffer_get_glyph_positions(hb_buffer_t* buffer, unsigned int* length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial GlyphPosition* hb_buffer_get_glyph_positions (IntPtr buffer, UInt32* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GlyphPosition* hb_buffer_get_glyph_positions (IntPtr buffer, UInt32* length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GlyphPosition* hb_buffer_get_glyph_positions (IntPtr buffer, UInt32* length);
		}
		private static Delegates.hb_buffer_get_glyph_positions hb_buffer_get_glyph_positions_delegate;
		internal static GlyphPosition* hb_buffer_get_glyph_positions (IntPtr buffer, UInt32* length) =>
			(hb_buffer_get_glyph_positions_delegate ??= GetSymbol<Delegates.hb_buffer_get_glyph_positions> ("hb_buffer_get_glyph_positions")).Invoke (buffer, length);
		#endif

		// extern hb_codepoint_t hb_buffer_get_invisible_glyph(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_invisible_glyph (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_invisible_glyph (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_invisible_glyph (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_invisible_glyph hb_buffer_get_invisible_glyph_delegate;
		internal static UInt32 hb_buffer_get_invisible_glyph (IntPtr buffer) =>
			(hb_buffer_get_invisible_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_get_invisible_glyph> ("hb_buffer_get_invisible_glyph")).Invoke (buffer);
		#endif

		// extern hb_language_t hb_buffer_get_language(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_buffer_get_language (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_get_language (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_get_language (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_language hb_buffer_get_language_delegate;
		internal static IntPtr hb_buffer_get_language (IntPtr buffer) =>
			(hb_buffer_get_language_delegate ??= GetSymbol<Delegates.hb_buffer_get_language> ("hb_buffer_get_language")).Invoke (buffer);
		#endif

		// extern unsigned int hb_buffer_get_length(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_length (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_length (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_length (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_length hb_buffer_get_length_delegate;
		internal static UInt32 hb_buffer_get_length (IntPtr buffer) =>
			(hb_buffer_get_length_delegate ??= GetSymbol<Delegates.hb_buffer_get_length> ("hb_buffer_get_length")).Invoke (buffer);
		#endif

		// extern hb_codepoint_t hb_buffer_get_not_found_glyph(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_not_found_glyph (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_not_found_glyph (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_not_found_glyph (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_not_found_glyph hb_buffer_get_not_found_glyph_delegate;
		internal static UInt32 hb_buffer_get_not_found_glyph (IntPtr buffer) =>
			(hb_buffer_get_not_found_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_get_not_found_glyph> ("hb_buffer_get_not_found_glyph")).Invoke (buffer);
		#endif

		// extern hb_codepoint_t hb_buffer_get_not_found_variation_selector_glyph(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_not_found_variation_selector_glyph (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_not_found_variation_selector_glyph (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_not_found_variation_selector_glyph (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_not_found_variation_selector_glyph hb_buffer_get_not_found_variation_selector_glyph_delegate;
		internal static UInt32 hb_buffer_get_not_found_variation_selector_glyph (IntPtr buffer) =>
			(hb_buffer_get_not_found_variation_selector_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_get_not_found_variation_selector_glyph> ("hb_buffer_get_not_found_variation_selector_glyph")).Invoke (buffer);
		#endif

		// extern unsigned int hb_buffer_get_random_state(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_random_state (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_random_state (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_random_state (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_random_state hb_buffer_get_random_state_delegate;
		internal static UInt32 hb_buffer_get_random_state (IntPtr buffer) =>
			(hb_buffer_get_random_state_delegate ??= GetSymbol<Delegates.hb_buffer_get_random_state> ("hb_buffer_get_random_state")).Invoke (buffer);
		#endif

		// extern hb_codepoint_t hb_buffer_get_replacement_codepoint(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_replacement_codepoint (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_replacement_codepoint (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_replacement_codepoint (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_replacement_codepoint hb_buffer_get_replacement_codepoint_delegate;
		internal static UInt32 hb_buffer_get_replacement_codepoint (IntPtr buffer) =>
			(hb_buffer_get_replacement_codepoint_delegate ??= GetSymbol<Delegates.hb_buffer_get_replacement_codepoint> ("hb_buffer_get_replacement_codepoint")).Invoke (buffer);
		#endif

		// extern hb_script_t hb_buffer_get_script(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_get_script (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_script (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_script (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_script hb_buffer_get_script_delegate;
		internal static UInt32 hb_buffer_get_script (IntPtr buffer) =>
			(hb_buffer_get_script_delegate ??= GetSymbol<Delegates.hb_buffer_get_script> ("hb_buffer_get_script")).Invoke (buffer);
		#endif

		// extern hb_unicode_funcs_t* hb_buffer_get_unicode_funcs(const hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_buffer_get_unicode_funcs (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_get_unicode_funcs (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_get_unicode_funcs (IntPtr buffer);
		}
		private static Delegates.hb_buffer_get_unicode_funcs hb_buffer_get_unicode_funcs_delegate;
		internal static IntPtr hb_buffer_get_unicode_funcs (IntPtr buffer) =>
			(hb_buffer_get_unicode_funcs_delegate ??= GetSymbol<Delegates.hb_buffer_get_unicode_funcs> ("hb_buffer_get_unicode_funcs")).Invoke (buffer);
		#endif

		// extern void hb_buffer_guess_segment_properties(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_guess_segment_properties (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_guess_segment_properties (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_guess_segment_properties (IntPtr buffer);
		}
		private static Delegates.hb_buffer_guess_segment_properties hb_buffer_guess_segment_properties_delegate;
		internal static void hb_buffer_guess_segment_properties (IntPtr buffer) =>
			(hb_buffer_guess_segment_properties_delegate ??= GetSymbol<Delegates.hb_buffer_guess_segment_properties> ("hb_buffer_guess_segment_properties")).Invoke (buffer);
		#endif

		// extern hb_bool_t hb_buffer_has_positions(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_buffer_has_positions (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_has_positions (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_has_positions (IntPtr buffer);
		}
		private static Delegates.hb_buffer_has_positions hb_buffer_has_positions_delegate;
		internal static bool hb_buffer_has_positions (IntPtr buffer) =>
			(hb_buffer_has_positions_delegate ??= GetSymbol<Delegates.hb_buffer_has_positions> ("hb_buffer_has_positions")).Invoke (buffer);
		#endif

		// extern void hb_buffer_normalize_glyphs(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_normalize_glyphs (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_normalize_glyphs (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_normalize_glyphs (IntPtr buffer);
		}
		private static Delegates.hb_buffer_normalize_glyphs hb_buffer_normalize_glyphs_delegate;
		internal static void hb_buffer_normalize_glyphs (IntPtr buffer) =>
			(hb_buffer_normalize_glyphs_delegate ??= GetSymbol<Delegates.hb_buffer_normalize_glyphs> ("hb_buffer_normalize_glyphs")).Invoke (buffer);
		#endif

		// extern hb_bool_t hb_buffer_pre_allocate(hb_buffer_t* buffer, unsigned int size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_buffer_pre_allocate (IntPtr buffer, UInt32 size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_pre_allocate (IntPtr buffer, UInt32 size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_pre_allocate (IntPtr buffer, UInt32 size);
		}
		private static Delegates.hb_buffer_pre_allocate hb_buffer_pre_allocate_delegate;
		internal static bool hb_buffer_pre_allocate (IntPtr buffer, UInt32 size) =>
			(hb_buffer_pre_allocate_delegate ??= GetSymbol<Delegates.hb_buffer_pre_allocate> ("hb_buffer_pre_allocate")).Invoke (buffer, size);
		#endif

		// extern hb_buffer_t* hb_buffer_reference(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_buffer_reference (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_reference (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_reference (IntPtr buffer);
		}
		private static Delegates.hb_buffer_reference hb_buffer_reference_delegate;
		internal static IntPtr hb_buffer_reference (IntPtr buffer) =>
			(hb_buffer_reference_delegate ??= GetSymbol<Delegates.hb_buffer_reference> ("hb_buffer_reference")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reset(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_reset (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reset (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reset (IntPtr buffer);
		}
		private static Delegates.hb_buffer_reset hb_buffer_reset_delegate;
		internal static void hb_buffer_reset (IntPtr buffer) =>
			(hb_buffer_reset_delegate ??= GetSymbol<Delegates.hb_buffer_reset> ("hb_buffer_reset")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reverse(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_reverse (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reverse (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reverse (IntPtr buffer);
		}
		private static Delegates.hb_buffer_reverse hb_buffer_reverse_delegate;
		internal static void hb_buffer_reverse (IntPtr buffer) =>
			(hb_buffer_reverse_delegate ??= GetSymbol<Delegates.hb_buffer_reverse> ("hb_buffer_reverse")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reverse_clusters(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_reverse_clusters (IntPtr buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reverse_clusters (IntPtr buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reverse_clusters (IntPtr buffer);
		}
		private static Delegates.hb_buffer_reverse_clusters hb_buffer_reverse_clusters_delegate;
		internal static void hb_buffer_reverse_clusters (IntPtr buffer) =>
			(hb_buffer_reverse_clusters_delegate ??= GetSymbol<Delegates.hb_buffer_reverse_clusters> ("hb_buffer_reverse_clusters")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reverse_range(hb_buffer_t* buffer, unsigned int start, unsigned int end)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_reverse_range (IntPtr buffer, UInt32 start, UInt32 end);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reverse_range (IntPtr buffer, UInt32 start, UInt32 end);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reverse_range (IntPtr buffer, UInt32 start, UInt32 end);
		}
		private static Delegates.hb_buffer_reverse_range hb_buffer_reverse_range_delegate;
		internal static void hb_buffer_reverse_range (IntPtr buffer, UInt32 start, UInt32 end) =>
			(hb_buffer_reverse_range_delegate ??= GetSymbol<Delegates.hb_buffer_reverse_range> ("hb_buffer_reverse_range")).Invoke (buffer, start, end);
		#endif

		// extern unsigned int hb_buffer_serialize(hb_buffer_t* buffer, unsigned int start, unsigned int end, char* buf, unsigned int buf_size, unsigned int* buf_consumed, hb_font_t* font, hb_buffer_serialize_format_t format, hb_buffer_serialize_flags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_serialize (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_serialize (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_serialize (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);
		}
		private static Delegates.hb_buffer_serialize hb_buffer_serialize_delegate;
		internal static UInt32 hb_buffer_serialize (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags) =>
			(hb_buffer_serialize_delegate ??= GetSymbol<Delegates.hb_buffer_serialize> ("hb_buffer_serialize")).Invoke (buffer, start, end, buf, buf_size, buf_consumed, font, format, flags);
		#endif

		// extern hb_buffer_serialize_format_t hb_buffer_serialize_format_from_string(const char* str, int len)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial SerializeFormat hb_buffer_serialize_format_from_string (/* char */ void* str, Int32 len);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SerializeFormat hb_buffer_serialize_format_from_string (/* char */ void* str, Int32 len);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SerializeFormat hb_buffer_serialize_format_from_string (/* char */ void* str, Int32 len);
		}
		private static Delegates.hb_buffer_serialize_format_from_string hb_buffer_serialize_format_from_string_delegate;
		internal static SerializeFormat hb_buffer_serialize_format_from_string (/* char */ void* str, Int32 len) =>
			(hb_buffer_serialize_format_from_string_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_format_from_string> ("hb_buffer_serialize_format_from_string")).Invoke (str, len);
		#endif

		// extern const char* hb_buffer_serialize_format_to_string(hb_buffer_serialize_format_t format)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void* hb_buffer_serialize_format_to_string (SerializeFormat format);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_buffer_serialize_format_to_string (SerializeFormat format);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_buffer_serialize_format_to_string (SerializeFormat format);
		}
		private static Delegates.hb_buffer_serialize_format_to_string hb_buffer_serialize_format_to_string_delegate;
		internal static /* char */ void* hb_buffer_serialize_format_to_string (SerializeFormat format) =>
			(hb_buffer_serialize_format_to_string_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_format_to_string> ("hb_buffer_serialize_format_to_string")).Invoke (format);
		#endif

		// extern unsigned int hb_buffer_serialize_glyphs(hb_buffer_t* buffer, unsigned int start, unsigned int end, char* buf, unsigned int buf_size, unsigned int* buf_consumed, hb_font_t* font, hb_buffer_serialize_format_t format, hb_buffer_serialize_flags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_serialize_glyphs (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_serialize_glyphs (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_serialize_glyphs (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags);
		}
		private static Delegates.hb_buffer_serialize_glyphs hb_buffer_serialize_glyphs_delegate;
		internal static UInt32 hb_buffer_serialize_glyphs (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, IntPtr font, SerializeFormat format, SerializeFlag flags) =>
			(hb_buffer_serialize_glyphs_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_glyphs> ("hb_buffer_serialize_glyphs")).Invoke (buffer, start, end, buf, buf_size, buf_consumed, font, format, flags);
		#endif

		// extern const char** hb_buffer_serialize_list_formats()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void** hb_buffer_serialize_list_formats ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void** hb_buffer_serialize_list_formats ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void** hb_buffer_serialize_list_formats ();
		}
		private static Delegates.hb_buffer_serialize_list_formats hb_buffer_serialize_list_formats_delegate;
		internal static /* char */ void** hb_buffer_serialize_list_formats () =>
			(hb_buffer_serialize_list_formats_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_list_formats> ("hb_buffer_serialize_list_formats")).Invoke ();
		#endif

		// extern unsigned int hb_buffer_serialize_unicode(hb_buffer_t* buffer, unsigned int start, unsigned int end, char* buf, unsigned int buf_size, unsigned int* buf_consumed, hb_buffer_serialize_format_t format, hb_buffer_serialize_flags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_buffer_serialize_unicode (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_serialize_unicode (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_serialize_unicode (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags);
		}
		private static Delegates.hb_buffer_serialize_unicode hb_buffer_serialize_unicode_delegate;
		internal static UInt32 hb_buffer_serialize_unicode (IntPtr buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags) =>
			(hb_buffer_serialize_unicode_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_unicode> ("hb_buffer_serialize_unicode")).Invoke (buffer, start, end, buf, buf_size, buf_consumed, format, flags);
		#endif

		// extern void hb_buffer_set_cluster_level(hb_buffer_t* buffer, hb_buffer_cluster_level_t cluster_level)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_cluster_level (IntPtr buffer, ClusterLevel cluster_level);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_cluster_level (IntPtr buffer, ClusterLevel cluster_level);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_cluster_level (IntPtr buffer, ClusterLevel cluster_level);
		}
		private static Delegates.hb_buffer_set_cluster_level hb_buffer_set_cluster_level_delegate;
		internal static void hb_buffer_set_cluster_level (IntPtr buffer, ClusterLevel cluster_level) =>
			(hb_buffer_set_cluster_level_delegate ??= GetSymbol<Delegates.hb_buffer_set_cluster_level> ("hb_buffer_set_cluster_level")).Invoke (buffer, cluster_level);
		#endif

		// extern void hb_buffer_set_content_type(hb_buffer_t* buffer, hb_buffer_content_type_t content_type)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_content_type (IntPtr buffer, ContentType content_type);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_content_type (IntPtr buffer, ContentType content_type);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_content_type (IntPtr buffer, ContentType content_type);
		}
		private static Delegates.hb_buffer_set_content_type hb_buffer_set_content_type_delegate;
		internal static void hb_buffer_set_content_type (IntPtr buffer, ContentType content_type) =>
			(hb_buffer_set_content_type_delegate ??= GetSymbol<Delegates.hb_buffer_set_content_type> ("hb_buffer_set_content_type")).Invoke (buffer, content_type);
		#endif

		// extern void hb_buffer_set_direction(hb_buffer_t* buffer, hb_direction_t direction)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_direction (IntPtr buffer, Direction direction);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_direction (IntPtr buffer, Direction direction);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_direction (IntPtr buffer, Direction direction);
		}
		private static Delegates.hb_buffer_set_direction hb_buffer_set_direction_delegate;
		internal static void hb_buffer_set_direction (IntPtr buffer, Direction direction) =>
			(hb_buffer_set_direction_delegate ??= GetSymbol<Delegates.hb_buffer_set_direction> ("hb_buffer_set_direction")).Invoke (buffer, direction);
		#endif

		// extern void hb_buffer_set_flags(hb_buffer_t* buffer, hb_buffer_flags_t flags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_flags (IntPtr buffer, BufferFlags flags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_flags (IntPtr buffer, BufferFlags flags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_flags (IntPtr buffer, BufferFlags flags);
		}
		private static Delegates.hb_buffer_set_flags hb_buffer_set_flags_delegate;
		internal static void hb_buffer_set_flags (IntPtr buffer, BufferFlags flags) =>
			(hb_buffer_set_flags_delegate ??= GetSymbol<Delegates.hb_buffer_set_flags> ("hb_buffer_set_flags")).Invoke (buffer, flags);
		#endif

		// extern void hb_buffer_set_invisible_glyph(hb_buffer_t* buffer, hb_codepoint_t invisible)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_invisible_glyph (IntPtr buffer, UInt32 invisible);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_invisible_glyph (IntPtr buffer, UInt32 invisible);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_invisible_glyph (IntPtr buffer, UInt32 invisible);
		}
		private static Delegates.hb_buffer_set_invisible_glyph hb_buffer_set_invisible_glyph_delegate;
		internal static void hb_buffer_set_invisible_glyph (IntPtr buffer, UInt32 invisible) =>
			(hb_buffer_set_invisible_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_set_invisible_glyph> ("hb_buffer_set_invisible_glyph")).Invoke (buffer, invisible);
		#endif

		// extern void hb_buffer_set_language(hb_buffer_t* buffer, hb_language_t language)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_language (IntPtr buffer, IntPtr language);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_language (IntPtr buffer, IntPtr language);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_language (IntPtr buffer, IntPtr language);
		}
		private static Delegates.hb_buffer_set_language hb_buffer_set_language_delegate;
		internal static void hb_buffer_set_language (IntPtr buffer, IntPtr language) =>
			(hb_buffer_set_language_delegate ??= GetSymbol<Delegates.hb_buffer_set_language> ("hb_buffer_set_language")).Invoke (buffer, language);
		#endif

		// extern hb_bool_t hb_buffer_set_length(hb_buffer_t* buffer, unsigned int length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_buffer_set_length (IntPtr buffer, UInt32 length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_set_length (IntPtr buffer, UInt32 length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_set_length (IntPtr buffer, UInt32 length);
		}
		private static Delegates.hb_buffer_set_length hb_buffer_set_length_delegate;
		internal static bool hb_buffer_set_length (IntPtr buffer, UInt32 length) =>
			(hb_buffer_set_length_delegate ??= GetSymbol<Delegates.hb_buffer_set_length> ("hb_buffer_set_length")).Invoke (buffer, length);
		#endif

		// extern void hb_buffer_set_message_func(hb_buffer_t* buffer, hb_buffer_message_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_message_func (IntPtr buffer, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_message_func (IntPtr buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_message_func (IntPtr buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_buffer_set_message_func hb_buffer_set_message_func_delegate;
		internal static void hb_buffer_set_message_func (IntPtr buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_buffer_set_message_func_delegate ??= GetSymbol<Delegates.hb_buffer_set_message_func> ("hb_buffer_set_message_func")).Invoke (buffer, func, user_data, destroy);
		#endif

		// extern void hb_buffer_set_not_found_glyph(hb_buffer_t* buffer, hb_codepoint_t not_found)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_not_found_glyph (IntPtr buffer, UInt32 not_found);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_not_found_glyph (IntPtr buffer, UInt32 not_found);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_not_found_glyph (IntPtr buffer, UInt32 not_found);
		}
		private static Delegates.hb_buffer_set_not_found_glyph hb_buffer_set_not_found_glyph_delegate;
		internal static void hb_buffer_set_not_found_glyph (IntPtr buffer, UInt32 not_found) =>
			(hb_buffer_set_not_found_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_set_not_found_glyph> ("hb_buffer_set_not_found_glyph")).Invoke (buffer, not_found);
		#endif

		// extern void hb_buffer_set_not_found_variation_selector_glyph(hb_buffer_t* buffer, hb_codepoint_t not_found_variation_selector)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_not_found_variation_selector_glyph (IntPtr buffer, UInt32 not_found_variation_selector);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_not_found_variation_selector_glyph (IntPtr buffer, UInt32 not_found_variation_selector);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_not_found_variation_selector_glyph (IntPtr buffer, UInt32 not_found_variation_selector);
		}
		private static Delegates.hb_buffer_set_not_found_variation_selector_glyph hb_buffer_set_not_found_variation_selector_glyph_delegate;
		internal static void hb_buffer_set_not_found_variation_selector_glyph (IntPtr buffer, UInt32 not_found_variation_selector) =>
			(hb_buffer_set_not_found_variation_selector_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_set_not_found_variation_selector_glyph> ("hb_buffer_set_not_found_variation_selector_glyph")).Invoke (buffer, not_found_variation_selector);
		#endif

		// extern void hb_buffer_set_random_state(hb_buffer_t* buffer, unsigned int state)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_random_state (IntPtr buffer, UInt32 state);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_random_state (IntPtr buffer, UInt32 state);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_random_state (IntPtr buffer, UInt32 state);
		}
		private static Delegates.hb_buffer_set_random_state hb_buffer_set_random_state_delegate;
		internal static void hb_buffer_set_random_state (IntPtr buffer, UInt32 state) =>
			(hb_buffer_set_random_state_delegate ??= GetSymbol<Delegates.hb_buffer_set_random_state> ("hb_buffer_set_random_state")).Invoke (buffer, state);
		#endif

		// extern void hb_buffer_set_replacement_codepoint(hb_buffer_t* buffer, hb_codepoint_t replacement)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_replacement_codepoint (IntPtr buffer, UInt32 replacement);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_replacement_codepoint (IntPtr buffer, UInt32 replacement);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_replacement_codepoint (IntPtr buffer, UInt32 replacement);
		}
		private static Delegates.hb_buffer_set_replacement_codepoint hb_buffer_set_replacement_codepoint_delegate;
		internal static void hb_buffer_set_replacement_codepoint (IntPtr buffer, UInt32 replacement) =>
			(hb_buffer_set_replacement_codepoint_delegate ??= GetSymbol<Delegates.hb_buffer_set_replacement_codepoint> ("hb_buffer_set_replacement_codepoint")).Invoke (buffer, replacement);
		#endif

		// extern void hb_buffer_set_script(hb_buffer_t* buffer, hb_script_t script)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_script (IntPtr buffer, UInt32 script);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_script (IntPtr buffer, UInt32 script);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_script (IntPtr buffer, UInt32 script);
		}
		private static Delegates.hb_buffer_set_script hb_buffer_set_script_delegate;
		internal static void hb_buffer_set_script (IntPtr buffer, UInt32 script) =>
			(hb_buffer_set_script_delegate ??= GetSymbol<Delegates.hb_buffer_set_script> ("hb_buffer_set_script")).Invoke (buffer, script);
		#endif

		// extern void hb_buffer_set_unicode_funcs(hb_buffer_t* buffer, hb_unicode_funcs_t* unicode_funcs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_buffer_set_unicode_funcs (IntPtr buffer, IntPtr unicode_funcs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_unicode_funcs (IntPtr buffer, IntPtr unicode_funcs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_unicode_funcs (IntPtr buffer, IntPtr unicode_funcs);
		}
		private static Delegates.hb_buffer_set_unicode_funcs hb_buffer_set_unicode_funcs_delegate;
		internal static void hb_buffer_set_unicode_funcs (IntPtr buffer, IntPtr unicode_funcs) =>
			(hb_buffer_set_unicode_funcs_delegate ??= GetSymbol<Delegates.hb_buffer_set_unicode_funcs> ("hb_buffer_set_unicode_funcs")).Invoke (buffer, unicode_funcs);
		#endif

		// extern hb_glyph_flags_t hb_glyph_info_get_glyph_flags(const hb_glyph_info_t* info)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial GlyphFlags hb_glyph_info_get_glyph_flags (GlyphInfo* info);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GlyphFlags hb_glyph_info_get_glyph_flags (GlyphInfo* info);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GlyphFlags hb_glyph_info_get_glyph_flags (GlyphInfo* info);
		}
		private static Delegates.hb_glyph_info_get_glyph_flags hb_glyph_info_get_glyph_flags_delegate;
		internal static GlyphFlags hb_glyph_info_get_glyph_flags (GlyphInfo* info) =>
			(hb_glyph_info_get_glyph_flags_delegate ??= GetSymbol<Delegates.hb_glyph_info_get_glyph_flags> ("hb_glyph_info_get_glyph_flags")).Invoke (info);
		#endif

		#endregion

	}
}
