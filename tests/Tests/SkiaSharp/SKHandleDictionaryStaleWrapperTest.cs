using System;
using System.Threading;
using Xunit;
using static SkiaSharp.Tests.SKHandleDictionaryTestHelpers;

namespace SkiaSharp.Tests
{
	// Coverage for HandleDictionary's stale-wrapper deregistration window (PR #4080, fixes #3817).
	//
	// SKObject.Dispose() claims the isDisposed flag under the write lock, RELEASES the lock, runs
	// DisposeManaged()/DisposeNative() OUTSIDE the lock, and only then sets Handle = 0 ->
	// DeregisterHandle(). A handle can therefore be reused (ABA) by a brand-new wrapper while the old,
	// already-disposed wrapper is still parked mid-cleanup. These white-box tests park a wrapper in that
	// exact window (via GatedCleanupObject) and prove the late DeregisterHandle of the stale wrapper
	// never evicts the live replacement and never raises a THROW_OBJECT_EXCEPTIONS diagnostic. They use
	// a fake, non-owning SKObject (see SKHandleDictionaryTestHelpers) so no native memory is touched.
	public class SKHandleDictionaryStaleWrapperTest : SKTest
	{
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

			// Free the handle: public Dispose deregisters it from the registry.
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
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			var handle = NextHandle ();

			using var enteredCleanup = new ManualResetEventSlim (false);
			using var releaseCleanup = new ManualResetEventSlim (false);

			var first = new GatedCleanupObject (handle, enteredCleanup, releaseCleanup);
			Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var fetchedFirst));
			Assert.Same (first, fetchedFirst);

			GatedCleanupObject second = null;
			try {
				// Public-dispose W1 on another thread: it claims isDisposed under the write lock,
				// releases the lock, then parks in DisposeManaged BEFORE Handle=0 -> DeregisterHandle runs.
				var disposer = RunOnThread (() => first.Dispose ());
				try {
					Assert.True (enteredCleanup.Wait (10_000), "W1 never entered the managed-cleanup window.");
					Assert.True (first.IsDisposed);

					// While W1 is parked mid-cleanup, a new native object reuses the handle. Run the
					// construction under a timeout: if cleanup ever regressed to holding the lock,
					// this would block on the lock and we want a deterministic failure, not a suite hang.
					var creator = RunOnThread (() => HandleDictionary.GetOrAddObject<GatedCleanupObject> (
						handle, owns: false, unrefExisting: false, (h, o) => new GatedCleanupObject (h)));
					Assert.True (creator.Wait (10_000), "Replacement construction blocked — cleanup may hold the lock.");
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
		//     GetOrAddObject), so it travels through RegisterHandle. This pins claim #2: RegisterHandle's
		//     replacement branch only disposes an existing entry when it is NOT disposed (line guarded by
		//     !obj.IsDisposed) — a stale disposed W1 is simply overwritten, never recursively disposed,
		//     and W1's later DeregisterHandle still no-ops without evicting W2 or throwing. ---

		[SkippableFact]
		public void StaleWrapperCleanupDoesNotEvictDirectCtorReplacementForReusedHandle ()
		{
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			var handle = NextHandle ();

			using var enteredCleanup = new ManualResetEventSlim (false);
			using var releaseCleanup = new ManualResetEventSlim (false);

			var first = new GatedCleanupObject (handle, enteredCleanup, releaseCleanup);
			Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var fetchedFirst));
			Assert.Same (first, fetchedFirst);

			GatedCleanupObject second = null;
			try {
				var disposer = RunOnThread (() => first.Dispose ());
				try {
					Assert.True (enteredCleanup.Wait (10_000), "W1 never entered the managed-cleanup window.");
					Assert.True (first.IsDisposed);

					// Direct-construct the replacement: its base ctor RegisterHandle sees the disposed W1
					// entry, skips the dispose-the-old branch (!obj.IsDisposed is false), and overwrites it.
					// Timeout-guarded so a cleanup-holds-the-lock regression fails deterministically.
					var creator = RunOnThread (() => new GatedCleanupObject (handle));
					Assert.True (creator.Wait (10_000), "Replacement construction blocked — cleanup may hold the lock.");
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
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			var handle = NextHandle ();

			using var entered1 = new ManualResetEventSlim (false);
			using var release1 = new ManualResetEventSlim (false);
			using var entered2 = new ManualResetEventSlim (false);
			using var release2 = new ManualResetEventSlim (false);

			var w1 = new GatedCleanupObject (handle, entered1, release1);
			GatedCleanupObject w2 = null, w3 = null;
			ThreadResult disposer1 = null, disposer2 = null;

			try {
				// Park W1 (disposed) in its cleanup window.
				disposer1 = RunOnThread (() => w1.Dispose ());
				Assert.True (entered1.Wait (10_000), "W1 never entered the managed-cleanup window.");
				Assert.True (w1.IsDisposed);

				// Overwrite the disposed W1 with W2 (direct ctor -> RegisterHandle overwrite-stale branch),
				// then park W2 (disposed) in ITS cleanup window too. Now two stale wrappers are pending.
				var make2 = RunOnThread (() => new GatedCleanupObject (handle, entered2, release2));
				Assert.True (make2.Wait (10_000), "W2 construction blocked — cleanup may hold the lock.");
				w2 = make2.Result;
				Assert.NotSame (w1, w2);
				Assert.True (HandleDictionary.GetInstance<GatedCleanupObject> (handle, out var afterW2));
				Assert.Same (w2, afterW2);

				disposer2 = RunOnThread (() => w2.Dispose ());
				Assert.True (entered2.Wait (10_000), "W2 never entered the managed-cleanup window.");
				Assert.True (w2.IsDisposed);

				// Overwrite the disposed W2 with the live W3.
				var make3 = RunOnThread (() => new GatedCleanupObject (handle));
				Assert.True (make3.Wait (10_000), "W3 construction blocked — cleanup may hold the lock.");
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
	}
}
