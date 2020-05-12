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

		static SKObject ()
		{
			SKColorSpace.EnsureStaticInstanceAreInitialized ();
			SKData.EnsureStaticInstanceAreInitialized ();
			SKFontManager.EnsureStaticInstanceAreInitialized ();
			SKTypeface.EnsureStaticInstanceAreInitialized ();
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

		protected override void DisposeManaged ()
		{
			if (ownedObjects != null) {
				foreach (var child in ownedObjects) {
					child.Value?.DisposeInternal ();
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

		// indicate that the chile should not be garbage collected while
		// the owner still lives
		internal static T Referenced<T> (T owner, SKObject child)
			where T : SKObject
		{
			if (child != null && owner != null)
				owner.KeepAliveObjects[child.Handle] = child;

			return owner;
		}

		internal static int SizeOf<T> () =>
#if WINDOWS_UWP || NET_STANDARD
			Marshal.SizeOf<T> ();
#else
			Marshal.SizeOf (typeof (T));
#endif

		internal static T PtrToStructure<T> (IntPtr intPtr) =>
#if WINDOWS_UWP || NET_STANDARD
			Marshal.PtrToStructure<T> (intPtr);
#else
			(T)Marshal.PtrToStructure (intPtr, typeof (T));
#endif

		internal static T[] PtrToStructureArray<T> (IntPtr intPtr, int count)
		{
			var items = new T[count];
			var size = SizeOf<T> ();
			for (var i = 0; i < count; i++) {
				var newPtr = new IntPtr (intPtr.ToInt64 () + (i * size));
				items[i] = PtrToStructure<T> (newPtr);
			}
			return items;
		}

		internal static T PtrToStructure<T> (IntPtr intPtr, int index)
		{
			var size = SizeOf<T> ();
			var newPtr = new IntPtr (intPtr.ToInt64 () + (index * size));
			return PtrToStructure<T> (newPtr);
		}
	}

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

		public virtual IntPtr Handle { get; protected set; }

		protected internal virtual bool OwnsHandle { get; protected set; }

		protected internal bool IgnorePublicDispose { get; set; }

		protected internal bool IsDisposed => isDisposed == 1;

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

			if (Handle != IntPtr.Zero && OwnsHandle)
				DisposeNative ();

			if (disposing)
				DisposeManaged ();

			Handle = IntPtr.Zero;
		}

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
				SkiaApi.sk_refcnt_safe_unref (obj.Handle);
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

	internal interface ISKReferenceCounted
	{
		IntPtr Handle { get; }
	}

	internal interface ISKNonVirtualReferenceCounted : ISKReferenceCounted
	{
		void ReferenceNative ();

		void UnreferenceNative ();
	}

	internal interface ISKSkipObjectRegistration
	{
	}
}
