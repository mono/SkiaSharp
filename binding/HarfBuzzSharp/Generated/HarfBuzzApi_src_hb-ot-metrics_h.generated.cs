using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-metrics.h

		// extern hb_bool_t hb_ot_metrics_get_position(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag, hb_position_t* position)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool hb_ot_metrics_get_position (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool hb_ot_metrics_get_position (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool hb_ot_metrics_get_position (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position);
		}
		private static Delegates.hb_ot_metrics_get_position hb_ot_metrics_get_position_delegate;
		internal static bool hb_ot_metrics_get_position (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position) =>
			(hb_ot_metrics_get_position_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_position> ("hb_ot_metrics_get_position")).Invoke (font, metrics_tag, position);
		#endif

		// extern void hb_ot_metrics_get_position_with_fallback(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag, hb_position_t* position)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_metrics_get_position_with_fallback (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_metrics_get_position_with_fallback (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_metrics_get_position_with_fallback (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position);
		}
		private static Delegates.hb_ot_metrics_get_position_with_fallback hb_ot_metrics_get_position_with_fallback_delegate;
		internal static void hb_ot_metrics_get_position_with_fallback (IntPtr font, OpenTypeMetricsTag metrics_tag, Int32* position) =>
			(hb_ot_metrics_get_position_with_fallback_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_position_with_fallback> ("hb_ot_metrics_get_position_with_fallback")).Invoke (font, metrics_tag, position);
		#endif

		// extern float hb_ot_metrics_get_variation(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Single hb_ot_metrics_get_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Single hb_ot_metrics_get_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Single hb_ot_metrics_get_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		}
		private static Delegates.hb_ot_metrics_get_variation hb_ot_metrics_get_variation_delegate;
		internal static Single hb_ot_metrics_get_variation (IntPtr font, OpenTypeMetricsTag metrics_tag) =>
			(hb_ot_metrics_get_variation_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_variation> ("hb_ot_metrics_get_variation")).Invoke (font, metrics_tag);
		#endif

		// extern hb_position_t hb_ot_metrics_get_x_variation(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_metrics_get_x_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_metrics_get_x_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_metrics_get_x_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		}
		private static Delegates.hb_ot_metrics_get_x_variation hb_ot_metrics_get_x_variation_delegate;
		internal static Int32 hb_ot_metrics_get_x_variation (IntPtr font, OpenTypeMetricsTag metrics_tag) =>
			(hb_ot_metrics_get_x_variation_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_x_variation> ("hb_ot_metrics_get_x_variation")).Invoke (font, metrics_tag);
		#endif

		// extern hb_position_t hb_ot_metrics_get_y_variation(hb_font_t* font, hb_ot_metrics_tag_t metrics_tag)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial Int32 hb_ot_metrics_get_y_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 hb_ot_metrics_get_y_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 hb_ot_metrics_get_y_variation (IntPtr font, OpenTypeMetricsTag metrics_tag);
		}
		private static Delegates.hb_ot_metrics_get_y_variation hb_ot_metrics_get_y_variation_delegate;
		internal static Int32 hb_ot_metrics_get_y_variation (IntPtr font, OpenTypeMetricsTag metrics_tag) =>
			(hb_ot_metrics_get_y_variation_delegate ??= GetSymbol<Delegates.hb_ot_metrics_get_y_variation> ("hb_ot_metrics_get_y_variation")).Invoke (font, metrics_tag);
		#endif

		#endregion

	}
}
