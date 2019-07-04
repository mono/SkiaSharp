using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public abstract class SKObject : SKNativeObject
	{
		internal static readonly Dictionary<Type, ConstructorInfo> constructors = new Dictionary<Type, ConstructorInfo> ();
		internal static readonly ConcurrentDictionary<IntPtr, WeakReference> instances = new ConcurrentDictionary<IntPtr, WeakReference> ();

		internal readonly ConcurrentDictionary<IntPtr, SKObject> ownedObjects = new ConcurrentDictionary<IntPtr, SKObject> ();

		[Preserve]
		internal SKObject (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public override IntPtr Handle {
			get => base.Handle;
			protected set {
				base.Handle = value;
				RegisterHandle (Handle, this);
			}
		}

		protected override void DisposeManaged ()
		{
			foreach (var child in ownedObjects) {
				child.Value.DisposeInternal ();
			}
			ownedObjects.Clear ();

			DeregisterHandle (Handle, this);
		}

		protected override void DisposeNative ()
		{
			if (this is ISKReferenceCounted refcnt)
				refcnt.SafeUnRef ();
		}

		internal static TSkiaObject GetObject<TSkiaObject> (IntPtr handle, bool owns = true, bool unrefNative = true)
			where TSkiaObject : SKObject
		{
			return GetObject<TSkiaObject, TSkiaObject> (handle, owns, unrefNative);
		}

		internal static TSkiaObject GetObject<TSkiaObject, TSkiaImplementation> (IntPtr handle, bool owns = true, bool unrefNative = true)
			where TSkiaObject : SKObject
			where TSkiaImplementation : SKObject, TSkiaObject
		{
			if (handle == IntPtr.Zero)
				return null;

			if (GetInstance<TSkiaObject> (handle, out var instance)) {
				// some object get automatically referenced on the native side,
				// but managed code just has the same reference
				if (unrefNative && instance is ISKReferenceCounted refcnt) {
#if DEBUG
					if (refcnt.GetReferenceCount () == 1) {
						throw new InvalidOperationException (
							$"About to unreference an object that has no references. " +
							$"Type: {instance.GetType ()}");
					}
#endif
					refcnt.SafeUnRef ();
				}

				return instance;
			}

			var type = typeof (TSkiaImplementation);
			if (!constructors.TryGetValue (type, out var constructor)) {
				constructor = type.GetTypeInfo ().DeclaredConstructors.FirstOrDefault (c => {
					var parameters = c.GetParameters ();
					return parameters.Length == 2 &&
						parameters[0].ParameterType == typeof (IntPtr) &&
						parameters[1].ParameterType == typeof (bool);
				});
				constructors[type] = constructor ?? throw new MissingMethodException ($"No constructor found for {type.FullName}.ctor(System.IntPtr, System.Boolean)");
			}

			return (TSkiaObject)constructor.Invoke (new object[] { handle, owns });
		}

		internal static void RegisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero || instance == null)
				return;

#if DEBUG
			if (GetInstance<SKObject> (handle, out var obj) && obj.OwnsHandle)
				throw new InvalidOperationException (
					$"A managed object already exists for the specified native object. " +
					$"Type: ({obj.GetType ()}, {instance.GetType ()})");
#endif

			instances[handle] = new WeakReference (instance);
		}

		internal static void DeregisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
				return;

			var removed = instances.TryRemove (handle, out _);

#if DEBUG
			if (!removed) {
				throw new InvalidOperationException (
					$"A managed object did not exist for the specified native object. " +
					$"Type: {instance.GetType ()}");
			}
#endif
		}

		internal static bool GetInstance<TSkiaObject> (IntPtr handle, out TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (instances.TryGetValue (handle, out var weak) && weak?.Target is TSkiaObject obj && obj.Handle != IntPtr.Zero) {
				instance = obj;
				return true;
			}
			instance = null;
			return false;
		}

		// indicate that when this object is disposed on the managed side,
		// dispose the child as well
		internal void SetDisposeChild (SKObject child)
		{
			if (child == null)
				return;
			ownedObjects[child.Handle] = child;
		}

		// indicate that the ownership of this object is now in the hands of
		// the native object
		internal void RevokeOwnership (SKObject newOwner)
		{
			OwnsHandle = false;
			IgnoreDispose = true;

			if (newOwner == null)
				DisposeInternal ();
			else
				newOwner.SetDisposeChild (this);
		}

		internal static int SizeOf<T> () =>
#if WINDOWS_UWP || NET_STANDARD
			Marshal.SizeOf <T> ();
#else
			Marshal.SizeOf (typeof (T));
#endif

		internal static T PtrToStructure<T> (IntPtr intPtr) =>
#if WINDOWS_UWP || NET_STANDARD
			Marshal.PtrToStructure <T> (intPtr);
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
		private bool isDisposed = false;

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
			Dispose (false);
		}

		public virtual IntPtr Handle { get; protected set; }

		protected internal bool OwnsHandle { get; set; }

		protected internal bool IgnoreDispose { get; set; }

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
			if (isDisposed)
				return;
			isDisposed = true;

			if (disposing) {
				DisposeManaged ();
			}

			if (Handle != IntPtr.Zero && OwnsHandle)
				DisposeNative ();

			Handle = IntPtr.Zero;
		}

		public void Dispose ()
		{
			if (IgnoreDispose)
				return;

			DisposeInternal ();
		}

		protected void DisposeInternal ()
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
		void UnreferenceNative ();
	}
}
