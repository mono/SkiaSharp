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
		public void InitialOffsetAndOverflowingReadArguments_HandledCorrectly()
		{
			using var ms = new MemoryStream(TwentyBytes);
			ms.Position = 5;
			using var buffered = new SKFrontBufferedStream(ms, 10);

			Assert.Equal(15, buffered.Length);
			Assert.Throws<ArgumentOutOfRangeException>(() => buffered.Read(new byte[1], int.MaxValue, 1));
			Assert.Equal(0, buffered.Position);
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
	}
}
