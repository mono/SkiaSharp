using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace SkiaSharp.Tests
{
	public class SKManagedStreamTest : SKTest
	{
		[Test]
		public void ManagedStreamReadsByteCorrectly()
		{
			var data = new byte[1024];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var skManagedStream = new SKManagedStream(stream);

			skManagedStream.Rewind();
			Assert.AreEqual(0, stream.Position);
			Assert.AreEqual(0, skManagedStream.Position);

			for (int i = 0; i < data.Length; i++)
			{
				skManagedStream.Position = i;

				Assert.AreEqual(i, stream.Position);
				Assert.AreEqual(i, skManagedStream.Position);

				Assert.AreEqual((byte)(i % byte.MaxValue), data[i]);
				Assert.AreEqual((byte)(i % byte.MaxValue), skManagedStream.ReadByte());

				Assert.AreEqual(i + 1, stream.Position);
				Assert.AreEqual(i + 1, skManagedStream.Position);
			}
		}

		[Test]
		public void ManagedStreamReadsChunkCorrectly()
		{
			var data = new byte[1024];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var skManagedStream = new SKManagedStream(stream);

			skManagedStream.Rewind();
			Assert.AreEqual(0, stream.Position);
			Assert.AreEqual(0, skManagedStream.Position);

			var buffer = new byte[data.Length / 2];
			skManagedStream.Read(buffer, buffer.Length);

			Assert.AreEqual(data.Take(buffer.Length), buffer);
		}

		[Test]
		public void ManagedStreamReadsOffsetChunkCorrectly()
		{
			var data = new byte[1024];
			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(i % byte.MaxValue);
			}

			var stream = new MemoryStream(data);
			var skManagedStream = new SKManagedStream(stream);

			var offset = 768;

			skManagedStream.Position = offset;

			var buffer = new byte[data.Length];
			var taken = skManagedStream.Read(buffer, buffer.Length);

			Assert.AreEqual(data.Length - offset, taken);

			var resultData = data.Skip(offset).Take(buffer.Length).ToArray();
			resultData = resultData.Concat(Enumerable.Repeat<byte>(0, offset)).ToArray();
			Assert.AreEqual(resultData, buffer);
		}
	}
}
