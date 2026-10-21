using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GoldenResourceTests
	{
		[Fact]
		public void PackagedGoldenKeysAndBytesMatchTheirSourceAssets()
		{
			var assembly = TestConfig.Current.GetType().Assembly;
			var sourceRoot = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
				.Single(a => a.Key == "SkiaSharp.GoldenSourceRoot").Value;
			var resources = assembly.GetManifestResourceNames()
				.Where(n => n.StartsWith(GoldenStoreIndex.ResourcePrefix, StringComparison.Ordinal)).ToArray();
			var packaged = resources.Length > 0
				? GoldenStoreIndex.FromAssembly(assembly)
				: GoldenStoreIndex.FromDirectory(Path.Combine(AppContext.BaseDirectory, "Goldens", "v1"));
			if (!Directory.Exists(sourceRoot))
			{
				Assert.NotEmpty(resources);
				foreach (var name in resources)
				{
					var key = GoldenKey.Parse(name.Substring(GoldenStoreIndex.ResourcePrefix.Length));
					var resolution = packaged.Resolve(new GoldenRequest(key.Profile, key.Version,
						key.CaseName, key.Architecture ?? "arm64", key.Variant));
					Assert.NotNull(resolution.Asset);
					using var data = SKData.CreateCopy(resolution.Asset.ReadBytes());
					using var codec = SKCodec.Create(data);
					Assert.NotNull(codec);
					Assert.True(codec.Info.Width > 0 && codec.Info.Height > 0);
				}
				return;
			}
			var source = GoldenStoreIndex.FromDirectory(sourceRoot);
			foreach (var path in Directory.EnumerateFiles(sourceRoot, "visual-*.png", SearchOption.AllDirectories))
			{
				var relative = path.Substring(sourceRoot.TrimEnd(Path.DirectorySeparatorChar).Length + 1)
					.Replace(Path.DirectorySeparatorChar, '/');
				var key = GoldenKey.Parse(relative);
				var request = new GoldenRequest(key.Profile, key.Version, key.CaseName,
					key.Architecture ?? "arm64", key.Variant);
				var expected = source.Resolve(request).Asset;
				var actual = packaged.Resolve(request).Asset;
				Assert.NotNull(expected);
				Assert.NotNull(actual);
				Assert.Equal(expected.Key.ToString(), actual.Key.ToString());
				Assert.Equal(expected.ReadBytes(), actual.ReadBytes());
			}
			Assert.DoesNotContain(resources, n => !n.Substring(GoldenStoreIndex.ResourcePrefix.Length).Split('/')[2].StartsWith("visual-", StringComparison.Ordinal));
		}
	}
}
