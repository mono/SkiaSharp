using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t unicode, hb_codepoint_t variation_selector, hb_codepoint_t* glyph, void* user_data)* hb_font_get_glyph_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphProxyDelegate(IntPtr font, void* font_data, UInt32 unicode, UInt32 variation_selector, UInt32* glyph, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT
