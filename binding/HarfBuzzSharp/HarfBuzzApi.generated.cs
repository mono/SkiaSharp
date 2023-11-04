using System;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#region Class declarations

using hb_blob_t = System.IntPtr;
using hb_buffer_t = System.IntPtr;
using hb_face_t = System.IntPtr;
using hb_font_funcs_t = System.IntPtr;
using hb_font_t = System.IntPtr;
using hb_language_impl_t = System.IntPtr;
using hb_map_t = System.IntPtr;
using hb_set_t = System.IntPtr;
using hb_shape_plan_t = System.IntPtr;
using hb_unicode_funcs_t = System.IntPtr;

#endregion

#region Functions

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-blob.h

		// extern hb_blob_t* hb_blob_copy_writable_or_fail(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_copy_writable_or_fail (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_copy_writable_or_fail (hb_blob_t blob);
		}
		private static Delegates.hb_blob_copy_writable_or_fail hb_blob_copy_writable_or_fail_delegate;
		internal static hb_blob_t hb_blob_copy_writable_or_fail (hb_blob_t blob) =>
			(hb_blob_copy_writable_or_fail_delegate ??= GetSymbol<Delegates.hb_blob_copy_writable_or_fail> ("hb_blob_copy_writable_or_fail")).Invoke (blob);
		#endif

		// extern hb_blob_t* hb_blob_create(const char* data, unsigned int length, hb_memory_mode_t mode, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_create (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_blob_create hb_blob_create_delegate;
		internal static hb_blob_t hb_blob_create (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_blob_create_delegate ??= GetSymbol<Delegates.hb_blob_create> ("hb_blob_create")).Invoke (data, length, mode, user_data, destroy);
		#endif

		// extern hb_blob_t* hb_blob_create_from_file(const char* file_name)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_from_file ([MarshalAs (UnmanagedType.LPStr)] String file_name);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_create_from_file ([MarshalAs (UnmanagedType.LPStr)] String file_name);
		}
		private static Delegates.hb_blob_create_from_file hb_blob_create_from_file_delegate;
		internal static hb_blob_t hb_blob_create_from_file ([MarshalAs (UnmanagedType.LPStr)] String file_name) =>
			(hb_blob_create_from_file_delegate ??= GetSymbol<Delegates.hb_blob_create_from_file> ("hb_blob_create_from_file")).Invoke (file_name);
		#endif

		// extern hb_blob_t* hb_blob_create_from_file_or_fail(const char* file_name)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_from_file_or_fail (/* char */ void* file_name);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_create_from_file_or_fail (/* char */ void* file_name);
		}
		private static Delegates.hb_blob_create_from_file_or_fail hb_blob_create_from_file_or_fail_delegate;
		internal static hb_blob_t hb_blob_create_from_file_or_fail (/* char */ void* file_name) =>
			(hb_blob_create_from_file_or_fail_delegate ??= GetSymbol<Delegates.hb_blob_create_from_file_or_fail> ("hb_blob_create_from_file_or_fail")).Invoke (file_name);
		#endif

		// extern hb_blob_t* hb_blob_create_or_fail(const char* data, unsigned int length, hb_memory_mode_t mode, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_or_fail (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_create_or_fail (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_blob_create_or_fail hb_blob_create_or_fail_delegate;
		internal static hb_blob_t hb_blob_create_or_fail (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_blob_create_or_fail_delegate ??= GetSymbol<Delegates.hb_blob_create_or_fail> ("hb_blob_create_or_fail")).Invoke (data, length, mode, user_data, destroy);
		#endif

		// extern hb_blob_t* hb_blob_create_sub_blob(hb_blob_t* parent, unsigned int offset, unsigned int length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_sub_blob (hb_blob_t parent, UInt32 offset, UInt32 length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_create_sub_blob (hb_blob_t parent, UInt32 offset, UInt32 length);
		}
		private static Delegates.hb_blob_create_sub_blob hb_blob_create_sub_blob_delegate;
		internal static hb_blob_t hb_blob_create_sub_blob (hb_blob_t parent, UInt32 offset, UInt32 length) =>
			(hb_blob_create_sub_blob_delegate ??= GetSymbol<Delegates.hb_blob_create_sub_blob> ("hb_blob_create_sub_blob")).Invoke (parent, offset, length);
		#endif

		// extern void hb_blob_destroy(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_blob_destroy (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_blob_destroy (hb_blob_t blob);
		}
		private static Delegates.hb_blob_destroy hb_blob_destroy_delegate;
		internal static void hb_blob_destroy (hb_blob_t blob) =>
			(hb_blob_destroy_delegate ??= GetSymbol<Delegates.hb_blob_destroy> ("hb_blob_destroy")).Invoke (blob);
		#endif

		// extern const char* hb_blob_get_data(hb_blob_t* blob, unsigned int* length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_blob_get_data (hb_blob_t blob, UInt32* length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_blob_get_data (hb_blob_t blob, UInt32* length);
		}
		private static Delegates.hb_blob_get_data hb_blob_get_data_delegate;
		internal static /* char */ void* hb_blob_get_data (hb_blob_t blob, UInt32* length) =>
			(hb_blob_get_data_delegate ??= GetSymbol<Delegates.hb_blob_get_data> ("hb_blob_get_data")).Invoke (blob, length);
		#endif

		// extern char* hb_blob_get_data_writable(hb_blob_t* blob, unsigned int* length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_blob_get_data_writable (hb_blob_t blob, UInt32* length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_blob_get_data_writable (hb_blob_t blob, UInt32* length);
		}
		private static Delegates.hb_blob_get_data_writable hb_blob_get_data_writable_delegate;
		internal static /* char */ void* hb_blob_get_data_writable (hb_blob_t blob, UInt32* length) =>
			(hb_blob_get_data_writable_delegate ??= GetSymbol<Delegates.hb_blob_get_data_writable> ("hb_blob_get_data_writable")).Invoke (blob, length);
		#endif

		// extern hb_blob_t* hb_blob_get_empty()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_get_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_get_empty ();
		}
		private static Delegates.hb_blob_get_empty hb_blob_get_empty_delegate;
		internal static hb_blob_t hb_blob_get_empty () =>
			(hb_blob_get_empty_delegate ??= GetSymbol<Delegates.hb_blob_get_empty> ("hb_blob_get_empty")).Invoke ();
		#endif

		// extern unsigned int hb_blob_get_length(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_blob_get_length (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_blob_get_length (hb_blob_t blob);
		}
		private static Delegates.hb_blob_get_length hb_blob_get_length_delegate;
		internal static UInt32 hb_blob_get_length (hb_blob_t blob) =>
			(hb_blob_get_length_delegate ??= GetSymbol<Delegates.hb_blob_get_length> ("hb_blob_get_length")).Invoke (blob);
		#endif

		// extern hb_bool_t hb_blob_is_immutable(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_blob_is_immutable (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_blob_is_immutable (hb_blob_t blob);
		}
		private static Delegates.hb_blob_is_immutable hb_blob_is_immutable_delegate;
		internal static bool hb_blob_is_immutable (hb_blob_t blob) =>
			(hb_blob_is_immutable_delegate ??= GetSymbol<Delegates.hb_blob_is_immutable> ("hb_blob_is_immutable")).Invoke (blob);
		#endif

		// extern void hb_blob_make_immutable(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_blob_make_immutable (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_blob_make_immutable (hb_blob_t blob);
		}
		private static Delegates.hb_blob_make_immutable hb_blob_make_immutable_delegate;
		internal static void hb_blob_make_immutable (hb_blob_t blob) =>
			(hb_blob_make_immutable_delegate ??= GetSymbol<Delegates.hb_blob_make_immutable> ("hb_blob_make_immutable")).Invoke (blob);
		#endif

		// extern hb_blob_t* hb_blob_reference(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_reference (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_blob_reference (hb_blob_t blob);
		}
		private static Delegates.hb_blob_reference hb_blob_reference_delegate;
		internal static hb_blob_t hb_blob_reference (hb_blob_t blob) =>
			(hb_blob_reference_delegate ??= GetSymbol<Delegates.hb_blob_reference> ("hb_blob_reference")).Invoke (blob);
		#endif

		#endregion

		#region hb-buffer.h

		// extern void hb_buffer_add(hb_buffer_t* buffer, hb_codepoint_t codepoint, unsigned int cluster)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add (hb_buffer_t buffer, UInt32 codepoint, UInt32 cluster);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add (hb_buffer_t buffer, UInt32 codepoint, UInt32 cluster);
		}
		private static Delegates.hb_buffer_add hb_buffer_add_delegate;
		internal static void hb_buffer_add (hb_buffer_t buffer, UInt32 codepoint, UInt32 cluster) =>
			(hb_buffer_add_delegate ??= GetSymbol<Delegates.hb_buffer_add> ("hb_buffer_add")).Invoke (buffer, codepoint, cluster);
		#endif

		// extern void hb_buffer_add_codepoints(hb_buffer_t* buffer, const hb_codepoint_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_codepoints (hb_buffer_t buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_codepoints (hb_buffer_t buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_codepoints hb_buffer_add_codepoints_delegate;
		internal static void hb_buffer_add_codepoints (hb_buffer_t buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_codepoints_delegate ??= GetSymbol<Delegates.hb_buffer_add_codepoints> ("hb_buffer_add_codepoints")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_latin1(hb_buffer_t* buffer, const uint8_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_latin1 (hb_buffer_t buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_latin1 (hb_buffer_t buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_latin1 hb_buffer_add_latin1_delegate;
		internal static void hb_buffer_add_latin1 (hb_buffer_t buffer, Byte* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_latin1_delegate ??= GetSymbol<Delegates.hb_buffer_add_latin1> ("hb_buffer_add_latin1")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_utf16(hb_buffer_t* buffer, const uint16_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_utf16 (hb_buffer_t buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_utf16 (hb_buffer_t buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_utf16 hb_buffer_add_utf16_delegate;
		internal static void hb_buffer_add_utf16 (hb_buffer_t buffer, UInt16* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_utf16_delegate ??= GetSymbol<Delegates.hb_buffer_add_utf16> ("hb_buffer_add_utf16")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_utf32(hb_buffer_t* buffer, const uint32_t* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_utf32 (hb_buffer_t buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_utf32 (hb_buffer_t buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_utf32 hb_buffer_add_utf32_delegate;
		internal static void hb_buffer_add_utf32 (hb_buffer_t buffer, UInt32* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_utf32_delegate ??= GetSymbol<Delegates.hb_buffer_add_utf32> ("hb_buffer_add_utf32")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern void hb_buffer_add_utf8(hb_buffer_t* buffer, const char* text, int text_length, unsigned int item_offset, int item_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_add_utf8 (hb_buffer_t buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_add_utf8 (hb_buffer_t buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length);
		}
		private static Delegates.hb_buffer_add_utf8 hb_buffer_add_utf8_delegate;
		internal static void hb_buffer_add_utf8 (hb_buffer_t buffer, /* char */ void* text, Int32 text_length, UInt32 item_offset, Int32 item_length) =>
			(hb_buffer_add_utf8_delegate ??= GetSymbol<Delegates.hb_buffer_add_utf8> ("hb_buffer_add_utf8")).Invoke (buffer, text, text_length, item_offset, item_length);
		#endif

		// extern hb_bool_t hb_buffer_allocation_successful(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_allocation_successful (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_allocation_successful (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_allocation_successful hb_buffer_allocation_successful_delegate;
		internal static bool hb_buffer_allocation_successful (hb_buffer_t buffer) =>
			(hb_buffer_allocation_successful_delegate ??= GetSymbol<Delegates.hb_buffer_allocation_successful> ("hb_buffer_allocation_successful")).Invoke (buffer);
		#endif

		// extern void hb_buffer_append(hb_buffer_t* buffer, hb_buffer_t* source, unsigned int start, unsigned int end)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_append (hb_buffer_t buffer, hb_buffer_t source, UInt32 start, UInt32 end);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_append (hb_buffer_t buffer, hb_buffer_t source, UInt32 start, UInt32 end);
		}
		private static Delegates.hb_buffer_append hb_buffer_append_delegate;
		internal static void hb_buffer_append (hb_buffer_t buffer, hb_buffer_t source, UInt32 start, UInt32 end) =>
			(hb_buffer_append_delegate ??= GetSymbol<Delegates.hb_buffer_append> ("hb_buffer_append")).Invoke (buffer, source, start, end);
		#endif

		// extern void hb_buffer_clear_contents(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_clear_contents (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_clear_contents (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_clear_contents hb_buffer_clear_contents_delegate;
		internal static void hb_buffer_clear_contents (hb_buffer_t buffer) =>
			(hb_buffer_clear_contents_delegate ??= GetSymbol<Delegates.hb_buffer_clear_contents> ("hb_buffer_clear_contents")).Invoke (buffer);
		#endif

		// extern hb_buffer_t* hb_buffer_create()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_buffer_t hb_buffer_create ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_buffer_t hb_buffer_create ();
		}
		private static Delegates.hb_buffer_create hb_buffer_create_delegate;
		internal static hb_buffer_t hb_buffer_create () =>
			(hb_buffer_create_delegate ??= GetSymbol<Delegates.hb_buffer_create> ("hb_buffer_create")).Invoke ();
		#endif

		// extern hb_bool_t hb_buffer_deserialize_glyphs(hb_buffer_t* buffer, const char* buf, int buf_len, const char** end_ptr, hb_font_t* font, hb_buffer_serialize_format_t format)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_deserialize_glyphs (hb_buffer_t buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, hb_font_t font, SerializeFormat format);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_deserialize_glyphs (hb_buffer_t buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, hb_font_t font, SerializeFormat format);
		}
		private static Delegates.hb_buffer_deserialize_glyphs hb_buffer_deserialize_glyphs_delegate;
		internal static bool hb_buffer_deserialize_glyphs (hb_buffer_t buffer, [MarshalAs (UnmanagedType.LPStr)] String buf, Int32 buf_len, /* char */ void** end_ptr, hb_font_t font, SerializeFormat format) =>
			(hb_buffer_deserialize_glyphs_delegate ??= GetSymbol<Delegates.hb_buffer_deserialize_glyphs> ("hb_buffer_deserialize_glyphs")).Invoke (buffer, buf, buf_len, end_ptr, font, format);
		#endif

		// extern hb_bool_t hb_buffer_deserialize_unicode(hb_buffer_t* buffer, const char* buf, int buf_len, const char** end_ptr, hb_buffer_serialize_format_t format)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_deserialize_unicode (hb_buffer_t buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_deserialize_unicode (hb_buffer_t buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format);
		}
		private static Delegates.hb_buffer_deserialize_unicode hb_buffer_deserialize_unicode_delegate;
		internal static bool hb_buffer_deserialize_unicode (hb_buffer_t buffer, /* char */ void* buf, Int32 buf_len, /* char */ void** end_ptr, SerializeFormat format) =>
			(hb_buffer_deserialize_unicode_delegate ??= GetSymbol<Delegates.hb_buffer_deserialize_unicode> ("hb_buffer_deserialize_unicode")).Invoke (buffer, buf, buf_len, end_ptr, format);
		#endif

		// extern void hb_buffer_destroy(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_destroy (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_destroy (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_destroy hb_buffer_destroy_delegate;
		internal static void hb_buffer_destroy (hb_buffer_t buffer) =>
			(hb_buffer_destroy_delegate ??= GetSymbol<Delegates.hb_buffer_destroy> ("hb_buffer_destroy")).Invoke (buffer);
		#endif

		// extern hb_buffer_diff_flags_t hb_buffer_diff(hb_buffer_t* buffer, hb_buffer_t* reference, hb_codepoint_t dottedcircle_glyph, unsigned int position_fuzz)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern BufferDiffFlags hb_buffer_diff (hb_buffer_t buffer, hb_buffer_t reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate BufferDiffFlags hb_buffer_diff (hb_buffer_t buffer, hb_buffer_t reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz);
		}
		private static Delegates.hb_buffer_diff hb_buffer_diff_delegate;
		internal static BufferDiffFlags hb_buffer_diff (hb_buffer_t buffer, hb_buffer_t reference, UInt32 dottedcircle_glyph, UInt32 position_fuzz) =>
			(hb_buffer_diff_delegate ??= GetSymbol<Delegates.hb_buffer_diff> ("hb_buffer_diff")).Invoke (buffer, reference, dottedcircle_glyph, position_fuzz);
		#endif

		// extern hb_buffer_cluster_level_t hb_buffer_get_cluster_level(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ClusterLevel hb_buffer_get_cluster_level (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate ClusterLevel hb_buffer_get_cluster_level (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_cluster_level hb_buffer_get_cluster_level_delegate;
		internal static ClusterLevel hb_buffer_get_cluster_level (hb_buffer_t buffer) =>
			(hb_buffer_get_cluster_level_delegate ??= GetSymbol<Delegates.hb_buffer_get_cluster_level> ("hb_buffer_get_cluster_level")).Invoke (buffer);
		#endif

		// extern hb_buffer_content_type_t hb_buffer_get_content_type(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern ContentType hb_buffer_get_content_type (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate ContentType hb_buffer_get_content_type (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_content_type hb_buffer_get_content_type_delegate;
		internal static ContentType hb_buffer_get_content_type (hb_buffer_t buffer) =>
			(hb_buffer_get_content_type_delegate ??= GetSymbol<Delegates.hb_buffer_get_content_type> ("hb_buffer_get_content_type")).Invoke (buffer);
		#endif

		// extern hb_direction_t hb_buffer_get_direction(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Direction hb_buffer_get_direction (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Direction hb_buffer_get_direction (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_direction hb_buffer_get_direction_delegate;
		internal static Direction hb_buffer_get_direction (hb_buffer_t buffer) =>
			(hb_buffer_get_direction_delegate ??= GetSymbol<Delegates.hb_buffer_get_direction> ("hb_buffer_get_direction")).Invoke (buffer);
		#endif

		// extern hb_buffer_t* hb_buffer_get_empty()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_buffer_t hb_buffer_get_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_buffer_t hb_buffer_get_empty ();
		}
		private static Delegates.hb_buffer_get_empty hb_buffer_get_empty_delegate;
		internal static hb_buffer_t hb_buffer_get_empty () =>
			(hb_buffer_get_empty_delegate ??= GetSymbol<Delegates.hb_buffer_get_empty> ("hb_buffer_get_empty")).Invoke ();
		#endif

		// extern hb_buffer_flags_t hb_buffer_get_flags(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern BufferFlags hb_buffer_get_flags (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate BufferFlags hb_buffer_get_flags (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_flags hb_buffer_get_flags_delegate;
		internal static BufferFlags hb_buffer_get_flags (hb_buffer_t buffer) =>
			(hb_buffer_get_flags_delegate ??= GetSymbol<Delegates.hb_buffer_get_flags> ("hb_buffer_get_flags")).Invoke (buffer);
		#endif

		// extern hb_glyph_info_t* hb_buffer_get_glyph_infos(hb_buffer_t* buffer, unsigned int* length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GlyphInfo* hb_buffer_get_glyph_infos (hb_buffer_t buffer, UInt32* length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GlyphInfo* hb_buffer_get_glyph_infos (hb_buffer_t buffer, UInt32* length);
		}
		private static Delegates.hb_buffer_get_glyph_infos hb_buffer_get_glyph_infos_delegate;
		internal static GlyphInfo* hb_buffer_get_glyph_infos (hb_buffer_t buffer, UInt32* length) =>
			(hb_buffer_get_glyph_infos_delegate ??= GetSymbol<Delegates.hb_buffer_get_glyph_infos> ("hb_buffer_get_glyph_infos")).Invoke (buffer, length);
		#endif

		// extern hb_glyph_position_t* hb_buffer_get_glyph_positions(hb_buffer_t* buffer, unsigned int* length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GlyphPosition* hb_buffer_get_glyph_positions (hb_buffer_t buffer, UInt32* length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate GlyphPosition* hb_buffer_get_glyph_positions (hb_buffer_t buffer, UInt32* length);
		}
		private static Delegates.hb_buffer_get_glyph_positions hb_buffer_get_glyph_positions_delegate;
		internal static GlyphPosition* hb_buffer_get_glyph_positions (hb_buffer_t buffer, UInt32* length) =>
			(hb_buffer_get_glyph_positions_delegate ??= GetSymbol<Delegates.hb_buffer_get_glyph_positions> ("hb_buffer_get_glyph_positions")).Invoke (buffer, length);
		#endif

		// extern hb_codepoint_t hb_buffer_get_invisible_glyph(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_invisible_glyph (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_invisible_glyph (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_invisible_glyph hb_buffer_get_invisible_glyph_delegate;
		internal static UInt32 hb_buffer_get_invisible_glyph (hb_buffer_t buffer) =>
			(hb_buffer_get_invisible_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_get_invisible_glyph> ("hb_buffer_get_invisible_glyph")).Invoke (buffer);
		#endif

		// extern hb_language_t hb_buffer_get_language(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_buffer_get_language (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_buffer_get_language (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_language hb_buffer_get_language_delegate;
		internal static IntPtr hb_buffer_get_language (hb_buffer_t buffer) =>
			(hb_buffer_get_language_delegate ??= GetSymbol<Delegates.hb_buffer_get_language> ("hb_buffer_get_language")).Invoke (buffer);
		#endif

		// extern unsigned int hb_buffer_get_length(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_length (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_length (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_length hb_buffer_get_length_delegate;
		internal static UInt32 hb_buffer_get_length (hb_buffer_t buffer) =>
			(hb_buffer_get_length_delegate ??= GetSymbol<Delegates.hb_buffer_get_length> ("hb_buffer_get_length")).Invoke (buffer);
		#endif

		// extern hb_codepoint_t hb_buffer_get_replacement_codepoint(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_replacement_codepoint (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_replacement_codepoint (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_replacement_codepoint hb_buffer_get_replacement_codepoint_delegate;
		internal static UInt32 hb_buffer_get_replacement_codepoint (hb_buffer_t buffer) =>
			(hb_buffer_get_replacement_codepoint_delegate ??= GetSymbol<Delegates.hb_buffer_get_replacement_codepoint> ("hb_buffer_get_replacement_codepoint")).Invoke (buffer);
		#endif

		// extern hb_script_t hb_buffer_get_script(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_get_script (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_get_script (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_script hb_buffer_get_script_delegate;
		internal static UInt32 hb_buffer_get_script (hb_buffer_t buffer) =>
			(hb_buffer_get_script_delegate ??= GetSymbol<Delegates.hb_buffer_get_script> ("hb_buffer_get_script")).Invoke (buffer);
		#endif

		// extern hb_unicode_funcs_t* hb_buffer_get_unicode_funcs(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_unicode_funcs_t hb_buffer_get_unicode_funcs (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_unicode_funcs_t hb_buffer_get_unicode_funcs (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_get_unicode_funcs hb_buffer_get_unicode_funcs_delegate;
		internal static hb_unicode_funcs_t hb_buffer_get_unicode_funcs (hb_buffer_t buffer) =>
			(hb_buffer_get_unicode_funcs_delegate ??= GetSymbol<Delegates.hb_buffer_get_unicode_funcs> ("hb_buffer_get_unicode_funcs")).Invoke (buffer);
		#endif

		// extern void hb_buffer_guess_segment_properties(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_guess_segment_properties (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_guess_segment_properties (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_guess_segment_properties hb_buffer_guess_segment_properties_delegate;
		internal static void hb_buffer_guess_segment_properties (hb_buffer_t buffer) =>
			(hb_buffer_guess_segment_properties_delegate ??= GetSymbol<Delegates.hb_buffer_guess_segment_properties> ("hb_buffer_guess_segment_properties")).Invoke (buffer);
		#endif

		// extern hb_bool_t hb_buffer_has_positions(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_has_positions (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_has_positions (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_has_positions hb_buffer_has_positions_delegate;
		internal static bool hb_buffer_has_positions (hb_buffer_t buffer) =>
			(hb_buffer_has_positions_delegate ??= GetSymbol<Delegates.hb_buffer_has_positions> ("hb_buffer_has_positions")).Invoke (buffer);
		#endif

		// extern void hb_buffer_normalize_glyphs(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_normalize_glyphs (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_normalize_glyphs (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_normalize_glyphs hb_buffer_normalize_glyphs_delegate;
		internal static void hb_buffer_normalize_glyphs (hb_buffer_t buffer) =>
			(hb_buffer_normalize_glyphs_delegate ??= GetSymbol<Delegates.hb_buffer_normalize_glyphs> ("hb_buffer_normalize_glyphs")).Invoke (buffer);
		#endif

		// extern hb_bool_t hb_buffer_pre_allocate(hb_buffer_t* buffer, unsigned int size)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_pre_allocate (hb_buffer_t buffer, UInt32 size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_pre_allocate (hb_buffer_t buffer, UInt32 size);
		}
		private static Delegates.hb_buffer_pre_allocate hb_buffer_pre_allocate_delegate;
		internal static bool hb_buffer_pre_allocate (hb_buffer_t buffer, UInt32 size) =>
			(hb_buffer_pre_allocate_delegate ??= GetSymbol<Delegates.hb_buffer_pre_allocate> ("hb_buffer_pre_allocate")).Invoke (buffer, size);
		#endif

		// extern hb_buffer_t* hb_buffer_reference(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_buffer_t hb_buffer_reference (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_buffer_t hb_buffer_reference (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_reference hb_buffer_reference_delegate;
		internal static hb_buffer_t hb_buffer_reference (hb_buffer_t buffer) =>
			(hb_buffer_reference_delegate ??= GetSymbol<Delegates.hb_buffer_reference> ("hb_buffer_reference")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reset(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reset (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reset (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_reset hb_buffer_reset_delegate;
		internal static void hb_buffer_reset (hb_buffer_t buffer) =>
			(hb_buffer_reset_delegate ??= GetSymbol<Delegates.hb_buffer_reset> ("hb_buffer_reset")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reverse(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reverse (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reverse (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_reverse hb_buffer_reverse_delegate;
		internal static void hb_buffer_reverse (hb_buffer_t buffer) =>
			(hb_buffer_reverse_delegate ??= GetSymbol<Delegates.hb_buffer_reverse> ("hb_buffer_reverse")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reverse_clusters(hb_buffer_t* buffer)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reverse_clusters (hb_buffer_t buffer);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reverse_clusters (hb_buffer_t buffer);
		}
		private static Delegates.hb_buffer_reverse_clusters hb_buffer_reverse_clusters_delegate;
		internal static void hb_buffer_reverse_clusters (hb_buffer_t buffer) =>
			(hb_buffer_reverse_clusters_delegate ??= GetSymbol<Delegates.hb_buffer_reverse_clusters> ("hb_buffer_reverse_clusters")).Invoke (buffer);
		#endif

		// extern void hb_buffer_reverse_range(hb_buffer_t* buffer, unsigned int start, unsigned int end)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_reverse_range (hb_buffer_t buffer, UInt32 start, UInt32 end);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_reverse_range (hb_buffer_t buffer, UInt32 start, UInt32 end);
		}
		private static Delegates.hb_buffer_reverse_range hb_buffer_reverse_range_delegate;
		internal static void hb_buffer_reverse_range (hb_buffer_t buffer, UInt32 start, UInt32 end) =>
			(hb_buffer_reverse_range_delegate ??= GetSymbol<Delegates.hb_buffer_reverse_range> ("hb_buffer_reverse_range")).Invoke (buffer, start, end);
		#endif

		// extern unsigned int hb_buffer_serialize(hb_buffer_t* buffer, unsigned int start, unsigned int end, char* buf, unsigned int buf_size, unsigned int* buf_consumed, hb_font_t* font, hb_buffer_serialize_format_t format, hb_buffer_serialize_flags_t flags)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_serialize (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_serialize (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags);
		}
		private static Delegates.hb_buffer_serialize hb_buffer_serialize_delegate;
		internal static UInt32 hb_buffer_serialize (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags) =>
			(hb_buffer_serialize_delegate ??= GetSymbol<Delegates.hb_buffer_serialize> ("hb_buffer_serialize")).Invoke (buffer, start, end, buf, buf_size, buf_consumed, font, format, flags);
		#endif

		// extern hb_buffer_serialize_format_t hb_buffer_serialize_format_from_string(const char* str, int len)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SerializeFormat hb_buffer_serialize_format_from_string (/* char */ void* str, Int32 len);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_buffer_serialize_format_to_string (SerializeFormat format);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_serialize_glyphs (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_serialize_glyphs (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags);
		}
		private static Delegates.hb_buffer_serialize_glyphs hb_buffer_serialize_glyphs_delegate;
		internal static UInt32 hb_buffer_serialize_glyphs (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, hb_font_t font, SerializeFormat format, SerializeFlag flags) =>
			(hb_buffer_serialize_glyphs_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_glyphs> ("hb_buffer_serialize_glyphs")).Invoke (buffer, start, end, buf, buf_size, buf_consumed, font, format, flags);
		#endif

		// extern const char** hb_buffer_serialize_list_formats()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void** hb_buffer_serialize_list_formats ();
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_buffer_serialize_unicode (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_buffer_serialize_unicode (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags);
		}
		private static Delegates.hb_buffer_serialize_unicode hb_buffer_serialize_unicode_delegate;
		internal static UInt32 hb_buffer_serialize_unicode (hb_buffer_t buffer, UInt32 start, UInt32 end, /* char */ void* buf, UInt32 buf_size, UInt32* buf_consumed, SerializeFormat format, SerializeFlag flags) =>
			(hb_buffer_serialize_unicode_delegate ??= GetSymbol<Delegates.hb_buffer_serialize_unicode> ("hb_buffer_serialize_unicode")).Invoke (buffer, start, end, buf, buf_size, buf_consumed, format, flags);
		#endif

		// extern void hb_buffer_set_cluster_level(hb_buffer_t* buffer, hb_buffer_cluster_level_t cluster_level)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_cluster_level (hb_buffer_t buffer, ClusterLevel cluster_level);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_cluster_level (hb_buffer_t buffer, ClusterLevel cluster_level);
		}
		private static Delegates.hb_buffer_set_cluster_level hb_buffer_set_cluster_level_delegate;
		internal static void hb_buffer_set_cluster_level (hb_buffer_t buffer, ClusterLevel cluster_level) =>
			(hb_buffer_set_cluster_level_delegate ??= GetSymbol<Delegates.hb_buffer_set_cluster_level> ("hb_buffer_set_cluster_level")).Invoke (buffer, cluster_level);
		#endif

		// extern void hb_buffer_set_content_type(hb_buffer_t* buffer, hb_buffer_content_type_t content_type)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_content_type (hb_buffer_t buffer, ContentType content_type);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_content_type (hb_buffer_t buffer, ContentType content_type);
		}
		private static Delegates.hb_buffer_set_content_type hb_buffer_set_content_type_delegate;
		internal static void hb_buffer_set_content_type (hb_buffer_t buffer, ContentType content_type) =>
			(hb_buffer_set_content_type_delegate ??= GetSymbol<Delegates.hb_buffer_set_content_type> ("hb_buffer_set_content_type")).Invoke (buffer, content_type);
		#endif

		// extern void hb_buffer_set_direction(hb_buffer_t* buffer, hb_direction_t direction)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_direction (hb_buffer_t buffer, Direction direction);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_direction (hb_buffer_t buffer, Direction direction);
		}
		private static Delegates.hb_buffer_set_direction hb_buffer_set_direction_delegate;
		internal static void hb_buffer_set_direction (hb_buffer_t buffer, Direction direction) =>
			(hb_buffer_set_direction_delegate ??= GetSymbol<Delegates.hb_buffer_set_direction> ("hb_buffer_set_direction")).Invoke (buffer, direction);
		#endif

		// extern void hb_buffer_set_flags(hb_buffer_t* buffer, hb_buffer_flags_t flags)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_flags (hb_buffer_t buffer, BufferFlags flags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_flags (hb_buffer_t buffer, BufferFlags flags);
		}
		private static Delegates.hb_buffer_set_flags hb_buffer_set_flags_delegate;
		internal static void hb_buffer_set_flags (hb_buffer_t buffer, BufferFlags flags) =>
			(hb_buffer_set_flags_delegate ??= GetSymbol<Delegates.hb_buffer_set_flags> ("hb_buffer_set_flags")).Invoke (buffer, flags);
		#endif

		// extern void hb_buffer_set_invisible_glyph(hb_buffer_t* buffer, hb_codepoint_t invisible)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_invisible_glyph (hb_buffer_t buffer, UInt32 invisible);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_invisible_glyph (hb_buffer_t buffer, UInt32 invisible);
		}
		private static Delegates.hb_buffer_set_invisible_glyph hb_buffer_set_invisible_glyph_delegate;
		internal static void hb_buffer_set_invisible_glyph (hb_buffer_t buffer, UInt32 invisible) =>
			(hb_buffer_set_invisible_glyph_delegate ??= GetSymbol<Delegates.hb_buffer_set_invisible_glyph> ("hb_buffer_set_invisible_glyph")).Invoke (buffer, invisible);
		#endif

		// extern void hb_buffer_set_language(hb_buffer_t* buffer, hb_language_t language)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_language (hb_buffer_t buffer, IntPtr language);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_language (hb_buffer_t buffer, IntPtr language);
		}
		private static Delegates.hb_buffer_set_language hb_buffer_set_language_delegate;
		internal static void hb_buffer_set_language (hb_buffer_t buffer, IntPtr language) =>
			(hb_buffer_set_language_delegate ??= GetSymbol<Delegates.hb_buffer_set_language> ("hb_buffer_set_language")).Invoke (buffer, language);
		#endif

		// extern hb_bool_t hb_buffer_set_length(hb_buffer_t* buffer, unsigned int length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_buffer_set_length (hb_buffer_t buffer, UInt32 length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_buffer_set_length (hb_buffer_t buffer, UInt32 length);
		}
		private static Delegates.hb_buffer_set_length hb_buffer_set_length_delegate;
		internal static bool hb_buffer_set_length (hb_buffer_t buffer, UInt32 length) =>
			(hb_buffer_set_length_delegate ??= GetSymbol<Delegates.hb_buffer_set_length> ("hb_buffer_set_length")).Invoke (buffer, length);
		#endif

		// extern void hb_buffer_set_message_func(hb_buffer_t* buffer, hb_buffer_message_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_message_func (hb_buffer_t buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_message_func (hb_buffer_t buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_buffer_set_message_func hb_buffer_set_message_func_delegate;
		internal static void hb_buffer_set_message_func (hb_buffer_t buffer, BufferMessageProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_buffer_set_message_func_delegate ??= GetSymbol<Delegates.hb_buffer_set_message_func> ("hb_buffer_set_message_func")).Invoke (buffer, func, user_data, destroy);
		#endif

		// extern void hb_buffer_set_replacement_codepoint(hb_buffer_t* buffer, hb_codepoint_t replacement)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_replacement_codepoint (hb_buffer_t buffer, UInt32 replacement);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_replacement_codepoint (hb_buffer_t buffer, UInt32 replacement);
		}
		private static Delegates.hb_buffer_set_replacement_codepoint hb_buffer_set_replacement_codepoint_delegate;
		internal static void hb_buffer_set_replacement_codepoint (hb_buffer_t buffer, UInt32 replacement) =>
			(hb_buffer_set_replacement_codepoint_delegate ??= GetSymbol<Delegates.hb_buffer_set_replacement_codepoint> ("hb_buffer_set_replacement_codepoint")).Invoke (buffer, replacement);
		#endif

		// extern void hb_buffer_set_script(hb_buffer_t* buffer, hb_script_t script)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_script (hb_buffer_t buffer, UInt32 script);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_script (hb_buffer_t buffer, UInt32 script);
		}
		private static Delegates.hb_buffer_set_script hb_buffer_set_script_delegate;
		internal static void hb_buffer_set_script (hb_buffer_t buffer, UInt32 script) =>
			(hb_buffer_set_script_delegate ??= GetSymbol<Delegates.hb_buffer_set_script> ("hb_buffer_set_script")).Invoke (buffer, script);
		#endif

		// extern void hb_buffer_set_unicode_funcs(hb_buffer_t* buffer, hb_unicode_funcs_t* unicode_funcs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_buffer_set_unicode_funcs (hb_buffer_t buffer, hb_unicode_funcs_t unicode_funcs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_buffer_set_unicode_funcs (hb_buffer_t buffer, hb_unicode_funcs_t unicode_funcs);
		}
		private static Delegates.hb_buffer_set_unicode_funcs hb_buffer_set_unicode_funcs_delegate;
		internal static void hb_buffer_set_unicode_funcs (hb_buffer_t buffer, hb_unicode_funcs_t unicode_funcs) =>
			(hb_buffer_set_unicode_funcs_delegate ??= GetSymbol<Delegates.hb_buffer_set_unicode_funcs> ("hb_buffer_set_unicode_funcs")).Invoke (buffer, unicode_funcs);
		#endif

		// extern hb_glyph_flags_t hb_glyph_info_get_glyph_flags(const hb_glyph_info_t* info)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern GlyphFlags hb_glyph_info_get_glyph_flags (GlyphInfo* info);
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

		#region hb-common.h

		// extern uint8_t hb_color_get_alpha(hb_color_t color)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_alpha (UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_alpha (UInt32 color);
		}
		private static Delegates.hb_color_get_alpha hb_color_get_alpha_delegate;
		internal static Byte hb_color_get_alpha (UInt32 color) =>
			(hb_color_get_alpha_delegate ??= GetSymbol<Delegates.hb_color_get_alpha> ("hb_color_get_alpha")).Invoke (color);
		#endif

		// extern uint8_t hb_color_get_blue(hb_color_t color)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_blue (UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_blue (UInt32 color);
		}
		private static Delegates.hb_color_get_blue hb_color_get_blue_delegate;
		internal static Byte hb_color_get_blue (UInt32 color) =>
			(hb_color_get_blue_delegate ??= GetSymbol<Delegates.hb_color_get_blue> ("hb_color_get_blue")).Invoke (color);
		#endif

		// extern uint8_t hb_color_get_green(hb_color_t color)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_green (UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_green (UInt32 color);
		}
		private static Delegates.hb_color_get_green hb_color_get_green_delegate;
		internal static Byte hb_color_get_green (UInt32 color) =>
			(hb_color_get_green_delegate ??= GetSymbol<Delegates.hb_color_get_green> ("hb_color_get_green")).Invoke (color);
		#endif

		// extern uint8_t hb_color_get_red(hb_color_t color)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_red (UInt32 color);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_red (UInt32 color);
		}
		private static Delegates.hb_color_get_red hb_color_get_red_delegate;
		internal static Byte hb_color_get_red (UInt32 color) =>
			(hb_color_get_red_delegate ??= GetSymbol<Delegates.hb_color_get_red> ("hb_color_get_red")).Invoke (color);
		#endif

		// extern hb_direction_t hb_direction_from_string(const char* str, int len)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Direction hb_direction_from_string (/* char */ void* str, Int32 len);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Direction hb_direction_from_string (/* char */ void* str, Int32 len);
		}
		private static Delegates.hb_direction_from_string hb_direction_from_string_delegate;
		internal static Direction hb_direction_from_string (/* char */ void* str, Int32 len) =>
			(hb_direction_from_string_delegate ??= GetSymbol<Delegates.hb_direction_from_string> ("hb_direction_from_string")).Invoke (str, len);
		#endif

		// extern const char* hb_direction_to_string(hb_direction_t direction)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_direction_to_string (Direction direction);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_direction_to_string (Direction direction);
		}
		private static Delegates.hb_direction_to_string hb_direction_to_string_delegate;
		internal static /* char */ void* hb_direction_to_string (Direction direction) =>
			(hb_direction_to_string_delegate ??= GetSymbol<Delegates.hb_direction_to_string> ("hb_direction_to_string")).Invoke (direction);
		#endif

		// extern hb_bool_t hb_feature_from_string(const char* str, int len, hb_feature_t* feature)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature);
		}
		private static Delegates.hb_feature_from_string hb_feature_from_string_delegate;
		internal static bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature) =>
			(hb_feature_from_string_delegate ??= GetSymbol<Delegates.hb_feature_from_string> ("hb_feature_from_string")).Invoke (str, len, feature);
		#endif

		// extern void hb_feature_to_string(hb_feature_t* feature, char* buf, unsigned int size)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size);
		}
		private static Delegates.hb_feature_to_string hb_feature_to_string_delegate;
		internal static void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size) =>
			(hb_feature_to_string_delegate ??= GetSymbol<Delegates.hb_feature_to_string> ("hb_feature_to_string")).Invoke (feature, buf, size);
		#endif

		// extern hb_language_t hb_language_from_string(const char* str, int len)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		}
		private static Delegates.hb_language_from_string hb_language_from_string_delegate;
		internal static IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len) =>
			(hb_language_from_string_delegate ??= GetSymbol<Delegates.hb_language_from_string> ("hb_language_from_string")).Invoke (str, len);
		#endif

		// extern hb_language_t hb_language_get_default()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_language_get_default ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_language_get_default ();
		}
		private static Delegates.hb_language_get_default hb_language_get_default_delegate;
		internal static IntPtr hb_language_get_default () =>
			(hb_language_get_default_delegate ??= GetSymbol<Delegates.hb_language_get_default> ("hb_language_get_default")).Invoke ();
		#endif

		// extern const char* hb_language_to_string(hb_language_t language)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_language_to_string (IntPtr language);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_language_to_string (IntPtr language);
		}
		private static Delegates.hb_language_to_string hb_language_to_string_delegate;
		internal static /* char */ void* hb_language_to_string (IntPtr language) =>
			(hb_language_to_string_delegate ??= GetSymbol<Delegates.hb_language_to_string> ("hb_language_to_string")).Invoke (language);
		#endif

		// extern hb_script_t hb_script_from_iso15924_tag(hb_tag_t tag)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_script_from_iso15924_tag (UInt32 tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_script_from_iso15924_tag (UInt32 tag);
		}
		private static Delegates.hb_script_from_iso15924_tag hb_script_from_iso15924_tag_delegate;
		internal static UInt32 hb_script_from_iso15924_tag (UInt32 tag) =>
			(hb_script_from_iso15924_tag_delegate ??= GetSymbol<Delegates.hb_script_from_iso15924_tag> ("hb_script_from_iso15924_tag")).Invoke (tag);
		#endif

		// extern hb_script_t hb_script_from_string(const char* str, int len)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		}
		private static Delegates.hb_script_from_string hb_script_from_string_delegate;
		internal static UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len) =>
			(hb_script_from_string_delegate ??= GetSymbol<Delegates.hb_script_from_string> ("hb_script_from_string")).Invoke (str, len);
		#endif

		// extern hb_direction_t hb_script_get_horizontal_direction(hb_script_t script)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Direction hb_script_get_horizontal_direction (UInt32 script);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Direction hb_script_get_horizontal_direction (UInt32 script);
		}
		private static Delegates.hb_script_get_horizontal_direction hb_script_get_horizontal_direction_delegate;
		internal static Direction hb_script_get_horizontal_direction (UInt32 script) =>
			(hb_script_get_horizontal_direction_delegate ??= GetSymbol<Delegates.hb_script_get_horizontal_direction> ("hb_script_get_horizontal_direction")).Invoke (script);
		#endif

		// extern hb_tag_t hb_script_to_iso15924_tag(hb_script_t script)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_script_to_iso15924_tag (UInt32 script);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_script_to_iso15924_tag (UInt32 script);
		}
		private static Delegates.hb_script_to_iso15924_tag hb_script_to_iso15924_tag_delegate;
		internal static UInt32 hb_script_to_iso15924_tag (UInt32 script) =>
			(hb_script_to_iso15924_tag_delegate ??= GetSymbol<Delegates.hb_script_to_iso15924_tag> ("hb_script_to_iso15924_tag")).Invoke (script);
		#endif

		// extern hb_tag_t hb_tag_from_string(const char* str, int len)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_tag_from_string (/* char */ void* str, Int32 len);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_tag_from_string (/* char */ void* str, Int32 len);
		}
		private static Delegates.hb_tag_from_string hb_tag_from_string_delegate;
		internal static UInt32 hb_tag_from_string (/* char */ void* str, Int32 len) =>
			(hb_tag_from_string_delegate ??= GetSymbol<Delegates.hb_tag_from_string> ("hb_tag_from_string")).Invoke (str, len);
		#endif

		// extern void hb_tag_to_string(hb_tag_t tag, char* buf)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_tag_to_string (UInt32 tag, /* char */ void* buf);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_tag_to_string (UInt32 tag, /* char */ void* buf);
		}
		private static Delegates.hb_tag_to_string hb_tag_to_string_delegate;
		internal static void hb_tag_to_string (UInt32 tag, /* char */ void* buf) =>
			(hb_tag_to_string_delegate ??= GetSymbol<Delegates.hb_tag_to_string> ("hb_tag_to_string")).Invoke (tag, buf);
		#endif

		// extern hb_bool_t hb_variation_from_string(const char* str, int len, hb_variation_t* variation)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation);
		}
		private static Delegates.hb_variation_from_string hb_variation_from_string_delegate;
		internal static bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation) =>
			(hb_variation_from_string_delegate ??= GetSymbol<Delegates.hb_variation_from_string> ("hb_variation_from_string")).Invoke (str, len, variation);
		#endif

		// extern void hb_variation_to_string(hb_variation_t* variation, char* buf, unsigned int size)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size);
		}
		private static Delegates.hb_variation_to_string hb_variation_to_string_delegate;
		internal static void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size) =>
			(hb_variation_to_string_delegate ??= GetSymbol<Delegates.hb_variation_to_string> ("hb_variation_to_string")).Invoke (variation, buf, size);
		#endif

		#endregion

		#region hb-face.h

		// extern hb_bool_t hb_face_builder_add_table(hb_face_t* face, hb_tag_t tag, hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_face_builder_add_table (hb_face_t face, UInt32 tag, hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_face_builder_add_table (hb_face_t face, UInt32 tag, hb_blob_t blob);
		}
		private static Delegates.hb_face_builder_add_table hb_face_builder_add_table_delegate;
		internal static bool hb_face_builder_add_table (hb_face_t face, UInt32 tag, hb_blob_t blob) =>
			(hb_face_builder_add_table_delegate ??= GetSymbol<Delegates.hb_face_builder_add_table> ("hb_face_builder_add_table")).Invoke (face, tag, blob);
		#endif

		// extern hb_face_t* hb_face_builder_create()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_face_builder_create ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_face_t hb_face_builder_create ();
		}
		private static Delegates.hb_face_builder_create hb_face_builder_create_delegate;
		internal static hb_face_t hb_face_builder_create () =>
			(hb_face_builder_create_delegate ??= GetSymbol<Delegates.hb_face_builder_create> ("hb_face_builder_create")).Invoke ();
		#endif

		// extern void hb_face_collect_unicodes(hb_face_t* face, hb_set_t* out)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_collect_unicodes (hb_face_t face, hb_set_t @out);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_collect_unicodes (hb_face_t face, hb_set_t @out);
		}
		private static Delegates.hb_face_collect_unicodes hb_face_collect_unicodes_delegate;
		internal static void hb_face_collect_unicodes (hb_face_t face, hb_set_t @out) =>
			(hb_face_collect_unicodes_delegate ??= GetSymbol<Delegates.hb_face_collect_unicodes> ("hb_face_collect_unicodes")).Invoke (face, @out);
		#endif

		// extern void hb_face_collect_variation_selectors(hb_face_t* face, hb_set_t* out)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_collect_variation_selectors (hb_face_t face, hb_set_t @out);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_collect_variation_selectors (hb_face_t face, hb_set_t @out);
		}
		private static Delegates.hb_face_collect_variation_selectors hb_face_collect_variation_selectors_delegate;
		internal static void hb_face_collect_variation_selectors (hb_face_t face, hb_set_t @out) =>
			(hb_face_collect_variation_selectors_delegate ??= GetSymbol<Delegates.hb_face_collect_variation_selectors> ("hb_face_collect_variation_selectors")).Invoke (face, @out);
		#endif

		// extern void hb_face_collect_variation_unicodes(hb_face_t* face, hb_codepoint_t variation_selector, hb_set_t* out)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_collect_variation_unicodes (hb_face_t face, UInt32 variation_selector, hb_set_t @out);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_collect_variation_unicodes (hb_face_t face, UInt32 variation_selector, hb_set_t @out);
		}
		private static Delegates.hb_face_collect_variation_unicodes hb_face_collect_variation_unicodes_delegate;
		internal static void hb_face_collect_variation_unicodes (hb_face_t face, UInt32 variation_selector, hb_set_t @out) =>
			(hb_face_collect_variation_unicodes_delegate ??= GetSymbol<Delegates.hb_face_collect_variation_unicodes> ("hb_face_collect_variation_unicodes")).Invoke (face, variation_selector, @out);
		#endif

		// extern unsigned int hb_face_count(hb_blob_t* blob)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_count (hb_blob_t blob);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_count (hb_blob_t blob);
		}
		private static Delegates.hb_face_count hb_face_count_delegate;
		internal static UInt32 hb_face_count (hb_blob_t blob) =>
			(hb_face_count_delegate ??= GetSymbol<Delegates.hb_face_count> ("hb_face_count")).Invoke (blob);
		#endif

		// extern hb_face_t* hb_face_create(hb_blob_t* blob, unsigned int index)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_face_create (hb_blob_t blob, UInt32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_face_t hb_face_create (hb_blob_t blob, UInt32 index);
		}
		private static Delegates.hb_face_create hb_face_create_delegate;
		internal static hb_face_t hb_face_create (hb_blob_t blob, UInt32 index) =>
			(hb_face_create_delegate ??= GetSymbol<Delegates.hb_face_create> ("hb_face_create")).Invoke (blob, index);
		#endif

		// extern hb_face_t* hb_face_create_for_tables(hb_reference_table_func_t reference_table_func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_face_create_for_tables (ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_face_t hb_face_create_for_tables (ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_face_create_for_tables hb_face_create_for_tables_delegate;
		internal static hb_face_t hb_face_create_for_tables (ReferenceTableProxyDelegate reference_table_func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_face_create_for_tables_delegate ??= GetSymbol<Delegates.hb_face_create_for_tables> ("hb_face_create_for_tables")).Invoke (reference_table_func, user_data, destroy);
		#endif

		// extern void hb_face_destroy(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_destroy (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_destroy (hb_face_t face);
		}
		private static Delegates.hb_face_destroy hb_face_destroy_delegate;
		internal static void hb_face_destroy (hb_face_t face) =>
			(hb_face_destroy_delegate ??= GetSymbol<Delegates.hb_face_destroy> ("hb_face_destroy")).Invoke (face);
		#endif

		// extern hb_face_t* hb_face_get_empty()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_face_get_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_face_t hb_face_get_empty ();
		}
		private static Delegates.hb_face_get_empty hb_face_get_empty_delegate;
		internal static hb_face_t hb_face_get_empty () =>
			(hb_face_get_empty_delegate ??= GetSymbol<Delegates.hb_face_get_empty> ("hb_face_get_empty")).Invoke ();
		#endif

		// extern unsigned int hb_face_get_glyph_count(const hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_glyph_count (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_glyph_count (hb_face_t face);
		}
		private static Delegates.hb_face_get_glyph_count hb_face_get_glyph_count_delegate;
		internal static UInt32 hb_face_get_glyph_count (hb_face_t face) =>
			(hb_face_get_glyph_count_delegate ??= GetSymbol<Delegates.hb_face_get_glyph_count> ("hb_face_get_glyph_count")).Invoke (face);
		#endif

		// extern unsigned int hb_face_get_index(const hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_index (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_index (hb_face_t face);
		}
		private static Delegates.hb_face_get_index hb_face_get_index_delegate;
		internal static UInt32 hb_face_get_index (hb_face_t face) =>
			(hb_face_get_index_delegate ??= GetSymbol<Delegates.hb_face_get_index> ("hb_face_get_index")).Invoke (face);
		#endif

		// extern unsigned int hb_face_get_table_tags(const hb_face_t* face, unsigned int start_offset, unsigned int* table_count, hb_tag_t* table_tags)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_table_tags (hb_face_t face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_table_tags (hb_face_t face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags);
		}
		private static Delegates.hb_face_get_table_tags hb_face_get_table_tags_delegate;
		internal static UInt32 hb_face_get_table_tags (hb_face_t face, UInt32 start_offset, UInt32* table_count, UInt32* table_tags) =>
			(hb_face_get_table_tags_delegate ??= GetSymbol<Delegates.hb_face_get_table_tags> ("hb_face_get_table_tags")).Invoke (face, start_offset, table_count, table_tags);
		#endif

		// extern unsigned int hb_face_get_upem(const hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_face_get_upem (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_face_get_upem (hb_face_t face);
		}
		private static Delegates.hb_face_get_upem hb_face_get_upem_delegate;
		internal static UInt32 hb_face_get_upem (hb_face_t face) =>
			(hb_face_get_upem_delegate ??= GetSymbol<Delegates.hb_face_get_upem> ("hb_face_get_upem")).Invoke (face);
		#endif

		// extern hb_bool_t hb_face_is_immutable(const hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_face_is_immutable (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_face_is_immutable (hb_face_t face);
		}
		private static Delegates.hb_face_is_immutable hb_face_is_immutable_delegate;
		internal static bool hb_face_is_immutable (hb_face_t face) =>
			(hb_face_is_immutable_delegate ??= GetSymbol<Delegates.hb_face_is_immutable> ("hb_face_is_immutable")).Invoke (face);
		#endif

		// extern void hb_face_make_immutable(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_make_immutable (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_make_immutable (hb_face_t face);
		}
		private static Delegates.hb_face_make_immutable hb_face_make_immutable_delegate;
		internal static void hb_face_make_immutable (hb_face_t face) =>
			(hb_face_make_immutable_delegate ??= GetSymbol<Delegates.hb_face_make_immutable> ("hb_face_make_immutable")).Invoke (face);
		#endif

		// extern hb_face_t* hb_face_reference(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_face_reference (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_face_t hb_face_reference (hb_face_t face);
		}
		private static Delegates.hb_face_reference hb_face_reference_delegate;
		internal static hb_face_t hb_face_reference (hb_face_t face) =>
			(hb_face_reference_delegate ??= GetSymbol<Delegates.hb_face_reference> ("hb_face_reference")).Invoke (face);
		#endif

		// extern hb_blob_t* hb_face_reference_blob(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_face_reference_blob (hb_face_t face);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_face_reference_blob (hb_face_t face);
		}
		private static Delegates.hb_face_reference_blob hb_face_reference_blob_delegate;
		internal static hb_blob_t hb_face_reference_blob (hb_face_t face) =>
			(hb_face_reference_blob_delegate ??= GetSymbol<Delegates.hb_face_reference_blob> ("hb_face_reference_blob")).Invoke (face);
		#endif

		// extern hb_blob_t* hb_face_reference_table(const hb_face_t* face, hb_tag_t tag)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_face_reference_table (hb_face_t face, UInt32 tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_face_reference_table (hb_face_t face, UInt32 tag);
		}
		private static Delegates.hb_face_reference_table hb_face_reference_table_delegate;
		internal static hb_blob_t hb_face_reference_table (hb_face_t face, UInt32 tag) =>
			(hb_face_reference_table_delegate ??= GetSymbol<Delegates.hb_face_reference_table> ("hb_face_reference_table")).Invoke (face, tag);
		#endif

		// extern void hb_face_set_glyph_count(hb_face_t* face, unsigned int glyph_count)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_set_glyph_count (hb_face_t face, UInt32 glyph_count);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_set_glyph_count (hb_face_t face, UInt32 glyph_count);
		}
		private static Delegates.hb_face_set_glyph_count hb_face_set_glyph_count_delegate;
		internal static void hb_face_set_glyph_count (hb_face_t face, UInt32 glyph_count) =>
			(hb_face_set_glyph_count_delegate ??= GetSymbol<Delegates.hb_face_set_glyph_count> ("hb_face_set_glyph_count")).Invoke (face, glyph_count);
		#endif

		// extern void hb_face_set_index(hb_face_t* face, unsigned int index)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_set_index (hb_face_t face, UInt32 index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_set_index (hb_face_t face, UInt32 index);
		}
		private static Delegates.hb_face_set_index hb_face_set_index_delegate;
		internal static void hb_face_set_index (hb_face_t face, UInt32 index) =>
			(hb_face_set_index_delegate ??= GetSymbol<Delegates.hb_face_set_index> ("hb_face_set_index")).Invoke (face, index);
		#endif

		// extern void hb_face_set_upem(hb_face_t* face, unsigned int upem)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_face_set_upem (hb_face_t face, UInt32 upem);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_face_set_upem (hb_face_t face, UInt32 upem);
		}
		private static Delegates.hb_face_set_upem hb_face_set_upem_delegate;
		internal static void hb_face_set_upem (hb_face_t face, UInt32 upem) =>
			(hb_face_set_upem_delegate ??= GetSymbol<Delegates.hb_face_set_upem> ("hb_face_set_upem")).Invoke (face, upem);
		#endif

		#endregion

		#region hb-font.h

		// extern void hb_font_add_glyph_origin_for_direction(hb_font_t* font, hb_codepoint_t glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
		}
		private static Delegates.hb_font_add_glyph_origin_for_direction hb_font_add_glyph_origin_for_direction_delegate;
		internal static void hb_font_add_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y) =>
			(hb_font_add_glyph_origin_for_direction_delegate ??= GetSymbol<Delegates.hb_font_add_glyph_origin_for_direction> ("hb_font_add_glyph_origin_for_direction")).Invoke (font, glyph, direction, x, y);
		#endif

		// extern hb_font_t* hb_font_create(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_create (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_create_sub_font (hb_font_t parent);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_destroy (hb_font_t font);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_funcs_t hb_font_funcs_create ();
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_destroy (hb_font_funcs_t ffuncs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_funcs_t hb_font_funcs_get_empty ();
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_funcs_is_immutable (hb_font_funcs_t ffuncs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_make_immutable (hb_font_funcs_t ffuncs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_funcs_t hb_font_funcs_reference (hb_font_funcs_t ffuncs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_font_h_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_font_v_extents_func (hb_font_funcs_t ffuncs, FontGetFontExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_contour_point_func (hb_font_funcs_t ffuncs, FontGetGlyphContourPointProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_extents_func (hb_font_funcs_t ffuncs, FontGetGlyphExtentsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_from_name_func (hb_font_funcs_t ffuncs, FontGetGlyphFromNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_kerning_func (hb_font_funcs_t ffuncs, FontGetGlyphKerningProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_h_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_name_func (hb_font_funcs_t ffuncs, FontGetGlyphNameProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_v_advance_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvanceProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_v_advances_func (hb_font_funcs_t ffuncs, FontGetGlyphAdvancesProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_glyph_v_origin_func (hb_font_funcs_t ffuncs, FontGetGlyphOriginProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_nominal_glyph_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_nominal_glyphs_func (hb_font_funcs_t ffuncs, FontGetNominalGlyphsProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_funcs_set_variation_glyph_func (hb_font_funcs_t ffuncs, FontGetVariationGlyphProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_get_empty ();
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_extents_for_direction (hb_font_t font, Direction direction, FontExtents* extents);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_face_t hb_font_get_face (hb_font_t font);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_advance_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_advances_for_direction (hb_font_t font, Direction direction, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_contour_point (hb_font_t font, UInt32 glyph, UInt32 point_index, Int32* x, Int32* y);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_contour_point_for_origin (hb_font_t font, UInt32 glyph, UInt32 point_index, Direction direction, Int32* x, Int32* y);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_extents (hb_font_t font, UInt32 glyph, GlyphExtents* extents);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_extents_for_origin (hb_font_t font, UInt32 glyph, Direction direction, GlyphExtents* extents);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_from_name (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String name, Int32 len, UInt32* glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_font_get_glyph_h_advance (hb_font_t font, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_h_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_font_get_glyph_h_kerning (hb_font_t font, UInt32 left_glyph, UInt32 right_glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_h_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
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

		// extern void hb_font_get_glyph_kerning_for_direction(hb_font_t* font, hb_codepoint_t first_glyph, hb_codepoint_t second_glyph, hb_direction_t direction, hb_position_t* x, hb_position_t* y)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_kerning_for_direction (hb_font_t font, UInt32 first_glyph, UInt32 second_glyph, Direction direction, Int32* x, Int32* y);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_name (hb_font_t font, UInt32 glyph, /* char */ void* name, UInt32 size);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_font_get_glyph_v_advance (hb_font_t font, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_glyph_v_advances (hb_font_t font, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_glyph_v_origin (hb_font_t font, UInt32 glyph, Int32* x, Int32* y);
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

		// extern hb_bool_t hb_font_get_h_extents(hb_font_t* font, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_h_extents (hb_font_t font, FontExtents* extents);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_nominal_glyph (hb_font_t font, UInt32 unicode, UInt32* glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_font_get_nominal_glyphs (hb_font_t font, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_get_parent (hb_font_t font);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_ppem (hb_font_t font, UInt32* x_ppem, UInt32* y_ppem);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single hb_font_get_ptem (hb_font_t font);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale);
		}
		private static Delegates.hb_font_get_scale hb_font_get_scale_delegate;
		internal static void hb_font_get_scale (hb_font_t font, Int32* x_scale, Int32* y_scale) =>
			(hb_font_get_scale_delegate ??= GetSymbol<Delegates.hb_font_get_scale> ("hb_font_get_scale")).Invoke (font, x_scale, y_scale);
		#endif

		// extern hb_bool_t hb_font_get_v_extents(hb_font_t* font, hb_font_extents_t* extents)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_v_extents (hb_font_t font, FontExtents* extents);
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

		// extern const int* hb_font_get_var_coords_normalized(hb_font_t* font, unsigned int* length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length);
		}
		private static Delegates.hb_font_get_var_coords_normalized hb_font_get_var_coords_normalized_delegate;
		internal static Int32* hb_font_get_var_coords_normalized (hb_font_t font, UInt32* length) =>
			(hb_font_get_var_coords_normalized_delegate ??= GetSymbol<Delegates.hb_font_get_var_coords_normalized> ("hb_font_get_var_coords_normalized")).Invoke (font, length);
		#endif

		// extern hb_bool_t hb_font_get_variation_glyph(hb_font_t* font, hb_codepoint_t unicode, hb_codepoint_t variation_selector, hb_codepoint_t* glyph)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_get_variation_glyph (hb_font_t font, UInt32 unicode, UInt32 variation_selector, UInt32* glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_glyph_from_string (hb_font_t font, [MarshalAs (UnmanagedType.LPStr)] String s, Int32 len, UInt32* glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_glyph_to_string (hb_font_t font, UInt32 glyph, /* char */ void* s, UInt32 size);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_font_is_immutable (hb_font_t font);
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

		// extern void hb_font_make_immutable(hb_font_t* font)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_make_immutable (hb_font_t font);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_font_t hb_font_reference (hb_font_t font);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_face (hb_font_t font, hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_funcs (hb_font_t font, hb_font_funcs_t klass, void* font_data, DestroyProxyDelegate destroy);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_funcs_data (hb_font_t font, void* font_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_funcs_data (hb_font_t font, void* font_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_font_set_funcs_data hb_font_set_funcs_data_delegate;
		internal static void hb_font_set_funcs_data (hb_font_t font, void* font_data, DestroyProxyDelegate destroy) =>
			(hb_font_set_funcs_data_delegate ??= GetSymbol<Delegates.hb_font_set_funcs_data> ("hb_font_set_funcs_data")).Invoke (font, font_data, destroy);
		#endif

		// extern void hb_font_set_parent(hb_font_t* font, hb_font_t* parent)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_parent (hb_font_t font, hb_font_t parent);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_ppem (hb_font_t font, UInt32 x_ppem, UInt32 y_ppem);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_ptem (hb_font_t font, Single ptem);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale);
		}
		private static Delegates.hb_font_set_scale hb_font_set_scale_delegate;
		internal static void hb_font_set_scale (hb_font_t font, Int32 x_scale, Int32 y_scale) =>
			(hb_font_set_scale_delegate ??= GetSymbol<Delegates.hb_font_set_scale> ("hb_font_set_scale")).Invoke (font, x_scale, y_scale);
		#endif

		// extern void hb_font_set_var_coords_design(hb_font_t* font, const float* coords, unsigned int coords_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_var_coords_design (hb_font_t font, Single* coords, UInt32 coords_length);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_var_coords_normalized (hb_font_t font, Int32* coords, UInt32 coords_length);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index);
		}
		private static Delegates.hb_font_set_var_named_instance hb_font_set_var_named_instance_delegate;
		internal static void hb_font_set_var_named_instance (hb_font_t font, UInt32 instance_index) =>
			(hb_font_set_var_named_instance_delegate ??= GetSymbol<Delegates.hb_font_set_var_named_instance> ("hb_font_set_var_named_instance")).Invoke (font, instance_index);
		#endif

		// extern void hb_font_set_variations(hb_font_t* font, const hb_variation_t* variations, unsigned int variations_length)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_set_variations (hb_font_t font, Variation* variations, UInt32 variations_length);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_font_subtract_glyph_origin_for_direction (hb_font_t font, UInt32 glyph, Direction direction, Int32* x, Int32* y);
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

		#region hb-map.h

		// extern hb_bool_t hb_map_allocation_successful(const hb_map_t* map)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_allocation_successful (hb_map_t map);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_allocation_successful (hb_map_t map);
		}
		private static Delegates.hb_map_allocation_successful hb_map_allocation_successful_delegate;
		internal static bool hb_map_allocation_successful (hb_map_t map) =>
			(hb_map_allocation_successful_delegate ??= GetSymbol<Delegates.hb_map_allocation_successful> ("hb_map_allocation_successful")).Invoke (map);
		#endif

		// extern void hb_map_clear(hb_map_t* map)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_clear (hb_map_t map);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_clear (hb_map_t map);
		}
		private static Delegates.hb_map_clear hb_map_clear_delegate;
		internal static void hb_map_clear (hb_map_t map) =>
			(hb_map_clear_delegate ??= GetSymbol<Delegates.hb_map_clear> ("hb_map_clear")).Invoke (map);
		#endif

		// extern hb_map_t* hb_map_create()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_map_t hb_map_create ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_map_t hb_map_create ();
		}
		private static Delegates.hb_map_create hb_map_create_delegate;
		internal static hb_map_t hb_map_create () =>
			(hb_map_create_delegate ??= GetSymbol<Delegates.hb_map_create> ("hb_map_create")).Invoke ();
		#endif

		// extern void hb_map_del(hb_map_t* map, hb_codepoint_t key)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_del (hb_map_t map, UInt32 key);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_del (hb_map_t map, UInt32 key);
		}
		private static Delegates.hb_map_del hb_map_del_delegate;
		internal static void hb_map_del (hb_map_t map, UInt32 key) =>
			(hb_map_del_delegate ??= GetSymbol<Delegates.hb_map_del> ("hb_map_del")).Invoke (map, key);
		#endif

		// extern void hb_map_destroy(hb_map_t* map)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_destroy (hb_map_t map);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_destroy (hb_map_t map);
		}
		private static Delegates.hb_map_destroy hb_map_destroy_delegate;
		internal static void hb_map_destroy (hb_map_t map) =>
			(hb_map_destroy_delegate ??= GetSymbol<Delegates.hb_map_destroy> ("hb_map_destroy")).Invoke (map);
		#endif

		// extern hb_codepoint_t hb_map_get(const hb_map_t* map, hb_codepoint_t key)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_map_get (hb_map_t map, UInt32 key);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_map_get (hb_map_t map, UInt32 key);
		}
		private static Delegates.hb_map_get hb_map_get_delegate;
		internal static UInt32 hb_map_get (hb_map_t map, UInt32 key) =>
			(hb_map_get_delegate ??= GetSymbol<Delegates.hb_map_get> ("hb_map_get")).Invoke (map, key);
		#endif

		// extern hb_map_t* hb_map_get_empty()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_map_t hb_map_get_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_map_t hb_map_get_empty ();
		}
		private static Delegates.hb_map_get_empty hb_map_get_empty_delegate;
		internal static hb_map_t hb_map_get_empty () =>
			(hb_map_get_empty_delegate ??= GetSymbol<Delegates.hb_map_get_empty> ("hb_map_get_empty")).Invoke ();
		#endif

		// extern unsigned int hb_map_get_population(const hb_map_t* map)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_map_get_population (hb_map_t map);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_map_get_population (hb_map_t map);
		}
		private static Delegates.hb_map_get_population hb_map_get_population_delegate;
		internal static UInt32 hb_map_get_population (hb_map_t map) =>
			(hb_map_get_population_delegate ??= GetSymbol<Delegates.hb_map_get_population> ("hb_map_get_population")).Invoke (map);
		#endif

		// extern hb_bool_t hb_map_has(const hb_map_t* map, hb_codepoint_t key)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_has (hb_map_t map, UInt32 key);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_has (hb_map_t map, UInt32 key);
		}
		private static Delegates.hb_map_has hb_map_has_delegate;
		internal static bool hb_map_has (hb_map_t map, UInt32 key) =>
			(hb_map_has_delegate ??= GetSymbol<Delegates.hb_map_has> ("hb_map_has")).Invoke (map, key);
		#endif

		// extern hb_bool_t hb_map_is_empty(const hb_map_t* map)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_is_empty (hb_map_t map);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_is_empty (hb_map_t map);
		}
		private static Delegates.hb_map_is_empty hb_map_is_empty_delegate;
		internal static bool hb_map_is_empty (hb_map_t map) =>
			(hb_map_is_empty_delegate ??= GetSymbol<Delegates.hb_map_is_empty> ("hb_map_is_empty")).Invoke (map);
		#endif

		// extern hb_map_t* hb_map_reference(hb_map_t* map)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_map_t hb_map_reference (hb_map_t map);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_map_t hb_map_reference (hb_map_t map);
		}
		private static Delegates.hb_map_reference hb_map_reference_delegate;
		internal static hb_map_t hb_map_reference (hb_map_t map) =>
			(hb_map_reference_delegate ??= GetSymbol<Delegates.hb_map_reference> ("hb_map_reference")).Invoke (map);
		#endif

		// extern void hb_map_set(hb_map_t* map, hb_codepoint_t key, hb_codepoint_t value)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_set (hb_map_t map, UInt32 key, UInt32 value);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_set (hb_map_t map, UInt32 key, UInt32 value);
		}
		private static Delegates.hb_map_set hb_map_set_delegate;
		internal static void hb_map_set (hb_map_t map, UInt32 key, UInt32 value) =>
			(hb_map_set_delegate ??= GetSymbol<Delegates.hb_map_set> ("hb_map_set")).Invoke (map, key, value);
		#endif

		#endregion

		#region hb-ot-color.h

		// extern unsigned int hb_ot_color_glyph_get_layers(hb_face_t* face, hb_codepoint_t glyph, unsigned int start_offset, unsigned int* layer_count, hb_ot_color_layer_t* layers)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers);
		}
		private static Delegates.hb_ot_color_glyph_get_layers hb_ot_color_glyph_get_layers_delegate;
		internal static UInt32 hb_ot_color_glyph_get_layers (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* layer_count, OpenTypeColorLayer* layers) =>
			(hb_ot_color_glyph_get_layers_delegate ??= GetSymbol<Delegates.hb_ot_color_glyph_get_layers> ("hb_ot_color_glyph_get_layers")).Invoke (face, glyph, start_offset, layer_count, layers);
		#endif

		// extern hb_blob_t* hb_ot_color_glyph_reference_png(hb_font_t* font, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_ot_color_glyph_reference_png (hb_font_t font, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_ot_color_glyph_reference_svg (hb_face_t face, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_layers (hb_face_t face);
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

		// extern hb_bool_t hb_ot_color_has_palettes(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_palettes (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_png (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_color_has_svg (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameId hb_ot_color_palette_color_get_name_id (hb_face_t face, UInt32 color_index);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, UInt32* colors);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, UInt32* colors);
		}
		private static Delegates.hb_ot_color_palette_get_colors hb_ot_color_palette_get_colors_delegate;
		internal static UInt32 hb_ot_color_palette_get_colors (hb_face_t face, UInt32 palette_index, UInt32 start_offset, UInt32* color_count, UInt32* colors) =>
			(hb_ot_color_palette_get_colors_delegate ??= GetSymbol<Delegates.hb_ot_color_palette_get_colors> ("hb_ot_color_palette_get_colors")).Invoke (face, palette_index, start_offset, color_count, colors);
		#endif

		// extern unsigned int hb_ot_color_palette_get_count(hb_face_t* face)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_color_palette_get_count (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeColorPaletteFlags hb_ot_color_palette_get_flags (hb_face_t face, UInt32 palette_index);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameId hb_ot_color_palette_get_name_id (hb_face_t face, UInt32 palette_index);
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

		#region hb-ot-font.h

		// extern void hb_ot_font_set_funcs(hb_font_t* font)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_font_set_funcs (hb_font_t font);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_font_set_funcs (hb_font_t font);
		}
		private static Delegates.hb_ot_font_set_funcs hb_ot_font_set_funcs_delegate;
		internal static void hb_ot_font_set_funcs (hb_font_t font) =>
			(hb_ot_font_set_funcs_delegate ??= GetSymbol<Delegates.hb_ot_font_set_funcs> ("hb_ot_font_set_funcs")).Invoke (font);
		#endif

		#endregion

		#region hb-ot-layout.h

		// extern void hb_ot_layout_collect_features(hb_face_t* face, hb_tag_t table_tag, const hb_tag_t* scripts, const hb_tag_t* languages, const hb_tag_t* features, hb_set_t* feature_indexes)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes);
		}
		private static Delegates.hb_ot_layout_collect_features hb_ot_layout_collect_features_delegate;
		internal static void hb_ot_layout_collect_features (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t feature_indexes) =>
			(hb_ot_layout_collect_features_delegate ??= GetSymbol<Delegates.hb_ot_layout_collect_features> ("hb_ot_layout_collect_features")).Invoke (face, table_tag, scripts, languages, features, feature_indexes);
		#endif

		// extern void hb_ot_layout_collect_lookups(hb_face_t* face, hb_tag_t table_tag, const hb_tag_t* scripts, const hb_tag_t* languages, const hb_tag_t* features, hb_set_t* lookup_indexes)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_collect_lookups (hb_face_t face, UInt32 table_tag, UInt32* scripts, UInt32* languages, UInt32* features, hb_set_t lookup_indexes);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_feature_get_characters (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* char_count, UInt32* characters);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_feature_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_feature_get_name_ids (hb_face_t face, UInt32 table_tag, UInt32 feature_index, OpenTypeNameId* label_id, OpenTypeNameId* tooltip_id, OpenTypeNameId* sample_id, UInt32* num_named_parameters, OpenTypeNameId* first_param_id);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_feature_with_variations_get_lookups (hb_face_t face, UInt32 table_tag, UInt32 feature_index, UInt32 variations_index, UInt32 start_offset, UInt32* lookup_count, UInt32* lookup_indexes);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_get_attach_points (hb_face_t face, UInt32 glyph, UInt32 start_offset, UInt32* point_count, UInt32* point_array);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_baseline (hb_font_t font, OpenTypeLayoutBaselineTag baseline_tag, Direction direction, UInt32 script_tag, UInt32 language_tag, Int32* coord);
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

		// extern hb_ot_layout_glyph_class_t hb_ot_layout_get_glyph_class(hb_face_t* face, hb_codepoint_t glyph)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeLayoutGlyphClass hb_ot_layout_get_glyph_class (hb_face_t face, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs);
		}
		private static Delegates.hb_ot_layout_get_glyphs_in_class hb_ot_layout_get_glyphs_in_class_delegate;
		internal static void hb_ot_layout_get_glyphs_in_class (hb_face_t face, OpenTypeLayoutGlyphClass klass, hb_set_t glyphs) =>
			(hb_ot_layout_get_glyphs_in_class_delegate ??= GetSymbol<Delegates.hb_ot_layout_get_glyphs_in_class> ("hb_ot_layout_get_glyphs_in_class")).Invoke (face, klass, glyphs);
		#endif

		// extern unsigned int hb_ot_layout_get_ligature_carets(hb_font_t* font, hb_direction_t direction, hb_codepoint_t glyph, unsigned int start_offset, unsigned int* caret_count, hb_position_t* caret_array)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_get_ligature_carets (hb_font_t font, Direction direction, UInt32 glyph, UInt32 start_offset, UInt32* caret_count, Int32* caret_array);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_get_size_params (hb_face_t face, UInt32* design_size, UInt32* subfamily_id, OpenTypeNameId* subfamily_name_id, UInt32* range_start, UInt32* range_end);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_has_glyph_classes (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_has_positioning (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_has_substitution (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_language_find_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 feature_tag, UInt32* feature_index);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_language_get_feature_indexes (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_indexes);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_language_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_language_get_required_feature (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index, UInt32* feature_tag);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_language_get_required_feature_index (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_index, UInt32* feature_index);
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

		// extern void hb_ot_layout_lookup_collect_glyphs(hb_face_t* face, hb_tag_t table_tag, unsigned int lookup_index, hb_set_t* glyphs_before, hb_set_t* glyphs_input, hb_set_t* glyphs_after, hb_set_t* glyphs_output)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_lookup_collect_glyphs (hb_face_t face, UInt32 table_tag, UInt32 lookup_index, hb_set_t glyphs_before, hb_set_t glyphs_input, hb_set_t glyphs_after, hb_set_t glyphs_output);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs);
		}
		private static Delegates.hb_ot_layout_lookup_get_glyph_alternates hb_ot_layout_lookup_get_glyph_alternates_delegate;
		internal static UInt32 hb_ot_layout_lookup_get_glyph_alternates (hb_face_t face, UInt32 lookup_index, UInt32 glyph, UInt32 start_offset, UInt32* alternate_count, UInt32* alternate_glyphs) =>
			(hb_ot_layout_lookup_get_glyph_alternates_delegate ??= GetSymbol<Delegates.hb_ot_layout_lookup_get_glyph_alternates> ("hb_ot_layout_lookup_get_glyph_alternates")).Invoke (face, lookup_index, glyph, start_offset, alternate_count, alternate_glyphs);
		#endif

		// extern void hb_ot_layout_lookup_substitute_closure(hb_face_t* face, unsigned int lookup_index, hb_set_t* glyphs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_lookup_substitute_closure (hb_face_t face, UInt32 lookup_index, hb_set_t glyphs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_lookup_would_substitute (hb_face_t face, UInt32 lookup_index, UInt32* glyphs, UInt32 glyphs_length, [MarshalAs (UnmanagedType.I1)] bool zero_context);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_layout_lookups_substitute_closure (hb_face_t face, hb_set_t lookups, hb_set_t glyphs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_script_get_language_tags (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 start_offset, UInt32* language_count, UInt32* language_tags);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_script_select_language (hb_face_t face, UInt32 table_tag, UInt32 script_index, UInt32 language_count, UInt32* language_tags, UInt32* language_index);
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

		// extern hb_bool_t hb_ot_layout_table_find_feature_variations(hb_face_t* face, hb_tag_t table_tag, const int* coords, unsigned int num_coords, unsigned int* variations_index)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_table_find_feature_variations (hb_face_t face, UInt32 table_tag, Int32* coords, UInt32 num_coords, UInt32* variations_index);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_table_find_script (hb_face_t face, UInt32 table_tag, UInt32 script_tag, UInt32* script_index);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_table_get_feature_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* feature_count, UInt32* feature_tags);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_table_get_lookup_count (hb_face_t face, UInt32 table_tag);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_layout_table_get_script_tags (hb_face_t face, UInt32 table_tag, UInt32 start_offset, UInt32* script_count, UInt32* script_tags);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_layout_table_select_script (hb_face_t face, UInt32 table_tag, UInt32 script_count, UInt32* script_tags, UInt32* script_index, UInt32* chosen_script);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_ot_tag_to_language (UInt32 tag);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_tag_to_script (UInt32 tag);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_tags_from_script_and_language (UInt32 script, IntPtr language, UInt32* script_count, UInt32* script_tags, UInt32* language_count, UInt32* language_tags);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_tags_to_script_and_language (UInt32 script_tag, UInt32 language_tag, UInt32* script, IntPtr* language);
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

		#region hb-ot-math.h

		// extern hb_position_t hb_ot_math_get_constant(hb_font_t* font, hb_ot_math_constant_t constant)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_constant (hb_font_t font, OpenTypeMathConstant constant);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_math_get_glyph_assembly (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* parts_count, OpenTypeMathGlyphPart* parts, Int32* italics_correction);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_glyph_italics_correction (hb_font_t font, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_glyph_kerning (hb_font_t font, UInt32 glyph, OpenTypeMathKern kern, Int32 correction_height);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_glyph_top_accent_attachment (hb_font_t font, UInt32 glyph);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_math_get_glyph_variants (hb_font_t font, UInt32 glyph, Direction direction, UInt32 start_offset, UInt32* variants_count, OpenTypeMathGlyphVariant* variants);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_math_get_min_connector_overlap (hb_font_t font, Direction direction);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_math_has_data (hb_face_t face);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_math_is_glyph_extended_shape (hb_face_t face, UInt32 glyph);
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

		#region hb-ot-meta.h

		// extern unsigned int hb_ot_meta_get_entry_tags(hb_face_t* face, unsigned int start_offset, unsigned int* entries_count, hb_ot_meta_tag_t* entries)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_meta_get_entry_tags (hb_face_t face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_meta_get_entry_tags (hb_face_t face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries);
		}
		private static Delegates.hb_ot_meta_get_entry_tags hb_ot_meta_get_entry_tags_delegate;
		internal static UInt32 hb_ot_meta_get_entry_tags (hb_face_t face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries) =>
			(hb_ot_meta_get_entry_tags_delegate ??= GetSymbol<Delegates.hb_ot_meta_get_entry_tags> ("hb_ot_meta_get_entry_tags")).Invoke (face, start_offset, entries_count, entries);
		#endif

		// extern hb_blob_t* hb_ot_meta_reference_entry(hb_face_t* face, hb_ot_meta_tag_t meta_tag)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_ot_meta_reference_entry (hb_face_t face, OpenTypeMetaTag meta_tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_blob_t hb_ot_meta_reference_entry (hb_face_t face, OpenTypeMetaTag meta_tag);
		}
		private static Delegates.hb_ot_meta_reference_entry hb_ot_meta_reference_entry_delegate;
		internal static hb_blob_t hb_ot_meta_reference_entry (hb_face_t face, OpenTypeMetaTag meta_tag) =>
			(hb_ot_meta_reference_entry_delegate ??= GetSymbol<Delegates.hb_ot_meta_reference_entry> ("hb_ot_meta_reference_entry")).Invoke (face, meta_tag);
		#endif

		#endregion

		#region hb-ot-metrics.h

		// extern hb_bool_t hb_ot_metrics_get_position(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag, hb_position_t* position)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_metrics_get_position (hb_font_t font, OpenTypeMetricsTag metrics_tag, Int32* position);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_metrics_get_position (hb_font_t font, OpenTypeMetricsTag metrics_tag, Int32* position);
		}
		private static Delegates.hb_ot_metrics_get_position hb_ot_metrics_get_position_delegate;
		internal static bool hb_ot_metrics_get_position (hb_font_t font, OpenTypeMetricsTag metrics_tag, Int32* position) =>
			(hb_ot_metrics_get_position_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_position> ("hb_ot_metrics_get_position")).Invoke (font, metrics_tag, position);
		#endif

		// extern float hb_ot_metrics_get_variation(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single hb_ot_metrics_get_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single hb_ot_metrics_get_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		}
		private static Delegates.hb_ot_metrics_get_variation hb_ot_metrics_get_variation_delegate;
		internal static Single hb_ot_metrics_get_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag) =>
			(hb_ot_metrics_get_variation_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_variation> ("hb_ot_metrics_get_variation")).Invoke (font, metrics_tag);
		#endif

		// extern hb_position_t hb_ot_metrics_get_x_variation(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_metrics_get_x_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_metrics_get_x_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		}
		private static Delegates.hb_ot_metrics_get_x_variation hb_ot_metrics_get_x_variation_delegate;
		internal static Int32 hb_ot_metrics_get_x_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag) =>
			(hb_ot_metrics_get_x_variation_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_x_variation> ("hb_ot_metrics_get_x_variation")).Invoke (font, metrics_tag);
		#endif

		// extern hb_position_t hb_ot_metrics_get_y_variation(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_metrics_get_y_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_metrics_get_y_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag);
		}
		private static Delegates.hb_ot_metrics_get_y_variation hb_ot_metrics_get_y_variation_delegate;
		internal static Int32 hb_ot_metrics_get_y_variation (hb_font_t font, OpenTypeMetricsTag metrics_tag) =>
			(hb_ot_metrics_get_y_variation_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_y_variation> ("hb_ot_metrics_get_y_variation")).Invoke (font, metrics_tag);
		#endif

		#endregion

		#region hb-ot-name.h

		// extern unsigned int hb_ot_name_get_utf16(hb_face_t* face, hb_ot_name_id_t name_id, hb_language_t language, unsigned int* text_size, uint16_t* text)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_name_get_utf16 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_name_get_utf16 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text);
		}
		private static Delegates.hb_ot_name_get_utf16 hb_ot_name_get_utf16_delegate;
		internal static UInt32 hb_ot_name_get_utf16 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text) =>
			(hb_ot_name_get_utf16_delegate ??= GetSymbol<Delegates.hb_ot_name_get_utf16> ("hb_ot_name_get_utf16")).Invoke (face, name_id, language, text_size, text);
		#endif

		// extern unsigned int hb_ot_name_get_utf32(hb_face_t* face, hb_ot_name_id_t name_id, hb_language_t language, unsigned int* text_size, uint32_t* text)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_name_get_utf32 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_name_get_utf32 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text);
		}
		private static Delegates.hb_ot_name_get_utf32 hb_ot_name_get_utf32_delegate;
		internal static UInt32 hb_ot_name_get_utf32 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text) =>
			(hb_ot_name_get_utf32_delegate ??= GetSymbol<Delegates.hb_ot_name_get_utf32> ("hb_ot_name_get_utf32")).Invoke (face, name_id, language, text_size, text);
		#endif

		// extern unsigned int hb_ot_name_get_utf8(hb_face_t* face, hb_ot_name_id_t name_id, hb_language_t language, unsigned int* text_size, char* text)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_name_get_utf8 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_name_get_utf8 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text);
		}
		private static Delegates.hb_ot_name_get_utf8 hb_ot_name_get_utf8_delegate;
		internal static UInt32 hb_ot_name_get_utf8 (hb_face_t face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text) =>
			(hb_ot_name_get_utf8_delegate ??= GetSymbol<Delegates.hb_ot_name_get_utf8> ("hb_ot_name_get_utf8")).Invoke (face, name_id, language, text_size, text);
		#endif

		// extern const hb_ot_name_entry_t* hb_ot_name_list_names(hb_face_t* face, unsigned int* num_entries)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameEntry* hb_ot_name_list_names (hb_face_t face, UInt32* num_entries);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeNameEntry* hb_ot_name_list_names (hb_face_t face, UInt32* num_entries);
		}
		private static Delegates.hb_ot_name_list_names hb_ot_name_list_names_delegate;
		internal static OpenTypeNameEntry* hb_ot_name_list_names (hb_face_t face, UInt32* num_entries) =>
			(hb_ot_name_list_names_delegate ??= GetSymbol<Delegates.hb_ot_name_list_names> ("hb_ot_name_list_names")).Invoke (face, num_entries);
		#endif

		#endregion

		#region hb-ot-shape.h

		// extern void hb_ot_shape_glyphs_closure(hb_font_t* font, hb_buffer_t* buffer, const hb_feature_t* features, unsigned int num_features, hb_set_t* glyphs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_shape_glyphs_closure (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, hb_set_t glyphs);
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
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes);
		}
		private static Delegates.hb_ot_shape_plan_collect_lookups hb_ot_shape_plan_collect_lookups_delegate;
		internal static void hb_ot_shape_plan_collect_lookups (hb_shape_plan_t shape_plan, UInt32 table_tag, hb_set_t lookup_indexes) =>
			(hb_ot_shape_plan_collect_lookups_delegate ??= GetSymbol<Delegates.hb_ot_shape_plan_collect_lookups> ("hb_ot_shape_plan_collect_lookups")).Invoke (shape_plan, table_tag, lookup_indexes);
		#endif

		#endregion

		#region hb-set.h

		// extern void hb_set_add(hb_set_t* set, hb_codepoint_t codepoint)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_add (hb_set_t set, UInt32 codepoint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_add (hb_set_t set, UInt32 codepoint);
		}
		private static Delegates.hb_set_add hb_set_add_delegate;
		internal static void hb_set_add (hb_set_t set, UInt32 codepoint) =>
			(hb_set_add_delegate ??= GetSymbol<Delegates.hb_set_add> ("hb_set_add")).Invoke (set, codepoint);
		#endif

		// extern void hb_set_add_range(hb_set_t* set, hb_codepoint_t first, hb_codepoint_t last)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last);
		}
		private static Delegates.hb_set_add_range hb_set_add_range_delegate;
		internal static void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last) =>
			(hb_set_add_range_delegate ??= GetSymbol<Delegates.hb_set_add_range> ("hb_set_add_range")).Invoke (set, first, last);
		#endif

		// extern hb_bool_t hb_set_allocation_successful(const hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_allocation_successful (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_allocation_successful (hb_set_t set);
		}
		private static Delegates.hb_set_allocation_successful hb_set_allocation_successful_delegate;
		internal static bool hb_set_allocation_successful (hb_set_t set) =>
			(hb_set_allocation_successful_delegate ??= GetSymbol<Delegates.hb_set_allocation_successful> ("hb_set_allocation_successful")).Invoke (set);
		#endif

		// extern void hb_set_clear(hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_clear (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_clear (hb_set_t set);
		}
		private static Delegates.hb_set_clear hb_set_clear_delegate;
		internal static void hb_set_clear (hb_set_t set) =>
			(hb_set_clear_delegate ??= GetSymbol<Delegates.hb_set_clear> ("hb_set_clear")).Invoke (set);
		#endif

		// extern hb_set_t* hb_set_copy(const hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_copy (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_copy (hb_set_t set);
		}
		private static Delegates.hb_set_copy hb_set_copy_delegate;
		internal static hb_set_t hb_set_copy (hb_set_t set) =>
			(hb_set_copy_delegate ??= GetSymbol<Delegates.hb_set_copy> ("hb_set_copy")).Invoke (set);
		#endif

		// extern hb_set_t* hb_set_create()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_create ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_create ();
		}
		private static Delegates.hb_set_create hb_set_create_delegate;
		internal static hb_set_t hb_set_create () =>
			(hb_set_create_delegate ??= GetSymbol<Delegates.hb_set_create> ("hb_set_create")).Invoke ();
		#endif

		// extern void hb_set_del(hb_set_t* set, hb_codepoint_t codepoint)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_del (hb_set_t set, UInt32 codepoint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_del (hb_set_t set, UInt32 codepoint);
		}
		private static Delegates.hb_set_del hb_set_del_delegate;
		internal static void hb_set_del (hb_set_t set, UInt32 codepoint) =>
			(hb_set_del_delegate ??= GetSymbol<Delegates.hb_set_del> ("hb_set_del")).Invoke (set, codepoint);
		#endif

		// extern void hb_set_del_range(hb_set_t* set, hb_codepoint_t first, hb_codepoint_t last)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last);
		}
		private static Delegates.hb_set_del_range hb_set_del_range_delegate;
		internal static void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last) =>
			(hb_set_del_range_delegate ??= GetSymbol<Delegates.hb_set_del_range> ("hb_set_del_range")).Invoke (set, first, last);
		#endif

		// extern void hb_set_destroy(hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_destroy (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_destroy (hb_set_t set);
		}
		private static Delegates.hb_set_destroy hb_set_destroy_delegate;
		internal static void hb_set_destroy (hb_set_t set) =>
			(hb_set_destroy_delegate ??= GetSymbol<Delegates.hb_set_destroy> ("hb_set_destroy")).Invoke (set);
		#endif

		// extern hb_set_t* hb_set_get_empty()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_get_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_get_empty ();
		}
		private static Delegates.hb_set_get_empty hb_set_get_empty_delegate;
		internal static hb_set_t hb_set_get_empty () =>
			(hb_set_get_empty_delegate ??= GetSymbol<Delegates.hb_set_get_empty> ("hb_set_get_empty")).Invoke ();
		#endif

		// extern hb_codepoint_t hb_set_get_max(const hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_get_max (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_get_max (hb_set_t set);
		}
		private static Delegates.hb_set_get_max hb_set_get_max_delegate;
		internal static UInt32 hb_set_get_max (hb_set_t set) =>
			(hb_set_get_max_delegate ??= GetSymbol<Delegates.hb_set_get_max> ("hb_set_get_max")).Invoke (set);
		#endif

		// extern hb_codepoint_t hb_set_get_min(const hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_get_min (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_get_min (hb_set_t set);
		}
		private static Delegates.hb_set_get_min hb_set_get_min_delegate;
		internal static UInt32 hb_set_get_min (hb_set_t set) =>
			(hb_set_get_min_delegate ??= GetSymbol<Delegates.hb_set_get_min> ("hb_set_get_min")).Invoke (set);
		#endif

		// extern unsigned int hb_set_get_population(const hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_get_population (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_get_population (hb_set_t set);
		}
		private static Delegates.hb_set_get_population hb_set_get_population_delegate;
		internal static UInt32 hb_set_get_population (hb_set_t set) =>
			(hb_set_get_population_delegate ??= GetSymbol<Delegates.hb_set_get_population> ("hb_set_get_population")).Invoke (set);
		#endif

		// extern hb_bool_t hb_set_has(const hb_set_t* set, hb_codepoint_t codepoint)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_has (hb_set_t set, UInt32 codepoint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_has (hb_set_t set, UInt32 codepoint);
		}
		private static Delegates.hb_set_has hb_set_has_delegate;
		internal static bool hb_set_has (hb_set_t set, UInt32 codepoint) =>
			(hb_set_has_delegate ??= GetSymbol<Delegates.hb_set_has> ("hb_set_has")).Invoke (set, codepoint);
		#endif

		// extern void hb_set_intersect(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_intersect (hb_set_t set, hb_set_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_intersect (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_intersect hb_set_intersect_delegate;
		internal static void hb_set_intersect (hb_set_t set, hb_set_t other) =>
			(hb_set_intersect_delegate ??= GetSymbol<Delegates.hb_set_intersect> ("hb_set_intersect")).Invoke (set, other);
		#endif

		// extern hb_bool_t hb_set_is_empty(const hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_empty (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_empty (hb_set_t set);
		}
		private static Delegates.hb_set_is_empty hb_set_is_empty_delegate;
		internal static bool hb_set_is_empty (hb_set_t set) =>
			(hb_set_is_empty_delegate ??= GetSymbol<Delegates.hb_set_is_empty> ("hb_set_is_empty")).Invoke (set);
		#endif

		// extern hb_bool_t hb_set_is_equal(const hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_equal (hb_set_t set, hb_set_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_equal (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_is_equal hb_set_is_equal_delegate;
		internal static bool hb_set_is_equal (hb_set_t set, hb_set_t other) =>
			(hb_set_is_equal_delegate ??= GetSymbol<Delegates.hb_set_is_equal> ("hb_set_is_equal")).Invoke (set, other);
		#endif

		// extern hb_bool_t hb_set_is_subset(const hb_set_t* set, const hb_set_t* larger_set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set);
		}
		private static Delegates.hb_set_is_subset hb_set_is_subset_delegate;
		internal static bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set) =>
			(hb_set_is_subset_delegate ??= GetSymbol<Delegates.hb_set_is_subset> ("hb_set_is_subset")).Invoke (set, larger_set);
		#endif

		// extern hb_bool_t hb_set_next(const hb_set_t* set, hb_codepoint_t* codepoint)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_next (hb_set_t set, UInt32* codepoint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_next (hb_set_t set, UInt32* codepoint);
		}
		private static Delegates.hb_set_next hb_set_next_delegate;
		internal static bool hb_set_next (hb_set_t set, UInt32* codepoint) =>
			(hb_set_next_delegate ??= GetSymbol<Delegates.hb_set_next> ("hb_set_next")).Invoke (set, codepoint);
		#endif

		// extern hb_bool_t hb_set_next_range(const hb_set_t* set, hb_codepoint_t* first, hb_codepoint_t* last)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last);
		}
		private static Delegates.hb_set_next_range hb_set_next_range_delegate;
		internal static bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last) =>
			(hb_set_next_range_delegate ??= GetSymbol<Delegates.hb_set_next_range> ("hb_set_next_range")).Invoke (set, first, last);
		#endif

		// extern hb_bool_t hb_set_previous(const hb_set_t* set, hb_codepoint_t* codepoint)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_previous (hb_set_t set, UInt32* codepoint);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_previous (hb_set_t set, UInt32* codepoint);
		}
		private static Delegates.hb_set_previous hb_set_previous_delegate;
		internal static bool hb_set_previous (hb_set_t set, UInt32* codepoint) =>
			(hb_set_previous_delegate ??= GetSymbol<Delegates.hb_set_previous> ("hb_set_previous")).Invoke (set, codepoint);
		#endif

		// extern hb_bool_t hb_set_previous_range(const hb_set_t* set, hb_codepoint_t* first, hb_codepoint_t* last)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last);
		}
		private static Delegates.hb_set_previous_range hb_set_previous_range_delegate;
		internal static bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last) =>
			(hb_set_previous_range_delegate ??= GetSymbol<Delegates.hb_set_previous_range> ("hb_set_previous_range")).Invoke (set, first, last);
		#endif

		// extern hb_set_t* hb_set_reference(hb_set_t* set)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_reference (hb_set_t set);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_reference (hb_set_t set);
		}
		private static Delegates.hb_set_reference hb_set_reference_delegate;
		internal static hb_set_t hb_set_reference (hb_set_t set) =>
			(hb_set_reference_delegate ??= GetSymbol<Delegates.hb_set_reference> ("hb_set_reference")).Invoke (set);
		#endif

		// extern void hb_set_set(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_set (hb_set_t set, hb_set_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_set (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_set hb_set_set_delegate;
		internal static void hb_set_set (hb_set_t set, hb_set_t other) =>
			(hb_set_set_delegate ??= GetSymbol<Delegates.hb_set_set> ("hb_set_set")).Invoke (set, other);
		#endif

		// extern void hb_set_subtract(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_subtract (hb_set_t set, hb_set_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_subtract (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_subtract hb_set_subtract_delegate;
		internal static void hb_set_subtract (hb_set_t set, hb_set_t other) =>
			(hb_set_subtract_delegate ??= GetSymbol<Delegates.hb_set_subtract> ("hb_set_subtract")).Invoke (set, other);
		#endif

		// extern void hb_set_symmetric_difference(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_symmetric_difference (hb_set_t set, hb_set_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_symmetric_difference (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_symmetric_difference hb_set_symmetric_difference_delegate;
		internal static void hb_set_symmetric_difference (hb_set_t set, hb_set_t other) =>
			(hb_set_symmetric_difference_delegate ??= GetSymbol<Delegates.hb_set_symmetric_difference> ("hb_set_symmetric_difference")).Invoke (set, other);
		#endif

		// extern void hb_set_union(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_union (hb_set_t set, hb_set_t other);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_union (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_union hb_set_union_delegate;
		internal static void hb_set_union (hb_set_t set, hb_set_t other) =>
			(hb_set_union_delegate ??= GetSymbol<Delegates.hb_set_union> ("hb_set_union")).Invoke (set, other);
		#endif

		#endregion

		#region hb-shape.h

		// extern void hb_shape(hb_font_t* font, hb_buffer_t* buffer, const hb_feature_t* features, unsigned int num_features)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_shape (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_shape (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features);
		}
		private static Delegates.hb_shape hb_shape_delegate;
		internal static void hb_shape (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features) =>
			(hb_shape_delegate ??= GetSymbol<Delegates.hb_shape> ("hb_shape")).Invoke (font, buffer, features, num_features);
		#endif

		// extern hb_bool_t hb_shape_full(hb_font_t* font, hb_buffer_t* buffer, const hb_feature_t* features, unsigned int num_features, const const char** shaper_list)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_shape_full (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_shape_full (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list);
		}
		private static Delegates.hb_shape_full hb_shape_full_delegate;
		internal static bool hb_shape_full (hb_font_t font, hb_buffer_t buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list) =>
			(hb_shape_full_delegate ??= GetSymbol<Delegates.hb_shape_full> ("hb_shape_full")).Invoke (font, buffer, features, num_features, shaper_list);
		#endif

		// extern const char** hb_shape_list_shapers()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void** hb_shape_list_shapers ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void** hb_shape_list_shapers ();
		}
		private static Delegates.hb_shape_list_shapers hb_shape_list_shapers_delegate;
		internal static /* char */ void** hb_shape_list_shapers () =>
			(hb_shape_list_shapers_delegate ??= GetSymbol<Delegates.hb_shape_list_shapers> ("hb_shape_list_shapers")).Invoke ();
		#endif

		#endregion

		#region hb-unicode.h

		// extern hb_unicode_combining_class_t hb_unicode_combining_class(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UnicodeCombiningClass hb_unicode_combining_class (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UnicodeCombiningClass hb_unicode_combining_class (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_combining_class hb_unicode_combining_class_delegate;
		internal static UnicodeCombiningClass hb_unicode_combining_class (hb_unicode_funcs_t ufuncs, UInt32 unicode) =>
			(hb_unicode_combining_class_delegate ??= GetSymbol<Delegates.hb_unicode_combining_class> ("hb_unicode_combining_class")).Invoke (ufuncs, unicode);
		#endif

		// extern hb_bool_t hb_unicode_compose(hb_unicode_funcs_t* ufuncs, hb_codepoint_t a, hb_codepoint_t b, hb_codepoint_t* ab)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_unicode_compose (hb_unicode_funcs_t ufuncs, UInt32 a, UInt32 b, UInt32* ab);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_unicode_compose (hb_unicode_funcs_t ufuncs, UInt32 a, UInt32 b, UInt32* ab);
		}
		private static Delegates.hb_unicode_compose hb_unicode_compose_delegate;
		internal static bool hb_unicode_compose (hb_unicode_funcs_t ufuncs, UInt32 a, UInt32 b, UInt32* ab) =>
			(hb_unicode_compose_delegate ??= GetSymbol<Delegates.hb_unicode_compose> ("hb_unicode_compose")).Invoke (ufuncs, a, b, ab);
		#endif

		// extern hb_bool_t hb_unicode_decompose(hb_unicode_funcs_t* ufuncs, hb_codepoint_t ab, hb_codepoint_t* a, hb_codepoint_t* b)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_unicode_decompose (hb_unicode_funcs_t ufuncs, UInt32 ab, UInt32* a, UInt32* b);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_unicode_decompose (hb_unicode_funcs_t ufuncs, UInt32 ab, UInt32* a, UInt32* b);
		}
		private static Delegates.hb_unicode_decompose hb_unicode_decompose_delegate;
		internal static bool hb_unicode_decompose (hb_unicode_funcs_t ufuncs, UInt32 ab, UInt32* a, UInt32* b) =>
			(hb_unicode_decompose_delegate ??= GetSymbol<Delegates.hb_unicode_decompose> ("hb_unicode_decompose")).Invoke (ufuncs, ab, a, b);
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_create(hb_unicode_funcs_t* parent)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_unicode_funcs_t hb_unicode_funcs_create (hb_unicode_funcs_t parent);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_unicode_funcs_t hb_unicode_funcs_create (hb_unicode_funcs_t parent);
		}
		private static Delegates.hb_unicode_funcs_create hb_unicode_funcs_create_delegate;
		internal static hb_unicode_funcs_t hb_unicode_funcs_create (hb_unicode_funcs_t parent) =>
			(hb_unicode_funcs_create_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_create> ("hb_unicode_funcs_create")).Invoke (parent);
		#endif

		// extern void hb_unicode_funcs_destroy(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_destroy (hb_unicode_funcs_t ufuncs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_destroy (hb_unicode_funcs_t ufuncs);
		}
		private static Delegates.hb_unicode_funcs_destroy hb_unicode_funcs_destroy_delegate;
		internal static void hb_unicode_funcs_destroy (hb_unicode_funcs_t ufuncs) =>
			(hb_unicode_funcs_destroy_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_destroy> ("hb_unicode_funcs_destroy")).Invoke (ufuncs);
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_get_default()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_unicode_funcs_t hb_unicode_funcs_get_default ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_unicode_funcs_t hb_unicode_funcs_get_default ();
		}
		private static Delegates.hb_unicode_funcs_get_default hb_unicode_funcs_get_default_delegate;
		internal static hb_unicode_funcs_t hb_unicode_funcs_get_default () =>
			(hb_unicode_funcs_get_default_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_get_default> ("hb_unicode_funcs_get_default")).Invoke ();
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_get_empty()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_unicode_funcs_t hb_unicode_funcs_get_empty ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_unicode_funcs_t hb_unicode_funcs_get_empty ();
		}
		private static Delegates.hb_unicode_funcs_get_empty hb_unicode_funcs_get_empty_delegate;
		internal static hb_unicode_funcs_t hb_unicode_funcs_get_empty () =>
			(hb_unicode_funcs_get_empty_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_get_empty> ("hb_unicode_funcs_get_empty")).Invoke ();
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_get_parent(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_unicode_funcs_t hb_unicode_funcs_get_parent (hb_unicode_funcs_t ufuncs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_unicode_funcs_t hb_unicode_funcs_get_parent (hb_unicode_funcs_t ufuncs);
		}
		private static Delegates.hb_unicode_funcs_get_parent hb_unicode_funcs_get_parent_delegate;
		internal static hb_unicode_funcs_t hb_unicode_funcs_get_parent (hb_unicode_funcs_t ufuncs) =>
			(hb_unicode_funcs_get_parent_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_get_parent> ("hb_unicode_funcs_get_parent")).Invoke (ufuncs);
		#endif

		// extern hb_bool_t hb_unicode_funcs_is_immutable(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_unicode_funcs_is_immutable (hb_unicode_funcs_t ufuncs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_unicode_funcs_is_immutable (hb_unicode_funcs_t ufuncs);
		}
		private static Delegates.hb_unicode_funcs_is_immutable hb_unicode_funcs_is_immutable_delegate;
		internal static bool hb_unicode_funcs_is_immutable (hb_unicode_funcs_t ufuncs) =>
			(hb_unicode_funcs_is_immutable_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_is_immutable> ("hb_unicode_funcs_is_immutable")).Invoke (ufuncs);
		#endif

		// extern void hb_unicode_funcs_make_immutable(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_make_immutable (hb_unicode_funcs_t ufuncs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_make_immutable (hb_unicode_funcs_t ufuncs);
		}
		private static Delegates.hb_unicode_funcs_make_immutable hb_unicode_funcs_make_immutable_delegate;
		internal static void hb_unicode_funcs_make_immutable (hb_unicode_funcs_t ufuncs) =>
			(hb_unicode_funcs_make_immutable_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_make_immutable> ("hb_unicode_funcs_make_immutable")).Invoke (ufuncs);
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_reference(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_unicode_funcs_t hb_unicode_funcs_reference (hb_unicode_funcs_t ufuncs);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_unicode_funcs_t hb_unicode_funcs_reference (hb_unicode_funcs_t ufuncs);
		}
		private static Delegates.hb_unicode_funcs_reference hb_unicode_funcs_reference_delegate;
		internal static hb_unicode_funcs_t hb_unicode_funcs_reference (hb_unicode_funcs_t ufuncs) =>
			(hb_unicode_funcs_reference_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_reference> ("hb_unicode_funcs_reference")).Invoke (ufuncs);
		#endif

		// extern void hb_unicode_funcs_set_combining_class_func(hb_unicode_funcs_t* ufuncs, hb_unicode_combining_class_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_combining_class_func (hb_unicode_funcs_t ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_combining_class_func (hb_unicode_funcs_t ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_combining_class_func hb_unicode_funcs_set_combining_class_func_delegate;
		internal static void hb_unicode_funcs_set_combining_class_func (hb_unicode_funcs_t ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_combining_class_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_combining_class_func> ("hb_unicode_funcs_set_combining_class_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_compose_func(hb_unicode_funcs_t* ufuncs, hb_unicode_compose_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_compose_func (hb_unicode_funcs_t ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_compose_func (hb_unicode_funcs_t ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_compose_func hb_unicode_funcs_set_compose_func_delegate;
		internal static void hb_unicode_funcs_set_compose_func (hb_unicode_funcs_t ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_compose_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_compose_func> ("hb_unicode_funcs_set_compose_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_decompose_func(hb_unicode_funcs_t* ufuncs, hb_unicode_decompose_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_decompose_func (hb_unicode_funcs_t ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_decompose_func (hb_unicode_funcs_t ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_decompose_func hb_unicode_funcs_set_decompose_func_delegate;
		internal static void hb_unicode_funcs_set_decompose_func (hb_unicode_funcs_t ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_decompose_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_decompose_func> ("hb_unicode_funcs_set_decompose_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_general_category_func(hb_unicode_funcs_t* ufuncs, hb_unicode_general_category_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_general_category_func (hb_unicode_funcs_t ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_general_category_func (hb_unicode_funcs_t ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_general_category_func hb_unicode_funcs_set_general_category_func_delegate;
		internal static void hb_unicode_funcs_set_general_category_func (hb_unicode_funcs_t ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_general_category_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_general_category_func> ("hb_unicode_funcs_set_general_category_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_mirroring_func(hb_unicode_funcs_t* ufuncs, hb_unicode_mirroring_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_mirroring_func (hb_unicode_funcs_t ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_mirroring_func (hb_unicode_funcs_t ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_mirroring_func hb_unicode_funcs_set_mirroring_func_delegate;
		internal static void hb_unicode_funcs_set_mirroring_func (hb_unicode_funcs_t ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_mirroring_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_mirroring_func> ("hb_unicode_funcs_set_mirroring_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_script_func(hb_unicode_funcs_t* ufuncs, hb_unicode_script_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_script_func (hb_unicode_funcs_t ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_script_func (hb_unicode_funcs_t ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_script_func hb_unicode_funcs_set_script_func_delegate;
		internal static void hb_unicode_funcs_set_script_func (hb_unicode_funcs_t ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_script_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_script_func> ("hb_unicode_funcs_set_script_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern hb_unicode_general_category_t hb_unicode_general_category(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UnicodeGeneralCategory hb_unicode_general_category (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UnicodeGeneralCategory hb_unicode_general_category (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_general_category hb_unicode_general_category_delegate;
		internal static UnicodeGeneralCategory hb_unicode_general_category (hb_unicode_funcs_t ufuncs, UInt32 unicode) =>
			(hb_unicode_general_category_delegate ??= GetSymbol<Delegates.hb_unicode_general_category> ("hb_unicode_general_category")).Invoke (ufuncs, unicode);
		#endif

		// extern hb_codepoint_t hb_unicode_mirroring(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_unicode_mirroring (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_unicode_mirroring (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_mirroring hb_unicode_mirroring_delegate;
		internal static UInt32 hb_unicode_mirroring (hb_unicode_funcs_t ufuncs, UInt32 unicode) =>
			(hb_unicode_mirroring_delegate ??= GetSymbol<Delegates.hb_unicode_mirroring> ("hb_unicode_mirroring")).Invoke (ufuncs, unicode);
		#endif

		// extern hb_script_t hb_unicode_script(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_unicode_script (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_unicode_script (hb_unicode_funcs_t ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_script hb_unicode_script_delegate;
		internal static UInt32 hb_unicode_script (hb_unicode_funcs_t ufuncs, UInt32 unicode) =>
			(hb_unicode_script_delegate ??= GetSymbol<Delegates.hb_unicode_script> ("hb_unicode_script")).Invoke (ufuncs, unicode);
		#endif

		#endregion

		#region hb-version.h

		// extern void hb_version(unsigned int* major, unsigned int* minor, unsigned int* micro)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_version (UInt32* major, UInt32* minor, UInt32* micro);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_version (UInt32* major, UInt32* minor, UInt32* micro);
		}
		private static Delegates.hb_version hb_version_delegate;
		internal static void hb_version (UInt32* major, UInt32* minor, UInt32* micro) =>
			(hb_version_delegate ??= GetSymbol<Delegates.hb_version> ("hb_version")).Invoke (major, minor, micro);
		#endif

		// extern hb_bool_t hb_version_atleast(unsigned int major, unsigned int minor, unsigned int micro)
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_version_atleast (UInt32 major, UInt32 minor, UInt32 micro);
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_version_atleast (UInt32 major, UInt32 minor, UInt32 micro);
		}
		private static Delegates.hb_version_atleast hb_version_atleast_delegate;
		internal static bool hb_version_atleast (UInt32 major, UInt32 minor, UInt32 micro) =>
			(hb_version_atleast_delegate ??= GetSymbol<Delegates.hb_version_atleast> ("hb_version_atleast")).Invoke (major, minor, micro);
		#endif

		// extern const char* hb_version_string()
		#if !USE_DELEGATES
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_version_string ();
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_version_string ();
		}
		private static Delegates.hb_version_string hb_version_string_delegate;
		internal static /* char */ void* hb_version_string () =>
			(hb_version_string_delegate ??= GetSymbol<Delegates.hb_version_string> ("hb_version_string")).Invoke ();
		#endif

		#endregion

	}
}

#endregion Functions

#region Delegates

namespace HarfBuzzSharp {
	// typedef hb_bool_t (*)(hb_buffer_t* buffer, hb_font_t* font, const char* message, void* user_data)* hb_buffer_message_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool BufferMessageProxyDelegate(hb_buffer_t buffer, hb_font_t font, /* char */ void* message, void* user_data);

	// typedef void (*)(void* user_data)* hb_destroy_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void DestroyProxyDelegate(void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_font_extents_t* extents, void* user_data)* hb_font_get_font_extents_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetFontExtentsProxyDelegate(hb_font_t font, void* font_data, FontExtents* extents, void* user_data);

	// typedef hb_position_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t glyph, void* user_data)* hb_font_get_glyph_advance_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate Int32 FontGetGlyphAdvanceProxyDelegate(hb_font_t font, void* font_data, UInt32 glyph, void* user_data);

	// typedef void (*)(hb_font_t* font, void* font_data, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_advance, unsigned int advance_stride, void* user_data)* hb_font_get_glyph_advances_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void FontGetGlyphAdvancesProxyDelegate(hb_font_t font, void* font_data, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t glyph, unsigned int point_index, hb_position_t* x, hb_position_t* y, void* user_data)* hb_font_get_glyph_contour_point_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphContourPointProxyDelegate(hb_font_t font, void* font_data, UInt32 glyph, UInt32 point_index, Int32* x, Int32* y, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t glyph, hb_glyph_extents_t* extents, void* user_data)* hb_font_get_glyph_extents_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphExtentsProxyDelegate(hb_font_t font, void* font_data, UInt32 glyph, GlyphExtents* extents, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, const char* name, int len, hb_codepoint_t* glyph, void* user_data)* hb_font_get_glyph_from_name_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphFromNameProxyDelegate(hb_font_t font, void* font_data, /* char */ void* name, Int32 len, UInt32* glyph, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t unicode, hb_codepoint_t variation_selector, hb_codepoint_t* glyph, void* user_data)* hb_font_get_glyph_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphProxyDelegate(hb_font_t font, void* font_data, UInt32 unicode, UInt32 variation_selector, UInt32* glyph, void* user_data);

	// typedef hb_position_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t first_glyph, hb_codepoint_t second_glyph, void* user_data)* hb_font_get_glyph_kerning_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate Int32 FontGetGlyphKerningProxyDelegate(hb_font_t font, void* font_data, UInt32 first_glyph, UInt32 second_glyph, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t glyph, char* name, unsigned int size, void* user_data)* hb_font_get_glyph_name_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphNameProxyDelegate(hb_font_t font, void* font_data, UInt32 glyph, /* char */ void* name, UInt32 size, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t glyph, hb_position_t* x, hb_position_t* y, void* user_data)* hb_font_get_glyph_origin_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphOriginProxyDelegate(hb_font_t font, void* font_data, UInt32 glyph, Int32* x, Int32* y, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t unicode, hb_codepoint_t* glyph, void* user_data)* hb_font_get_nominal_glyph_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetNominalGlyphProxyDelegate(hb_font_t font, void* font_data, UInt32 unicode, UInt32* glyph, void* user_data);

	// typedef unsigned int (*)(hb_font_t* font, void* font_data, unsigned int count, const hb_codepoint_t* first_unicode, unsigned int unicode_stride, hb_codepoint_t* first_glyph, unsigned int glyph_stride, void* user_data)* hb_font_get_nominal_glyphs_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 FontGetNominalGlyphsProxyDelegate(hb_font_t font, void* font_data, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride, void* user_data);

	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t unicode, hb_codepoint_t variation_selector, hb_codepoint_t* glyph, void* user_data)* hb_font_get_variation_glyph_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetVariationGlyphProxyDelegate(hb_font_t font, void* font_data, UInt32 unicode, UInt32 variation_selector, UInt32* glyph, void* user_data);

// TODO: typedef const hb_language_impl_t* hb_language_t
	// typedef hb_blob_t* (*)(hb_face_t* face, hb_tag_t tag, void* user_data)* hb_reference_table_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate hb_blob_t ReferenceTableProxyDelegate(hb_face_t face, UInt32 tag, void* user_data);

	// typedef hb_unicode_combining_class_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode, void* user_data)* hb_unicode_combining_class_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate int UnicodeCombiningClassProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 unicode, void* user_data);

	// typedef hb_bool_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t a, hb_codepoint_t b, hb_codepoint_t* ab, void* user_data)* hb_unicode_compose_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool UnicodeComposeProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 a, UInt32 b, UInt32* ab, void* user_data);

	// typedef unsigned int (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t u, hb_codepoint_t* decomposed, void* user_data)* hb_unicode_decompose_compatibility_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 UnicodeDecomposeCompatibilityProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 u, UInt32* decomposed, void* user_data);

	// typedef hb_bool_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t ab, hb_codepoint_t* a, hb_codepoint_t* b, void* user_data)* hb_unicode_decompose_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool UnicodeDecomposeProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 ab, UInt32* a, UInt32* b, void* user_data);

	// typedef unsigned int (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode, void* user_data)* hb_unicode_eastasian_width_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 UnicodeEastasianWidthProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 unicode, void* user_data);

	// typedef hb_unicode_general_category_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode, void* user_data)* hb_unicode_general_category_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate int UnicodeGeneralCategoryProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 unicode, void* user_data);

	// typedef hb_codepoint_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode, void* user_data)* hb_unicode_mirroring_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 UnicodeMirroringProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 unicode, void* user_data);

	// typedef hb_script_t (*)(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode, void* user_data)* hb_unicode_script_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 UnicodeScriptProxyDelegate(hb_unicode_funcs_t ufuncs, UInt32 unicode, void* user_data);

}

#endregion

#region Structs

namespace HarfBuzzSharp {

	// hb_feature_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct Feature : IEquatable<Feature> {
		// public hb_tag_t tag
		private UInt32 tag;

		// public uint32_t value
		private UInt32 value;

		// public unsigned int start
		private UInt32 start;

		// public unsigned int end
		private UInt32 end;

		public readonly bool Equals (Feature obj) =>
			tag == obj.tag && value == obj.value && start == obj.start && end == obj.end;

		public readonly override bool Equals (object obj) =>
			obj is Feature f && Equals (f);

		public static bool operator == (Feature left, Feature right) =>
			left.Equals (right);

		public static bool operator != (Feature left, Feature right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (value);
			hash.Add (start);
			hash.Add (end);
			return hash.ToHashCode ();
		}

	}

	// hb_font_extents_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct FontExtents : IEquatable<FontExtents> {
		// public hb_position_t ascender
		private Int32 ascender;
		public Int32 Ascender {
			readonly get => ascender;
			set => ascender = value;
		}

		// public hb_position_t descender
		private Int32 descender;
		public Int32 Descender {
			readonly get => descender;
			set => descender = value;
		}

		// public hb_position_t line_gap
		private Int32 line_gap;
		public Int32 LineGap {
			readonly get => line_gap;
			set => line_gap = value;
		}

		// public hb_position_t reserved9
		private Int32 reserved9;

		// public hb_position_t reserved8
		private Int32 reserved8;

		// public hb_position_t reserved7
		private Int32 reserved7;

		// public hb_position_t reserved6
		private Int32 reserved6;

		// public hb_position_t reserved5
		private Int32 reserved5;

		// public hb_position_t reserved4
		private Int32 reserved4;

		// public hb_position_t reserved3
		private Int32 reserved3;

		// public hb_position_t reserved2
		private Int32 reserved2;

		// public hb_position_t reserved1
		private Int32 reserved1;

		public readonly bool Equals (FontExtents obj) =>
			ascender == obj.ascender && descender == obj.descender && line_gap == obj.line_gap && reserved9 == obj.reserved9 && reserved8 == obj.reserved8 && reserved7 == obj.reserved7 && reserved6 == obj.reserved6 && reserved5 == obj.reserved5 && reserved4 == obj.reserved4 && reserved3 == obj.reserved3 && reserved2 == obj.reserved2 && reserved1 == obj.reserved1;

		public readonly override bool Equals (object obj) =>
			obj is FontExtents f && Equals (f);

		public static bool operator == (FontExtents left, FontExtents right) =>
			left.Equals (right);

		public static bool operator != (FontExtents left, FontExtents right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (ascender);
			hash.Add (descender);
			hash.Add (line_gap);
			hash.Add (reserved9);
			hash.Add (reserved8);
			hash.Add (reserved7);
			hash.Add (reserved6);
			hash.Add (reserved5);
			hash.Add (reserved4);
			hash.Add (reserved3);
			hash.Add (reserved2);
			hash.Add (reserved1);
			return hash.ToHashCode ();
		}

	}

	// hb_glyph_extents_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphExtents : IEquatable<GlyphExtents> {
		// public hb_position_t x_bearing
		private Int32 x_bearing;
		public Int32 XBearing {
			readonly get => x_bearing;
			set => x_bearing = value;
		}

		// public hb_position_t y_bearing
		private Int32 y_bearing;
		public Int32 YBearing {
			readonly get => y_bearing;
			set => y_bearing = value;
		}

		// public hb_position_t width
		private Int32 width;
		public Int32 Width {
			readonly get => width;
			set => width = value;
		}

		// public hb_position_t height
		private Int32 height;
		public Int32 Height {
			readonly get => height;
			set => height = value;
		}

		public readonly bool Equals (GlyphExtents obj) =>
			x_bearing == obj.x_bearing && y_bearing == obj.y_bearing && width == obj.width && height == obj.height;

		public readonly override bool Equals (object obj) =>
			obj is GlyphExtents f && Equals (f);

		public static bool operator == (GlyphExtents left, GlyphExtents right) =>
			left.Equals (right);

		public static bool operator != (GlyphExtents left, GlyphExtents right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x_bearing);
			hash.Add (y_bearing);
			hash.Add (width);
			hash.Add (height);
			return hash.ToHashCode ();
		}

	}

	// hb_glyph_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphInfo : IEquatable<GlyphInfo> {
		// public hb_codepoint_t codepoint
		private UInt32 codepoint;
		public UInt32 Codepoint {
			readonly get => codepoint;
			set => codepoint = value;
		}

		// public hb_mask_t mask
		private UInt32 mask;
		public UInt32 Mask {
			readonly get => mask;
			set => mask = value;
		}

		// public uint32_t cluster
		private UInt32 cluster;
		public UInt32 Cluster {
			readonly get => cluster;
			set => cluster = value;
		}

		// public hb_var_int_t var1
		private Int32 var1;

		// public hb_var_int_t var2
		private Int32 var2;

		public readonly bool Equals (GlyphInfo obj) =>
			codepoint == obj.codepoint && mask == obj.mask && cluster == obj.cluster && var1 == obj.var1 && var2 == obj.var2;

		public readonly override bool Equals (object obj) =>
			obj is GlyphInfo f && Equals (f);

		public static bool operator == (GlyphInfo left, GlyphInfo right) =>
			left.Equals (right);

		public static bool operator != (GlyphInfo left, GlyphInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (codepoint);
			hash.Add (mask);
			hash.Add (cluster);
			hash.Add (var1);
			hash.Add (var2);
			return hash.ToHashCode ();
		}

	}

	// hb_glyph_position_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct GlyphPosition : IEquatable<GlyphPosition> {
		// public hb_position_t x_advance
		private Int32 x_advance;
		public Int32 XAdvance {
			readonly get => x_advance;
			set => x_advance = value;
		}

		// public hb_position_t y_advance
		private Int32 y_advance;
		public Int32 YAdvance {
			readonly get => y_advance;
			set => y_advance = value;
		}

		// public hb_position_t x_offset
		private Int32 x_offset;
		public Int32 XOffset {
			readonly get => x_offset;
			set => x_offset = value;
		}

		// public hb_position_t y_offset
		private Int32 y_offset;
		public Int32 YOffset {
			readonly get => y_offset;
			set => y_offset = value;
		}

		// public hb_var_int_t var
		private Int32 var;

		public readonly bool Equals (GlyphPosition obj) =>
			x_advance == obj.x_advance && y_advance == obj.y_advance && x_offset == obj.x_offset && y_offset == obj.y_offset && var == obj.var;

		public readonly override bool Equals (object obj) =>
			obj is GlyphPosition f && Equals (f);

		public static bool operator == (GlyphPosition left, GlyphPosition right) =>
			left.Equals (right);

		public static bool operator != (GlyphPosition left, GlyphPosition right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (x_advance);
			hash.Add (y_advance);
			hash.Add (x_offset);
			hash.Add (y_offset);
			hash.Add (var);
			return hash.ToHashCode ();
		}

	}

	// hb_ot_color_layer_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeColorLayer : IEquatable<OpenTypeColorLayer> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public unsigned int color_index
		private UInt32 color_index;
		public UInt32 ColorIndex {
			readonly get => color_index;
			set => color_index = value;
		}

		public readonly bool Equals (OpenTypeColorLayer obj) =>
			glyph == obj.glyph && color_index == obj.color_index;

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeColorLayer f && Equals (f);

		public static bool operator == (OpenTypeColorLayer left, OpenTypeColorLayer right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeColorLayer left, OpenTypeColorLayer right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (color_index);
			return hash.ToHashCode ();
		}

	}

	// hb_ot_math_glyph_part_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeMathGlyphPart : IEquatable<OpenTypeMathGlyphPart> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public hb_position_t start_connector_length
		private Int32 start_connector_length;
		public Int32 StartConnectorLength {
			readonly get => start_connector_length;
			set => start_connector_length = value;
		}

		// public hb_position_t end_connector_length
		private Int32 end_connector_length;
		public Int32 EndConnectorLength {
			readonly get => end_connector_length;
			set => end_connector_length = value;
		}

		// public hb_position_t full_advance
		private Int32 full_advance;
		public Int32 FullAdvance {
			readonly get => full_advance;
			set => full_advance = value;
		}

		// public hb_ot_math_glyph_part_flags_t flags
		private OpenTypeMathGlyphPartFlags flags;
		public OpenTypeMathGlyphPartFlags Flags {
			readonly get => flags;
			set => flags = value;
		}

		public readonly bool Equals (OpenTypeMathGlyphPart obj) =>
			glyph == obj.glyph && start_connector_length == obj.start_connector_length && end_connector_length == obj.end_connector_length && full_advance == obj.full_advance && flags == obj.flags;

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeMathGlyphPart f && Equals (f);

		public static bool operator == (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeMathGlyphPart left, OpenTypeMathGlyphPart right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (start_connector_length);
			hash.Add (end_connector_length);
			hash.Add (full_advance);
			hash.Add (flags);
			return hash.ToHashCode ();
		}

	}

	// hb_ot_math_glyph_variant_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeMathGlyphVariant : IEquatable<OpenTypeMathGlyphVariant> {
		// public hb_codepoint_t glyph
		private UInt32 glyph;
		public UInt32 Glyph {
			readonly get => glyph;
			set => glyph = value;
		}

		// public hb_position_t advance
		private Int32 advance;
		public Int32 Advance {
			readonly get => advance;
			set => advance = value;
		}

		public readonly bool Equals (OpenTypeMathGlyphVariant obj) =>
			glyph == obj.glyph && advance == obj.advance;

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeMathGlyphVariant f && Equals (f);

		public static bool operator == (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeMathGlyphVariant left, OpenTypeMathGlyphVariant right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (glyph);
			hash.Add (advance);
			return hash.ToHashCode ();
		}

	}

	// hb_ot_name_entry_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeNameEntry : IEquatable<OpenTypeNameEntry> {
		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public hb_var_int_t var
		private Int32 var;
		public Int32 Var {
			readonly get => var;
			set => var = value;
		}

		// public hb_language_t language
		private IntPtr language;
		public IntPtr Language {
			readonly get => language;
			set => language = value;
		}

		public readonly bool Equals (OpenTypeNameEntry obj) =>
			name_id == obj.name_id && var == obj.var && language == obj.language;

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeNameEntry f && Equals (f);

		public static bool operator == (OpenTypeNameEntry left, OpenTypeNameEntry right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeNameEntry left, OpenTypeNameEntry right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (name_id);
			hash.Add (var);
			hash.Add (language);
			return hash.ToHashCode ();
		}

	}

	// hb_ot_var_axis_info_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeVarAxisInfo : IEquatable<OpenTypeVarAxisInfo> {
		// public unsigned int axis_index
		private UInt32 axis_index;
		public UInt32 AxisIndex {
			readonly get => axis_index;
			set => axis_index = value;
		}

		// public hb_tag_t tag
		private UInt32 tag;
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public hb_ot_var_axis_flags_t flags
		private OpenTypeVarAxisFlags flags;
		public OpenTypeVarAxisFlags Flags {
			readonly get => flags;
			set => flags = value;
		}

		// public float min_value
		private Single min_value;
		public Single MinValue {
			readonly get => min_value;
			set => min_value = value;
		}

		// public float default_value
		private Single default_value;
		public Single DefaultValue {
			readonly get => default_value;
			set => default_value = value;
		}

		// public float max_value
		private Single max_value;
		public Single MaxValue {
			readonly get => max_value;
			set => max_value = value;
		}

		// public unsigned int reserved
		private UInt32 reserved;

		public readonly bool Equals (OpenTypeVarAxisInfo obj) =>
			axis_index == obj.axis_index && tag == obj.tag && name_id == obj.name_id && flags == obj.flags && min_value == obj.min_value && default_value == obj.default_value && max_value == obj.max_value && reserved == obj.reserved;

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeVarAxisInfo f && Equals (f);

		public static bool operator == (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeVarAxisInfo left, OpenTypeVarAxisInfo right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (axis_index);
			hash.Add (tag);
			hash.Add (name_id);
			hash.Add (flags);
			hash.Add (min_value);
			hash.Add (default_value);
			hash.Add (max_value);
			hash.Add (reserved);
			return hash.ToHashCode ();
		}

	}

	// hb_ot_var_axis_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct OpenTypeVarAxis : IEquatable<OpenTypeVarAxis> {
		// public hb_tag_t tag
		private UInt32 tag;
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public hb_ot_name_id_t name_id
		private OpenTypeNameId name_id;
		public OpenTypeNameId NameId {
			readonly get => name_id;
			set => name_id = value;
		}

		// public float min_value
		private Single min_value;
		public Single MinValue {
			readonly get => min_value;
			set => min_value = value;
		}

		// public float default_value
		private Single default_value;
		public Single DefaultValue {
			readonly get => default_value;
			set => default_value = value;
		}

		// public float max_value
		private Single max_value;
		public Single MaxValue {
			readonly get => max_value;
			set => max_value = value;
		}

		public readonly bool Equals (OpenTypeVarAxis obj) =>
			tag == obj.tag && name_id == obj.name_id && min_value == obj.min_value && default_value == obj.default_value && max_value == obj.max_value;

		public readonly override bool Equals (object obj) =>
			obj is OpenTypeVarAxis f && Equals (f);

		public static bool operator == (OpenTypeVarAxis left, OpenTypeVarAxis right) =>
			left.Equals (right);

		public static bool operator != (OpenTypeVarAxis left, OpenTypeVarAxis right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (name_id);
			hash.Add (min_value);
			hash.Add (default_value);
			hash.Add (max_value);
			return hash.ToHashCode ();
		}

	}

	// hb_variation_t
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct Variation : IEquatable<Variation> {
		// public hb_tag_t tag
		private UInt32 tag;
		public UInt32 Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public float value
		private Single value;
		public Single Value {
			readonly get => this.value;
			set => this.value = value;
		}

		public readonly bool Equals (Variation obj) =>
			tag == obj.tag && value == obj.value;

		public readonly override bool Equals (object obj) =>
			obj is Variation f && Equals (f);

		public static bool operator == (Variation left, Variation right) =>
			left.Equals (right);

		public static bool operator != (Variation left, Variation right) =>
			!left.Equals (right);

		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (value);
			return hash.ToHashCode ();
		}

	}
}

