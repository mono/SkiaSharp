using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_position_t (*)(hb_font_t* font, void* font_data, hb_codepoint_t first_glyph, hb_codepoint_t second_glyph, void* user_data)* hb_font_get_glyph_kerning_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal unsafe delegate Int32 FontGetGlyphKerningProxyDelegate(IntPtr font, void* font_data, UInt32 first_glyph, UInt32 second_glyph, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT
