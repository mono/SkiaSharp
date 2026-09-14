using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_manageddrawable_getBounds_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_manageddrawable_t, void*, SKRect*, void> SKManagedDrawableGetBoundsProxy = &SKManagedDrawableGetBoundsProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedDrawableGetBoundsProxyDelegate SKManagedDrawableGetBoundsProxy = SKManagedDrawableGetBoundsProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedDrawableGetBoundsProxyDelegate))]
#endif
	private static partial void SKManagedDrawableGetBoundsProxyImplementation(sk_manageddrawable_t d,void* context,SKRect* rect);

	}
}
