using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_data.h

		// const uint8_t* sk_data_get_bytes(const sk_data_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Byte* sk_data_get_bytes (sk_data_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte* sk_data_get_bytes (sk_data_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte* sk_data_get_bytes (sk_data_t param0);
		}
		private static Delegates.sk_data_get_bytes sk_data_get_bytes_delegate;
		internal static Byte* sk_data_get_bytes (sk_data_t param0) =>
			(sk_data_get_bytes_delegate ??= GetSymbol<Delegates.sk_data_get_bytes> ("sk_data_get_bytes")).Invoke (param0);
		#endif

		// const void* sk_data_get_data(const sk_data_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_data_get_data (sk_data_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_data_get_data (sk_data_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_data_get_data (sk_data_t param0);
		}
		private static Delegates.sk_data_get_data sk_data_get_data_delegate;
		internal static void* sk_data_get_data (sk_data_t param0) =>
			(sk_data_get_data_delegate ??= GetSymbol<Delegates.sk_data_get_data> ("sk_data_get_data")).Invoke (param0);
		#endif

		// size_t sk_data_get_size(const sk_data_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_data_get_size (sk_data_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_data_get_size (sk_data_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_data_get_size (sk_data_t param0);
		}
		private static Delegates.sk_data_get_size sk_data_get_size_delegate;
		internal static /* size_t */ IntPtr sk_data_get_size (sk_data_t param0) =>
			(sk_data_get_size_delegate ??= GetSymbol<Delegates.sk_data_get_size> ("sk_data_get_size")).Invoke (param0);
		#endif

		// sk_data_t* sk_data_new_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_empty ();
		}
		private static Delegates.sk_data_new_empty sk_data_new_empty_delegate;
		internal static sk_data_t sk_data_new_empty () =>
			(sk_data_new_empty_delegate ??= GetSymbol<Delegates.sk_data_new_empty> ("sk_data_new_empty")).Invoke ();
		#endif

		// sk_data_t* sk_data_new_from_file(const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_from_file (/* char */ void* path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_from_file (/* char */ void* path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_from_file (/* char */ void* path);
		}
		private static Delegates.sk_data_new_from_file sk_data_new_from_file_delegate;
		internal static sk_data_t sk_data_new_from_file (/* char */ void* path) =>
			(sk_data_new_from_file_delegate ??= GetSymbol<Delegates.sk_data_new_from_file> ("sk_data_new_from_file")).Invoke (path);
		#endif

		// sk_data_t* sk_data_new_from_stream(sk_stream_t* stream, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_data_new_from_stream sk_data_new_from_stream_delegate;
		internal static sk_data_t sk_data_new_from_stream (sk_stream_t stream, /* size_t */ IntPtr length) =>
			(sk_data_new_from_stream_delegate ??= GetSymbol<Delegates.sk_data_new_from_stream> ("sk_data_new_from_stream")).Invoke (stream, length);
		#endif

		// sk_data_t* sk_data_new_subset(const sk_data_t* src, size_t offset, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_data_new_subset sk_data_new_subset_delegate;
		internal static sk_data_t sk_data_new_subset (sk_data_t src, /* size_t */ IntPtr offset, /* size_t */ IntPtr length) =>
			(sk_data_new_subset_delegate ??= GetSymbol<Delegates.sk_data_new_subset> ("sk_data_new_subset")).Invoke (src, offset, length);
		#endif

		// sk_data_t* sk_data_new_uninitialized(size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size);
		}
		private static Delegates.sk_data_new_uninitialized sk_data_new_uninitialized_delegate;
		internal static sk_data_t sk_data_new_uninitialized (/* size_t */ IntPtr size) =>
			(sk_data_new_uninitialized_delegate ??= GetSymbol<Delegates.sk_data_new_uninitialized> ("sk_data_new_uninitialized")).Invoke (size);
		#endif

		// sk_data_t* sk_data_new_with_copy(const void* src, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_data_new_with_copy sk_data_new_with_copy_delegate;
		internal static sk_data_t sk_data_new_with_copy (void* src, /* size_t */ IntPtr length) =>
			(sk_data_new_with_copy_delegate ??= GetSymbol<Delegates.sk_data_new_with_copy> ("sk_data_new_with_copy")).Invoke (src, length);
		#endif

		// sk_data_t* sk_data_new_with_proc(const void* ptr, size_t length, sk_data_release_proc proc, void* ctx)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, void* proc, void* ctx);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx);
		}
		private static Delegates.sk_data_new_with_proc sk_data_new_with_proc_delegate;
		internal static sk_data_t sk_data_new_with_proc (void* ptr, /* size_t */ IntPtr length, SKDataReleaseProxyDelegate proc, void* ctx) =>
			(sk_data_new_with_proc_delegate ??= GetSymbol<Delegates.sk_data_new_with_proc> ("sk_data_new_with_proc")).Invoke (ptr, length, proc, ctx);
		#endif

		// void sk_data_ref(const sk_data_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_data_ref (sk_data_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_data_ref (sk_data_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_data_ref (sk_data_t param0);
		}
		private static Delegates.sk_data_ref sk_data_ref_delegate;
		internal static void sk_data_ref (sk_data_t param0) =>
			(sk_data_ref_delegate ??= GetSymbol<Delegates.sk_data_ref> ("sk_data_ref")).Invoke (param0);
		#endif

		// void sk_data_unref(const sk_data_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_data_unref (sk_data_t param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_data_unref (sk_data_t param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_data_unref (sk_data_t param0);
		}
		private static Delegates.sk_data_unref sk_data_unref_delegate;
		internal static void sk_data_unref (sk_data_t param0) =>
			(sk_data_unref_delegate ??= GetSymbol<Delegates.sk_data_unref> ("sk_data_unref")).Invoke (param0);
		#endif

		#endregion

	}
}
