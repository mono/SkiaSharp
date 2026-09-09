using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, const char* name, int len, hb_codepoint_t* glyph, void* user_data)* hb_font_get_glyph_from_name_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetGlyphFromNameProxyDelegate(IntPtr font, void* font_data, /* char */ void* name, Int32 len, UInt32* glyph, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT
