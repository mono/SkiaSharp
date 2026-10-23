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

		private GoldenResolution Resolve(string[] keys, GoldenRequest request)
		{
			var directory = Path.Combine(root, Guid.NewGuid().ToString("N"));
			foreach (var key in keys)
			{
				var path = Path.Combine(directory, key.Replace('/', Path.DirectorySeparatorChar));
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				File.WriteAllBytes(path, new byte[] { 1, 2, 3 });
			}
			return new GoldenStore(new[] { directory }).Resolve(request);
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
			var request = new GoldenRequest("mac", "Text", Version.Parse(runtime));
			Assert.Equal(expected, Resolve(keys.AsEnumerable().Reverse().ToArray(), request).Key);
		}

		[Fact]
		public void UnversionedRequestsUseOnlyTheLatestRoot()
		{
			var result = Resolve(new[] { "mac/18.2.0/Text.png", "mac/Text.png" },
				new GoldenRequest("mac", "Text"));
			Assert.Equal("mac/Text.png", result.Key);
			Assert.Equal(new[] { "mac/Text.png" }, result.Candidates);
		}

		[Fact]
		public void MissingTraceShowsFutureSnapshotsThenLatestRoot()
		{
			var request = new GoldenRequest("mac", "Group/Text", new Version(18, 2, 1), "arm64");
			var result = Resolve(new[] { "mac/18.2.0/Other.png", "mac/26.1.0/Other.png" }, request);
			Assert.Null(result.Key);
			Assert.Null(result.Location);
			Assert.Equal(new[]
			{
				"mac/26.1.0/Group/Text.arm64.png", "mac/26.1.0/Group/Text.png",
				"mac/Group/Text.arm64.png", "mac/Group/Text.png",
				"portable/Group/Text.arm64.png", "portable/Group/Text.png",
			}, result.Candidates);
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
			var request = new GoldenRequest("mac", "Text", Version.Parse(input));
			Assert.Equal(Version.Parse(expected), request.Version);
			Assert.Equal("mac/Text.png", request.CaptureKey);
		}

		[Theory]
		[InlineData("macos", "mac")]
		[InlineData("mac", "mac")]
		[InlineData("linux", "linux")]
		[InlineData("new-platform", "new-platform")]
		[InlineData("docker-linux", "docker-linux")]
		public void PlatformsNormalizeAndUseTheGeneralPortableFallback(string platform, string normalized)
		{
			var request = new GoldenRequest(platform, "Text");
			Assert.Equal(normalized, request.Platform);
			Assert.Equal("portable/Text.png",
				Resolve(new[] { "portable/Text.png" }, request).Key);
		}

		[Fact]
		public void EachProfileIsExhaustedBeforeTheNextFallback()
		{
			var request = new GoldenRequest("nanoserver", "Text", new Version(20, 0, 0));
			var keys = new[]
			{
				"nanoserver/Text.png", "windows/26.1.0/Text.png", "windows/Text.png",
				"portable/21.0.0/Text.png", "portable/Text.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(keys[i], Resolve(keys.Skip(i).ToArray(), request).Key);
		}

		[Fact]
		public void VariantThenArchitectureOrderDoesNotBorrowNeutralOrSiblingVariants()
		{
			var ordered = new[]
			{
				"portable/Text.ganesh-gl.arm64.png", "portable/Text.ganesh-gl.png",
			};
			var request = new GoldenRequest("portable", "Text", architecture: "arm64",
				variant: "ganesh-gl");
			for (var i = 0; i < ordered.Length; i++)
				Assert.Equal(ordered[i], Resolve(ordered.Skip(i).ToArray(), request).Key);

			var missing = Resolve(new[]
			{
				"portable/Text.arm64.png", "portable/Text.png",
				"portable/Text.ganesh-metal.arm64.png", "portable/Text.ganesh-gl.x64.png",
			}, request);
			Assert.Null(missing.Key);
			Assert.Equal(ordered, missing.Candidates);
		}

		[Theory]
		[InlineData("Console/output")]
		[InlineData("Web/page")]
		[InlineData("Maui/home")]
		[InlineData("DiagonalLines")]
		[InlineData("3D")]
		public void HierarchicalCasePathsPreserveCase(string casePath)
		{
			var request = new GoldenRequest("mac", casePath);
			Assert.Equal(casePath, request.CasePath);
			var key = "mac/" + casePath + ".png";
			Assert.Equal(key, Resolve(new[] { key }, request).Key);
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
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", casePath));

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
			var request = new GoldenRequest("portable", "Text", architecture: architecture, variant: "raster");
			var key = "portable/Text.raster." + architecture + ".png";
			Assert.Equal(key, Resolve(new[] { key }, request).Key);
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "Text", variant: architecture));
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
			Assert.Throws<ArgumentException>(() => Resolve(new[] { key }, new GoldenRequest("mac", "Text")));

		[Fact]
		public void OrderedSourcesWinAndBytesAreReadFresh()
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
			var result = new GoldenStore(new[] { first, second, first })
				.Resolve(new GoldenRequest("portable", "Group/Text"));
			Assert.Equal(Path.Combine(first, key.Replace('/', Path.DirectorySeparatorChar)), result.Location);
			Assert.Equal(new byte[] { 1 }, result.ReadBytes());
			File.WriteAllBytes(result.Location!, new byte[] { 3 });
			Assert.Equal(new byte[] { 3 }, result.ReadBytes());
		}

		[Fact]
		public void DiagnosticsAlwaysProposeTheLatestRoot()
		{
			var request = new GoldenRequest("macos", "Maui/home", new Version(12, 6), "arm64", "raster");
			var result = Resolve(new[] { "portable/Maui/home.raster.png" }, request);
			Assert.Equal("portable/Maui/home.raster.png", result.Key);
			Assert.Equal("mac/Maui/home.raster.arm64.png", request.CaptureKey);
			Assert.Equal("##SKIA-GOLDEN## role=actual key=mac/Maui/home.raster.arm64.png size=1x2 base64=AQID",
				GoldenImage.Marker("actual", request, 1, 2, new byte[] { 1, 2, 3 }));
		}

		public void Dispose()
		{
			if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
		}
	}
}
