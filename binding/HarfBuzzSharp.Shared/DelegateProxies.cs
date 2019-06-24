using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// public delegates

	public delegate void ReleaseDelegate (object context);

	public delegate Blob GetTableDelegate (Face face, Tag tag, object context);

	// bad choices.
	// this should not have had the "blob" prefix
	// it is a global dispose method, but we can't switch now
	// it is a breaking change since it will become ambiguous
	public delegate void BlobReleaseDelegate (object context);

	// internal proxy delegates

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate void ReleaseDelegateProxyDelegate (IntPtr context);

	[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
	internal delegate IntPtr GetTableDelegateProxyDelegate (IntPtr face, Tag tag, IntPtr context);

	internal static partial class DelegateProxies
	{
		// references to the proxy implementations
		public static readonly ReleaseDelegateProxyDelegate ReleaseDelegateProxy = ReleaseDelegateProxyImplementation;
		public static readonly ReleaseDelegateProxyDelegate ReleaseDelegateProxyForMulti = ReleaseDelegateProxyImplementationForMulti;
		public static readonly GetTableDelegateProxyDelegate GetTableDelegateProxy = GetTableDelegateProxyImplementation;

		// helper methods

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateMulti<T> (T wrappedDelegate, object context, ReleaseDelegate destroy)
			where T : Delegate
		{
			var destroyDelegate = destroy != null && context != null
				? new ReleaseDelegate ((_) => destroy (context))
				: destroy;

			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T))
					return wrappedDelegate;
				if (type == typeof (ReleaseDelegate))
					return destroyDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			Create (del, out _, out var ctx);

			return ctx;
		}

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (ReleaseDelegateProxyDelegate))]
		private static void ReleaseDelegateProxyImplementation (IntPtr context)
		{
			var del = Get<ReleaseDelegate> (context, out var gch);
			try {
				del.Invoke (null);
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GetTableDelegateProxyDelegate))]
		private static IntPtr GetTableDelegateProxyImplementation (IntPtr face, Tag tag, IntPtr context)
		{
			var multi = Get<GetMultiDelegateDelegate> (context, out var gch);
			var del = (GetTableDelegate)multi.Invoke (typeof (GetTableDelegate));
			var blob = del.Invoke (null, tag, null);
			return blob?.Handle ?? IntPtr.Zero;
		}

		[MonoPInvokeCallback (typeof (ReleaseDelegateProxyDelegate))]
		private static void ReleaseDelegateProxyImplementationForMulti (IntPtr context)
		{
			var multi = Get<GetMultiDelegateDelegate> (context, out var gch);
			var del = (ReleaseDelegate)multi.Invoke (typeof (ReleaseDelegate));
			try {
				del?.Invoke (null);
			} finally {
				gch.Free ();
			}
		}
	}
}
