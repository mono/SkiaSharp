using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace HarfBuzzSharp
{
	// public delegates

	public delegate void ReleaseDelegate ();

	public delegate Blob GetTableDelegate (Face face, Tag tag);

	[Obsolete ("Use ReleaseDelegate instead.")]
	public delegate void BlobReleaseDelegate (object context);

	internal delegate object UserDataDelegate ();

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
		public static IntPtr CreateMulti<T> (T wrappedDelegate, ReleaseDelegate destroy)
			where T : Delegate
		{
			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T))
					return wrappedDelegate;
				if (type == typeof (ReleaseDelegate))
					return destroy;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			Create (del, out _, out var ctx);

			return ctx;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T GetMulti<T> (IntPtr contextPtr, out GCHandle gch)
			where T : Delegate
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			return (T)multi.Invoke (typeof (T));
		}

		// internal proxy implementations

		[MonoPInvokeCallback (typeof (ReleaseDelegateProxyDelegate))]
		private static void ReleaseDelegateProxyImplementation (IntPtr context)
		{
			var del = Get<ReleaseDelegate> (context, out var gch);
			try {
				del.Invoke ();
			} finally {
				gch.Free ();
			}
		}

		[MonoPInvokeCallback (typeof (GetTableDelegateProxyDelegate))]
		private static IntPtr GetTableDelegateProxyImplementation (IntPtr face, Tag tag, IntPtr context)
		{
			var del = GetMulti<GetTableDelegate> (context, out var gch);
			var blob = del.Invoke (null, tag);
			return blob?.Handle ?? IntPtr.Zero;
		}

		[MonoPInvokeCallback (typeof (ReleaseDelegateProxyDelegate))]
		private static void ReleaseDelegateProxyImplementationForMulti (IntPtr context)
		{
			var del = GetMulti<ReleaseDelegate> (context, out var gch);
			try {
				del?.Invoke ();
			} finally {
				gch.Free ();
			}
		}
	}
}
