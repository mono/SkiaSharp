using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_advances_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, UInt32, UInt32*, UInt32, Int32*, UInt32, void*, void> FontGetGlyphAdvancesProxy = &FontGetGlyphAdvancesProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphAdvancesProxyDelegate FontGetGlyphAdvancesProxy = FontGetGlyphAdvancesProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphAdvancesProxyDelegate))]
#endif
	private static partial void FontGetGlyphAdvancesProxyImplementation(IntPtr font,void* font_data,UInt32 count,UInt32* first_glyph,UInt32 glyph_stride,Int32* first_advance,UInt32 advance_stride,void* user_data);

	}
}
