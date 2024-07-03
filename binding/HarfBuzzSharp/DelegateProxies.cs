#nullable disable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	public delegate void ReleaseDelegate ();

	public delegate Blob GetTableDelegate (Face face, Tag tag);

	internal static unsafe partial class DelegateProxies
	{
		// references to the proxy implementations
#if USE_LIBRARY_IMPORT
		public static readonly delegate* unmanaged[Cdecl] <void*, void> ReleaseDelegateProxy = &ReleaseDelegateProxyImplementation;
		public static readonly delegate* unmanaged[Cdecl] <void*, void> ReleaseDelegateProxyForMulti = &ReleaseDelegateProxyImplementationForMulti;
		public static readonly delegate* unmanaged[Cdecl] <IntPtr, uint, void*, IntPtr> GetTableDelegateProxy = &GetTableDelegateProxyImplementation;
#else
		public static readonly DestroyProxyDelegate ReleaseDelegateProxy = ReleaseDelegateProxyImplementation;
		public static readonly DestroyProxyDelegate ReleaseDelegateProxyForMulti = ReleaseDelegateProxyImplementationForMulti;
		public static readonly ReferenceTableProxyDelegate GetTableDelegateProxy = GetTableDelegateProxyImplementation;
#endif

		// internal proxy implementations
#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (DestroyProxyDelegate))]
#endif
		private static void ReleaseDelegateProxyImplementation (void* context)
		{
			var del = Get<ReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ();
			} finally {
				gch.Free ();
			}
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (ReferenceTableProxyDelegate))]
#endif
		private static IntPtr GetTableDelegateProxyImplementation (IntPtr face, uint tag, void* context)
		{
			GetMultiUserData<GetTableDelegate, Face> ((IntPtr)context, out var getTable, out var userData, out _);
			var blob = getTable.Invoke (userData, tag);
			return blob?.Handle ?? IntPtr.Zero;
		}

#if USE_LIBRARY_IMPORT
		[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
		[MonoPInvokeCallback (typeof (DestroyProxyDelegate))]
#endif
		private static void ReleaseDelegateProxyImplementationForMulti (void* context)
		{
			var del = GetMulti<ReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del?.Invoke ();
			} finally {
				gch.Free ();
			}
		}
	}
}
