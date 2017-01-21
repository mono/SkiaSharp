using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKManagedStreamTest : SKTest
	{
		[Fact]
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
			Assert.Equal(0, stream.Position);
			Assert.Equal(0, skManagedStream.Position);

			for (int i = 0; i < data.Length; i++)
			{
				skManagedStream.Position = i;

				Assert.Equal(i, stream.Position);
				Assert.Equal(i, skManagedStream.Position);

				Assert.Equal((byte)(i % byte.MaxValue), data[i]);
				Assert.Equal((byte)(i % byte.MaxValue), skManagedStream.ReadByte());

				Assert.Equal(i + 1, stream.Position);
				Assert.Equal(i + 1, skManagedStream.Position);
			}
		}

		[Fact]
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
			Assert.Equal(0, stream.Position);
			Assert.Equal(0, skManagedStream.Position);

			var buffer = new byte[data.Length / 2];
			skManagedStream.Read(buffer, buffer.Length);

			Assert.Equal(data.Take(buffer.Length), buffer);
		}

		[Fact]
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

			Assert.Equal(data.Length - offset, taken);

			var resultData = data.Skip(offset).Take(buffer.Length).ToArray();
			resultData = resultData.Concat(Enumerable.Repeat<byte>(0, offset)).ToArray();
			Assert.Equal(resultData, buffer);
		}
	}
}
