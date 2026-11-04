using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_hasPosition_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_stream_managedstream_t, void*, Byte> SKManagedStreamHasPositionProxy = &SKManagedStreamHasPositionProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamHasPositionProxyDelegate SKManagedStreamHasPositionProxy = SKManagedStreamHasPositionProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamHasPositionProxyDelegate))]
#endif
	private static partial Byte SKManagedStreamHasPositionProxyImplementation(sk_stream_managedstream_t s,void* context);

	}
}
