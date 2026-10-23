using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp.Tests.Shared;
using SkiaSharp.Tests.Visual;
using SkiaSharp.Tests.Visual.Tests;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GoldenResourceTests
	{
		[Fact]
		public void PackagedGoldenLoadsInTheConfiguredStorageMode()
		{
			var assembly = typeof(GoldenResourceTests).Assembly;
			var embedded = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
				.Single(attribute => attribute.Key == "SkiaSharp.Tests.EmbedGoldens").Value == "true";
			var scene = new FilledCircleScene();
			var resolution = VisualMatrixTestsBase.ResolveGolden("raster", scene.Name);
			Assert.NotNull(resolution.Key);
			Assert.NotNull(resolution.Location);
			var pixels = GoldenImages.Decode(resolution.ReadBytes(), scene.Info, resolution.Location);
			Assert.Equal(scene.Info.BytesSize, pixels.Length);

			if (embedded)
			{
				var names = assembly.GetManifestResourceNames();
				Assert.Contains(GoldenStore.ResourcePrefix + resolution.Key, names);
				var resourceStore = new GoldenStore(Array.Empty<string>(), new[] { assembly });
				var resource = resourceStore.Resolve(VisualMatrixTestsBase.Request("raster", scene.Name));
				Assert.Equal(resolution.Key, resource.Key);
				Assert.Contains("!" + GoldenStore.ResourcePrefix, resource.Location);
				var png = resource.ReadBytes();
				using var image = SKImage.FromEncodedData(png);
				Assert.NotNull(image);
				Assert.Equal(scene.Info.Width, image.Width);
				Assert.Equal(scene.Info.Height, image.Height);
				Assert.Contains(names, name => name.StartsWith(GoldenStore.ResourcePrefix, StringComparison.Ordinal) &&
					name.Contains("ganesh-gl"));
				var missing = resourceStore.Resolve(new GoldenRequest("missing", new Version(0, 0, 0), "Missing/case"));
				Assert.Null(missing.Key);
				Assert.Throws<InvalidOperationException>(() => missing.ReadBytes());

				var root = Path.Combine(AppContext.BaseDirectory, "golden-resource-" + Guid.NewGuid().ToString("N"));
				try
				{
					var path = Path.Combine(root, resolution.Key.Replace('/', Path.DirectorySeparatorChar));
					Directory.CreateDirectory(Path.GetDirectoryName(path));
					File.WriteAllBytes(path, png);
					var mixed = new GoldenStore(new[] { root }, new[] { assembly })
						.Resolve(VisualMatrixTestsBase.Request("raster", scene.Name));
					Assert.Equal(path, mixed.Location);
					Assert.Equal(png, mixed.ReadBytes());
					File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
					Assert.Equal(new byte[] { 1, 2, 3 }, mixed.ReadBytes());
				}
				finally
				{
					if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
				}
			}
			else
			{
				Assert.True(File.Exists(resolution.Location), resolution.Describe());
				Assert.DoesNotContain(assembly.GetManifestResourceNames(),
					name => name.StartsWith(GoldenStore.ResourcePrefix, StringComparison.Ordinal));
			}
		}
	}
}
