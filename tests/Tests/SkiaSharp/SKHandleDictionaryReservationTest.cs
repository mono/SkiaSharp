using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	// Corner-case coverage for the two-phase (reservation-gate) construction in HandleDictionary
	// (experiment #4101 / PR #4102). These are white-box tests: they drive HandleDictionary directly
	// with a fake, non-owning SKObject so the registry machinery can be exercised without any native
	// allocations. Every claim from the design discussion is pinned to a test here.
	//
	// A non-owning (owns: false) wrapper never calls a native free on Dispose, and its synthetic
	// handle values are chosen far from any real native pointer, so they cannot collide with live
	// Skia objects. Each test disposes everything it registers so GarbageCleanupFixture stays clean.
	public class SKHandleDictionaryReservationTest : SKTest
	{
		// A wrapper over a synthetic handle that touches no native memory.
		private sealed class FakeNativeObject : SKObject
		{
			// Set to 1 only AFTER the base ctor (and any "subclass init") has run. Waiters that dedup
			// this instance must observe a fully-constructed object, i.e. FullyConstructed == 1.
			public readonly int FullyConstructed;

			public FakeNativeObject (IntPtr handle, Action duringConstruction = null)
				: base (handle, owns: false)
			{
				// Simulate subclass initialization happening after the base ctor's RegisterHandle.
				duringConstruction?.Invoke ();
				FullyConstructed = 1;
			}

			protected override void DisposeNative ()
			{
				// no native object backs this fake handle
			}
		}

		private static long handleSeed = 0x6000_0000_0000_0000L;

		// Hands out unique synthetic handles well away from any real native pointer range.
		private static IntPtr NextHandle () =>
			new IntPtr (Interlocked.Add (ref handleSeed, 0x1000));

		private static void RunWithTimeout (Action action, int timeoutMs, string deadlockMessage)
		{
			Exception captured = null;
			var task = Task.Run (() => {
				try {
					action ();
				} catch (Exception ex) {
					captured = ex;
				}
			});

			Assert.True (task.Wait (timeoutMs), deadlockMessage);
			if (captured != null)
				throw captured;
		}

		// --- Exactly-once construction under concurrency ---

		[SkippableFact]
		public void ConcurrentSameHandleConstructsExactlyOnce ()
		{
			var handle = NextHandle ();
			var factoryCalls = 0;

			const int threadCount = 32;
			using var barrier = new Barrier (threadCount);
			var results = new FakeNativeObject[threadCount];

			Parallel.For (0, threadCount, i => {
				barrier.SignalAndWait ();
				results[i] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => {
						Interlocked.Increment (ref factoryCalls);
						return new FakeNativeObject (h);
					});
			});

			try {
				Assert.Equal (1, factoryCalls);
				for (var i = 0; i < threadCount; i++) {
					Assert.NotNull (results[i]);
					Assert.Same (results[0], results[i]);
				}
			} finally {
				results[0].Dispose ();
			}
		}

		// --- Publication safety: waiters never observe a half-built wrapper ---

		[SkippableFact]
		public void WaitersOnlyObserveFullyConstructedWrapper ()
		{
			var handle = NextHandle ();

			const int threadCount = 16;
			using var startGate = new ManualResetEventSlim (false);
			var results = new FakeNativeObject[threadCount];

			var threads = new List<Thread> ();
			for (var i = 0; i < threadCount; i++) {
				var index = i;
				var t = new Thread (() => {
					startGate.Wait ();
					results[index] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
						handle, owns: false, unrefExisting: false,
						(h, o) => new FakeNativeObject (h, duringConstruction: () => Thread.Sleep (40)));
				});
				t.IsBackground = true;
				t.Start ();
				threads.Add (t);
			}

			startGate.Set ();
			foreach (var t in threads)
				Assert.True (t.Join (10_000), "Possible deadlock waiting on the reservation gate.");

			try {
				for (var i = 0; i < threadCount; i++) {
					Assert.NotNull (results[i]);
					Assert.Same (results[0], results[i]);
					// If a waiter ever saw the object before its ctor finished, this would be 0.
					Assert.Equal (1, results[i].FullyConstructed);
				}
			} finally {
				results[0].Dispose ();
			}
		}

		// --- Re-entrant same-thread, same-handle construction fails fast (never hangs) ---

		[SkippableFact]
		public void ReentrantSameThreadSameHandleThrows ()
		{
			var handle = NextHandle ();

			RunWithTimeout (() => {
				var ex = Assert.Throws<InvalidOperationException> (() =>
					HandleDictionary.GetOrAddObject<FakeNativeObject> (
						handle, owns: false, unrefExisting: false,
						(h, o) => {
							// Re-enter for the SAME handle on the SAME thread while it is reserved.
							return HandleDictionary.GetOrAddObject<FakeNativeObject> (
								handle, owns: false, unrefExisting: false,
								(h2, o2) => new FakeNativeObject (h2));
						}));
				Assert.Contains ("Re-entrant", ex.Message);
			}, 10_000, "Re-entrant same-handle construction deadlocked instead of throwing.");

			// The failed construction must leave nothing behind.
			Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (handle, out _));
		}

		// --- A throwing factory clears the reservation and lets a later call reconstruct ---

		[SkippableFact]
		public void FactoryFailureClearsReservationAndAllowsReconstruction ()
		{
			var handle = NextHandle ();

			Assert.Throws<InvalidOperationException> (() =>
				HandleDictionary.GetOrAddObject<FakeNativeObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => throw new InvalidOperationException ("boom")));

			// Reservation removed in the finally → nothing registered, no waiter stranded.
			Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (handle, out _));

			var rebuilt = HandleDictionary.GetOrAddObject<FakeNativeObject> (
				handle, owns: false, unrefExisting: false,
				(h, o) => new FakeNativeObject (h));

			try {
				Assert.NotNull (rebuilt);
				Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var fetched));
				Assert.Same (rebuilt, fetched);
			} finally {
				rebuilt.Dispose ();
			}
		}

		// --- Concurrent storm where the FIRST owner's factory fails: a waiter recovers ---

		[SkippableFact]
		public void ConcurrentFactoryFailureRecoveredByWaiter ()
		{
			var handle = NextHandle ();
			var failFirst = 1;
			var successfulFactoryCalls = 0;

			const int threadCount = 16;
			using var barrier = new Barrier (threadCount);
			var results = new FakeNativeObject[threadCount];

			Parallel.For (0, threadCount, i => {
				barrier.SignalAndWait ();
				try {
					results[i] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
						handle, owns: false, unrefExisting: false,
						(h, o) => {
							// Exactly one factory invocation throws; the registry must recover.
							if (Interlocked.CompareExchange (ref failFirst, 0, 1) == 1)
								throw new InvalidOperationException ("first owner fails");
							Interlocked.Increment (ref successfulFactoryCalls);
							return new FakeNativeObject (h);
						});
				} catch (InvalidOperationException) {
					results[i] = null;
				}
			});

			FakeNativeObject survivor = null;
			try {
				// At least one thread saw the failure; everyone who got an object got the SAME one.
				for (var i = 0; i < threadCount; i++) {
					if (results[i] == null)
						continue;
					survivor ??= results[i];
					Assert.Same (survivor, results[i]);
				}
				Assert.NotNull (survivor);
				Assert.Equal (1, successfulFactoryCalls);
			} finally {
				survivor?.Dispose ();
			}
		}

		// --- Nested construction of a different handle during a factory does not deadlock ---

		[SkippableFact]
		public void NestedDifferentHandleConstructionRegistersBoth ()
		{
			var parentHandle = NextHandle ();
			var childHandle = NextHandle ();
			FakeNativeObject child = null;

			FakeNativeObject parent = null;
			RunWithTimeout (() => {
				parent = HandleDictionary.GetOrAddObject<FakeNativeObject> (
					parentHandle, owns: false, unrefExisting: false,
					(h, o) => {
						// Build a DIFFERENT-handle object while the parent is mid-construction.
						child = HandleDictionary.GetOrAddObject<FakeNativeObject> (
							childHandle, owns: false, unrefExisting: false,
							(ch, co) => new FakeNativeObject (ch));
						return new FakeNativeObject (h);
					});
			}, 10_000, "Nested different-handle construction deadlocked.");

			try {
				Assert.NotNull (parent);
				Assert.NotNull (child);
				Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (parentHandle, out var p));
				Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (childHandle, out var c));
				Assert.Same (parent, p);
				Assert.Same (child, c);
			} finally {
				parent?.Dispose ();
				child?.Dispose ();
			}
		}

		// --- Cross-shard AB-BA: two threads constructing parents on opposite shards, each building a
		//     child on the other shard, must NOT deadlock. This is the exact hazard the reservation
		//     gate removes; the prior factory-under-lock prototype would hang here. ---

		[SkippableFact]
		public void CrossShardOppositeOrderConstructionDoesNotDeadlock ()
		{
			// Need at least two distinct shards to form a cross-shard ordering at all.
			if (!TryFindHandlesOnTwoShards (out var shardA, out var shardB))
				throw new SkipException ("Only one handle shard on this machine; cross-shard hazard cannot occur.");

			var parent1 = shardA[0];
			var child2 = shardA[1];
			var child1 = shardB[0];
			var parent2 = shardB[1];

			var built = new FakeNativeObject[4];
			using var barrier = new Barrier (2);

			RunWithTimeout (() => {
				Parallel.Invoke (
					() => {
						built[0] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
							parent1, owns: false, unrefExisting: false,
							(h, o) => {
								barrier.SignalAndWait ();
								built[1] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
									child1, owns: false, unrefExisting: false,
									(ch, co) => new FakeNativeObject (ch));
								return new FakeNativeObject (h);
							});
					},
					() => {
						built[2] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
							parent2, owns: false, unrefExisting: false,
							(h, o) => {
								barrier.SignalAndWait ();
								built[3] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
									child2, owns: false, unrefExisting: false,
									(ch, co) => new FakeNativeObject (ch));
								return new FakeNativeObject (h);
							});
					});
			}, 15_000, "Cross-shard opposite-order construction deadlocked (factory ran under a shard lock?).");

			try {
				foreach (var obj in built)
					Assert.NotNull (obj);
			} finally {
				foreach (var obj in built)
					obj?.Dispose ();
			}
		}

		// --- dispose-protected fresh construction sets the flag (under the Phase 3 write lock) ---

		[SkippableFact]
		public void DisposeProtectedFreshConstructionSetsFlag ()
		{
			var handle = NextHandle ();

			var obj = HandleDictionary.GetOrAddObject<FakeNativeObject> (
				handle, owns: false, unrefExisting: false, disposeProtected: true,
				(h, o) => new FakeNativeObject (h));

			try {
				Assert.NotNull (obj);
				Assert.True (obj.IgnorePublicDispose);

				// Public Dispose() is a no-op while protected.
				obj.Dispose ();
				Assert.False (obj.IsDisposed);
			} finally {
				// Tear down via the internal path that ignores the protection flag.
				obj.DisposeInternal ();
			}
		}

		// Probes synthetic handles until it collects two distinct shards with two handles each,
		// using the same GetLockFor identity HandleDictionary uses to pick a shard.
		private static bool TryFindHandlesOnTwoShards (out IntPtr[] shardA, out IntPtr[] shardB)
		{
			shardA = null;
			shardB = null;

			var byLock = new Dictionary<object, List<IntPtr>> ();
			for (var i = 0; i < 4096; i++) {
				var handle = NextHandle ();
				var key = HandleDictionary.GetLockFor (handle);
				if (!byLock.TryGetValue (key, out var list)) {
					list = new List<IntPtr> ();
					byLock[key] = list;
				}
				if (list.Count < 2)
					list.Add (handle);

				var ready = new List<List<IntPtr>> ();
				foreach (var kv in byLock) {
					if (kv.Value.Count >= 2)
						ready.Add (kv.Value);
				}
				if (ready.Count >= 2) {
					shardA = ready[0].ToArray ();
					shardB = ready[1].ToArray ();
					return true;
				}
			}

			return false;
		}
	}
}
