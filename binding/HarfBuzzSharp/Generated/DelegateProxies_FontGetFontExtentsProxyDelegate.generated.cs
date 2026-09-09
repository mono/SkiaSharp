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
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, FontExtents*, void*, bool> FontGetFontExtentsProxy = &FontGetFontExtentsProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly FontGetFontExtentsProxyDelegate FontGetFontExtentsProxy = FontGetFontExtentsProxyImplementation;
	[MonoPInvokeCallback (typeof (FontGetFontExtentsProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool FontGetFontExtentsProxyImplementation(IntPtr font,void* font_data,FontExtents* extents,void* user_data);

	}
}
