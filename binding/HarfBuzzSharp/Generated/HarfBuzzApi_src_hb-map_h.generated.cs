using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-map.h

		// extern hb_bool_t hb_map_allocation_successful(const hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_map_allocation_successful (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_allocation_successful (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_allocation_successful (IntPtr map);
		}
		private static Delegates.hb_map_allocation_successful hb_map_allocation_successful_delegate;
		internal static bool hb_map_allocation_successful (IntPtr map) =>
			(hb_map_allocation_successful_delegate ??= GetSymbol<Delegates.hb_map_allocation_successful> ("hb_map_allocation_successful")).Invoke (map);
		#endif

		// extern void hb_map_clear(hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_clear (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_clear (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_clear (IntPtr map);
		}
		private static Delegates.hb_map_clear hb_map_clear_delegate;
		internal static void hb_map_clear (IntPtr map) =>
			(hb_map_clear_delegate ??= GetSymbol<Delegates.hb_map_clear> ("hb_map_clear")).Invoke (map);
		#endif

		// extern hb_map_t* hb_map_copy(const hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_map_copy (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_map_copy (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_map_copy (IntPtr map);
		}
		private static Delegates.hb_map_copy hb_map_copy_delegate;
		internal static IntPtr hb_map_copy (IntPtr map) =>
			(hb_map_copy_delegate ??= GetSymbol<Delegates.hb_map_copy> ("hb_map_copy")).Invoke (map);
		#endif

		// extern hb_map_t* hb_map_create()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_map_create ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_map_create ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_map_create ();
		}
		private static Delegates.hb_map_create hb_map_create_delegate;
		internal static IntPtr hb_map_create () =>
			(hb_map_create_delegate ??= GetSymbol<Delegates.hb_map_create> ("hb_map_create")).Invoke ();
		#endif

		// extern void hb_map_del(hb_map_t* map, hb_codepoint_t key)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_del (IntPtr map, UInt32 key);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_del (IntPtr map, UInt32 key);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_del (IntPtr map, UInt32 key);
		}
		private static Delegates.hb_map_del hb_map_del_delegate;
		internal static void hb_map_del (IntPtr map, UInt32 key) =>
			(hb_map_del_delegate ??= GetSymbol<Delegates.hb_map_del> ("hb_map_del")).Invoke (map, key);
		#endif

		// extern void hb_map_destroy(hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_destroy (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_destroy (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_destroy (IntPtr map);
		}
		private static Delegates.hb_map_destroy hb_map_destroy_delegate;
		internal static void hb_map_destroy (IntPtr map) =>
			(hb_map_destroy_delegate ??= GetSymbol<Delegates.hb_map_destroy> ("hb_map_destroy")).Invoke (map);
		#endif

		// extern hb_codepoint_t hb_map_get(const hb_map_t* map, hb_codepoint_t key)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_map_get (IntPtr map, UInt32 key);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_map_get (IntPtr map, UInt32 key);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_map_get (IntPtr map, UInt32 key);
		}
		private static Delegates.hb_map_get hb_map_get_delegate;
		internal static UInt32 hb_map_get (IntPtr map, UInt32 key) =>
			(hb_map_get_delegate ??= GetSymbol<Delegates.hb_map_get> ("hb_map_get")).Invoke (map, key);
		#endif

		// extern hb_map_t* hb_map_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_map_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_map_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_map_get_empty ();
		}
		private static Delegates.hb_map_get_empty hb_map_get_empty_delegate;
		internal static IntPtr hb_map_get_empty () =>
			(hb_map_get_empty_delegate ??= GetSymbol<Delegates.hb_map_get_empty> ("hb_map_get_empty")).Invoke ();
		#endif

		// extern unsigned int hb_map_get_population(const hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_map_get_population (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_map_get_population (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_map_get_population (IntPtr map);
		}
		private static Delegates.hb_map_get_population hb_map_get_population_delegate;
		internal static UInt32 hb_map_get_population (IntPtr map) =>
			(hb_map_get_population_delegate ??= GetSymbol<Delegates.hb_map_get_population> ("hb_map_get_population")).Invoke (map);
		#endif

		// extern hb_bool_t hb_map_has(const hb_map_t* map, hb_codepoint_t key)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_map_has (IntPtr map, UInt32 key);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_has (IntPtr map, UInt32 key);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_has (IntPtr map, UInt32 key);
		}
		private static Delegates.hb_map_has hb_map_has_delegate;
		internal static bool hb_map_has (IntPtr map, UInt32 key) =>
			(hb_map_has_delegate ??= GetSymbol<Delegates.hb_map_has> ("hb_map_has")).Invoke (map, key);
		#endif

		// extern unsigned int hb_map_hash(const hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_map_hash (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_map_hash (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_map_hash (IntPtr map);
		}
		private static Delegates.hb_map_hash hb_map_hash_delegate;
		internal static UInt32 hb_map_hash (IntPtr map) =>
			(hb_map_hash_delegate ??= GetSymbol<Delegates.hb_map_hash> ("hb_map_hash")).Invoke (map);
		#endif

		// extern hb_bool_t hb_map_is_empty(const hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_map_is_empty (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_is_empty (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_is_empty (IntPtr map);
		}
		private static Delegates.hb_map_is_empty hb_map_is_empty_delegate;
		internal static bool hb_map_is_empty (IntPtr map) =>
			(hb_map_is_empty_delegate ??= GetSymbol<Delegates.hb_map_is_empty> ("hb_map_is_empty")).Invoke (map);
		#endif

		// extern hb_bool_t hb_map_is_equal(const hb_map_t* map, const hb_map_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_map_is_equal (IntPtr map, IntPtr other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_is_equal (IntPtr map, IntPtr other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_is_equal (IntPtr map, IntPtr other);
		}
		private static Delegates.hb_map_is_equal hb_map_is_equal_delegate;
		internal static bool hb_map_is_equal (IntPtr map, IntPtr other) =>
			(hb_map_is_equal_delegate ??= GetSymbol<Delegates.hb_map_is_equal> ("hb_map_is_equal")).Invoke (map, other);
		#endif

		// extern void hb_map_keys(const hb_map_t* map, hb_set_t* keys)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_keys (IntPtr map, IntPtr keys);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_keys (IntPtr map, IntPtr keys);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_keys (IntPtr map, IntPtr keys);
		}
		private static Delegates.hb_map_keys hb_map_keys_delegate;
		internal static void hb_map_keys (IntPtr map, IntPtr keys) =>
			(hb_map_keys_delegate ??= GetSymbol<Delegates.hb_map_keys> ("hb_map_keys")).Invoke (map, keys);
		#endif

		// extern hb_bool_t hb_map_next(const hb_map_t* map, int* idx, hb_codepoint_t* key, hb_codepoint_t* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_map_next (IntPtr map, Int32* idx, UInt32* key, UInt32* value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_map_next (IntPtr map, Int32* idx, UInt32* key, UInt32* value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_map_next (IntPtr map, Int32* idx, UInt32* key, UInt32* value);
		}
		private static Delegates.hb_map_next hb_map_next_delegate;
		internal static bool hb_map_next (IntPtr map, Int32* idx, UInt32* key, UInt32* value) =>
			(hb_map_next_delegate ??= GetSymbol<Delegates.hb_map_next> ("hb_map_next")).Invoke (map, idx, key, value);
		#endif

		// extern hb_map_t* hb_map_reference(hb_map_t* map)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_map_reference (IntPtr map);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_map_reference (IntPtr map);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_map_reference (IntPtr map);
		}
		private static Delegates.hb_map_reference hb_map_reference_delegate;
		internal static IntPtr hb_map_reference (IntPtr map) =>
			(hb_map_reference_delegate ??= GetSymbol<Delegates.hb_map_reference> ("hb_map_reference")).Invoke (map);
		#endif

		// extern void hb_map_set(hb_map_t* map, hb_codepoint_t key, hb_codepoint_t value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_set (IntPtr map, UInt32 key, UInt32 value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_set (IntPtr map, UInt32 key, UInt32 value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_set (IntPtr map, UInt32 key, UInt32 value);
		}
		private static Delegates.hb_map_set hb_map_set_delegate;
		internal static void hb_map_set (IntPtr map, UInt32 key, UInt32 value) =>
			(hb_map_set_delegate ??= GetSymbol<Delegates.hb_map_set> ("hb_map_set")).Invoke (map, key, value);
		#endif

		// extern void hb_map_update(hb_map_t* map, const hb_map_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_update (IntPtr map, IntPtr other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_update (IntPtr map, IntPtr other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_update (IntPtr map, IntPtr other);
		}
		private static Delegates.hb_map_update hb_map_update_delegate;
		internal static void hb_map_update (IntPtr map, IntPtr other) =>
			(hb_map_update_delegate ??= GetSymbol<Delegates.hb_map_update> ("hb_map_update")).Invoke (map, other);
		#endif

		// extern void hb_map_values(const hb_map_t* map, hb_set_t* values)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_map_values (IntPtr map, IntPtr values);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_map_values (IntPtr map, IntPtr values);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_map_values (IntPtr map, IntPtr values);
		}
		private static Delegates.hb_map_values hb_map_values_delegate;
		internal static void hb_map_values (IntPtr map, IntPtr values) =>
			(hb_map_values_delegate ??= GetSymbol<Delegates.hb_map_values> ("hb_map_values")).Invoke (map, values);
		#endif

		#endregion

	}
}
