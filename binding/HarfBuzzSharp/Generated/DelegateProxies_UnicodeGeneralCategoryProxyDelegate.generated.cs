using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_unicode_general_category_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_unicode_funcs_t, UInt32, void*, int> UnicodeGeneralCategoryProxy = &UnicodeGeneralCategoryProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly UnicodeGeneralCategoryProxyDelegate UnicodeGeneralCategoryProxy = UnicodeGeneralCategoryProxyImplementation;
	[MonoPInvokeCallback (typeof (UnicodeGeneralCategoryProxyDelegate))]
#endif
	private static partial int UnicodeGeneralCategoryProxyImplementation(hb_unicode_funcs_t ufuncs,UInt32 unicode,void* user_data);

	}
}
