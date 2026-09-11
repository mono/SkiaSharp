using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-blob.h

		// extern hb_blob_t* hb_blob_copy_writable_or_fail(hb_blob_t* blob)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_copy_writable_or_fail (hb_blob_t blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_copy_writable_or_fail (hb_blob_t blob);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_create (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_create_from_file ([MarshalAs (UnmanagedType.LPStr)] String file_name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_from_file ([MarshalAs (UnmanagedType.LPStr)] String file_name);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_create_from_file_or_fail (/* char */ void* file_name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_from_file_or_fail (/* char */ void* file_name);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_create_or_fail (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_or_fail (/* char */ void* data, UInt32 length, MemoryMode mode, void* user_data, DestroyProxyDelegate destroy);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_create_sub_blob (hb_blob_t parent, UInt32 offset, UInt32 length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_create_sub_blob (hb_blob_t parent, UInt32 offset, UInt32 length);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_blob_destroy (hb_blob_t blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_blob_destroy (hb_blob_t blob);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void* hb_blob_get_data (hb_blob_t blob, UInt32* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_blob_get_data (hb_blob_t blob, UInt32* length);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void* hb_blob_get_data_writable (hb_blob_t blob, UInt32* length);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_blob_get_data_writable (hb_blob_t blob, UInt32* length);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_get_empty ();
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_blob_get_length (hb_blob_t blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_blob_get_length (hb_blob_t blob);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_blob_is_immutable (hb_blob_t blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_blob_is_immutable (hb_blob_t blob);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_blob_make_immutable (hb_blob_t blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_blob_make_immutable (hb_blob_t blob);
		#endif
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
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_blob_t hb_blob_reference (hb_blob_t blob);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_blob_t hb_blob_reference (hb_blob_t blob);
		#endif
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

	}
}
