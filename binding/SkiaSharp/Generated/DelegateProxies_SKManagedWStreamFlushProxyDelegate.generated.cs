using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedwstream_flush_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, void> SKManagedWStreamFlushProxy = &SKManagedWStreamFlushProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedWStreamFlushProxyDelegate SKManagedWStreamFlushProxy = SKManagedWStreamFlushProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedWStreamFlushProxyDelegate))]
#endif
	private static partial void SKManagedWStreamFlushProxyImplementation(sk_wstream_managedstream_t s,void* context);

	}
}
