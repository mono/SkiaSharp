using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_getPosition_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_stream_managedstream_t, void*, /* size_t */ IntPtr> SKManagedStreamGetPositionProxy = &SKManagedStreamGetPositionProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamGetPositionProxyDelegate SKManagedStreamGetPositionProxy = SKManagedStreamGetPositionProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamGetPositionProxyDelegate))]
#endif
	private static partial /* size_t */ IntPtr SKManagedStreamGetPositionProxyImplementation(sk_stream_managedstream_t s,void* context);

	}
}
