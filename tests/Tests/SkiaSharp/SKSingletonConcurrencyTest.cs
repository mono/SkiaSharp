using System;
using System.Collections.Generic;
using Xunit;

namespace SkiaSharp.Tests
{
	// Proposed gap-filling tests for PR #4080.
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class SKSingletonConcurrencyTest : SKTest
	{
		// A real immortal singleton (sRGB, SKData.Empty, ...) is ALWAYS held alive for the process
		// lifetime by a static field, which is precisely why its registry WeakReference never goes null.
		// The immortal CountingObject fake below models such a singleton but, being a throwaway local,
		// would be collected after its test returns — leaving a dead (null-Target) WeakReference in
		// HandleDictionary.instances. Rooting it here reproduces the real singleton's permanent reachability
		// so the fake stays a faithful stand-in and the GarbageCleanupFixture sees a live singleton, not a
		// stale entry. (Immortal wrappers can never be deregistered by design, so rooting — not disposal —
		// is the correct cleanup, exactly as production does it.)
		private static readonly List<SKObject> ImmortalRoots = new List<SKObject> ();
		// PROBABILISTIC deadlock canary (NOT a deterministic race proof).
		//
		// Every thread touches the same set of singleton accessors. Each accessor's factory
		// transitively acquires several locks, e.g. SKTypeface.Default:
		//   defaultTypefaceLock -> SKFontManager.Default lock -> SKFontStyle cctor -> HD lock.
		// No other test exercises this multi-lock path (the author's concurrent test only
		// hammers CreateSrgb, a single-lock path). Because the threads run unsynchronized
		// after the barrier, at any instant they sit at different accessors and therefore
		// hold different locks. If the PRODUCT code ever acquires the same pair of locks in
		// opposite orders across two of these factories (a lock-order inversion in our code,
		// not one the test manufactures), this contention can surface it as a hang. The test
		// cannot create an inversion the product does not have, so a clean run is necessary
		// but not sufficient evidence.
		//
		// The Assert.Same checks are the deterministic part: a correct singleton must
		// always return one instance regardless of interleaving.
		//
		// Execution is delegated to RunConcurrent, which runs the body on N dedicated
		// background threads released together by a Barrier and joins them against a
		// shared deadline. That matters here: if the product code does have a
		// lock-order inversion across these factories, the contention surfaces as a
		// failed Join (a deterministic test FAILURE) instead of a hung suite, and the
		// background threads never keep the host process alive after a hang.
		[SkippableFact]
		public void AllSingletonAccessorsAreStableUnderContention()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			const int threadCount = 32;

			var colorSpaces = new SKColorSpace[threadCount];
			var colorSpaceLinears = new SKColorSpace[threadCount];
			var datas = new SKData[threadCount];
			var fontManagers = new SKFontManager[threadCount];
			var typefaces = new SKTypeface[threadCount];
			var empties = new SKTypeface[threadCount];
			var blenders = new SKBlender[threadCount];

			SKHandleDictionaryTestHelpers.RunConcurrent(threadCount, i =>
			{
				colorSpaces[i] = SKColorSpace.CreateSrgb();
				colorSpaceLinears[i] = SKColorSpace.CreateSrgbLinear();
				datas[i] = SKData.Empty;
				fontManagers[i] = SKFontManager.Default;
				typefaces[i] = SKTypeface.Default;
				empties[i] = SKTypeface.Empty;
				blenders[i] = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
			}, deadlockMessage: "Singleton accessors deadlocked under contention (possible product-side lock-order inversion).");

			AssertAllSame(colorSpaces);
			AssertAllSame(colorSpaceLinears);
			AssertAllSame(datas);
			AssertAllSame(fontManagers);
			AssertAllSame(typefaces);
			AssertAllSame(empties);
			AssertAllSame(blenders);

			Assert.True(colorSpaces[0].IgnorePublicDispose);
			Assert.False(colorSpaces[0].IsDisposed);
			Assert.True(fontManagers[0].IgnorePublicDispose);
			Assert.False(fontManagers[0].IsDisposed);
			Assert.True(typefaces[0].IgnorePublicDispose);
			Assert.False(typefaces[0].IsDisposed);
		}

		// DETERMINISTIC clear-path test for the PR's central new behavior.
		//
		// Uses the same seam as SKObjectTest.ImmediateRecreationObject: override
		// DisposeNative() to run adversarial code at the exact mid-dispose window.
		// At that point isDisposed==1, Handle is still the original, and the wrapper
		// is STILL registered in the HandleDictionary (deregistration happens later,
		// when Handle is set to zero). Crucially, the new Dispose() design releases
		// the HD write lock BEFORE running cleanup, so a re-entrant GetObject here
		// acquires the lock cleanly.
		//
		// This forces exactly the interleaving "look up a handle whose registered
		// wrapper is disposed-but-not-yet-deregistered" and asserts:
		//   1. GetInstanceNoLocks filters the disposed wrapper (returns a fresh one).
		//   2. RegisterHandle replaces the disposed entry WITHOUT double-disposing it.
		//   3. The trailing DeregisterHandle(this) is a no-op because the entry now
		//      points at the fresh wrapper (weak.Target != this), so the replacement
		//      survives.
		[SkippableFact]
		public void DisposedWrapperIsFilteredAndReplacedDuringReentrantGetObject()
		{
			var handle = GetNextPtr();

			var original = ReentrantGetObject.Create(handle, refetchDuringDispose: true);
			Assert.Same(original, HandleDictionary.instances[handle].Target);

			original.Dispose();

			// The flag was captured INSIDE DisposeNative, proving isDisposed was
			// already set while we were still registered.
			Assert.True(original.WasDisposedDuringRefetch);

			// The disposed wrapper was filtered out: we got a brand new instance.
			var refetched = original.Refetched;
			Assert.NotNull(refetched);
			Assert.NotSame(original, refetched);
			Assert.False(refetched.IsDisposed);

			// The replacement was NOT disposed by RegisterHandle (no double dispose).
			Assert.False(refetched.DestroyedNative);

			// The dictionary now holds the fresh wrapper, and the disposed one's
			// trailing DeregisterHandle did not evict it.
			Assert.True(SKObject.GetInstance<ReentrantGetObject>(handle, out var current));
			Assert.Same(refetched, current);

			refetched.Dispose();
			Assert.False(SKObject.GetInstance<ReentrantGetObject>(handle, out _));
		}