#endregion

#region Enums

namespace HarfBuzzSharp {

	// hb_buffer_cluster_level_t
	public enum ClusterLevel {
		// HB_BUFFER_CLUSTER_LEVEL_MONOTONE_GRAPHEMES = 0
		MonotoneGraphemes = 0,
		// HB_BUFFER_CLUSTER_LEVEL_MONOTONE_CHARACTERS = 1
		MonotoneCharacters = 1,
		// HB_BUFFER_CLUSTER_LEVEL_CHARACTERS = 2
		Characters = 2,
		// HB_BUFFER_CLUSTER_LEVEL_DEFAULT = HB_BUFFER_CLUSTER_LEVEL_MONOTONE_GRAPHEMES
		Default = 0,
	}

	// hb_buffer_content_type_t
	public enum ContentType {
		// HB_BUFFER_CONTENT_TYPE_INVALID = 0
		Invalid = 0,
		// HB_BUFFER_CONTENT_TYPE_UNICODE = 1
		Unicode = 1,
		// HB_BUFFER_CONTENT_TYPE_GLYPHS = 2
		Glyphs = 2,
	}

	// hb_buffer_diff_flags_t
	public enum BufferDiffFlags {
		// HB_BUFFER_DIFF_FLAG_EQUAL = 0x0000
		Equal = 0,
		// HB_BUFFER_DIFF_FLAG_CONTENT_TYPE_MISMATCH = 0x0001
		ContentTypeMismatch = 1,
		// HB_BUFFER_DIFF_FLAG_LENGTH_MISMATCH = 0x0002
		LengthMismatch = 2,
		// HB_BUFFER_DIFF_FLAG_NOTDEF_PRESENT = 0x0004
		NotdefPresent = 4,
		// HB_BUFFER_DIFF_FLAG_DOTTED_CIRCLE_PRESENT = 0x0008
		DottedCirclePresent = 8,
		// HB_BUFFER_DIFF_FLAG_CODEPOINT_MISMATCH = 0x0010
		CodepointMismatch = 16,
		// HB_BUFFER_DIFF_FLAG_CLUSTER_MISMATCH = 0x0020
		ClusterMismatch = 32,
		// HB_BUFFER_DIFF_FLAG_GLYPH_FLAGS_MISMATCH = 0x0040
		GlyphFlagsMismatch = 64,
		// HB_BUFFER_DIFF_FLAG_POSITION_MISMATCH = 0x0080
		PositionMismatch = 128,
	}

	// hb_buffer_flags_t
	[Flags]
	public enum BufferFlags {
		// HB_BUFFER_FLAG_DEFAULT = 0x00000000u
		Default = 0,
		// HB_BUFFER_FLAG_BOT = 0x00000001u
		BeginningOfText = 1,
		// HB_BUFFER_FLAG_EOT = 0x00000002u
		EndOfText = 2,
		// HB_BUFFER_FLAG_PRESERVE_DEFAULT_IGNORABLES = 0x00000004u
		PreserveDefaultIgnorables = 4,
		// HB_BUFFER_FLAG_REMOVE_DEFAULT_IGNORABLES = 0x00000008u
		RemoveDefaultIgnorables = 8,
		// HB_BUFFER_FLAG_DO_NOT_INSERT_DOTTED_CIRCLE = 0x00000010u
		DoNotInsertDottedCircle = 16,
	}

	// hb_buffer_serialize_flags_t
	[Flags]
	public enum SerializeFlag {
		// HB_BUFFER_SERIALIZE_FLAG_DEFAULT = 0x00000000u
		Default = 0,
		// HB_BUFFER_SERIALIZE_FLAG_NO_CLUSTERS = 0x00000001u
		NoClusters = 1,
		// HB_BUFFER_SERIALIZE_FLAG_NO_POSITIONS = 0x00000002u
		NoPositions = 2,
		// HB_BUFFER_SERIALIZE_FLAG_NO_GLYPH_NAMES = 0x00000004u
		NoGlyphNames = 4,
		// HB_BUFFER_SERIALIZE_FLAG_GLYPH_EXTENTS = 0x00000008u
		GlyphExtents = 8,
		// HB_BUFFER_SERIALIZE_FLAG_GLYPH_FLAGS = 0x00000010u
		GlyphFlags = 16,
		// HB_BUFFER_SERIALIZE_FLAG_NO_ADVANCES = 0x00000020u
		NoAdvances = 32,
	}

