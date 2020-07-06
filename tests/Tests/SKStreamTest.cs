using System;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests
{
	public class SKStreamTest : SKTest
	{
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
			VerifyImmediateFinalizers();

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
			Assert.IsType<SKStream.SKStreamImplementation>(dupe);
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
			Assert.IsType<SKStream.SKStreamImplementation>(dupe);
			Assert.Equal(3, dupe.ReadByte());
			Assert.Equal(3, stream.ReadByte());
			Assert.Equal(4, dupe.ReadByte());
			Assert.Equal(4, stream.ReadByte());
		}
	}
}
