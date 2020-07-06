#nullable enable

using System;
using System.Collections.Concurrent;

//using NonBlockingDictionary = NonBlocking.ConcurrentDictionary<System.IntPtr, System.WeakReference>;

namespace SkiaSharp
{
	internal static class HandleDictionary
	{
		private static readonly Type SkipObjectRegistrationType = typeof (ISKSkipObjectRegistration);

		private static readonly WeakReference emptyRef = new WeakReference (null);

#if THROW_OBJECT_EXCEPTIONS
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif
		internal static readonly ConcurrentDictionary<System.IntPtr, System.WeakReference> instances = new ConcurrentDictionary<System.IntPtr, System.WeakReference> ();

		/// <summary>
		/// Retrieve the living instance if there is one, or null if not.
		/// </summary>
		/// <returns>The instance if it is alive, or null if there is none.</returns>
		internal static bool GetInstance<TSkiaObject> (IntPtr handle, out TSkiaObject? instance)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero) {
				instance = null;
				return false;
			}

			if (SkipObjectRegistrationType.IsAssignableFrom (typeof (TSkiaObject))) {
				//#if THROW_OBJECT_EXCEPTIONS
				//				throw new InvalidOperationException (
				//					$"For some reason, the object was constructed using a factory instead of the constructor. " +
				//					$"H: {handle.ToString ("x")} Type: {typeof (TSkiaObject)}");
				//#else
				instance = null;
				return false;
				//#endif
			}

			if (!instances.TryGetValue (handle, out var weak)) {
				instance = null;
				return false;
			}

			return TryGetTarget (weak, handle, out instance);
		}

		internal static bool TryGetTarget<TSkiaObject> (WeakReference? weak, IntPtr handle, out TSkiaObject? instance)
			where TSkiaObject : SKObject
		{
			if (weak != null) {
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
		/// Retrieve or create an instance for the native handle.
		/// </summary>
		/// <returns>The instance, or null if the handle was null.</returns>
		internal static TSkiaObject? GetOrAddObject<TSkiaObject> (IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return null;

			if (SkipObjectRegistrationType.IsAssignableFrom (typeof (TSkiaObject))) {
#if THROW_OBJECT_EXCEPTIONS
				throw new InvalidOperationException (
					$"For some reason, the object was constructed using a factory instead of the constructor. " +
					$"H: {handle.ToString ("x")} Type: {typeof (TSkiaObject)}");
#else
				Create (out var obj);
				return obj;
#endif
			}

			SKObject? objectToDispose = null;
			TSkiaObject? instance = null;

			var newWeakHandle = instances.AddOrUpdate (handle, _ => Create (out instance), (_, oldReference) => {
				// check the existing object and see if we can use it
				if (TryGetTarget<TSkiaObject> (oldReference, handle, out var oldInstance)) {
					// some objects get automatically referenced on the native side,
					// but managed code just has the same reference
					if (unrefExisting && oldInstance is ISKReferenceCounted refcnt) {
#if THROW_OBJECT_EXCEPTIONS
						if (refcnt.GetReferenceCount () == 1) {
							throw new InvalidOperationException (
								$"About to unreference an object that has no references. " +
								$"H: {handle.ToString ("x")} Type: {oldInstance.GetType ()}");
						}
#endif
						refcnt.SafeUnRef ();
					}

					// we found the object we were looking for, so jump out now
					instance = oldInstance!;
					return oldReference;
				}

				// ownership was handed off to a native object, so clean up the managed side
				if (oldReference.Target is SKObject obj && !obj.IsDisposed)
					objectToDispose = obj;

				// there was no valid object, so create
				return Create (out instance);
			});

			objectToDispose?.DisposeInternal ();

#if THROW_OBJECT_EXCEPTIONS
			if (newWeakHandle == null) {
				throw new InvalidOperationException (
					$"For some reason, the factory returned an unexpected object. " +
					$"H: {handle.ToString ("x")} Type: ({typeof (TSkiaObject)})");
			} else if (!newWeakHandle.IsAlive || newWeakHandle?.Target == null) {
				throw new InvalidOperationException (
					$"For some reason, the factory returned a null object. " +
					$"H: {handle.ToString ("x")} Type: ({typeof (TSkiaObject)})");
			}

			var hasTarget = TryGetTarget<TSkiaObject> (newWeakHandle, handle, out var newInstance);
			if (!hasTarget) {
				throw new InvalidOperationException (
					$"For some reason, the weak reference did not have a target object. " +
					$"H: {handle.ToString ("x")} Type: ({typeof (TSkiaObject)})");
			} else if (newInstance != instance) {
				throw new InvalidOperationException (
					$"For some reason, the weak reference has a different object than what was just created. " +
					$"H: {handle.ToString ("x")} Type: ({typeof (TSkiaObject)})");
			}
#endif

			return instance;

			WeakReference Create (out TSkiaObject obj)
			{
				obj = objectFactory.Invoke (handle, owns);

#if THROW_OBJECT_EXCEPTIONS
				if (obj == null) {
					throw new InvalidOperationException (
						$"For some reason, the factory returned null. " +
						$"H: {handle.ToString ("x")} Type: ({typeof (TSkiaObject)})");
				}
#endif

				return obj.WeakRef;
			}
		}

		/// <summary>
		/// Registers the specified instance with the dictionary.
		/// </summary>
		internal static void RegisterHandle<TSkiaObject> (IntPtr handle, TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return;

			if (instance is ISKSkipObjectRegistration)
				return;

			SKObject? objectToDispose = null;

			instances.AddOrUpdate (handle, _ => instance.WeakRef, (_, oldReference) => {
				if (oldReference.Target is SKObject obj && !obj.IsDisposed) {
#if THROW_OBJECT_EXCEPTIONS
					if (obj.OwnsHandle) {
						// a mostly recoverable error
						// if there is a managed object, then maybe something happened and the native object is dead
						throw new InvalidOperationException (
							$"A managed object already exists for the specified native object. " +
							$"H: {handle.ToString ("x")} Type: ({obj.GetType ()}, {instance.GetType ()})");
					}
#endif
					// ownership was handed off to a native object, so clean up the managed side
					objectToDispose = obj;
				}

				// there was no valid object, so add
				return instance.WeakRef;
			});

			objectToDispose?.DisposeInternal ();
		}

		/// <summary>
		/// Removes the registered instance from the dictionary.
		/// </summary>
		internal static void DeregisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
				return;

			if (instance is ISKSkipObjectRegistration)
				return;

			var existed = instances.TryUpdate (handle, emptyRef, instance.WeakRef);
			if (existed) {
				// TODO: remove from the dictionary or, better yet, do this in a single op
			} else {
#if THROW_OBJECT_EXCEPTIONS
				// the object may have been replaced
				if (!instance.IsDisposed) {
					// recoverable error
					// there was no object there, but we are still alive
					var ex = new InvalidOperationException (
						$"A managed object did not exist for the specified native object. " +
						$"H: {handle.ToString ("x")} Type: {instance.GetType ()}");
					if (instance.fromFinalizer)
						exceptions.Add (ex);
					else
						throw ex;
				}
#endif
			}
		}
	}
}
