using System;
using System.IO;
using SkiaSharp.Resources;
using Xunit;

namespace SkiaSharp.Tests
{
	public class ResourceProviderTest : SKTest
	{
		[SkippableFact]
		public void FileResourceProviderCanReadFiles()
		{
			var fullPath = Path.Combine(PathToImages, "baboon.png");
			var expectedData = SKData.Create(fullPath);

			using var rp = ResourceProvider.CreateFile(PathToImages);

			using var data = rp.Load("baboon.png");

			Assert.Equal(expectedData.ToArray(), data.ToArray());
		}

		[SkippableFact]
		public void ProxyProviderCanReadFiles()
		{
			var fullPath = Path.Combine(PathToImages, "baboon.png");
			var expectedData = SKData.Create(fullPath);

			using var files = ResourceProvider.CreateFile(PathToImages);
			using var datauri = ResourceProvider.CreateDataUri(files);
			using var caching = ResourceProvider.CreateCaching(datauri);

			using var data = caching.Load("baboon.png");

			Assert.Equal(expectedData.ToArray(), data.ToArray());
		}
	}
}
