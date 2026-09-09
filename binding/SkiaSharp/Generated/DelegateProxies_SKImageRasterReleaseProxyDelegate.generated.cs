using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_image_raster_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKImageRasterReleaseProxy = &SKImageRasterReleaseProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseProxy = SKImageRasterReleaseProxyImplementation;
	[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
#endif
	private static partial void SKImageRasterReleaseProxyImplementation(void* addr,void* context);

	/// Proxy for sk_image_raster_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKImageRasterReleaseProxyForCoTaskMem = &SKImageRasterReleaseProxyImplementationForCoTaskMem;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKImageRasterReleaseProxyDelegate SKImageRasterReleaseProxyForCoTaskMem = SKImageRasterReleaseProxyImplementationForCoTaskMem;
	[MonoPInvokeCallback (typeof (SKImageRasterReleaseProxyDelegate))]
#endif
	private static partial void SKImageRasterReleaseProxyImplementationForCoTaskMem(void* addr,void* context);

	}
}
