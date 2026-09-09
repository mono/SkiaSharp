using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_string.h

		// void sk_string_destructor(const sk_string_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_string_destructor (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_string_destructor (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_string_destructor (IntPtr param0);
		}
		private static Delegates.sk_string_destructor sk_string_destructor_delegate;
		internal static void sk_string_destructor (IntPtr param0) =>
			(sk_string_destructor_delegate ??= GetSymbol<Delegates.sk_string_destructor> ("sk_string_destructor")).Invoke (param0);
		#endif

		// const char* sk_string_get_c_str(const sk_string_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* char */ void* sk_string_get_c_str (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* sk_string_get_c_str (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* sk_string_get_c_str (IntPtr param0);
		}
		private static Delegates.sk_string_get_c_str sk_string_get_c_str_delegate;
		internal static /* char */ void* sk_string_get_c_str (IntPtr param0) =>
			(sk_string_get_c_str_delegate ??= GetSymbol<Delegates.sk_string_get_c_str> ("sk_string_get_c_str")).Invoke (param0);
		#endif

		// size_t sk_string_get_size(const sk_string_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_string_get_size (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_string_get_size (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_string_get_size (IntPtr param0);
		}
		private static Delegates.sk_string_get_size sk_string_get_size_delegate;
		internal static /* size_t */ IntPtr sk_string_get_size (IntPtr param0) =>
			(sk_string_get_size_delegate ??= GetSymbol<Delegates.sk_string_get_size> ("sk_string_get_size")).Invoke (param0);
		#endif

		// sk_string_t* sk_string_new_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_string_new_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_string_new_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_string_new_empty ();
		}
		private static Delegates.sk_string_new_empty sk_string_new_empty_delegate;
		internal static IntPtr sk_string_new_empty () =>
			(sk_string_new_empty_delegate ??= GetSymbol<Delegates.sk_string_new_empty> ("sk_string_new_empty")).Invoke ();
		#endif

		// sk_string_t* sk_string_new_with_copy(const char* src, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_string_new_with_copy sk_string_new_with_copy_delegate;
		internal static IntPtr sk_string_new_with_copy (/* char */ void* src, /* size_t */ IntPtr length) =>
			(sk_string_new_with_copy_delegate ??= GetSymbol<Delegates.sk_string_new_with_copy> ("sk_string_new_with_copy")).Invoke (src, length);
		#endif

		#endregion

	}
}
