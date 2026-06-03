using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using static SkiaSharp.Tests.SKHandleDictionaryTestHelpers;

namespace SkiaSharp.Tests
{
	// Coverage for HandleDictionary's dispose-protected promotion (PR #4080, fixes #3817).
	//
	// GetOrAddObject(..., disposeProtected: true) calls SKObject.PreventPublicDisposal() on the
	// returned wrapper while holding the instancesLock. The one-way IgnorePublicDispose latch is set
	// under that lock; public Dispose() reads it (and pairs it with the isDisposed CAS) under the
	// mutually-exclusive write lock. These white-box tests pin that a dispose-protected caller always
	// gets back a live, protected wrapper, even when racing a concurrent public Dispose() for the same
	// handle. They use a fake, non-owning SKObject (see SKHandleDictionaryTestHelpers) so no native
	// memory is touched.
	public class SKHandleDictionaryDisposeProtectedTest : SKTest
	{
		// --- dispose-protected fresh construction sets the IgnorePublicDispose latch ---

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

		// --- Promote (disposeProtected) racing a concurrent public Dispose() on the SAME handle.
		//     PreventPublicDisposal (under the instancesLock) and Dispose()'s IgnorePublicDispose
		//     check + isDisposed CAS (under the write lock) are mutually exclusive, so the outcome is
		//     always well-defined and never torn: the disposeProtected caller ALWAYS gets back a live,
		//     protected wrapper (the original if it promoted in time, or a freshly reconstructed one if
		//     Dispose won the race). ---

		[SkippableFact]
		public void DisposeProtectedRacingPublicDisposeNeverTears ()
		{
			SkipOnPlatform (IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			const int iterations = 300;
			var created = new System.Collections.Concurrent.ConcurrentBag<FakeNativeObject> ();

			for (var i = 0; i < iterations; i++) {
				var handle = NextHandle ();

				var original = HandleDictionary.GetOrAddObject<FakeNativeObject> (
					handle, owns: false, unrefExisting: false,
					(h, o) => {
						var obj = new FakeNativeObject (h);
						created.Add (obj);
						return obj;
					});

				FakeNativeObject promoted = null;

				// Two dedicated threads rendezvous, then race promote-vs-dispose on the SAME handle.
				RunConcurrent (2, idx => {
					if (idx == 0) {
						// Promote: returns the live instance protected, or reconstructs if Dispose won.
						promoted = HandleDictionary.GetOrAddObject<FakeNativeObject> (
							handle, owns: false, unrefExisting: false, disposeProtected: true,
							(h, o) => {
								var obj = new FakeNativeObject (h);
								created.Add (obj);
								return obj;
							});
					} else {
						original.Dispose ();
					}
				}, deadlockMessage: "Promote-vs-dispose race deadlocked.");

				// The disposeProtected contract: always a live, protected wrapper for this handle.
				Assert.NotNull (promoted);
				Assert.False (promoted.IsDisposed);
				Assert.True (promoted.IgnorePublicDispose);
				Assert.True (HandleDictionary.GetInstance<FakeNativeObject> (handle, out var current));
				Assert.Same (promoted, current);
			}

			// Tear everything down via the internal path (ignores IgnorePublicDispose).
			foreach (var obj in created)
				obj.DisposeInternal ();
		}
	}
}
