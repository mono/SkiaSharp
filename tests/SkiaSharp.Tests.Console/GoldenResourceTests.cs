using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp.Tests.Visual;
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
			var golden = GoldenStore.TryLoad("raster", scene.Name, scene.Info, out var resolution);
			Assert.True(golden.HasValue, resolution.Describe());
			Assert.Equal(scene.Info.BytesSize, golden.Value.Pixels.Length);
			Assert.NotNull(resolution.Key);
			Assert.NotNull(resolution.Path);

			if (embedded)
			{
				var names = assembly.GetManifestResourceNames();
				Assert.Contains(GoldenStore.ResourcePrefix + resolution.Key, names);
				var png = GoldenStore.ReadEmbedded(assembly, resolution.Key);
				using var image = SKImage.FromEncodedData(png);
				Assert.NotNull(image);
				Assert.Equal(scene.Info.Width, image.Width);
				Assert.Equal(scene.Info.Height, image.Height);
				Assert.Contains(names, name => name.StartsWith(GoldenStore.ResourcePrefix, StringComparison.Ordinal) &&
					name.Contains("ganesh-gl"));
				Assert.Throws<InvalidOperationException>(() =>
					GoldenStore.ReadEmbedded(assembly, "missing/base/missing.png"));
			}
			else
			{
				Assert.True(File.Exists(resolution.Path), resolution.Describe());
				Assert.DoesNotContain(assembly.GetManifestResourceNames(),
					name => name.StartsWith(GoldenStore.ResourcePrefix, StringComparison.Ordinal));
			}
		}
	}
}
