using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_glyph_contour_point_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, UInt32, UInt32, Int32*, Int32*, void*, bool> FontGetGlyphContourPointProxy = &FontGetGlyphContourPointProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetGlyphContourPointProxyDelegate FontGetGlyphContourPointProxy = FontGetGlyphContourPointProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetGlyphContourPointProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetGlyphContourPointProxyImplementation(IntPtr font,void* font_data,UInt32 glyph,UInt32 point_index,Int32* x,Int32* y,void* user_data);

	}
}
