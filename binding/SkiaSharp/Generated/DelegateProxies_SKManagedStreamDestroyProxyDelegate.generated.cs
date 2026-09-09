using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_destroy_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, void> SKManagedStreamDestroyProxy = &SKManagedStreamDestroyProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamDestroyProxyDelegate SKManagedStreamDestroyProxy = SKManagedStreamDestroyProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamDestroyProxyDelegate))]
#endif
	private static partial void SKManagedStreamDestroyProxyImplementation(IntPtr s,void* context);

	}
}
