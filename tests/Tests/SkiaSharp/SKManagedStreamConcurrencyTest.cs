using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests
{
	[Collection (HandleDictionaryThreadingCollection.Name)]
	public class SKManagedStreamConcurrencyTest : SKTest
	{
		[SkippableFact]
		public void ConcurrentDisposeOfSameManagedStreamIsIdempotent()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// Many threads racing Dispose() on the SAME wrapper must funnel through the single
			// isDisposed CAS: one cleanup, native freed once, destroy callback flips fromNative once.
			var stream = new SKManagedStream(CreateTestStream(), true);
			var handle = stream.Handle;

			SKHandleDictionaryTestHelpers.RunWithTimeout(
				() => Parallel.For(0, 64, _ => stream.Dispose()),
				deadlockMessage: "Concurrent Dispose() of the same managed stream deadlocked.");

			Assert.Equal(1, stream.fromNative);
			Assert.True(stream.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public void ConcurrentDisposeOfManyManagedStreamsIsSafe()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// Mobile interpreter runtimes (iOS / Mac Catalyst / Android) schedule the thread pool too
			// sparsely to push 128 work items through the native dispose path before the timeout, so the
			// stress is sized down there. The work is driven through dedicated threads (RunConcurrent),
			// not Parallel.For: all N threads rendezvous at a barrier and then Dispose() simultaneously,
			// which is both deterministic (no pool dependency) and a stronger test of the registry's
			// concurrent-deregister path than pool-scheduled work items.
			var count = (IsAndroid || IsIOS || IsMacCatalyst) ? 16 : 128;
			var streams = new List<SKManagedStream>(count);
			for (var i = 0; i < count; i++)
				streams.Add(new SKManagedStream(CreateTestStream(), true));

			SKHandleDictionaryTestHelpers.RunConcurrent(
				count,
				i => streams[i].Dispose(),
				deadlockMessage: "Concurrent Dispose() of many managed streams deadlocked.");

			foreach (var stream in streams)
			{
				Assert.Equal(1, stream.fromNative);
				Assert.True(stream.IsDisposed);
			}
		}

		// After SKCodec.Create reparents the managed stream (OwnsHandle=false, IgnorePublicDispose
		// latched under the lock), the user STILL holds the wrapper and may call Dispose() on it
		// from another thread. The PR's lock-paired public Dispose must IGNORE that call so the codec
		// keeps reading through a live stream and remains its sole disposer. Before the LAZY-reparent
		// fix an eager public close here tore the native stream out from under an in-flight native read
		// -> use-after-free / host crash. The existing concurrency tests only race Dispose() against
		// itself on an OWNED stream; these two pin the reparented supported concurrency: (1) an ignored
		// public Dispose racing a lazy native read, and (2) an ignored public Dispose racing the codec's
		// owner-teardown (which must still be the single, exactly-once disposer of the wrapper).

		[SkippableFact]
		public unsafe void PublicDisposeWhileCodecReadIsInFlightIsIgnored()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// DETERMINISTIC version of the race: park the native lazy read INSIDE the .NET
			// Stream.Read() callback, fire the (reparented) wrapper's public Dispose() while the
			// native read is provably in-flight, then release the read. The public Dispose must be
			// ignored (IgnorePublicDispose latched), so the in-flight native read completes against a
			// live stream and GetPixels succeeds. This is the exact use-after-free the LAZY-reparent
			// fix prevents — an eager public close here would close the .NET stream mid-read.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			using var readEntered = new ManualResetEventSlim(false);
			using var releaseRead = new ManualResetEventSlim(false);

			var dotnet = new GatedCountingStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;
			var codec = SKCodec.Create(stream);
			ThreadResult<SKCodecResult> reader = null;
			try
			{
				Assert.False(stream.OwnsHandle);
				Assert.True(stream.IgnorePublicDispose);

				// Arm only AFTER Create() so the gate trips on a GetPixels read, not header parsing.
				dotnet.Arm(readEntered, releaseRead);

				reader = SKHandleDictionaryTestHelpers.RunOnThread (() => codec.GetPixels (out _));

				Assert.True(readEntered.Wait(TimeSpan.FromSeconds(30)), "Native lazy read never entered the managed stream.");

				// The native read is parked inside Read(); a public Dispose now must be a no-op.
				stream.Dispose();
				Assert.False(stream.IsDisposed);

				releaseRead.Set();
				Assert.True(reader.Wait(30_000), "GetPixels did not complete after the read was released.");

				// If the ignored Dispose had wrongly closed the .NET stream, the parked inner.Read would
				// have thrown and GetPixels would not be Success; DisposeCount would also not be 0.
				Assert.Equal(SKCodecResult.Success, reader.Result);
				Assert.False(stream.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);
			}
			finally
			{
				// Drain the reader before tearing the codec down so no background decode runs against a
				// disposed codec. Swallow here so a primary in-try assertion failure is not masked.
				releaseRead.Set();
				try { reader?.Wait(30_000); } catch { }
				codec.Dispose();
			}

			// The codec is the sole disposer; the underlying .NET stream was closed exactly once.
			Assert.True(stream.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public unsafe void PublicDisposeAroundLazyCodecReadIsIgnoredStress()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// STRESS version: barrier-synchronised public Dispose vs a full GetPixels, repeated many
			// times to shake out interleavings beyond the single overlap the deterministic gate pins.
			// (The barrier only co-starts the two operations; it does NOT guarantee Dispose overlaps an
			// in-flight Read — that exact overlap is proven deterministically by the gated test above.)
			const int iterations = 100;
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			for (var i = 0; i < iterations; i++)
			{
				var stream = new SKManagedStream(new MemoryStream(bytes), true);
				var handle = stream.Handle;
				var codec = SKCodec.Create(stream);
				try
				{
					Assert.False(stream.OwnsHandle);
					Assert.True(stream.IgnorePublicDispose);

					var result = SKCodecResult.InternalError;
					using (var barrier = new Barrier(2))
					{
						// Dedicated threads (not Task.Run) so the Barrier(2) always has both participants
						// present even under full-suite load; see RunConcurrent rationale.
						var read = SKHandleDictionaryTestHelpers.RunOnThread (() => { barrier.SignalAndWait (); return codec.GetPixels (out _); });
						var dispose = SKHandleDictionaryTestHelpers.RunOnThread (() => { barrier.SignalAndWait (); stream.Dispose (); });
						Assert.True (read.Wait (30_000) & dispose.Wait (30_000), "Dispose/GetPixels race did not complete in time.");
						result = read.Result;
					}

					// The lazy native read saw a live stream; the racing public Dispose was a no-op.
					Assert.Equal(SKCodecResult.Success, result);
					Assert.False(stream.IsDisposed);
					Assert.True(HandleDictionary.GetInstance<SKManagedStream>(handle, out _));
				}
				finally
				{
					codec.Dispose();
				}

				// The codec is the sole disposer; teardown ran after the race.
				Assert.Equal(1, stream.fromNative);
				Assert.True(stream.IsDisposed);
				SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
			}
		}

		[SkippableFact]
		public unsafe void PublicDisposeRacingCodecTeardownClosesManagedStreamExactlyOnce()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// Ignored public Dispose (no-ops on IgnorePublicDispose) racing the codec's owner-teardown
			// (the sole disposer, via DisposeUnownedManaged -> child.DisposeInternal). The underlying
			// .NET stream must be closed EXACTLY once — proven by DisposeCount. fromNative is only a
			// one-way latch (Interlocked.Exchange), so fromNative==1 asserts the native destroy callback
			// was OBSERVED, not that it fired exactly once; DisposeCount carries the exactly-once proof.
			const int iterations = 200;
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			for (var i = 0; i < iterations; i++)
			{
				var dotnet = new GatedCountingStream(bytes);
				var stream = new SKManagedStream(dotnet, true);
				var handle = stream.Handle;
				var codec = SKCodec.Create(stream);
				try
				{
					Assert.False(stream.OwnsHandle);
					Assert.True(stream.IgnorePublicDispose);

					using (var barrier = new Barrier(2))
					{
						var teardownThreadId = 0;
						// Dedicated threads (not Task.Run) so the Barrier(2) always has both participants.
						var dispose = SKHandleDictionaryTestHelpers.RunOnThread (() => { barrier.SignalAndWait (); stream.Dispose (); });
						var teardown = SKHandleDictionaryTestHelpers.RunOnThread (() => { barrier.SignalAndWait (); teardownThreadId = Environment.CurrentManagedThreadId; codec.Dispose (); });
						Assert.True (dispose.Wait (30_000) & teardown.Wait (30_000), "Dispose/teardown race did not complete in time.");

						// The ignored public Dispose never closes the stream; the codec owner-teardown is the
						// real disposer. Proven by the closing thread being the teardown task, not the public
						// Dispose task — DisposeCount==1 alone could not distinguish which path won.
						Assert.Equal (teardownThreadId, dotnet.FirstDisposeThreadId);

						// Keep both wrappers rooted across the race assertion so a GC + finalizer
						// can never flip FirstDisposeThreadId from the finalizer thread mid-assert.
						GC.KeepAlive (stream);
						GC.KeepAlive (dotnet);
					}
				}
				finally
				{
					// Idempotent: redundant if the racing thread already tore the codec down.
					codec.Dispose();
				}

				// Ignored public Dispose + single owner teardown => one .NET stream close, one latch flip.
				Assert.Equal(1, dotnet.DisposeCount);
				Assert.Equal(1, stream.fromNative);
				Assert.True(stream.IsDisposed);
				SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
			}
		}

		[SkippableFact]
		public unsafe void NonSeekableStreamReparentedToCodecTearsDownNestedStreamExactlyOnce()
		{
			// A NON-SEEKABLE stream routes SKCodec.Create through SKFrontBufferedManagedStream, which
			// holds a private *inner* SKManagedStream wrapping the user's .NET stream. Only the OUTER
			// front-buffered wrapper is reparented onto the codec (OwnsHandle=false, IgnorePublicDispose
			// =true); the inner SKManagedStream is UNTOUCHED (still owns its handle, still forwards public
			// Dispose). The user's racing fb.Dispose() must be a no-op, and codec teardown must walk
			// fb -> inner -> .NET stream and close the .NET stream EXACTLY once. We supply the inner
			// explicitly (public ctor) so both wrappers' deregistration is asserted without reflection.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var inner = new SKManagedStream(dotnet, true);
			var innerHandle = inner.Handle;
			var fb = new SKFrontBufferedManagedStream(inner, SKCodec.MinBufferedBytesNeeded, true);
			var fbHandle = fb.Handle;
			var codec = SKCodec.Create(fb);
			try
			{
				Assert.NotNull(codec);

				// Only the outer front-buffered wrapper is reparented; the inner stays a normal owner.
				Assert.False(fb.OwnsHandle);
				Assert.True(fb.IgnorePublicDispose);
				Assert.True(inner.OwnsHandle);
				Assert.False(inner.IgnorePublicDispose);

				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out _));

				// The user still holds fb and (wrongly) disposes it: it is reparented, so this is a no-op.
				fb.Dispose();
				Assert.False(fb.IsDisposed);
				Assert.False(inner.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);
			}
			finally
			{
				codec.Dispose();
			}

			// Codec teardown is the sole disposer: fb -> inner -> .NET stream, closed exactly once.
			Assert.True(fb.IsDisposed);
			Assert.True(inner.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(innerHandle, inner);
		}

		[SkippableFact]
		public unsafe void PublicDisposeWhileFrontBufferedCodecReadIsInFlightIsIgnored()
		{
			SkipOnPlatform(IsBrowser, "WASM is single-threaded; this test requires real OS threads");

			// DETERMINISTIC in-flight-read race for the NESTED non-seekable graph. Park the native lazy
			// read inside the underlying .NET Stream.Read() (reached through fb's front buffer -> inner
			// SKManagedStream), fire the reparented fb's public Dispose() while that read is provably
			// in-flight, then release. The ignored public Dispose must not close anything mid-read, so
			// GetPixels succeeds and the .NET stream is closed exactly once by codec teardown.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			using var readEntered = new ManualResetEventSlim(false);
			using var releaseRead = new ManualResetEventSlim(false);

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var fb = new SKFrontBufferedManagedStream(dotnet, SKCodec.MinBufferedBytesNeeded, true);
			var fbHandle = fb.Handle;
			var codec = SKCodec.Create(fb);
			ThreadResult<SKCodecResult> reader = null;
			try
			{
				Assert.False(fb.OwnsHandle);
				Assert.True(fb.IgnorePublicDispose);

				// Arm only AFTER Create() so the gate trips on a GetPixels read past the front buffer, not
				// on the header bytes buffered during format detection.
				dotnet.Arm(readEntered, releaseRead);

				reader = SKHandleDictionaryTestHelpers.RunOnThread (() => codec.GetPixels (out _));

				Assert.True(readEntered.Wait(TimeSpan.FromSeconds(30)), "Native lazy read never reached the underlying non-seekable stream.");

				// The native read is parked inside the underlying Read(); a public Dispose now must no-op.
				fb.Dispose();
				Assert.False(fb.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);

				releaseRead.Set();
				Assert.True(reader.Wait(30_000), "GetPixels did not complete after the read was released.");

				Assert.Equal(SKCodecResult.Success, reader.Result);
				Assert.False(fb.IsDisposed);
				Assert.Equal(0, dotnet.DisposeCount);
			}
			finally
			{
				releaseRead.Set();
				try { reader?.Wait(30_000); } catch { }
				codec.Dispose();
			}

			// The codec is the sole disposer; the underlying .NET stream was closed exactly once.
			Assert.True(fb.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
		}

		[SkippableFact]
		public unsafe void InvalidNonSeekableStreamFailedCodecCreateClosesNestedStreamExactlyOnce()
		{
			// FAILURE path for the nested non-seekable graph: when the codec cannot be created, Create
			// still transfers ownership to a null native object, which disposes the front-buffered
			// wrapper immediately. That teardown must walk fb -> inner -> .NET stream and close it
			// exactly once, and both wrappers must deregister — no leak, no double-free.
			var bytes = new byte[256];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (byte)(i + 1);

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var inner = new SKManagedStream(dotnet, true);
			var innerHandle = inner.Handle;
			var fb = new SKFrontBufferedManagedStream(inner, SKCodec.MinBufferedBytesNeeded, true);
			var fbHandle = fb.Handle;

			var codec = SKCodec.Create(fb, out var result);

			Assert.Null(codec);
			Assert.NotEqual(SKCodecResult.Success, result);

			// The failed Create disposed fb immediately, which tore down the whole nested chain once.
			Assert.True(fb.IsDisposed);
			Assert.True(inner.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(innerHandle, inner);
		}

		[SkippableFact]
		public unsafe void InvalidSeekableManagedStreamFailedCodecCreateClosesStreamExactlyOnce()
		{
			// FAILURE path for the FLAT seekable graph (no front-buffering): a seekable managed Stream
			// of invalid bytes is wrapped in a single SKManagedStream. When the codec cannot be created,
			// Create transfers ownership to a null native object, so RevokeOwnership(null) disposes the
			// wrapper immediately. That teardown must close the .NET stream EXACTLY once and deregister
			// the wrapper — no leak, no double-free. Complements the non-seekable/front-buffered failure
			// test above, which exercises a different (nested) teardown graph.
			var bytes = new byte[256];
			for (var i = 0; i < bytes.Length; i++)
				bytes[i] = (byte)(i + 1);

			var dotnet = new GatedCountingStream(bytes);
			var stream = new SKManagedStream(dotnet, true);
			var handle = stream.Handle;

			Assert.True(stream.OwnsHandle);
			Assert.True(SKObject.GetInstance<SKManagedStream>(handle, out _));

			var codec = SKCodec.Create(stream, out var result);

			Assert.Null(codec);
			Assert.NotEqual(SKCodecResult.Success, result);

			// Failed Create disposed the flat wrapper, closing the .NET stream exactly once.
			Assert.True(stream.IsDisposed);
			Assert.Equal(1, dotnet.DisposeCount);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKManagedStream>(handle, stream);
		}

		[SkippableFact]
		public unsafe void FrontBufferedCodecWithoutOwnershipDoesNotCloseUnderlyingStream()
		{
			// disposeUnderlyingStream:false — the user keeps ownership of the .NET stream. The nested
			// SKManagedStream is still disposed by codec teardown, but it must NOT close the user's .NET
			// stream. We then close it ourselves to prove the count is driven solely by the user, not the
			// codec chain.
			var bytes = File.ReadAllBytes(Path.Combine(PathToImages, "color-wheel.png"));

			var dotnet = new NonSeekableGatedCountingStream(bytes);
			var fb = new SKFrontBufferedManagedStream(dotnet, SKCodec.MinBufferedBytesNeeded, false);
			var fbHandle = fb.Handle;
			var codec = SKCodec.Create(fb);
			try
			{
				Assert.NotNull(codec);
				Assert.False(fb.OwnsHandle);
				Assert.True(fb.IgnorePublicDispose);
				Assert.Equal(SKCodecResult.Success, codec.GetPixels(out _));
			}
			finally
			{
				codec.Dispose();
			}

			// Codec teardown disposed the wrappers but, lacking ownership, left the .NET stream open.
			Assert.True(fb.IsDisposed);
			SKHandleDictionaryTestHelpers.AssertDeregistered<SKFrontBufferedManagedStream>(fbHandle, fb);
			Assert.Equal(0, dotnet.DisposeCount);

			// The user is the only owner: closing now is the first and only close.
			dotnet.Dispose();
			Assert.Equal(1, dotnet.DisposeCount);
		}

		// A seekable .NET Stream over a byte buffer that (a) counts how many times it is disposed and
		// (b) can park the FIRST read after Arm() until released — used to drive a deterministic
		// "public Dispose while the native lazy read is in-flight" race and to count exact teardown.
		private sealed class GatedCountingStream : Stream
		{
			private readonly MemoryStream inner;
			private ManualResetEventSlim readEntered;
			private ManualResetEventSlim releaseRead;
			private int gateArmed;
			private int disposeCount;
			private int firstDisposeThreadId;

			public GatedCountingStream(byte[] bytes) => inner = new MemoryStream(bytes, writable: false);

			public int DisposeCount => Volatile.Read(ref disposeCount);

			// Managed thread id of whoever closed the .NET stream first. Lets a test prove the codec
			// owner-teardown — not the ignored public Dispose — was the actual disposer.
			public int FirstDisposeThreadId => Volatile.Read(ref firstDisposeThreadId);

			public void Arm(ManualResetEventSlim entered, ManualResetEventSlim release)
			{
				readEntered = entered;
				releaseRead = release;
				Volatile.Write(ref gateArmed, 1);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (Interlocked.CompareExchange(ref gateArmed, 0, 1) == 1)
				{
					readEntered.Set();
					releaseRead.Wait();
				}
				return inner.Read(buffer, offset, count);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (Interlocked.Increment(ref disposeCount) == 1)
						Volatile.Write(ref firstDisposeThreadId, Environment.CurrentManagedThreadId);
					// Actually close the backing buffer: a premature close during an in-flight read
					// then surfaces as an ObjectDisposedException from the parked inner.Read.
					inner.Dispose();
				}
				base.Dispose(disposing);
			}

			public override bool CanRead => true;
			public override bool CanSeek => true;
			public override bool CanWrite => false;
			public override long Length => inner.Length;
			public override long Position { get => inner.Position; set => inner.Position = value; }
			public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
			public override void Flush() => inner.Flush();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}

		// A NON-SEEKABLE .NET Stream over a byte buffer that (a) counts how many times it is disposed,
		// (b) records the disposing thread id, and (c) can park the first read after Arm() until
		// released. Non-seekable so SKCodec.Create routes it through the SKFrontBufferedManagedStream
		// (nested SKManagedStream) path — a different teardown graph from the flat seekable wrapper.
		private sealed class NonSeekableGatedCountingStream : Stream
		{
			private readonly MemoryStream inner;
			private ManualResetEventSlim readEntered;
			private ManualResetEventSlim releaseRead;
			private int gateArmed;
			private int disposeCount;
			private int firstDisposeThreadId;

			public NonSeekableGatedCountingStream(byte[] bytes) => inner = new MemoryStream(bytes, writable: false);

			public int DisposeCount => Volatile.Read(ref disposeCount);

			public int FirstDisposeThreadId => Volatile.Read(ref firstDisposeThreadId);

			public void Arm(ManualResetEventSlim entered, ManualResetEventSlim release)
			{
				readEntered = entered;
				releaseRead = release;
				Volatile.Write(ref gateArmed, 1);
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (Interlocked.CompareExchange(ref gateArmed, 0, 1) == 1)
				{
					readEntered.Set();
					releaseRead.Wait();
				}
				return inner.Read(buffer, offset, count);
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (Interlocked.Increment(ref disposeCount) == 1)
						Volatile.Write(ref firstDisposeThreadId, Environment.CurrentManagedThreadId);
					inner.Dispose();
				}
				base.Dispose(disposing);
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void Flush() => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}
	}
}
