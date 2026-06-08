#nullable disable

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if HARFBUZZ
namespace HarfBuzzSharp
#else
namespace SkiaSharp
#endif
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = GCHandleProxy;
#endif

	// helper delegates

	internal delegate Delegate GetMultiDelegateDelegate (Type index);

	internal delegate object UserDataDelegate ();

	internal static partial class DelegateProxies
	{
		// normal delegates

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void Create (object managedDel, out GCHandle gch, out IntPtr contextPtr)
		{
			if (managedDel == null) {
				gch = default (GCHandle);
				contextPtr = IntPtr.Zero;
				return;
			}

			gch = GCHandle.Alloc (managedDel);
			contextPtr = GCHandle.ToIntPtr (gch);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T Get<T> (IntPtr contextPtr, out GCHandle gch)
		{
			if (contextPtr == IntPtr.Zero) {
				gch = default (GCHandle);
				return default (T);
			}

			gch = GCHandle.FromIntPtr (contextPtr);
			return (T)gch.Target;
		}

		// user data delegates

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateUserData (object userData, bool makeWeak = false)
		{
			userData = makeWeak ? new WeakReference (userData) : userData;
			var del = new UserDataDelegate (() => userData);
			Create (del, out _, out var ctx);
			return ctx;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static T GetUserData<T> (IntPtr contextPtr, out GCHandle gch)
		{
			var del = Get<UserDataDelegate> (contextPtr, out gch);
			var value = del.Invoke ();
			return value is WeakReference weak ? (T)weak.Target : (T)value;
		}

		// multi-value delegates

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateMulti<T1, T2> (T1 wrappedDelegate1, T2 wrappedDelegate2)
			where T1 : Delegate
			where T2 : Delegate
		{
			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T1))
					return wrappedDelegate1;
				if (type == typeof (T2))
					return wrappedDelegate2;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			Create (del, out _, out var ctx);

			return ctx;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateMulti<T1, T2, T3> (T1 wrappedDelegate1, T2 wrappedDelegate2, T3 wrappedDelegate3)
			where T1 : Delegate
			where T2 : Delegate
			where T3 : Delegate
		{
			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T1))
					return wrappedDelegate1;
				if (type == typeof (T2))
					return wrappedDelegate2;
				if (type == typeof (T3))
					return wrappedDelegate3;
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

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void GetMulti<T1, T2, T3> (IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out T3 wrappedDelegate3, out GCHandle gch)
			where T1 : Delegate
			where T2 : Delegate
			where T3 : Delegate
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			wrappedDelegate1 = (T1)multi.Invoke (typeof (T1));
			wrappedDelegate2 = (T2)multi.Invoke (typeof (T2));
			wrappedDelegate3 = (T3)multi.Invoke (typeof (T3));
		}

		// multi-value delegate with user data

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateMultiUserData<T> (T wrappedDelegate, object userData, bool makeWeak = false)
			where T : Delegate
		{
			userData = makeWeak ? new WeakReference (userData) : userData;
			var userDataDelegate = new UserDataDelegate (() => userData);

			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T))
					return wrappedDelegate;
				if (type == typeof (UserDataDelegate))
					return userDataDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			Create (del, out _, out var ctx);

			return ctx;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateMultiUserData<T1, T2> (T1 wrappedDelegate1, T2 wrappedDelegate2, object userData, bool makeWeak = false)
			where T1 : Delegate
			where T2 : Delegate
		{
			userData = makeWeak ? new WeakReference (userData) : userData;
			var userDataDelegate = new UserDataDelegate (() => userData);

			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T1))
					return wrappedDelegate1;
				if (type == typeof (T2))
					return wrappedDelegate2;
				if (type == typeof (UserDataDelegate))
					return userDataDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			Create (del, out _, out var ctx);

			return ctx;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static IntPtr CreateMultiUserData<T1, T2, T3> (T1 wrappedDelegate1, T2 wrappedDelegate2, T3 wrappedDelegate3, object userData, bool makeWeak = false)
			where T1 : Delegate
			where T2 : Delegate
			where T3 : Delegate
		{
			userData = makeWeak ? new WeakReference (userData) : userData;
			var userDataDelegate = new UserDataDelegate (() => userData);

			var del = new GetMultiDelegateDelegate ((type) => {
				if (type == typeof (T1))
					return wrappedDelegate1;
				if (type == typeof (T2))
					return wrappedDelegate2;
				if (type == typeof (T3))
					return wrappedDelegate3;
				if (type == typeof (UserDataDelegate))
					return userDataDelegate;
				throw new ArgumentOutOfRangeException (nameof (type));
			});

			Create (del, out _, out var ctx);

			return ctx;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static TUserData GetMultiUserData<TUserData> (IntPtr contextPtr, out GCHandle gch)
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			return GetUserData<TUserData> (multi);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void GetMultiUserData<T, TUserData> (IntPtr contextPtr, out T wrappedDelegate, out TUserData userData, out GCHandle gch)
			where T : Delegate
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			wrappedDelegate = (T)multi.Invoke (typeof (T));
			userData = GetUserData<TUserData> (multi);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void GetMultiUserData<T1, T2, TUserData> (IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out TUserData userData, out GCHandle gch)
			where T1 : Delegate
			where T2 : Delegate
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			wrappedDelegate1 = (T1)multi.Invoke (typeof (T1));
			wrappedDelegate2 = (T2)multi.Invoke (typeof (T2));
			userData = GetUserData<TUserData> (multi);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static void GetMultiUserData<T1, T2, T3, TUserData> (IntPtr contextPtr, out T1 wrappedDelegate1, out T2 wrappedDelegate2, out T3 wrappedDelegate3, out TUserData userData, out GCHandle gch)
			where T1 : Delegate
			where T2 : Delegate
			where T3 : Delegate
		{
			var multi = Get<GetMultiDelegateDelegate> (contextPtr, out gch);
			wrappedDelegate1 = (T1)multi.Invoke (typeof (T1));
			wrappedDelegate2 = (T2)multi.Invoke (typeof (T2));
			wrappedDelegate3 = (T3)multi.Invoke (typeof (T3));
			userData = GetUserData<TUserData> (multi);
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static TUserData GetUserData<TUserData> (GetMultiDelegateDelegate multi)
		{
			var userDataDelegate = (UserDataDelegate)multi.Invoke (typeof (UserDataDelegate));
			var value = userDataDelegate.Invoke ();
			return value is WeakReference weak ? (TUserData)weak.Target : (TUserData)value;
		}
	}

#if THROW_OBJECT_EXCEPTIONS
	// an internal, debug-only proxy that we can use to make sure we are not
	// leaking GC handles by accident
	internal struct GCHandleProxy
	{
		internal static readonly ConcurrentDictionary<IntPtr, WeakReference> allocatedHandles = new ConcurrentDictionary<IntPtr, WeakReference> ();

		private System.Runtime.InteropServices.GCHandle gch;

		public GCHandleProxy (System.Runtime.InteropServices.GCHandle gcHandle)
		{
			gch = gcHandle;
		}

		public bool IsAllocated => gch.IsAllocated;

		public object Target => gch.Target;

		public void Free ()
		{
			if (!allocatedHandles.TryRemove (ToIntPtr (this), out _))
				throw new InvalidOperationException ($"Allocated GC handle has already been freed.");

			gch.Free ();
		}

		internal static GCHandleProxy Alloc (object value)
		{
			var gch = new GCHandleProxy (System.Runtime.InteropServices.GCHandle.Alloc (value));

			var weak = new WeakReference (value);
			var oldWeak = allocatedHandles.GetOrAdd (ToIntPtr (gch), weak);
			if (weak != oldWeak)
				throw new InvalidOperationException (
					$"GC handle has already been allocated for this memory location. " +
					$"Old: {oldWeak.Target} New: {value}");

			return gch;
		}

		internal static GCHandleProxy FromIntPtr (IntPtr value) =>
			new GCHandleProxy (System.Runtime.InteropServices.GCHandle.FromIntPtr (value));

		internal static IntPtr ToIntPtr (GCHandleProxy value) =>
			System.Runtime.InteropServices.GCHandle.ToIntPtr (value.gch);
	}
#endif

	[AttributeUsage (AttributeTargets.Method)]
	internal sealed class MonoPInvokeCallbackAttribute : Attribute
	{
		public MonoPInvokeCallbackAttribute (Type type)
		{
			Type = type;
		}

		public Type Type { get; private set; }
	}
}
