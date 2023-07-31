using System;
using System.ComponentModel;

namespace HarfBuzzSharp
{
	public delegate void ReleaseDelegate ();

	public delegate Blob GetTableDelegate (Face face, Tag tag);

	internal static unsafe partial class DelegateProxies
	{
		// references to the proxy implementations
		public static readonly DestroyProxyDelegate ReleaseDelegateProxy = ReleaseDelegateProxyImplementation;
		public static readonly DestroyProxyDelegate ReleaseDelegateProxyForMulti = ReleaseDelegateProxyImplementationForMulti;
		public static readonly ReferenceTableProxyDelegate GetTableDelegateProxy = GetTableDelegateProxyImplementation;

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (DestroyProxyDelegate))]
		private static void ReleaseDelegateProxyImplementation (void* context)
		{
			var del = Get<ReleaseDelegate> ((IntPtr)context, out var gch);
			try {
				del.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (ReferenceTableProxyDelegate))]
		private static IntPtr GetTableDelegateProxyImplementation (IntPtr face, uint tag, void* context)
		{
			GetMultiUserData<GetTableDelegate, Face> ((IntPtr)context, out var getTable, out var userData, out _);
			var blob = getTable.Invoke (userData, tag);
			return blob?.Handle ?? IntPtr.Zero;
		}

		[MonoPInvokeCallback (typeof (DestroyProxyDelegate))]
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
