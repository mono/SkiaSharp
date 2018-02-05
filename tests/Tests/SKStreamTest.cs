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
				if (IsWindows)
				{
					Assert.Equal(0, stream.Length);

					throw new SkipException("Windows does not support non-ASCII characters: https://github.com/mono/SkiaSharp/issues/390");
				}
				else
				{
					Assert.NotNull(stream);
					Assert.True(stream.Length > 0);
				}
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
				if (IsWindows)
				{
					Assert.IsType<SKManagedWStream>(stream);
				}
				else
				{
					Assert.IsType<SKFileWStream>(stream);
				}
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
				if (IsWindows)
				{
					Assert.IsType<SKManagedStream>(stream);
				}
				else
				{
					Assert.IsType<SKFileStream>(stream);
				}
			}
		}
	}
}
