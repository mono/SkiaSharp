using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_typeface.h

		// int sk_fontmgr_count_families(sk_fontmgr_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_fontmgr_count_families (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontmgr_count_families (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontmgr_count_families (IntPtr param0);
		}
		private static Delegates.sk_fontmgr_count_families sk_fontmgr_count_families_delegate;
		internal static Int32 sk_fontmgr_count_families (IntPtr param0) =>
			(sk_fontmgr_count_families_delegate ??= GetSymbol<Delegates.sk_fontmgr_count_families> ("sk_fontmgr_count_families")).Invoke (param0);
		#endif

		// sk_fontmgr_t* sk_fontmgr_create_default()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_create_default ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_create_default ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_create_default ();
		}
		private static Delegates.sk_fontmgr_create_default sk_fontmgr_create_default_delegate;
		internal static IntPtr sk_fontmgr_create_default () =>
			(sk_fontmgr_create_default_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_default> ("sk_fontmgr_create_default")).Invoke ();
		#endif

		// sk_typeface_t* sk_fontmgr_create_from_data(sk_fontmgr_t*, sk_data_t* data, int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_create_from_data (IntPtr param0, IntPtr data, Int32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_create_from_data (IntPtr param0, IntPtr data, Int32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_create_from_data (IntPtr param0, IntPtr data, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_from_data sk_fontmgr_create_from_data_delegate;
		internal static IntPtr sk_fontmgr_create_from_data (IntPtr param0, IntPtr data, Int32 index) =>
			(sk_fontmgr_create_from_data_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_from_data> ("sk_fontmgr_create_from_data")).Invoke (param0, data, index);
		#endif

		// sk_typeface_t* sk_fontmgr_create_from_file(sk_fontmgr_t*, const char* path, int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_create_from_file (IntPtr param0, /* char */ void* path, Int32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_create_from_file (IntPtr param0, /* char */ void* path, Int32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_create_from_file (IntPtr param0, /* char */ void* path, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_from_file sk_fontmgr_create_from_file_delegate;
		internal static IntPtr sk_fontmgr_create_from_file (IntPtr param0, /* char */ void* path, Int32 index) =>
			(sk_fontmgr_create_from_file_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_from_file> ("sk_fontmgr_create_from_file")).Invoke (param0, path, index);
		#endif

		// sk_typeface_t* sk_fontmgr_create_from_stream(sk_fontmgr_t*, sk_stream_asset_t* stream, int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_create_from_stream (IntPtr param0, IntPtr stream, Int32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_create_from_stream (IntPtr param0, IntPtr stream, Int32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_create_from_stream (IntPtr param0, IntPtr stream, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_from_stream sk_fontmgr_create_from_stream_delegate;
		internal static IntPtr sk_fontmgr_create_from_stream (IntPtr param0, IntPtr stream, Int32 index) =>
			(sk_fontmgr_create_from_stream_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_from_stream> ("sk_fontmgr_create_from_stream")).Invoke (param0, stream, index);
		#endif

		// sk_fontstyleset_t* sk_fontmgr_create_styleset(sk_fontmgr_t*, int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_create_styleset (IntPtr param0, Int32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_create_styleset (IntPtr param0, Int32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_create_styleset (IntPtr param0, Int32 index);
		}
		private static Delegates.sk_fontmgr_create_styleset sk_fontmgr_create_styleset_delegate;
		internal static IntPtr sk_fontmgr_create_styleset (IntPtr param0, Int32 index) =>
			(sk_fontmgr_create_styleset_delegate ??= GetSymbol<Delegates.sk_fontmgr_create_styleset> ("sk_fontmgr_create_styleset")).Invoke (param0, index);
		#endif

		// void sk_fontmgr_get_family_name(sk_fontmgr_t*, int index, sk_string_t* familyName)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_fontmgr_get_family_name (IntPtr param0, Int32 index, IntPtr familyName);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontmgr_get_family_name (IntPtr param0, Int32 index, IntPtr familyName);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontmgr_get_family_name (IntPtr param0, Int32 index, IntPtr familyName);
		}
		private static Delegates.sk_fontmgr_get_family_name sk_fontmgr_get_family_name_delegate;
		internal static void sk_fontmgr_get_family_name (IntPtr param0, Int32 index, IntPtr familyName) =>
			(sk_fontmgr_get_family_name_delegate ??= GetSymbol<Delegates.sk_fontmgr_get_family_name> ("sk_fontmgr_get_family_name")).Invoke (param0, index, familyName);
		#endif

		// sk_typeface_t* sk_fontmgr_legacy_create_typeface(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_legacy_create_typeface (IntPtr param0, IntPtr familyName, IntPtr style);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_legacy_create_typeface (IntPtr param0, IntPtr familyName, IntPtr style);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_legacy_create_typeface (IntPtr param0, IntPtr familyName, IntPtr style);
		}
		private static Delegates.sk_fontmgr_legacy_create_typeface sk_fontmgr_legacy_create_typeface_delegate;
		internal static IntPtr sk_fontmgr_legacy_create_typeface (IntPtr param0, IntPtr familyName, IntPtr style) =>
			(sk_fontmgr_legacy_create_typeface_delegate ??= GetSymbol<Delegates.sk_fontmgr_legacy_create_typeface> ("sk_fontmgr_legacy_create_typeface")).Invoke (param0, familyName, style);
		#endif

		// sk_fontstyleset_t* sk_fontmgr_match_family(sk_fontmgr_t*, const char* familyName)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_match_family (IntPtr param0, IntPtr familyName);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_match_family (IntPtr param0, IntPtr familyName);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_match_family (IntPtr param0, IntPtr familyName);
		}
		private static Delegates.sk_fontmgr_match_family sk_fontmgr_match_family_delegate;
		internal static IntPtr sk_fontmgr_match_family (IntPtr param0, IntPtr familyName) =>
			(sk_fontmgr_match_family_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_family> ("sk_fontmgr_match_family")).Invoke (param0, familyName);
		#endif

		// sk_typeface_t* sk_fontmgr_match_family_style(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_match_family_style (IntPtr param0, IntPtr familyName, IntPtr style);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_match_family_style (IntPtr param0, IntPtr familyName, IntPtr style);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_match_family_style (IntPtr param0, IntPtr familyName, IntPtr style);
		}
		private static Delegates.sk_fontmgr_match_family_style sk_fontmgr_match_family_style_delegate;
		internal static IntPtr sk_fontmgr_match_family_style (IntPtr param0, IntPtr familyName, IntPtr style) =>
			(sk_fontmgr_match_family_style_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_family_style> ("sk_fontmgr_match_family_style")).Invoke (param0, familyName, style);
		#endif

		// sk_typeface_t* sk_fontmgr_match_family_style_character(sk_fontmgr_t*, const char* familyName, sk_fontstyle_t* style, const char** bcp47, int bcp47Count, int32_t character)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontmgr_match_family_style_character (IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontmgr_match_family_style_character (IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontmgr_match_family_style_character (IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character);
		}
		private static Delegates.sk_fontmgr_match_family_style_character sk_fontmgr_match_family_style_character_delegate;
		internal static IntPtr sk_fontmgr_match_family_style_character (IntPtr param0, IntPtr familyName, IntPtr style, [MarshalAs (UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] String[] bcp47, Int32 bcp47Count, Int32 character) =>
			(sk_fontmgr_match_family_style_character_delegate ??= GetSymbol<Delegates.sk_fontmgr_match_family_style_character> ("sk_fontmgr_match_family_style_character")).Invoke (param0, familyName, style, bcp47, bcp47Count, character);
		#endif

		// void sk_fontmgr_unref(sk_fontmgr_t*)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_fontmgr_unref (IntPtr param0);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontmgr_unref (IntPtr param0);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontmgr_unref (IntPtr param0);
		}
		private static Delegates.sk_fontmgr_unref sk_fontmgr_unref_delegate;
		internal static void sk_fontmgr_unref (IntPtr param0) =>
			(sk_fontmgr_unref_delegate ??= GetSymbol<Delegates.sk_fontmgr_unref> ("sk_fontmgr_unref")).Invoke (param0);
		#endif

		// void sk_fontstyle_delete(sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_fontstyle_delete (IntPtr fs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyle_delete (IntPtr fs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontstyle_delete (IntPtr fs);
		}
		private static Delegates.sk_fontstyle_delete sk_fontstyle_delete_delegate;
		internal static void sk_fontstyle_delete (IntPtr fs) =>
			(sk_fontstyle_delete_delegate ??= GetSymbol<Delegates.sk_fontstyle_delete> ("sk_fontstyle_delete")).Invoke (fs);
		#endif

		// sk_font_style_slant_t sk_fontstyle_get_slant(const sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKFontStyleSlant sk_fontstyle_get_slant (IntPtr fs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontStyleSlant sk_fontstyle_get_slant (IntPtr fs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontStyleSlant sk_fontstyle_get_slant (IntPtr fs);
		}
		private static Delegates.sk_fontstyle_get_slant sk_fontstyle_get_slant_delegate;
		internal static SKFontStyleSlant sk_fontstyle_get_slant (IntPtr fs) =>
			(sk_fontstyle_get_slant_delegate ??= GetSymbol<Delegates.sk_fontstyle_get_slant> ("sk_fontstyle_get_slant")).Invoke (fs);
		#endif

		// int sk_fontstyle_get_weight(const sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_fontstyle_get_weight (IntPtr fs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyle_get_weight (IntPtr fs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontstyle_get_weight (IntPtr fs);
		}
		private static Delegates.sk_fontstyle_get_weight sk_fontstyle_get_weight_delegate;
		internal static Int32 sk_fontstyle_get_weight (IntPtr fs) =>
			(sk_fontstyle_get_weight_delegate ??= GetSymbol<Delegates.sk_fontstyle_get_weight> ("sk_fontstyle_get_weight")).Invoke (fs);
		#endif

		// int sk_fontstyle_get_width(const sk_fontstyle_t* fs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_fontstyle_get_width (IntPtr fs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyle_get_width (IntPtr fs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontstyle_get_width (IntPtr fs);
		}
		private static Delegates.sk_fontstyle_get_width sk_fontstyle_get_width_delegate;
		internal static Int32 sk_fontstyle_get_width (IntPtr fs) =>
			(sk_fontstyle_get_width_delegate ??= GetSymbol<Delegates.sk_fontstyle_get_width> ("sk_fontstyle_get_width")).Invoke (fs);
		#endif

		// sk_fontstyle_t* sk_fontstyle_new(int weight, int width, sk_font_style_slant_t slant)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant);
		}
		private static Delegates.sk_fontstyle_new sk_fontstyle_new_delegate;
		internal static IntPtr sk_fontstyle_new (Int32 weight, Int32 width, SKFontStyleSlant slant) =>
			(sk_fontstyle_new_delegate ??= GetSymbol<Delegates.sk_fontstyle_new> ("sk_fontstyle_new")).Invoke (weight, width, slant);
		#endif

		// sk_fontstyleset_t* sk_fontstyleset_create_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontstyleset_create_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontstyleset_create_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontstyleset_create_empty ();
		}
		private static Delegates.sk_fontstyleset_create_empty sk_fontstyleset_create_empty_delegate;
		internal static IntPtr sk_fontstyleset_create_empty () =>
			(sk_fontstyleset_create_empty_delegate ??= GetSymbol<Delegates.sk_fontstyleset_create_empty> ("sk_fontstyleset_create_empty")).Invoke ();
		#endif

		// sk_typeface_t* sk_fontstyleset_create_typeface(sk_fontstyleset_t* fss, int index)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontstyleset_create_typeface (IntPtr fss, Int32 index);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontstyleset_create_typeface (IntPtr fss, Int32 index);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontstyleset_create_typeface (IntPtr fss, Int32 index);
		}
		private static Delegates.sk_fontstyleset_create_typeface sk_fontstyleset_create_typeface_delegate;
		internal static IntPtr sk_fontstyleset_create_typeface (IntPtr fss, Int32 index) =>
			(sk_fontstyleset_create_typeface_delegate ??= GetSymbol<Delegates.sk_fontstyleset_create_typeface> ("sk_fontstyleset_create_typeface")).Invoke (fss, index);
		#endif

		// int sk_fontstyleset_get_count(sk_fontstyleset_t* fss)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_fontstyleset_get_count (IntPtr fss);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_fontstyleset_get_count (IntPtr fss);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_fontstyleset_get_count (IntPtr fss);
		}
		private static Delegates.sk_fontstyleset_get_count sk_fontstyleset_get_count_delegate;
		internal static Int32 sk_fontstyleset_get_count (IntPtr fss) =>
			(sk_fontstyleset_get_count_delegate ??= GetSymbol<Delegates.sk_fontstyleset_get_count> ("sk_fontstyleset_get_count")).Invoke (fss);
		#endif

		// void sk_fontstyleset_get_style(sk_fontstyleset_t* fss, int index, sk_fontstyle_t* fs, sk_string_t* style)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_fontstyleset_get_style (IntPtr fss, Int32 index, IntPtr fs, IntPtr style);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyleset_get_style (IntPtr fss, Int32 index, IntPtr fs, IntPtr style);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontstyleset_get_style (IntPtr fss, Int32 index, IntPtr fs, IntPtr style);
		}
		private static Delegates.sk_fontstyleset_get_style sk_fontstyleset_get_style_delegate;
		internal static void sk_fontstyleset_get_style (IntPtr fss, Int32 index, IntPtr fs, IntPtr style) =>
			(sk_fontstyleset_get_style_delegate ??= GetSymbol<Delegates.sk_fontstyleset_get_style> ("sk_fontstyleset_get_style")).Invoke (fss, index, fs, style);
		#endif

		// sk_typeface_t* sk_fontstyleset_match_style(sk_fontstyleset_t* fss, sk_fontstyle_t* style)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_fontstyleset_match_style (IntPtr fss, IntPtr style);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_fontstyleset_match_style (IntPtr fss, IntPtr style);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_fontstyleset_match_style (IntPtr fss, IntPtr style);
		}
		private static Delegates.sk_fontstyleset_match_style sk_fontstyleset_match_style_delegate;
		internal static IntPtr sk_fontstyleset_match_style (IntPtr fss, IntPtr style) =>
			(sk_fontstyleset_match_style_delegate ??= GetSymbol<Delegates.sk_fontstyleset_match_style> ("sk_fontstyleset_match_style")).Invoke (fss, style);
		#endif

		// void sk_fontstyleset_unref(sk_fontstyleset_t* fss)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_fontstyleset_unref (IntPtr fss);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_fontstyleset_unref (IntPtr fss);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_fontstyleset_unref (IntPtr fss);
		}
		private static Delegates.sk_fontstyleset_unref sk_fontstyleset_unref_delegate;
		internal static void sk_fontstyleset_unref (IntPtr fss) =>
			(sk_fontstyleset_unref_delegate ??= GetSymbol<Delegates.sk_fontstyleset_unref> ("sk_fontstyleset_unref")).Invoke (fss);
		#endif

		// sk_typeface_t* sk_typeface_clone_with_arguments(const sk_typeface_t* typeface, const sk_fontarguments_variation_position_coordinate_t* coordinates, int coordinateCount, int collectionIndex, int paletteIndex, const sk_fontarguments_palette_override_t* paletteOverrides, int paletteOverrideCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_clone_with_arguments (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount, Int32 collectionIndex, Int32 paletteIndex, SKFontPaletteOverride* paletteOverrides, Int32 paletteOverrideCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_clone_with_arguments (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount, Int32 collectionIndex, Int32 paletteIndex, SKFontPaletteOverride* paletteOverrides, Int32 paletteOverrideCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_clone_with_arguments (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount, Int32 collectionIndex, Int32 paletteIndex, SKFontPaletteOverride* paletteOverrides, Int32 paletteOverrideCount);
		}
		private static Delegates.sk_typeface_clone_with_arguments sk_typeface_clone_with_arguments_delegate;
		internal static IntPtr sk_typeface_clone_with_arguments (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount, Int32 collectionIndex, Int32 paletteIndex, SKFontPaletteOverride* paletteOverrides, Int32 paletteOverrideCount) =>
			(sk_typeface_clone_with_arguments_delegate ??= GetSymbol<Delegates.sk_typeface_clone_with_arguments> ("sk_typeface_clone_with_arguments")).Invoke (typeface, coordinates, coordinateCount, collectionIndex, paletteIndex, paletteOverrides, paletteOverrideCount);
		#endif

		// sk_data_t* sk_typeface_copy_table_data(const sk_typeface_t* typeface, sk_font_table_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_copy_table_data (IntPtr typeface, UInt32 tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_copy_table_data (IntPtr typeface, UInt32 tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_copy_table_data (IntPtr typeface, UInt32 tag);
		}
		private static Delegates.sk_typeface_copy_table_data sk_typeface_copy_table_data_delegate;
		internal static IntPtr sk_typeface_copy_table_data (IntPtr typeface, UInt32 tag) =>
			(sk_typeface_copy_table_data_delegate ??= GetSymbol<Delegates.sk_typeface_copy_table_data> ("sk_typeface_copy_table_data")).Invoke (typeface, tag);
		#endif

		// int sk_typeface_count_glyphs(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_count_glyphs (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_count_glyphs (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_count_glyphs (IntPtr typeface);
		}
		private static Delegates.sk_typeface_count_glyphs sk_typeface_count_glyphs_delegate;
		internal static Int32 sk_typeface_count_glyphs (IntPtr typeface) =>
			(sk_typeface_count_glyphs_delegate ??= GetSymbol<Delegates.sk_typeface_count_glyphs> ("sk_typeface_count_glyphs")).Invoke (typeface);
		#endif

		// int sk_typeface_count_tables(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_count_tables (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_count_tables (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_count_tables (IntPtr typeface);
		}
		private static Delegates.sk_typeface_count_tables sk_typeface_count_tables_delegate;
		internal static Int32 sk_typeface_count_tables (IntPtr typeface) =>
			(sk_typeface_count_tables_delegate ??= GetSymbol<Delegates.sk_typeface_count_tables> ("sk_typeface_count_tables")).Invoke (typeface);
		#endif

		// sk_typeface_t* sk_typeface_create_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_create_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_create_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_create_empty ();
		}
		private static Delegates.sk_typeface_create_empty sk_typeface_create_empty_delegate;
		internal static IntPtr sk_typeface_create_empty () =>
			(sk_typeface_create_empty_delegate ??= GetSymbol<Delegates.sk_typeface_create_empty> ("sk_typeface_create_empty")).Invoke ();
		#endif

		// sk_string_t* sk_typeface_get_family_name(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_get_family_name (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_get_family_name (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_get_family_name (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_family_name sk_typeface_get_family_name_delegate;
		internal static IntPtr sk_typeface_get_family_name (IntPtr typeface) =>
			(sk_typeface_get_family_name_delegate ??= GetSymbol<Delegates.sk_typeface_get_family_name> ("sk_typeface_get_family_name")).Invoke (typeface);
		#endif

		// sk_font_style_slant_t sk_typeface_get_font_slant(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial SKFontStyleSlant sk_typeface_get_font_slant (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKFontStyleSlant sk_typeface_get_font_slant (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKFontStyleSlant sk_typeface_get_font_slant (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_font_slant sk_typeface_get_font_slant_delegate;
		internal static SKFontStyleSlant sk_typeface_get_font_slant (IntPtr typeface) =>
			(sk_typeface_get_font_slant_delegate ??= GetSymbol<Delegates.sk_typeface_get_font_slant> ("sk_typeface_get_font_slant")).Invoke (typeface);
		#endif

		// int sk_typeface_get_font_weight(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_get_font_weight (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_font_weight (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_font_weight (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_font_weight sk_typeface_get_font_weight_delegate;
		internal static Int32 sk_typeface_get_font_weight (IntPtr typeface) =>
			(sk_typeface_get_font_weight_delegate ??= GetSymbol<Delegates.sk_typeface_get_font_weight> ("sk_typeface_get_font_weight")).Invoke (typeface);
		#endif

		// int sk_typeface_get_font_width(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_get_font_width (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_font_width (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_font_width (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_font_width sk_typeface_get_font_width_delegate;
		internal static Int32 sk_typeface_get_font_width (IntPtr typeface) =>
			(sk_typeface_get_font_width_delegate ??= GetSymbol<Delegates.sk_typeface_get_font_width> ("sk_typeface_get_font_width")).Invoke (typeface);
		#endif

		// sk_fontstyle_t* sk_typeface_get_fontstyle(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_get_fontstyle (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_get_fontstyle (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_get_fontstyle (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_fontstyle sk_typeface_get_fontstyle_delegate;
		internal static IntPtr sk_typeface_get_fontstyle (IntPtr typeface) =>
			(sk_typeface_get_fontstyle_delegate ??= GetSymbol<Delegates.sk_typeface_get_fontstyle> ("sk_typeface_get_fontstyle")).Invoke (typeface);
		#endif

		// bool sk_typeface_get_kerning_pair_adjustments(const sk_typeface_t* typeface, const uint16_t[-1] glyphs, int count, int32_t[-1] adjustments)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_typeface_get_kerning_pair_adjustments (IntPtr typeface, UInt16* glyphs, Int32 count, Int32* adjustments);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_typeface_get_kerning_pair_adjustments (IntPtr typeface, UInt16* glyphs, Int32 count, Int32* adjustments);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_typeface_get_kerning_pair_adjustments (IntPtr typeface, UInt16* glyphs, Int32 count, Int32* adjustments);
		}
		private static Delegates.sk_typeface_get_kerning_pair_adjustments sk_typeface_get_kerning_pair_adjustments_delegate;
		internal static bool sk_typeface_get_kerning_pair_adjustments (IntPtr typeface, UInt16* glyphs, Int32 count, Int32* adjustments) =>
			(sk_typeface_get_kerning_pair_adjustments_delegate ??= GetSymbol<Delegates.sk_typeface_get_kerning_pair_adjustments> ("sk_typeface_get_kerning_pair_adjustments")).Invoke (typeface, glyphs, count, adjustments);
		#endif

		// sk_string_t* sk_typeface_get_post_script_name(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_get_post_script_name (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_get_post_script_name (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_get_post_script_name (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_post_script_name sk_typeface_get_post_script_name_delegate;
		internal static IntPtr sk_typeface_get_post_script_name (IntPtr typeface) =>
			(sk_typeface_get_post_script_name_delegate ??= GetSymbol<Delegates.sk_typeface_get_post_script_name> ("sk_typeface_get_post_script_name")).Invoke (typeface);
		#endif

		// size_t sk_typeface_get_table_data(const sk_typeface_t* typeface, sk_font_table_tag_t tag, size_t offset, size_t length, void* data)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_typeface_get_table_data (IntPtr typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_typeface_get_table_data (IntPtr typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_typeface_get_table_data (IntPtr typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data);
		}
		private static Delegates.sk_typeface_get_table_data sk_typeface_get_table_data_delegate;
		internal static /* size_t */ IntPtr sk_typeface_get_table_data (IntPtr typeface, UInt32 tag, /* size_t */ IntPtr offset, /* size_t */ IntPtr length, void* data) =>
			(sk_typeface_get_table_data_delegate ??= GetSymbol<Delegates.sk_typeface_get_table_data> ("sk_typeface_get_table_data")).Invoke (typeface, tag, offset, length, data);
		#endif

		// size_t sk_typeface_get_table_size(const sk_typeface_t* typeface, sk_font_table_tag_t tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_typeface_get_table_size (IntPtr typeface, UInt32 tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_typeface_get_table_size (IntPtr typeface, UInt32 tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_typeface_get_table_size (IntPtr typeface, UInt32 tag);
		}
		private static Delegates.sk_typeface_get_table_size sk_typeface_get_table_size_delegate;
		internal static /* size_t */ IntPtr sk_typeface_get_table_size (IntPtr typeface, UInt32 tag) =>
			(sk_typeface_get_table_size_delegate ??= GetSymbol<Delegates.sk_typeface_get_table_size> ("sk_typeface_get_table_size")).Invoke (typeface, tag);
		#endif

		// int sk_typeface_get_table_tags(const sk_typeface_t* typeface, sk_font_table_tag_t[-1] tags)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_get_table_tags (IntPtr typeface, UInt32* tags);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_table_tags (IntPtr typeface, UInt32* tags);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_table_tags (IntPtr typeface, UInt32* tags);
		}
		private static Delegates.sk_typeface_get_table_tags sk_typeface_get_table_tags_delegate;
		internal static Int32 sk_typeface_get_table_tags (IntPtr typeface, UInt32* tags) =>
			(sk_typeface_get_table_tags_delegate ??= GetSymbol<Delegates.sk_typeface_get_table_tags> ("sk_typeface_get_table_tags")).Invoke (typeface, tags);
		#endif

		// int sk_typeface_get_units_per_em(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_get_units_per_em (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_units_per_em (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_units_per_em (IntPtr typeface);
		}
		private static Delegates.sk_typeface_get_units_per_em sk_typeface_get_units_per_em_delegate;
		internal static Int32 sk_typeface_get_units_per_em (IntPtr typeface) =>
			(sk_typeface_get_units_per_em_delegate ??= GetSymbol<Delegates.sk_typeface_get_units_per_em> ("sk_typeface_get_units_per_em")).Invoke (typeface);
		#endif

		// int sk_typeface_get_variation_design_parameters(const sk_typeface_t* typeface, sk_fontarguments_variation_axis_t* parameters, int parameterCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_get_variation_design_parameters (IntPtr typeface, SKFontVariationAxis* parameters, Int32 parameterCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_variation_design_parameters (IntPtr typeface, SKFontVariationAxis* parameters, Int32 parameterCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_variation_design_parameters (IntPtr typeface, SKFontVariationAxis* parameters, Int32 parameterCount);
		}
		private static Delegates.sk_typeface_get_variation_design_parameters sk_typeface_get_variation_design_parameters_delegate;
		internal static Int32 sk_typeface_get_variation_design_parameters (IntPtr typeface, SKFontVariationAxis* parameters, Int32 parameterCount) =>
			(sk_typeface_get_variation_design_parameters_delegate ??= GetSymbol<Delegates.sk_typeface_get_variation_design_parameters> ("sk_typeface_get_variation_design_parameters")).Invoke (typeface, parameters, parameterCount);
		#endif

		// int sk_typeface_get_variation_design_position(const sk_typeface_t* typeface, sk_fontarguments_variation_position_coordinate_t* coordinates, int coordinateCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_typeface_get_variation_design_position (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_typeface_get_variation_design_position (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_typeface_get_variation_design_position (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount);
		}
		private static Delegates.sk_typeface_get_variation_design_position sk_typeface_get_variation_design_position_delegate;
		internal static Int32 sk_typeface_get_variation_design_position (IntPtr typeface, SKFontVariationPositionCoordinate* coordinates, Int32 coordinateCount) =>
			(sk_typeface_get_variation_design_position_delegate ??= GetSymbol<Delegates.sk_typeface_get_variation_design_position> ("sk_typeface_get_variation_design_position")).Invoke (typeface, coordinates, coordinateCount);
		#endif

		// bool sk_typeface_is_fixed_pitch(const sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_typeface_is_fixed_pitch (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_typeface_is_fixed_pitch (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_typeface_is_fixed_pitch (IntPtr typeface);
		}
		private static Delegates.sk_typeface_is_fixed_pitch sk_typeface_is_fixed_pitch_delegate;
		internal static bool sk_typeface_is_fixed_pitch (IntPtr typeface) =>
			(sk_typeface_is_fixed_pitch_delegate ??= GetSymbol<Delegates.sk_typeface_is_fixed_pitch> ("sk_typeface_is_fixed_pitch")).Invoke (typeface);
		#endif

		// sk_stream_asset_t* sk_typeface_open_stream(const sk_typeface_t* typeface, int* ttcIndex)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_typeface_open_stream (IntPtr typeface, Int32* ttcIndex);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_typeface_open_stream (IntPtr typeface, Int32* ttcIndex);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_typeface_open_stream (IntPtr typeface, Int32* ttcIndex);
		}
		private static Delegates.sk_typeface_open_stream sk_typeface_open_stream_delegate;
		internal static IntPtr sk_typeface_open_stream (IntPtr typeface, Int32* ttcIndex) =>
			(sk_typeface_open_stream_delegate ??= GetSymbol<Delegates.sk_typeface_open_stream> ("sk_typeface_open_stream")).Invoke (typeface, ttcIndex);
		#endif

		// uint16_t sk_typeface_unichar_to_glyph(const sk_typeface_t* typeface, const int32_t unichar)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial UInt16 sk_typeface_unichar_to_glyph (IntPtr typeface, Int32 unichar);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern UInt16 sk_typeface_unichar_to_glyph (IntPtr typeface, Int32 unichar);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate UInt16 sk_typeface_unichar_to_glyph (IntPtr typeface, Int32 unichar);
		}
		private static Delegates.sk_typeface_unichar_to_glyph sk_typeface_unichar_to_glyph_delegate;
		internal static UInt16 sk_typeface_unichar_to_glyph (IntPtr typeface, Int32 unichar) =>
			(sk_typeface_unichar_to_glyph_delegate ??= GetSymbol<Delegates.sk_typeface_unichar_to_glyph> ("sk_typeface_unichar_to_glyph")).Invoke (typeface, unichar);
		#endif

		// void sk_typeface_unichars_to_glyphs(const sk_typeface_t* typeface, const int32_t[-1] unichars, int count, uint16_t[-1] glyphs)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_typeface_unichars_to_glyphs (IntPtr typeface, Int32* unichars, Int32 count, UInt16* glyphs);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_typeface_unichars_to_glyphs (IntPtr typeface, Int32* unichars, Int32 count, UInt16* glyphs);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_typeface_unichars_to_glyphs (IntPtr typeface, Int32* unichars, Int32 count, UInt16* glyphs);
		}
		private static Delegates.sk_typeface_unichars_to_glyphs sk_typeface_unichars_to_glyphs_delegate;
		internal static void sk_typeface_unichars_to_glyphs (IntPtr typeface, Int32* unichars, Int32 count, UInt16* glyphs) =>
			(sk_typeface_unichars_to_glyphs_delegate ??= GetSymbol<Delegates.sk_typeface_unichars_to_glyphs> ("sk_typeface_unichars_to_glyphs")).Invoke (typeface, unichars, count, glyphs);
		#endif

		// void sk_typeface_unref(sk_typeface_t* typeface)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_typeface_unref (IntPtr typeface);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_typeface_unref (IntPtr typeface);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_typeface_unref (IntPtr typeface);
		}
		private static Delegates.sk_typeface_unref sk_typeface_unref_delegate;
		internal static void sk_typeface_unref (IntPtr typeface) =>
			(sk_typeface_unref_delegate ??= GetSymbol<Delegates.sk_typeface_unref> ("sk_typeface_unref")).Invoke (typeface);
		#endif

		#endregion

	}
}
