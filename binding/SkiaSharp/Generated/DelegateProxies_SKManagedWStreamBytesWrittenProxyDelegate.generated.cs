using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedwstream_bytesWritten_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, /* size_t */ IntPtr> SKManagedWStreamBytesWrittenProxy = &SKManagedWStreamBytesWrittenProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedWStreamBytesWrittenProxyDelegate SKManagedWStreamBytesWrittenProxy = SKManagedWStreamBytesWrittenProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedWStreamBytesWrittenProxyDelegate))]
#endif
	private static partial /* size_t */ IntPtr SKManagedWStreamBytesWrittenProxyImplementation(sk_wstream_managedstream_t s,void* context);

	}
}
