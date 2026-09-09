using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-unicode.h

		// extern hb_unicode_combining_class_t hb_unicode_combining_class(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UnicodeCombiningClass hb_unicode_combining_class (IntPtr ufuncs, UInt32 unicode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UnicodeCombiningClass hb_unicode_combining_class (IntPtr ufuncs, UInt32 unicode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UnicodeCombiningClass hb_unicode_combining_class (IntPtr ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_combining_class hb_unicode_combining_class_delegate;
		internal static UnicodeCombiningClass hb_unicode_combining_class (IntPtr ufuncs, UInt32 unicode) =>
			(hb_unicode_combining_class_delegate ??= GetSymbol<Delegates.hb_unicode_combining_class> ("hb_unicode_combining_class")).Invoke (ufuncs, unicode);
		#endif

		// extern hb_bool_t hb_unicode_compose(hb_unicode_funcs_t* ufuncs, hb_codepoint_t a, hb_codepoint_t b, hb_codepoint_t* ab)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_unicode_compose (IntPtr ufuncs, UInt32 a, UInt32 b, UInt32* ab);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_unicode_compose (IntPtr ufuncs, UInt32 a, UInt32 b, UInt32* ab);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_unicode_compose (IntPtr ufuncs, UInt32 a, UInt32 b, UInt32* ab);
		}
		private static Delegates.hb_unicode_compose hb_unicode_compose_delegate;
		internal static bool hb_unicode_compose (IntPtr ufuncs, UInt32 a, UInt32 b, UInt32* ab) =>
			(hb_unicode_compose_delegate ??= GetSymbol<Delegates.hb_unicode_compose> ("hb_unicode_compose")).Invoke (ufuncs, a, b, ab);
		#endif

		// extern hb_bool_t hb_unicode_decompose(hb_unicode_funcs_t* ufuncs, hb_codepoint_t ab, hb_codepoint_t* a, hb_codepoint_t* b)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_unicode_decompose (IntPtr ufuncs, UInt32 ab, UInt32* a, UInt32* b);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_unicode_decompose (IntPtr ufuncs, UInt32 ab, UInt32* a, UInt32* b);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_unicode_decompose (IntPtr ufuncs, UInt32 ab, UInt32* a, UInt32* b);
		}
		private static Delegates.hb_unicode_decompose hb_unicode_decompose_delegate;
		internal static bool hb_unicode_decompose (IntPtr ufuncs, UInt32 ab, UInt32* a, UInt32* b) =>
			(hb_unicode_decompose_delegate ??= GetSymbol<Delegates.hb_unicode_decompose> ("hb_unicode_decompose")).Invoke (ufuncs, ab, a, b);
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_create(hb_unicode_funcs_t* parent)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_unicode_funcs_create (IntPtr parent);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_unicode_funcs_create (IntPtr parent);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_unicode_funcs_create (IntPtr parent);
		}
		private static Delegates.hb_unicode_funcs_create hb_unicode_funcs_create_delegate;
		internal static IntPtr hb_unicode_funcs_create (IntPtr parent) =>
			(hb_unicode_funcs_create_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_create> ("hb_unicode_funcs_create")).Invoke (parent);
		#endif

		// extern void hb_unicode_funcs_destroy(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_destroy (IntPtr ufuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_destroy (IntPtr ufuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_destroy (IntPtr ufuncs);
		}
		private static Delegates.hb_unicode_funcs_destroy hb_unicode_funcs_destroy_delegate;
		internal static void hb_unicode_funcs_destroy (IntPtr ufuncs) =>
			(hb_unicode_funcs_destroy_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_destroy> ("hb_unicode_funcs_destroy")).Invoke (ufuncs);
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_get_default()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_unicode_funcs_get_default ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_unicode_funcs_get_default ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_unicode_funcs_get_default ();
		}
		private static Delegates.hb_unicode_funcs_get_default hb_unicode_funcs_get_default_delegate;
		internal static IntPtr hb_unicode_funcs_get_default () =>
			(hb_unicode_funcs_get_default_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_get_default> ("hb_unicode_funcs_get_default")).Invoke ();
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_get_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_unicode_funcs_get_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_unicode_funcs_get_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_unicode_funcs_get_empty ();
		}
		private static Delegates.hb_unicode_funcs_get_empty hb_unicode_funcs_get_empty_delegate;
		internal static IntPtr hb_unicode_funcs_get_empty () =>
			(hb_unicode_funcs_get_empty_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_get_empty> ("hb_unicode_funcs_get_empty")).Invoke ();
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_get_parent(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_unicode_funcs_get_parent (IntPtr ufuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_unicode_funcs_get_parent (IntPtr ufuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_unicode_funcs_get_parent (IntPtr ufuncs);
		}
		private static Delegates.hb_unicode_funcs_get_parent hb_unicode_funcs_get_parent_delegate;
		internal static IntPtr hb_unicode_funcs_get_parent (IntPtr ufuncs) =>
			(hb_unicode_funcs_get_parent_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_get_parent> ("hb_unicode_funcs_get_parent")).Invoke (ufuncs);
		#endif

		// extern hb_bool_t hb_unicode_funcs_is_immutable(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_unicode_funcs_is_immutable (IntPtr ufuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_unicode_funcs_is_immutable (IntPtr ufuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_unicode_funcs_is_immutable (IntPtr ufuncs);
		}
		private static Delegates.hb_unicode_funcs_is_immutable hb_unicode_funcs_is_immutable_delegate;
		internal static bool hb_unicode_funcs_is_immutable (IntPtr ufuncs) =>
			(hb_unicode_funcs_is_immutable_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_is_immutable> ("hb_unicode_funcs_is_immutable")).Invoke (ufuncs);
		#endif

		// extern void hb_unicode_funcs_make_immutable(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_make_immutable (IntPtr ufuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_make_immutable (IntPtr ufuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_make_immutable (IntPtr ufuncs);
		}
		private static Delegates.hb_unicode_funcs_make_immutable hb_unicode_funcs_make_immutable_delegate;
		internal static void hb_unicode_funcs_make_immutable (IntPtr ufuncs) =>
			(hb_unicode_funcs_make_immutable_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_make_immutable> ("hb_unicode_funcs_make_immutable")).Invoke (ufuncs);
		#endif

		// extern hb_unicode_funcs_t* hb_unicode_funcs_reference(hb_unicode_funcs_t* ufuncs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_unicode_funcs_reference (IntPtr ufuncs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_unicode_funcs_reference (IntPtr ufuncs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_unicode_funcs_reference (IntPtr ufuncs);
		}
		private static Delegates.hb_unicode_funcs_reference hb_unicode_funcs_reference_delegate;
		internal static IntPtr hb_unicode_funcs_reference (IntPtr ufuncs) =>
			(hb_unicode_funcs_reference_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_reference> ("hb_unicode_funcs_reference")).Invoke (ufuncs);
		#endif

		// extern void hb_unicode_funcs_set_combining_class_func(hb_unicode_funcs_t* ufuncs, hb_unicode_combining_class_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_set_combining_class_func (IntPtr ufuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_combining_class_func (IntPtr ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_combining_class_func (IntPtr ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_combining_class_func hb_unicode_funcs_set_combining_class_func_delegate;
		internal static void hb_unicode_funcs_set_combining_class_func (IntPtr ufuncs, UnicodeCombiningClassProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_combining_class_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_combining_class_func> ("hb_unicode_funcs_set_combining_class_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_compose_func(hb_unicode_funcs_t* ufuncs, hb_unicode_compose_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_set_compose_func (IntPtr ufuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_compose_func (IntPtr ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_compose_func (IntPtr ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_compose_func hb_unicode_funcs_set_compose_func_delegate;
		internal static void hb_unicode_funcs_set_compose_func (IntPtr ufuncs, UnicodeComposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_compose_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_compose_func> ("hb_unicode_funcs_set_compose_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_decompose_func(hb_unicode_funcs_t* ufuncs, hb_unicode_decompose_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_set_decompose_func (IntPtr ufuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_decompose_func (IntPtr ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_decompose_func (IntPtr ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_decompose_func hb_unicode_funcs_set_decompose_func_delegate;
		internal static void hb_unicode_funcs_set_decompose_func (IntPtr ufuncs, UnicodeDecomposeProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_decompose_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_decompose_func> ("hb_unicode_funcs_set_decompose_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_general_category_func(hb_unicode_funcs_t* ufuncs, hb_unicode_general_category_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_set_general_category_func (IntPtr ufuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_general_category_func (IntPtr ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_general_category_func (IntPtr ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_general_category_func hb_unicode_funcs_set_general_category_func_delegate;
		internal static void hb_unicode_funcs_set_general_category_func (IntPtr ufuncs, UnicodeGeneralCategoryProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_general_category_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_general_category_func> ("hb_unicode_funcs_set_general_category_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_mirroring_func(hb_unicode_funcs_t* ufuncs, hb_unicode_mirroring_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_set_mirroring_func (IntPtr ufuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_mirroring_func (IntPtr ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_mirroring_func (IntPtr ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_mirroring_func hb_unicode_funcs_set_mirroring_func_delegate;
		internal static void hb_unicode_funcs_set_mirroring_func (IntPtr ufuncs, UnicodeMirroringProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_mirroring_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_mirroring_func> ("hb_unicode_funcs_set_mirroring_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern void hb_unicode_funcs_set_script_func(hb_unicode_funcs_t* ufuncs, hb_unicode_script_func_t func, void* user_data, hb_destroy_func_t destroy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_unicode_funcs_set_script_func (IntPtr ufuncs, void* func, void* user_data, void* destroy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_unicode_funcs_set_script_func (IntPtr ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_unicode_funcs_set_script_func (IntPtr ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy);
		}
		private static Delegates.hb_unicode_funcs_set_script_func hb_unicode_funcs_set_script_func_delegate;
		internal static void hb_unicode_funcs_set_script_func (IntPtr ufuncs, UnicodeScriptProxyDelegate func, void* user_data, DestroyProxyDelegate destroy) =>
			(hb_unicode_funcs_set_script_func_delegate ??= GetSymbol<Delegates.hb_unicode_funcs_set_script_func> ("hb_unicode_funcs_set_script_func")).Invoke (ufuncs, func, user_data, destroy);
		#endif

		// extern hb_unicode_general_category_t hb_unicode_general_category(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UnicodeGeneralCategory hb_unicode_general_category (IntPtr ufuncs, UInt32 unicode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UnicodeGeneralCategory hb_unicode_general_category (IntPtr ufuncs, UInt32 unicode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UnicodeGeneralCategory hb_unicode_general_category (IntPtr ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_general_category hb_unicode_general_category_delegate;
		internal static UnicodeGeneralCategory hb_unicode_general_category (IntPtr ufuncs, UInt32 unicode) =>
			(hb_unicode_general_category_delegate ??= GetSymbol<Delegates.hb_unicode_general_category> ("hb_unicode_general_category")).Invoke (ufuncs, unicode);
		#endif

		// extern hb_codepoint_t hb_unicode_mirroring(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_unicode_mirroring (IntPtr ufuncs, UInt32 unicode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_unicode_mirroring (IntPtr ufuncs, UInt32 unicode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_unicode_mirroring (IntPtr ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_mirroring hb_unicode_mirroring_delegate;
		internal static UInt32 hb_unicode_mirroring (IntPtr ufuncs, UInt32 unicode) =>
			(hb_unicode_mirroring_delegate ??= GetSymbol<Delegates.hb_unicode_mirroring> ("hb_unicode_mirroring")).Invoke (ufuncs, unicode);
		#endif

		// extern hb_script_t hb_unicode_script(hb_unicode_funcs_t* ufuncs, hb_codepoint_t unicode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_unicode_script (IntPtr ufuncs, UInt32 unicode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_unicode_script (IntPtr ufuncs, UInt32 unicode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_unicode_script (IntPtr ufuncs, UInt32 unicode);
		}
		private static Delegates.hb_unicode_script hb_unicode_script_delegate;
		internal static UInt32 hb_unicode_script (IntPtr ufuncs, UInt32 unicode) =>
			(hb_unicode_script_delegate ??= GetSymbol<Delegates.hb_unicode_script> ("hb_unicode_script")).Invoke (ufuncs, unicode);
		#endif

		#endregion

	}
}
