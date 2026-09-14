using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_bitmap_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKBitmapReleaseProxy = &SKBitmapReleaseProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKBitmapReleaseProxyDelegate SKBitmapReleaseProxy = SKBitmapReleaseProxyImplementation;
	[MonoPInvokeCallback (typeof (SKBitmapReleaseProxyDelegate))]
#endif
	private static partial void SKBitmapReleaseProxyImplementation(void* addr,void* context);

	}
}
