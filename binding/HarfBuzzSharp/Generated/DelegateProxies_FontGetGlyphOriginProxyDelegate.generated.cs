using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_origin_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_font_t, void*, UInt32, Int32*, Int32*, void*, bool> FontGetGlyphOriginProxy = &FontGetGlyphOriginProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphOriginProxyDelegate FontGetGlyphOriginProxy = FontGetGlyphOriginProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphOriginProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetGlyphOriginProxyImplementation(hb_font_t font,void* font_data,UInt32 glyph,Int32* x,Int32* y,void* user_data);

	}
}
