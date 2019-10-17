﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SkiaSharp
{
	public abstract class SKObject : SKNativeObject
	{
#if THROW_OBJECT_EXCEPTIONS
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif

		internal static readonly ConcurrentDictionary<Type, ConstructorInfo> constructors;
		internal static readonly ConcurrentDictionary<IntPtr, WeakReference> instances;

		internal readonly ConcurrentDictionary<IntPtr, SKObject> ownedObjects = new ConcurrentDictionary<IntPtr, SKObject> ();

		static SKObject ()
		{
			constructors = new ConcurrentDictionary<Type, ConstructorInfo> ();
			instances = new ConcurrentDictionary<IntPtr, WeakReference> ();

			SKColorSpace.EnsureStaticInstanceAreInitialized ();
			SKData.EnsureStaticInstanceAreInitialized ();
			SKFontManager.EnsureStaticInstanceAreInitialized ();
			SKTypeface.EnsureStaticInstanceAreInitialized ();
		}

		[Preserve]
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
			foreach (var child in ownedObjects) {
				child.Value.DisposeInternal ();
			}
			ownedObjects.Clear ();
		}

		protected override void DisposeNative ()
		{
			if (this is ISKReferenceCounted refcnt)
				refcnt.SafeUnRef ();
		}

		internal static TSkiaObject GetObject<TSkiaObject> (IntPtr handle, bool owns = true, bool unrefExisting = true, bool refNew = false)
			where TSkiaObject : SKObject
		{
			return GetObject<TSkiaObject, TSkiaObject> (handle, owns, unrefExisting, refNew);
		}

		internal static TSkiaObject GetObject<TSkiaObject, TSkiaImplementation> (IntPtr handle, bool owns = true, bool unrefExisting = true, bool refNew = false)
			where TSkiaObject : SKObject
			where TSkiaImplementation : SKObject, TSkiaObject
		{
			if (handle == IntPtr.Zero)
				return null;

			if (GetInstance<TSkiaObject> (handle, out var instance)) {
				// some object get automatically referenced on the native side,
				// but managed code just has the same reference
				if (unrefExisting && instance is ISKReferenceCounted refcnt) {
#if THROW_OBJECT_EXCEPTIONS
					if (refcnt.GetReferenceCount () == 1)
						throw new InvalidOperationException (
							$"About to unreference an object that has no references. " +
							$"H: {handle.ToString ("x")} Type: {instance.GetType ()}");
#endif
					refcnt.SafeUnRef ();
				}

				return instance;
			}

			var type = typeof (TSkiaImplementation);
			var constructor = constructors.GetOrAdd (type, t => {
				var ctor = type.GetTypeInfo ().DeclaredConstructors.FirstOrDefault (c => {
					var parameters = c.GetParameters ();
					return
						parameters.Length == 2 &&
						parameters[0].ParameterType == typeof (IntPtr) &&
						parameters[1].ParameterType == typeof (bool);
				});
				return ctor ?? throw new MissingMethodException ($"No constructor found for {type.FullName}.ctor(System.IntPtr, System.Boolean)");
			});

			var obj = (TSkiaObject)constructor.Invoke (new object[] { handle, owns });
			if (refNew && obj is ISKReferenceCounted toRef)
				toRef.SafeRef ();
			return obj;
		}

		internal static void RegisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero || instance == null)
				return;

			var weak = new WeakReference (instance);
			instances.AddOrUpdate (handle, weak, Update);

			WeakReference Update (IntPtr key, WeakReference oldValue)
			{
				if (oldValue.Target is SKObject obj && !obj.IsDisposed) {
#if THROW_OBJECT_EXCEPTIONS
					if (obj.OwnsHandle)
						throw new InvalidOperationException (
							$"A managed object already exists for the specified native object. " +
							$"H: {handle.ToString ("x")} Type: ({obj.GetType ()}, {instance.GetType ()})");
#endif

					obj.DisposeInternal ();
				}

				return weak;
			}
		}

		internal static void DeregisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
				return;

			var nonExistent = new WeakReference (null);
			var weak = instances.AddOrUpdate (handle, nonExistent, (ptr, old) => {
				if (old.Target is SKObject obj && !obj.IsDisposed)
					return old;
				return new WeakReference (null);
			});

#if THROW_OBJECT_EXCEPTIONS
			InvalidOperationException ex = null;
			if (weak == nonExistent) {
				// the dummy instance was added instead of being removed
				ex = new InvalidOperationException (
					$"A managed object did not exist for the specified native object. " +
					$"H: {handle.ToString ("x")} Type: {instance.GetType ()}");
			}
			if (weak.Target is SKObject o && o != instance && !instance.IsDisposed) {
				// there was a new living object there, but we are still alive
				ex = new InvalidOperationException (
					$"Trying to remove a different object with the same native handle. " +
					$"H: {handle.ToString ("x")} Type: ({o.GetType ()}, {instance.GetType ()})");
			}
			if (ex != null) {
				if (instance.fromFinalizer)
					exceptions.Add (ex);
				else
					throw ex;
			}
#endif
		}

		internal static bool GetInstance<TSkiaObject> (IntPtr handle, out TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (instances.TryGetValue (handle, out var weak)) {
				if (weak.Target is TSkiaObject match) {
					if (!match.IsDisposed) {
						instance = match;
						return true;
					}
#if THROW_OBJECT_EXCEPTIONS
				} else if (weak.Target is SKObject obj) {
					if (!obj.IsDisposed) {
						throw new InvalidOperationException (
							$"A managed object exists for the handle, but is not the expected type. " +
							$"H: {handle.ToString ("x")} Type: ({obj.GetType ()}, {typeof (TSkiaObject)})");
					}
				} else if (weak.Target is object o) {
					throw new InvalidOperationException (
						$"An unknown object exists for the handle when trying to fetch an instance. " +
						$"H: {handle.ToString ("x")} Type: ({o.GetType ()}, {typeof (TSkiaObject)})");
#endif
				}
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
			IgnorePublicDispose = true;

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
#if THROW_OBJECT_EXCEPTIONS
		internal bool fromFinalizer = false;
#endif

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
#if THROW_OBJECT_EXCEPTIONS
			fromFinalizer = true;
#endif

			Dispose (false);
		}

		public virtual IntPtr Handle { get; protected set; }

		protected internal virtual bool OwnsHandle { get; protected set; }

		protected internal bool IgnorePublicDispose { get; protected set; }

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

			if (disposing) {
				DisposeManaged ();
			}

			if (Handle != IntPtr.Zero && OwnsHandle)
				DisposeNative ();

			Handle = IntPtr.Zero;
		}

		public void Dispose ()
		{
			if (IgnorePublicDispose)
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
}
