using System;
using System.Collections.Concurrent;

using NonBlockingDictionary = NonBlocking.ConcurrentDictionary<System.IntPtr, System.WeakReference>;

namespace SkiaSharp
{
	internal static class HandleDictionary
	{
		private static readonly Type SkipObjectRegistrationType = typeof (ISKSkipObjectRegistration);

#if THROW_OBJECT_EXCEPTIONS
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif
		internal static readonly NonBlockingDictionary instances = new NonBlockingDictionary ();

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

			if (SkipObjectRegistrationType.IsAssignableFrom (typeof (TSkiaObject))) {
				instance = null;
				return false;
			}

			return GetInstanceNoLocks (handle, out instance);
		}

		/// <summary>
		/// Retrieve or create an instance for the native handle.
		/// </summary>
		/// <returns>The instance, or null if the handle was null.</returns>
		internal static TSkiaObject GetOrAddObject<TSkiaObject> (IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			if (handle == IntPtr.Zero)
				return null;

			if (SkipObjectRegistrationType.IsAssignableFrom (typeof (TSkiaObject))) {
#if THROW_OBJECT_EXCEPTIONS
				throw new InvalidOperationException (
					$"For some reason, the object was constructed using a factory function instead of the constructor. " +
					$"H: {handle.ToString ("x")} Type: {typeof (TSkiaObject)}");
#else
				return objectFactory.Invoke (handle, owns);
#endif
			}

			if (GetInstanceNoLocks<TSkiaObject> (handle, out var instance)) {
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

			// adding to the dictionary actually happens in the Handle set
			return objectFactory.Invoke (handle, owns);
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

			if (instance is ISKSkipObjectRegistration)
				return;

			instances.AddOrUpdate (handle, Add, Update);

			WeakReference Add (IntPtr h)
			{
				return instance.WeakHandle;
			}

			SKObject objectToDispose = null;

			WeakReference Update (IntPtr h, WeakReference oldValue)
			{
				if (oldValue.Target is SKObject obj && !obj.IsDisposed) {
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

				return instance.WeakHandle;
			}

			// dispose the object we just replaced
			objectToDispose?.DisposeInternal ();
		}

		private static readonly WeakReference emptyRef = new WeakReference (null);

		/// <summary>
		/// Removes the registered instance from the dictionary.
		/// </summary>
		internal static void DeregisterHandle (IntPtr handle, SKObject instance)
		{
			if (handle == IntPtr.Zero)
				return;

			if (instance is ISKSkipObjectRegistration)
				return;

			var existed = instances.TryUpdate (handle, emptyRef, instance.WeakHandle);
			if (existed) {
				// TODO: remove from the dictionary
				//       or, better yet, do this in a single op
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
				}
				if (ex != null) {
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
