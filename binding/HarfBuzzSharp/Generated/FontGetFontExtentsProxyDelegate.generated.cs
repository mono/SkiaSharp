using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

#if !USE_LIBRARY_IMPORT
namespace HarfBuzzSharp
{
	// typedef hb_bool_t (*)(hb_font_t* font, void* font_data, hb_font_extents_t* extents, void* user_data)* hb_font_get_font_extents_func_t
	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	[return: MarshalAs (UnmanagedType.I1)]
	internal unsafe delegate bool FontGetFontExtentsProxyDelegate(hb_font_t font, void* font_data, FontExtents* extents, void* user_data);

}
#endif // !USE_LIBRARY_IMPORT