	// hb_buffer_serialize_format_t
	public enum SerializeFormat {
		// HB_BUFFER_SERIALIZE_FORMAT_TEXT = 1413830740
		Text = 1413830740,
		// HB_BUFFER_SERIALIZE_FORMAT_JSON = 1246973774
		Json = 1246973774,
		// HB_BUFFER_SERIALIZE_FORMAT_INVALID = 0
		Invalid = 0,
	}

	// hb_direction_t
	public enum Direction {
		// HB_DIRECTION_INVALID = 0
		Invalid = 0,
		// HB_DIRECTION_LTR = 4
		LeftToRight = 4,
		// HB_DIRECTION_RTL = 5
		RightToLeft = 5,
		// HB_DIRECTION_TTB = 6
		TopToBottom = 6,
		// HB_DIRECTION_BTT = 7
		BottomToTop = 7,
	}

	// hb_glyph_flags_t
	[Flags]
	public enum GlyphFlags {
		// HB_GLYPH_FLAG_UNSAFE_TO_BREAK = 0x00000001
		UnsafeToBreak = 1,
		// HB_GLYPH_FLAG_DEFINED = 0x00000001
		Defined = 1,
	}

	// hb_memory_mode_t
	public enum MemoryMode {
		// HB_MEMORY_MODE_DUPLICATE = 0
		Duplicate = 0,
		// HB_MEMORY_MODE_READONLY = 1
		ReadOnly = 1,
		// HB_MEMORY_MODE_WRITABLE = 2
		Writeable = 2,
		// HB_MEMORY_MODE_READONLY_MAY_MAKE_WRITABLE = 3
		ReadOnlyMayMakeWriteable = 3,
	}

