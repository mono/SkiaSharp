using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_fork_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_stream_managedstream_t, void*, sk_stream_managedstream_t> SKManagedStreamForkProxy = &SKManagedStreamForkProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamForkProxyDelegate SKManagedStreamForkProxy = SKManagedStreamForkProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamForkProxyDelegate))]
#endif
	private static partial sk_stream_managedstream_t SKManagedStreamForkProxyImplementation(sk_stream_managedstream_t s,void* context);

	}
}
