using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedwstream_destroy_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, void> SKManagedWStreamDestroyProxy = &SKManagedWStreamDestroyProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedWStreamDestroyProxyDelegate SKManagedWStreamDestroyProxy = SKManagedWStreamDestroyProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedWStreamDestroyProxyDelegate))]
#endif
	private static partial void SKManagedWStreamDestroyProxyImplementation(IntPtr s,void* context);

	}
}
