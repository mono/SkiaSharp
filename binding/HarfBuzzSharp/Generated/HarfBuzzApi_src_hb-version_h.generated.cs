using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-version.h

		// extern void hb_version(unsigned int* major, unsigned int* minor, unsigned int* micro)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_version (UInt32* major, UInt32* minor, UInt32* micro);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_version (UInt32* major, UInt32* minor, UInt32* micro);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_version_atleast (UInt32 major, UInt32 minor, UInt32 micro);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_version_atleast (UInt32 major, UInt32 minor, UInt32 micro);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void* hb_version_string ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_version_string ();
		#endif
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
