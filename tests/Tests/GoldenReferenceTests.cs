#nullable enable
using System;
using System.IO;
using System.Linq;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GoldenReferenceTests : IDisposable
	{
		private readonly string root = Path.Combine(TestConfig.Current.PathRoot ?? AppContext.BaseDirectory,
			"golden-tests-" + Guid.NewGuid().ToString("N"));

		private Golden Resolve(
			string[] keys,
			string profile,
			string casePath,
			Version? version = null,
			string? architecture = null,
			string? variant = null)
		{
			var directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
			foreach (var key in keys)
			{
				var path = Path.Combine(directory, key.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
			}
			return new GoldenStore(new[] { directory })
				.Resolve(new GoldenCase(profile, casePath, version, architecture, variant));
		}

		[Theory]
		[InlineData("1.0.0", "mac/18.2.0/Text.png")]
		[InlineData("18.2.0", "mac/18.2.0/Text.png")]
		[InlineData("18.2.1", "mac/26.1.0/Text.png")]
		[InlineData("26.1.0", "mac/26.1.0/Text.png")]
		[InlineData("26.1.1", "mac/Text.png")]
		public void VersionFoldersAreInclusiveUpperBoundsAndRootIsLatest(string runtime, string expected)
		{
			var keys = new[] { "mac/18.2.0/Text.png", "mac/26.1.0/Text.png", "mac/Text.png" };
			Assert.Equal(
				expected,
				Resolve(keys.AsEnumerable().Reverse().ToArray(), "mac", "Text", Version.Parse(runtime))
					.ReferenceKey);
		}

		[Fact]
		public void UnversionedRequestsUseOnlyTheLatestRoot()
		{
			var result = Resolve(new[] { "mac/18.2.0/Text.png", "mac/Text.png" }, "mac", "Text");
			Assert.Equal("mac/Text.png", result.ReferenceKey);
			Assert.Equal(new[] { "mac/Text.png" }, result.Attempts);
		}

		[Fact]
		public void MissingTraceShowsFutureSnapshotsThenLatestRoot()
		{
			var result = Resolve(
				new[] { "mac/18.2.0/Other.png", "mac/26.1.0/Other.png" },
				"mac", "Group/Text", new Version(18, 2, 1), "arm64");
			Assert.Null(result.ReferenceKey);
			Assert.Null(result.ReferenceLocation);
			Assert.Equal(new[]
			{
				"mac/26.1.0/Group/Text.arm64.png", "mac/26.1.0/Group/Text.png",
				"mac/Group/Text.arm64.png", "mac/Group/Text.png",
				"portable/Group/Text.arm64.png", "portable/Group/Text.png",
			}, result.Attempts);
			Assert.Contains("Golden request: mac at 18.2.1/Group/Text", result.Describe());
			Assert.Contains("MISSING reference", result.Describe());
			Assert.Throws<InvalidOperationException>(() => result.ReadBytes());
		}

		[Theory]
		[InlineData("12.6", "12.6.0")]
		[InlineData("12.6.2", "12.6.2")]
		[InlineData("12.6.2.10", "12.6.2.10")]
		public void SystemVersionsNormalizeToAtLeastThreeComponents(string input, string expected)
		{
			var goldenCase = new GoldenCase("mac", "Text", Version.Parse(input));
			Assert.Equal(Version.Parse(expected), goldenCase.Version);
			Assert.Equal("mac/Text.png", goldenCase.CaptureKey);
		}

		[Theory]
		[InlineData("macos", "mac")]
		[InlineData("mac", "mac")]
		[InlineData("linux", "linux")]
		[InlineData("new-platform", "new-platform")]
		[InlineData("docker-linux", "docker-linux")]
		public void PlatformsNormalizeAndUseTheGeneralPortableFallback(string platform, string normalized)
		{
			var goldenCase = new GoldenCase(platform, "Text");
			Assert.Equal(normalized, goldenCase.Profile);
			Assert.Equal(
				"portable/Text.png",
				Resolve(new[] { "portable/Text.png" }, platform, "Text").ReferenceKey);
		}

		[Fact]
		public void EachProfileIsExhaustedBeforeTheNextFallback()
		{
			var keys = new[]
			{
				"nanoserver/Text.png", "windows/26.1.0/Text.png", "windows/Text.png",
				"portable/21.0.0/Text.png", "portable/Text.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(
					keys[i],
					Resolve(keys.Skip(i).ToArray(), "nanoserver", "Text", new Version(20, 0, 0))
						.ReferenceKey);
		}

		[Fact]
		public void VariantThenArchitectureOrderDoesNotBorrowNeutralOrSiblingVariants()
		{
			var ordered = new[]
			{
				"portable/Text.ganesh-gl.arm64.png", "portable/Text.ganesh-gl.png",
			};
			for (var i = 0; i < ordered.Length; i++)
				Assert.Equal(
					ordered[i],
					Resolve(
						ordered.Skip(i).ToArray(),
						"portable", "Text", architecture: "arm64", variant: "ganesh-gl")
						.ReferenceKey);

			var missing = Resolve(new[]
			{
				"portable/Text.arm64.png", "portable/Text.png",
				"portable/Text.ganesh-metal.arm64.png", "portable/Text.ganesh-gl.x64.png",
			}, "portable", "Text", architecture: "arm64", variant: "ganesh-gl");
			Assert.Null(missing.ReferenceKey);
			Assert.Equal(ordered, missing.Attempts);
		}

		[Theory]
		[InlineData("Console/output")]
		[InlineData("Web/page")]
		[InlineData("Maui/home")]
		[InlineData("DiagonalLines")]
		[InlineData("3D")]
		public void HierarchicalCasePathsPreserveCase(string casePath)
		{
			Assert.Equal(casePath, new GoldenCase("mac", casePath).CasePath);
			var key = "mac/" + casePath + ".png";
			Assert.Equal(key, Resolve(new[] { key }, "mac", casePath).ReferenceKey);
		}

		[Theory]
		[InlineData("")]
		[InlineData("/Text")]
		[InlineData("C:/Text")]
		[InlineData("Text\\Lines")]
		[InlineData("Text//Lines")]
		[InlineData("Text/../Lines")]
		[InlineData("Text/./Lines")]
		[InlineData("Text/")]
		[InlineData("Text.png")]
		[InlineData("Text Lines")]
		[InlineData("Text/CON")]
		public void UnsafeCasePathsAreRejected(string casePath) =>
			Assert.Throws<ArgumentException>(() =>
				new GoldenCase("mac", casePath));

		[Theory]
		[InlineData("x86")]
		[InlineData("x64")]
		[InlineData("arm")]
		[InlineData("arm64")]
		[InlineData("armv6")]
		[InlineData("wasm")]
		[InlineData("s390x")]
		[InlineData("loongarch64")]
		[InlineData("ppc64le")]
		[InlineData("riscv64")]
		public void ArchitectureTokensAreReserved(string architecture)
		{
			var goldenCase = new GoldenCase(
				"portable",
				"Text",
				architecture: architecture,
				variant: "raster");
			Assert.Equal(architecture, goldenCase.Architecture);
			var key = "portable/Text.raster." + architecture + ".png";
			Assert.Equal(
				key,
				Resolve(
					new[] { key }, "portable", "Text", architecture: architecture, variant: "raster")
					.ReferenceKey);
			Assert.Throws<ArgumentException>(() =>
				new GoldenCase("mac", "Text", variant: architecture));
		}

		[Theory]
		[InlineData("mac/12.0/Text.png")]
		[InlineData("mac/12.00.0/Text.png")]
		[InlineData("mac/1.0.0/CON/Text.png")]
		[InlineData("mac/Text.x64.raster.png")]
		[InlineData("mac/Text.raster.unknown.png")]
		[InlineData("mac/Text.x64.arm64.png")]
		[InlineData("mac/Text.PNG")]
		public void MalformedStoredKeysAreRejected(string key) =>
			Assert.Throws<ArgumentException>(() => Resolve(new[] { key }, "mac", "Text"));

		[Fact]
		public void StoreCachesItsSourceIndexButReadsSelectedBytesFresh()
		{
			var first = Path.Combine(root, "first");
			var second = Path.Combine(root, "second");
			const string key = "portable/Group/Text.png";
			foreach (var directory in new[] { first, second })
			{
				var path = Path.Combine(directory, key.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				File.WriteAllBytes(path, new byte[] { directory == first ? (byte)1 : (byte)2 });
			}
			var store = new GoldenStore(new[] { first, second, first });
			var result = store.Resolve(new GoldenCase("portable", "Group/Text"));
			Assert.Equal(
				Path.Combine(first, key.Replace('/', Path.DirectorySeparatorChar)),
				result.ReferenceLocation);
			Assert.Equal(new byte[] { 1 }, result.ReadBytes());
			File.WriteAllBytes(result.ReferenceLocation!, new byte[] { 3 });
			Assert.Equal(new byte[] { 3 }, result.ReadBytes());

			var added = Path.Combine(first, "portable", "Group", "Added.png");
			Directory.CreateDirectory(Path.GetDirectoryName(added)!);
			File.WriteAllBytes(added, new byte[] { 4 });
			Assert.Null(store.Resolve(new GoldenCase("portable", "Group/Added")).ReferenceKey);
		}

		[Fact]
		public void DiagnosticsAlwaysProposeTheLatestRoot()
		{
			var result = Resolve(
				new[] { "portable/Maui/home.raster.png" },
				"macos", "Maui/home", new Version(12, 6), "arm64", "raster");
			Assert.Equal("portable/Maui/home.raster.png", result.ReferenceKey);
			Assert.Equal("mac/Maui/home.raster.arm64.png", result.CaptureKey);
			Assert.StartsWith(
				"##SKIA-GOLDEN## role=actual key=mac/Maui/home.raster.arm64.png size=1x2 base64=",
				result.FormatMarker("actual", OneByTwoPng()));
		}

		private static byte[] OneByTwoPng()
		{
			using var bitmap = new SKBitmap(1, 2);
			using var image = SKImage.FromBitmap(bitmap);
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			return data.ToArray();
		}

		public void Dispose()
		{
			if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
		}
	}
}
