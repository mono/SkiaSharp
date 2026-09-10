#nullable disable

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>Represents a tracked native object.</summary>
	/// <remarks>This object wraps a native handle and keeps track of its lifetime for the garbage collector. For a simple object, use <see cref="T:SkiaSharp.SKNativeObject" />.</remarks>
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

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKObject" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKObject" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Gets or sets the handle to the underlying native object.</summary>
		/// <value>The pointer to the native object.</value>
		/// <remarks>Setting this value will register this object with the lifetime tracker.</remarks>
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

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to dispose managed objects that are not owned by this instance.</summary>
		/// <remarks>This method is called during disposal to clean up managed objects that were created by native code but are not owned by this instance. The default implementation disposes any child objects that don't own their handles.</remarks>
		protected override void DisposeUnownedManaged ()
		{
			if (ownedObjects != null) {
				foreach (var child in ownedObjects) {
					if (child.Value is SKObject c && !c.OwnsHandle)
						c.DisposeInternal ();
				}
			}
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any managed objects.</summary>
		/// <remarks />
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

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
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

		internal void RevokeOwnership (SKObject newOwner)
		{
			// We cannot dispose this wrapper because the native object might
			// call back into the wrapper e.g. via the proxies in SKAbstractManagedStream
			// so we have to wait until newOwner's lifetime ends.

			OwnsHandle = false;

			if (newOwner == null) {
				DisposeInternal ();
			}
			else {
				HandleDictionary.instancesLock.EnterWriteLock ();
				try {
					PreventPublicDisposal ();
				} finally {
					HandleDictionary.instancesLock.ExitWriteLock ();
				}
				newOwner.OwnedObjects[Handle] = this;
			}
		}
	}

	/// <summary>Represents a native object.</summary>
	/// <remarks>This object just wraps a native handle with the managed dispose pattern. For a tracked object, use <see cref="T:SkiaSharp.SKObject" />.</remarks>
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

		/// <summary>Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.</summary>
		/// <remarks />
		~SKNativeObject ()
		{
			fromFinalizer = true;

			// The public Dispose path additionally holds a HandleDictionary lock to check IgnorePublicDispose
			// but this is an internal Dispose, racing with a PreventPublicDisposal is not a concern.
			if (Interlocked.CompareExchange (ref isDisposed, 1, 0) != 0)
				return;
			Dispose (false);
		}

		/// <summary>Gets or sets the handle to the underlying native object.</summary>
		/// <value>The native object handle.</value>
		/// <remarks />
		public virtual IntPtr Handle { get; protected set; }

		/// <summary>Gets or sets a value indicating whether this object owns its handle and should destroy the native object when it is disposed.</summary>
		/// <value><see langword="true" /> if the object owns the handle and will destroy it when disposed; otherwise, <see langword="false" />.</value>
		/// <remarks />
		protected internal virtual bool OwnsHandle { get; protected set; }

		// One-way latch: once set to true, only stays true. Use PreventPublicDisposal() to set.
		// The set happens under the HandleDictionary upgradeable-read lock (via GetOrAddObject); the
		// public Dispose() decision reads it under the mutually-exclusive HandleDictionary write lock,
		// paired with the isDisposed CAS. So the latch can never be set on an instance that is concurrently
		// claiming public disposal. (The only unpaired read is the post-disposal diagnostic
		// re-check in Dispose(), which runs after isDisposed is already set.)
		/// <summary>Gets a value indicating whether the call the public <see cref="M:SkiaSharp.SKNativeObject.Dispose" /> should be no-op.</summary>
		/// <value><see langword="true" /> if the public Dispose call should be ignored; otherwise, <see langword="false" />.</value>
		/// <remarks>This only affects the public disposal. Objects that own this one can still forcefull trigger a dispose using <see cref="M:SkiaSharp.SKNativeObject.DisposeInternal" />.</remarks>
		protected internal bool IgnorePublicDispose { get; private set; }

		// Make this wrapper unreachable via the public Dispose() method.
		// This method does NOT take any lock itself: correctness relies on the CALLER
		// holding HandleDictionary.instancesLock (the upgradeable-read lock taken by
		// GetOrAddObject). That caller-held lock is mutually exclusive with the write
		// lock public Dispose() holds around its IgnorePublicDispose check + CAS, so the
		// flag set here cannot race a concurrent public disposal.
		// DO NOT USE DIRECTLY except from inside HandleDictionary.GetOrAddObject's
		// critical section, or when a concurrent Dispose() is guaranteed to be impossible.
		internal void PreventPublicDisposal ()
		{
#if THROW_OBJECT_EXCEPTIONS
			// All callers (GetOrAddObject) hold the HandleDictionary upgradeable-read lock and target either a
			// freshly-created wrapper or one that GetInstanceNoLocks just confirmed !IsDisposed.
			// A live target can only become disposed via: public Dispose() (blocked here by the
			// mutually-exclusive write lock), an owned-child / ownership-handoff / replacement
			// DisposeInternal, or the finalizer. No dispose-protected call site registers its
			// singleton as an owned child, hands off its ownership, or lets it be replaced, and
			// the promoting thread holds a strong reference (so no finalizer race). Observing a
			// disposed wrapper here therefore means one of those invariants was broken — a real
			// bug worth surfacing.
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
		/// <summary>Gets a value indicating whether the object has already been disposed.</summary>
		/// <value><see langword="true" /> if the object has been disposed; otherwise, <see langword="false" />.</value>
		/// <remarks />
		protected internal bool IsDisposed => Volatile.Read (ref isDisposed) == 1;

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKNativeObject" /> types to dispose managed objects that are not owned by this instance.</summary>
		/// <remarks>This method is called during disposal to clean up managed objects that were created by native code but are not owned by this instance.</remarks>
		protected virtual void DisposeUnownedManaged ()
		{
			// dispose of any managed resources that are not actually owned
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKNativeObject" /> types to destroy any managed objects.</summary>
		/// <remarks />
		protected virtual void DisposeManaged ()
		{
			// dispose of any managed resources
		}

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKNativeObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected virtual void DisposeNative ()
		{
			// dispose of any unmanaged resources
		}

		// The isDisposed CAS that used to guard this method has moved to the public
		// entry points (Dispose, DisposeInternal, finalizer) so that each entry's CAS
		// can be paired atomically with whatever check it needs (e.g. Dispose's
		// IgnorePublicDispose check under the HandleDictionary write lock). This method now assumes
		// the caller has already claimed the disposal — it is the cleanup body only.
		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKNativeObject" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKNativeObject" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
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

		/// <summary>Releases all resources used by this <see cref="T:SkiaSharp.SKNativeObject" />.</summary>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKNativeObject" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		public void Dispose ()
		{
			// Hold the HandleDictionary write lock only across the flag check + the isDisposed CAS.
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

		/// <summary>Triggers a dispose, ignoring the value of <see cref="P:SkiaSharp.SKNativeObject.IgnorePublicDispose" />.</summary>
		/// <remarks />
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