	// hb_ot_color_palette_flags_t
	public enum OpenTypeColorPaletteFlags {
		// HB_OT_COLOR_PALETTE_FLAG_DEFAULT = 0x00000000u
		Default = 0,
		// HB_OT_COLOR_PALETTE_FLAG_USABLE_WITH_LIGHT_BACKGROUND = 0x00000001u
		UsableWithLightBackground = 1,
		// HB_OT_COLOR_PALETTE_FLAG_USABLE_WITH_DARK_BACKGROUND = 0x00000002u
		UsableWithDarkBackground = 2,
	}

	// hb_ot_layout_baseline_tag_t
	public enum OpenTypeLayoutBaselineTag {
		// HB_OT_LAYOUT_BASELINE_TAG_ROMAN = 1919905134
		Roman = 1919905134,
		// HB_OT_LAYOUT_BASELINE_TAG_HANGING = 1751215719
		Hanging = 1751215719,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_FACE_BOTTOM_OR_LEFT = 1768121954
		IdeoFaceBottomOrLeft = 1768121954,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_FACE_TOP_OR_RIGHT = 1768121972
		IdeoFaceTopOrRight = 1768121972,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_EMBOX_BOTTOM_OR_LEFT = 1768187247
		IdeoEmboxBottomOrLeft = 1768187247,
		// HB_OT_LAYOUT_BASELINE_TAG_IDEO_EMBOX_TOP_OR_RIGHT = 1768191088
		IdeoEmboxTopOrRight = 1768191088,
		// HB_OT_LAYOUT_BASELINE_TAG_MATH = 1835103336
		Math = 1835103336,
	}

