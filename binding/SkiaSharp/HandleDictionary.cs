#nullable disable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
#if THROW_OBJECT_EXCEPTIONS
using System.Collections.Concurrent;
#endif
using SkiaSharp.Internals;

namespace SkiaSharp
{
	// EXPERIMENT (issue #4101): lock striping for the handle registry.
	//
	// The registry is partitioned into ShardCount independently-locked shards. The
	// shard for a handle is a bit-mixed hash of the handle value. Every public
	// operation here is keyed on a SINGLE handle, so an object's handle always maps
	// to exactly one shard; its GetOrAddObject (upgradeable/write) and its public
	// Dispose (write, via GetLockFor) therefore contend on the SAME shard lock. This
	// preserves the promote/dispose mutual-exclusion invariant of the original
	// single-lock design while letting unrelated handles proceed in parallel.
	//
	// ShardCount == 1 reduces EXACTLY to the original single-lock behavior.
	//
	// KNOWN OPEN HAZARD (the un-drafting blocker, see #4101): GetOrAddObject invokes
	// the object factory WHILE holding a shard lock. If a wrapper constructor were to
	// construct and register a DIFFERENT-handle SKObject, a second shard lock would be
	// acquired while the first is held — a cross-shard lock-ordering risk that cannot
	// occur with a single global lock. This must be resolved before this is shippable.
	internal static class HandleDictionary
	{
		private static readonly Type SkipObjectRegistrationType = typeof (ISKSkipObjectRegistration);

#if THROW_OBJECT_EXCEPTIONS
		// One process-wide bag (already concurrent); not part of the striped state.
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif

		private sealed class Shard
		{
			public readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference> ();
#if DEBUG
			public readonly Dictionary<IntPtr, string> stackTraces = new Dictionary<IntPtr, string> ();
#endif
			public readonly IPlatformLock instancesLock = PlatformLock.Create ();
		}

		// Resolved once at first use. Power of two so the shard index is hash & (N-1).
		internal static readonly int ShardCount = ResolveShardCount ();
		private static readonly int shardMask = ShardCount - 1;
		private static readonly Shard[] shards = CreateShards (ShardCount);

		private static Shard[] CreateShards (int count)
		{
			var arr = new Shard[count];
			for (var i = 0; i < count; i++)
				arr[i] = new Shard ();
			return arr;
		}

		private static int ResolveShardCount ()
		{
			var configured = ReadConfiguredShardCount ();
			var n = configured > 0 ? configured : Environment.ProcessorCount;
			if (n < 1)
				n = 1;
			if (n > 64)
				n = 64;

			// round up to the next power of two
			var p = 1;
			while (p < n)
				p <<= 1;
			return p;
		}

		private static int ReadConfiguredShardCount ()
		{
			// Additive, ABI-clean override resolved once at first use (same constraint as
			// PlatformLock.Factory): AppContext data first, then environment variable.
			try {
				var data = AppContext.GetData ("SkiaSharp.HandleDictionary.ShardCount");
				if (data is int i && i > 0)
					return i;
				if (data is string ds && int.TryParse (ds, out var dv) && dv > 0)
					return dv;
			} catch {
				// ignore and fall through to env / auto
			}

			try {
				var env = Environment.GetEnvironmentVariable ("SKIASHARP_HANDLE_SHARDS");
				if (!string.IsNullOrEmpty (env) && int.TryParse (env, out var ev) && ev > 0)
					return ev;
			} catch {
				// ignore and fall through to auto
			}

			return 0;
		}

		private static Shard ShardFor (IntPtr handle)
		{
			if (shardMask == 0)
				return shards[0];

			// Native handles are aligned pointers, so their low bits are always zero.
			// Multiplicative (Fibonacci) mixing on the 64-bit value spreads them before
			// we mask the high word with (N-1).
			var mixed = unchecked ((ulong) handle.ToInt64 () * 0x9E3779B97F4A7C15UL);
			var idx = (int) (mixed >> 32) & shardMask;
			return shards[idx];
		}

		/// <summary>
		/// The shard lock that protects a given handle. Used by <see cref="SKObject.Dispose()"/>
		/// so its IgnorePublicDispose check + isDisposed claim pair against the same lock that
		/// GetOrAddObject holds for that handle.
		/// </summary>
		internal static IPlatformLock GetLockFor (IntPtr handle) => ShardFor (handle).instancesLock;

