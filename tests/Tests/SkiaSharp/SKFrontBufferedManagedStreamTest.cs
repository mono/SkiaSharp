using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFrontBufferedManagedStreamTest : SKTest
	{
		// All tests will buffer this string, and compare output to the original.
		// The string is long to ensure that all of our lengths being tested are
		// smaller than the string length.
		private const string gAbcsString = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwx";
		private readonly byte[] gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

		[Fact]
		public async Task DisposedStreamDoesNotRetainBorrowedNativeStream ()
		{
			var weakStream = CreateDisposedStreamReference (out var bufferedStream);

			await AssertEx.EventuallyGC (weakStream);
			GC.KeepAlive (bufferedStream);
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static WeakReference CreateDisposedStreamReference (out SKFrontBufferedManagedStream bufferedStream)
		{
			var stream = new SKMemoryStream (new byte[] { 1 });
			bufferedStream = new SKFrontBufferedManagedStream (stream, 1, disposeUnderlyingStream: false);
			bufferedStream.Dispose ();

			return new WeakReference (stream);
		}

		// Test that hasLength() returns the correct value, based on the stream
		// being wrapped. A length can only be known if the wrapped stream has a
		// length and it has a position (so its initial position can be taken into
		// account when computing the length).
		private static void test_hasLength (SKStream bufferedStream, SKStream streamBeingBuffered)
		{
			if (streamBeingBuffered.HasLength && streamBeingBuffered.HasPosition)
			{
				Assert.True (bufferedStream.HasLength);
			}
			else
			{
				Assert.False (bufferedStream.HasLength);
			}
		}

		private static void test_rewind (SKStream bufferedStream, bool shouldSucceed)
		{
			var success = bufferedStream.Rewind ();
			Assert.Equal (shouldSucceed, success);
		}

		static void test_read (SKStream bufferedStream, byte[] expectations, int bytesToRead)
		{
			// output for reading bufferedStream.
			var storage = new byte[bytesToRead];

			var bytesRead = bufferedStream.Read (storage, bytesToRead);
			Assert.True (bytesRead == bytesToRead || bufferedStream.IsAtEnd);
			Assert.Equal (expectations.Take (bytesRead), storage.Take (bytesRead));
		}

		[Theory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void SkippingDoesNotPreventReading (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			test_hasLength (bufferedStream, memStream);

			// Skip half the buffer.
			bufferedStream.Skip (bufferSize / 2);

			// Rewind, then read part of the buffer, which should have been read.
			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize / 4);

			// Now skip beyond the buffered piece, but still within the total buffer.
			bufferedStream.Skip (bufferSize / 2);

			// Test that reading will still work.
			test_read (bufferedStream, gAbcs.Skip (memStream.Position).ToArray (), bufferSize / 4);

			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize);
		}

		[Theory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void IncrementalBuffering (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			test_hasLength (bufferedStream, memStream);

			// First, test reading less than the max buffer size.
			test_read (bufferedStream, gAbcs, bufferSize / 2);

			// Now test rewinding back to the beginning and reading less than what was
			// already buffered.
			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize / 4);

			// Now test reading part of what was buffered, and buffering new data.
			test_read (bufferedStream, gAbcs.Skip (bufferSize / 4).ToArray (), bufferSize / 2);

			// Now test reading what was buffered, buffering new data, and
			// reading directly from the stream.
			test_rewind (bufferedStream, true);
			test_read (bufferedStream, gAbcs, bufferSize << 1);

			// We have reached the end of the buffer, so rewinding will fail.
			// This test assumes that the stream is larger than the buffer; otherwise the
			// result of rewind should be true.
			test_rewind (bufferedStream, false);
		}

		[Theory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void PerfectlySizedBuffer (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			test_hasLength (bufferedStream, memStream);

			// Read exactly the amount that fits in the buffer.
			test_read (bufferedStream, gAbcs, bufferSize);

			// Rewinding should succeed.
			test_rewind (bufferedStream, true);

			// Once again reading buffered info should succeed
			test_read (bufferedStream, gAbcs, bufferSize);

			// Read past the size of the buffer. At this point, we cannot return.
			test_read (bufferedStream, gAbcs.Skip (memStream.Position).ToArray (), 1);
			test_rewind (bufferedStream, false);
		}

		[Theory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void InitialOffset (int bufferSize)
		{
			var memStream = new SKMemoryStream (gAbcs);

			// Skip a few characters into the memStream, so that bufferedStream represents an offset into
			// the stream it wraps.
			var arbitraryOffset = 17;
			memStream.Skip (arbitraryOffset);
			var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

			// Since SkMemoryStream has a length, bufferedStream must also.
			Assert.True (bufferedStream.HasLength);

			var amountToRead = 10;
			var bufferedLength = bufferedStream.Length;
			var currentPosition = 0;

			// Read the stream in chunks. After each read, the position must match currentPosition,
			// which sums the amount attempted to read, unless the end of the stream has been reached.
			// Importantly, the end should not have been reached until currentPosition == bufferedLength.
			while (currentPosition < bufferedLength)
			{
				Assert.False (bufferedStream.IsAtEnd);
				test_read (bufferedStream, gAbcs.Skip (arbitraryOffset + currentPosition).ToArray (), amountToRead);
				currentPosition = Math.Min (currentPosition + amountToRead, bufferedLength);
				Assert.Equal (currentPosition, memStream.Position - arbitraryOffset);
			}
			Assert.True (bufferedStream.IsAtEnd);
			Assert.Equal (bufferedLength, currentPosition);
		}

		// Dummy stream that optionally has a length and/or position. Tests that FrontBufferedStream's
		// length depends on the stream it's buffering having a length and position.
		private class LengthOptionalStream : ManagedStream
		{
			private readonly bool fHasLength;
			private readonly bool fHasPosition;

			public LengthOptionalStream (bool hasLength, bool hasPosition)
			{
				fHasLength = hasLength;
				fHasPosition = hasPosition;
			}

			protected internal override bool OnHasLength () => fHasLength;

			protected internal override bool OnHasPosition () => fHasPosition;
		}

		[Theory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void LengthCombinations (int bufferSize)
		{
			{
				var stream = new LengthOptionalStream (true, true);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
			{
				var stream = new LengthOptionalStream (true, false);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
			{
				var stream = new LengthOptionalStream (false, true);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
			{
				var stream = new LengthOptionalStream (false, false);
				var buffered = new SKFrontBufferedManagedStream (stream, bufferSize);
				test_hasLength (buffered, stream);
			}
		}

		//// Test that a FrontBufferedStream does not allow reading after the end of a stream.
		// This class is a dummy SkStream which reports that it is at the end on the first
		// read (simulating a failure). Then it tracks whether someone calls read() again.
		private class FailingStream : ManagedStream
		{
			private bool fAtEnd;

			public bool ReadCalledAfterEnd { get; private set; }

			protected internal override bool OnIsAtEnd () => fAtEnd;

			protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
			{
				if (fAtEnd)
					ReadCalledAfterEnd = true;
				fAtEnd = true;
				return (IntPtr)0;
			}
		}

		[Fact]
		public void FailingStreamDoesNotReadAfterEnd ()
		{
			using var failingStream = new FailingStream ();
			using var bufferedStream = new SKFrontBufferedManagedStream (failingStream, 64);
			using var codec = SKCodec.Create (bufferedStream);

			Assert.Null (codec);
			Assert.False (failingStream.ReadCalledAfterEnd);
		}

		private sealed class ShortReadingCustomSKStream : ManagedStream
		{
			private readonly byte[] data;
			private readonly int maxPerRead;
			private int pos;

			public ShortReadingCustomSKStream (byte[] data, int maxPerRead)
			{
				this.data = data;
				this.maxPerRead = maxPerRead;
			}

			protected internal override bool OnIsAtEnd () => pos >= data.Length;

			protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
			{
				var remaining = data.Length - pos;
				if (remaining <= 0 || (long)size <= 0)
					return IntPtr.Zero;

				var count = (int)Math.Min ((long)size, Math.Min (remaining, maxPerRead));
				if (buffer != IntPtr.Zero)
					Marshal.Copy (data, pos, buffer, count);
				pos += count;
				return (IntPtr)count;
			}
		}

		[Theory]
		[InlineData (1)]
		[InlineData (5)]
		[InlineData (17)]
		public void ShortReadNonSeekableStreamBuffersAndRewindsCorrectly (int maxBytesPerRead)
		{
			using var inner = new ConfigurableTestStream (gAbcs, canSeek: false, maxBytesPerRead: maxBytesPerRead);
			using var buffered = new SKFrontBufferedManagedStream (inner, 32, disposeUnderlyingStream: true);

			test_read (buffered, gAbcs, 16);
			Assert.False (buffered.IsAtEnd);
			test_rewind (buffered, true);

			test_read (buffered, gAbcs, 32);
			Assert.False (buffered.IsAtEnd);
			test_rewind (buffered, true);

			// Read the entire stream (across and past the front buffer) in a single Read call
			test_read (buffered, gAbcs, gAbcs.Length);
			test_rewind (buffered, false);

			// For a non-seekable stream, EOF is detected when a read past the last byte returns 0
			Assert.Equal (0, buffered.Read (new byte[1], 1));
			Assert.True (buffered.IsAtEnd);
		}

		[Fact]
		public void ShortReadNonSeekableStream_CanDecodeImagePastFrontBuffer ()
		{
			var bytes = File.ReadAllBytes (Path.Combine (PathToImages, "color-wheel.png"));
			using var inner = new ConfigurableTestStream (bytes, canSeek: false, maxBytesPerRead: 3);
			using var buffered = new SKFrontBufferedManagedStream (inner, SKCodec.MinBufferedBytesNeeded, disposeUnderlyingStream: true);
			using var codec = SKCodec.Create (buffered);

			Assert.NotNull (codec);
			Assert.Equal (SKCodecResult.Success, codec.GetPixels (out var pixels));
			Assert.NotEmpty (pixels);
		}

		[Theory]
		[InlineData (6)]
		[InlineData (15)]
		[InlineData (64)]
		public void ReadBeyondBufferDoesNotInvalidateFrontBuffer (int bufferSize)
		{
			var exactData = gAbcs.Take (bufferSize).ToArray ();
			using var inner = new ConfigurableTestStream (exactData, canSeek: false, maxBytesPerRead: 5);
			using var buffered = new SKFrontBufferedManagedStream (inner, bufferSize, disposeUnderlyingStream: true);

			// Attempt to read one byte more than bufferSize (which is also the stream's total length)
			test_read (buffered, exactData, bufferSize + 1);
			Assert.True (buffered.IsAtEnd);

			// Rewinding must still succeed because 0 bytes were read past the front buffer
			test_rewind (buffered, true);
			Assert.False (buffered.IsAtEnd);
			test_read (buffered, exactData, bufferSize);
		}

		[Fact]
		public void ShortReadingNativeSKStream_DoesNotDropFrontBufferPrematurely ()
		{
			using var inner = new ShortReadingCustomSKStream (gAbcs, maxPerRead: 2);
			using var buffered = new SKFrontBufferedManagedStream (inner, 16, disposeUnderlyingStream: true);

			test_read (buffered, gAbcs, 8);
			test_read (buffered, gAbcs.Skip (8).ToArray (), 8);
			test_rewind (buffered, true);
			test_read (buffered, gAbcs, 16);
			test_rewind (buffered, true);
			test_read (buffered, gAbcs, gAbcs.Length);
			Assert.True (buffered.IsAtEnd);
		}

		[Fact]
		public void FrontBufferedManagedStream_DuplicateAndForkReturnNull ()
		{
			using var memStream = new SKMemoryStream (gAbcs);
			using var buffered = new SKFrontBufferedManagedStream (memStream, 16);

			Assert.Null (buffered.Duplicate ());
			Assert.Null (buffered.Fork ());
		}

		[Fact]
		public void FrontBufferedNonSeekableStreamWrappedInManagedStream_ReadsAllBytes ()
		{
			var data = Enumerable.Range (0, 64).Select (i => (byte)i).ToArray ();
			using var inner = new ConfigurableTestStream (data, canSeek: false, maxBytesPerRead: 5);
			using var frontBuffered = new SKFrontBufferedStream (inner, 16);
			using var managed = new SKManagedStream (frontBuffered);

			Assert.False (managed.HasLength);
			Assert.False (managed.IsAtEnd);

			using var nestedFrontBuffered = new SKFrontBufferedManagedStream (managed, 16, disposeUnderlyingStream: false);
			var actual = new byte[data.Length];
			Assert.Equal (data.Length, nestedFrontBuffered.Read (actual, actual.Length));
			Assert.Equal (data, actual);
		}

		private sealed class VirtualLargeSkipSKStream : ManagedStream
		{
			private long remaining = (long)int.MaxValue + 1;

			protected internal override bool OnIsAtEnd () => remaining <= 0;

			protected internal override unsafe IntPtr OnRead (IntPtr buffer, IntPtr size)
			{
				var req = (long)size;
				if (req <= 0 || remaining <= 0)
					return IntPtr.Zero;

				var count = Math.Min (req, remaining);
				if (buffer != IntPtr.Zero)
					new Span<byte> ((void*)buffer, (int)Math.Min (count, 64)).Clear ();
				remaining -= count;
				return (IntPtr)count;
			}
		}

		[Fact]
		public void CumulativeSkipBeyond2GB_DoesNotWrapOffsetOrAllowInvalidRewind ()
		{
			using var inner = new VirtualLargeSkipSKStream ();
			using var buffered = new SKFrontBufferedManagedStream (inner, 16, disposeUnderlyingStream: true);

			Assert.Equal (int.MaxValue, buffered.Skip (int.MaxValue));
			Assert.False (buffered.IsAtEnd);
			Assert.Equal (1, buffered.Skip (1));

			// Offset is now 2,147,483,648 (> int.MaxValue). If offset wrapped negative (< bufferedSoFar),
			// IsAtEnd would falsely return false and Read would crash trying to read from frontBuffer.
			Assert.True (buffered.IsAtEnd);
			Assert.Equal (0, buffered.Read (new byte[1], 1));
			Assert.False (buffered.Rewind ());
		}

		private sealed class LargeLengthSKStream : ManagedStream
		{
			private readonly long length;
			private readonly long position;

			public LargeLengthSKStream (long length, long position)
			{
				this.length = length;
				this.position = position;
			}

			protected internal override bool OnHasLength () => true;
			protected internal override bool OnHasPosition () => true;
			protected internal override IntPtr OnGetLength () =>
				IntPtr.Size == 4 ? (IntPtr)unchecked((int)(uint)length) : (IntPtr)length;
			protected internal override IntPtr OnGetPosition () =>
				IntPtr.Size == 4 ? (IntPtr)unchecked((int)(uint)position) : (IntPtr)position;
		}

		[Fact]
		public unsafe void LargeStreamLengthBeyond2GB_IsPreservedWithoutSignExtension ()
		{
			var twoGiB = (long)int.MaxValue + 1;
			using var inner = new LargeLengthSKStream (twoGiB + 100, 100);
			using var buffered = new SKFrontBufferedManagedStream (inner, 16, disposeUnderlyingStream: true);

			var rawLen = SkiaApi.sk_stream_get_length (buffered.Handle);
			var nativeLen = IntPtr.Size == 4 ? (uint)(int)rawLen : (long)rawLen;
			Assert.Equal (twoGiB, nativeLen);
		}

		[Fact]
		public void InvalidConstructorArguments_ThrowExpectedExceptions ()
		{
			Assert.Throws<ArgumentNullException> (() => new SKFrontBufferedManagedStream ((Stream)null, 16));
			Assert.Throws<ArgumentNullException> (() => new SKFrontBufferedManagedStream ((SKStream)null, 16));
			using var ms = new MemoryStream (gAbcs);
			Assert.Throws<ArgumentOutOfRangeException> (() => new SKFrontBufferedManagedStream (ms, -1));
			using var skms = new SKMemoryStream (gAbcs);
			Assert.Throws<ArgumentOutOfRangeException> (() => new SKFrontBufferedManagedStream (skms, -1));
		}

		private sealed class ConstructorValidationStream : SKFrontBufferedManagedStream
		{
			public ConstructorValidationStream (SKStream stream, int bufferSize)
				: base (stream, bufferSize)
			{
			}

			protected override void Dispose (bool disposing)
			{
				if (disposing)
					throw new InvalidOperationException ("Disposal must not run during constructor validation.");
				base.Dispose (disposing);
			}
		}

		[Fact]
		public void InvalidConstructorArgumentsDoNotInvokeVirtualDisposal ()
		{
			Assert.Throws<ArgumentNullException> (() => new ConstructorValidationStream (null, 16));
			using var inner = new SKMemoryStream (gAbcs);
			Assert.Throws<ArgumentOutOfRangeException> (() => new ConstructorValidationStream (inner, -1));
		}

		[Fact]
		public void FailedConstructionDoesNotDisposeUnderlyingStream ()
		{
			using var inner = new SKMemoryStream (gAbcs);
			Assert.Throws<ArgumentOutOfRangeException> (() => new SKFrontBufferedManagedStream (inner, -1, true));

			Assert.False (inner.IsDisposed);
			Assert.Equal (gAbcs[0], inner.ReadByte ());

			using var managed = new MemoryStream (gAbcs);
			Assert.Throws<ArgumentOutOfRangeException> (() => new SKFrontBufferedManagedStream (managed, -1, true));
			Assert.Equal (gAbcs[0], managed.ReadByte ());
		}

		[Fact]
		public void NativeSizeReadAndPeekRequestsPreserveUnsignedBits ()
		{
			var size = IntPtr.Size == 4 ? new IntPtr (int.MinValue) : new IntPtr ((long)int.MaxValue + 1);
			using var inner = new VirtualLargeSkipSKStream ();
			using var buffered = new SKFrontBufferedManagedStream (inner, 16);
			using var storage = SKData.Create (16);

			Assert.Equal (new IntPtr (16), buffered.OnPeek (storage.Data, size));
			Assert.True (buffered.Rewind ());
			Assert.Equal (size, buffered.OnRead (IntPtr.Zero, size));
			Assert.True (buffered.IsAtEnd);
			Assert.False (buffered.Rewind ());
		}

		[Theory]
		[InlineData (0x8000000000000080UL)]
		[InlineData (ulong.MaxValue)]
		public void NativeSkipPreservesFullUnsignedCount (ulong requested)
		{
			using var source = new ConfigurableTestStream (gAbcs, canSeek: false, maxBytesPerRead: 7);
			using var buffered = new SKFrontBufferedManagedStream (source, 16);
			var size = Utils.ToNativeSize (IntPtr.Size == 8 ? requested : uint.MaxValue);

			var skipped = SkiaApi.sk_stream_skip (buffered.Handle, size);

			Assert.Equal ((ulong)gAbcs.Length, Utils.FromNativeSize (skipped));
			Assert.InRange (source.MaxRequestedReadCount, 1, SKData.CopyBufferSize);
			Assert.True (buffered.IsAtEnd);
			Assert.False (buffered.Rewind ());
		}

		[Fact]
		public void NativeLengthPreservesFullUnsignedRemainingLength ()
		{
			var length = IntPtr.Size == 8 ? -1L : uint.MaxValue;
			var expected = IntPtr.Size == 8 ? ulong.MaxValue - 100 : (ulong)uint.MaxValue - 100;
			using var source = new LargeLengthSKStream (length, 100);
			using var buffered = new SKFrontBufferedManagedStream (source, 16);

			Assert.True (buffered.HasLength);
			Assert.Equal (expected, Utils.FromNativeSize (SkiaApi.sk_stream_get_length (buffered.Handle)));
		}

		[Theory]
		[InlineData (0)]
		[InlineData (2)]
		[InlineData (8)]
		public void ExplicitReadResumesAfterBackingMemoryStreamGrows (int bufferSize)
		{
			using var source = new MemoryStream ();
			source.Write (new byte[] { 1, 2, 3 }, 0, 3);
			source.Position = 0;
			using var buffered = new SKFrontBufferedManagedStream (source, bufferSize);
			var bytes = new byte[4];
			Assert.Equal (3, buffered.Read (bytes, bytes.Length));
			Assert.True (buffered.IsAtEnd);

			source.SetLength (5);
			source.GetBuffer ()[3] = 4;
			source.GetBuffer ()[4] = 5;
			Assert.Equal (3, source.Position);

			Assert.Equal (2, buffered.Read (bytes, 2));
			Assert.Equal (new byte[] { 4, 5 }, bytes.Take (2));
			Assert.Equal (5, source.Position);
			Assert.True (buffered.IsAtEnd);
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public void ShortReadThenExceptionPreservesBufferedPrefix (bool callerWrapped)
		{
			using var source = new PartialFailureStream (gAbcs.Take (8).ToArray (), failOnCall: 2);
			using var managed = new OverrideTrackingManagedStream (source);
			using var buffered = callerWrapped
				? new SKFrontBufferedManagedStream (managed, 10)
				: new SKFrontBufferedManagedStream (source, 10);
			var bytes = new byte[4];

			Assert.Equal (0, buffered.Read (bytes, bytes.Length));
			Assert.Equal (2, source.ConsumedBytes);
			source.FailReads = false;
			Assert.True (buffered.Rewind ());
			Assert.Equal (bytes.Length, buffered.Read (bytes, bytes.Length));
			Assert.Equal (gAbcs.Take (4), bytes);
			if (callerWrapped)
				Assert.Equal (2, managed.ReadCalls);
		}

		[Theory]
		[InlineData (false)]
		[InlineData (true)]
		public void ShortDirectReadThenExceptionInvalidatesRewind (bool callerWrapped)
		{
			using var source = new PartialFailureStream (gAbcs.Take (8).ToArray (), failOnCall: 3);
			using var managed = new OverrideTrackingManagedStream (source);
			using var buffered = callerWrapped
				? new SKFrontBufferedManagedStream (managed, 1)
				: new SKFrontBufferedManagedStream (source, 1);

			Assert.Equal (0, buffered.Read (new byte[8], 8));
			Assert.Equal (3, source.ConsumedBytes);
			Assert.False (buffered.Rewind ());
			if (callerWrapped)
				Assert.Equal (2, managed.ReadCalls);
		}

		private sealed class OverrideTrackingManagedStream : SKManagedStream
		{
			public OverrideTrackingManagedStream (Stream source) : base (source) { }

			public int ReadCalls { get; private set; }

			protected internal override IntPtr OnRead (IntPtr buffer, IntPtr size)
			{
				ReadCalls++;
				return base.OnRead (buffer, size);
			}
		}

		private sealed class PartialFailureStream : Stream
		{
			private readonly byte[] data;
			private readonly int failOnCall;
			private int readCalls;
			private int position;

			public PartialFailureStream (byte[] data, int failOnCall)
			{
				this.data = data;
				this.failOnCall = failOnCall;
			}

			public bool FailReads { get; set; } = true;
			public int ConsumedBytes => position;
			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException ();
			public override long Position {
				get => throw new NotSupportedException ();
				set => throw new NotSupportedException ();
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				if (++readCalls == failOnCall && FailReads)
					throw new IOException ("Read failed after partial progress.");
				var read = Math.Min (Math.Min (count, 2), data.Length - position);
				Array.Copy (data, position, buffer, offset, read);
				position += read;
				return read;
			}

			public override void Flush () => throw new NotSupportedException ();
			public override long Seek (long offset, SeekOrigin origin) => throw new NotSupportedException ();
			public override void SetLength (long value) => throw new NotSupportedException ();
			public override void Write (byte[] buffer, int offset, int count) => throw new NotSupportedException ();
		}

		[Fact]
		public void StreamPeek ()
		{
			var gAbcsString = "abcdefghijklmnopqrstuvwxyz";
			var gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

			var memStream = new SKMemoryStream (gAbcs);

			test_fully_peekable_stream (memStream, memStream.Length);
		}

		[Fact]
		public void StreamPeek2 ()
		{
			var gAbcsString = "abcdefghijklmnopqrstuvwxyz";
			var gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

			var memStream = new SKMemoryStream (gAbcs);

			for (var bufferSize = 1; bufferSize < memStream.Length; bufferSize++)
			{
				var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

				var peeked = 0;
				for (var i = 1; !bufferedStream.IsAtEnd; i++)
				{
					var unpeekableBytes = compare_peek_to_read (bufferedStream, i);
					if (unpeekableBytes > 0)
					{
						// This could not have returned a number greater than i.
						Assert.True (unpeekableBytes <= i);

						// We have reached the end of the buffer. Verify that it was at least
						// bufferSize.
						Assert.True (peeked + i - unpeekableBytes >= bufferSize);
						// No more peeking is supported.
						break;
					}
					peeked += i;
				}
			}
		}

		[Fact]
		public void StreamPeek3 ()
		{
			var gAbcsString = "abcdefghijklmnopqrstuvwxyz";
			var gAbcs = StringUtilities.GetEncodedText (gAbcsString, SKTextEncoding.Utf8);

			for (var bufferSize = 1; bufferSize < gAbcs.Length; bufferSize++)
			{
				var memStream = new SKMemoryStream (gAbcs);
				var bufferedStream = new SKFrontBufferedManagedStream (memStream, bufferSize);

				var bytesToPeek = bufferSize + 1;

				var peekStorage = SKData.Create (bytesToPeek);
				var readStorage = SKData.Create (bytesToPeek);

				for (var start = 0; start <= bufferSize; start++)
				{
					// Skip to the starting point
					Assert.Equal (start, bufferedStream.Skip (start));

					var bytesPeeked = bufferedStream.Peek (peekStorage.Data, bytesToPeek);
					if (0 == bytesPeeked)
					{
						// Peeking should only fail completely if we have read/skipped beyond the buffer.
						Assert.True (start >= bufferSize);
						break;
					}

					// Only read the amount that was successfully peeked.
					var bytesRead = bufferedStream.Read (readStorage.Data, bytesPeeked);
					Assert.Equal (bytesPeeked, bytesRead);
					Assert.Equal (peekStorage.ToArray ().Take (bytesPeeked), readStorage.ToArray ().Take (bytesPeeked));

					// This should be safe to rewind.
					Assert.True (bufferedStream.Rewind ());
				}
			}
		}

		private void test_fully_peekable_stream (SKStream stream, int limit)
		{
			for (var i = 1; !stream.IsAtEnd; i++)
			{
				Assert.Equal (0, compare_peek_to_read (stream, i));
			}
		}

		private int compare_peek_to_read (SKStream stream, int bytesToPeek)
		{
			Assert.True (bytesToPeek > 0);

			var peekStorage = SKData.Create (bytesToPeek);
			var readStorage = SKData.Create (bytesToPeek);

			var bytesPeeked = stream.Peek (peekStorage.Data, bytesToPeek);
			var bytesRead = stream.Read (readStorage.Data, bytesToPeek);

			// bytesRead should only be less than attempted if the stream is at the
			// end.
			Assert.True (bytesRead == bytesToPeek || stream.IsAtEnd);

			// peek and read should behave the same, except peek returned to the
			// original position, so they read the same data.
			Assert.Equal (peekStorage.ToArray ().Take (bytesPeeked), readStorage.ToArray ().Take (bytesPeeked));

			// A stream should never be able to peek more than it can read.
			Assert.True (bytesRead >= bytesPeeked);

			return bytesRead - bytesPeeked;
		}
	}
}
