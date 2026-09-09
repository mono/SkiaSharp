using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_unicode_mirroring_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <IntPtr, UInt32, void*, UInt32> UnicodeMirroringProxy = &UnicodeMirroringProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly UnicodeMirroringProxyDelegate UnicodeMirroringProxy = UnicodeMirroringProxyImplementation;
	[MonoPInvokeCallback (typeof (UnicodeMirroringProxyDelegate))]
#endif
	private static partial UInt32 UnicodeMirroringProxyImplementation(IntPtr ufuncs,UInt32 unicode,void* user_data);

	}
}
