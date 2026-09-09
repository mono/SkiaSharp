using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_read_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, void*, /* size_t */ IntPtr, /* size_t */ IntPtr> SKManagedStreamReadProxy = &SKManagedStreamReadProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamReadProxyDelegate SKManagedStreamReadProxy = SKManagedStreamReadProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamReadProxyDelegate))]
#endif
	private static partial /* size_t */ IntPtr SKManagedStreamReadProxyImplementation(IntPtr s,void* context,void* buffer,/* size_t */ IntPtr size);

	}
}