	// hb_ot_layout_glyph_class_t
	public enum OpenTypeLayoutGlyphClass {
		// HB_OT_LAYOUT_GLYPH_CLASS_UNCLASSIFIED = 0
		Unclassified = 0,
		// HB_OT_LAYOUT_GLYPH_CLASS_BASE_GLYPH = 1
		BaseGlyph = 1,
		// HB_OT_LAYOUT_GLYPH_CLASS_LIGATURE = 2
		Ligature = 2,
		// HB_OT_LAYOUT_GLYPH_CLASS_MARK = 3
		Mark = 3,
		// HB_OT_LAYOUT_GLYPH_CLASS_COMPONENT = 4
		Component = 4,
	}

	// hb_ot_math_constant_t
	public enum OpenTypeMathConstant {
		// HB_OT_MATH_CONSTANT_SCRIPT_PERCENT_SCALE_DOWN = 0
		ScriptPercentScaleDown = 0,
		// HB_OT_MATH_CONSTANT_SCRIPT_SCRIPT_PERCENT_SCALE_DOWN = 1
		ScriptScriptPercentScaleDown = 1,
		// HB_OT_MATH_CONSTANT_DELIMITED_SUB_FORMULA_MIN_HEIGHT = 2
		DelimitedSubFormulaMinHeight = 2,
		// HB_OT_MATH_CONSTANT_DISPLAY_OPERATOR_MIN_HEIGHT = 3
		DisplayOperatorMinHeight = 3,
		// HB_OT_MATH_CONSTANT_MATH_LEADING = 4
		MathLeading = 4,
		// HB_OT_MATH_CONSTANT_AXIS_HEIGHT = 5
		AxisHeight = 5,
		// HB_OT_MATH_CONSTANT_ACCENT_BASE_HEIGHT = 6
		AccentBaseHeight = 6,
		// HB_OT_MATH_CONSTANT_FLATTENED_ACCENT_BASE_HEIGHT = 7
		FlattenedAccentBaseHeight = 7,
		// HB_OT_MATH_CONSTANT_SUBSCRIPT_SHIFT_DOWN = 8
		SubscriptShiftDown = 8,
		// HB_OT_MATH_CONSTANT_SUBSCRIPT_TOP_MAX = 9
		SubscriptTopMax = 9,
		// HB_OT_MATH_CONSTANT_SUBSCRIPT_BASELINE_DROP_MIN = 10
		SubscriptBaselineDropMin = 10,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_SHIFT_UP = 11
		SuperscriptShiftUp = 11,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_SHIFT_UP_CRAMPED = 12
		SuperscriptShiftUpCramped = 12,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_BOTTOM_MIN = 13
		SuperscriptBottomMin = 13,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_BASELINE_DROP_MAX = 14
		SuperscriptBaselineDropMax = 14,
		// HB_OT_MATH_CONSTANT_SUB_SUPERSCRIPT_GAP_MIN = 15
		SubSuperscriptGapMin = 15,
		// HB_OT_MATH_CONSTANT_SUPERSCRIPT_BOTTOM_MAX_WITH_SUBSCRIPT = 16
		SuperscriptBottomMaxWithSubscript = 16,
		// HB_OT_MATH_CONSTANT_SPACE_AFTER_SCRIPT = 17
		SpaceAfterScript = 17,
		// HB_OT_MATH_CONSTANT_UPPER_LIMIT_GAP_MIN = 18
		UpperLimitGapMin = 18,
		// HB_OT_MATH_CONSTANT_UPPER_LIMIT_BASELINE_RISE_MIN = 19
		UpperLimitBaselineRiseMin = 19,
		// HB_OT_MATH_CONSTANT_LOWER_LIMIT_GAP_MIN = 20
		LowerLimitGapMin = 20,
		// HB_OT_MATH_CONSTANT_LOWER_LIMIT_BASELINE_DROP_MIN = 21
		LowerLimitBaselineDropMin = 21,
		// HB_OT_MATH_CONSTANT_STACK_TOP_SHIFT_UP = 22
		StackTopShiftUp = 22,
		// HB_OT_MATH_CONSTANT_STACK_TOP_DISPLAY_STYLE_SHIFT_UP = 23
		StackTopDisplayStyleShiftUp = 23,
		// HB_OT_MATH_CONSTANT_STACK_BOTTOM_SHIFT_DOWN = 24
		StackBottomShiftDown = 24,
		// HB_OT_MATH_CONSTANT_STACK_BOTTOM_DISPLAY_STYLE_SHIFT_DOWN = 25
		StackBottomDisplayStyleShiftDown = 25,
		// HB_OT_MATH_CONSTANT_STACK_GAP_MIN = 26
		StackGapMin = 26,
		// HB_OT_MATH_CONSTANT_STACK_DISPLAY_STYLE_GAP_MIN = 27
		StackDisplayStyleGapMin = 27,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_TOP_SHIFT_UP = 28
		StretchStackTopShiftUp = 28,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_BOTTOM_SHIFT_DOWN = 29
		StretchStackBottomShiftDown = 29,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_GAP_ABOVE_MIN = 30
		StretchStackGapAboveMin = 30,
		// HB_OT_MATH_CONSTANT_STRETCH_STACK_GAP_BELOW_MIN = 31
		StretchStackGapBelowMin = 31,
		// HB_OT_MATH_CONSTANT_FRACTION_NUMERATOR_SHIFT_UP = 32
		FractionNumeratorShiftUp = 32,
		// HB_OT_MATH_CONSTANT_FRACTION_NUMERATOR_DISPLAY_STYLE_SHIFT_UP = 33
		FractionNumeratorDisplayStyleShiftUp = 33,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOMINATOR_SHIFT_DOWN = 34
		FractionDenominatorShiftDown = 34,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOMINATOR_DISPLAY_STYLE_SHIFT_DOWN = 35
		FractionDenominatorDisplayStyleShiftDown = 35,
		// HB_OT_MATH_CONSTANT_FRACTION_NUMERATOR_GAP_MIN = 36
		FractionNumeratorGapMin = 36,
		// HB_OT_MATH_CONSTANT_FRACTION_NUM_DISPLAY_STYLE_GAP_MIN = 37
		FractionNumDisplayStyleGapMin = 37,
		// HB_OT_MATH_CONSTANT_FRACTION_RULE_THICKNESS = 38
		FractionRuleThickness = 38,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOMINATOR_GAP_MIN = 39
		FractionDenominatorGapMin = 39,
		// HB_OT_MATH_CONSTANT_FRACTION_DENOM_DISPLAY_STYLE_GAP_MIN = 40
		FractionDenomDisplayStyleGapMin = 40,
		// HB_OT_MATH_CONSTANT_SKEWED_FRACTION_HORIZONTAL_GAP = 41
		SkewedFractionHorizontalGap = 41,
		// HB_OT_MATH_CONSTANT_SKEWED_FRACTION_VERTICAL_GAP = 42
		SkewedFractionVerticalGap = 42,
		// HB_OT_MATH_CONSTANT_OVERBAR_VERTICAL_GAP = 43
		OverbarVerticalGap = 43,
		// HB_OT_MATH_CONSTANT_OVERBAR_RULE_THICKNESS = 44
		OverbarRuleThickness = 44,
		// HB_OT_MATH_CONSTANT_OVERBAR_EXTRA_ASCENDER = 45
		OverbarExtraAscender = 45,
		// HB_OT_MATH_CONSTANT_UNDERBAR_VERTICAL_GAP = 46
		UnderbarVerticalGap = 46,
		// HB_OT_MATH_CONSTANT_UNDERBAR_RULE_THICKNESS = 47
		UnderbarRuleThickness = 47,
		// HB_OT_MATH_CONSTANT_UNDERBAR_EXTRA_DESCENDER = 48
		UnderbarExtraDescender = 48,
		// HB_OT_MATH_CONSTANT_RADICAL_VERTICAL_GAP = 49
		RadicalVerticalGap = 49,
		// HB_OT_MATH_CONSTANT_RADICAL_DISPLAY_STYLE_VERTICAL_GAP = 50
		RadicalDisplayStyleVerticalGap = 50,
		// HB_OT_MATH_CONSTANT_RADICAL_RULE_THICKNESS = 51
		RadicalRuleThickness = 51,
		// HB_OT_MATH_CONSTANT_RADICAL_EXTRA_ASCENDER = 52
		RadicalExtraAscender = 52,
		// HB_OT_MATH_CONSTANT_RADICAL_KERN_BEFORE_DEGREE = 53
		RadicalKernBeforeDegree = 53,
		// HB_OT_MATH_CONSTANT_RADICAL_KERN_AFTER_DEGREE = 54
		RadicalKernAfterDegree = 54,
		// HB_OT_MATH_CONSTANT_RADICAL_DEGREE_BOTTOM_RAISE_PERCENT = 55
		RadicalDegreeBottomRaisePercent = 55,
	}

