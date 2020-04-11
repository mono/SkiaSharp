using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace SkiaSharp
{
	internal static class HandleDictionary
	{
		private static readonly Type IntPtrType = typeof (IntPtr);
		private static readonly Type BoolType = typeof (bool);

#if THROW_OBJECT_EXCEPTIONS
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif

		internal static readonly ConcurrentDictionary<Type, ConstructorInfo> constructors = new ConcurrentDictionary<Type, ConstructorInfo> ();
		internal static readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference> ();

		internal static readonly ReaderWriterLockSlim instancesLock = new ReaderWriterLockSlim ();

		/// <summary>
		/// Retrieve the living instance if there is one, or null if not.
		/// </summary>
		/// <returns>The instance if it is alive, or null if there is none.</returns>
		internal static bool GetInstance<TSkiaObject> (IntPtr handle, out TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero) {
				instance = null;
				return false;
			}

			instancesLock.EnterReadLock ();
			try {
				return GetInstanceNoLocks (handle, out instance);
			} finally {
				instancesLock.ExitReadLock ();
			}
		}

		/// <summary>
		/// Retrieve or create an instance for the native handle.
		/// </summary>
		/// <returns>The instance, or null if the handle was null.</returns>
		internal static TSkiaObject GetObject<TSkiaObject, TSkiaImplementation> (IntPtr handle, bool owns, bool unrefExisting)
			where TSkiaObject : SKObject
			where TSkiaImplementation : SKObject, TSkiaObject
		{
			if (handle == IntPtr.Zero)
				return null;

			instancesLock.EnterUpgradeableReadLock ();
			try {
				if (GetInstanceNoLocks<TSkiaObject> (handle, out var instance)) {
					// some object get automatically referenced on the native side,
					// but managed code just has the same reference
					if (unrefExisting && instance is ISKReferenceCounted refcnt && instance.OwnsHandle) {
#if THROW_OBJECT_EXCEPTIONS
						var count = refcnt.GetReferenceCount ();
						if (count <= 1)
							throw new InvalidOperationException (
								$"About to unreference an object that has no references. " +
								$"H: {handle.ToString ("x")} Type: {instance.GetType ()}");
#endif
						refcnt.SafeUnRef ();
					}

					return instance;
				}

				var type = typeof (TSkiaImplementation);
				var constructor = constructors.GetOrAdd (type, t => GetConstructor (t));

				// we don't need to go into a writable here as the object will do it in the Handle property
				var obj = (TSkiaObject)constructor.Invoke (new object[] { handle, owns });
				return obj;
			} finally {
				instancesLock.ExitUpgradeableReadLock ();
			}

			static ConstructorInfo GetConstructor (Type type)
			{
				var ctors = type.GetTypeInfo ().DeclaredConstructors;

				foreach (var ctor in ctors) {
					var param = ctor.GetParameters ();
					if (param.Length == 2 && param[0].ParameterType == IntPtrType && param[1].ParameterType == BoolType)
						return ctor;
				}

				throw new MissingMethodException ($"No constructor found for {type.FullName}.ctor(System.IntPtr, System.Boolean)");
			}
		}

		/// <summary>
		/// Retrieve the living instance if there is one, or null if not. This does not use locks.
		/// </summary>
		/// <returns>The instance if it is alive, or null if there is none.</returns>
		private static bool GetInstanceNoLocks<TSkiaObject> (IntPtr handle, out TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (instances.TryGetValue (handle, out var weak) && weak.IsAlive) {
				if (weak.Target is TSkiaObject match) {
					if (!match.IsDisposed) {
						instance = match;
						return true;
					}
#if THROW_OBJECT_EXCEPTIONS
				} else if (weak.Target is SKObject obj) {
					if (!obj.IsDisposed && obj.OwnsHandle) {
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

		/// <summary>
		/// Registers the specified instance with the dictionary.
		/// </summary>
		internal static void RegisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero || instance == null)
				return;

			SKObject objectToDispose = null;

			instancesLock.EnterWriteLock ();
			try {
				if (instances.TryGetValue (handle, out var oldValue) && oldValue.Target is SKObject obj && !obj.IsDisposed) {
#if THROW_OBJECT_EXCEPTIONS
					if (obj.OwnsHandle) {
						// a mostly recoverable error
						// if there is a managed object, then maybe something happened and the native object is dead
						throw new InvalidOperationException (
							$"A managed object already exists for the specified native object. " +
							$"H: {handle.ToString ("x")} Type: ({obj.GetType ()}, {instance.GetType ()})");
					}
#endif
					// this means the ownership was handed off to a native object, so clean up the managed side
					objectToDispose = obj;
				}

				instances[handle] = new WeakReference (instance);
			} finally {
				instancesLock.ExitWriteLock ();
			}

			// dispose the object we just replaced
			objectToDispose?.DisposeInternal ();
		}

		/// <summary>
		/// Removes the registered instance from the dictionary.
		/// </summary>
		internal static void DeregisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
				return;

			instancesLock.EnterWriteLock ();
			try {
				var existed = instances.TryGetValue (handle, out var weak);
				if (existed && (!weak.IsAlive || weak.Target == instance)) {
					instances.Remove (handle);
				} else {
#if THROW_OBJECT_EXCEPTIONS
					InvalidOperationException ex = null;
					if (!existed) {
						// the object may have been replaced

						if (!instance.IsDisposed) {
							// recoverable error
							// there was no object there, but we are still alive
							ex = new InvalidOperationException (
								$"A managed object did not exist for the specified native object. " +
								$"H: {handle.ToString ("x")} Type: {instance.GetType ()}");
						}
					} else if (weak.Target is SKObject o && o != instance) {
						// there was an object in the dictionary, but it was NOT this object

						if (!instance.IsDisposed) {
							// recoverable error
							// there was a new living object there, but we are still alive
							ex = new InvalidOperationException (
								$"Trying to remove a different object with the same native handle. " +
								$"H: {handle.ToString ("x")} Type: ({o.GetType ()}, {instance.GetType ()})");
						}
					}
					if (ex != null) {
						if (instance.fromFinalizer)
							exceptions.Add (ex);
						else
							throw ex;
					}
#endif
				}
			} finally {
				instancesLock.ExitWriteLock ();
			}
		}
	}
}
