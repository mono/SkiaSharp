using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_unicode_compose_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, UInt32, UInt32, UInt32*, void*, bool> UnicodeComposeProxy = &UnicodeComposeProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly UnicodeComposeProxyDelegate UnicodeComposeProxy = UnicodeComposeProxyImplementation;
	[MonoPInvokeCallback (typeof (UnicodeComposeProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool UnicodeComposeProxyImplementation(IntPtr ufuncs,UInt32 a,UInt32 b,UInt32* ab,void* user_data);

	}
}
