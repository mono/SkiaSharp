using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_image_texture_release_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void> SKImageTextureReleaseProxy = &SKImageTextureReleaseProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKImageTextureReleaseProxyDelegate SKImageTextureReleaseProxy = SKImageTextureReleaseProxyImplementation;
	[MonoPInvokeCallback (typeof (SKImageTextureReleaseProxyDelegate))]
#endif
	private static partial void SKImageTextureReleaseProxyImplementation(void* context);

	}
}
