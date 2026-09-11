using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_general.h

		// sk_colortype_t sk_colortype_get_default_8888()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKColorTypeNative sk_colortype_get_default_8888 ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKColorTypeNative sk_colortype_get_default_8888 ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKColorTypeNative sk_colortype_get_default_8888 ();
		}
		private static Delegates.sk_colortype_get_default_8888 sk_colortype_get_default_8888_delegate;
		internal static SKColorTypeNative sk_colortype_get_default_8888 () =>
			(sk_colortype_get_default_8888_delegate ??= GetSymbol<Delegates.sk_colortype_get_default_8888> ("sk_colortype_get_default_8888")).Invoke ();
		#endif

		// int sk_nvrefcnt_get_ref_count(const sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_get_ref_count sk_nvrefcnt_get_ref_count_delegate;
		internal static Int32 sk_nvrefcnt_get_ref_count (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_get_ref_count_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_get_ref_count> ("sk_nvrefcnt_get_ref_count")).Invoke (refcnt);
		#endif

		// void sk_nvrefcnt_safe_ref(sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_safe_ref sk_nvrefcnt_safe_ref_delegate;
		internal static void sk_nvrefcnt_safe_ref (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_safe_ref_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_safe_ref> ("sk_nvrefcnt_safe_ref")).Invoke (refcnt);
		#endif

		// void sk_nvrefcnt_safe_unref(sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_safe_unref sk_nvrefcnt_safe_unref_delegate;
		internal static void sk_nvrefcnt_safe_unref (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_safe_unref_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_safe_unref> ("sk_nvrefcnt_safe_unref")).Invoke (refcnt);
		#endif

		// bool sk_nvrefcnt_unique(const sk_nvrefcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt);
		}
		private static Delegates.sk_nvrefcnt_unique sk_nvrefcnt_unique_delegate;
		internal static bool sk_nvrefcnt_unique (sk_nvrefcnt_t refcnt) =>
			(sk_nvrefcnt_unique_delegate ??= GetSymbol<Delegates.sk_nvrefcnt_unique> ("sk_nvrefcnt_unique")).Invoke (refcnt);
		#endif

		// int sk_refcnt_get_ref_count(const sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_get_ref_count sk_refcnt_get_ref_count_delegate;
		internal static Int32 sk_refcnt_get_ref_count (sk_refcnt_t refcnt) =>
			(sk_refcnt_get_ref_count_delegate ??= GetSymbol<Delegates.sk_refcnt_get_ref_count> ("sk_refcnt_get_ref_count")).Invoke (refcnt);
		#endif

		// void sk_refcnt_safe_ref(sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_refcnt_safe_ref (sk_refcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_refcnt_safe_ref (sk_refcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_refcnt_safe_ref (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_safe_ref sk_refcnt_safe_ref_delegate;
		internal static void sk_refcnt_safe_ref (sk_refcnt_t refcnt) =>
			(sk_refcnt_safe_ref_delegate ??= GetSymbol<Delegates.sk_refcnt_safe_ref> ("sk_refcnt_safe_ref")).Invoke (refcnt);
		#endif

		// void sk_refcnt_safe_unref(sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_refcnt_safe_unref (sk_refcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_refcnt_safe_unref (sk_refcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_refcnt_safe_unref (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_safe_unref sk_refcnt_safe_unref_delegate;
		internal static void sk_refcnt_safe_unref (sk_refcnt_t refcnt) =>
			(sk_refcnt_safe_unref_delegate ??= GetSymbol<Delegates.sk_refcnt_safe_unref> ("sk_refcnt_safe_unref")).Invoke (refcnt);
		#endif

		// bool sk_refcnt_unique(const sk_refcnt_t* refcnt)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_refcnt_unique (sk_refcnt_t refcnt);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_refcnt_unique (sk_refcnt_t refcnt);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_refcnt_unique (sk_refcnt_t refcnt);
		}
		private static Delegates.sk_refcnt_unique sk_refcnt_unique_delegate;
		internal static bool sk_refcnt_unique (sk_refcnt_t refcnt) =>
			(sk_refcnt_unique_delegate ??= GetSymbol<Delegates.sk_refcnt_unique> ("sk_refcnt_unique")).Invoke (refcnt);
		#endif

		// int sk_version_get_increment()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_version_get_increment ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_version_get_increment ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_version_get_increment ();
		}
		private static Delegates.sk_version_get_increment sk_version_get_increment_delegate;
		internal static Int32 sk_version_get_increment () =>
			(sk_version_get_increment_delegate ??= GetSymbol<Delegates.sk_version_get_increment> ("sk_version_get_increment")).Invoke ();
		#endif

		// int sk_version_get_milestone()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_version_get_milestone ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_version_get_milestone ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_version_get_milestone ();
		}
		private static Delegates.sk_version_get_milestone sk_version_get_milestone_delegate;
		internal static Int32 sk_version_get_milestone () =>
			(sk_version_get_milestone_delegate ??= GetSymbol<Delegates.sk_version_get_milestone> ("sk_version_get_milestone")).Invoke ();
		#endif

		// const char* sk_version_get_string()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* char */ void* sk_version_get_string ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* sk_version_get_string ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* sk_version_get_string ();
		}
		private static Delegates.sk_version_get_string sk_version_get_string_delegate;
		internal static /* char */ void* sk_version_get_string () =>
			(sk_version_get_string_delegate ??= GetSymbol<Delegates.sk_version_get_string> ("sk_version_get_string")).Invoke ();
		#endif

		#endregion

	}
}
