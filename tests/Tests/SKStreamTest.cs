using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
	}
}
