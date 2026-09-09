using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_graphite_vk_get_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, /* char */ void*, IntPtr, IntPtr, IntPtr> SKGraphiteVkGetProxy = &SKGraphiteVkGetProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKGraphiteVkGetProxyDelegate SKGraphiteVkGetProxy = SKGraphiteVkGetProxyImplementation;
	[MonoPInvokeCallback (typeof (SKGraphiteVkGetProxyDelegate))]
#endif
	private static partial IntPtr SKGraphiteVkGetProxyImplementation(void* userData,/* char */ void* name,IntPtr instance,IntPtr device);

	}
}
