using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedwstream_write_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <sk_wstream_managedstream_t, void*, void*, /* size_t */ IntPtr, bool> SKManagedWStreamWriteProxy = &SKManagedWStreamWriteProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedWStreamWriteProxyDelegate SKManagedWStreamWriteProxy = SKManagedWStreamWriteProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedWStreamWriteProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool SKManagedWStreamWriteProxyImplementation(sk_wstream_managedstream_t s,void* context,void* buffer,/* size_t */ IntPtr size);

	}
}