	// hb_ot_math_glyph_part_flags_t
	public enum OpenTypeMathGlyphPartFlags {
		// HB_OT_MATH_GLYPH_PART_FLAG_EXTENDER = 0x00000001u
		Extender = 1,
	}

	// hb_ot_math_kern_t
	public enum OpenTypeMathKern {
		// HB_OT_MATH_KERN_TOP_RIGHT = 0
		TopRight = 0,
		// HB_OT_MATH_KERN_TOP_LEFT = 1
		TopLeft = 1,
		// HB_OT_MATH_KERN_BOTTOM_RIGHT = 2
		BottomRight = 2,
		// HB_OT_MATH_KERN_BOTTOM_LEFT = 3
		BottomLeft = 3,
	}

	// hb_ot_meta_tag_t
	public enum OpenTypeMetaTag {
		// HB_OT_META_TAG_DESIGN_LANGUAGES = 1684827751
		DesignLanguages = 1684827751,
		// HB_OT_META_TAG_SUPPORTED_LANGUAGES = 1936485991
		SupportedLanguages = 1936485991,
	}

	// hb_ot_metrics_tag_t
	public enum OpenTypeMetricsTag {
		// HB_OT_METRICS_TAG_HORIZONTAL_ASCENDER = 1751216995
		HorizontalAscender = 1751216995,
		// HB_OT_METRICS_TAG_HORIZONTAL_DESCENDER = 1751413603
		HorizontalDescender = 1751413603,
		// HB_OT_METRICS_TAG_HORIZONTAL_LINE_GAP = 1751934832
		HorizontalLineGap = 1751934832,
		// HB_OT_METRICS_TAG_HORIZONTAL_CLIPPING_ASCENT = 1751346273
		HorizontalClippingAscent = 1751346273,
		// HB_OT_METRICS_TAG_HORIZONTAL_CLIPPING_DESCENT = 1751346276
		HorizontalClippingDescent = 1751346276,
		// HB_OT_METRICS_TAG_VERTICAL_ASCENDER = 1986098019
		VerticalAscender = 1986098019,
		// HB_OT_METRICS_TAG_VERTICAL_DESCENDER = 1986294627
		VerticalDescender = 1986294627,
		// HB_OT_METRICS_TAG_VERTICAL_LINE_GAP = 1986815856
		VerticalLineGap = 1986815856,
		// HB_OT_METRICS_TAG_HORIZONTAL_CARET_RISE = 1751347827
		HorizontalCaretRise = 1751347827,
		// HB_OT_METRICS_TAG_HORIZONTAL_CARET_RUN = 1751347822
		HorizontalCaretRun = 1751347822,
		// HB_OT_METRICS_TAG_HORIZONTAL_CARET_OFFSET = 1751347046
		HorizontalCaretOffset = 1751347046,
		// HB_OT_METRICS_TAG_VERTICAL_CARET_RISE = 1986228851
		VerticalCaretRise = 1986228851,
		// HB_OT_METRICS_TAG_VERTICAL_CARET_RUN = 1986228846
		VerticalCaretRun = 1986228846,
		// HB_OT_METRICS_TAG_VERTICAL_CARET_OFFSET = 1986228070
		VerticalCaretOffset = 1986228070,
		// HB_OT_METRICS_TAG_X_HEIGHT = 2020108148
		XHeight = 2020108148,
		// HB_OT_METRICS_TAG_CAP_HEIGHT = 1668311156
		CapHeight = 1668311156,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_X_SIZE = 1935833203
		SubScriptEmXSize = 1935833203,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_Y_SIZE = 1935833459
		SubScriptEmYSize = 1935833459,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_X_OFFSET = 1935833199
		SubScriptEmXOffset = 1935833199,
		// HB_OT_METRICS_TAG_SUBSCRIPT_EM_Y_OFFSET = 1935833455
		SubScriptEmYOffset = 1935833455,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_X_SIZE = 1936750707
		SuperScriptEmXSize = 1936750707,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_Y_SIZE = 1936750963
		SuperScriptEmYSize = 1936750963,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_X_OFFSET = 1936750703
		SuperScriptEmXOffset = 1936750703,
		// HB_OT_METRICS_TAG_SUPERSCRIPT_EM_Y_OFFSET = 1936750959
		SuperScriptEmYOffset = 1936750959,
		// HB_OT_METRICS_TAG_STRIKEOUT_SIZE = 1937011315
		StrikeoutSize = 1937011315,
		// HB_OT_METRICS_TAG_STRIKEOUT_OFFSET = 1937011311
		StrikeoutOffset = 1937011311,
		// HB_OT_METRICS_TAG_UNDERLINE_SIZE = 1970168947
		UnderlineSize = 1970168947,
		// HB_OT_METRICS_TAG_UNDERLINE_OFFSET = 1970168943
		UnderlineOffset = 1970168943,
	}

