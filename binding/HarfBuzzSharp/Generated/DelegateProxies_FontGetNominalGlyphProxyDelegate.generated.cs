using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_nominal_glyph_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_font_t, void*, UInt32, UInt32*, void*, bool> FontGetNominalGlyphProxy = &FontGetNominalGlyphProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetNominalGlyphProxyDelegate FontGetNominalGlyphProxy = FontGetNominalGlyphProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetNominalGlyphProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetNominalGlyphProxyImplementation(hb_font_t font,void* font_data,UInt32 unicode,UInt32* glyph,void* user_data);

	}
}
