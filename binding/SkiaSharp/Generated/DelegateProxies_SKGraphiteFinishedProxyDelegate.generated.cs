using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_graphite_finished_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, Byte, void> SKGraphiteFinishedProxy = &SKGraphiteFinishedProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGraphiteFinishedProxyDelegate SKGraphiteFinishedProxy = SKGraphiteFinishedProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGraphiteFinishedProxyDelegate))]
#endif
	private static partial void SKGraphiteFinishedProxyImplementation(void* context,Byte success);

	}
}
