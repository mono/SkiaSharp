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

		// A non-owning wrapper whose managed-cleanup phase can be parked on a gate. DisposeManaged runs
		// AFTER SKObject.Dispose() has claimed the disposal (isDisposed CAS) and RELEASED the per-handle
		// shard write lock, but BEFORE Handle is zeroed (which triggers DeregisterHandle(this)). Parking
		// here lets a test interleave a replacement registration for the SAME handle into the exact
		// "cleanup runs outside the lock" window that SKObject.Dispose()'s comment claims is safe.
		private sealed class GatedCleanupObject : SKObject
		{
			private readonly ManualResetEventSlim enteredCleanup;
			private readonly ManualResetEventSlim releaseCleanup;

			public GatedCleanupObject (
				IntPtr handle,
				ManualResetEventSlim enteredCleanup = null,
				ManualResetEventSlim releaseCleanup = null)
				: base (handle, owns: false)
			{
				this.enteredCleanup = enteredCleanup;
				this.releaseCleanup = releaseCleanup;
			}

			protected override void DisposeNative ()
			{
				// no native object backs this fake handle
			}

			protected override void DisposeManaged ()
			{
				if (enteredCleanup == null)
					return;

				// Signal that we are parked in the post-claim / pre-deregister window, then wait for the
				// test to release us. A timeout guards against a test bug leaving this thread parked forever.
				enteredCleanup.Set ();
				releaseCleanup?.Wait (30_000);
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

		// --- Address reuse (ABA): a handle freed then re-used for a new native object. The registry
		//     must hand out a fresh wrapper, not the disposed one. ---

		[SkippableFact]
		public void AddressReuseAfterDisposeConstructsFreshWrapper ()
		{
			var handle = NextHandle ();

			var first = HandleDictionary.GetOrAddObject<FakeNativeObject> (
				handle, owns: false, unrefExisting: false,
				(h, o) => new FakeNativeObject (h));
			Assert.NotNull (first);
			Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var fetchedFirst));
			Assert.Same (first, fetchedFirst);

			// Free the handle: public Dispose deregisters it from the shard.
			first.Dispose ();
			Assert.True (first.IsDisposed);
			Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (handle, out _));

			// A new native object reuses the same pointer value: must build a brand-new wrapper.
			var second = HandleDictionary.GetOrAddObject<FakeNativeObject> (
				handle, owns: false, unrefExisting: false,
				(h, o) => new FakeNativeObject (h));

			try {
				Assert.NotNull (second);
				Assert.NotSame (first, second);
				Assert.False (second.IsDisposed);
				Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var fetchedSecond));
				Assert.Same (second, fetchedSecond);
			} finally {
				second.Dispose ();
			}
		}

		// --- Promote (disposeProtected) racing a concurrent public Dispose() on the SAME handle.
		//     Invariant #1: PreventPublicDisposal (under the shard upgradeable-read lock) and
		//     Dispose()'s IgnorePublicDispose check + isDisposed CAS (under the shard write lock) are
		//     mutually exclusive, so the outcome is always well-defined and never torn: the
		//     disposeProtected caller ALWAYS gets back a live, protected wrapper (the original if it
		//     promoted in time, or a freshly reconstructed one if Dispose won the race). ---

		[SkippableFact]
		public void DisposeProtectedRacingPublicDisposeNeverTears ()
		{
			const int iterations = 300;
			var created = new System.Collections.Concurrent.ConcurrentBag<FakeNativeObject> ();

			RunWithTimeout (() => {
				for (var i = 0; i < iterations; i++) {
					var handle = NextHandle ();

					var original = HandleDictionary.GetOrAddObject<FakeNativeObject> (
						handle, owns: false, unrefExisting: false,
						(h, o) => {
							var obj = new FakeNativeObject (h);
							created.Add (obj);
							return obj;
						});

					using var gate = new Barrier (2);
					FakeNativeObject promoted = null;

					Parallel.Invoke (
						() => {
							gate.SignalAndWait ();
							// Promote: returns the live instance protected, or reconstructs if Dispose won.
							promoted = HandleDictionary.GetOrAddObject<FakeNativeObject> (
								handle, owns: false, unrefExisting: false, disposeProtected: true,
								(h, o) => {
									var obj = new FakeNativeObject (h);
									created.Add (obj);
									return obj;
								});
						},
						() => {
							gate.SignalAndWait ();
							original.Dispose ();
						});

					// The disposeProtected contract: always a live, protected wrapper for this handle.
					Assert.NotNull (promoted);
					Assert.False (promoted.IsDisposed);
					Assert.True (promoted.IgnorePublicDispose);
					Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var current));
					Assert.Same (promoted, current);
				}
			}, 30_000, "Promote-vs-dispose race deadlocked.");

			// Tear everything down via the internal path (ignores IgnorePublicDispose).
			foreach (var obj in created)
				obj.DisposeInternal ();
		}

		// --- Stale wrapper finishing cleanup OUTSIDE the lock must not evict a replacement that took
		//     over the SAME (reused) handle while it was parked mid-cleanup. This pins all three safety
		//     claims in SKObject.Dispose()'s comment for the "cleanup runs outside the lock" window:
		//       #1 a concurrent GetOrAddObject after the isDisposed CAS sees the disposed wrapper and is
		//          filtered out (GetInstanceNoLocks), so it builds a fresh wrapper;
		//       #3 the stale wrapper's own Handle=0 -> DeregisterHandle is a no-op when the entry now
		//          points to someone else (release), and raises no THROW_OBJECT_EXCEPTIONS diagnostic
		//          (the deregistering instance is itself already disposed).
		//     Variant below covers claim #2 (RegisterHandle's replacement branch). ---

		[SkippableFact]
		public void StaleWrapperCleanupDoesNotEvictGetOrAddReplacementForReusedHandle ()
		{
			var handle = NextHandle ();

			using var enteredCleanup = new ManualResetEventSlim (false);
			using var releaseCleanup = new ManualResetEventSlim (false);

			var first = new GatedCleanupObject (handle, enteredCleanup, releaseCleanup);
			Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var fetchedFirst));
			Assert.Same (first, fetchedFirst);

			GatedCleanupObject second = null;
			try {
				// Public-dispose W1 on another thread: it claims isDisposed under the shard write lock,
				// releases the lock, then parks in DisposeManaged BEFORE Handle=0 -> DeregisterHandle runs.
				var disposer = Task.Run (() => first.Dispose ());
				try {
					Assert.True (enteredCleanup.Wait (10_000), "W1 never entered the managed-cleanup window.");
					Assert.True (first.IsDisposed);

					// While W1 is parked mid-cleanup, a new native object reuses the handle. Run the
					// construction under a timeout: if cleanup ever regressed to holding the shard lock,
					// this would block on the lock and we want a deterministic failure, not a suite hang.
					var creator = Task.Run (() => HandleDictionary.GetOrAddObject<GatedCleanupObject> (
						handle, owns: false, unrefExisting: false, (h, o) => new GatedCleanupObject (h)));
					Assert.True (creator.Wait (10_000), "Replacement construction blocked — cleanup may hold the shard lock.");
					second = creator.Result;

					// GetInstanceNoLocks filtered the disposed W1, so a brand-new wrapper was published.
					Assert.NotNull (second);
					Assert.NotSame (first, second);
					Assert.False (second.IsDisposed);
					Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var afterReplace));
					Assert.Same (second, afterReplace);
				} finally {
					// Let W1 finish: Handle=0 -> DeregisterHandle(W1) must NOT evict W2 and must NOT throw.
					releaseCleanup.Set ();
					Assert.True (disposer.Wait (10_000), "W1 dispose did not complete.");
				}

				// W2 survived the stale wrapper's deregistration and is still the registered instance.
				Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var survivor));
				Assert.Same (second, survivor);
				Assert.False (second.IsDisposed);
			} finally {
				// Dispose the replacement on every path so an assertion failure mid-window cannot leak
				// a live wrapper and have GarbageCleanupFixture mask the real failure.
				second?.Dispose ();
			}

			Assert.False (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out _));
		}

		// --- Same window as above, but the replacement is constructed via a DIRECT constructor (no
		//     reservation), so it travels through RegisterHandle. This pins claim #2: RegisterHandle's
		//     replacement branch only disposes an existing entry when it is NOT disposed (line guarded by
		//     !obj.IsDisposed) — a stale disposed W1 is simply overwritten, never recursively disposed,
		//     and W1's later DeregisterHandle still no-ops without evicting W2 or throwing. ---

		[SkippableFact]
		public void StaleWrapperCleanupDoesNotEvictDirectCtorReplacementForReusedHandle ()
		{
			var handle = NextHandle ();

			using var enteredCleanup = new ManualResetEventSlim (false);
			using var releaseCleanup = new ManualResetEventSlim (false);

			var first = new GatedCleanupObject (handle, enteredCleanup, releaseCleanup);
			Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var fetchedFirst));
			Assert.Same (first, fetchedFirst);

			GatedCleanupObject second = null;
			try {
				var disposer = Task.Run (() => first.Dispose ());
				try {
					Assert.True (enteredCleanup.Wait (10_000), "W1 never entered the managed-cleanup window.");
					Assert.True (first.IsDisposed);

					// Direct-construct the replacement: its base ctor RegisterHandle sees the disposed W1
					// entry, skips the dispose-the-old branch (!obj.IsDisposed is false), and overwrites it.
					// Timeout-guarded so a cleanup-holds-the-lock regression fails deterministically.
					var creator = Task.Run (() => new GatedCleanupObject (handle));
					Assert.True (creator.Wait (10_000), "Replacement construction blocked — cleanup may hold the shard lock.");
					second = creator.Result;

					Assert.NotNull (second);
					Assert.NotSame (first, second);
					Assert.False (second.IsDisposed);
					Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var afterReplace));
					Assert.Same (second, afterReplace);
				} finally {
					releaseCleanup.Set ();
					Assert.True (disposer.Wait (10_000), "W1 dispose did not complete.");
				}

				Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var survivor));
				Assert.Same (second, survivor);
				Assert.False (second.IsDisposed);
			} finally {
				second?.Dispose ();
			}

			Assert.False (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out _));
		}

		// --- Two stale wrappers in flight at once: W1 and W2 are BOTH parked in DisposeManaged (disposed,
		//     lock released, Handle not yet zeroed) for the same reused handle, while a live W3 owns the
		//     registry slot. When the two stale wrappers later run DeregisterHandle — in an order DIFFERENT
		//     from how they were published — neither may evict W3 nor throw. This stresses the "only remove
		//     if the entry still points to THIS instance" guard across overlapping stale deregistrations,
		//     which the single-stale-wrapper tests above do not exercise. ---

		[SkippableFact]
		public void TwoStaleWrappersDeregisteringDoNotEvictThirdReplacementForReusedHandle ()
		{
			var handle = NextHandle ();

			using var entered1 = new ManualResetEventSlim (false);
			using var release1 = new ManualResetEventSlim (false);
			using var entered2 = new ManualResetEventSlim (false);
			using var release2 = new ManualResetEventSlim (false);

			var w1 = new GatedCleanupObject (handle, entered1, release1);
			GatedCleanupObject w2 = null, w3 = null;
			Task disposer1 = null, disposer2 = null;

			try {
				// Park W1 (disposed) in its cleanup window.
				disposer1 = Task.Run (() => w1.Dispose ());
				Assert.True (entered1.Wait (10_000), "W1 never entered the managed-cleanup window.");
				Assert.True (w1.IsDisposed);

				// Overwrite the disposed W1 with W2 (direct ctor -> RegisterHandle overwrite-stale branch),
				// then park W2 (disposed) in ITS cleanup window too. Now two stale wrappers are pending.
				var make2 = Task.Run (() => new GatedCleanupObject (handle, entered2, release2));
				Assert.True (make2.Wait (10_000), "W2 construction blocked — cleanup may hold the shard lock.");
				w2 = make2.Result;
				Assert.NotSame (w1, w2);
				Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var afterW2));
				Assert.Same (w2, afterW2);

				disposer2 = Task.Run (() => w2.Dispose ());
				Assert.True (entered2.Wait (10_000), "W2 never entered the managed-cleanup window.");
				Assert.True (w2.IsDisposed);

				// Overwrite the disposed W2 with the live W3.
				var make3 = Task.Run (() => new GatedCleanupObject (handle));
				Assert.True (make3.Wait (10_000), "W3 construction blocked — cleanup may hold the shard lock.");
				w3 = make3.Result;
				Assert.NotSame (w1, w3);
				Assert.NotSame (w2, w3);
				Assert.False (w3.IsDisposed);
				Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var afterW3));
				Assert.Same (w3, afterW3);

				try {
					// Drain the two stale wrappers in an order DIFFERENT from publication (W2 before W1).
					// Each runs Handle=0 -> DeregisterHandle; the entry is the live W3, so both must no-op.
					release2.Set ();
					Assert.True (disposer2.Wait (10_000), "W2 dispose did not complete.");
					release1.Set ();
					Assert.True (disposer1.Wait (10_000), "W1 dispose did not complete.");
				} finally {
					// Belt-and-braces in case an assertion above threw before the ordered drain.
					release1.Set ();
					release2.Set ();
				}

				// Both stale deregistrations ran; W3 must still be the registered instance.
				Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var survivor));
				Assert.Same (w3, survivor);
				Assert.False (w3.IsDisposed);
			} finally {
				// Unblock anything still parked and make sure all wrappers are drained/disposed so an early
				// assertion failure cannot leak a wrapper and have GarbageCleanupFixture mask the real cause.
				release1.Set ();
				release2.Set ();
				disposer1?.Wait (10_000);
				disposer2?.Wait (10_000);
				w3?.Dispose ();
			}

			Assert.False (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out _));
		}

		// --- ShardCount == 1 parity: with a single shard every handle routes to index 0, i.e. the
		//     registry collapses to the original single-lock / single-dictionary behavior. Verified on
		//     the pure routing function so it holds regardless of this process's actual ShardCount. ---

		[SkippableFact]
		public void SingleShardRoutesEveryHandleToZero ()
		{
			for (var i = 0; i < 10_000; i++) {
				var handle = NextHandle ();
				Assert.Equal (0, HandleDictionary.ShardIndexFor (handle, mask: 0));
			}

			// Sanity: a few hand-picked edge values also collapse to 0 under a single shard.
			foreach (var raw in new long[] { 0x1, -1, long.MaxValue, long.MinValue, 0x1000, unchecked((long)0x9E3779B97F4A7C15UL) }) {
				Assert.Equal (0, HandleDictionary.ShardIndexFor (new IntPtr (raw), mask: 0));
			}
		}

		// --- Multi-shard routing stays in range and actually spreads handles across shards. ---

		[SkippableFact]
		public void MultiShardRoutingStaysInRangeAndDistributes ()
		{
			Skip.If (HandleDictionary.ShardCount < 2, "Only one shard configured on this machine.");

			var mask = HandleDictionary.ShardCount - 1;
			var seen = new HashSet<int> ();

			for (var i = 0; i < 10_000; i++) {
				var idx = HandleDictionary.ShardIndexFor (NextHandle (), mask);
				Assert.InRange (idx, 0, HandleDictionary.ShardCount - 1);
				seen.Add (idx);
			}

			// With thousands of distinct handles, more than one shard must be hit.
			Assert.True (seen.Count > 1, "Routing collapsed every handle onto a single shard.");
		}

