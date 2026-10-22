#nullable enable
using System;
using System.IO;
using System.Linq;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GoldenReferenceTests
	{
		[Fact]
		public void ExactThenMinorThenFamilyBaselinesBeatOlderCaptures()
		{
			var request = new GoldenRequest("mac", "12.6.2", "sample", "arm64");
			var keys = new[]
			{
				"mac/12.6.2/sample.png", "mac/12.6/sample.png", "mac/12/sample.png",
				"mac/12.6.1/sample.arm64.png", "mac/base/sample.arm64.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(keys[i], GoldenReference.Resolve(keys.Skip(i), request).Key);
		}

		[Fact]
		public void MissingLookupExplainsTheOrderedChain()
		{
			var request = new GoldenRequest("mac", "12.6.2", "sample", "arm64");
			var result = GoldenReference.Resolve(Array.Empty<string>(), request);
			Assert.Same(request, result.Request);
			Assert.Null(result.Key);
			Assert.Null(result.Path);
			Assert.Equal(new[]
			{
				"mac/12.6.2/sample.arm64.png", "mac/12.6.2/sample.png",
				"mac/12.6/sample.arm64.png", "mac/12.6/sample.png",
				"mac/12/sample.arm64.png", "mac/12/sample.png",
				"mac/base/sample.arm64.png", "mac/base/sample.png",
			}, result.Candidates);
			Assert.Contains("MISSING", result.Describe());
			Assert.Contains(string.Join(Environment.NewLine + "  ", result.Candidates), result.Describe());
		}

		[Fact]
		public void OlderCapturesUseNumericOrderAndOnlyMatchingFiles()
		{
			var result = GoldenReference.Resolve(new[]
			{
				"ios/26.9/sample.png", "ios/26.10/sample.png", "ios/26.11/other.png",
				"ios/26.11/sample.x64.png", "ios/26.11/sample.ganesh-gl.png",
			}, new GoldenRequest("ios", "26.12", "sample", "arm64"));
			Assert.Equal("ios/26.10/sample.png", result.Key);
			Assert.DoesNotContain(result.Candidates, k => k.Contains("/26.11/"));
		}

		[Theory]
		[InlineData("12.6.2", "12.6", "12.6.0", "12.6")]
		[InlineData("12.6.0", "12.6", "12.6.0", "12.6.0")]
		[InlineData("12.6", "12.6.0", "base", "base")]
		[InlineData("12.7", "12.6", "12.6.0", "12.6.0")]
		public void SharedMinorAndExactZeroPatchKeepTheirPrecision(string runtime, string first, string second, string expected)
		{
			var keys = new[] { "mac/" + first + "/sample.png", "mac/" + second + "/sample.png" };
			var request = new GoldenRequest("mac", runtime, "sample", null);
			Assert.Equal("mac/" + expected + "/sample.png", GoldenReference.Resolve(keys, request).Key);
			Assert.Equal(GoldenReference.Resolve(keys, request).Key, GoldenReference.Resolve(Enumerable.Reverse(keys), request).Key);
		}

		[Theory]
		[InlineData("ios", "26.3.2", "26.3.3", (int)GoldenVersionScope.Major)]
		[InlineData("ios", "26.3.2", "25.9", (int)GoldenVersionScope.Major)]
		[InlineData("ubuntu", "22.04.5", "22.10", (int)GoldenVersionScope.Minor)]
		[InlineData("ubuntu", "22.10.2", "22.04.5", (int)GoldenVersionScope.Minor)]
		[InlineData("alpine", "3.23.3", "3.22.9", (int)GoldenVersionScope.Minor)]
		[InlineData("azurelinux", "3.0.7", "3.1", (int)GoldenVersionScope.Minor)]
		[InlineData("mac", "10.15.7", "10.14.9", (int)GoldenVersionScope.Minor)]
		[InlineData("windows-client", "26100.2033", "22621.9999", (int)GoldenVersionScope.Major)]
		public void FutureAndCrossFamilyCapturesAreNeverAttempted(string profile, string runtime, string forbidden, int scope)
		{
			var result = GoldenReference.Resolve(new[] { profile + "/" + forbidden + "/sample.png" },
				new GoldenRequest(profile, runtime, "sample", null, versionScope: (GoldenVersionScope)scope));
			Assert.Null(result.Key);
			Assert.DoesNotContain(profile + "/" + forbidden + "/sample.png", result.Candidates);
		}

		[Fact]
		public void MinorScopePreservesLeadingZeroMinorAndNeverFallsBackToMajor()
		{
			var request = new GoldenRequest("ubuntu", "22.04.5", "sample", null, versionScope: GoldenVersionScope.Minor);
			var keys = new[] { "ubuntu/22.04/sample.png", "ubuntu/22.04.4/sample.png", "ubuntu/22/sample.png" };
			Assert.Equal("ubuntu/22.04/sample.png", GoldenReference.Resolve(keys, request).Key);
			Assert.Equal("ubuntu/22.04.4/sample.png", GoldenReference.Resolve(keys.Skip(1), request).Key);
			Assert.Null(GoldenReference.Resolve(keys.Skip(2), request).Key);
		}

		[Theory]
		[InlineData("36.r8", "36.r7", "36.r6", "36.r7")]
		[InlineData("36.1.r8", "36.1.r7", "36.1.r6", "36.1.r7")]
		[InlineData("36.1.r8", "36.1.r7", "36.1", "36.1")]
		[InlineData("36.1.r8", "36.2", "36.1.r9", null)]
		[InlineData("36.r8", "36.1", "36.r9", null)]
		public void ImageRevisionsFollowNumericPlatformComponents(string runtime, string first, string second, string? expected)
		{
			var result = GoldenReference.Resolve(new[] { "device/" + first + "/sample.png", "device/" + second + "/sample.png" },
				new GoldenRequest("device", runtime, "sample", null));
			Assert.Equal(expected == null ? null : "device/" + expected + "/sample.png", result.Key);
		}

		[Fact]
		public void CallerChoosesUnversionedScopeWithoutLosingRuntimeMetadata()
		{
			var request = new GoldenRequest("custom-profile", "130.0.1.2", "custom-case", null,
				versionScope: GoldenVersionScope.Unversioned);
			var result = GoldenReference.Resolve(new[] { "custom-profile/130.0.1.2/custom-case.png", "custom-profile/base/custom-case.png" }, request);
			Assert.Equal("custom-profile/base/custom-case.png", result.Key);
			Assert.Equal("130.0.1.2", result.Request.Version);
			Assert.Single(result.Candidates);
		}

		[Fact]
		public void ProfileFallbacksAreExplicitOrderedBaseOnlyAndFollowTheWholePrimaryProfile()
		{
			var keys = new[] { "device/base/sample.png", "os/26/sample.png", "os/base/sample.png", "portable/base/sample.png" };
			var request = new GoldenRequest("device", "26.3", "sample", null, fallbackProfiles: new[] { "os", "portable" });
			Assert.Equal(keys[0], GoldenReference.Resolve(keys, request).Key);
			Assert.Equal(keys[2], GoldenReference.Resolve(keys.Skip(1), request).Key);
			Assert.Equal(keys[3], GoldenReference.Resolve(new[] { keys[1], keys[3] }, request).Key);
			Assert.Null(GoldenReference.Resolve(keys.Skip(1), new GoldenRequest("device", "26.3", "sample", null)).Key);
			Assert.Null(GoldenReference.Resolve(new[] { keys[1] }, request).Key);
		}

		[Fact]
		public void VariantThenArchitectureOrderNeverBorrowsSiblingBackendsOrArchitectures()
		{
			var request = new GoldenRequest("custom", "base", "visual-text", "custom-arch", "ganesh-gl", allowNeutralVariant: true);
			var ordered = new[]
			{
				"custom/base/visual-text.ganesh-gl.custom-arch.png", "custom/base/visual-text.ganesh-gl.png",
				"custom/base/visual-text.custom-arch.png", "custom/base/visual-text.png",
			};
			for (var i = 0; i < ordered.Length; i++)
				Assert.Equal(ordered[i], GoldenReference.Resolve(ordered.Skip(i), request).Key);
			var missing = GoldenReference.Resolve(new[]
			{
				"custom/base/visual-text.ganesh-metal.custom-arch.png", "custom/base/visual-text.ganesh-gl.x64.png",
			}, request);
			Assert.Null(missing.Key);
			Assert.Equal(ordered, missing.Candidates);
			Assert.Null(GoldenReference.Resolve(ordered.Skip(2),
				new GoldenRequest("custom", "base", "visual-text", "custom-arch", "ganesh-gl")).Key);
		}

		[Fact]
		public void MoreSpecificVersionBeatsVariantSpecificityAtOlderVersions()
		{
			var result = GoldenReference.Resolve(new[] { "device/26.3.2/sample.png", "device/26.3.1/sample.engine.arm64.png" },
				new GoldenRequest("device", "26.3.2", "sample", "arm64", "engine", allowNeutralVariant: true));
			Assert.Equal("device/26.3.2/sample.png", result.Key);
			Assert.Null(result.Path);
			Assert.Contains("Selected: " + result.Key, result.Describe());
		}

		[Theory]
		[InlineData("../mac/base/sample.png")]
		[InlineData("/mac/base/sample.png")]
		[InlineData("mac\\base\\sample.png")]
		[InlineData("mac/../sample.png")]
		[InlineData("mac/base/../sample.png")]
		[InlineData("mac/latest/sample.png")]
		[InlineData("mac/*/sample.png")]
		[InlineData("mac/base/Sample.png")]
		[InlineData("mac/base/con.png")]
		[InlineData("mac/base/sample.aux.png")]
		[InlineData("mac/base/sample..png")]
		[InlineData("mac/base/sample.a.b.c.png")]
		[InlineData("mac/base/sample.PNG")]
		public void MalformedPngKeysAreRejected(string key) =>
			Assert.Throws<ArgumentException>(() => GoldenReference.Resolve(new[] { key }, new GoldenRequest("mac", "base", "sample", null)));

		[Theory]
		[InlineData("")]
		[InlineData("../escape")]
		[InlineData("bad/name")]
		[InlineData("bad\\name")]
		[InlineData("Upper")]
		[InlineData("trailing-")]
		[InlineData("nul")]
		[InlineData("com1")]
		public void RequestSegmentsMustBeSafe(string value)
		{
			Assert.Throws<ArgumentException>(() => new GoldenRequest(value, "base", "sample", null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", value, null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "sample", value));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "sample", null, value));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "sample", null, fallbackProfiles: new[] { value }));
		}

		[Theory]
		[InlineData("")]
		[InlineData("latest")]
		[InlineData("26.x")]
		[InlineData("0")]
		[InlineData("26..1")]
		[InlineData("36.r0")]
		[InlineData("2147483648")]
		public void RequestsRequireActualNumericVersionOrBase(string version) =>
			Assert.Throws<ArgumentException>(() => new GoldenRequest("custom", version, "sample", null));

		[Fact]
		public void DirectoryLookupUsesScopedRelativeNamesAndDoesNotReadImageContents()
		{
			var root = Path.Combine(Path.GetTempPath(), "golden-reference-tests-" + Guid.NewGuid().ToString("N"));
			const string key = "custom/base/sample.png";
			var path = Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar));
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path)!);
				File.WriteAllText(path, "not image data");
				File.WriteAllText(Path.Combine(root, "ignored.txt"), "not a golden");
				Assert.Equal(new[] { key }, GoldenReference.Keys(root));
				var request = new GoldenRequest("custom", "base", "sample", null);
				var result = GoldenReference.Resolve(root, request);
				Assert.Equal(key, result.Key);
				Assert.Equal(Path.GetFullPath(path), result.Path);
				Assert.Contains(result.Path!, result.Describe());
				Assert.Equal(key, GoldenReference.Resolve(new[] { "README.md", "ignored/sample.jpg", key }, request).Key);
				var missing = GoldenReference.Resolve(root, new GoldenRequest("custom", "base", "missing", null));
				Assert.Null(missing.Key);
				Assert.Null(missing.Path);
				Assert.Equal(2, Directory.GetFiles(root, "*", SearchOption.AllDirectories).Length);
				Assert.Throws<DirectoryNotFoundException>(() => GoldenReference.Resolve(Path.Combine(root, "absent"), request));
				Assert.False(Directory.Exists(Path.Combine(root, "absent")));
			}
			finally
			{
				if (Directory.Exists(root)) Directory.Delete(root, true);
			}
		}

		[Theory]
		[InlineData("DiagonalLines", "visual-diagonal-lines")]
		[InlineData("GPUReadback", "visual-gpu-readback")]
		[InlineData("RGBAGradient", "visual-rgba-gradient")]
		[InlineData("visual-text", "visual-text")]
		public void ExistingVisualCaseNamesRemainStable(string scene, string expected) =>
			Assert.Equal(expected, GoldenNames.Case(scene));

		[Theory]
		[InlineData("Client", "windows-client")]
		[InlineData("Server", "windows-server")]
		[InlineData("Server Core", "windows-servercore")]
		[InlineData("Nano Server", "windows-nanoserver")]
		public void WindowsInstallationKindsRemainDistinct(string kind, string profile) =>
			Assert.Equal(profile, GoldenEnvironment.WindowsProfile(kind));

		[Theory]
		[InlineData("")]
		[InlineData("Windows PE")]
		[InlineData("Unrecognized")]
		public void UnknownWindowsInstallationKindsNeverDefaultToServer(string kind) =>
			Assert.Throws<InvalidOperationException>(() => GoldenEnvironment.WindowsProfile(kind));
	}
}
