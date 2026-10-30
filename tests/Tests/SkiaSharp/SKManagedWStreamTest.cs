using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKManagedWStreamTest : SKTest
	{
		[Fact]
		public void DotNetStreamIsCollected()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, true);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Throws<ObjectDisposedException>(() => dotnet.Position);
		}

		[Fact]
		public void DotNetStreamIsNotCollected()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet, false);

			Assert.Equal(0, dotnet.Position);

			stream.Dispose();

			Assert.Equal(0, dotnet.Position);
		}

		[Fact]
		public unsafe void StreamIsNotDisposedWhenReferencedIsDisposed()
		{
			var stream = new SKManagedWStream(new MemoryStream(), true);
			var handle = stream.Handle;

			var document = SKDocument.CreatePdf(stream);
			document.Dispose();

			Assert.True(SKObject.GetInstance<SKManagedWStream>(handle, out _));
		}

		[Fact]
		public void StreamIsCollectedEvenWhenNotProperlyDisposed()
		{
			SkipOnMono();

			DoWork(out var handle, out var weakStream);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(handle, weakStream);

			[MethodImpl(MethodImplOptions.NoInlining)]
			static void DoWork(out IntPtr streamHandle, out WeakReference streamRef)
			{
				var dotnet = new MemoryStream();
				var stream = new SKManagedWStream(dotnet, true);
				SKHandleDictionaryTestHelpers.Track(stream, out streamHandle, out streamRef);
			}
		}

		[Fact]
		public void ManagedStreamWritesByteCorrectly()
		{
			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet);

			Assert.Equal(0, dotnet.Position);
			Assert.Equal(0, stream.BytesWritten);

			stream.Write8(123);

			Assert.Equal(1, dotnet.Position);
			Assert.Equal(1, stream.BytesWritten);
			Assert.Equal(new byte[] { 123 }, dotnet.ToArray());

			stream.Write8(246);

			Assert.Equal(2, dotnet.Position);
			Assert.Equal(2, stream.BytesWritten);
			Assert.Equal(new byte[] { 123, 246 }, dotnet.ToArray());
		}

		[Fact]
		public void ManagedStreamWritesChunkCorrectly()
		{
			var data = new byte[1024];
			for (var i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var dotnet = new MemoryStream();
			var stream = new SKManagedWStream(dotnet);

			Assert.Equal(0, dotnet.Position);
			Assert.Equal(0, stream.BytesWritten);

			stream.Write(data, data.Length);

			Assert.Equal(data.Length, dotnet.Position);
			Assert.Equal(data.Length, stream.BytesWritten);
			Assert.Equal(data, dotnet.ToArray());
		}

		[Fact]
		public unsafe void StreamIsReferencedAndNotDisposedPrematurely()
		{
			SkipOnMono();

			DoWork(out var docH, out var weakDoc, out var streamH, out var weakStream);

			CollectGarbage();

			SKHandleDictionaryTestHelpers.AssertCollected(streamH, weakStream);
			SKHandleDictionaryTestHelpers.AssertCollected(docH, weakDoc);

			[MethodImpl(MethodImplOptions.NoInlining)]
			static void DoWork(out IntPtr documentHandle, out WeakReference documentRef, out IntPtr streamHandle, out WeakReference streamRef)
			{
				var document = CreateDocument(out streamHandle, out streamRef);
				SKHandleDictionaryTestHelpers.Track(document, out documentHandle, out documentRef);

				CollectGarbage();

				Assert.NotNull(document.BeginPage(100, 100));
				document.EndPage();
				document.Close();

				Assert.True(SKObject.GetInstance<SKManagedWStream>(streamHandle, out var stream));
				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);

				GC.KeepAlive(document);
			}

			[MethodImpl(MethodImplOptions.NoInlining)]
			static SKDocument CreateDocument(out IntPtr streamHandle, out WeakReference streamRef)
			{
				var stream = new SKManagedWStream(new MemoryStream(), true);
				SKHandleDictionaryTestHelpers.Track(stream, out streamHandle, out streamRef);

				Assert.True(stream.OwnsHandle);
				Assert.False(stream.IgnorePublicDispose);
				Assert.True(SKObject.GetInstance<SKManagedWStream>(streamHandle, out _));

				return SKDocument.CreatePdf(stream);
			}
		}

		[Theory]
		[InlineData(false, -1)]
		[InlineData(false, int.MinValue)]
		[InlineData(false, 2)]
		[InlineData(false, int.MaxValue)]
		[InlineData(true, -1)]
		public void InvalidArrayWriteSizeIsRejectedBeforeNativeCallback(bool nullBuffer, int size)
		{
			using var stream = new RejectingManagedWStream();
			var buffer = nullBuffer ? null : new byte[1];

			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Write(buffer, size));
			Assert.Equal(0, stream.WriteCalls);
		}

		private sealed class RejectingManagedWStream : SKAbstractManagedWStream
		{
			public int WriteCalls { get; private set; }

			protected internal override bool OnWrite(IntPtr buffer, IntPtr size)
			{
				// Reject without touching the pointer so invalid-size regression checks stay safe.
				WriteCalls++;
				return false;
			}

			protected internal override void OnFlush() { }

			protected internal override IntPtr OnBytesWritten() => IntPtr.Zero;
		}

		[Theory]
		[InlineData(-1)]
		[InlineData(int.MinValue)]
		[InlineData(2)]
		[InlineData(int.MaxValue)]
		public void InvalidArrayWriteSizeDoesNotWriteOrFaultManagedDestination(int size)
		{
			using var destination = new MemoryStream();
			using SKWStream stream = new SKManagedWStream(destination);
			var buffer = new byte[] { 42 };

			Assert.Throws<ArgumentOutOfRangeException>("size", () => stream.Write(buffer, size));
			Assert.Empty(destination.ToArray());
			Assert.Equal(0, stream.BytesWritten);

			Assert.True(stream.Write(buffer, buffer.Length));
			Assert.Equal(buffer, destination.ToArray());
			Assert.Equal(1, stream.BytesWritten);
		}

		[Fact]
		public void NullArrayWriteStillZeroFillsManagedDestination()
		{
			using var destination = new MemoryStream();
			using SKWStream stream = new SKManagedWStream(destination);

			Assert.True(stream.Write(null, 3));
			Assert.Equal(new byte[3], destination.ToArray());
			Assert.Equal(3, stream.BytesWritten);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(81919)]
		[InlineData(81920)]
		[InlineData(81921)]
		[InlineData(200000)]
		public void ManagedWStreamWritesAcrossChunkBoundariesCorrectly(int size)
		{
			var data = new byte[size];
			for (var i = 0; i < data.Length; i++)
				data[i] = (byte)((i * 31 + 7) & 0xFF);

			using var dotnet = new MemoryStream();
			using var stream = new SKManagedWStream(dotnet);

			Assert.True(stream.Write(data, data.Length));
			Assert.Equal(size, dotnet.Position);
			Assert.Equal(size, stream.BytesWritten);
			Assert.Equal(data, dotnet.ToArray());
		}

		[Fact]
		public void NonSeekableAndOffsetStreams_TrackBytesWrittenCorrectly()
		{
			using var nonSeekable = new NonSeekableWriteOnlyStream();
			using (var wstream = new SKManagedWStream(nonSeekable))
			{
				Assert.Equal(0, wstream.BytesWritten);
				Assert.True(wstream.Write(new byte[] { 1, 2, 3, 4, 5 }, 5));
				Assert.Equal(5, wstream.BytesWritten);
				Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, nonSeekable.ToArray());
			}

			using var offsetStream = new MemoryStream();
			offsetStream.Write(new byte[40], 0, 40);
			using (var wstream = new SKManagedWStream(offsetStream))
			{
				Assert.Equal(0, wstream.BytesWritten);
				Assert.True(wstream.Write(new byte[] { 10, 20, 30 }, 3));
				Assert.Equal(3, wstream.BytesWritten);
			}
		}

		[Fact]
		public void FailingWriteStream_ReturnsFalseWithoutAbortingProcess()
		{
			using var failing = new ThrowingWriteStream();
			using var wstream = new SKManagedWStream(failing);

			Assert.False(wstream.Write(new byte[] { 1, 2, 3 }, 3));
			Assert.False(wstream.Write8(42));
			Assert.Equal(0, wstream.BytesWritten);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void LaterChunkWriteFailurePreservesCompletedBytesAndLatchesFault(bool nullBuffer)
		{
			var bytes = new byte[SKData.CopyBufferSize * 2 + 17];
			if (!nullBuffer)
			{
				for (var i = 0; i < bytes.Length; i++)
					bytes[i] = (byte)(i * 31 + 7);
			}

			using var destination = new RecordingWriteStream(failOnSecondWrite: true);
			using var stream = new SKManagedWStream(destination);

			Assert.False(stream.Write(nullBuffer ? null : bytes, bytes.Length));
			Assert.Equal(SKData.CopyBufferSize, stream.BytesWritten);
			Assert.Equal(bytes.AsSpan(0, SKData.CopyBufferSize).ToArray(), destination.ToArray());
			Assert.Equal(new[] { SKData.CopyBufferSize, SKData.CopyBufferSize }, destination.WriteSizes);

			Assert.False(stream.Write8(42));
			Assert.False(stream.Write(null, 3));
			stream.Flush();

			Assert.Equal(SKData.CopyBufferSize, stream.BytesWritten);
			Assert.Equal(SKData.CopyBufferSize, destination.ToArray().Length);
			Assert.Equal(2, destination.WriteSizes.Count);
			Assert.Equal(0, destination.FlushCalls);
		}

		[Fact]
		public void NullBufferZeroFillsAcrossChunksAndPartialTail()
		{
			var size = SKData.CopyBufferSize * 2 + 17;
			using var destination = new RecordingWriteStream();
			using var stream = new SKManagedWStream(destination);

			// Exercise reuse of pooled storage containing nonzero bytes.
			using (var dirty = Utils.RentArray<byte>(SKData.CopyBufferSize))
				dirty.Span.Fill(0xA5);

			Assert.True(stream.Write(null, size));
			Assert.Equal(new byte[size], destination.ToArray());
			Assert.Equal(size, stream.BytesWritten);
			Assert.Equal(new[] { SKData.CopyBufferSize, SKData.CopyBufferSize, 17 }, destination.WriteSizes);
		}

		private sealed class RecordingWriteStream : NonSeekableWriteOnlyStream
		{
			private readonly bool failOnSecondWrite;

			public RecordingWriteStream(bool failOnSecondWrite = false) =>
				this.failOnSecondWrite = failOnSecondWrite;

			public List<int> WriteSizes { get; } = new List<int>();
			public int FlushCalls { get; private set; }

			public override void Write(byte[] buffer, int offset, int count)
			{
				WriteSizes.Add(count);
				if (failOnSecondWrite && WriteSizes.Count == 2)
					throw new IOException("The second chunk could not be written.");
				base.Write(buffer, offset, count);
			}

			public override void Flush()
			{
				FlushCalls++;
				base.Flush();
			}
		}

		[Fact]
		public void NullStreamConstructor_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>("managedStream", () => new SKManagedWStream(null));
			Assert.Throws<ArgumentNullException>("managedStream", () => new SKManagedWStream(null, false));
			Assert.Throws<ArgumentNullException>("managedStream", () => new SKManagedWStream(null, true));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void NullStreamConstructorDoesNotDisposePartiallyConstructedSubclass(bool disposeManagedStream)
		{
			Assert.Throws<ArgumentNullException>("managedStream",
				() => new ConstructionGuardedWStream(null, disposeManagedStream));
		}

		private sealed class ConstructionGuardedWStream : SKManagedWStream
		{
			private readonly bool initialized;

			public ConstructionGuardedWStream(Stream stream, bool disposeManagedStream)
				: base(stream, disposeManagedStream)
			{
				initialized = true;
			}

			protected override void Dispose(bool disposing)
			{
				base.Dispose(disposing);
				if (disposing && !initialized)
					throw new InvalidOperationException("Disposed before construction completed.");
			}
		}

		[Fact]
		public void FlushFailure_LatchesFaultAndRejectsSubsequentWrites()
		{
			using var failingFlush = new ThrowingWriteStream(throwOnWrite: false, throwOnFlush: true);
			using var wstream = new SKManagedWStream(failingFlush);

			Assert.True(wstream.Write(new byte[] { 1, 2, 3 }, 3));
			Assert.Equal(3, wstream.BytesWritten);

			wstream.Flush();

			Assert.False(wstream.Write(new byte[] { 4, 5 }, 2));
			Assert.Equal(3, wstream.BytesWritten);
		}

		[Fact]
		public void OnWrite_WithZeroPointer_WritesZeroBytesAndUpdatesBytesWritten()
		{
			using var ms = new MemoryStream();
			ms.WriteByte(0xFF);
			ms.Position = 0;

			using var wstream = new SKManagedWStream(ms);
			Assert.True(wstream.OnWrite(IntPtr.Zero, (IntPtr)16));
			Assert.Equal(16, wstream.BytesWritten);
			Assert.Equal(new byte[16], ms.ToArray());
		}
	}
}
