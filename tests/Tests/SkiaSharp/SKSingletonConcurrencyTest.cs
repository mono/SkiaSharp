using System;
using Xunit;

namespace SkiaSharp.Tests
{
	// Proposed gap-filling tests for PR #4080.
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class SKSingletonConcurrencyTest : SKTest
	{
		// PROBABILISTIC deadlock canary (NOT a deterministic race proof).
		//
		// Every thread touches the same set of singleton accessors. Each accessor's factory
		// transitively runs its type's static constructor (which reads raw handles from
		// SkiaSharpStatics) and then takes the HandleDictionary lock to dedup/latch the wrapper.
		// No other test exercises this many distinct singleton factories at once (the author's
		// concurrent test only hammers CreateSrgb, a single factory). Because the threads run
		// unsynchronized after the barrier, at any instant they sit at different accessors and
		// therefore hold different locks. If the PRODUCT code ever acquires the same pair of locks in
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
			var srgbToLinear = new SKColorFilter[threadCount];
			var linearToSrgb = new SKColorFilter[threadCount];
			var fontStyles = new SKFontStyle[threadCount];

			SKHandleDictionaryTestHelpers.RunConcurrent(threadCount, i =>
			{
				colorSpaces[i] = SKColorSpace.CreateSrgb();
				colorSpaceLinears[i] = SKColorSpace.CreateSrgbLinear();
				datas[i] = SKData.Empty;
				fontManagers[i] = SKFontManager.Default;
				typefaces[i] = SKTypeface.Default;
				empties[i] = SKTypeface.Empty;
				blenders[i] = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
				srgbToLinear[i] = SKColorFilter.CreateSrgbToLinearGamma();
				linearToSrgb[i] = SKColorFilter.CreateLinearToSrgbGamma();
				fontStyles[i] = SKFontStyle.Normal;
			}, deadlockMessage: "Singleton accessors deadlocked under contention (possible product-side lock-order inversion).");

			AssertAllSame(colorSpaces);
			AssertAllSame(colorSpaceLinears);
			AssertAllSame(datas);
			AssertAllSame(fontManagers);
			AssertAllSame(typefaces);
			AssertAllSame(empties);
			AssertAllSame(blenders);
			AssertAllSame(srgbToLinear);
			AssertAllSame(linearToSrgb);
			AssertAllSame(fontStyles);

			Assert.True(colorSpaces[0].IgnorePublicDispose);
			Assert.False(colorSpaces[0].IsDisposed);
			Assert.True(fontManagers[0].IgnorePublicDispose);
			Assert.False(fontManagers[0].IsDisposed);
			Assert.True(typefaces[0].IgnorePublicDispose);
			Assert.False(typefaces[0].IsDisposed);
			Assert.True(srgbToLinear[0].IgnorePublicDispose);
			Assert.False(srgbToLinear[0].IsDisposed);
			Assert.True(fontStyles[0].IgnorePublicDispose);
			Assert.False(fontStyles[0].IsDisposed);
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

		// DETERMINISTIC, NATIVE-FREE mechanism check for DisposeInternal().
		//
		// An owns:true wrapper's DisposeNative would, in production, unref/free the native object. The
		// lifecycle rework moved the isDisposed CAS out of Dispose(bool) and into the three entry points.
		// This test pins down the contract for an ordinary (non-singleton) owning wrapper: DisposeInternal()
		// runs the native-free path exactly once — even when called repeatedly — and then deregisters. A
		// counting fake stands in for the native object so the proof needs no real Skia allocation and is
		// fully deterministic on every platform.
		//
		// (Singletons are NOT exercised through DisposeInternal here: no production path calls
		// DisposeInternal()/the finalizer on a singleton — they are rooted by static fields and deduped to a
		// single dispose-protected wrapper — so there is nothing to assert beyond this owning-wrapper contract.)

		[SkippableFact]
		public void OwningObjectDisposeInternalFreesNativeObjectExactlyOnce()
		{
			var handle = GetNextPtr();
			var obj = CountingObject.CreateMortal(handle);

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
		// object. It counts how many times that free path actually runs. DisposeNativeCount is only read
		// after the disposing thread has returned, so a plain field is sufficient.
		private class CountingObject : SKObject
		{
			public int DisposeNativeCount;

			public CountingObject(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}

			protected override void DisposeNative() =>
				DisposeNativeCount++;

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
