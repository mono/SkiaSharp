using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_name_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, UInt32, /* char */ void*, UInt32, void*, bool> FontGetGlyphNameProxy = &FontGetGlyphNameProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphNameProxyDelegate FontGetGlyphNameProxy = FontGetGlyphNameProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphNameProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetGlyphNameProxyImplementation(IntPtr font,void* font_data,UInt32 glyph,/* char */ void* name,UInt32 size,void* user_data);

	}
}
