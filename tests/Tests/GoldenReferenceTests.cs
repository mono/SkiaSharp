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

		[Fact]
		public void ExactThenExistingOlderVersionsThenZeroBaseline()
		{
			var request = new GoldenRequest("mac", new Version(12, 12, 2), "Text", "arm64");
			var keys = new[]
			{
				"mac/12.12.2/Text.png", "mac/12.12.1/Text.arm64.png", "mac/12.10.0/Text.png",
				"mac/12.9.0/Text.png", "mac/11.0.0/Text.png", "mac/0.0.0/Text.arm64.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(keys[i], Resolve(keys.Skip(i).Reverse().ToArray(), request).Key);
		}

		[Fact]
		public void FutureVersionsAreNotAttemptedAndMissingTraceIsReadable()
		{
			var request = new GoldenRequest("mac", new Version(12, 6, 2), "Group/Text", "arm64");
			var result = Resolve(new[] { "mac/12.6.3/Group/Text.png", "portable/13.0.0/Group/Text.png" }, request);
			Assert.Same(request, result.Request);
			Assert.Null(result.Key);
			Assert.Null(result.Location);
			Assert.Equal(new[]
			{
				"mac/12.6.2/Group/Text.arm64.png", "mac/12.6.2/Group/Text.png",
				"mac/0.0.0/Group/Text.arm64.png", "mac/0.0.0/Group/Text.png",
				"portable/12.6.2/Group/Text.arm64.png", "portable/12.6.2/Group/Text.png",
				"portable/0.0.0/Group/Text.arm64.png", "portable/0.0.0/Group/Text.png",
			}, result.Candidates);
			Assert.Contains("Golden request: mac/12.6.2/Group/Text", result.Describe());
			Assert.Contains("MISSING reference", result.Describe());
			Assert.Contains("Attempted (in order):", result.Describe());
			Assert.Contains(string.Join(Environment.NewLine + "  ", result.Candidates), result.Describe());
			Assert.Throws<InvalidOperationException>(() => result.ReadBytes());
		}

		[Theory]
		[InlineData("12.6", "12.6.0")]
		[InlineData("0.0", "0.0.0")]
		[InlineData("12.6.2", "12.6.2")]
		[InlineData("12.6.2.0", "12.6.2.0")]
		[InlineData("12.6.2.10", "12.6.2.10")]
		public void SystemVersionsNormalizeToAtLeastThreeComponents(string input, string expected)
		{
			var request = new GoldenRequest("mac", Version.Parse(input), "Text");
			Assert.Equal(Version.Parse(expected), request.Version);
			Assert.Equal("Visual/mac/" + expected + "/Text.png", GoldenDiagnostics.CaptureKey("Visual", request));
			Assert.Equal("mac/" + expected + "/Text.png", Resolve(new[] { "mac/" + expected + "/Text.png" }, request).Key);
		}

		[Fact]
		public void FourComponentVersionsUseSystemVersionOrdering()
		{
			var keys = new[] { "mac/1.2.3.9/Text.png", "mac/1.2.3.10/Text.png", "mac/1.2.3/Text.png" };
			Assert.Equal(keys[1], Resolve(keys, new GoldenRequest("mac", new Version(1, 2, 3, 11), "Text")).Key);
			var result = Resolve(keys, new GoldenRequest("mac", new Version(1, 2, 3), "Text"));
			Assert.Equal(keys[2], result.Key);
			Assert.Single(result.Candidates);
		}

		[Theory]
		[InlineData("macos", "mac,portable")]
		[InlineData("mac", "mac,portable")]
		[InlineData("linux", "linux,portable")]
		[InlineData("new-platform", "new-platform,portable")]
		[InlineData("docker-linux", "docker-linux,portable")]
		[InlineData("docker-windows", "docker-windows,portable")]
		[InlineData("nanoserver", "nanoserver,windows,portable")]
		[InlineData("portable", "portable")]
		public void PlatformPolicyIsDeterministicAndDeduplicated(string profile, string chain) =>
			Assert.Equal(chain.Split(','), GoldenPlatform.Profiles(profile));

		[Fact]
		public void EachProfileSearchesNormalVersionsBeforeMovingToTheNext()
		{
			var request = new GoldenRequest("nanoserver", new Version(12, 6, 2), "Text");
			var keys = new[]
			{
				"nanoserver/0.0.0/Text.png", "windows/12.6.2/Text.png",
				"windows/12.5.0/Text.png", "windows/0.0.0/Text.png", "portable/12.6.1/Text.png",
				"portable/0.0.0/Text.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(keys[i], Resolve(keys.Skip(i).ToArray(), request).Key);
		}

		[Fact]
		public void VariantThenArchitectureOrderDoesNotBorrowSiblingVariants()
		{
			var ordered = new[]
			{
				"portable/0.0.0/Text.ganesh-gl.arm64.png", "portable/0.0.0/Text.ganesh-gl.png",
				"portable/0.0.0/Text.arm64.png", "portable/0.0.0/Text.png",
			};
			var request = new GoldenRequest("portable", new Version(0, 0, 0), "Text", "arm64", "ganesh-gl", allowNeutralVariant: true);
			for (var i = 0; i < ordered.Length; i++)
			{
				var result = Resolve(ordered.Skip(i).ToArray(), request);
				Assert.Equal(ordered[i], result.Key);
				Assert.Equal(ordered.Take(i + 1), result.Candidates);
			}
			var strict = new GoldenRequest("portable", new Version(0, 0, 0), "Text", "arm64", "ganesh-gl");
			var missing = Resolve(ordered.Skip(2).Concat(new[]
			{
				"portable/0.0.0/Text.ganesh-metal.arm64.png", "portable/0.0.0/Text.ganesh-gl.x64.png",
			}).ToArray(), strict);
			Assert.Null(missing.Key);
			Assert.Equal(ordered.Take(2), missing.Candidates);
			Assert.Null(Resolve(ordered.Take(3).ToArray(), new GoldenRequest("portable", new Version(0, 0, 0), "Text")).Key);
		}

		[Fact]
		public void ExactVersionBeatsOlderVariantSpecificityAndInputOrderDoesNotMatter()
		{
			var keys = new[] { "mac/12.6.2/Text.png", "mac/12.6.1/Text.ganesh-gl.arm64.png" };
			var request = new GoldenRequest("mac", new Version(12, 6, 2), "Text", "arm64", "ganesh-gl", allowNeutralVariant: true);
			var result = Resolve(keys, request);
			var reordered = Resolve(keys.Reverse().ToArray(), request);
			Assert.Equal(keys[0], result.Key);
			Assert.Equal(result.Key, reordered.Key);
			Assert.Equal(result.Candidates, reordered.Candidates);
			Assert.Contains("Selected: " + result.Key + " at ", result.Describe());
		}

		[Theory]
		[InlineData("Console/output")]
		[InlineData("Web/page")]
		[InlineData("Maui/home")]
		[InlineData("DiagonalLines")]
		public void HierarchicalCasePathsPreserveCase(string casePath)
		{
			var request = new GoldenRequest("mac", new Version(0, 0, 0), casePath);
			Assert.Equal(casePath, request.CasePath);
			var key = "mac/0.0.0/" + casePath + ".png";
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
		[InlineData("Text/aux")]
		[InlineData("Text/unsafe\n")]
		public void UnsafeCasePathsAreRejected(string casePath) =>
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", new Version(0, 0, 0), casePath));

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
			var request = new GoldenRequest("portable", new Version(0, 0, 0), "Text", architecture, "raster");
			var key = "portable/0.0.0/Text.raster." + architecture + ".png";
			Assert.Equal(key, Resolve(new[] { key }, request).Key);
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", new Version(0, 0, 0), "Text", null, architecture));
		}

		[Theory]
		[InlineData("Upper")]
		[InlineData("bad/name")]
		[InlineData("bad\\name")]
		[InlineData("trailing-")]
		[InlineData("con")]
		public void ProfileAndVariantMustRemainSafeLowercase(string value)
		{
			Assert.Throws<ArgumentException>(() => new GoldenRequest(value, new Version(0, 0, 0), "Text"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", new Version(0, 0, 0), "Text", null, value));
		}

		[Theory]
		[InlineData("amd64")]
		[InlineData("ARM64")]
		[InlineData("custom-arch")]
		public void UnknownArchitecturesAreRejected(string architecture) =>
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", new Version(0, 0, 0), "Text", architecture));

		[Theory]
		[InlineData("mac/base/Text.png")]
		[InlineData("mac/latest/Text.png")]
		[InlineData("mac/12.0.0.r1/Text.png")]
		[InlineData("mac/12.0/Text.png")]
		[InlineData("mac/12.00.0/Text.png")]
		[InlineData("mac/2147483648.0.0/Text.png")]
		[InlineData("mac/1.0.0/CON/Text.png")]
		[InlineData("mac/1.0.0/Text.x64.raster.png")]
		[InlineData("mac/1.0.0/Text.raster.unknown.png")]
		[InlineData("mac/1.0.0/Text.x64.arm64.png")]
		[InlineData("mac/1.0.0/Text.PNG")]
		public void MalformedStoredKeysAreRejected(string key) =>
			Assert.Throws<ArgumentException>(() => Resolve(new[] { key }, new GoldenRequest("mac", new Version(1, 0, 0), "Text")));

		[Fact]
		public void OrderedSourcesWinAndBytesAreReadFresh()
		{
			var first = Path.Combine(root, "first");
			var second = Path.Combine(root, "second");
			const string key = "portable/0.0.0/Group/Text.png";
			foreach (var directory in new[] { first, second })
			{
				var path = Path.Combine(directory, key);
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				File.WriteAllBytes(path, new byte[] { directory == first ? (byte)1 : (byte)2 });
			}
			var request = new GoldenRequest("portable", new Version(0, 0, 0), "Group/Text");
			var result = new GoldenStore(new[] { first, second, first }).Resolve(request);
			Assert.Equal(Path.Combine(first, key), result.Location);
			Assert.Equal(new byte[] { 1 }, result.ReadBytes());
			File.WriteAllBytes(result.Location!, new byte[] { 3 });
			Assert.Equal(new byte[] { 3 }, result.ReadBytes());
			Assert.Equal(new byte[] { 2 }, new GoldenStore(new[] { second, first }).Resolve(request).ReadBytes());
		}

		[Fact]
		public void DiagnosticsUseCanonicalCaptureRequestRatherThanSelectedBaseline()
		{
			var request = new GoldenRequest("macos", new Version(12, 6), "Maui/home", "arm64", "raster");
			var result = Resolve(new[] { "portable/0.0.0/Maui/home.raster.png" }, request);
			Assert.Equal("portable/0.0.0/Maui/home.raster.png", result.Key);
			Assert.Equal("Samples/mac/12.6.0/Maui/home.raster.arm64.png", GoldenDiagnostics.CaptureKey("Samples", request));
			Assert.Equal("##SKIA-GOLDEN## role=actual key=Visual/mac/12.6.0/Maui/home.raster.arm64.png size=1x2 base64=AQID",
				GoldenDiagnostics.Marker("Visual", "actual", request, 1, 2, new byte[] { 1, 2, 3 }));
		}

		public void Dispose()
		{
			if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
		}
	}
}
