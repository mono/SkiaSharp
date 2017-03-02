using System;
using System.IO;
using NUnit.Framework;

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

		[Test]
		public void ImageCanBeDecoded()
		{
			var path = Path.Combine(PathToImages, "color-wheel.png");
			var buffered = CreateNonSeekableReadOnlyStream(File.OpenRead(path), SKCodec.MinBufferedBytesNeeded);

			var bitmap = SKBitmap.Decode(buffered);

			Assert.IsNotNull(bitmap);
			Assert.IsNotNull(bitmap.PeekPixels());
		}

		[Test]
		public void OnlyReadWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			var buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 8, 9, 10, 11, 12, 13, 14 }, buffer);

			buffer = new byte[7];
			Assert.AreEqual(6, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 15, 16, 17, 18, 19, 20, 0 }, buffer);
		}

		[Test]
		public void RewindInBufferWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// rewind
			buffered.Position = 0;

			// re-read (from the internal buffer)
			buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// continue reading
			buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 8, 9, 10, 11, 12, 13, 14 }, buffer);
		}

		[Test]
		public void SeekInBufferWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// rewind
			buffered.Position = 2;

			// re-read (from the internal buffer and the stream)
			buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 3, 4, 5, 6, 7, 8, 9 }, buffer);
		}

		[Test]
		public void ReadPastBufferAfterSeekInBufferWorks()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// rewind
			buffered.Position = 5;

			// re-read (from the internal buffer and the stream)
			buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 6, 7, 8, 9, 10, 11, 12 }, buffer);
		}

		[Test]
		public void RewindOutsideBufferThrows()
		{
			var ms = new MemoryStream(TwentyBytes);
			var buffered = CreateNonSeekableReadOnlyStream(ms, 10);

			// read
			var buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, buffer);

			// read past the buffer
			buffer = new byte[7];
			Assert.AreEqual(7, buffered.Read(buffer, 0, 7));
			Assert.AreEqual(new byte[] { 8, 9, 10, 11, 12, 13, 14 }, buffer);

			// we can't seek after we have passed the buffer
			Assert.Throws<InvalidOperationException>(() => buffered.Position = 0);
		}
	}
}
