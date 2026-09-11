using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_colorfilter.h

		// sk_colorfilter_t* sk_colorfilter_new_color_matrix(const float[20] array = 20)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array);
		}
		private static Delegates.sk_colorfilter_new_color_matrix sk_colorfilter_new_color_matrix_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_color_matrix (Single* array) =>
			(sk_colorfilter_new_color_matrix_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_color_matrix> ("sk_colorfilter_new_color_matrix")).Invoke (array);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_compose(sk_colorfilter_t* outer, sk_colorfilter_t* inner)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner);
		}
		private static Delegates.sk_colorfilter_new_compose sk_colorfilter_new_compose_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_compose (sk_colorfilter_t outer, sk_colorfilter_t inner) =>
			(sk_colorfilter_new_compose_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_compose> ("sk_colorfilter_new_compose")).Invoke (outer, inner);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_high_contrast(const sk_highcontrastconfig_t* config)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config);
		}
		private static Delegates.sk_colorfilter_new_high_contrast sk_colorfilter_new_high_contrast_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_high_contrast (SKHighContrastConfig* config) =>
			(sk_colorfilter_new_high_contrast_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_high_contrast> ("sk_colorfilter_new_high_contrast")).Invoke (config);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_hsla_matrix(const float[20] array = 20)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_hsla_matrix (Single* array);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_hsla_matrix (Single* array);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_hsla_matrix (Single* array);
		}
		private static Delegates.sk_colorfilter_new_hsla_matrix sk_colorfilter_new_hsla_matrix_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_hsla_matrix (Single* array) =>
			(sk_colorfilter_new_hsla_matrix_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_hsla_matrix> ("sk_colorfilter_new_hsla_matrix")).Invoke (array);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_lerp(float weight, sk_colorfilter_t* filter0, sk_colorfilter_t* filter1)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_lerp (Single weight, sk_colorfilter_t filter0, sk_colorfilter_t filter1);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_lerp (Single weight, sk_colorfilter_t filter0, sk_colorfilter_t filter1);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_lerp (Single weight, sk_colorfilter_t filter0, sk_colorfilter_t filter1);
		}
		private static Delegates.sk_colorfilter_new_lerp sk_colorfilter_new_lerp_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_lerp (Single weight, sk_colorfilter_t filter0, sk_colorfilter_t filter1) =>
			(sk_colorfilter_new_lerp_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_lerp> ("sk_colorfilter_new_lerp")).Invoke (weight, filter0, filter1);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_lighting(sk_color_t mul, sk_color_t add)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add);
		}
		private static Delegates.sk_colorfilter_new_lighting sk_colorfilter_new_lighting_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_lighting (UInt32 mul, UInt32 add) =>
			(sk_colorfilter_new_lighting_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_lighting> ("sk_colorfilter_new_lighting")).Invoke (mul, add);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_linear_to_srgb_gamma()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_linear_to_srgb_gamma ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_linear_to_srgb_gamma ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_linear_to_srgb_gamma ();
		}
		private static Delegates.sk_colorfilter_new_linear_to_srgb_gamma sk_colorfilter_new_linear_to_srgb_gamma_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_linear_to_srgb_gamma () =>
			(sk_colorfilter_new_linear_to_srgb_gamma_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_linear_to_srgb_gamma> ("sk_colorfilter_new_linear_to_srgb_gamma")).Invoke ();
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_luma_color()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_luma_color ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_luma_color ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_luma_color ();
		}
		private static Delegates.sk_colorfilter_new_luma_color sk_colorfilter_new_luma_color_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_luma_color () =>
			(sk_colorfilter_new_luma_color_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_luma_color> ("sk_colorfilter_new_luma_color")).Invoke ();
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_mode(sk_color_t c, sk_blendmode_t mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode);
		}
		private static Delegates.sk_colorfilter_new_mode sk_colorfilter_new_mode_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_mode (UInt32 c, SKBlendMode mode) =>
			(sk_colorfilter_new_mode_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_mode> ("sk_colorfilter_new_mode")).Invoke (c, mode);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_overdraw(const sk_color_t[6] colors = 6)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_overdraw (UInt32* colors);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_overdraw (UInt32* colors);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_overdraw (UInt32* colors);
		}
		private static Delegates.sk_colorfilter_new_overdraw sk_colorfilter_new_overdraw_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_overdraw (UInt32* colors) =>
			(sk_colorfilter_new_overdraw_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_overdraw> ("sk_colorfilter_new_overdraw")).Invoke (colors);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_srgb_to_linear_gamma()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_srgb_to_linear_gamma ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_srgb_to_linear_gamma ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_srgb_to_linear_gamma ();
		}
		private static Delegates.sk_colorfilter_new_srgb_to_linear_gamma sk_colorfilter_new_srgb_to_linear_gamma_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_srgb_to_linear_gamma () =>
			(sk_colorfilter_new_srgb_to_linear_gamma_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_srgb_to_linear_gamma> ("sk_colorfilter_new_srgb_to_linear_gamma")).Invoke ();
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_table(const uint8_t[256] table = 256)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_table (Byte* table);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_table (Byte* table);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_table (Byte* table);
		}
		private static Delegates.sk_colorfilter_new_table sk_colorfilter_new_table_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_table (Byte* table) =>
			(sk_colorfilter_new_table_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_table> ("sk_colorfilter_new_table")).Invoke (table);
		#endif

		// sk_colorfilter_t* sk_colorfilter_new_table_argb(const uint8_t[256] tableA = 256, const uint8_t[256] tableR = 256, const uint8_t[256] tableG = 256, const uint8_t[256] tableB = 256)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB);
		}
		private static Delegates.sk_colorfilter_new_table_argb sk_colorfilter_new_table_argb_delegate;
		internal static sk_colorfilter_t sk_colorfilter_new_table_argb (Byte* tableA, Byte* tableR, Byte* tableG, Byte* tableB) =>
			(sk_colorfilter_new_table_argb_delegate ??= GetSymbol<Delegates.sk_colorfilter_new_table_argb> ("sk_colorfilter_new_table_argb")).Invoke (tableA, tableR, tableG, tableB);
		#endif

		// void sk_colorfilter_unref(sk_colorfilter_t* filter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_colorfilter_unref (sk_colorfilter_t filter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_colorfilter_unref (sk_colorfilter_t filter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_colorfilter_unref (sk_colorfilter_t filter);
		}
		private static Delegates.sk_colorfilter_unref sk_colorfilter_unref_delegate;
		internal static void sk_colorfilter_unref (sk_colorfilter_t filter) =>
			(sk_colorfilter_unref_delegate ??= GetSymbol<Delegates.sk_colorfilter_unref> ("sk_colorfilter_unref")).Invoke (filter);
		#endif

		#endregion

	}
}