		/// <summary>
		/// Aggregating snapshot of every registered instance across all shards. Test/diagnostic
		/// use only (e.g. GarbageCleanupFixture); not used on any hot path.
		/// </summary>
		internal static Dictionary<IntPtr, WeakReference> instances {
			get {
				var all = new Dictionary<IntPtr, WeakReference> ();
				foreach (var shard in shards) {
					shard.instancesLock.EnterReadLock ();
					try {
						foreach (var kv in shard.instances)
							all[kv.Key] = kv.Value;
					} finally {
						shard.instancesLock.ExitReadLock ();
					}
				}
				return all;
			}
		}

#if DEBUG
		/// <summary>
		/// Aggregating snapshot of registration stack traces across all shards. DEBUG/diagnostic only.
		/// </summary>
		internal static Dictionary<IntPtr, string> stackTraces {
			get {
				var all = new Dictionary<IntPtr, string> ();
				foreach (var shard in shards) {
					shard.instancesLock.EnterReadLock ();
					try {
						foreach (var kv in shard.stackTraces)
							all[kv.Key] = kv.Value;
					} finally {
						shard.instancesLock.ExitReadLock ();
					}
				}
				return all;
			}
		}
#endif

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

			var shard = ShardFor (handle);
			shard.instancesLock.EnterReadLock ();
			try {
				return GetInstanceNoLocks (shard, handle, out instance);
			} finally {
				shard.instancesLock.ExitReadLock ();
			}
		}

		/// <summary>
		/// Retrieve or create an instance for the native handle.
		/// </summary>
		/// <returns>The instance, or null if the handle was null.</returns>
		internal static TSkiaObject GetOrAddObject<TSkiaObject> (IntPtr handle, bool owns, bool unrefExisting, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject =>
			GetOrAddObject (handle, owns, unrefExisting, disposeProtected: false, objectFactory);

		/// <summary>
		/// Retrieve or create an instance for the native handle. When <paramref name="disposeProtected"/> is true,
		/// IgnorePublicDispose is set via PreventPublicDisposal on the wrapper that is returned (whether an
		/// existing one was found or a new one was created).
		/// This is safe because this method holds the shard's upgradeable-read lock for its whole duration, which is
		/// mutually exclusive with the write lock public Dispose() holds (on the same shard) around its
		/// IgnorePublicDispose check — so the flag set cannot race a concurrent public disposal.
		/// (PreventPublicDisposal itself takes no lock.)
		/// </summary>
		/// <returns>The instance, or null if the handle was null.</returns>
		internal static TSkiaObject GetOrAddObject<TSkiaObject> (IntPtr handle, bool owns, bool unrefExisting, bool disposeProtected, Func<IntPtr, bool, TSkiaObject> objectFactory)
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

			var shard = ShardFor (handle);
			shard.instancesLock.EnterUpgradeableReadLock ();
			try {
				if (GetInstanceNoLocks<TSkiaObject> (shard, handle, out var instance)) {
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

					if (disposeProtected)
						// Safe against a concurrent PUBLIC Dispose: it holds this shard's write lock, which is
						// mutually exclusive with the upgradeable-read lock held here. Internal Dispose
						// paths don't affect the flag's purpose, and no dispose-protected target can be
						// internally disposed concurrently either (see PreventPublicDisposal's guard).
						instance.PreventPublicDisposal ();

					return instance;
				}

				// NOTE: the factory runs under this shard's lock. Registering the SAME handle (the
				// normal case) re-enters this same shard lock on the same thread (upgradeable->write,
				// permitted). Registering a DIFFERENT handle here would take a second shard lock — the
				// cross-shard hazard documented at the top of this file.
				var obj = objectFactory.Invoke (handle, owns);

				// Cannot race with a concurrent public Dispose call. same reasoning as above.
				if (disposeProtected && obj is not null)
					obj.PreventPublicDisposal ();

				return obj;
			} finally {
				shard.instancesLock.ExitUpgradeableReadLock ();
			}
		}

		/// <summary>
		/// Retrieve the living instance if there is one, or null if not. This does not use locks.
		/// </summary>
		/// <returns>The instance if it is alive, or null if there is none.</returns>
		private static bool GetInstanceNoLocks<TSkiaObject> (Shard shard, IntPtr handle, out TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (shard.instances.TryGetValue (handle, out var weak) && weak.IsAlive) {
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

			var shard = ShardFor (handle);
			shard.instancesLock.EnterWriteLock ();
			try {
				if (shard.instances.TryGetValue (handle, out var oldValue) && oldValue.Target is SKObject obj && !obj.IsDisposed) {
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

				shard.instances[handle] = new WeakReference (instance);
#if DEBUG
				shard.stackTraces[handle] = Environment.StackTrace;
#endif
			} finally {
				shard.instancesLock.ExitWriteLock ();
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

			var shard = ShardFor (handle);
			shard.instancesLock.EnterWriteLock ();
			try {
				var existed = shard.instances.TryGetValue (handle, out var weak);
				if (existed && (!weak.IsAlive || weak.Target == instance)) {
					shard.instances.Remove (handle);
#if DEBUG
					shard.stackTraces.Remove (handle);
#endif
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
				shard.instancesLock.ExitWriteLock ();
			}
		}
	}



}
