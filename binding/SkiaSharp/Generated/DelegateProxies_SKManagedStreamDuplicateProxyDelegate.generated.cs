using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for sk_managedstream_duplicate_proc native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, void*, IntPtr> SKManagedStreamDuplicateProxy = &SKManagedStreamDuplicateProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly SKManagedStreamDuplicateProxyDelegate SKManagedStreamDuplicateProxy = SKManagedStreamDuplicateProxyImplementation;
	[MonoPInvokeCallback (typeof (SKManagedStreamDuplicateProxyDelegate))]
#endif
	private static partial IntPtr SKManagedStreamDuplicateProxyImplementation(IntPtr s,void* context);

	}
}