		// DETERMINISTIC, NATIVE-FREE mechanism proof for the immortal latch.
		//
		// A real process-global singleton (SKColorSpace.CreateSrgb, SKData.Empty, ...) is an owns:true
		// wrapper whose DisposeNative would unref/free the SHARED native object. The lifecycle rework moved
		// the isDisposed CAS out of Dispose(bool) and into the three entry points, and IgnorePublicDispose
		// only guards the PUBLIC Dispose(). So DisposeInternal() (owned-child teardown, ownership handoff,
		// stale-wrapper replacement) and the finalizer could still reach DisposeNative on a singleton and
		// free the global out from under every other consumer. The immortal latch closes those two paths.
		//
		// These two tests are a matched pair over the SAME owning wrapper and SAME entry point
		// (DisposeInternal): the ONLY difference is whether the wrapper is latched immortal. Immortal => the
		// native-free path must never run; mortal (the control) => it must run exactly once. A counting fake
		// stands in for the native object so the proof needs no real Skia allocation and is fully
		// deterministic on every platform.

		[SkippableFact]
		public void ImmortalSingletonDisposeInternalDoesNotFreeNativeObject()
		{
			var handle = GetNextPtr();
			var obj = CountingObject.CreateImmortal(handle);

			// Pin the fake for the process lifetime, mirroring how every real immortal singleton is held
			// by a static field. Without this the throwaway local is collected after the test, leaving a
			// dead WeakReference in the registry that the immortal latch (by design) can never deregister.
			ImmortalRoots.Add(obj);

			Assert.True(obj.IsImmortalSingleton);
			Assert.True(obj.IgnorePublicDispose);

			// DisposeInternal bypasses the public-dispose guard; call it twice to also prove idempotence.
			obj.DisposeInternal();
			obj.DisposeInternal();

			Assert.Equal(0, obj.DisposeNativeCount);
			Assert.False(obj.IsDisposed);
			Assert.Equal(handle, obj.Handle);
			Assert.True(SKObject.GetInstance<CountingObject>(handle, out var still));
			Assert.Same(obj, still);
		}

		[SkippableFact]
		public void NonImmortalObjectDisposeInternalFreesNativeObjectExactlyOnce()
		{
			// Control: identical owning wrapper and identical DisposeInternal() call, but NOT immortal.
			// The native-free path runs exactly once and the wrapper deregisters — proving the immortal
			// latch (the only variable between the two tests) is what suppresses the free above.
			var handle = GetNextPtr();
			var obj = CountingObject.CreateMortal(handle);

			Assert.False(obj.IsImmortalSingleton);

			obj.DisposeInternal();
			obj.DisposeInternal();

			Assert.Equal(1, obj.DisposeNativeCount);
			Assert.True(obj.IsDisposed);
			Assert.False(SKObject.GetInstance<CountingObject>(handle, out _));
		}

		private static void AssertAllSame<T>(T[] instances)
			where T : class
		{
			Assert.NotNull(instances[0]);
			for (var i = 1; i < instances.Length; i++)
				Assert.Same(instances[0], instances[i]);
		}

		// An OWNING fake (owns: true) whose DisposeNative would, in production, unref/free a real native
		// object. It counts how many times that free path actually runs so the immortal latch's suppression
		// of it can be asserted directly. DisposeNativeCount is only read after the disposing thread has
		// returned, so a plain field is sufficient.
		private class CountingObject : SKObject
		{
			public int DisposeNativeCount;

			public CountingObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}

			protected override void DisposeNative() =>
				DisposeNativeCount++;

			// Drives the real immortal accessor path: dispose-protected + immortal latched inside
			// HandleDictionary under the registry lock, exactly as the production singleton accessors do.
			public static CountingObject CreateImmortal(IntPtr handle) =>
				GetOrAddImmortalSingletonObject(handle, true, true, (h, o) => new CountingObject(h, o));

			public static CountingObject CreateMortal(IntPtr handle) =>
				GetOrAddObject(handle, true, (h, o) => new CountingObject(h, o));
		}

		private class ReentrantGetObject : SKObject
		{
			private readonly bool refetchDuringDispose;

			public ReentrantGetObject(IntPtr handle, bool owns, bool refetchDuringDispose)
				: base(handle, owns)
			{
				this.refetchDuringDispose = refetchDuringDispose;
			}

			public bool DestroyedNative { get; private set; }

			public bool WasDisposedDuringRefetch { get; private set; }

			public ReentrantGetObject Refetched { get; private set; }

			protected override void DisposeNative()
			{
				if (refetchDuringDispose)
				{
					WasDisposedDuringRefetch = IsDisposed;
					Refetched = Create(Handle, refetchDuringDispose: false);
				}

				DestroyedNative = true;
				base.DisposeNative();
			}

			public static ReentrantGetObject Create(IntPtr handle, bool refetchDuringDispose) =>
				GetOrAddObject(handle, true, (h, o) => new ReentrantGetObject(h, o, refetchDuringDispose));
		}
	}
}
