using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_kerning_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, UInt32, UInt32, void*, Int32> FontGetGlyphKerningProxy = &FontGetGlyphKerningProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphKerningProxyDelegate FontGetGlyphKerningProxy = FontGetGlyphKerningProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphKerningProxyDelegate))]
#endif
	private static partial Int32 FontGetGlyphKerningProxyImplementation(IntPtr font,void* font_data,UInt32 first_glyph,UInt32 second_glyph,void* user_data);

	}
}
