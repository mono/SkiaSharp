using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_seek_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_stream_managedstream_t, void*, /* size_t */ IntPtr, Byte> SKManagedStreamSeekProxy = &SKManagedStreamSeekProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamSeekProxyDelegate SKManagedStreamSeekProxy = SKManagedStreamSeekProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamSeekProxyDelegate))]
#endif
	private static partial Byte SKManagedStreamSeekProxyImplementation(sk_stream_managedstream_t s,void* context,/* size_t */ IntPtr position);

	}
}
