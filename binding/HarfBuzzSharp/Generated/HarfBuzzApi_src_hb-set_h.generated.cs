using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-set.h

		// extern void hb_set_add(hb_set_t* set, hb_codepoint_t codepoint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_add (hb_set_t set, UInt32 codepoint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_add (hb_set_t set, UInt32 codepoint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_add (hb_set_t set, UInt32 codepoint);
		}
		private static Delegates.hb_set_add hb_set_add_delegate;
		internal static void hb_set_add (hb_set_t set, UInt32 codepoint) =>
			(hb_set_add_delegate ??= GetSymbol<Delegates.hb_set_add> ("hb_set_add")).Invoke (set, codepoint);
		#endif

		// extern void hb_set_add_range(hb_set_t* set, hb_codepoint_t first, hb_codepoint_t last)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last);
		}
		private static Delegates.hb_set_add_range hb_set_add_range_delegate;
		internal static void hb_set_add_range (hb_set_t set, UInt32 first, UInt32 last) =>
			(hb_set_add_range_delegate ??= GetSymbol<Delegates.hb_set_add_range> ("hb_set_add_range")).Invoke (set, first, last);
		#endif

		// extern void hb_set_add_sorted_array(hb_set_t* set, const hb_codepoint_t* sorted_codepoints, unsigned int num_codepoints)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_add_sorted_array (hb_set_t set, UInt32* sorted_codepoints, UInt32 num_codepoints);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_add_sorted_array (hb_set_t set, UInt32* sorted_codepoints, UInt32 num_codepoints);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_add_sorted_array (hb_set_t set, UInt32* sorted_codepoints, UInt32 num_codepoints);
		}
		private static Delegates.hb_set_add_sorted_array hb_set_add_sorted_array_delegate;
		internal static void hb_set_add_sorted_array (hb_set_t set, UInt32* sorted_codepoints, UInt32 num_codepoints) =>
			(hb_set_add_sorted_array_delegate ??= GetSymbol<Delegates.hb_set_add_sorted_array> ("hb_set_add_sorted_array")).Invoke (set, sorted_codepoints, num_codepoints);
		#endif

		// extern hb_bool_t hb_set_allocation_successful(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_allocation_successful (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_allocation_successful (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_allocation_successful (hb_set_t set);
		}
		private static Delegates.hb_set_allocation_successful hb_set_allocation_successful_delegate;
		internal static bool hb_set_allocation_successful (hb_set_t set) =>
			(hb_set_allocation_successful_delegate ??= GetSymbol<Delegates.hb_set_allocation_successful> ("hb_set_allocation_successful")).Invoke (set);
		#endif

		// extern void hb_set_clear(hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_clear (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_clear (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_clear (hb_set_t set);
		}
		private static Delegates.hb_set_clear hb_set_clear_delegate;
		internal static void hb_set_clear (hb_set_t set) =>
			(hb_set_clear_delegate ??= GetSymbol<Delegates.hb_set_clear> ("hb_set_clear")).Invoke (set);
		#endif

		// extern hb_set_t* hb_set_copy(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_set_t hb_set_copy (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_copy (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_copy (hb_set_t set);
		}
		private static Delegates.hb_set_copy hb_set_copy_delegate;
		internal static hb_set_t hb_set_copy (hb_set_t set) =>
			(hb_set_copy_delegate ??= GetSymbol<Delegates.hb_set_copy> ("hb_set_copy")).Invoke (set);
		#endif

		// extern hb_set_t* hb_set_create()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_set_t hb_set_create ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_create ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_create ();
		}
		private static Delegates.hb_set_create hb_set_create_delegate;
		internal static hb_set_t hb_set_create () =>
			(hb_set_create_delegate ??= GetSymbol<Delegates.hb_set_create> ("hb_set_create")).Invoke ();
		#endif

		// extern void hb_set_del(hb_set_t* set, hb_codepoint_t codepoint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_del (hb_set_t set, UInt32 codepoint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_del (hb_set_t set, UInt32 codepoint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_del (hb_set_t set, UInt32 codepoint);
		}
		private static Delegates.hb_set_del hb_set_del_delegate;
		internal static void hb_set_del (hb_set_t set, UInt32 codepoint) =>
			(hb_set_del_delegate ??= GetSymbol<Delegates.hb_set_del> ("hb_set_del")).Invoke (set, codepoint);
		#endif

		// extern void hb_set_del_range(hb_set_t* set, hb_codepoint_t first, hb_codepoint_t last)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last);
		}
		private static Delegates.hb_set_del_range hb_set_del_range_delegate;
		internal static void hb_set_del_range (hb_set_t set, UInt32 first, UInt32 last) =>
			(hb_set_del_range_delegate ??= GetSymbol<Delegates.hb_set_del_range> ("hb_set_del_range")).Invoke (set, first, last);
		#endif

		// extern void hb_set_destroy(hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_destroy (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_destroy (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_destroy (hb_set_t set);
		}
		private static Delegates.hb_set_destroy hb_set_destroy_delegate;
		internal static void hb_set_destroy (hb_set_t set) =>
			(hb_set_destroy_delegate ??= GetSymbol<Delegates.hb_set_destroy> ("hb_set_destroy")).Invoke (set);
		#endif

		// extern hb_set_t* hb_set_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_set_t hb_set_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_get_empty ();
		}
		private static Delegates.hb_set_get_empty hb_set_get_empty_delegate;
		internal static hb_set_t hb_set_get_empty () =>
			(hb_set_get_empty_delegate ??= GetSymbol<Delegates.hb_set_get_empty> ("hb_set_get_empty")).Invoke ();
		#endif

		// extern hb_codepoint_t hb_set_get_max(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_set_get_max (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_get_max (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_get_max (hb_set_t set);
		}
		private static Delegates.hb_set_get_max hb_set_get_max_delegate;
		internal static UInt32 hb_set_get_max (hb_set_t set) =>
			(hb_set_get_max_delegate ??= GetSymbol<Delegates.hb_set_get_max> ("hb_set_get_max")).Invoke (set);
		#endif

		// extern hb_codepoint_t hb_set_get_min(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_set_get_min (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_get_min (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_get_min (hb_set_t set);
		}
		private static Delegates.hb_set_get_min hb_set_get_min_delegate;
		internal static UInt32 hb_set_get_min (hb_set_t set) =>
			(hb_set_get_min_delegate ??= GetSymbol<Delegates.hb_set_get_min> ("hb_set_get_min")).Invoke (set);
		#endif

		// extern unsigned int hb_set_get_population(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_set_get_population (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_get_population (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_get_population (hb_set_t set);
		}
		private static Delegates.hb_set_get_population hb_set_get_population_delegate;
		internal static UInt32 hb_set_get_population (hb_set_t set) =>
			(hb_set_get_population_delegate ??= GetSymbol<Delegates.hb_set_get_population> ("hb_set_get_population")).Invoke (set);
		#endif

		// extern hb_bool_t hb_set_has(const hb_set_t* set, hb_codepoint_t codepoint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_has (hb_set_t set, UInt32 codepoint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_has (hb_set_t set, UInt32 codepoint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_has (hb_set_t set, UInt32 codepoint);
		}
		private static Delegates.hb_set_has hb_set_has_delegate;
		internal static bool hb_set_has (hb_set_t set, UInt32 codepoint) =>
			(hb_set_has_delegate ??= GetSymbol<Delegates.hb_set_has> ("hb_set_has")).Invoke (set, codepoint);
		#endif

		// extern unsigned int hb_set_hash(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_set_hash (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_hash (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_hash (hb_set_t set);
		}
		private static Delegates.hb_set_hash hb_set_hash_delegate;
		internal static UInt32 hb_set_hash (hb_set_t set) =>
			(hb_set_hash_delegate ??= GetSymbol<Delegates.hb_set_hash> ("hb_set_hash")).Invoke (set);
		#endif

		// extern void hb_set_intersect(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_intersect (hb_set_t set, hb_set_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_intersect (hb_set_t set, hb_set_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_intersect (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_intersect hb_set_intersect_delegate;
		internal static void hb_set_intersect (hb_set_t set, hb_set_t other) =>
			(hb_set_intersect_delegate ??= GetSymbol<Delegates.hb_set_intersect> ("hb_set_intersect")).Invoke (set, other);
		#endif

		// extern void hb_set_invert(hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_invert (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_invert (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_invert (hb_set_t set);
		}
		private static Delegates.hb_set_invert hb_set_invert_delegate;
		internal static void hb_set_invert (hb_set_t set) =>
			(hb_set_invert_delegate ??= GetSymbol<Delegates.hb_set_invert> ("hb_set_invert")).Invoke (set);
		#endif

		// extern hb_bool_t hb_set_is_empty(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_is_empty (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_empty (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_empty (hb_set_t set);
		}
		private static Delegates.hb_set_is_empty hb_set_is_empty_delegate;
		internal static bool hb_set_is_empty (hb_set_t set) =>
			(hb_set_is_empty_delegate ??= GetSymbol<Delegates.hb_set_is_empty> ("hb_set_is_empty")).Invoke (set);
		#endif

		// extern hb_bool_t hb_set_is_equal(const hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_is_equal (hb_set_t set, hb_set_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_equal (hb_set_t set, hb_set_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_equal (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_is_equal hb_set_is_equal_delegate;
		internal static bool hb_set_is_equal (hb_set_t set, hb_set_t other) =>
			(hb_set_is_equal_delegate ??= GetSymbol<Delegates.hb_set_is_equal> ("hb_set_is_equal")).Invoke (set, other);
		#endif

		// extern hb_bool_t hb_set_is_inverted(const hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_is_inverted (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_inverted (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_inverted (hb_set_t set);
		}
		private static Delegates.hb_set_is_inverted hb_set_is_inverted_delegate;
		internal static bool hb_set_is_inverted (hb_set_t set) =>
			(hb_set_is_inverted_delegate ??= GetSymbol<Delegates.hb_set_is_inverted> ("hb_set_is_inverted")).Invoke (set);
		#endif

		// extern hb_bool_t hb_set_is_subset(const hb_set_t* set, const hb_set_t* larger_set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set);
		}
		private static Delegates.hb_set_is_subset hb_set_is_subset_delegate;
		internal static bool hb_set_is_subset (hb_set_t set, hb_set_t larger_set) =>
			(hb_set_is_subset_delegate ??= GetSymbol<Delegates.hb_set_is_subset> ("hb_set_is_subset")).Invoke (set, larger_set);
		#endif

		// extern hb_bool_t hb_set_next(const hb_set_t* set, hb_codepoint_t* codepoint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_next (hb_set_t set, UInt32* codepoint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_next (hb_set_t set, UInt32* codepoint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_next (hb_set_t set, UInt32* codepoint);
		}
		private static Delegates.hb_set_next hb_set_next_delegate;
		internal static bool hb_set_next (hb_set_t set, UInt32* codepoint) =>
			(hb_set_next_delegate ??= GetSymbol<Delegates.hb_set_next> ("hb_set_next")).Invoke (set, codepoint);
		#endif

		// extern unsigned int hb_set_next_many(const hb_set_t* set, hb_codepoint_t codepoint, hb_codepoint_t* out, unsigned int size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_set_next_many (hb_set_t set, UInt32 codepoint, UInt32* @out, UInt32 size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_set_next_many (hb_set_t set, UInt32 codepoint, UInt32* @out, UInt32 size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_set_next_many (hb_set_t set, UInt32 codepoint, UInt32* @out, UInt32 size);
		}
		private static Delegates.hb_set_next_many hb_set_next_many_delegate;
		internal static UInt32 hb_set_next_many (hb_set_t set, UInt32 codepoint, UInt32* @out, UInt32 size) =>
			(hb_set_next_many_delegate ??= GetSymbol<Delegates.hb_set_next_many> ("hb_set_next_many")).Invoke (set, codepoint, @out, size);
		#endif

		// extern hb_bool_t hb_set_next_range(const hb_set_t* set, hb_codepoint_t* first, hb_codepoint_t* last)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last);
		}
		private static Delegates.hb_set_next_range hb_set_next_range_delegate;
		internal static bool hb_set_next_range (hb_set_t set, UInt32* first, UInt32* last) =>
			(hb_set_next_range_delegate ??= GetSymbol<Delegates.hb_set_next_range> ("hb_set_next_range")).Invoke (set, first, last);
		#endif

		// extern hb_bool_t hb_set_previous(const hb_set_t* set, hb_codepoint_t* codepoint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_previous (hb_set_t set, UInt32* codepoint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_previous (hb_set_t set, UInt32* codepoint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_previous (hb_set_t set, UInt32* codepoint);
		}
		private static Delegates.hb_set_previous hb_set_previous_delegate;
		internal static bool hb_set_previous (hb_set_t set, UInt32* codepoint) =>
			(hb_set_previous_delegate ??= GetSymbol<Delegates.hb_set_previous> ("hb_set_previous")).Invoke (set, codepoint);
		#endif

		// extern hb_bool_t hb_set_previous_range(const hb_set_t* set, hb_codepoint_t* first, hb_codepoint_t* last)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last);
		}
		private static Delegates.hb_set_previous_range hb_set_previous_range_delegate;
		internal static bool hb_set_previous_range (hb_set_t set, UInt32* first, UInt32* last) =>
			(hb_set_previous_range_delegate ??= GetSymbol<Delegates.hb_set_previous_range> ("hb_set_previous_range")).Invoke (set, first, last);
		#endif

		// extern hb_set_t* hb_set_reference(hb_set_t* set)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial hb_set_t hb_set_reference (hb_set_t set);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern hb_set_t hb_set_reference (hb_set_t set);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate hb_set_t hb_set_reference (hb_set_t set);
		}
		private static Delegates.hb_set_reference hb_set_reference_delegate;
		internal static hb_set_t hb_set_reference (hb_set_t set) =>
			(hb_set_reference_delegate ??= GetSymbol<Delegates.hb_set_reference> ("hb_set_reference")).Invoke (set);
		#endif

		// extern void hb_set_set(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_set (hb_set_t set, hb_set_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_set (hb_set_t set, hb_set_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_set (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_set hb_set_set_delegate;
		internal static void hb_set_set (hb_set_t set, hb_set_t other) =>
			(hb_set_set_delegate ??= GetSymbol<Delegates.hb_set_set> ("hb_set_set")).Invoke (set, other);
		#endif

		// extern void hb_set_subtract(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_subtract (hb_set_t set, hb_set_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_subtract (hb_set_t set, hb_set_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_subtract (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_subtract hb_set_subtract_delegate;
		internal static void hb_set_subtract (hb_set_t set, hb_set_t other) =>
			(hb_set_subtract_delegate ??= GetSymbol<Delegates.hb_set_subtract> ("hb_set_subtract")).Invoke (set, other);
		#endif

		// extern void hb_set_symmetric_difference(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_symmetric_difference (hb_set_t set, hb_set_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_symmetric_difference (hb_set_t set, hb_set_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_symmetric_difference (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_symmetric_difference hb_set_symmetric_difference_delegate;
		internal static void hb_set_symmetric_difference (hb_set_t set, hb_set_t other) =>
			(hb_set_symmetric_difference_delegate ??= GetSymbol<Delegates.hb_set_symmetric_difference> ("hb_set_symmetric_difference")).Invoke (set, other);
		#endif

		// extern void hb_set_union(hb_set_t* set, const hb_set_t* other)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_set_union (hb_set_t set, hb_set_t other);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_set_union (hb_set_t set, hb_set_t other);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_set_union (hb_set_t set, hb_set_t other);
		}
		private static Delegates.hb_set_union hb_set_union_delegate;
		internal static void hb_set_union (hb_set_t set, hb_set_t other) =>
			(hb_set_union_delegate ??= GetSymbol<Delegates.hb_set_union> ("hb_set_union")).Invoke (set, other);
		#endif

		#endregion

	}
}
