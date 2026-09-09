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
	public static readonly delegate* unmanaged[Cdecl] <void*, IntPtr, IntPtr, bool, IntPtr> SKGraphiteImageProviderProxy = &SKGraphiteImageProviderProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGraphiteImageProviderProxyDelegate SKGraphiteImageProviderProxy = SKGraphiteImageProviderProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGraphiteImageProviderProxyDelegate))]
#endif
	private static partial IntPtr SKGraphiteImageProviderProxyImplementation(void* userData,IntPtr recorder,IntPtr image,[MarshalAs (UnmanagedType.I1)] bool mipmapped);

	}
}