#if THROW_OBJECT_EXCEPTIONS
		// --- Publish-phase failure leaves a "retired but present" reservation; the ORIGINAL owner
		//     thread retrying the same handle must take it over and reconstruct, NOT be mistaken for
		//     active same-thread re-entrancy. This pins the ordering of the Phase 1b guards: a retired
		//     reservation is fully unwound, so the retired-takeover branch is checked before the
		//     re-entrancy throw. Without the fault-injection seam this branch is unreachable. ---

		[SkippableFact]
		public void RetiredReservationTakenOverBySameThreadAfterPublishFailure ()
		{
			var handle = NextHandle ();
			var armed = 1;

			// Throw exactly once, only for our handle, the first time the publish phase runs for it.
			HandleDictionary.PublishPhaseHook = h => {
				if (h == handle && Interlocked.Exchange (ref armed, 0) == 1)
					throw new InvalidOperationException ("injected publish-phase failure");
			};

			try {
				// The factory SUCCEEDS, but the publish phase throws before Remove runs, so the
				// reservation is left in the map with retired == true and its gate already signaled.
				var ex = Assert.Throws<InvalidOperationException> (() =>
					HandleDictionary.GetOrAddObject<FakeNativeObject> (
						handle, owns: false, unrefExisting: false,
						(h, o) => new FakeNativeObject (h)));
				Assert.Contains ("injected publish-phase failure", ex.Message);

				// Nothing reached "instances".
				Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (handle, out _));

				// The SAME thread (matching the retired reservation's ownerThreadId) retries the SAME
				// handle. The retired reservation must be taken over and reconstructed. The takeover
				// path waits on no gate, so a correct implementation returns synchronously; a wrong one
				// would instead throw "Re-entrant ..." (caught by the assertion below).
				var rebuilt = HandleDictionary.GetOrAddObject<FakeNativeObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => new FakeNativeObject (h));

				try {
					Assert.NotNull (rebuilt);
					Assert.False (rebuilt.IsDisposed);
					Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var fetched));
					Assert.Same (rebuilt, fetched);
				} finally {
					rebuilt.Dispose ();
				}
			} finally {
				HandleDictionary.PublishPhaseHook = null;
			}
		}

		// --- Same scenario, but a DIFFERENT thread observes the retired reservation: it must take over
		//     and reconstruct rather than livelocking on the already-signaled gate (the canonical H1
		//     recovery the "retired" flag exists for). ---

		[SkippableFact]
		public void RetiredReservationTakenOverByOtherThreadAfterPublishFailure ()
		{
			var handle = NextHandle ();
			var armed = 1;

			HandleDictionary.PublishPhaseHook = h => {
				if (h == handle && Interlocked.Exchange (ref armed, 0) == 1)
					throw new InvalidOperationException ("injected publish-phase failure");
			};

			try {
				// Owner runs on its own dedicated thread so its ManagedThreadId is distinct from the
				// waiter's, exercising the cross-thread arm of the retired-takeover branch.
				Exception ownerEx = null;
				var owner = new Thread (() => {
					try {
						HandleDictionary.GetOrAddObject<FakeNativeObject> (
							handle, owns: false, unrefExisting: false,
							(h, o) => new FakeNativeObject (h));
					} catch (Exception e) {
						ownerEx = e;
					}
				});
				owner.IsBackground = true;
				owner.Start ();
				Assert.True (owner.Join (10_000), "Owner thread hung during the failing publish phase.");
				Assert.IsType<InvalidOperationException> (ownerEx);
				Assert.False (HandleDictionary.GetInstance<FakeNativeObject> (handle, out _));

				// A different thread takes over the retired reservation and reconstructs, within timeout.
				FakeNativeObject rebuilt = null;
				RunWithTimeout (() => {
					rebuilt = HandleDictionary.GetOrAddObject<FakeNativeObject> (
						handle, owns: false, unrefExisting: false,
						(h, o) => new FakeNativeObject (h));
				}, 10_000, "Waiter livelocked on a retired reservation's already-signaled gate.");

				try {
					Assert.NotNull (rebuilt);
					Assert.False (rebuilt.IsDisposed);
					Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var fetched));
					Assert.Same (rebuilt, fetched);
				} finally {
					rebuilt?.Dispose ();
				}
			} finally {
				HandleDictionary.PublishPhaseHook = null;
			}
		}
#endif

		// Finds two synthetic handles that land on two different shard locks, so a test can
		// drive opposite-order construction across distinct shards. Returns false when the
		// machine only has a single shard.
		static bool TryFindHandlesOnTwoShards (out IntPtr[] shardA, out IntPtr[] shardB)
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
