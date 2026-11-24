using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef unsigned int (*)(hb_font_t* font, void* font_data, unsigned int count, const hb_codepoint_t* first_unicode, unsigned int unicode_stride, hb_codepoint_t* first_glyph, unsigned int glyph_stride, void* user_data)* hb_font_get_nominal_glyphs_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate UInt32 FontGetNominalGlyphsProxyDelegate(hb_font_t font, void* font_data, UInt32 count, UInt32* first_unicode, UInt32 unicode_stride, UInt32* first_glyph, UInt32 glyph_stride, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT
