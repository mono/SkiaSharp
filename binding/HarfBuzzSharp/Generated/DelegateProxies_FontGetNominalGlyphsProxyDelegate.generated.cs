using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_nominal_glyphs_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_font_t, void*, UInt32, UInt32*, UInt32, UInt32*, UInt32, void*, UInt32> FontGetNominalGlyphsProxy = &FontGetNominalGlyphsProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetNominalGlyphsProxyDelegate FontGetNominalGlyphsProxy = FontGetNominalGlyphsProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetNominalGlyphsProxyDelegate))]
#endif
	private static partial UInt32 FontGetNominalGlyphsProxyImplementation(hb_font_t font,void* font_data,UInt32 count,UInt32* first_unicode,UInt32 unicode_stride,UInt32* first_glyph,UInt32 glyph_stride,void* user_data);

	}
}
