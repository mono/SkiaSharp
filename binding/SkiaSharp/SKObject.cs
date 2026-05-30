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

		// The public Dispose() decision atomically compare-and-swaps (CAS) this field under the
		// HandleDictionary write lock, paired with its read of IgnorePublicDispose, so a concurrent
		// PreventPublicDisposal (which runs under the mutually-exclusive upgradeable-read lock)
		// can't latch dispose-protection onto an instance that is simultaneously claiming public
		// disposal. Internal paths (DisposeInternal, finalizer) CAS this field with no lock — they
		// never read IgnorePublicDispose, so they have nothing to pair.
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

			// The public Dispose path additionally holds a HandleDictionary lock to check IgnorePublicDispose
			// but this is an internal Dispose, racing with a PreventPublicDisposal is not a concern.
			if (Interlocked.CompareExchange (ref isDisposed, 1, 0) != 0)
				return;
			Dispose (false);
		}

		public virtual IntPtr Handle { get; protected set; }

		protected internal virtual bool OwnsHandle { get; protected set; }

		// One-way latch: once set to true, only stays true. Use PreventPublicDisposal() to set.
		// The set happens under the HandleDictionary upgradeable-read lock (via GetOrAddObject); the
		// public Dispose() decision reads it under the mutually-exclusive HandleDictionary write lock,
		// paired with the isDisposed CAS. So the latch can never be set on an instance that is concurrently
		// claiming public disposal. (The only unpaired read is the post-disposal diagnostic
		// re-check in Dispose(), which runs after isDisposed is already set.)
		protected internal bool IgnorePublicDispose { get; private set; }

		// Make this wrapper unreachable via the public Dispose() method.
		// This method does NOT take any lock itself: correctness relies on the CALLER
		// holding the handle's HandleDictionary shard lock. Two callers satisfy this:
		// GetOrAddObject (holds the upgradeable-read lock when promoting a singleton),
		// and TransferOwnershipToNative (takes the write lock via GetLockFor before the
		// ownership handoff). Either lock is mutually exclusive with the write lock
		// public Dispose() holds (on the same shard) around its IgnorePublicDispose
		// check + CAS, so the flag set here cannot race a concurrent public disposal.
		// DO NOT USE DIRECTLY except from inside one of those locked critical sections,
		// or when a concurrent Dispose() is guaranteed to be impossible.
		internal void PreventPublicDisposal ()
		{
#if THROW_OBJECT_EXCEPTIONS
			// Callers hold the handle's HandleDictionary shard lock and target either a
			// freshly-created wrapper (GetOrAddObject promotion / internally-created
			// handoff stream) or one that GetInstanceNoLocks just confirmed !IsDisposed.
			// A live target can only become disposed via: public Dispose() (blocked here
			// by the mutually-exclusive write lock), an owned-child / ownership-handoff /
			// replacement DisposeInternal, or the finalizer. For the GetOrAddObject path
			// no dispose-protected singleton is registered as an owned child, handed off,
			// or replaced, and the promoting thread holds a strong reference (no finalizer
			// race). For the TransferOwnershipToNative path the wrapper is either created
			// internally (no external disposer) or a user-supplied stream — observing it
			// disposed here means the user disposed a stream they had already handed to
			// Create()/CreateTypeface() on another thread, i.e. genuine misuse. Either
			// way, a disposed wrapper here signals a broken invariant worth surfacing.
			if (IsDisposed)
				throw new InvalidOperationException (
					$"Attempted to dispose-protect an already-disposed wrapper. " +
					$"H: {Handle.ToString ("x")} Type: {GetType ()}");
#endif
			IgnorePublicDispose = true;
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
		// IgnorePublicDispose check under the HandleDictionary write lock). This method now assumes
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
			// Hold the HandleDictionary write lock for THIS handle only across the flag check
			// + the isDisposed CAS. This pairs the read of IgnorePublicDispose with the disposal
			// claim atomically: a concurrent PreventPublicDisposal (set under the same shard's
			// lock via GetOrAddObject) is mutually exclusive with this section. The handle maps
			// to a single shard, so GetLockFor returns the very lock GetOrAddObject holds for it.
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
			var disposalLock = HandleDictionary.GetLockFor (Handle);
			disposalLock.EnterWriteLock ();
			bool proceed;
			try {
				if (IgnorePublicDispose)
					return;
				proceed = Interlocked.CompareExchange (ref isDisposed, 1, 0) == 0;
			} finally {
				disposalLock.ExitWriteLock ();
			}

			if (!proceed)
				return;

#if THROW_OBJECT_EXCEPTIONS
			// Capture before cleanup: Dispose(true) zeroes Handle, and the diagnostic throw
			// path must not leak the native object. So claim+clean up first, then signal.
			var raced = IgnorePublicDispose;
			var racedHandle = Handle;
			var racedType = GetType ();
#endif

			Dispose (true);
			GC.SuppressFinalize (this);

#if THROW_OBJECT_EXCEPTIONS
			// We claimed the public disposal with IgnorePublicDispose observed false inside
			// the lock. Once isDisposed is set, GetInstanceNoLocks filters this wrapper out,
			// so no correct path can promote it afterwards. Seeing the flag set now means a
			// PreventPublicDisposal raced this disposal without holding the HandleDictionary lock.
			if (raced)
				throw new InvalidOperationException (
					$"A wrapper was dispose-protected concurrently with its public disposal. " +
					$"H: {racedHandle.ToString ("x")} Type: {racedType}");
#endif
		}

		protected internal void DisposeInternal ()
		{
			// Claim disposal via the CAS; if already claimed, no-op. No outer HandleDictionary lock —
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
		// Marks the wrapper as non-owning so the disposal won't unref the native.
		//
		// There are two cases, matching the pre-refactor RevokeOwnership semantics:
		//
		// 1. newOwner == null (native creation failed, so there is no receiver to
		//    keep the wrapper alive): dispose the managed wrapper eagerly.
		//
		// 2. newOwner != null: the receiving native object (codec/typeface) may read
		//    through this wrapper LAZILY. For example SKManagedStream /
		//    SKFrontBufferedManagedStream invoke managed read callbacks on demand
		//    (during SKCodec.GetPixels()), long after Create() returned. Disposing
		//    the wrapper now would close/null the underlying managed stream and crash
		//    that later read across the native->managed boundary. Instead we keep the
		//    wrapper alive by re-parenting it as a non-owning owned-child of newOwner;
		//    it is torn down only when newOwner is disposed. DisposeUnownedManaged()
		//    disposes !OwnsHandle children BEFORE the owner frees its native object, so
		//    the wrapper stops being read before the codec/typeface is freed.
		//
		// Order matters: OwnsHandle = false MUST precede any disposal. A racing
		// Dispose() that sees OwnsHandle = true would call DisposeNative and free
		// the native object that the receiving native code is about to use.
		//
		// For the reparenting case we latch OwnsHandle = false AND IgnorePublicDispose
		// (via PreventPublicDisposal) together under the same shard write lock that
		// public Dispose() takes before reading IgnorePublicDispose + claiming the
		// isDisposed CAS. This is essential: if we set OwnsHandle = false outside the
		// lock first, a racing Dispose() could observe IgnorePublicDispose == false,
		// win the disposal claim, skip DisposeNative (OwnsHandle already false) and run
		// DisposeManaged() — closing/nulling the managed stream the native owner still
		// reads lazily, re-creating the crash as a race. Setting both flags atomically
		// under the lock makes the two operations mutually exclusive with Dispose().
		//
		// Note: a public Dispose() that fully wins the lock BEFORE this method runs (the
		// caller disposed the stream concurrently with the Create() that is consuming it)
		// is unsupported misuse and out of scope here — that races native ownership itself.
		internal void TransferOwnershipToNative (SKObject newOwner)
		{
			if (newOwner == null) {
				// Native creation failed: no receiver to keep the wrapper alive.
				// DisposeInternal claims via the isDisposed CAS (no IgnorePublicDispose
				// read), so no shard lock is required to pair with Dispose().
				OwnsHandle = false;
				DisposeInternal ();
				return;
			}

			// Capture the handle before touching flags: if a racing Dispose() zeroed
			// Handle, keying OwnedObjects with the captured value keeps teardown reliable.
			var handle = Handle;
			var disposalLock = HandleDictionary.GetLockFor (handle);
			disposalLock.EnterWriteLock ();
			try {
				OwnsHandle = false;
				PreventPublicDisposal ();
			} finally {
				disposalLock.ExitWriteLock ();
			}

			newOwner.OwnedObjects[handle] = (SKObject)this;
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
