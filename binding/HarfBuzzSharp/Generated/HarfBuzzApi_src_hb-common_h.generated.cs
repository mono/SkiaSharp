using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-common.h

		// extern uint8_t hb_color_get_alpha(hb_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Byte hb_color_get_alpha (HBColor color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_alpha (HBColor color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_alpha (HBColor color);
		}
		private static Delegates.hb_color_get_alpha hb_color_get_alpha_delegate;
		internal static Byte hb_color_get_alpha (HBColor color) =>
			(hb_color_get_alpha_delegate ??= GetSymbol<Delegates.hb_color_get_alpha> ("hb_color_get_alpha")).Invoke (color);
		#endif

		// extern uint8_t hb_color_get_blue(hb_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Byte hb_color_get_blue (HBColor color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_blue (HBColor color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_blue (HBColor color);
		}
		private static Delegates.hb_color_get_blue hb_color_get_blue_delegate;
		internal static Byte hb_color_get_blue (HBColor color) =>
			(hb_color_get_blue_delegate ??= GetSymbol<Delegates.hb_color_get_blue> ("hb_color_get_blue")).Invoke (color);
		#endif

		// extern uint8_t hb_color_get_green(hb_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Byte hb_color_get_green (HBColor color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_green (HBColor color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_green (HBColor color);
		}
		private static Delegates.hb_color_get_green hb_color_get_green_delegate;
		internal static Byte hb_color_get_green (HBColor color) =>
			(hb_color_get_green_delegate ??= GetSymbol<Delegates.hb_color_get_green> ("hb_color_get_green")).Invoke (color);
		#endif

		// extern uint8_t hb_color_get_red(hb_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Byte hb_color_get_red (HBColor color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Byte hb_color_get_red (HBColor color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Byte hb_color_get_red (HBColor color);
		}
		private static Delegates.hb_color_get_red hb_color_get_red_delegate;
		internal static Byte hb_color_get_red (HBColor color) =>
			(hb_color_get_red_delegate ??= GetSymbol<Delegates.hb_color_get_red> ("hb_color_get_red")).Invoke (color);
		#endif

		// extern hb_direction_t hb_direction_from_string(const char* str, int len)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Direction hb_direction_from_string (/* char */ void* str, Int32 len);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Direction hb_direction_from_string (/* char */ void* str, Int32 len);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Direction hb_direction_from_string (/* char */ void* str, Int32 len);
		}
		private static Delegates.hb_direction_from_string hb_direction_from_string_delegate;
		internal static Direction hb_direction_from_string (/* char */ void* str, Int32 len) =>
			(hb_direction_from_string_delegate ??= GetSymbol<Delegates.hb_direction_from_string> ("hb_direction_from_string")).Invoke (str, len);
		#endif

		// extern const char* hb_direction_to_string(hb_direction_t direction)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void* hb_direction_to_string (Direction direction);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_direction_to_string (Direction direction);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_direction_to_string (Direction direction);
		}
		private static Delegates.hb_direction_to_string hb_direction_to_string_delegate;
		internal static /* char */ void* hb_direction_to_string (Direction direction) =>
			(hb_direction_to_string_delegate ??= GetSymbol<Delegates.hb_direction_to_string> ("hb_direction_to_string")).Invoke (direction);
		#endif

		// extern hb_bool_t hb_feature_from_string(const char* str, int len, hb_feature_t* feature)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature);
		}
		private static Delegates.hb_feature_from_string hb_feature_from_string_delegate;
		internal static bool hb_feature_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len, Feature* feature) =>
			(hb_feature_from_string_delegate ??= GetSymbol<Delegates.hb_feature_from_string> ("hb_feature_from_string")).Invoke (str, len, feature);
		#endif

		// extern void hb_feature_to_string(hb_feature_t* feature, char* buf, unsigned int size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size);
		}
		private static Delegates.hb_feature_to_string hb_feature_to_string_delegate;
		internal static void hb_feature_to_string (Feature* feature, /* char */ void* buf, UInt32 size) =>
			(hb_feature_to_string_delegate ??= GetSymbol<Delegates.hb_feature_to_string> ("hb_feature_to_string")).Invoke (feature, buf, size);
		#endif

		// extern void hb_free(void* ptr)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_free (void* ptr);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_free (void* ptr);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_free (void* ptr);
		}
		private static Delegates.hb_free hb_free_delegate;
		internal static void hb_free (void* ptr) =>
			(hb_free_delegate ??= GetSymbol<Delegates.hb_free> ("hb_free")).Invoke (ptr);
		#endif

		// extern hb_language_t hb_language_from_string(const char* str, int len)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		}
		private static Delegates.hb_language_from_string hb_language_from_string_delegate;
		internal static IntPtr hb_language_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len) =>
			(hb_language_from_string_delegate ??= GetSymbol<Delegates.hb_language_from_string> ("hb_language_from_string")).Invoke (str, len);
		#endif

		// extern hb_language_t hb_language_get_default()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial IntPtr hb_language_get_default ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr hb_language_get_default ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr hb_language_get_default ();
		}
		private static Delegates.hb_language_get_default hb_language_get_default_delegate;
		internal static IntPtr hb_language_get_default () =>
			(hb_language_get_default_delegate ??= GetSymbol<Delegates.hb_language_get_default> ("hb_language_get_default")).Invoke ();
		#endif

		// extern hb_bool_t hb_language_matches(hb_language_t language, hb_language_t specific)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_language_matches (IntPtr language, IntPtr specific);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_language_matches (IntPtr language, IntPtr specific);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_language_matches (IntPtr language, IntPtr specific);
		}
		private static Delegates.hb_language_matches hb_language_matches_delegate;
		internal static bool hb_language_matches (IntPtr language, IntPtr specific) =>
			(hb_language_matches_delegate ??= GetSymbol<Delegates.hb_language_matches> ("hb_language_matches")).Invoke (language, specific);
		#endif

		// extern const char* hb_language_to_string(hb_language_t language)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void* hb_language_to_string (IntPtr language);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void* hb_language_to_string (IntPtr language);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void* hb_language_to_string (IntPtr language);
		}
		private static Delegates.hb_language_to_string hb_language_to_string_delegate;
		internal static /* char */ void* hb_language_to_string (IntPtr language) =>
			(hb_language_to_string_delegate ??= GetSymbol<Delegates.hb_language_to_string> ("hb_language_to_string")).Invoke (language);
		#endif

		// extern void* hb_malloc(size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void* hb_malloc (/* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* hb_malloc (/* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* hb_malloc (/* size_t */ IntPtr size);
		}
		private static Delegates.hb_malloc hb_malloc_delegate;
		internal static void* hb_malloc (/* size_t */ IntPtr size) =>
			(hb_malloc_delegate ??= GetSymbol<Delegates.hb_malloc> ("hb_malloc")).Invoke (size);
		#endif

		// extern void* hb_realloc(void* ptr, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void* hb_realloc (void* ptr, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void* hb_realloc (void* ptr, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void* hb_realloc (void* ptr, /* size_t */ IntPtr size);
		}
		private static Delegates.hb_realloc hb_realloc_delegate;
		internal static void* hb_realloc (void* ptr, /* size_t */ IntPtr size) =>
			(hb_realloc_delegate ??= GetSymbol<Delegates.hb_realloc> ("hb_realloc")).Invoke (ptr, size);
		#endif

		// extern hb_script_t hb_script_from_iso15924_tag(hb_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_script_from_iso15924_tag (UInt32 tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_script_from_iso15924_tag (UInt32 tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_script_from_iso15924_tag (UInt32 tag);
		}
		private static Delegates.hb_script_from_iso15924_tag hb_script_from_iso15924_tag_delegate;
		internal static UInt32 hb_script_from_iso15924_tag (UInt32 tag) =>
			(hb_script_from_iso15924_tag_delegate ??= GetSymbol<Delegates.hb_script_from_iso15924_tag> ("hb_script_from_iso15924_tag")).Invoke (tag);
		#endif

		// extern hb_script_t hb_script_from_string(const char* str, int len)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len);
		}
		private static Delegates.hb_script_from_string hb_script_from_string_delegate;
		internal static UInt32 hb_script_from_string ([MarshalAs (UnmanagedType.LPStr)] String str, Int32 len) =>
			(hb_script_from_string_delegate ??= GetSymbol<Delegates.hb_script_from_string> ("hb_script_from_string")).Invoke (str, len);
		#endif

		// extern hb_direction_t hb_script_get_horizontal_direction(hb_script_t script)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Direction hb_script_get_horizontal_direction (UInt32 script);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Direction hb_script_get_horizontal_direction (UInt32 script);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Direction hb_script_get_horizontal_direction (UInt32 script);
		}
		private static Delegates.hb_script_get_horizontal_direction hb_script_get_horizontal_direction_delegate;
		internal static Direction hb_script_get_horizontal_direction (UInt32 script) =>
			(hb_script_get_horizontal_direction_delegate ??= GetSymbol<Delegates.hb_script_get_horizontal_direction> ("hb_script_get_horizontal_direction")).Invoke (script);
		#endif

		// extern hb_tag_t hb_script_to_iso15924_tag(hb_script_t script)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_script_to_iso15924_tag (UInt32 script);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_script_to_iso15924_tag (UInt32 script);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_script_to_iso15924_tag (UInt32 script);
		}
		private static Delegates.hb_script_to_iso15924_tag hb_script_to_iso15924_tag_delegate;
		internal static UInt32 hb_script_to_iso15924_tag (UInt32 script) =>
			(hb_script_to_iso15924_tag_delegate ??= GetSymbol<Delegates.hb_script_to_iso15924_tag> ("hb_script_to_iso15924_tag")).Invoke (script);
		#endif

		// extern hb_tag_t hb_tag_from_string(const char* str, int len)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial UInt32 hb_tag_from_string (/* char */ void* str, Int32 len);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt32 hb_tag_from_string (/* char */ void* str, Int32 len);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt32 hb_tag_from_string (/* char */ void* str, Int32 len);
		}
		private static Delegates.hb_tag_from_string hb_tag_from_string_delegate;
		internal static UInt32 hb_tag_from_string (/* char */ void* str, Int32 len) =>
			(hb_tag_from_string_delegate ??= GetSymbol<Delegates.hb_tag_from_string> ("hb_tag_from_string")).Invoke (str, len);
		#endif

		// extern void hb_tag_to_string(hb_tag_t tag, char* buf)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_tag_to_string (UInt32 tag, /* char */ void* buf);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_tag_to_string (UInt32 tag, /* char */ void* buf);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_tag_to_string (UInt32 tag, /* char */ void* buf);
		}
		private static Delegates.hb_tag_to_string hb_tag_to_string_delegate;
		internal static void hb_tag_to_string (UInt32 tag, /* char */ void* buf) =>
			(hb_tag_to_string_delegate ??= GetSymbol<Delegates.hb_tag_to_string> ("hb_tag_to_string")).Invoke (tag, buf);
		#endif

		// extern hb_bool_t hb_variation_from_string(const char* str, int len, hb_variation_t* variation)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation);
		}
		private static Delegates.hb_variation_from_string hb_variation_from_string_delegate;
		internal static bool hb_variation_from_string (/* char */ void* str, Int32 len, Variation* variation) =>
			(hb_variation_from_string_delegate ??= GetSymbol<Delegates.hb_variation_from_string> ("hb_variation_from_string")).Invoke (str, len, variation);
		#endif

		// extern void hb_variation_to_string(hb_variation_t* variation, char* buf, unsigned int size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size);
		}
		private static Delegates.hb_variation_to_string hb_variation_to_string_delegate;
		internal static void hb_variation_to_string (Variation* variation, /* char */ void* buf, UInt32 size) =>
			(hb_variation_to_string_delegate ??= GetSymbol<Delegates.hb_variation_to_string> ("hb_variation_to_string")).Invoke (variation, buf, size);
		#endif

		#endregion

	}
}
