using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_unicode_decompose_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, UInt32, UInt32*, UInt32*, void*, bool> UnicodeDecomposeProxy = &UnicodeDecomposeProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly UnicodeDecomposeProxyDelegate UnicodeDecomposeProxy = UnicodeDecomposeProxyImplementation;
	[MonoPInvokeCallback (typeof (UnicodeDecomposeProxyDelegate))]
#endif
	[return: MarshalAs (UnmanagedType.I1)]
	private static partial bool UnicodeDecomposeProxyImplementation(IntPtr ufuncs,UInt32 ab,UInt32* a,UInt32* b,void* user_data);

	}
}
