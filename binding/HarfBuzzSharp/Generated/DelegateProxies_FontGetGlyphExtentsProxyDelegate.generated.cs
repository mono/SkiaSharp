using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_extents_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, UInt32, GlyphExtents*, void*, bool> FontGetGlyphExtentsProxy = &FontGetGlyphExtentsProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphExtentsProxyDelegate FontGetGlyphExtentsProxy = FontGetGlyphExtentsProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphExtentsProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetGlyphExtentsProxyImplementation(IntPtr font,void* font_data,UInt32 glyph,GlyphExtents* extents,void* user_data);

	}
}
