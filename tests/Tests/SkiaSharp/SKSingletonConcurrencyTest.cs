using System;
using System.Collections.Concurrent;
using System.Threading;
using Xunit;

namespace SkiaSharp.Tests
{
	// Proposed gap-filling tests for PR #4080.
	public class SKSingletonConcurrencyTest : SKTest
	{
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
		// NOTE: explicit Thread[] is used deliberately instead of Parallel.For. A
		// Barrier(N) combined with Parallel.For can deadlock spuriously because
		// Parallel.For does not guarantee N concurrent workers — a worker blocked on
		// SignalAndWait never runs its remaining assigned iterations, so the barrier
		// may never reach N participants. Real threads guarantee the barrier is
		// satisfiable, so a hang here means a genuine product-side lock-ordering bug.
		[SkippableFact]
		public void AllSingletonAccessorsAreStableUnderContention()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			const int threadCount = 32;
			using var barrier = new Barrier(threadCount);

			var colorSpaces = new SKColorSpace[threadCount];
			var colorSpaceLinears = new SKColorSpace[threadCount];
			var datas = new SKData[threadCount];
			var fontManagers = new SKFontManager[threadCount];
			var typefaces = new SKTypeface[threadCount];
			var empties = new SKTypeface[threadCount];
			var blenders = new SKBlender[threadCount];

			var errors = new ConcurrentBag<Exception>();
			var threads = new Thread[threadCount];
			for (var t = 0; t < threadCount; t++)
			{
				var i = t;
				threads[i] = new Thread(() =>
				{
					try
					{
						barrier.SignalAndWait();
						colorSpaces[i] = SKColorSpace.CreateSrgb();
						colorSpaceLinears[i] = SKColorSpace.CreateSrgbLinear();
						datas[i] = SKData.Empty;
						fontManagers[i] = SKFontManager.Default;
						typefaces[i] = SKTypeface.Default;
						empties[i] = SKTypeface.Empty;
						blenders[i] = SKBlender.CreateBlendMode(SKBlendMode.SrcOver);
					}
					catch (Exception ex)
					{
						errors.Add(ex);
					}
				});
			}

			foreach (var thread in threads)
				thread.Start();
			foreach (var thread in threads)
				thread.Join();

			Assert.Empty(errors);
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

		private static void AssertAllSame<T>(T[] instances)
			where T : class
		{
			Assert.NotNull(instances[0]);
			for (var i = 1; i < instances.Length; i++)
				Assert.Same(instances[0], instances[i]);
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
