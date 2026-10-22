#nullable enable
using System;
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
			var request = new GoldenRequest("mac", "12.6.2", "visual-text", "arm64");
			var keys = new[]
			{
				"mac/12.6.2/visual-text.png", "mac/12.6/visual-text.png", "mac/12/visual-text.png",
				"mac/12.6.1/visual-text.arm64.png", "mac/base/visual-text.arm64.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(keys[i], GoldenReference.Resolve(keys.Skip(i), request).Key);
		}

		[Fact]
		public void MissingLookupExplainsTheOrderedChain()
		{
			var request = new GoldenRequest("mac", "12.6.2", "visual-text", "arm64");
			var result = GoldenReference.Resolve(Array.Empty<string>(), request);
			Assert.Same(request, result.Request);
			Assert.Null(result.Key);
			Assert.Null(result.Path);
			Assert.Equal(new[]
			{
				"mac/12.6.2/visual-text.arm64.png", "mac/12.6.2/visual-text.png",
				"mac/12.6/visual-text.arm64.png", "mac/12.6/visual-text.png",
				"mac/12/visual-text.arm64.png", "mac/12/visual-text.png",
				"mac/base/visual-text.arm64.png", "mac/base/visual-text.png",
			}, result.Candidates);
			Assert.Equal("Golden request: mac/12.6.2/visual-text" + Environment.NewLine +
				"MISSING reference" + Environment.NewLine + "Attempted (in order):" + Environment.NewLine + "  " +
				string.Join(Environment.NewLine + "  ", result.Candidates), result.Describe());
		}

		[Fact]
		public void OlderCapturesUseNumericOrderAndOnlyMatchingFiles()
		{
			var request = new GoldenRequest("mac", "12.12", "visual-text", "arm64");
			var keys = new[]
			{
				"mac/12.9/visual-text.png", "mac/12.10/visual-text.png", "mac/12.11/visual-lines.png",
				"mac/12.11/visual-text.x64.png", "mac/12.11/visual-text.ganesh-gl.png",
			};
			var result = GoldenReference.Resolve(keys, request);
			Assert.Equal(keys[1], result.Key);
			Assert.DoesNotContain(result.Candidates, k => k.Contains("/12.11/"));
			Assert.Equal(keys[0], GoldenReference.Resolve(keys.Where(k => k != keys[1]), request).Key);
		}

		[Theory]
		[InlineData("12.6.2", "12.6", "12.6.0", "12.6")]
		[InlineData("12.6.0", "12.6", "12.6.0", "12.6.0")]
		[InlineData("12.6", "12.6.0", "base", "base")]
		[InlineData("12.7", "12.6", "12.6.0", "12.6.0")]
		public void SharedMinorAndExactZeroPatchKeepTheirPrecision(string runtime, string first, string second, string expected)
		{
			var keys = new[] { "mac/" + first + "/visual-text.png", "mac/" + second + "/visual-text.png" };
			var request = new GoldenRequest("mac", runtime, "visual-text", null);
			Assert.Equal("mac/" + expected + "/visual-text.png", GoldenReference.Resolve(keys, request).Key);
			Assert.Equal(GoldenReference.Resolve(keys, request).Candidates,
				GoldenReference.Resolve(Enumerable.Reverse(keys), request).Candidates);
		}

		[Theory]
		[InlineData("mac", "12.6.2", "12.6.3", (int)GoldenVersionScope.Major)]
		[InlineData("mac", "12.6.2", "11.9", (int)GoldenVersionScope.Major)]
		[InlineData("ubuntu", "22.04.5", "22.10", (int)GoldenVersionScope.Minor)]
		[InlineData("ubuntu", "22.10.2", "22.04.5", (int)GoldenVersionScope.Minor)]
		[InlineData("mac", "10.15.7", "10.14.9", (int)GoldenVersionScope.Minor)]
		[InlineData("windows", "26100.2033", "22621.9999", (int)GoldenVersionScope.Major)]
		public void FutureAndCrossFamilyCapturesAreNeverAttempted(string profile, string runtime, string forbidden, int scope)
		{
			var key = profile + "/" + forbidden + "/visual-text.png";
			var result = GoldenReference.Resolve(new[] { key },
				new GoldenRequest(profile, runtime, "visual-text", null, versionScope: (GoldenVersionScope)scope));
			Assert.Null(result.Key);
			Assert.DoesNotContain(key, result.Candidates);
		}

		[Fact]
		public void MajorScopeAllowsOlderMinorsButMinorScopeDoesNot()
		{
			const string key = "mac/12.5.9/visual-text.png";
			Assert.Equal(key, GoldenReference.Resolve(new[] { key },
				new GoldenRequest("mac", "12.6.2", "visual-text", null)).Key);
			Assert.Null(GoldenReference.Resolve(new[] { key },
				new GoldenRequest("mac", "12.6.2", "visual-text", null, versionScope: GoldenVersionScope.Minor)).Key);
		}

		[Fact]
		public void MinorScopePreservesLeadingZeroMinorAndNeverFallsBackToMajor()
		{
			var request = new GoldenRequest("ubuntu", "22.04.5", "visual-text", null, versionScope: GoldenVersionScope.Minor);
			var keys = new[] { "ubuntu/22.04/visual-text.png", "ubuntu/22.04.4/visual-text.png", "ubuntu/22/visual-text.png" };
			Assert.Equal(keys[0], GoldenReference.Resolve(keys, request).Key);
			Assert.Equal(keys[1], GoldenReference.Resolve(keys.Skip(1), request).Key);
			var missing = GoldenReference.Resolve(keys.Skip(2), request);
			Assert.Null(missing.Key);
			Assert.DoesNotContain(keys[2], missing.Candidates);
		}

		[Theory]
		[InlineData("12.r8", "12.r7", "12.r6", "12.r7")]
		[InlineData("12.1.r12", "12.1.r9", "12.1.r10", "12.1.r10")]
		[InlineData("12.1.r8", "12.1.r7", "12.1", "12.1")]
		[InlineData("12.1.r8", "12.2", "12.1.r9", null)]
		[InlineData("12.r8", "12.1", "12.r9", null)]
		[InlineData("12.1", "12.1.r1", "12.1.r2", null)]
		public void ImageRevisionsFollowNumericPlatformComponents(string runtime, string first, string second, string? expected)
		{
			var result = GoldenReference.Resolve(new[] { "generic/" + first + "/visual-text.png", "generic/" + second + "/visual-text.png" },
				new GoldenRequest("generic", runtime, "visual-text", null));
			Assert.Equal(expected == null ? null : "generic/" + expected + "/visual-text.png", result.Key);
		}

		[Fact]
		public void ExactRevisionPrecedesUnrevisedAndSharedBaselines()
		{
			var request = new GoldenRequest("mac", "12.6.2.r3", "visual-text", null);
			var keys = new[]
			{
				"mac/12.6.2.r3/visual-text.png", "mac/12.6.2/visual-text.png",
				"mac/12.6/visual-text.png", "mac/12/visual-text.png",
				"mac/12.6.2.r2/visual-text.png", "mac/base/visual-text.png",
			};
			for (var i = 0; i < keys.Length; i++)
				Assert.Equal(keys[i], GoldenReference.Resolve(keys.Skip(i), request).Key);
		}

		[Fact]
		public void CallerChoosesUnversionedScopeWithoutLosingRuntimeMetadata()
		{
			var request = new GoldenRequest("generic", "130.0.1.2", "visual-text", null,
				versionScope: GoldenVersionScope.Unversioned);
			var result = GoldenReference.Resolve(new[] { "generic/130.0.1.2/visual-text.png", "generic/base/visual-text.png" }, request);
			Assert.Equal("generic/base/visual-text.png", result.Key);
			Assert.Equal("130.0.1.2", result.Request.Version);
			Assert.Single(result.Candidates);
		}

		[Fact]
		public void ProfileFallbacksAreExplicitOrderedBaseOnlyAndFollowTheWholePrimaryProfile()
		{
			var keys = new[] { "ubuntu/base/visual-text.png", "linux/22/visual-text.png", "linux/base/visual-text.png", "generic/base/visual-text.png" };
			var request = new GoldenRequest("ubuntu", "22.04", "visual-text", null, fallbackProfiles: new[] { "linux", "generic" });
			Assert.Equal(keys[0], GoldenReference.Resolve(keys, request).Key);
			Assert.Equal(keys[2], GoldenReference.Resolve(keys.Skip(1), request).Key);
			Assert.Equal(keys[3], GoldenReference.Resolve(new[] { keys[1], keys[3] }, request).Key);
			Assert.Null(GoldenReference.Resolve(keys.Skip(1), new GoldenRequest("ubuntu", "22.04", "visual-text", null)).Key);
			var missing = GoldenReference.Resolve(new[] { keys[1] }, request);
			Assert.Null(missing.Key);
			Assert.Equal(new[]
			{
				"ubuntu/22.04/visual-text.png", "ubuntu/22/visual-text.png", "ubuntu/base/visual-text.png",
				"linux/base/visual-text.png", "generic/base/visual-text.png",
			}, missing.Candidates);
		}

		[Fact]
		public void VariantThenArchitectureOrderNeverBorrowsSiblingBackendsOrArchitectures()
		{
			var request = new GoldenRequest("generic", "base", "visual-text", "arm64", "ganesh-gl", allowNeutralVariant: true);
			var ordered = new[]
			{
				"generic/base/visual-text.ganesh-gl.arm64.png", "generic/base/visual-text.ganesh-gl.png",
				"generic/base/visual-text.arm64.png", "generic/base/visual-text.png",
			};
			for (var i = 0; i < ordered.Length; i++)
			{
				var result = GoldenReference.Resolve(ordered.Skip(i), request);
				Assert.Equal(ordered[i], result.Key);
				Assert.Equal(ordered.Take(i + 1), result.Candidates);
			}
			var missing = GoldenReference.Resolve(new[]
			{
				"generic/base/visual-text.ganesh-metal.arm64.png", "generic/base/visual-text.ganesh-gl.x64.png",
			}, request);
			Assert.Null(missing.Key);
			Assert.Equal(ordered, missing.Candidates);
			var strict = GoldenReference.Resolve(ordered.Skip(2),
				new GoldenRequest("generic", "base", "visual-text", "arm64", "ganesh-gl"));
			Assert.Null(strict.Key);
			Assert.Equal(ordered.Take(2), strict.Candidates);
		}

		[Fact]
		public void NeutralRequestDoesNotBorrowNamedVariantsOrArchitectures()
		{
			var result = GoldenReference.Resolve(new[]
			{
				"generic/base/visual-text.ganesh-gl.png", "generic/base/visual-text.arm64.png",
			}, new GoldenRequest("generic", "base", "visual-text", null, allowNeutralVariant: true));
			Assert.Null(result.Key);
			Assert.Equal(new[] { "generic/base/visual-text.png" }, result.Candidates);
		}

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
		public void SupportedArchitecturesAreReservedSuffixesNotVariants(string architecture)
		{
			Assert.True(GoldenReference.IsArchitecture(architecture));
			var neutralKey = "generic/base/visual-text." + architecture + ".png";
			var variantKey = "generic/base/visual-text.ganesh-gl." + architecture + ".png";
			Assert.Equal(neutralKey, GoldenReference.Resolve(new[] { neutralKey },
				new GoldenRequest("generic", "base", "visual-text", architecture)).Key);
			Assert.Equal(variantKey, GoldenReference.Resolve(new[] { variantKey },
				new GoldenRequest("generic", "base", "visual-text", architecture, "ganesh-gl")).Key);
			Assert.Throws<ArgumentException>(() => new GoldenRequest("generic", "base", "visual-text", null, architecture));
		}

		[Fact]
		public void ArchitectureSuffixCannotMasqueradeAsAStrictVariant()
		{
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "visual-text", "arm64", "x64"));
			var result = GoldenReference.Resolve(new[] { "mac/base/visual-text.x64.png" },
				new GoldenRequest("mac", "base", "visual-text", "arm64", "ganesh-gl"));
			Assert.Null(result.Key);
			Assert.Equal(new[]
			{
				"mac/base/visual-text.ganesh-gl.arm64.png", "mac/base/visual-text.ganesh-gl.png",
			}, result.Candidates);
		}

		[Theory]
		[InlineData("custom-arch")]
		[InlineData("amd64")]
		[InlineData("ARM64")]
		[InlineData("arm64-extra")]
		public void UnknownRequestArchitecturesAreRejected(string architecture)
		{
			Assert.False(GoldenReference.IsArchitecture(architecture));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("generic", "base", "visual-text", architecture));
		}

		[Theory]
		[InlineData("visual-text.arm64.ganesh-gl.png")]
		[InlineData("visual-text.ganesh-gl.custom-arch.png")]
		[InlineData("visual-text.ganesh-gl.amd64.png")]
		[InlineData("visual-text.arm64.arm64.png")]
		[InlineData("visual-text.x64.arm64.png")]
		[InlineData("visual-text.arm64.x64.png")]
		public void AmbiguousOrUnknownArchitectureSuffixesAreRejected(string fileName) =>
			Assert.Throws<ArgumentException>(() => GoldenReference.Resolve(new[] { "generic/base/" + fileName },
				new GoldenRequest("generic", "base", "visual-text", "arm64")));

		[Fact]
		public void MoreSpecificVersionBeatsVariantSpecificityAtOlderVersions()
		{
			var result = GoldenReference.Resolve(new[] { "mac/12.6.2/visual-text.png", "mac/12.6.1/visual-text.ganesh-gl.arm64.png" },
				new GoldenRequest("mac", "12.6.2", "visual-text", "arm64", "ganesh-gl", allowNeutralVariant: true));
			Assert.Equal("mac/12.6.2/visual-text.png", result.Key);
			Assert.Null(result.Path);
			Assert.Contains("Selected: " + result.Key, result.Describe());
		}

		[Fact]
		public void ConsumersCanAttachPathsWithoutChangingTheResolution()
		{
			var request = new GoldenRequest("generic", "base", "visual-text", null);
			var selected = GoldenReference.Resolve(new[] { "generic/base/visual-text.png" }, request);
			var result = new GoldenResolution(selected.Request, selected.Key, "consumer-resource-path", selected.Candidates);
			Assert.Same(request, result.Request);
			Assert.Equal(selected.Key, result.Key);
			Assert.Equal(selected.Candidates, result.Candidates);
			Assert.Equal("consumer-resource-path", result.Path);
			Assert.Contains("Selected: " + selected.Key + " at consumer-resource-path", result.Describe());
		}

		[Fact]
		public void EnumerationOrderAndDuplicatesDoNotChangeTheTrace()
		{
			var keys = new[]
			{
				"mac/12.06.1/visual-text.png", "mac/12.6.1/visual-text.png",
				"mac/12.5/visual-text.png", "linux/12.6/visual-text.png",
			};
			var request = new GoldenRequest("mac", "12.7", "visual-text", "arm64",
				fallbackProfiles: new[] { "mac", "linux", "linux" });
			var result = GoldenReference.Resolve(keys, request);
			var reordered = GoldenReference.Resolve(keys.Reverse().Concat(keys), request);
			Assert.Equal("mac/12.06.1/visual-text.png", result.Key);
			Assert.Equal(result.Key, reordered.Key);
			Assert.Equal(result.Candidates, reordered.Candidates);
			Assert.Equal(result.Describe(), reordered.Describe());
			var missing = GoldenReference.Resolve(Array.Empty<string>(), request);
			Assert.Equal(missing.Candidates.Count, missing.Candidates.Distinct(StringComparer.Ordinal).Count());
			Assert.Equal(new[] { "linux/base/visual-text.arm64.png", "linux/base/visual-text.png" },
				missing.Candidates.Skip(missing.Candidates.Count - 2));
		}

		[Fact]
		public void CallerOwnedCollectionsAreSnapshotted()
		{
			var profiles = new[] { "linux" };
			var request = new GoldenRequest("ubuntu", "base", "visual-text", null, fallbackProfiles: profiles);
			profiles[0] = "windows";
			Assert.Equal(new[] { "linux" }, request.FallbackProfiles);
			var candidates = new[] { "ubuntu/base/visual-text.png" };
			var resolution = new GoldenResolution(request, null, null, candidates);
			candidates[0] = "changed";
			Assert.Equal(new[] { "ubuntu/base/visual-text.png" }, resolution.Candidates);
		}

		[Theory]
		[InlineData("../mac/base/visual-text.png")]
		[InlineData("/mac/base/visual-text.png")]
		[InlineData("mac\\base\\visual-text.png")]
		[InlineData("mac/../visual-text.png")]
		[InlineData("mac/base/../visual-text.png")]
		[InlineData("mac/latest/visual-text.png")]
		[InlineData("mac/*/visual-text.png")]
		[InlineData("mac/base/Visual-text.png")]
		[InlineData("mac/base/con.png")]
		[InlineData("mac/base/visual-text.aux.png")]
		[InlineData("mac/base/visual-text..png")]
		[InlineData("mac/base/visual-text.a.b.c.png")]
		[InlineData("mac/base/visual-text.PNG")]
		public void MalformedPngKeysAreRejected(string key) =>
			Assert.Throws<ArgumentException>(() => GoldenReference.Resolve(new[] { key },
				new GoldenRequest("mac", "base", "visual-text", null)));

		[Theory]
		[InlineData("")]
		[InlineData("../escape")]
		[InlineData("bad/name")]
		[InlineData("bad\\name")]
		[InlineData("Upper")]
		[InlineData("trailing-")]
		[InlineData("nul")]
		[InlineData("com1")]
		[InlineData("bad\n")]
		public void RequestSegmentsMustBeSafe(string value)
		{
			Assert.Throws<ArgumentException>(() => new GoldenRequest(value, "base", "visual-text", null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", value, null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "visual-text", value));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "visual-text", null, value));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "visual-text", null, fallbackProfiles: new[] { value }));
		}

		[Theory]
		[InlineData("")]
		[InlineData("latest")]
		[InlineData("12.x")]
		[InlineData("0")]
		[InlineData("12..1")]
		[InlineData("12.r0")]
		[InlineData("12.r1.2")]
		[InlineData("2147483648")]
		[InlineData("12.2147483648")]
		[InlineData("12.r2147483648")]
		[InlineData("12\n")]
		public void RequestsRequireActualNumericVersionOrBase(string version) =>
			Assert.Throws<ArgumentException>(() => new GoldenRequest("generic", version, "visual-text", null));

		[Fact]
		public void NullInputsAndInvalidScopesAreRejected()
		{
			var request = new GoldenRequest("generic", "base", "visual-text", null);
			Assert.Throws<ArgumentNullException>(() => GoldenReference.Resolve(null!, request));
			Assert.Throws<ArgumentNullException>(() => GoldenReference.Resolve(Array.Empty<string>(), null!));
			Assert.Throws<ArgumentException>(() => GoldenReference.Resolve(new[] { (string)null! }, request));
			Assert.Throws<ArgumentException>(() => new GoldenRequest(null!, "base", "visual-text", null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("generic", null!, "visual-text", null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("generic", "base", null!, null));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("generic", "base", "visual-text", null,
				fallbackProfiles: new[] { (string)null! }));
			Assert.Throws<ArgumentException>(() => GoldenNames.Case(null!));
			Assert.Throws<ArgumentOutOfRangeException>(() => new GoldenRequest("generic", "base", "visual-text", null,
				versionScope: (GoldenVersionScope)(-1)));
			Assert.Throws<ArgumentOutOfRangeException>(() => new GoldenRequest("generic", "base", "visual-text", null,
				versionScope: (GoldenVersionScope)3));
		}

		[Fact]
		public void NonPngEntriesAreIgnored()
		{
			const string key = "generic/base/visual-text.png";
			var result = GoldenReference.Resolve(new[] { "README.md", "ignored/visual-text.jpg", key },
				new GoldenRequest("generic", "base", "visual-text", null));
			Assert.Equal(key, result.Key);
		}

		[Theory]
		[InlineData("DiagonalLines", "visual-diagonal-lines")]
		[InlineData("GPUReadback", "visual-gpu-readback")]
		[InlineData("RGBAGradient", "visual-rgba-gradient")]
		[InlineData("Text2D", "visual-text2-d")]
		[InlineData("VisualText", "visual-text")]
		[InlineData("visual-text", "visual-text")]
		public void ExistingVisualCaseNamesRemainStable(string scene, string expected) =>
			Assert.Equal(expected, GoldenNames.Case(scene));

		[Theory]
		[InlineData("")]
		[InlineData("../Text")]
		[InlineData("Text/Lines")]
		[InlineData("Text Lines")]
		[InlineData("Text.png")]
		public void UnsafeVisualCaseNamesAreRejected(string scene) =>
			Assert.Throws<ArgumentException>(() => GoldenNames.Case(scene));
	}
}
