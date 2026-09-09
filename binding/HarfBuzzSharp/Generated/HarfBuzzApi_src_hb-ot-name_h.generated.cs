using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-name.h

		// extern unsigned int hb_ot_name_get_utf16(hb_face_t* face, hb_ot_name_id_t name_id, hb_language_t language, unsigned int* text_size, uint16_t* text)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_name_get_utf16 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_name_get_utf16 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_name_get_utf16 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text);
		}
		private static Delegates.hb_ot_name_get_utf16 hb_ot_name_get_utf16_delegate;
		internal static UInt32 hb_ot_name_get_utf16 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt16* text) =>
			(hb_ot_name_get_utf16_delegate ??= GetSymbol<Delegates.hb_ot_name_get_utf16> ("hb_ot_name_get_utf16")).Invoke (face, name_id, language, text_size, text);
		#endif

		// extern unsigned int hb_ot_name_get_utf32(hb_face_t* face, hb_ot_name_id_t name_id, hb_language_t language, unsigned int* text_size, uint32_t* text)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_name_get_utf32 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_name_get_utf32 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_name_get_utf32 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text);
		}
		private static Delegates.hb_ot_name_get_utf32 hb_ot_name_get_utf32_delegate;
		internal static UInt32 hb_ot_name_get_utf32 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, UInt32* text) =>
			(hb_ot_name_get_utf32_delegate ??= GetSymbol<Delegates.hb_ot_name_get_utf32> ("hb_ot_name_get_utf32")).Invoke (face, name_id, language, text_size, text);
		#endif

		// extern unsigned int hb_ot_name_get_utf8(hb_face_t* face, hb_ot_name_id_t name_id, hb_language_t language, unsigned int* text_size, char* text)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_name_get_utf8 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_name_get_utf8 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_name_get_utf8 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text);
		}
		private static Delegates.hb_ot_name_get_utf8 hb_ot_name_get_utf8_delegate;
		internal static UInt32 hb_ot_name_get_utf8 (IntPtr face, OpenTypeNameId name_id, IntPtr language, UInt32* text_size, /* char */ void* text) =>
			(hb_ot_name_get_utf8_delegate ??= GetSymbol<Delegates.hb_ot_name_get_utf8> ("hb_ot_name_get_utf8")).Invoke (face, name_id, language, text_size, text);
		#endif

		// extern const hb_ot_name_entry_t* hb_ot_name_list_names(hb_face_t* face, unsigned int* num_entries)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial OpenTypeNameEntry* hb_ot_name_list_names (IntPtr face, UInt32* num_entries);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern OpenTypeNameEntry* hb_ot_name_list_names (IntPtr face, UInt32* num_entries);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate OpenTypeNameEntry* hb_ot_name_list_names (IntPtr face, UInt32* num_entries);
		}
		private static Delegates.hb_ot_name_list_names hb_ot_name_list_names_delegate;
		internal static OpenTypeNameEntry* hb_ot_name_list_names (IntPtr face, UInt32* num_entries) =>
			(hb_ot_name_list_names_delegate ??= GetSymbol<Delegates.hb_ot_name_list_names> ("hb_ot_name_list_names")).Invoke (face, num_entries);
		#endif

		#endregion

	}
}
