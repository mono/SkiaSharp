using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_from_name_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, /* char */ void*, Int32, UInt32*, void*, bool> FontGetGlyphFromNameProxy = &FontGetGlyphFromNameProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphFromNameProxyDelegate FontGetGlyphFromNameProxy = FontGetGlyphFromNameProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphFromNameProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetGlyphFromNameProxyImplementation(IntPtr font,void* font_data,/* char */ void* name,Int32 len,UInt32* glyph,void* user_data);

	}
}
