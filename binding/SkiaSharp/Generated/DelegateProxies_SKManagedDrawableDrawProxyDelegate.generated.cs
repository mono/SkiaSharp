using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_manageddrawable_draw_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_manageddrawable_t, void*, sk_canvas_t, void> SKManagedDrawableDrawProxy = &SKManagedDrawableDrawProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedDrawableDrawProxyDelegate SKManagedDrawableDrawProxy = SKManagedDrawableDrawProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedDrawableDrawProxyDelegate))]
#endif
	private static partial void SKManagedDrawableDrawProxyImplementation(sk_manageddrawable_t d,void* context,sk_canvas_t ccanvas);

	}
}
