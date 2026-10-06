using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_data_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKDataReleaseProxy = &SKDataReleaseProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKDataReleaseProxyDelegate SKDataReleaseProxy = SKDataReleaseProxyImplementation;
	[MonoPInvokeCallback (typeof (SKDataReleaseProxyDelegate))]
#endif
	private static partial void SKDataReleaseProxyImplementation(void* ptr,void* context);

	}
}
