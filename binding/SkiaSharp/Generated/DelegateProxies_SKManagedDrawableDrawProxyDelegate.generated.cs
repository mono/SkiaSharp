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
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, IntPtr, void> SKManagedDrawableDrawProxy = &SKManagedDrawableDrawProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedDrawableDrawProxyDelegate SKManagedDrawableDrawProxy = SKManagedDrawableDrawProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedDrawableDrawProxyDelegate))]
#endif
	private static partial void SKManagedDrawableDrawProxyImplementation(IntPtr d,void* context,IntPtr ccanvas);

	}
}
