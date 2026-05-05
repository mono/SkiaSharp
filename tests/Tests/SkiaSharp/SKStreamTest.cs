using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKStreamTest : SKTest
	{
		[SkippableFact]
		[Trait(Traits.Category.Key, Traits.Category.Values.Smoke)]
		public void CanWriteTextToStream()
		{
			using (var stream = new SKDynamicMemoryWStream())
			{
				Assert.NotNull(stream);
				Assert.Equal(0, stream.BytesWritten);

				stream.WriteText("Hello");

				Assert.Equal(5, stream.BytesWritten);
			}
		}

		[SkippableFact]
		public void SupportsNonASCIICharactersInPath()
		{
			var path = Path.Combine(PathToImages, "上田雅美.jpg");

			using (var stream = new SKFileStream(path))
			{
				Assert.NotNull(stream);
				Assert.True(stream.Length > 0);
				Assert.True(stream.IsValid);
			}
		}

		[SkippableFact]
		public void WriteableFileStreamSelectCorrectStreamForASCIIPath()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + ".jpg");

			using (var stream = SKFileWStream.OpenStream(path))
			{
				Assert.IsType<SKFileWStream>(stream);
			}
		}

		[SkippableFact]
		public void WriteableFileStreamSelectCorrectStreamForNonASCIIPath()
		{
			var path = Path.Combine(PathToImages, Guid.NewGuid().ToString("D") + "-上田雅美.jpg");

			using (var stream = SKFileWStream.OpenStream(path))
			{
				Assert.IsType<SKFileWStream>(stream);
				Assert.True(((SKFileWStream)stream).IsValid);
			}
		}

		[SkippableFact]
		public void FileStreamSelectCorrectStreamForASCIIPath()
		{
			var path = Path.Combine(PathToImages, "baboon.jpg");

			using (var stream = SKFileStream.OpenStream(path))
			{
				Assert.IsType<SKFileStream>(stream);
			}
		}

		[SkippableFact]
		public void FileStreamSelectCorrectStreamForNonASCIIPath()
		{
			var path = Path.Combine(PathToImages, "上田雅美.jpg");

			using (var stream = SKFileStream.OpenStream(path))
			{
				Assert.IsType<SKFileStream>(stream);
				Assert.True(((SKFileStream)stream).IsValid);
			}
		}

		[SkippableFact]
		public void FileStreamForMissingFile()
		{
			var path = Path.Combine(PathToImages, "missing-image.png");

			Assert.Null(SKFileStream.OpenStream(path));

			var stream = new SKFileStream(path);

			Assert.Equal(0, stream.Length);
			Assert.False(stream.IsValid);
		}

		[SkippableFact]
		public void GarbageCollectionCollectsStreams()
		{
			SkipOnMono();

			var path = Path.Combine(PathToImages, "baboon.jpg");

			var weak = DoWork();

			CollectGarbage();

			Assert.False(weak.IsAlive);
			Assert.Null(weak.Target);

			WeakReference DoWork()
			{
				var stream = new SKFileStream(path);
				return new WeakReference(stream);
			}
		}

		[SkippableFact]
		public void MemoryStreamCanBeDuplicated()
		{
			var stream = new SKMemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());
			Assert.Equal(3, stream.ReadByte());

			var dupe = stream.Duplicate();
			Assert.NotSame(stream, dupe);
			Assert.IsType<SKStreamImplementation>(dupe);
			Assert.Equal(1, dupe.ReadByte());
			Assert.Equal(4, stream.ReadByte());
			Assert.Equal(2, dupe.ReadByte());
			Assert.Equal(5, stream.ReadByte());
			Assert.Equal(3, dupe.ReadByte());
		}

		[SkippableFact]
		public void MemoryStreamCanBeForked()
		{
			var stream = new SKMemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			Assert.Equal(1, stream.ReadByte());
			Assert.Equal(2, stream.ReadByte());

			var dupe = stream.Fork();
			Assert.NotSame(stream, dupe);
			Assert.IsType<SKStreamImplementation>(dupe);
			Assert.Equal(3, dupe.ReadByte());
			Assert.Equal(3, stream.ReadByte());
			Assert.Equal(4, dupe.ReadByte());
			Assert.Equal(4, stream.ReadByte());
		}

		[SkippableFact]
		public void MemoryStreamGetDataReturnsBackingData()
		{
			var bytes = new byte[] { 10, 20, 30, 40, 50 };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(bytes.Length, (int)result.Size);
			Assert.Equal(bytes, result.ToArray());
		}

		[SkippableFact]
		public void MemoryStreamGetDataFromByteArrayReturnsData()
		{
			var bytes = new byte[] { 1, 2, 3 };
			using var stream = new SKMemoryStream(bytes);

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(bytes.Length, (int)result.Size);
			Assert.Equal(bytes, result.ToArray());
		}

		[SkippableFact]
		public void MemoryStreamGetDataIsZeroCopy()
		{
			var bytes = new byte[] { 100, 200 };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			using var result1 = stream.GetData();
			using var result2 = stream.GetData();

			Assert.NotNull(result1);
			Assert.NotNull(result2);
			Assert.Equal(result1.Data, result2.Data);
		}

		[SkippableFact]
		public void FileStreamGetDataReturnsNull()
		{
			var path = Path.Combine(PathToImages, "baboon.jpg");
			using var stream = new SKFileStream(path);

			var result = stream.GetData();

			Assert.Null(result);
		}

		[SkippableFact]
		public void EmptyMemoryStreamGetDataReturnsEmptyData()
		{
			using var stream = new SKMemoryStream();

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(0, (int)result.Size);
		}

		[SkippableFact]
		public void GetDataInteropRoundTrip()
		{
			var bytes = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			using var result = stream.GetData();

			Assert.NotNull(result);
			Assert.Equal(bytes, result.ToArray());
		}

		[SkippableFact]
		public void ManagedStreamGetDataReturnsNull()
		{
			using var memoryStream = new MemoryStream(new byte[] { 1, 2, 3 });
			using var stream = new SKManagedStream(memoryStream);

			var result = stream.GetData();

			Assert.Null(result);
		}

		[SkippableFact]
		public void ManagedStreamGetDataReturnsNullForNonSeekableStream()
		{
			using var inner = new MemoryStream(new byte[] { 10, 20, 30 });
			using var nonSeekable = new NonSeekableReadOnlyStream(inner);
			using var stream = new SKManagedStream(nonSeekable);

			var result = stream.GetData();

			Assert.Null(result);
		}

		[SkippableFact]
		public void ManagedStreamGetDataReturnsNullEvenAfterRead()
		{
			using var memoryStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
			using var stream = new SKManagedStream(memoryStream);

			// read some data first, then ask for getData
			stream.ReadByte();
			stream.ReadByte();

			var result = stream.GetData();

			Assert.Null(result);
		}

		[SkippableFact]
		public void GetDataReturnsSameManagedObject()
		{
			var bytes = new byte[] { 10, 20, 30, 40 };
			using var data = SKData.CreateCopy(bytes);
			using var stream = new SKMemoryStream(data);

			var result = stream.GetData();

			// GetOrAddObject finds the existing C# wrapper and returns it
			Assert.Same(data, result);
		}

		[SkippableFact]
		public void GetDataSurvivesStreamDisposal()
		{
			var bytes = new byte[] { 10, 20, 30, 40 };
			using var data = SKData.CreateCopy(bytes);
			var stream = new SKMemoryStream(data);

			var result = stream.GetData();
			Assert.NotNull(result);

			// dispose stream — data survives because SkData is ref-counted
			stream.Dispose();

			Assert.Equal(bytes.Length, (int)data.Size);
			Assert.Equal(bytes, data.ToArray());
		}

		[SkippableFact]
		public void GetDataSurvivesNativeCreatedStreamDisposal()
		{
			var bytes = new byte[] { 0xCA, 0xFE, 0xBA, 0xBE };

			// byte[] constructor: native side creates the SkData internally
			var stream = new SKMemoryStream(bytes);

			var result = stream.GetData();
			Assert.NotNull(result);

			// dispose the stream — result holds its own native ref
			stream.Dispose();

			// result survives because ref-counted SkData keeps it alive
			Assert.Equal(bytes.Length, (int)result.Size);
			Assert.Equal(bytes, result.ToArray());

			result.Dispose();
		}
	}
}
