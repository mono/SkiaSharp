using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
#if THROW_OBJECT_EXCEPTIONS
using System.Collections.Concurrent;
#endif

namespace SkiaSharp
{
	internal static class HandleDictionary
	{
		private static readonly Type SkipObjectRegistrationType = typeof (ISKSkipObjectRegistration);

#if THROW_OBJECT_EXCEPTIONS
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif
		internal static readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference> ();

		internal static readonly IPlatformLock instancesLock = PlatformLock.Create ();

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

			instancesLock.EnterUpgradeableReadLock ();
			try {
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

				var obj = objectFactory.Invoke (handle, owns);

				return obj;
			} finally {
				instancesLock.ExitUpgradeableReadLock ();
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

			if (instance is ISKSkipObjectRegistration)
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

			if (instance is ISKSkipObjectRegistration)
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


	/*
	 * This is a (hopefully) temporary fix for issue #1383.
	 * 
	 *    https://github.com/mono/SkiaSharp/issues/1383
	 * 
	 * On Windows, .NET locks are alertable when using the STA threading model and can 
	 * cause the Windows message loop to be dispatched (typically on WM_PAINT messages. 
	 * This can lead to re-entrancy and a deadlock on the HandleDictionary lock. 
	 * 
	 * This fix replaces the ReaderWriteLockSlim instance on Windows with a native Win32 
	 * CRITICAL_SECTION.
	 */

	/// <summary>
	/// Abstracts a platform dependant lock implementation
	/// </summary>
	interface IPlatformLock
	{
		void EnterReadLock ();
		void ExitReadLock ();
		void EnterWriteLock ();
		void ExitWriteLock ();
		void EnterUpgradeableReadLock ();
		void ExitUpgradeableReadLock ();
	}

	/// <summary>
	/// Helper class to create a IPlatformLock instance depending on the platform
	/// </summary>
	static class PlatformLock
	{
		public static IPlatformLock Create ()
		{
			if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows))
				return new NonAlertableWin32Lock ();
			else
				return new ReadWriteLock ();
		}
	}

	/// <summary>
	/// Non-Windows platform lock uses ReaderWriteLockSlim
	/// </summary>
	class ReadWriteLock : IPlatformLock
	{
		public void EnterReadLock () => _lock.EnterReadLock ();
		public void ExitReadLock () => _lock.ExitReadLock ();
		public void EnterWriteLock () => _lock.EnterWriteLock ();
		public void ExitWriteLock () => _lock.ExitWriteLock ();
		public void EnterUpgradeableReadLock () => _lock.EnterUpgradeableReadLock ();
		public void ExitUpgradeableReadLock () => _lock.ExitUpgradeableReadLock ();

		ReaderWriterLockSlim _lock = new ReaderWriterLockSlim ();
	}

	/// <summary>
	/// Windows platform lock uses Win32 CRITICAL_SECTION
	/// </summary>
	class NonAlertableWin32Lock : IPlatformLock
	{
		public NonAlertableWin32Lock ()
		{
			_cs = Marshal.AllocHGlobal (Marshal.SizeOf<CRITICAL_SECTION> ());
			if (_cs == IntPtr.Zero)
				throw new OutOfMemoryException ("Failed to allocate memory for critical section");

			InitializeCriticalSectionEx (_cs, 4000, 0);
		}

		~NonAlertableWin32Lock ()
		{
			if (_cs != IntPtr.Zero) {
				DeleteCriticalSection (_cs);
				Marshal.FreeHGlobal (_cs);
			}
		}

		IntPtr _cs;

		void Enter ()
		{
			EnterCriticalSection (_cs);
		}

		void Leave ()
		{
			LeaveCriticalSection (_cs);
		}

		public void EnterReadLock () { Enter (); }
		public void ExitReadLock () { Leave (); }
		public void EnterWriteLock () { Enter (); }
		public void ExitWriteLock () { Leave (); }
		public void EnterUpgradeableReadLock () { Enter (); }
		public void ExitUpgradeableReadLock () { Leave (); }

		[StructLayout (LayoutKind.Sequential)]
		public struct CRITICAL_SECTION
		{
			public IntPtr DebugInfo;
			public int LockCount;
			public int RecursionCount;
			public IntPtr OwningThread;
			public IntPtr LockSemaphore;
			public UIntPtr SpinCount;
		}

		[DllImport ("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs (UnmanagedType.Bool)]
		static extern bool InitializeCriticalSectionEx (IntPtr lpCriticalSection, uint dwSpinCount, uint Flags);
		[DllImport ("Kernel32.dll")]
		static extern void DeleteCriticalSection (IntPtr lpCriticalSection);
		[DllImport ("Kernel32.dll")]
		static extern void EnterCriticalSection (IntPtr lpCriticalSection);
		[DllImport ("Kernel32.dll")]
		static extern void LeaveCriticalSection (IntPtr lpCriticalSection);
	}

}
