using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKDrawableTest : SKTest
	{
		[SkippableFact]
		public void CanInstantiateDrawable()
		{
			using (var drawable = new TestDrawable())
			{
			}
		}

		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void CanAccessBounds()
		{
			using (var drawable = new TestDrawable())
			{
				Assert.Equal(SKRect.Create(100, 100), drawable.Bounds);
				Assert.Equal(1, drawable.BoundsFireCount);
			}
		}

		[SkippableFact]
		public void CanAccessApproxBytes()
		{
			using (var drawable = new TestDrawable())
			{
				Assert.Equal(123, drawable.ApproximateBytesUsed);
				Assert.Equal(1, drawable.ApproxBytesCount);
			}
		}

		[SkippableFact]
		public void CanCreateSnapshot()
		{
			SkipOnPlatform(IsMac || IsIOS || IsMacCatalyst, "sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues on Apple platforms");

			using (var drawable = new TestDrawable())
			{
				var picture = drawable.Snapshot();
				Assert.NotNull(picture);
				Assert.Equal(1, drawable.SnapshotFireCount);
			}
		}

		[SkippableFact]
		public void CanUseAllMembers()
		{
			SkipOnPlatform(IsMac || IsIOS || IsMacCatalyst, "sk_sp<SkPicture> SkDrawable::onMakePictureSnapshot() is causing issues on Apple platforms");

			using (var drawable = new TestDrawable())
			{
				Assert.Equal(SKRect.Create(100, 100), drawable.Bounds);
				Assert.Equal(1, drawable.BoundsFireCount);

				using (var bmp = new SKBitmap(100, 100))
				using (var canvas = new SKCanvas(bmp))
				{
					drawable.Draw(canvas, 0, 0);
					Assert.Equal(1, drawable.DrawFireCount);

					canvas.DrawDrawable(drawable, 0, 0);
					Assert.Equal(2, drawable.DrawFireCount);
				}

				var picture = drawable.Snapshot();
				Assert.NotNull(picture);
				Assert.Equal(1, drawable.SnapshotFireCount);
			}
		}

		[SkippableFact]
		public void DrawableDrawDraws()
		{
			using (var drawable = new TestDrawable())
			using (var bmp = new SKBitmap(100, 100))
			using (var canvas = new SKCanvas(bmp))
			{
				drawable.Draw(canvas, 0, 0);

				Assert.Equal(SKColors.Blue, bmp.GetPixel(50, 50));
			}
		}

		[SkippableFact]
		public void CanvasDrawsDrawable()
		{
			using (var drawable = new TestDrawable())
			using (var bmp = new SKBitmap(100, 100))
			using (var canvas = new SKCanvas(bmp))
			{
				canvas.DrawDrawable(drawable, 0, 0);

				Assert.Equal(SKColors.Blue, bmp.GetPixel(50, 50));
			}
		}

		// Lifecycle / concurrency coverage for the fromNative destroy-callback path.
		//
		// A managed-created SKDrawable owns a single native reference (refcount 1).
		// Disposing it runs DisposeNative() -> sk_drawable_unref, which drops the
		// native refcount to 0 and SYNCHRONOUSLY re-enters the managed wrapper via
		// SKManagedDrawableDestroyProxy on the SAME thread. That proxy sets
		// fromNative = 1 and calls Dispose() again. Under the lock-paired SKObject
		// disposal the outer Dispose() has already released the per-handle write lock
		// before DisposeNative() runs, so the re-entrant Dispose() acquires the lock
		// fresh and no-ops on the already-claimed isDisposed flag (no recursion
		// deadlock, no double-free). These tests pin that invariant.

		[SkippableFact]
		public void DisposingManagedDrawableFiresNativeDestroyCallback()
		{
			var drawable = new TestDrawable();
			var handle = drawable.Handle;

			try
			{
				Assert.NotEqual(IntPtr.Zero, handle);
				Assert.Equal(0, drawable.fromNative);
				Assert.True(HandleDictionary.GetInstance<SKDrawable>(handle, out var live));
				Assert.Same(drawable, live);
			}
			finally
			{
				drawable.Dispose();
			}

			// The synchronous native destroy callback must have re-entered and flipped fromNative.
			Assert.Equal(1, drawable.fromNative);
			Assert.True(drawable.IsDisposed);
			// The wrapper must be deregistered so no stale entry can be promoted.
			Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));
		}

		[SkippableFact]
		public void DisposingManagedDrawableTwiceIsNoOp()
		{
			var drawable = new TestDrawable();
			var handle = drawable.Handle;

			drawable.Dispose();
			Assert.Equal(1, drawable.fromNative);
			Assert.True(drawable.IsDisposed);

			// Second dispose must be a safe no-op (CAS already claimed, native already freed).
			drawable.Dispose();
			Assert.Equal(1, drawable.fromNative);
			Assert.True(drawable.IsDisposed);
			Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));
		}

		[SkippableFact]
		public void ConcurrentDisposeOfSameDrawableIsIdempotent()
		{
			// Many threads racing Dispose() on the SAME wrapper must all funnel through the
			// single isDisposed CAS in SKObject.Dispose: exactly one thread runs cleanup,
			// the native object is freed once, and the re-entrant destroy callback flips
			// fromNative exactly once. No double-free, no crash.
			var drawable = new TestDrawable();
			var handle = drawable.Handle;

			Parallel.For(0, 64, _ => drawable.Dispose());

			Assert.Equal(1, drawable.fromNative);
			Assert.True(drawable.IsDisposed);
			Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));
		}

		[SkippableFact]
		public void NativeOwnerOutlivingManagedDrawableDefersDestroyExactlyOnce()
		{
			// Forces the "native outlives managed" path: a live SKPictureRecorder retains a
			// native reference to the drawable across the managed wrapper's Dispose(). This is
			// the lifecycle-sensitive case for the fromNative destroy-callback machinery.
			//
			// Invariants under test:
			//  - Managed Dispose() only UNREFS (does not fire destroy) while a native owner
			//    still holds a reference, yet the wrapper is fully disposed + deregistered and
			//    its GCHandle is kept alive (freed later by the destroy proxy, not now). This is
			//    proven by asserting fromNative == 0 immediately after the managed dispose.
			//  - Once the LAST native owner is released, the deferred destroy callback fires
			//    against the already-disposed wrapper: re-entrant Dispose() no-ops on the CAS,
			//    DisposeNative() is not re-run, the GCHandle is freed. fromNative latches to 1.
			//    No double-free, no use-after-free, no crash.
			//
			// If a future Skia version stops retaining the drawable in the recorder, the
			// fromNative == 0 assertion below will fail loudly — that is intentional: it signals
			// the deferred-destroy path is no longer exercised here and needs a new native owner.
			SKPictureRecorder recorder = null;
			SKPicture picture = null;
			TestDrawable drawable = null;
			try
			{
				recorder = new SKPictureRecorder();
				var recordingCanvas = recorder.BeginRecording(SKRect.Create(100, 100));
				drawable = new TestDrawable();
				var handle = drawable.Handle;
				recordingCanvas.DrawDrawable(drawable, 0, 0);
				picture = recorder.EndRecording();

				// Wrapper disposed while the recorder still owns a native reference: managed
				// Dispose() only unrefs, so the destroy callback has NOT fired yet.
				drawable.Dispose();
				Assert.Equal(0, drawable.fromNative);
				Assert.True(drawable.IsDisposed);
				Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));

				// Releasing every native owner (recorder, then the snapshot picture) drives the
				// native refcount to zero and fires the deferred destroy callback.
				recorder.Dispose();
				picture.Dispose();

				Assert.Equal(1, drawable.fromNative);
				Assert.True(drawable.IsDisposed);
				Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));

				// A second managed Dispose() after the deferred destroy must remain a safe no-op.
				drawable.Dispose();
				Assert.Equal(1, drawable.fromNative);
			}
			finally
			{
				drawable?.Dispose();
				picture?.Dispose();
				recorder?.Dispose();
			}
		}

		[SkippableFact]
		public void ConcurrentDisposeOfDrawableAndNativeOwnerIsSafe()
		{
			// Stress test: races managed Dispose() against release of the native owner that
			// keeps the drawable alive. Whichever unref observes refcount 0 fires the single
			// destroy callback; the other path no-ops on the CAS. Repeated to shake out
			// interleavings. (This cannot deterministically pin the narrow window between the
			// fromNative read and sk_drawable_unref without production/native hooks, so it is a
			// repeated stress test rather than a deterministic interleaving reproducer.) The
			// invariant verified at each join: destroy fired (fromNative latched to 1), the
			// wrapper is disposed + deregistered, and no crash/double-free occurred.
			const int iterations = 200;

			for (var i = 0; i < iterations; i++)
			{
				SKPictureRecorder recorder = null;
				SKPicture picture = null;
				TestDrawable drawable = null;
				try
				{
					recorder = new SKPictureRecorder();
					var recordingCanvas = recorder.BeginRecording(SKRect.Create(10, 10));
					drawable = new TestDrawable();
					var handle = drawable.Handle;
					recordingCanvas.DrawDrawable(drawable, 0, 0);
					picture = recorder.EndRecording();

					var localDrawable = drawable;
					var localRecorder = recorder;
					var localPicture = picture;
					using (var barrier = new System.Threading.Barrier(2))
					{
						Parallel.Invoke(
							() => { barrier.SignalAndWait(); localDrawable.Dispose(); },
							() => { barrier.SignalAndWait(); localRecorder.Dispose(); localPicture.Dispose(); });
					}

					Assert.Equal(1, drawable.fromNative);
					Assert.True(drawable.IsDisposed);
					Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));
				}
				finally
				{
					drawable?.Dispose();
					picture?.Dispose();
					recorder?.Dispose();
				}
			}
		}

		[SkippableFact]
		public void RecordedPictureIsIndependentOfManagedDrawableLifetime()
		{
			// Recording a drawable into an SKPicture must not couple the picture's
			// renderability to the managed wrapper's lifetime: disposing the wrapper
			// after recording is crash-safe and the picture keeps rendering.
			//
			// NOTE on destroy timing: whether the native destroy callback fires during
			// the wrapper's Dispose() depends on whether a native owner still holds a
			// reference at that moment (e.g. the live SKPictureRecorder retains the
			// drawable until it is itself disposed, whereas a finished SKPicture keeps
			// only a snapshot). That timing is a Skia-internal detail and intentionally
			// NOT asserted here. What MUST hold on every path is: no crash, the wrapper
			// is disposed + deregistered, the destroy callback never fires more than once
			// (fromNative ends at 0 or 1, never higher), and playback still renders.
			SKDrawable drawable;
			IntPtr handle;

			using var recorder = new SKPictureRecorder();
			var recordingCanvas = recorder.BeginRecording(SKRect.Create(100, 100));
			drawable = new TestDrawable();
			handle = drawable.Handle;
			recordingCanvas.DrawDrawable(drawable, 0, 0);
			using var picture = recorder.EndRecording();

			drawable.Dispose();
			Assert.True(drawable.IsDisposed);
			Assert.False(HandleDictionary.GetInstance<SKDrawable>(handle, out _));
			Assert.InRange(drawable.fromNative, 0, 1);

			// The recorded picture renders regardless of the drawable wrapper's state.
			using var bmp = new SKBitmap(100, 100);
			using var canvas = new SKCanvas(bmp);
			canvas.DrawPicture(picture);
			Assert.Equal(SKColors.Blue, bmp.GetPixel(50, 50));
		}

		[SkippableFact]
		public void ConcurrentDisposeOfManyManagedDrawablesIsSafe()
		{
			const int count = 128;

			var drawables = new TestDrawable[count];
			var handles = new IntPtr[count];
			for (var i = 0; i < count; i++)
			{
				drawables[i] = new TestDrawable();
				handles[i] = drawables[i].Handle;
			}

			Parallel.For(0, count, i => drawables[i].Dispose());

			for (var i = 0; i < count; i++)
			{
				Assert.Equal(1, drawables[i].fromNative);
				Assert.True(drawables[i].IsDisposed);
				Assert.False(HandleDictionary.GetInstance<SKDrawable>(handles[i], out _));
			}
		}

		[SkippableFact]
		public void ConcurrentCreateUseAndDisposeOfDrawablesIsSafe()
		{
			const int threads = 8;
			const int perThread = 64;

			var exceptions = new List<Exception>();

			Parallel.For(0, threads, _ =>
			{
				try
				{
					for (var i = 0; i < perThread; i++)
					{
						using var drawable = new TestDrawable();
						// Touch a native member so registration + use + destroy all race.
						Assert.Equal(SKRect.Create(100, 100), drawable.Bounds);
					}
				}
				catch (Exception ex)
				{
					lock (exceptions)
						exceptions.Add(ex);
				}
			});

			Assert.Empty(exceptions);
		}
	}

	class TestDrawable : SKDrawable
	{
		public int DrawFireCount;
		public int BoundsFireCount;
		public int SnapshotFireCount;
		public int ApproxBytesCount;

		protected internal override void OnDraw(SKCanvas canvas)
		{
			DrawFireCount++;

			canvas.DrawColor(SKColors.Blue);
		}

		protected internal override SKRect OnGetBounds()
		{
			BoundsFireCount++;

			return SKRect.Create(100, 100);
		}

		protected internal override SKPicture OnSnapshot()
		{
			SnapshotFireCount++;

			return base.OnSnapshot();
		}

		protected internal override int OnGetApproximateBytesUsed ()
		{
			ApproxBytesCount++;

			return 123;
		}
	}
}
