using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_manageddrawable_destroy_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, void> SKManagedDrawableDestroyProxy = &SKManagedDrawableDestroyProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedDrawableDestroyProxyDelegate SKManagedDrawableDestroyProxy = SKManagedDrawableDestroyProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedDrawableDestroyProxyDelegate))]
#endif
	private static partial void SKManagedDrawableDestroyProxyImplementation(IntPtr d,void* context);

	}
}
