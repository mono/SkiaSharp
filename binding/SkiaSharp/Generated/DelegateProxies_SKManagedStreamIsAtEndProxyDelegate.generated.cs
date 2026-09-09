using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_isAtEnd_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, bool> SKManagedStreamIsAtEndProxy = &SKManagedStreamIsAtEndProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamIsAtEndProxyDelegate SKManagedStreamIsAtEndProxy = SKManagedStreamIsAtEndProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamIsAtEndProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool SKManagedStreamIsAtEndProxyImplementation(IntPtr s,void* context);

	}
}
