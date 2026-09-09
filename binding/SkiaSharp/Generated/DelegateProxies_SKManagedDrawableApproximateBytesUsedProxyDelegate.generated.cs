using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_manageddrawable_approximateBytesUsed_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr> SKManagedDrawableApproximateBytesUsedProxy = &SKManagedDrawableApproximateBytesUsedProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedDrawableApproximateBytesUsedProxyDelegate SKManagedDrawableApproximateBytesUsedProxy = SKManagedDrawableApproximateBytesUsedProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedDrawableApproximateBytesUsedProxyDelegate))]
#endif
	private static partial /* size_t */ IntPtr SKManagedDrawableApproximateBytesUsedProxyImplementation(IntPtr d,void* context);

	}
}
