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
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, /* size_t */ IntPtr, bool> SKManagedStreamSeekProxy = &SKManagedStreamSeekProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamSeekProxyDelegate SKManagedStreamSeekProxy = SKManagedStreamSeekProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamSeekProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool SKManagedStreamSeekProxyImplementation(IntPtr s,void* context,/* size_t */ IntPtr position);

	}
}
