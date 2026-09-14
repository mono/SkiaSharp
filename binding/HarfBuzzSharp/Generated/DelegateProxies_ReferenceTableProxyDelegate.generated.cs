using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace HarfBuzzSharp
{
	internal static unsafe partial class DelegateProxies
	{
	/// Proxy for hb_reference_table_func_t native function.
#if USE_LIBRARY_IMPORT
	public static readonly delegate* unmanaged[Cdecl] <hb_face_t, UInt32, void*, hb_blob_t> ReferenceTableProxy = &ReferenceTableProxyImplementation;
	[UnmanagedCallersOnly(CallConvs = new [] {typeof(CallConvCdecl)})]
#else
	public static readonly ReferenceTableProxyDelegate ReferenceTableProxy = ReferenceTableProxyImplementation;
	[MonoPInvokeCallback (typeof (ReferenceTableProxyDelegate))]
#endif
	private static partial hb_blob_t ReferenceTableProxyImplementation(hb_face_t face,UInt32 tag,void* user_data);

	}
}
