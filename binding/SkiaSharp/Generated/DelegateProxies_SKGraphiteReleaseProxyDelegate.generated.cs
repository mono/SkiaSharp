using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_graphite_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void> SKGraphiteReleaseProxy = &SKGraphiteReleaseProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGraphiteReleaseProxyDelegate SKGraphiteReleaseProxy = SKGraphiteReleaseProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGraphiteReleaseProxyDelegate))]
#endif
	private static partial void SKGraphiteReleaseProxyImplementation(void* releaseContext);

	}
}
