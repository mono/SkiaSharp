using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal unsafe partial class HarfBuzzApi
	{
		#region hb-ot-font.h

		// extern void hb_ot_font_set_funcs(hb_font_t* font)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (HARFBUZZ)]
		internal static partial void hb_ot_font_set_funcs (hb_font_t font);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (HARFBUZZ, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void hb_ot_font_set_funcs (hb_font_t font);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void hb_ot_font_set_funcs (hb_font_t font);
		}
		private static Delegates.hb_ot_font_set_funcs hb_ot_font_set_funcs_delegate;
		internal static void hb_ot_font_set_funcs (hb_font_t font) =>
			(hb_ot_font_set_funcs_delegate ??= GetSymbol<Delegates.hb_ot_font_set_funcs> ("hb_ot_font_set_funcs")).Invoke (font);
		#endif

		#endregion

	}
}
