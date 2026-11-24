using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_font_get_font_extents_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_font_t, void*, FontExtents*, void*, Int32> FontGetFontExtentsProxy = &FontGetFontExtentsProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetFontExtentsProxyDelegate FontGetFontExtentsProxy = FontGetFontExtentsProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetFontExtentsProxyDelegate))]
#endif
	private static partial Int32 FontGetFontExtentsProxyImplementation(hb_font_t font,void* font_data,FontExtents* extents,void* user_data);

	}
}
