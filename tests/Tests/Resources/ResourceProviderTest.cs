﻿using System;
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

			using var rp = new FileResourceProvider(PathToImages);

			using var data = rp.Load("baboon.png");

			Assert.Equal(expectedData.ToArray(), data.ToArray());
		}

		[SkippableFact]
		public void ProxyProviderCanReadFiles()
		{
			var fullPath = Path.Combine(PathToImages, "baboon.png");
			var expectedData = SKData.Create(fullPath);

			using var files = new FileResourceProvider(PathToImages);
			using var datauri = new DataUriResourceProvider(files);
			using var caching = new CachingResourceProvider(datauri);

			using var data = caching.Load("baboon.png");

			Assert.Equal(expectedData.ToArray(), data.ToArray());
		}

		[SkippableFact]
		public void CanCreateDefaultDataUri()
		{
			using var datauri = new DataUriResourceProvider();

			Assert.NotNull(datauri);
		}

		[SkippableFact]
		public void CanCreateNullDataUri()
		{
			using var datauri = new DataUriResourceProvider(null);

			Assert.NotNull(datauri);
		}
	}
}
