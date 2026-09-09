using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-shape.h

		// extern void hb_shape(hb_font_t* font, hb_buffer_t* buffer, const hb_feature_t* features, unsigned int num_features)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_shape (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_shape (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_shape (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features);
		}
		private static Delegates.hb_shape hb_shape_delegate;
		internal static void hb_shape (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features) =>
			(hb_shape_delegate ??= GetSymbol<Delegates.hb_shape> ("hb_shape")).Invoke (font, buffer, features, num_features);
		#endif

		// extern hb_bool_t hb_shape_full(hb_font_t* font, hb_buffer_t* buffer, const hb_feature_t* features, unsigned int num_features, const char* const* shaper_list)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_shape_full (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_shape_full (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_shape_full (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list);
		}
		private static Delegates.hb_shape_full hb_shape_full_delegate;
		internal static bool hb_shape_full (IntPtr font, IntPtr buffer, Feature* features, UInt32 num_features, /* char */ void** shaper_list) =>
			(hb_shape_full_delegate ??= GetSymbol<Delegates.hb_shape_full> ("hb_shape_full")).Invoke (font, buffer, features, num_features, shaper_list);
		#endif

		// extern const char** hb_shape_list_shapers()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial /* char */ void** hb_shape_list_shapers ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* char */ void** hb_shape_list_shapers ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* char */ void** hb_shape_list_shapers ();
		}
		private static Delegates.hb_shape_list_shapers hb_shape_list_shapers_delegate;
		internal static /* char */ void** hb_shape_list_shapers () =>
			(hb_shape_list_shapers_delegate ??= GetSymbol<Delegates.hb_shape_list_shapers> ("hb_shape_list_shapers")).Invoke ();
		#endif

		#endregion

	}
}
