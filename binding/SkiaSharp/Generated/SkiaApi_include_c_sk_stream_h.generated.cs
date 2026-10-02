using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_stream.h

		// void sk_dynamicmemorywstream_copy_to(sk_wstream_dynamicmemorystream_t* cstream, void* data)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data);
		}
		private static Delegates.sk_dynamicmemorywstream_copy_to sk_dynamicmemorywstream_copy_to_delegate;
		internal static void sk_dynamicmemorywstream_copy_to (sk_wstream_dynamicmemorystream_t cstream, void* data) =>
			(sk_dynamicmemorywstream_copy_to_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_copy_to> ("sk_dynamicmemorywstream_copy_to")).Invoke (cstream, data);
		#endif

		// void sk_dynamicmemorywstream_destroy(sk_wstream_dynamicmemorystream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream);
		}
		private static Delegates.sk_dynamicmemorywstream_destroy sk_dynamicmemorywstream_destroy_delegate;
		internal static void sk_dynamicmemorywstream_destroy (sk_wstream_dynamicmemorystream_t cstream) =>
			(sk_dynamicmemorywstream_destroy_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_destroy> ("sk_dynamicmemorywstream_destroy")).Invoke (cstream);
		#endif

		// sk_data_t* sk_dynamicmemorywstream_detach_as_data(sk_wstream_dynamicmemorystream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream);
		}
		private static Delegates.sk_dynamicmemorywstream_detach_as_data sk_dynamicmemorywstream_detach_as_data_delegate;
		internal static sk_data_t sk_dynamicmemorywstream_detach_as_data (sk_wstream_dynamicmemorystream_t cstream) =>
			(sk_dynamicmemorywstream_detach_as_data_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_detach_as_data> ("sk_dynamicmemorywstream_detach_as_data")).Invoke (cstream);
		#endif

		// sk_stream_asset_t* sk_dynamicmemorywstream_detach_as_stream(sk_wstream_dynamicmemorystream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream);
		}
		private static Delegates.sk_dynamicmemorywstream_detach_as_stream sk_dynamicmemorywstream_detach_as_stream_delegate;
		internal static sk_stream_asset_t sk_dynamicmemorywstream_detach_as_stream (sk_wstream_dynamicmemorystream_t cstream) =>
			(sk_dynamicmemorywstream_detach_as_stream_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_detach_as_stream> ("sk_dynamicmemorywstream_detach_as_stream")).Invoke (cstream);
		#endif

		// sk_wstream_dynamicmemorystream_t* sk_dynamicmemorywstream_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new ();
		}
		private static Delegates.sk_dynamicmemorywstream_new sk_dynamicmemorywstream_new_delegate;
		internal static sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new () =>
			(sk_dynamicmemorywstream_new_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_new> ("sk_dynamicmemorywstream_new")).Invoke ();
		#endif

		// bool sk_dynamicmemorywstream_write_to_stream(sk_wstream_dynamicmemorystream_t* cstream, sk_wstream_t* dst)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst);
		}
		private static Delegates.sk_dynamicmemorywstream_write_to_stream sk_dynamicmemorywstream_write_to_stream_delegate;
		internal static bool sk_dynamicmemorywstream_write_to_stream (sk_wstream_dynamicmemorystream_t cstream, sk_wstream_t dst) =>
			(sk_dynamicmemorywstream_write_to_stream_delegate ??= GetSymbol<Delegates.sk_dynamicmemorywstream_write_to_stream> ("sk_dynamicmemorywstream_write_to_stream")).Invoke (cstream, dst);
		#endif

		// void sk_filestream_destroy(sk_stream_filestream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_filestream_destroy (sk_stream_filestream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_filestream_destroy (sk_stream_filestream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_filestream_destroy (sk_stream_filestream_t cstream);
		}
		private static Delegates.sk_filestream_destroy sk_filestream_destroy_delegate;
		internal static void sk_filestream_destroy (sk_stream_filestream_t cstream) =>
			(sk_filestream_destroy_delegate ??= GetSymbol<Delegates.sk_filestream_destroy> ("sk_filestream_destroy")).Invoke (cstream);
		#endif

		// bool sk_filestream_is_valid(sk_stream_filestream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_filestream_is_valid (sk_stream_filestream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_filestream_is_valid (sk_stream_filestream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_filestream_is_valid (sk_stream_filestream_t cstream);
		}
		private static Delegates.sk_filestream_is_valid sk_filestream_is_valid_delegate;
		internal static bool sk_filestream_is_valid (sk_stream_filestream_t cstream) =>
			(sk_filestream_is_valid_delegate ??= GetSymbol<Delegates.sk_filestream_is_valid> ("sk_filestream_is_valid")).Invoke (cstream);
		#endif

		// sk_stream_filestream_t* sk_filestream_new(const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_filestream_t sk_filestream_new (/* char */ void* path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_filestream_t sk_filestream_new (/* char */ void* path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_filestream_t sk_filestream_new (/* char */ void* path);
		}
		private static Delegates.sk_filestream_new sk_filestream_new_delegate;
		internal static sk_stream_filestream_t sk_filestream_new (/* char */ void* path) =>
			(sk_filestream_new_delegate ??= GetSymbol<Delegates.sk_filestream_new> ("sk_filestream_new")).Invoke (path);
		#endif

		// void sk_filewstream_destroy(sk_wstream_filestream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_filewstream_destroy (sk_wstream_filestream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_filewstream_destroy (sk_wstream_filestream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_filewstream_destroy (sk_wstream_filestream_t cstream);
		}
		private static Delegates.sk_filewstream_destroy sk_filewstream_destroy_delegate;
		internal static void sk_filewstream_destroy (sk_wstream_filestream_t cstream) =>
			(sk_filewstream_destroy_delegate ??= GetSymbol<Delegates.sk_filewstream_destroy> ("sk_filewstream_destroy")).Invoke (cstream);
		#endif

		// bool sk_filewstream_is_valid(sk_wstream_filestream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream);
		}
		private static Delegates.sk_filewstream_is_valid sk_filewstream_is_valid_delegate;
		internal static bool sk_filewstream_is_valid (sk_wstream_filestream_t cstream) =>
			(sk_filewstream_is_valid_delegate ??= GetSymbol<Delegates.sk_filewstream_is_valid> ("sk_filewstream_is_valid")).Invoke (cstream);
		#endif

		// sk_wstream_filestream_t* sk_filewstream_new(const char* path)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path);
		}
		private static Delegates.sk_filewstream_new sk_filewstream_new_delegate;
		internal static sk_wstream_filestream_t sk_filewstream_new (/* char */ void* path) =>
			(sk_filewstream_new_delegate ??= GetSymbol<Delegates.sk_filewstream_new> ("sk_filewstream_new")).Invoke (path);
		#endif

		// void sk_memorystream_destroy(sk_stream_memorystream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_memorystream_destroy (sk_stream_memorystream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_memorystream_destroy (sk_stream_memorystream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_memorystream_destroy (sk_stream_memorystream_t cstream);
		}
		private static Delegates.sk_memorystream_destroy sk_memorystream_destroy_delegate;
		internal static void sk_memorystream_destroy (sk_stream_memorystream_t cstream) =>
			(sk_memorystream_destroy_delegate ??= GetSymbol<Delegates.sk_memorystream_destroy> ("sk_memorystream_destroy")).Invoke (cstream);
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_memorystream_t sk_memorystream_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new ();
		}
		private static Delegates.sk_memorystream_new sk_memorystream_new_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new () =>
			(sk_memorystream_new_delegate ??= GetSymbol<Delegates.sk_memorystream_new> ("sk_memorystream_new")).Invoke ();
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new_with_data(const void* data, size_t length, bool copyData)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		}
		private static Delegates.sk_memorystream_new_with_data sk_memorystream_new_with_data_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new_with_data (void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData) =>
			(sk_memorystream_new_with_data_delegate ??= GetSymbol<Delegates.sk_memorystream_new_with_data> ("sk_memorystream_new_with_data")).Invoke (data, length, copyData);
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new_with_length(size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length);
		}
		private static Delegates.sk_memorystream_new_with_length sk_memorystream_new_with_length_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new_with_length (/* size_t */ IntPtr length) =>
			(sk_memorystream_new_with_length_delegate ??= GetSymbol<Delegates.sk_memorystream_new_with_length> ("sk_memorystream_new_with_length")).Invoke (length);
		#endif

		// sk_stream_memorystream_t* sk_memorystream_new_with_skdata(sk_data_t* data)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data);
		}
		private static Delegates.sk_memorystream_new_with_skdata sk_memorystream_new_with_skdata_delegate;
		internal static sk_stream_memorystream_t sk_memorystream_new_with_skdata (sk_data_t data) =>
			(sk_memorystream_new_with_skdata_delegate ??= GetSymbol<Delegates.sk_memorystream_new_with_skdata> ("sk_memorystream_new_with_skdata")).Invoke (data);
		#endif

		// void sk_memorystream_set_memory(sk_stream_memorystream_t* cmemorystream, const void* data, size_t length, bool copyData)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData);
		}
		private static Delegates.sk_memorystream_set_memory sk_memorystream_set_memory_delegate;
		internal static void sk_memorystream_set_memory (sk_stream_memorystream_t cmemorystream, void* data, /* size_t */ IntPtr length, [MarshalAs (UnmanagedType.I1)] bool copyData) =>
			(sk_memorystream_set_memory_delegate ??= GetSymbol<Delegates.sk_memorystream_set_memory> ("sk_memorystream_set_memory")).Invoke (cmemorystream, data, length, copyData);
		#endif

		// void sk_stream_asset_destroy(sk_stream_asset_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_stream_asset_destroy (sk_stream_asset_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_stream_asset_destroy (sk_stream_asset_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_stream_asset_destroy (sk_stream_asset_t cstream);
		}
		private static Delegates.sk_stream_asset_destroy sk_stream_asset_destroy_delegate;
		internal static void sk_stream_asset_destroy (sk_stream_asset_t cstream) =>
			(sk_stream_asset_destroy_delegate ??= GetSymbol<Delegates.sk_stream_asset_destroy> ("sk_stream_asset_destroy")).Invoke (cstream);
		#endif

		// void sk_stream_destroy(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_stream_destroy (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_stream_destroy (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_stream_destroy (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_destroy sk_stream_destroy_delegate;
		internal static void sk_stream_destroy (sk_stream_t cstream) =>
			(sk_stream_destroy_delegate ??= GetSymbol<Delegates.sk_stream_destroy> ("sk_stream_destroy")).Invoke (cstream);
		#endif

		// sk_stream_t* sk_stream_duplicate(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_t sk_stream_duplicate (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_t sk_stream_duplicate (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_t sk_stream_duplicate (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_duplicate sk_stream_duplicate_delegate;
		internal static sk_stream_t sk_stream_duplicate (sk_stream_t cstream) =>
			(sk_stream_duplicate_delegate ??= GetSymbol<Delegates.sk_stream_duplicate> ("sk_stream_duplicate")).Invoke (cstream);
		#endif

		// sk_stream_t* sk_stream_fork(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_stream_t sk_stream_fork (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_stream_t sk_stream_fork (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_stream_t sk_stream_fork (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_fork sk_stream_fork_delegate;
		internal static sk_stream_t sk_stream_fork (sk_stream_t cstream) =>
			(sk_stream_fork_delegate ??= GetSymbol<Delegates.sk_stream_fork> ("sk_stream_fork")).Invoke (cstream);
		#endif

		// sk_data_t* sk_stream_get_data(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_data_t sk_stream_get_data (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_data_t sk_stream_get_data (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_data_t sk_stream_get_data (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_data sk_stream_get_data_delegate;
		internal static sk_data_t sk_stream_get_data (sk_stream_t cstream) =>
			(sk_stream_get_data_delegate ??= GetSymbol<Delegates.sk_stream_get_data> ("sk_stream_get_data")).Invoke (cstream);
		#endif

		// size_t sk_stream_get_length(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_length sk_stream_get_length_delegate;
		internal static /* size_t */ IntPtr sk_stream_get_length (sk_stream_t cstream) =>
			(sk_stream_get_length_delegate ??= GetSymbol<Delegates.sk_stream_get_length> ("sk_stream_get_length")).Invoke (cstream);
		#endif

		// const void* sk_stream_get_memory_base(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void* sk_stream_get_memory_base (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* sk_stream_get_memory_base (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* sk_stream_get_memory_base (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_memory_base sk_stream_get_memory_base_delegate;
		internal static void* sk_stream_get_memory_base (sk_stream_t cstream) =>
			(sk_stream_get_memory_base_delegate ??= GetSymbol<Delegates.sk_stream_get_memory_base> ("sk_stream_get_memory_base")).Invoke (cstream);
		#endif

		// size_t sk_stream_get_position(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_get_position sk_stream_get_position_delegate;
		internal static /* size_t */ IntPtr sk_stream_get_position (sk_stream_t cstream) =>
			(sk_stream_get_position_delegate ??= GetSymbol<Delegates.sk_stream_get_position> ("sk_stream_get_position")).Invoke (cstream);
		#endif

		// bool sk_stream_has_length(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_has_length (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_has_length (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_has_length (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_has_length sk_stream_has_length_delegate;
		internal static bool sk_stream_has_length (sk_stream_t cstream) =>
			(sk_stream_has_length_delegate ??= GetSymbol<Delegates.sk_stream_has_length> ("sk_stream_has_length")).Invoke (cstream);
		#endif

		// bool sk_stream_has_position(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_has_position (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_has_position (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_has_position (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_has_position sk_stream_has_position_delegate;
		internal static bool sk_stream_has_position (sk_stream_t cstream) =>
			(sk_stream_has_position_delegate ??= GetSymbol<Delegates.sk_stream_has_position> ("sk_stream_has_position")).Invoke (cstream);
		#endif

		// bool sk_stream_is_at_end(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_is_at_end (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_is_at_end (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_is_at_end (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_is_at_end sk_stream_is_at_end_delegate;
		internal static bool sk_stream_is_at_end (sk_stream_t cstream) =>
			(sk_stream_is_at_end_delegate ??= GetSymbol<Delegates.sk_stream_is_at_end> ("sk_stream_is_at_end")).Invoke (cstream);
		#endif

		// bool sk_stream_move(sk_stream_t* cstream, int32_t offset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_move (sk_stream_t cstream, Int32 offset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_move (sk_stream_t cstream, Int32 offset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_move (sk_stream_t cstream, Int32 offset);
		}
		private static Delegates.sk_stream_move sk_stream_move_delegate;
		internal static bool sk_stream_move (sk_stream_t cstream, Int32 offset) =>
			(sk_stream_move_delegate ??= GetSymbol<Delegates.sk_stream_move> ("sk_stream_move")).Invoke (cstream, offset);
		#endif

		// size_t sk_stream_peek(sk_stream_t* cstream, void* buffer, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_stream_peek sk_stream_peek_delegate;
		internal static /* size_t */ IntPtr sk_stream_peek (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size) =>
			(sk_stream_peek_delegate ??= GetSymbol<Delegates.sk_stream_peek> ("sk_stream_peek")).Invoke (cstream, buffer, size);
		#endif

		// size_t sk_stream_read(sk_stream_t* cstream, void* buffer, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_stream_read sk_stream_read_delegate;
		internal static /* size_t */ IntPtr sk_stream_read (sk_stream_t cstream, void* buffer, /* size_t */ IntPtr size) =>
			(sk_stream_read_delegate ??= GetSymbol<Delegates.sk_stream_read> ("sk_stream_read")).Invoke (cstream, buffer, size);
		#endif

		// bool sk_stream_read_bool(sk_stream_t* cstream, bool* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer);
		}
		private static Delegates.sk_stream_read_bool sk_stream_read_bool_delegate;
		internal static bool sk_stream_read_bool (sk_stream_t cstream, Byte* buffer) =>
			(sk_stream_read_bool_delegate ??= GetSymbol<Delegates.sk_stream_read_bool> ("sk_stream_read_bool")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_s16(sk_stream_t* cstream, int16_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer);
		}
		private static Delegates.sk_stream_read_s16 sk_stream_read_s16_delegate;
		internal static bool sk_stream_read_s16 (sk_stream_t cstream, Int16* buffer) =>
			(sk_stream_read_s16_delegate ??= GetSymbol<Delegates.sk_stream_read_s16> ("sk_stream_read_s16")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_s32(sk_stream_t* cstream, int32_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer);
		}
		private static Delegates.sk_stream_read_s32 sk_stream_read_s32_delegate;
		internal static bool sk_stream_read_s32 (sk_stream_t cstream, Int32* buffer) =>
			(sk_stream_read_s32_delegate ??= GetSymbol<Delegates.sk_stream_read_s32> ("sk_stream_read_s32")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_s8(sk_stream_t* cstream, int8_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer);
		}
		private static Delegates.sk_stream_read_s8 sk_stream_read_s8_delegate;
		internal static bool sk_stream_read_s8 (sk_stream_t cstream, SByte* buffer) =>
			(sk_stream_read_s8_delegate ??= GetSymbol<Delegates.sk_stream_read_s8> ("sk_stream_read_s8")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_u16(sk_stream_t* cstream, uint16_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer);
		}
		private static Delegates.sk_stream_read_u16 sk_stream_read_u16_delegate;
		internal static bool sk_stream_read_u16 (sk_stream_t cstream, UInt16* buffer) =>
			(sk_stream_read_u16_delegate ??= GetSymbol<Delegates.sk_stream_read_u16> ("sk_stream_read_u16")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_u32(sk_stream_t* cstream, uint32_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer);
		}
		private static Delegates.sk_stream_read_u32 sk_stream_read_u32_delegate;
		internal static bool sk_stream_read_u32 (sk_stream_t cstream, UInt32* buffer) =>
			(sk_stream_read_u32_delegate ??= GetSymbol<Delegates.sk_stream_read_u32> ("sk_stream_read_u32")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_read_u8(sk_stream_t* cstream, uint8_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer);
		}
		private static Delegates.sk_stream_read_u8 sk_stream_read_u8_delegate;
		internal static bool sk_stream_read_u8 (sk_stream_t cstream, Byte* buffer) =>
			(sk_stream_read_u8_delegate ??= GetSymbol<Delegates.sk_stream_read_u8> ("sk_stream_read_u8")).Invoke (cstream, buffer);
		#endif

		// bool sk_stream_rewind(sk_stream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_rewind (sk_stream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_rewind (sk_stream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_rewind (sk_stream_t cstream);
		}
		private static Delegates.sk_stream_rewind sk_stream_rewind_delegate;
		internal static bool sk_stream_rewind (sk_stream_t cstream) =>
			(sk_stream_rewind_delegate ??= GetSymbol<Delegates.sk_stream_rewind> ("sk_stream_rewind")).Invoke (cstream);
		#endif

		// bool sk_stream_seek(sk_stream_t* cstream, size_t position)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position);
		}
		private static Delegates.sk_stream_seek sk_stream_seek_delegate;
		internal static bool sk_stream_seek (sk_stream_t cstream, /* size_t */ IntPtr position) =>
			(sk_stream_seek_delegate ??= GetSymbol<Delegates.sk_stream_seek> ("sk_stream_seek")).Invoke (cstream, position);
		#endif

		// size_t sk_stream_skip(sk_stream_t* cstream, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_stream_skip sk_stream_skip_delegate;
		internal static /* size_t */ IntPtr sk_stream_skip (sk_stream_t cstream, /* size_t */ IntPtr size) =>
			(sk_stream_skip_delegate ??= GetSymbol<Delegates.sk_stream_skip> ("sk_stream_skip")).Invoke (cstream, size);
		#endif

		// size_t sk_wstream_bytes_written(sk_wstream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream);
		}
		private static Delegates.sk_wstream_bytes_written sk_wstream_bytes_written_delegate;
		internal static /* size_t */ IntPtr sk_wstream_bytes_written (sk_wstream_t cstream) =>
			(sk_wstream_bytes_written_delegate ??= GetSymbol<Delegates.sk_wstream_bytes_written> ("sk_wstream_bytes_written")).Invoke (cstream);
		#endif

		// void sk_wstream_flush(sk_wstream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_wstream_flush (sk_wstream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_wstream_flush (sk_wstream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_wstream_flush (sk_wstream_t cstream);
		}
		private static Delegates.sk_wstream_flush sk_wstream_flush_delegate;
		internal static void sk_wstream_flush (sk_wstream_t cstream) =>
			(sk_wstream_flush_delegate ??= GetSymbol<Delegates.sk_wstream_flush> ("sk_wstream_flush")).Invoke (cstream);
		#endif

		// int sk_wstream_get_size_of_packed_uint(size_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value);
		}
		private static Delegates.sk_wstream_get_size_of_packed_uint sk_wstream_get_size_of_packed_uint_delegate;
		internal static Int32 sk_wstream_get_size_of_packed_uint (/* size_t */ IntPtr value) =>
			(sk_wstream_get_size_of_packed_uint_delegate ??= GetSymbol<Delegates.sk_wstream_get_size_of_packed_uint> ("sk_wstream_get_size_of_packed_uint")).Invoke (value);
		#endif

		// bool sk_wstream_newline(sk_wstream_t* cstream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_newline (sk_wstream_t cstream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_newline (sk_wstream_t cstream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_newline (sk_wstream_t cstream);
		}
		private static Delegates.sk_wstream_newline sk_wstream_newline_delegate;
		internal static bool sk_wstream_newline (sk_wstream_t cstream) =>
			(sk_wstream_newline_delegate ??= GetSymbol<Delegates.sk_wstream_newline> ("sk_wstream_newline")).Invoke (cstream);
		#endif

		// bool sk_wstream_write(sk_wstream_t* cstream, const void* buffer, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_wstream_write sk_wstream_write_delegate;
		internal static bool sk_wstream_write (sk_wstream_t cstream, void* buffer, /* size_t */ IntPtr size) =>
			(sk_wstream_write_delegate ??= GetSymbol<Delegates.sk_wstream_write> ("sk_wstream_write")).Invoke (cstream, buffer, size);
		#endif

		// bool sk_wstream_write_16(sk_wstream_t* cstream, uint16_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value);
		}
		private static Delegates.sk_wstream_write_16 sk_wstream_write_16_delegate;
		internal static bool sk_wstream_write_16 (sk_wstream_t cstream, UInt16 value) =>
			(sk_wstream_write_16_delegate ??= GetSymbol<Delegates.sk_wstream_write_16> ("sk_wstream_write_16")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_32(sk_wstream_t* cstream, uint32_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value);
		}
		private static Delegates.sk_wstream_write_32 sk_wstream_write_32_delegate;
		internal static bool sk_wstream_write_32 (sk_wstream_t cstream, UInt32 value) =>
			(sk_wstream_write_32_delegate ??= GetSymbol<Delegates.sk_wstream_write_32> ("sk_wstream_write_32")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_8(sk_wstream_t* cstream, uint8_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value);
		}
		private static Delegates.sk_wstream_write_8 sk_wstream_write_8_delegate;
		internal static bool sk_wstream_write_8 (sk_wstream_t cstream, Byte value) =>
			(sk_wstream_write_8_delegate ??= GetSymbol<Delegates.sk_wstream_write_8> ("sk_wstream_write_8")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_bigdec_as_text(sk_wstream_t* cstream, int64_t value, int minDigits)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits);
		}
		private static Delegates.sk_wstream_write_bigdec_as_text sk_wstream_write_bigdec_as_text_delegate;
		internal static bool sk_wstream_write_bigdec_as_text (sk_wstream_t cstream, Int64 value, Int32 minDigits) =>
			(sk_wstream_write_bigdec_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_bigdec_as_text> ("sk_wstream_write_bigdec_as_text")).Invoke (cstream, value, minDigits);
		#endif

		// bool sk_wstream_write_bool(sk_wstream_t* cstream, bool value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value);
		}
		private static Delegates.sk_wstream_write_bool sk_wstream_write_bool_delegate;
		internal static bool sk_wstream_write_bool (sk_wstream_t cstream, [MarshalAs (UnmanagedType.I1)] bool value) =>
			(sk_wstream_write_bool_delegate ??= GetSymbol<Delegates.sk_wstream_write_bool> ("sk_wstream_write_bool")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_dec_as_text(sk_wstream_t* cstream, int32_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value);
		}
		private static Delegates.sk_wstream_write_dec_as_text sk_wstream_write_dec_as_text_delegate;
		internal static bool sk_wstream_write_dec_as_text (sk_wstream_t cstream, Int32 value) =>
			(sk_wstream_write_dec_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_dec_as_text> ("sk_wstream_write_dec_as_text")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_hex_as_text(sk_wstream_t* cstream, uint32_t value, int minDigits)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits);
		}
		private static Delegates.sk_wstream_write_hex_as_text sk_wstream_write_hex_as_text_delegate;
		internal static bool sk_wstream_write_hex_as_text (sk_wstream_t cstream, UInt32 value, Int32 minDigits) =>
			(sk_wstream_write_hex_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_hex_as_text> ("sk_wstream_write_hex_as_text")).Invoke (cstream, value, minDigits);
		#endif

		// bool sk_wstream_write_packed_uint(sk_wstream_t* cstream, size_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value);
		}
		private static Delegates.sk_wstream_write_packed_uint sk_wstream_write_packed_uint_delegate;
		internal static bool sk_wstream_write_packed_uint (sk_wstream_t cstream, /* size_t */ IntPtr value) =>
			(sk_wstream_write_packed_uint_delegate ??= GetSymbol<Delegates.sk_wstream_write_packed_uint> ("sk_wstream_write_packed_uint")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_scalar(sk_wstream_t* cstream, float value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value);
		}
		private static Delegates.sk_wstream_write_scalar sk_wstream_write_scalar_delegate;
		internal static bool sk_wstream_write_scalar (sk_wstream_t cstream, Single value) =>
			(sk_wstream_write_scalar_delegate ??= GetSymbol<Delegates.sk_wstream_write_scalar> ("sk_wstream_write_scalar")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_scalar_as_text(sk_wstream_t* cstream, float value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value);
		}
		private static Delegates.sk_wstream_write_scalar_as_text sk_wstream_write_scalar_as_text_delegate;
		internal static bool sk_wstream_write_scalar_as_text (sk_wstream_t cstream, Single value) =>
			(sk_wstream_write_scalar_as_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_scalar_as_text> ("sk_wstream_write_scalar_as_text")).Invoke (cstream, value);
		#endif

		// bool sk_wstream_write_stream(sk_wstream_t* cstream, sk_stream_t* input, size_t length)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length);
		}
		private static Delegates.sk_wstream_write_stream sk_wstream_write_stream_delegate;
		internal static bool sk_wstream_write_stream (sk_wstream_t cstream, sk_stream_t input, /* size_t */ IntPtr length) =>
			(sk_wstream_write_stream_delegate ??= GetSymbol<Delegates.sk_wstream_write_stream> ("sk_wstream_write_stream")).Invoke (cstream, input, length);
		#endif

		// bool sk_wstream_write_text(sk_wstream_t* cstream, const char* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value);
		}
		private static Delegates.sk_wstream_write_text sk_wstream_write_text_delegate;
		internal static bool sk_wstream_write_text (sk_wstream_t cstream, [MarshalAs (UnmanagedType.LPStr)] String value) =>
			(sk_wstream_write_text_delegate ??= GetSymbol<Delegates.sk_wstream_write_text> ("sk_wstream_write_text")).Invoke (cstream, value);
		#endif

		#endregion

	}
}
