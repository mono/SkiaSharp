using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_hasLength_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, bool> SKManagedStreamHasLengthProxy = &SKManagedStreamHasLengthProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamHasLengthProxyDelegate SKManagedStreamHasLengthProxy = SKManagedStreamHasLengthProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamHasLengthProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool SKManagedStreamHasLengthProxyImplementation(IntPtr s,void* context);

	}
}
