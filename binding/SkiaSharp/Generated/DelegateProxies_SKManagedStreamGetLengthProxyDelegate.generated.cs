using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_getLength_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr> SKManagedStreamGetLengthProxy = &SKManagedStreamGetLengthProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamGetLengthProxyDelegate SKManagedStreamGetLengthProxy = SKManagedStreamGetLengthProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamGetLengthProxyDelegate))]
#endif
	private static partial /* size_t */ IntPtr SKManagedStreamGetLengthProxyImplementation(IntPtr s,void* context);

	}
}
