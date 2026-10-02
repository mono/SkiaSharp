using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_graphite_image_provider_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, sk_graphite_recorder_t, sk_image_t, bool, sk_image_t> SKGraphiteImageProviderProxy = &SKGraphiteImageProviderProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGraphiteImageProviderProxyDelegate SKGraphiteImageProviderProxy = SKGraphiteImageProviderProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGraphiteImageProviderProxyDelegate))]
#endif
	private static partial sk_image_t SKGraphiteImageProviderProxyImplementation(void* userData,sk_graphite_recorder_t recorder,sk_image_t image,[MarshalAs (UnmanagedType.I1)] bool mipmapped);

	}
}
