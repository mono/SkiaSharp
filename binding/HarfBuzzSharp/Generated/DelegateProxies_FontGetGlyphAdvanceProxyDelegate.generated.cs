using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_advance_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_font_t, void*, UInt32, void*, Int32> FontGetGlyphAdvanceProxy = &FontGetGlyphAdvanceProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphAdvanceProxyDelegate FontGetGlyphAdvanceProxy = FontGetGlyphAdvanceProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphAdvanceProxyDelegate))]
#endif
	private static partial Int32 FontGetGlyphAdvanceProxyImplementation(hb_font_t font,void* font_data,UInt32 glyph,void* user_data);

	}
}
