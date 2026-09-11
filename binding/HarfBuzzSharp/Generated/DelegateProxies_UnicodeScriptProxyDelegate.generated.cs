using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_unicode_script_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_unicode_funcs_t, UInt32, void*, UInt32> UnicodeScriptProxy = &UnicodeScriptProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly UnicodeScriptProxyDelegate UnicodeScriptProxy = UnicodeScriptProxyImplementation;
	[MonoPInvokeCallback (typeof (UnicodeScriptProxyDelegate))]
#endif
	private static partial UInt32 UnicodeScriptProxyImplementation(hb_unicode_funcs_t ufuncs,UInt32 unicode,void* user_data);

	}
}
