using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef void (*)(hb_font_t* font, void* font_data, unsigned int count, const hb_codepoint_t* first_glyph, unsigned int glyph_stride, hb_position_t* first_advance, unsigned int advance_stride, void* user_data)* hb_font_get_glyph_advances_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate void FontGetGlyphAdvancesProxyDelegate(IntPtr font, void* font_data, UInt32 count, UInt32* first_glyph, UInt32 glyph_stride, Int32* first_advance, UInt32 advance_stride, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT
