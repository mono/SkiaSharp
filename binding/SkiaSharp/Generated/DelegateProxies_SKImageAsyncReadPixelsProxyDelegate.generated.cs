using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_image_async_read_pixels_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, IntPtr, void> SKImageAsyncReadPixelsProxy = &SKImageAsyncReadPixelsProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKImageAsyncReadPixelsProxyDelegate SKImageAsyncReadPixelsProxy = SKImageAsyncReadPixelsProxyImplementation;
	[MonoPInvokeCallback (typeof (SKImageAsyncReadPixelsProxyDelegate))]
#endif
	private static partial void SKImageAsyncReadPixelsProxyImplementation(void* context,IntPtr result);

	}
}
