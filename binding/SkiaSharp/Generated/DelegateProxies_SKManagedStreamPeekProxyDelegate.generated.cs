using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_peek_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, void*, /* size_t */ IntPtr, /* size_t */ IntPtr> SKManagedStreamPeekProxy = &SKManagedStreamPeekProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamPeekProxyDelegate SKManagedStreamPeekProxy = SKManagedStreamPeekProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamPeekProxyDelegate))]
#endif
	private static partial /* size_t */ IntPtr SKManagedStreamPeekProxyImplementation(IntPtr s,void* context,void* buffer,/* size_t */ IntPtr size);

	}
}
