using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKFrontBufferedStreamTest : SKTest
	{
		private Stream CreateNonSeekableReadOnlyStream(Stream stream, int bufferSize)
		{
			var nonSeekable = new NonSeekableReadOnlyStream(stream);
			var buffered = new SKFrontBufferedStream(nonSeekable, bufferSize);
			return buffered;
		}

		private readonly byte[] TwentyBytes = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };

		[Fact]
		public void ImageCanBeDecoded()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var buffered = CreateNonSeekableReadOnlyStream(File.OpenRead(path), SKCodec.MinBufferedBytesNeeded);

			var bitmap = SKBitmap.Decode(buffered);

			Assert.NotNull(bitmap);
			Assert.NotNull(bitmap.PeekPixels());
		}

		[Fact]
		public void OnlyReadWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 8, 9, 10, 11, 12, 13, 14 }, buffer);

			buffer = new byte[7];
			Assert.Equal(6, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 15, 16, 17, 18, 19, 20, 0 }, buffer);
		}

		[Fact]
		public void RewindInBufferWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// rewind
			buffered.Position = 0;

			// re-read (from the internal buffer)
			buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// continue reading
			buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 8, 9, 10, 11, 12, 13, 14 }, buffer);
		}

		[Fact]
		public void SeekInBufferWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// rewind
			buffered.Position = 2;

			// re-read (from the internal buffer and the stream)
			buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 3, 4, 5, 6, 7, 8, 9 }, buffer);
		}

		[Fact]
		public void ReadPastBufferAfterSeekInBufferWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// rewind
			buffered.Position = 5;

			// re-read (from the internal buffer and the stream)
			buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 6, 7, 8, 9, 10, 11, 12 }, buffer);
		}

		[Fact]
		public void RewindOutsideBufferThrows()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// read past the buffer
			buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 8, 9, 10, 11, 12, 13, 14 }, buffer);

			// we can't seek after we have passed the buffer
			Assert.Throws<InvalidOperationException>(() => buffered.Position = 0);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(3)]
		[InlineData(5)]
		public void ShortReadUnderlyingStreamBuffersAndRewindsCorrectly(int maxBytesPerRead)
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false, maxBytesPerRead: maxBytesPerRead);
			using var buffered = new SKFrontBufferedStream(inner, 10);

			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, 7));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			buffered.Position = 0;

			buffer = new byte[10];
			Assert.Equal(10, buffered.Read(buffer, 0, 10));
			Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, buffer);

			buffered.Position = 0;

			buffer = new byte[20];
			Assert.Equal(20, buffered.Read(buffer, 0, 20));
			Assert.Equal(TwentyBytes, buffer);
		}

		[Fact]
		public void SeekForwardPastBufferOnNonSeekableStreamAdvancesCorrectly()
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false, maxBytesPerRead: 3);
			using var buffered = new SKFrontBufferedStream(inner, 10);

			var pos = buffered.Seek(14, SeekOrigin.Begin);
			Assert.Equal(14, pos);
			Assert.Equal(14, buffered.Position);

			var remaining = new byte[6];
			Assert.Equal(6, buffered.Read(remaining, 0, 6));
			Assert.Equal(new byte[] { 15, 16, 17, 18, 19, 20 }, remaining);
		}

		[Fact]
		public void SeekBeforeBeginningThrows()
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false);
			using var buffered = new SKFrontBufferedStream(inner, 10);

			Assert.Throws<ArgumentOutOfRangeException>(() => buffered.Seek(-1, SeekOrigin.Begin));
		}

		[Fact]
		public void SKDataCreate_FromNonSeekableFrontBufferedStream_ReadsAllBytes()
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false, maxBytesPerRead: 3);
			using var buffered = new SKFrontBufferedStream(inner, 10);

			using var data = SKData.Create(buffered);
			Assert.NotNull(data);
			Assert.Equal(TwentyBytes, data.ToArray());
		}

		[Fact]
		public void NestedFrontBufferedStreams_PreserveUnknownLengthAndCreateDataCorrectly()
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false, maxBytesPerRead: 4);
			using var outer1 = new SKFrontBufferedStream(inner, 8);
			using var outer2 = new SKFrontBufferedStream(outer1, 8);

			Assert.Equal(-1, outer1.Length);
			Assert.Equal(-1, outer2.Length);

			using var data = SKData.Create(outer2);
			Assert.NotNull(data);
			Assert.Equal(TwentyBytes, data.ToArray());
		}

		[Fact]
		public void SKDataCreate_FromEarlyEndingFrontBufferedStreamPastBuffer_SucceedsWithoutRewindException()
		{
			using var inner = new ConfigurableTestStream(
				TwentyBytes, canSeek: true, maxBytesPerRead: 3, earlyEndPosition: 12, reportedLength: 20);
			using var buffered = new SKFrontBufferedStream(inner, 4);

			using var data = SKData.Create(buffered);
			Assert.NotNull(data);
			Assert.Equal(TwentyBytes.AsSpan(0, 12).ToArray(), data.ToArray());
		}

		[Fact]
		public void InitialOffsetAndOverflowingReadArguments_HandledCorrectly()
		{
			Assert.Throws<ArgumentNullException>(() => new SKFrontBufferedStream(null, 10));
			using var ms = new MemoryStream(TwentyBytes);
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKFrontBufferedStream(ms, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => new SKFrontBufferedStream(ms, (long)int.MaxValue + 1));

			ms.Position = 5;
			using var buffered = new SKFrontBufferedStream(ms, 10);

			Assert.Equal(15, buffered.Length);
			Assert.Throws<ArgumentNullException>(() => buffered.Read(null, 0, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => buffered.Read(new byte[1], -1, 1));
			Assert.Throws<ArgumentOutOfRangeException>(() => buffered.Read(new byte[1], 0, -1));
			Assert.Throws<ArgumentOutOfRangeException>(() => buffered.Read(new byte[1], int.MaxValue, 1));
			Assert.Equal(0, buffered.Position);

			var remaining = new byte[15];
			Assert.Equal(15, buffered.Read(remaining, 0, 15));
			Assert.Equal(new byte[] { 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, remaining);
		}

		[Fact]
		public void NestedFrontBufferedStreams_PreserveUnknownLengthAndReadCorrectly()
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false, maxBytesPerRead: 4);
			using var outer1 = new SKFrontBufferedStream(inner, 8);
			using var outer2 = new SKFrontBufferedStream(outer1, 8);

			Assert.Equal(-1, outer1.Length);
			Assert.Equal(-1, outer2.Length);

			var buffer = new byte[20];
			Assert.Equal(20, outer2.Read(buffer, 0, 20));
			Assert.Equal(TwentyBytes, buffer);
		}

		[Fact]
		public void SeekForwardPastBufferOnSeekableAndNestedKnownLengthStreams_AdvancesCorrectly()
		{
			using var ms1 = new MemoryStream(TwentyBytes);
			using var buffered1 = new SKFrontBufferedStream(ms1, 6);
			Assert.Equal(14, buffered1.Seek(14, SeekOrigin.Begin));
			var tail1 = new byte[6];
			Assert.Equal(6, buffered1.Read(tail1, 0, 6));
			Assert.Equal(new byte[] { 15, 16, 17, 18, 19, 20 }, tail1);

			using var ms2 = new MemoryStream(TwentyBytes);
			using var inner = new SKFrontBufferedStream(ms2, 5);
			using var outer = new SKFrontBufferedStream(inner, 10);
			Assert.Equal(14, outer.Seek(14, SeekOrigin.Begin));
			var tail2 = new byte[6];
			Assert.Equal(6, outer.Read(tail2, 0, 6));
			Assert.Equal(new byte[] { 15, 16, 17, 18, 19, 20 }, tail2);
		}

		private sealed class TransientFailureStream : Stream
		{
			private readonly byte[] data;
			private readonly int failAtRead;
			private int pos;
			private int callCount;

			public TransientFailureStream(byte[] data, int failAtRead = 2)
			{
				this.data = data;
				this.failAtRead = failAtRead;
			}

			public override bool CanRead => true;
			public override bool CanSeek => false;
			public override bool CanWrite => false;
			public override long Length => throw new NotSupportedException();
			public override long Position
			{
				get => throw new NotSupportedException();
				set => throw new NotSupportedException();
			}
			public override void Flush() { }

			public override int Read(byte[] buffer, int offset, int count)
			{
				callCount++;
				if (callCount == failAtRead)
					throw new IOException("Simulated transient read failure.");

				var toCopy = Math.Min(Math.Min(count, 2), data.Length - pos);
				if (toCopy <= 0)
					return 0;

				Buffer.BlockCopy(data, pos, buffer, offset, toCopy);
				pos += toCopy;
				return toCopy;
			}

			public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
			public override void SetLength(long value) => throw new NotSupportedException();
			public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
		}

		[Fact]
		public void ShortReadFollowedByException_PreservesAlreadyBufferedBytes()
		{
			using var inner = new TransientFailureStream(TwentyBytes);
			using var buffered = new SKFrontBufferedStream(inner, 10);

			var buf = new byte[4];
			Assert.Throws<IOException>(() => buffered.Read(buf, 0, 4));
			Assert.Equal(2, buffered.Position);

			buffered.Position = 0;
			Assert.Equal(4, buffered.Read(buf, 0, 4));
			Assert.Equal(new byte[] { 1, 2, 3, 4 }, buf);
		}

		[Theory]
		[InlineData(0, 4)]
		[InlineData(1, 3)]
		public void DirectReadExceptionPreservesProgressAndPreventsRewind(int bufferSize, int consumed)
		{
			using var inner = new TransientFailureStream(TwentyBytes, failAtRead: 3);
			using var buffered = new SKFrontBufferedStream(inner, bufferSize);
			var buffer = new byte[8];

			Assert.Throws<IOException>(() => buffered.Read(buffer, 0, buffer.Length));
			Assert.Equal(consumed, buffered.Position);
			Assert.Throws<InvalidOperationException>(() => buffered.Seek(0, SeekOrigin.Begin));

			Assert.Equal(2, buffered.Read(buffer, 0, 2));
			Assert.Equal(TwentyBytes[consumed], buffer[0]);
			Assert.Equal(TwentyBytes[consumed + 1], buffer[1]);
		}

		private sealed class ForwardingStream : Stream
		{
			private readonly Stream inner;

			public ForwardingStream(Stream inner) => this.inner = inner;

			public override bool CanRead => inner.CanRead;
			public override bool CanSeek => inner.CanSeek;
			public override bool CanWrite => inner.CanWrite;
			public override long Length => inner.Length;
			public override long Position
			{
				get => inner.Position;
				set => inner.Position = value;
			}

			public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
			public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
			public override void Flush() => inner.Flush();
			public override void SetLength(long value) => inner.SetLength(value);
			public override void Write(byte[] buffer, int offset, int count) => inner.Write(buffer, offset, count);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(5)]
		public void SeekPastDecoratedFrontBufferReadsCorrectRemainingBytes(int innerBufferSize)
		{
			using var source = new MemoryStream(TwentyBytes);
			using var inner = new SKFrontBufferedStream(source, innerBufferSize);
			using var decorator = new ForwardingStream(inner);
			using var outer = new SKFrontBufferedStream(decorator, 10);

			Assert.Equal(14, outer.Seek(14, SeekOrigin.Begin));
			var tail = new byte[6];
			Assert.Equal(6, outer.Read(tail, 0, tail.Length));
			Assert.Equal(new byte[] { 15, 16, 17, 18, 19, 20 }, tail);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ForwardSeekAfterBufferIsExhaustedDoesNotPermitBackwardSeek(bool canSeek)
		{
			using var source = new ConfigurableTestStream(TwentyBytes, canSeek, maxBytesPerRead: 3);
			using var buffered = new SKFrontBufferedStream(source, 5);
			var buffer = new byte[7];
			Assert.Equal(7, buffered.Read(buffer, 0, buffer.Length));

			Assert.Equal(10, buffered.Seek(3, SeekOrigin.Current));
			Assert.Equal(10, buffered.Seek(0, SeekOrigin.Current));
			Assert.Equal(14, buffered.Seek(14, SeekOrigin.Begin));
			Assert.Throws<InvalidOperationException>(() => buffered.Seek(13, SeekOrigin.Begin));

			var tail = new byte[6];
			Assert.Equal(6, buffered.Read(tail, 0, tail.Length));
			Assert.Equal(new byte[] { 15, 16, 17, 18, 19, 20 }, tail);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(10)]
		public void NativeAdapterDoesNotReportEofWhenReadingPastFrontBuffer(int bufferSize)
		{
			using var source = new MemoryStream(TwentyBytes);
			using var buffered = new SKFrontBufferedStream(source, bufferSize);
			using var native = new SKManagedStream(buffered);
			Assert.True(buffered.CanSeek);
			var buffer = new byte[bufferSize + 1];

			Assert.Equal(buffer.Length, native.Read(buffer, buffer.Length));
			Assert.False(native.IsAtEnd);
			Assert.True(buffered.CanSeek);
			Assert.Throws<InvalidOperationException>(() => buffered.Seek(0, SeekOrigin.Begin));

			var remaining = new byte[TwentyBytes.Length - buffer.Length];
			Assert.Equal(remaining.Length, native.Read(remaining, remaining.Length));
			Assert.True(native.IsAtEnd);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(10)]
		public void SeekPastEndStopsAtActualEnd(int bufferSize)
		{
			using var inner = new ConfigurableTestStream(TwentyBytes, canSeek: false, maxBytesPerRead: 3);
			using var buffered = new SKFrontBufferedStream(inner, bufferSize);

			Assert.Equal(TwentyBytes.Length, buffered.Seek(long.MaxValue, SeekOrigin.Begin));
			Assert.Equal(TwentyBytes.Length, buffered.Position);
			Assert.Equal(0, buffered.Read(new byte[1], 0, 1));
		}

		[Fact]
		public void SeekCurrentAndEndPreserveInitialStreamOffset()
		{
			using var source = new MemoryStream(TwentyBytes);
			source.Position = 5;
			using var buffered = new SKFrontBufferedStream(source, 10);

			Assert.Equal(3, buffered.Seek(3, SeekOrigin.Current));
			Assert.Equal(2, buffered.Seek(-1, SeekOrigin.Current));
			Assert.Equal(9, buffered.Seek(-6, SeekOrigin.End));
			Assert.Equal(15, buffered.ReadByte());
		}
	}
}
