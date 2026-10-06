using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-fetch.h

		// extern uint32_t hb_ot_fetch_bits(hb_face_t* face, hb_ot_bits_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_ot_fetch_bits (hb_face_t face, OpenTypeBitsTag tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_ot_fetch_bits (hb_face_t face, OpenTypeBitsTag tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_ot_fetch_bits (hb_face_t face, OpenTypeBitsTag tag);
		}
		private static Delegates.hb_ot_fetch_bits hb_ot_fetch_bits_delegate;
		internal static UInt32 hb_ot_fetch_bits (hb_face_t face, OpenTypeBitsTag tag) =>
			(hb_ot_fetch_bits_delegate ??= GetSymbol<Delegates.hb_ot_fetch_bits> ("hb_ot_fetch_bits")).Invoke (face, tag);
		#endif

		// extern int32_t hb_ot_fetch_number(hb_face_t* face, hb_ot_number_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_fetch_number (hb_face_t face, OpenTypeNumberTag tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_fetch_number (hb_face_t face, OpenTypeNumberTag tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_fetch_number (hb_face_t face, OpenTypeNumberTag tag);
		}
		private static Delegates.hb_ot_fetch_number hb_ot_fetch_number_delegate;
		internal static Int32 hb_ot_fetch_number (hb_face_t face, OpenTypeNumberTag tag) =>
			(hb_ot_fetch_number_delegate ??= GetSymbol<Delegates.hb_ot_fetch_number> ("hb_ot_fetch_number")).Invoke (face, tag);
		#endif

		#endregion

	}
}
