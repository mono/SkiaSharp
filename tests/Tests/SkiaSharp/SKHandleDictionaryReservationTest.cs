using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static SkiaSharp.Tests.SKHandleDictionaryTestHelpers;

namespace SkiaSharp.Tests
{
	// Corner-case coverage for concurrent construction in HandleDictionary (PR #4080, fixes #3817).
	//
	// #4080 serializes construction with a SINGLE global lock: GetOrAddObject holds the
	// instancesLock (upgradeable-read) for its whole duration, INCLUDING the factory invocation.
	// There is no per-handle reservation gate. These white-box tests drive HandleDictionary directly
	// with a fake, non-owning SKObject (see SKHandleDictionaryTestHelpers) so the registry machinery
	// can be exercised without any native allocations, pinning the observable invariants that this
	// lock-the-whole-thing model guarantees: exactly-once construction, publication safety, and
	// recovery after a throwing factory.
	//
	// NOTE: nested/re-entrant GetOrAddObject from inside a factory is intentionally NOT tested here.
	// Because the factory runs under the non-recursive lock (ReaderWriterLockSlim with
	// LockRecursionPolicy.NoRecursion on non-Windows), a nested GetOrAddObject would throw
	// LockRecursionException rather than recurse — and on Windows (recursive CRITICAL_SECTION) it
	// would not — so any such test is platform-dependent and is omitted by design.
	public class SKHandleDictionaryReservationTest : SKTest
	{
		// --- Exactly-once construction under contention ---
		//
		// The upgradeable-read lock serializes the contending callers, so factoryCalls == 1
		// is the guaranteed outcome of the lock-the-whole-factory model rather than a race
		// the test wins. The value is a regression guard: narrowing the lock scope so two
		// callers could run the factory concurrently for the same handle would make this fail.

		[SkippableFact]
		public void ConcurrentSameHandleConstructsExactlyOnce ()
		{
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			var handle = NextHandle ();
			var factoryCalls = 0;

			const int threadCount = 32;
			var results = new FakeNativeObject[threadCount];

			RunConcurrent (threadCount, i => {
				results[i] = HandleDictionary.GetOrAddObject<FakeNativeObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => {
						Interlocked.Increment (ref factoryCalls);
						return new FakeNativeObject (h);
					});
			}, deadlockMessage: "Concurrent same-handle construction deadlocked.");

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
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

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
				Assert.True (t.Join (10_000), "Possible deadlock waiting on the construction lock.");

			try {
				for (var i = 0; i < threadCount; i++) {
					Assert.NotNull (results[i]);
					Assert.Same (results[0], results[i]);
					// Under the lock-the-whole-factory model a waiter can only obtain the wrapper
					// through GetOrAddObject's return value, i.e. after the ctor has run, so this
					// is 1 by construction. The assert is a regression guard: if the lock scope
					// were ever narrowed to publish the entry before the factory completed, a
					// waiter could observe FullyConstructed == 0 here.
					Assert.Equal (1, results[i].FullyConstructed);
				}
			} finally {
				results[0].Dispose ();
			}
		}

		// --- A throwing factory leaves nothing registered and lets a later call reconstruct ---

		[SkippableFact]
		public void FactoryFailureLeavesNothingRegisteredAndAllowsReconstruction ()
		{
			var handle = NextHandle ();

			Assert.Throws<InvalidOperationException> (() =>
				HandleDictionary.GetOrAddObject<FakeNativeObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => throw new InvalidOperationException ("boom")));

			// The factory threw under the lock; the lock is released in the finally and nothing was
			// ever registered, so no later caller is stranded.
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

		// --- The FIRST owner's factory fails; a later caller recovers ---
		//
		// The callers contend, but the upgradeable-read lock serializes them, so this reduces
		// to "first caller throws under the lock -> registry left empty -> next caller in line
		// reconstructs". It pins that a throwing factory leaves no stranded half-state for the
		// callers queued behind it.

		[SkippableFact]
		public void ConcurrentFactoryFailureRecoveredByWaiter ()
		{
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			var handle = NextHandle ();
			var failFirst = 1;
			var successfulFactoryCalls = 0;

			const int threadCount = 16;
			var results = new FakeNativeObject[threadCount];

			RunConcurrent (threadCount, i => {
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
			}, deadlockMessage: "Concurrent factory-failure recovery deadlocked.");

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
	}
}
