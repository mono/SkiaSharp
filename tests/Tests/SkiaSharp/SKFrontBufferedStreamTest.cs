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
		public void DeprecationIsWarningOnlyAndNamesSkiaReplacements()
		{
			var obsolete = (ObsoleteAttribute)Attribute.GetCustomAttribute(typeof(SKFrontBufferedStream), typeof(ObsoleteAttribute));

			Assert.NotNull(obsolete);
			Assert.False(obsolete.IsError);
			Assert.Contains(nameof(SKFrontBufferedManagedStream), obsolete.Message);
			Assert.Contains(nameof(SKCodec.Create), obsolete.Message);
			Assert.True(typeof(Stream).IsAssignableFrom(typeof(SKFrontBufferedStream)));
			Assert.False(typeof(Stream).IsAssignableFrom(typeof(SKFrontBufferedManagedStream)));
		}

		[Fact]
		public void UnknownLengthAndBufferedRewindRemainCompatible()
		{
			using var source = new MemoryStream(TwentyBytes);
			using var buffered = CreateNonSeekableReadOnlyStream(source, 10);
			var bytes = new byte[3];

			Assert.True(buffered.CanSeek);
			Assert.Equal(-1, buffered.Length);
			Assert.Equal(3, buffered.Read(bytes, 0, bytes.Length));
			Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
			Assert.Equal(0, buffered.Seek(0, SeekOrigin.Begin));
			Assert.Equal(3, buffered.Read(bytes, 0, bytes.Length));
			Assert.Equal(new byte[] { 1, 2, 3 }, bytes);
			Assert.Equal(-1, buffered.Length);
		}

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
	}
}
