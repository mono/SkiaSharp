using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_unicode_combining_class_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_unicode_funcs_t, UInt32, void*, int> UnicodeCombiningClassProxy = &UnicodeCombiningClassProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly UnicodeCombiningClassProxyDelegate UnicodeCombiningClassProxy = UnicodeCombiningClassProxyImplementation;
	[MonoPInvokeCallback (typeof (UnicodeCombiningClassProxyDelegate))]
#endif
	private static partial int UnicodeCombiningClassProxyImplementation(hb_unicode_funcs_t ufuncs,UInt32 unicode,void* user_data);

	}
}
