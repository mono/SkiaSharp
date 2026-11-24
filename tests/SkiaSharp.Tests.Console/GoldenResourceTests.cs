using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
			var golden = VisualMatrixTestsBase.Resolve("raster", scene.Name);
			Assert.NotNull(golden.ReferenceKey);
			Assert.NotNull(golden.ReferenceLocation);
			using var image = SKImage.FromEncodedData(golden.ReadBytes());
			Assert.NotNull(image);
			Assert.Equal(scene.Info.Width, image.Width);
			Assert.Equal(scene.Info.Height, image.Height);

			if (embedded)
			{
				var names = assembly.GetManifestResourceNames();
				Assert.Contains(GoldenStore.ResourcePrefix + golden.ReferenceKey, names);
				var resourceStore = new GoldenStore(Array.Empty<string>(), new[] { assembly });
				var resource = resourceStore.Resolve(new(
					TestConfig.Current.PlatformName,
					scene.Name,
					architecture: RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
					variant: "raster"));
				Assert.Equal(golden.ReferenceKey, resource.ReferenceKey);
				Assert.Contains("!" + GoldenStore.ResourcePrefix, resource.ReferenceLocation);
				var png = resource.ReadBytes();
				using var embeddedImage = SKImage.FromEncodedData(png);
				Assert.NotNull(embeddedImage);
				Assert.Equal(scene.Info.Width, embeddedImage.Width);
				Assert.Equal(scene.Info.Height, embeddedImage.Height);
				Assert.Contains(names, name => name.StartsWith(GoldenStore.ResourcePrefix, StringComparison.Ordinal) &&
					name.Contains("ganesh-gl"));
				var missing = resourceStore.Resolve(new("missing", "Missing/case"));
				Assert.Null(missing.ReferenceKey);
				Assert.Throws<InvalidOperationException>(() => missing.ReadBytes());

				var root = Path.Combine(AppContext.BaseDirectory, "golden-resource-" + Guid.NewGuid().ToString("N"));
				try
				{
					var path = Path.Combine(root, golden.ReferenceKey.Replace('/', Path.DirectorySeparatorChar));
					Directory.CreateDirectory(Path.GetDirectoryName(path));
					File.WriteAllBytes(path, png);
					var mixed = new GoldenStore(new[] { root }, new[] { assembly })
						.Resolve(new(
							TestConfig.Current.PlatformName,
							scene.Name,
							architecture: RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
							variant: "raster"));
					Assert.Equal(path, mixed.ReferenceLocation);
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
				Assert.True(File.Exists(golden.ReferenceLocation), golden.Describe());
				Assert.DoesNotContain(assembly.GetManifestResourceNames(),
					name => name.StartsWith(GoldenStore.ResourcePrefix, StringComparison.Ordinal));
			}
		}
	}
}
