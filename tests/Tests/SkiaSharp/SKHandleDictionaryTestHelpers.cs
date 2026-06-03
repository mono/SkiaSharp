using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	// Shared white-box fakes and helpers for the HandleDictionary corner-case tests
	// (SKHandleDictionaryReservationTest / DisposeProtectedTest / StaleWrapperTest).
	//
	// These drive HandleDictionary directly with a fake, non-owning SKObject so the registry
	// machinery (single-lock construction, dispose-protected promotion, stale-wrapper deregistration)
	// can be exercised without any native allocations. A non-owning (owns: false) wrapper never
	// calls a native free on Dispose, and the synthetic handle values are chosen far from any real
	// native pointer range, so they cannot collide with live Skia objects. Every test disposes
	// everything it registers so GarbageCleanupFixture stays clean.
	//
	// The fakes are internal top-level types (not private nested) so they can be reused across the
	// three split test files.

	// A wrapper over a synthetic handle that touches no native memory.
	internal sealed class FakeNativeObject : SKObject
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
	// AFTER SKObject.Dispose() has claimed the disposal (isDisposed CAS) and RELEASED the lock, but
	// BEFORE Handle is zeroed (which triggers DeregisterHandle(this)). Parking here lets a test
	// interleave a replacement registration for the SAME handle into the exact "cleanup runs outside
	// the lock" window that SKObject.Dispose()'s comment claims is safe.
	internal sealed class GatedCleanupObject : SKObject
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

	internal static class SKHandleDictionaryTestHelpers
	{
		// Synthetic-handle seed kept well away from any real native pointer range. On 64-bit targets
		// that is a high 64-bit value. On 32-bit targets (x86 Windows, x86 Linux, browser WASM) IntPtr
		// is 32-bit, so a 64-bit seed would overflow new IntPtr(long); use a high 32-bit value there.
		// 0x4000_0000 fits in Int32 and leaves room for many +0x1000 increments before overflow.
		private static long handleSeed = IntPtr.Size == 4 ? 0x4000_0000L : 0x6000_0000_0000_0000L;

		// Hands out unique synthetic handles well away from any real native pointer range.
		public static IntPtr NextHandle () =>
			new IntPtr (Interlocked.Add (ref handleSeed, 0x1000));

		// Runs body(0..threadCount-1) on that many DEDICATED threads that all rendezvous at an internal
		// Barrier before invoking the body, guaranteeing genuine simultaneity. This is deliberately NOT
		// Parallel.For / Parallel.Invoke / Task.Run: those draw workers from the thread pool, whose degree
		// of parallelism depends on pool availability. A Barrier(N) needs N participants present AT ONCE,
		// so under a loaded full-suite run the pool may fail to supply N workers in time and the Barrier
		// hangs (a non-deterministic, load-dependent deadlock). Dedicated threads always reach the barrier.
		// Every thread is joined against a shared deadline; a real (production) deadlock surfaces as a
		// failed Join rather than a hung suite. The first body exception (if any) is rethrown after join.
		public static void RunConcurrent (
			int threadCount,
			Action<int> body,
			int timeoutMs = 30_000,
			string deadlockMessage = "Concurrent operation deadlocked.")
		{
			using var barrier = new Barrier (threadCount);
			var errors = new System.Collections.Concurrent.ConcurrentQueue<Exception> ();
			var threads = new Thread[threadCount];

			for (var i = 0; i < threadCount; i++) {
				var index = i;
				threads[i] = new Thread (() => {
					try {
						barrier.SignalAndWait ();
						body (index);
					} catch (Exception ex) {
						errors.Enqueue (ex);
					}
				}) { IsBackground = true };
			}

			foreach (var t in threads)
				t.Start ();

			// Attempt to join every worker before asserting so that a single timed-out
			// thread cannot leave the others unjoined (a worker still holding
			// instancesLock would otherwise cascade into later-test hangs).
			var sw = System.Diagnostics.Stopwatch.StartNew ();
			var allJoined = true;
			foreach (var t in threads) {
				var remaining = (int) Math.Max (0, timeoutMs - sw.ElapsedMilliseconds);
				allJoined &= t.Join (remaining);
			}

			Assert.True (allJoined, deadlockMessage);

			if (errors.TryDequeue (out var first))
				throw first;
		}

		// Runs a body that internally spawns its own parallelism (e.g. Parallel.For) on a dedicated
		// thread and joins with a deadline, so a production-side dispose/lock deadlock surfaces as a
		// deterministic test FAILURE instead of hanging the whole suite. Exceptions thrown by the body
		// are captured and rethrown on the calling thread.
		public static void RunWithTimeout (
			Action body,
			int timeoutMs = 30_000,
			string deadlockMessage = "Operation deadlocked.")
		{
			Exception captured = null;
			var runner = new Thread (() => {
				try {
					body ();
				} catch (Exception ex) {
					captured = ex;
				}
			}) { IsBackground = true };

			runner.Start ();
			Assert.True (runner.Join (timeoutMs), deadlockMessage);

			if (captured != null)
				System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture (captured).Throw ();
		}

		// Asserts that a disposed wrapper has been deregistered from the HandleDictionary.
		//
		// The handle is a raw native pointer. Once the wrapper is disposed and its native object is
		// freed, the allocator is free to immediately hand that exact address back out for an unrelated
		// object created by a DIFFERENT test running concurrently (xUnit parallelizes test collections).
		// Asserting the address is entirely absent from the registry is therefore racy and can spuriously
		// fail when a parallel test re-registers a brand-new wrapper at the reused address. The real
		// lifecycle invariant we care about is narrower and address-reuse-safe: OUR specific disposed
		// wrapper must no longer be reachable through the registry under its handle. A different live
		// wrapper that legitimately reused the freed address is fine and must not fail the test.
		public static void AssertDeregistered<TSkiaObject> (IntPtr handle, TSkiaObject instance)
			where TSkiaObject : SKObject
		{
			if (HandleDictionary.GetInstance<TSkiaObject> (handle, out var found))
				Assert.False (
					ReferenceEquals (found, instance),
					$"Disposed {typeof (TSkiaObject).Name} is still registered under handle 0x{handle.ToString ("x")}.");
		}
	}
}
