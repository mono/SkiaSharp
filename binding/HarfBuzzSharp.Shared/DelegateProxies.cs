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
		public static T GetUserData<T> (IntPtr context)
		{
			var del = GetMulti<UserDataDelegate> (context, out _);
			var userData = del.Invoke ();
			return (T)userData;
		}

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
		public static IntPtr CreateMulti<T1, T2> (T1 wrappedDelegate1, T2 wrappedDelegate2, ReleaseDelegate destroy)
			where T1 : Delegate
			where T2 : Delegate
		{
			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T1))
					return wrappedDelegate1;
				if (type == typeof (T2))
					return wrappedDelegate2;
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

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void GetMulti<T1, T2> (IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out GCHandle gch)
			where T1 : Delegate
			where T2 : Delegate
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			wrappedDelegate1 = (T1)multi.Invoke (typeof (T1));
			wrappedDelegate2 = (T2)multi.Invoke (typeof (T2));
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
			GetMulti<GetTableDelegate, UserDataDelegate> (context, out var getTable, out var userData, out _);
			var actualFace = (Face)userData?.Invoke ();
			var blob = getTable.Invoke (actualFace, tag);
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