	// hb_ot_var_axis_flags_t
	public enum OpenTypeVarAxisFlags {
		// HB_OT_VAR_AXIS_FLAG_HIDDEN = 0x00000001u
		Hidden = 1,
	}

	// hb_unicode_combining_class_t
	public enum UnicodeCombiningClass {
		// HB_UNICODE_COMBINING_CLASS_NOT_REORDERED = 0
		NotReordered = 0,
		// HB_UNICODE_COMBINING_CLASS_OVERLAY = 1
		Overlay = 1,
		// HB_UNICODE_COMBINING_CLASS_NUKTA = 7
		Nukta = 7,
		// HB_UNICODE_COMBINING_CLASS_KANA_VOICING = 8
		KanaVoicing = 8,
		// HB_UNICODE_COMBINING_CLASS_VIRAMA = 9
		Virama = 9,
		// HB_UNICODE_COMBINING_CLASS_CCC10 = 10
		CCC10 = 10,
		// HB_UNICODE_COMBINING_CLASS_CCC11 = 11
		CCC11 = 11,
		// HB_UNICODE_COMBINING_CLASS_CCC12 = 12
		CCC12 = 12,
		// HB_UNICODE_COMBINING_CLASS_CCC13 = 13
		CCC13 = 13,
		// HB_UNICODE_COMBINING_CLASS_CCC14 = 14
		CCC14 = 14,
		// HB_UNICODE_COMBINING_CLASS_CCC15 = 15
		CCC15 = 15,
		// HB_UNICODE_COMBINING_CLASS_CCC16 = 16
		CCC16 = 16,
		// HB_UNICODE_COMBINING_CLASS_CCC17 = 17
		CCC17 = 17,
		// HB_UNICODE_COMBINING_CLASS_CCC18 = 18
		CCC18 = 18,
		// HB_UNICODE_COMBINING_CLASS_CCC19 = 19
		CCC19 = 19,
		// HB_UNICODE_COMBINING_CLASS_CCC20 = 20
		CCC20 = 20,
		// HB_UNICODE_COMBINING_CLASS_CCC21 = 21
		CCC21 = 21,
		// HB_UNICODE_COMBINING_CLASS_CCC22 = 22
		CCC22 = 22,
		// HB_UNICODE_COMBINING_CLASS_CCC23 = 23
		CCC23 = 23,
		// HB_UNICODE_COMBINING_CLASS_CCC24 = 24
		CCC24 = 24,
		// HB_UNICODE_COMBINING_CLASS_CCC25 = 25
		CCC25 = 25,
		// HB_UNICODE_COMBINING_CLASS_CCC26 = 26
		CCC26 = 26,
		// HB_UNICODE_COMBINING_CLASS_CCC27 = 27
		CCC27 = 27,
		// HB_UNICODE_COMBINING_CLASS_CCC28 = 28
		CCC28 = 28,
		// HB_UNICODE_COMBINING_CLASS_CCC29 = 29
		CCC29 = 29,
		// HB_UNICODE_COMBINING_CLASS_CCC30 = 30
		CCC30 = 30,
		// HB_UNICODE_COMBINING_CLASS_CCC31 = 31
		CCC31 = 31,
		// HB_UNICODE_COMBINING_CLASS_CCC32 = 32
		CCC32 = 32,
		// HB_UNICODE_COMBINING_CLASS_CCC33 = 33
		CCC33 = 33,
		// HB_UNICODE_COMBINING_CLASS_CCC34 = 34
		CCC34 = 34,
		// HB_UNICODE_COMBINING_CLASS_CCC35 = 35
		CCC35 = 35,
		// HB_UNICODE_COMBINING_CLASS_CCC36 = 36
		CCC36 = 36,
		// HB_UNICODE_COMBINING_CLASS_CCC84 = 84
		CCC84 = 84,
		// HB_UNICODE_COMBINING_CLASS_CCC91 = 91
		CCC91 = 91,
		// HB_UNICODE_COMBINING_CLASS_CCC103 = 103
		CCC103 = 103,
		// HB_UNICODE_COMBINING_CLASS_CCC107 = 107
		CCC107 = 107,
		// HB_UNICODE_COMBINING_CLASS_CCC118 = 118
		CCC118 = 118,
		// HB_UNICODE_COMBINING_CLASS_CCC122 = 122
		CCC122 = 122,
		// HB_UNICODE_COMBINING_CLASS_CCC129 = 129
		CCC129 = 129,
		// HB_UNICODE_COMBINING_CLASS_CCC130 = 130
		CCC130 = 130,
		// HB_UNICODE_COMBINING_CLASS_CCC133 = 132
		CCC133 = 132,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_BELOW_LEFT = 200
		AttachedBelowLeft = 200,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_BELOW = 202
		AttachedBelow = 202,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_ABOVE = 214
		AttachedAbove = 214,
		// HB_UNICODE_COMBINING_CLASS_ATTACHED_ABOVE_RIGHT = 216
		AttachedAboveRight = 216,
		// HB_UNICODE_COMBINING_CLASS_BELOW_LEFT = 218
		BelowLeft = 218,
		// HB_UNICODE_COMBINING_CLASS_BELOW = 220
		Below = 220,
		// HB_UNICODE_COMBINING_CLASS_BELOW_RIGHT = 222
		BelowRight = 222,
		// HB_UNICODE_COMBINING_CLASS_LEFT = 224
		Left = 224,
		// HB_UNICODE_COMBINING_CLASS_RIGHT = 226
		Right = 226,
		// HB_UNICODE_COMBINING_CLASS_ABOVE_LEFT = 228
		AboveLeft = 228,
		// HB_UNICODE_COMBINING_CLASS_ABOVE = 230
		Above = 230,
		// HB_UNICODE_COMBINING_CLASS_ABOVE_RIGHT = 232
		AboveRight = 232,
		// HB_UNICODE_COMBINING_CLASS_DOUBLE_BELOW = 233
		DoubleBelow = 233,
		// HB_UNICODE_COMBINING_CLASS_DOUBLE_ABOVE = 234
		DoubleAbove = 234,
		// HB_UNICODE_COMBINING_CLASS_IOTA_SUBSCRIPT = 240
		IotaSubscript = 240,
		// HB_UNICODE_COMBINING_CLASS_INVALID = 255
		Invalid = 255,
	}

	// hb_unicode_general_category_t
	public enum UnicodeGeneralCategory {
		// HB_UNICODE_GENERAL_CATEGORY_CONTROL = 0
		Control = 0,
		// HB_UNICODE_GENERAL_CATEGORY_FORMAT = 1
		Format = 1,
		// HB_UNICODE_GENERAL_CATEGORY_UNASSIGNED = 2
		Unassigned = 2,
		// HB_UNICODE_GENERAL_CATEGORY_PRIVATE_USE = 3
		PrivateUse = 3,
		// HB_UNICODE_GENERAL_CATEGORY_SURROGATE = 4
		Surrogate = 4,
		// HB_UNICODE_GENERAL_CATEGORY_LOWERCASE_LETTER = 5
		LowercaseLetter = 5,
		// HB_UNICODE_GENERAL_CATEGORY_MODIFIER_LETTER = 6
		ModifierLetter = 6,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_LETTER = 7
		OtherLetter = 7,
		// HB_UNICODE_GENERAL_CATEGORY_TITLECASE_LETTER = 8
		TitlecaseLetter = 8,
		// HB_UNICODE_GENERAL_CATEGORY_UPPERCASE_LETTER = 9
		UppercaseLetter = 9,
		// HB_UNICODE_GENERAL_CATEGORY_SPACING_MARK = 10
		SpacingMark = 10,
		// HB_UNICODE_GENERAL_CATEGORY_ENCLOSING_MARK = 11
		EnclosingMark = 11,
		// HB_UNICODE_GENERAL_CATEGORY_NON_SPACING_MARK = 12
		NonSpacingMark = 12,
		// HB_UNICODE_GENERAL_CATEGORY_DECIMAL_NUMBER = 13
		DecimalNumber = 13,
		// HB_UNICODE_GENERAL_CATEGORY_LETTER_NUMBER = 14
		LetterNumber = 14,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_NUMBER = 15
		OtherNumber = 15,
		// HB_UNICODE_GENERAL_CATEGORY_CONNECT_PUNCTUATION = 16
		ConnectPunctuation = 16,
		// HB_UNICODE_GENERAL_CATEGORY_DASH_PUNCTUATION = 17
		DashPunctuation = 17,
		// HB_UNICODE_GENERAL_CATEGORY_CLOSE_PUNCTUATION = 18
		ClosePunctuation = 18,
		// HB_UNICODE_GENERAL_CATEGORY_FINAL_PUNCTUATION = 19
		FinalPunctuation = 19,
		// HB_UNICODE_GENERAL_CATEGORY_INITIAL_PUNCTUATION = 20
		InitialPunctuation = 20,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_PUNCTUATION = 21
		OtherPunctuation = 21,
		// HB_UNICODE_GENERAL_CATEGORY_OPEN_PUNCTUATION = 22
		OpenPunctuation = 22,
		// HB_UNICODE_GENERAL_CATEGORY_CURRENCY_SYMBOL = 23
		CurrencySymbol = 23,
		// HB_UNICODE_GENERAL_CATEGORY_MODIFIER_SYMBOL = 24
		ModifierSymbol = 24,
		// HB_UNICODE_GENERAL_CATEGORY_MATH_SYMBOL = 25
		MathSymbol = 25,
		// HB_UNICODE_GENERAL_CATEGORY_OTHER_SYMBOL = 26
		OtherSymbol = 26,
		// HB_UNICODE_GENERAL_CATEGORY_LINE_SEPARATOR = 27
		LineSeparator = 27,
		// HB_UNICODE_GENERAL_CATEGORY_PARAGRAPH_SEPARATOR = 28
		ParagraphSeparator = 28,
		// HB_UNICODE_GENERAL_CATEGORY_SPACE_SEPARATOR = 29
		SpaceSeparator = 29,
	}
}

#endregion
