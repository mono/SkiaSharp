using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_surface_raster_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void*, void> SKSurfaceRasterReleaseProxy = &SKSurfaceRasterReleaseProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKSurfaceRasterReleaseProxyDelegate SKSurfaceRasterReleaseProxy = SKSurfaceRasterReleaseProxyImplementation;
	[MonoPInvokeCallback (typeof (SKSurfaceRasterReleaseProxyDelegate))]
#endif
	private static partial void SKSurfaceRasterReleaseProxyImplementation(void* addr,void* context);

	}
}
