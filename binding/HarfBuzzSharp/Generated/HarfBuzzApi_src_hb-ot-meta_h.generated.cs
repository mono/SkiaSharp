using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-meta.h

		// extern unsigned int hb_ot_meta_get_entry_tags(hb_face_t* face, unsigned int start_offset, unsigned int* entries_count, hb_ot_meta_tag_t* entries)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_meta_get_entry_tags (IntPtr face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_meta_get_entry_tags (IntPtr face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_meta_get_entry_tags (IntPtr face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries);
		}
		private static Delegates.hb_ot_meta_get_entry_tags hb_ot_meta_get_entry_tags_delegate;
		internal static UInt32 hb_ot_meta_get_entry_tags (IntPtr face, UInt32 start_offset, UInt32* entries_count, OpenTypeMetaTag* entries) =>
			(hb_ot_meta_get_entry_tags_delegate ??= GetSymbol<Delegates.hb_ot_meta_get_entry_tags> ("hb_ot_meta_get_entry_tags")).Invoke (face, start_offset, entries_count, entries);
		#endif

		// extern hb_blob_t* hb_ot_meta_reference_entry(hb_face_t* face, hb_ot_meta_tag_t meta_tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_ot_meta_reference_entry (IntPtr face, OpenTypeMetaTag meta_tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_ot_meta_reference_entry (IntPtr face, OpenTypeMetaTag meta_tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_ot_meta_reference_entry (IntPtr face, OpenTypeMetaTag meta_tag);
		}
		private static Delegates.hb_ot_meta_reference_entry hb_ot_meta_reference_entry_delegate;
		internal static IntPtr hb_ot_meta_reference_entry (IntPtr face, OpenTypeMetaTag meta_tag) =>
			(hb_ot_meta_reference_entry_delegate ??= GetSymbol<Delegates.hb_ot_meta_reference_entry> ("hb_ot_meta_reference_entry")).Invoke (face, meta_tag);
		#endif

		#endregion

	}
}
