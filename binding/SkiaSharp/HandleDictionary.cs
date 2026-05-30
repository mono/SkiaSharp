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
	// EXPERIMENT (issue #4101): lock striping for the handle registry, with two-phase
	// (reservation-gate) construction so the object factory NEVER runs under a shard lock.
	//
	// The registry is partitioned into ShardCount independently-locked shards. The shard for a
	// handle is a bit-mixed hash of the handle value. Every public operation here is keyed on a
	// SINGLE handle, so an object's handle always maps to exactly one shard; its registration and
	// its public Dispose (write, via GetLockFor) contend on the SAME shard lock. This preserves the
	// promote/dispose mutual-exclusion invariant of the original single-lock design while letting
	// unrelated handles proceed in parallel. ShardCount == 1 reduces to single-lock behavior.
	//
	// Two-phase construction (removes the cross-shard deadlock hazard):
	//   Phase 1 (under shard lock, O(1)): dedup against a live instance; else if another thread is
	//           already constructing this handle, wait on its per-handle gate and retry; else install
	//           our own Reservation and release the lock.
	//   Phase 2 (NO lock held): run the object factory. The wrapper ctor's RegisterHandle sees our
	//           reservation and SUPPRESSES publication (so a half-built wrapper is never visible).
	//   Phase 3 (under shard lock, O(1)): publish the finished wrapper into "instances", drop the
	//           reservation, then signal the gate so waiters observe only the completed object.
	//
	// Because no shard lock is ever held across the factory, a thread holds at most ONE shard lock at
	// a time, so nested different-handle construction cannot form a cross-shard AB-BA cycle. The only
	// residual hazards are (a) re-entrant same-thread same-handle construction (detected and thrown,
	// never hung) and (b) a genuine cross-object construction-time data-dependency cycle between two
	// threads (an application bug, far rarer than the structural hazard this removes).
	//
	// "instances" only ever holds fully-constructed wrappers; in-flight handles live in the separate
	// "reservations" map. So GetInstance/GetInstanceNoLocks/DeregisterHandle and the aggregating
	// diagnostics need no awareness of reservations.
	internal static class HandleDictionary
	{
		private static readonly Type SkipObjectRegistrationType = typeof (ISKSkipObjectRegistration);

#if THROW_OBJECT_EXCEPTIONS
		// One process-wide bag (already concurrent); not part of the striped state.
		internal static readonly ConcurrentBag<Exception> exceptions = new ConcurrentBag<Exception> ();
#endif

		// An in-flight construction for a single handle. Concurrent same-handle callers wait on the
		// gate; the owning thread publishes the finished wrapper and signals it. ownerThreadId is set
		// at creation and read under the owning shard's instancesLock.
		private sealed class Reservation
		{
			public readonly int ownerThreadId = Environment.CurrentManagedThreadId;
			public readonly ManualResetEventSlim gate = new ManualResetEventSlim (false);

			// Set true (before the gate is signaled) once the owner is completely finished with this
			// reservation, whether it published a wrapper or failed. If the owner could NOT remove the
			// reservation from its shard map on the failure path (e.g. re-acquiring the publish write
			// lock threw), a waiter that later observes this still-present-but-retired reservation takes
			// it over and reconstructs, instead of livelocking on an already-signaled gate.
			public volatile bool retired;
		}

		private sealed class Shard
		{
			public readonly Dictionary<IntPtr, WeakReference> instances = new Dictionary<IntPtr, WeakReference> ();
			// Handles currently mid-construction. A reservation masks the handle from dedup until
			// GetOrAddObject Phase 3 publishes the finished wrapper into "instances". Guarded by instancesLock.
			public readonly Dictionary<IntPtr, Reservation> reservations = new Dictionary<IntPtr, Reservation> ();
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
#if !NETFRAMEWORK
			try {
				var data = AppContext.GetData ("SkiaSharp.HandleDictionary.ShardCount");
				if (data is int i && i > 0)
					return i;
				if (data is string ds && int.TryParse (ds, out var dv) && dv > 0)
					return dv;
			} catch {
				// ignore and fall through to env / auto
			}
#endif

			try {
				var env = Environment.GetEnvironmentVariable ("SKIASHARP_HANDLE_SHARDS");
				if (!string.IsNullOrEmpty (env) && int.TryParse (env, out var ev) && ev > 0)
					return ev;
			} catch {
				// ignore and fall through to auto
			}

			return 0;
		}

		private static Shard ShardFor (IntPtr handle) => shards[ShardIndexFor (handle, shardMask)];

		/// <summary>
		/// Maps a handle to a shard index in [0, mask]. Pure function of (handle, mask) so the
		/// routing can be verified in isolation. When mask == 0 (ShardCount == 1) every handle
		/// collapses to shard 0, i.e. byte-for-byte the original single-lock/single-dictionary design.
		/// </summary>
		internal static int ShardIndexFor (IntPtr handle, int mask)
		{
			if (mask == 0)
				return 0;

			// Native handles are aligned pointers, so their low bits are always zero.
			// Multiplicative (Fibonacci) mixing on the 64-bit value spreads them before
			// we mask the high word with (N-1).
			var mixed = unchecked ((ulong) handle.ToInt64 () * 0x9E3779B97F4A7C15UL);
			return (int) (mixed >> 32) & mask;
		}

		/// <summary>
		/// The shard lock that protects a given handle. Used by SKObject.Dispose()
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
		/// This is safe because the flag is set while this method holds the shard lock (the upgradeable-read
		/// lock for an existing instance, or the Phase 3 write lock for a freshly constructed one), which is
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

			while (true) {
				Reservation mine = null;
				Reservation waitFor = null;

				shard.instancesLock.EnterUpgradeableReadLock ();
				try {
					// Phase 1a: dedup against a fully-constructed, living instance.
					if (GetInstanceNoLocks<TSkiaObject> (shard, handle, out var instance)) {
						// some objects get automatically referenced on the native side,
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
							// Safe against a concurrent PUBLIC Dispose: it holds this shard's write lock,
							// mutually exclusive with the upgradeable-read lock held here.
							instance.PreventPublicDisposal ();

						return instance;
					}

					// Phase 1b: is another thread already constructing this handle?
					if (shard.reservations.TryGetValue (handle, out var existing)) {
						if (existing.ownerThreadId == Environment.CurrentManagedThreadId) {
							// Re-entrant same-thread, same-handle construction (e.g. a native callback
							// during the factory re-enters GetOrAddObject for the same handle). Waiting on
							// our own gate would self-deadlock, so fail fast instead of hanging.
							throw new InvalidOperationException (
								$"Re-entrant construction of the same handle on the same thread. " +
								$"H: {handle.ToString ("x")} Type: {typeof (TSkiaObject)}");
						}

						if (existing.retired) {
							// The owner finished with this reservation but could not remove it from the map
							// (its publish-phase write-lock acquisition threw). Nothing was published (Phase
							// 1a above already checked), so take the stale reservation over and reconstruct
							// rather than waiting on its already-signaled gate (which would busy-livelock).
							mine = new Reservation ();
							shard.instancesLock.EnterWriteLock ();
							try {
								shard.reservations[handle] = mine;
							} finally {
								shard.instancesLock.ExitWriteLock ();
							}
						} else {
							// Another thread owns it: wait outside the lock, then retry from the top.
							waitFor = existing;
						}
					} else {
						// Phase 1c: claim the handle so concurrent callers wait instead of double-constructing.
						mine = new Reservation ();
						shard.instancesLock.EnterWriteLock ();
						try {
							shard.reservations[handle] = mine;
						} finally {
							shard.instancesLock.ExitWriteLock ();
						}
					}
				} finally {
					shard.instancesLock.ExitUpgradeableReadLock ();
				}

				// Phase 2 + 3 run with NO shard lock held across the factory.
				if (mine != null)
					return ConstructAndPublish (shard, handle, owns, disposeProtected, mine, objectFactory);

				// Block (holding no shard lock) until the owner finishes, then retry from the top.
				waitFor.gate.Wait ();
			}
		}

		// Phase 2 (construct outside the lock) + Phase 3 (publish under the lock, then signal). Kept in
		// its own method so the shard lock acquired in Phase 1 is fully released before the factory runs.
		private static TSkiaObject ConstructAndPublish<TSkiaObject> (Shard shard, IntPtr handle, bool owns, bool disposeProtected, Reservation reservation, Func<IntPtr, bool, TSkiaObject> objectFactory)
			where TSkiaObject : SKObject
		{
			TSkiaObject obj = null;
			var completed = false;
			try {
				// Phase 2: construct WITHOUT holding any shard lock. The wrapper ctor's Handle setter calls
				// RegisterHandle, which sees this reservation (same thread, same handle) and suppresses
				// publication so a half-built wrapper is never exposed to other threads.
				obj = objectFactory.Invoke (handle, owns);
				completed = true;
			} finally {
				try {
					// Phase 3: publish the finished wrapper (or just clear the reservation on failure),
					// under this shard's write lock.
					shard.instancesLock.EnterWriteLock ();
					try {
						shard.reservations.Remove (handle);

						// Only publish a wrapper the factory FULLY constructed. If the factory threw
						// (completed == false) or returned null, leave "instances" empty so a waiter
						// reconstructs from scratch instead of receiving a half-built object.
						if (completed && obj != null) {
							shard.instances[handle] = new WeakReference (obj);
#if DEBUG
							shard.stackTraces[handle] = Environment.StackTrace;
#endif
							if (disposeProtected)
								// Set under this shard's write lock, mutually exclusive with public Dispose's
								// check+CAS on the same lock. (No public Dispose can run before we return
								// anyway, since user code has no reference to a still-constructing object.)
								obj.PreventPublicDisposal ();
						}
					} finally {
						shard.instancesLock.ExitWriteLock ();
					}
				} finally {
					// ALWAYS retire and release waiters, even if the publish above threw (e.g. the write
					// lock could not be acquired) so they never hang. "retired" is set BEFORE the gate so a
					// waiter that finds the reservation still in the map (because Remove never ran) takes it
					// over and reconstructs instead of livelocking on the already-signaled gate.
					reservation.retired = true;
					reservation.gate.Set ();
				}
			}

			return obj;
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
				// Reserved-handle fast path: this handle is mid-construction via GetOrAddObject. If WE own
				// that reservation, do NOT publish into "instances" here — that would expose a not-yet-
				// fully-constructed wrapper. GetOrAddObject Phase 3 publishes once the factory returns.
				// A reservation owned by a DIFFERENT thread means two objects are racing to claim the same
				// live native handle (pathological); refuse rather than overwrite the in-flight wrapper.
				if (shard.reservations.TryGetValue (handle, out var reservation)) {
					if (reservation.ownerThreadId == Environment.CurrentManagedThreadId)
						return;
#if THROW_OBJECT_EXCEPTIONS
					throw new InvalidOperationException (
						$"A managed object is being constructed for a handle that another thread is already constructing. " +
						$"H: {handle.ToString ("x")} Type: {instance.GetType ()}");
#else
					return;
#endif
				}

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
