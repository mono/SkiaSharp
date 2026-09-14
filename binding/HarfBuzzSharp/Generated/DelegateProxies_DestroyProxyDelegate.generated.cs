using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_destroy_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void> DestroyProxy = &DestroyProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly DestroyProxyDelegate DestroyProxy = DestroyProxyImplementation;
	[MonoPInvokeCallback (typeof (DestroyProxyDelegate))]
#endif
	private static partial void DestroyProxyImplementation(void* user_data);

	/// Proxy for hb_destroy_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <void*, void> DestroyProxyForMulti = &DestroyProxyImplementationForMulti;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly DestroyProxyDelegate DestroyProxyForMulti = DestroyProxyImplementationForMulti;
	[MonoPInvokeCallback (typeof (DestroyProxyDelegate))]
#endif
	private static partial void DestroyProxyImplementationForMulti(void* user_data);

	}
}
