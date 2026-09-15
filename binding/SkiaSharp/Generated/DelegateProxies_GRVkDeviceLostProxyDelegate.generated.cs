using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for gr_vk_device_lost_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, GRVkDeviceLostInfoNative*, void> GRVkDeviceLostProxy = &GRVkDeviceLostProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly GRVkDeviceLostProxyDelegate GRVkDeviceLostProxy = GRVkDeviceLostProxyImplementation;
	[MonoPInvokeCallback (typeof (GRVkDeviceLostProxyDelegate))]
#endif
	private static partial void GRVkDeviceLostProxyImplementation(void* userData,GRVkDeviceLostInfoNative* info);

	}
}
