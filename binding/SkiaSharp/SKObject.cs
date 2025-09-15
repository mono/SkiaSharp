#nullable disable

using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a tracked native object.
	/// </summary>
	/// <remarks>
	/// This object wraps a native handle and keeps track of it's lifetime for the garbage collector. For a simple object, use <see cref="SKNativeObject" />.
	/// </remarks>
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

		static SKObject ()
		{
			SkiaSharpVersion.CheckNativeLibraryCompatible (true);

			SKColorSpace.EnsureStaticInstanceAreInitialized ();
			SKData.EnsureStaticInstanceAreInitialized ();
			SKFontManager.EnsureStaticInstanceAreInitialized ();
			SKTypeface.EnsureStaticInstanceAreInitialized ();
			SKBlender.EnsureStaticInstanceAreInitialized ();
			SKColorFilter.EnsureStaticInstanceAreInitialized ();
		}

		internal SKObject (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>
		/// Gets or sets the handle to the underlying native object.
		/// </summary>
		/// <remarks>
		/// Setting this value will register this object with the lifetime tracker.
		/// </remarks>
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

		// indicate that the user cannot dispose the object
		internal void PreventPublicDisposal ()
		{
			IgnorePublicDispose = true;
		}

		// indicate that the ownership of this object is now in the hands of
		// the native object
		internal void RevokeOwnership (SKObject newOwner)
		{
			OwnsHandle = false;
			IgnorePublicDispose = true;

			if (newOwner == null)
				DisposeInternal ();
			else
				newOwner.OwnedObjects[Handle] = this;
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

	/// <summary>
	/// Represents a native object.
	/// </summary>
	/// <remarks>
	/// This object just wraps a native handle with the managed dispose pattern. For a tracked object, use <see cref="SKObject" />.
	/// </remarks>
	public abstract class SKNativeObject : IDisposable
	{
		internal bool fromFinalizer = false;

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

			Dispose (false);
		}

		/// <summary>
		/// Gets or sets the handle to the underlying native object.
		/// </summary>
		public virtual IntPtr Handle { get; protected set; }

		protected internal virtual bool OwnsHandle { get; protected set; }

		protected internal bool IgnorePublicDispose { get; set; }

		protected internal bool IsDisposed => isDisposed == 1;

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

		protected virtual void Dispose (bool disposing)
		{
			if (Interlocked.CompareExchange (ref isDisposed, 1, 0) != 0)
				return;

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

		/// <summary>
		/// Releases all resources used by this <see cref="SKNativeObject" />.
		/// </summary>
		/// <remarks>
		/// Always dispose the object before you release your last reference to the <see cref="SKNativeObject" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.
		/// </remarks>
		public void Dispose ()
		{
			if (IgnorePublicDispose)
				return;

			DisposeInternal ();
		}

		protected internal void DisposeInternal ()
		{
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
