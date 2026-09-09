using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_glyph_path_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, SKMatrix*, void*, void> SKGlyphPathProxy = &SKGlyphPathProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGlyphPathProxyDelegate SKGlyphPathProxy = SKGlyphPathProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGlyphPathProxyDelegate))]
#endif
	private static partial void SKGlyphPathProxyImplementation(IntPtr pathOrNull,SKMatrix* matrix,void* context);

	}
}
