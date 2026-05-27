#nullable disable

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public abstract class SKObject : SKNativeObject
	{
		private readonly object locker = new object ();

		private ConcurrentDictionary<IntPtr, SKObject> ownedObjects;
		private ConcurrentDictionary<IntPtr, SKObject> keepAliveObjects;

		internal ConcurrentDictionary<IntPtr, SKObject> OwnedObjects {
			get {
				if (ownedObjects == null) {
					lock (locker) {
						ownedObjects ??= new ConcurrentDictionary<IntPtr, SKObject> ();
					}
				}
				return ownedObjects;
			}
		}

		internal ConcurrentDictionary<IntPtr, SKObject> KeepAliveObjects {
			get {
				if (keepAliveObjects == null) {
					lock (locker) {
						keepAliveObjects ??= new ConcurrentDictionary<IntPtr, SKObject> ();
					}
				}
				return keepAliveObjects;
			}
		}

		internal SKObject (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public override IntPtr Handle {
			get => base.Handle;
			protected set {
				if (value == IntPtr.Zero) {
					DeregisterHandle (Handle, this);
					base.Handle = value;
				} else {
					base.Handle = value;
					RegisterHandle (Handle, this);
				}
			}
		}

		protected override void DisposeUnownedManaged ()
		{
			if (ownedObjects != null) {
				foreach (var child in ownedObjects) {
					if (child.Value is SKObject c && !c.OwnsHandle)
						c.DisposeInternal ();
				}
			}
		}

		protected override void DisposeManaged ()
		{
			if (ownedObjects != null) {
				foreach (var child in ownedObjects) {
					if (child.Value is SKObject c && c.OwnsHandle)
						c.DisposeInternal ();
				}
				ownedObjects.Clear ();
			}
			keepAliveObjects?.Clear ();
		}

		protected override void DisposeNative ()
		{
			if (this is ISKReferenceCounted refcnt)
				refcnt.SafeUnRef ();
		}

		internal static TSkiaObject GetOrAddObject<TSkiaObject> (IntPtr handle, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return null;

			return HandleDictionary.GetOrAddObject (handle, true, true, objectFactory);
		}

		internal static TSkiaObject GetOrAddObject<TSkiaObject> (IntPtr handle, bool owns, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return null;

			return HandleDictionary.GetOrAddObject (handle, owns, true, objectFactory);
		}

		internal static TSkiaObject GetOrAddObject<TSkiaObject> (IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return null;

			return HandleDictionary.GetOrAddObject (handle, owns, unrefExisting, objectFactory);
		}

		// Variant that promotes the returned wrapper to dispose-protected
		// (IgnorePublicDispose = true) inside HandleDictionary's critical section.
		// Used by the singleton accessors (CreateSrgb, Default, etc.). "Dispose-protected"
		// means the public Dispose() is short-circuited — the wrapper is NOT immortal
		// from GC's perspective; finalization and DisposeInternal still tear it down.
		// The actual long-lived persistence comes from each accessor's static-field
		// cache acting as a GC root.
		internal static TSkiaObject GetOrAddDisposeProtectedObject<TSkiaObject> (IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return null;

			return HandleDictionary.GetOrAddObject (handle, owns, unrefExisting, disposeProtected: true, objectFactory);
		}

		internal static void RegisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero || instance == null)
				return;

			HandleDictionary.RegisterHandle (handle, instance);
		}

		internal static void DeregisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
				return;

			HandleDictionary.DeregisterHandle (handle, instance);
		}

		internal static bool GetInstance<TSkiaObject> (IntPtr handle, out TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero) {
				instance = null;
				return false;
			}

			return HandleDictionary.GetInstance<TSkiaObject> (handle, out instance);
		}

		// indicate that the child is controlled by the native code and
		// the managed wrapper should be disposed when the owner is
		internal static T OwnedBy<T> (T child, SKObject owner)
			where T : SKObject
		{
			if (child != null) {
				owner.OwnedObjects[child.Handle] = child;
			}

			return child;
		}

		// indicate that the child was created by the managed code and
		// should be disposed when the owner is
		internal static T Owned<T> (T owner, SKObject child)
			where T : SKObject
		{
			if (child != null) {
				if (owner != null)
					owner.OwnedObjects[child.Handle] = child;
				else
					child.Dispose ();
			}

			return owner;
		}

		// indicate that the child should not be garbage collected while
		// the owner still lives
		internal static T Referenced<T> (T owner, SKObject child)
			where T : SKObject
		{
			if (child != null && owner != null)
				owner.KeepAliveObjects[child.Handle] = child;

			return owner;
		}
	}

	public abstract class SKNativeObject : IDisposable
	{
		internal bool fromFinalizer = false;

		// reads/writes of this field need to be in the same critical section as IgnorePublicDispose
		// so that e.g. you don't set IgnorePublicDispose concurrently on an instance that's being disposed.
		private int isDisposed = 0;

		internal SKNativeObject (IntPtr handle)
			: this (handle, true)
		{
		}

		internal SKNativeObject (IntPtr handle, bool ownsHandle)
		{
			Handle = handle;
			OwnsHandle = ownsHandle;
		}

		~SKNativeObject ()
		{
			fromFinalizer = true;

			// Claim disposal here (the CAS used to be inside Dispose(bool), but that's
			// moved out so each entry point gates atomically with whatever check it
			// pairs with). No outer HD lock — same reasoning as DisposeInternal.
			if (Interlocked.CompareExchange (ref isDisposed, 1, 0) != 0)
				return;
			Dispose (false);
		}

		public virtual IntPtr Handle { get; protected set; }

		protected internal virtual bool OwnsHandle { get; protected set; }

		// One-way latch: once set to true, only stays true. Use PreventPublicDisposal() to set.
		// reads/writes of this property need to be in the same critical section as isDisposed.
		protected internal bool IgnorePublicDispose { get; private set; }

		// Make this wrapper unreachable via the public Dispose() method.
		// Acquires the HD write lock so the flag set is serialized with any concurrent
		// Dispose() (which holds the same lock around its check). Atomic with respect
		// to "is this wrapper still in HD" — closes the promote-existing race in
		// singleton init (#3817 discussion thread).
		internal void PreventPublicDisposal ()
		{
			if (IgnorePublicDispose)
				return;

			HandleDictionary.instancesLock.EnterWriteLock ();
			try {
				IgnorePublicDispose = true;
			} finally {
				HandleDictionary.instancesLock.ExitWriteLock ();
			}
		}

		// Volatile.Read for acquire semantics on weak memory models (ARM/ARM64). The
		// post-refactor design relies on HandleDictionary.GetInstanceNoLocks reading
		// this property to filter out disposed wrappers, so a stale read could let a
		// disposed wrapper escape the filter and become the cached singleton.
		protected internal bool IsDisposed => Volatile.Read (ref isDisposed) == 1;

		protected virtual void DisposeUnownedManaged ()
		{
			// dispose of any managed resources that are not actually owned
		}

		protected virtual void DisposeManaged ()
		{
			// dispose of any managed resources
		}

		protected virtual void DisposeNative ()
		{
			// dispose of any unmanaged resources
		}

		// The isDisposed CAS that used to guard this method has moved to the public
		// entry points (Dispose, DisposeInternal, finalizer) so that each entry's CAS
		// can be paired atomically with whatever check it needs (e.g. Dispose's
		// IgnorePublicDispose check under the HD write lock). This method now assumes
		// the caller has already claimed the disposal — it is the cleanup body only.
		protected virtual void Dispose (bool disposing)
		{
			// dispose any objects that are owned/created by native code
			if (disposing)
				DisposeUnownedManaged ();

			// destroy the native object
			if (Handle != IntPtr.Zero && OwnsHandle)
				DisposeNative ();

			// dispose any remaining managed-created objects
			if (disposing)
				DisposeManaged ();

			Handle = IntPtr.Zero;
		}

		public void Dispose ()
		{
			// Hold the HD write lock only across the flag check + the isDisposed CAS.
			// This pairs the read of IgnorePublicDispose with the disposal claim
			// atomically: a concurrent PreventPublicDisposal (which takes the same
			// write lock) is mutually exclusive with this section.
			//
			// The actual cleanup runs *outside* the lock. That's safe because:
			// 1. If a concurrent thread is in GetOrAddObject looking up this handle
			//    *after* our CAS landed, GetInstanceNoLocks filters out wrappers with
			//    IsDisposed=true — it returns false and the caller falls into the
			//    factory branch, producing a fresh wrapper.
			// 2. RegisterHandle's replacement branch only fires for !IsDisposed
			//    existing entries, so a fresh wrapper registering for this handle
			//    while we're still mid-cleanup just overwrites our stale weak ref
			//    without trying to dispose us recursively.
			// 3. Our own Handle = 0 → DeregisterHandle path correctly handles
			//    "weak.Target is now someone else" (no-op in release).
			HandleDictionary.instancesLock.EnterWriteLock ();
			bool proceed;
			try {
				if (IgnorePublicDispose)
					return;
				proceed = Interlocked.CompareExchange (ref isDisposed, 1, 0) == 0;
			} finally {
				HandleDictionary.instancesLock.ExitWriteLock ();
			}

			if (!proceed)
				return;

			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected internal void DisposeInternal ()
		{
			// Claim disposal via the CAS; if already claimed, no-op. No outer HD lock —
			// DisposeInternal doesn't read IgnorePublicDispose, so there's no flag
			// check that needs to be paired with the CAS.
			if (Interlocked.CompareExchange (ref isDisposed, 1, 0) != 0)
				return;
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		// Hand off ownership of the native object to native-side code that took it
		// (e.g. SKCodec.Create / SKFontManager.CreateTypeface, where the C wrapper
		// puts the stream into a unique_ptr that the codec/typeface then owns).
		// Marks the wrapper as non-owning so the disposal won't unref the native,
		// then disposes the managed wrapper. The user's reference to this wrapper
		// becomes a disposed wrapper; subsequent use fails loudly rather than
		// operating on a native object that's now owned elsewhere.
		//
		// Order matters: OwnsHandle = false MUST precede DisposeInternal. A racing
		// Dispose() that sees OwnsHandle = true would call DisposeNative and free
		// the native object that the receiving native code is about to use.
		internal void TransferOwnershipToNative ()
		{
			OwnsHandle = false;
			DisposeInternal ();
		}
	}

	internal static class SKObjectExtensions
	{
		public static bool IsUnique (this IntPtr handle, bool isVirtual)
		{
			if (isVirtual)
				return SkiaApi.sk_refcnt_unique (handle);
			else
				return SkiaApi.sk_nvrefcnt_unique (handle);
		}

		public static int GetReferenceCount (this IntPtr handle, bool isVirtual)
		{
			if (isVirtual)
				return SkiaApi.sk_refcnt_get_ref_count (handle);
			else
				return SkiaApi.sk_nvrefcnt_get_ref_count (handle);
		}

		public static void SafeRef (this ISKReferenceCounted obj)
		{
			if (obj is ISKNonVirtualReferenceCounted nvrefcnt)
				nvrefcnt.ReferenceNative ();
			else
				SkiaApi.sk_refcnt_safe_ref (obj.Handle);
		}

		public static void SafeUnRef (this ISKReferenceCounted obj)
		{
			if (obj is ISKNonVirtualReferenceCounted nvrefcnt)
				nvrefcnt.UnreferenceNative ();
			else
				SkiaApi.sk_refcnt_safe_unref (obj.Handle);
		}

		public static int GetReferenceCount (this ISKReferenceCounted obj)
		{
			if (obj is ISKNonVirtualReferenceCounted)
				return SkiaApi.sk_nvrefcnt_get_ref_count (obj.Handle);
			else
				return SkiaApi.sk_refcnt_get_ref_count (obj.Handle);
		}
	}

	/// <summary>
	/// This should be implemented on all types that inherit directly or
	/// indirectly from SkRefCnt or SkRefCntBase
	/// </summary>
	internal interface ISKReferenceCounted
	{
		IntPtr Handle { get; }
	}

	/// <summary>
	/// This should be implemented on all types that inherit directly or
	/// indirectly from SkNVRefCnt
	/// </summary>
	internal interface ISKNonVirtualReferenceCounted : ISKReferenceCounted
	{
		void ReferenceNative ();

		void UnreferenceNative ();
	}

	/// <summary>
	/// This should be implemented on all types that can skip the expensive
	/// registration in the global dictionary. Typically this would be the case
	/// if the type os _only_ constructed by the user and not provided as a
	/// return type for _any_ member.
	/// </summary>
	internal interface ISKSkipObjectRegistration
	{
	}
}
