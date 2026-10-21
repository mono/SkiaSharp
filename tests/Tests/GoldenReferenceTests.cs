using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests
{
	public class GoldenReferenceTests
	{
		[Fact]
		public void SharedPathConformanceFixtureAgreesWithCanonicalGrammar()
		{
			using (var stream = typeof(GoldenReferenceTests).Assembly.GetManifestResourceStream("SkiaSharp.GoldenPathConformance"))
			{
				Assert.NotNull(stream);
				using (var reader = new StreamReader(stream))
				{
					var lineNumber = 0;
					var valid = 0;
					var invalid = 0;
					string line;
					while ((line = reader.ReadLine()) != null)
					{
						lineNumber++;
						var context = "golden-paths.tsv row " + lineNumber + ": " + line;
						var fields = line.Split('\t');
						Assert.True(fields.Length == 2, context + " must contain exactly two tab-separated fields.");
						Assert.True(fields[0] == "valid" || fields[0] == "invalid", context + " has an unknown expectation.");
						var error = Record.Exception(() => GoldenKey.Parse(fields[1]));
						if (fields[0] == "valid")
						{
							valid++;
							Assert.True(error == null, context + " was rejected: " + error);
							Assert.True(GoldenKey.Parse(fields[1]).ToString() == fields[1], context + " did not round-trip canonically.");
						}
						else
						{
							invalid++;
							Assert.True(error is ArgumentException, context + " must be rejected with ArgumentException.");
						}
					}
					Assert.True(valid > 0 && invalid > 0, "The shared fixture must exercise both accepted and rejected paths.");
				}
			}
		}

		[Theory]
		[InlineData("mac/12.6.0/blazor-cpu.x64.png")]
		[InlineData("mac/10.15.7/basic-console.png")]
		[InlineData("mac/base/visual-diagonal-lines.ganesh-gl.arm64.png")]
		[InlineData("portable/base/visual-text.raster.png")]
		[InlineData("browser/base/visual-text.raster.wasm.png")]
		[InlineData("browser/base/browser-wasm-page.firefox.wasm.png")]
		[InlineData("windows/base/basic-console.png")]
		[InlineData("windows-client/26100.2033/basic-console.png")]
		[InlineData("windows-server/26100/basic-console.x64.png")]
		[InlineData("windows-nanoserver/base/basic-console.noto-sans.x64.png")]
		[InlineData("ubuntu/22.04.5/basic-console.noto-sans.png")]
		[InlineData("alpine/3.23.3/basic-console.arm.png")]
		[InlineData("azurelinux/3.0/basic-console.loongarch64.png")]
		[InlineData("debian/12.11/basic-console.riscv64.png")]
		[InlineData("linux/base/basic-console.x86.png")]
		[InlineData("iphone-16-pro/26.3.1/maui-basic-cpu.png")]
		[InlineData("ios/26/visual-text.ganesh-metal.png")]
		[InlineData("maccatalyst/26.3/visual-text.graphite-metal.png")]
		[InlineData("tvos/26/visual-text.ganesh-metal.png")]
		[InlineData("pixel-9/36.1.r8/native-basic-cpu.arm64.png")]
		[InlineData("android/36.r8/visual-text.ganesh-vulkan.png")]
		public void CanonicalKeysRoundTrip(string relative)
		{
			var key = GoldenKey.Parse(relative);
			Assert.Equal(relative, key.ToString());
			Assert.Equal(relative.Split('/')[0], key.Profile);
			Assert.Equal(relative.Split('/')[1], key.Version);
		}

		[Fact]
		public void FunctionalVariantPrecedesArchitecture()
		{
			var key = GoldenKey.Parse("mac/base/visual-diagonal-lines.ganesh-gl.arm64.png");
			Assert.Equal("visual-diagonal-lines", key.CaseName);
			Assert.Equal("ganesh-gl", key.Variant);
			Assert.Equal("arm64", key.Architecture);
			var neutral = GoldenKey.Parse("mac/base/blazor-cpu.png");
			Assert.Null(neutral.Variant);
			Assert.Null(neutral.Architecture);
			Assert.Throws<InvalidOperationException>(() => default(GoldenKey).ToString());
		}

		[Theory]
		[InlineData("")]
		[InlineData("/mac/base/basic-console.png")]
		[InlineData("../mac/base/basic-console.png")]
		[InlineData("mac/../basic-console.png")]
		[InlineData("mac/base/../basic-console.png")]
		[InlineData("mac\\base\\basic-console.png")]
		[InlineData("mac//basic-console.png")]
		[InlineData("mac/base/basic-console.png/extra")]
		[InlineData("host/mac/base/basic-console.png")]
		[InlineData("docker/base/basic-console.png")]
		[InlineData("macos/base/basic-console.png")]
		[InlineData("iPhone-16-Pro/26/maui-basic-cpu.png")]
		[InlineData("pixel-9-play/36/native-basic-cpu.png")]
		[InlineData("Mac/base/basic-console.png")]
		[InlineData("mac/Base/basic-console.png")]
		[InlineData("mac/latest/basic-console.png")]
		[InlineData("mac/12.06/basic-console.png")]
		[InlineData("mac/012.6/basic-console.png")]
		[InlineData("mac/12.6.0.0/basic-console.png")]
		[InlineData("mac/10/basic-console.png")]
		[InlineData("mac/Darwin-21.6.0/basic-console.png")]
		[InlineData("mac/0/basic-console.png")]
		[InlineData("mac/2147483648/basic-console.png")]
		[InlineData("mac/base/BasicConsole.png")]
		[InlineData("mac/base/basic_console.png")]
		[InlineData("mac/base/basic-console\n.png")]
		[InlineData("mac/base/basic-console.PNG")]
		[InlineData("mac/base/basic-console.jpg")]
		[InlineData("mac/base/basic-console..png")]
		[InlineData("mac/base/basic-console.x64.arm64.png")]
		[InlineData("mac/base/basic-console.ARM64.png")]
		[InlineData("mac/base/basic-console.amd64.png")]
		[InlineData("mac/base/basic-console.firefox.png")]
		[InlineData("mac/base/blazor-cpu.firefox.chromium.png")]
		[InlineData("mac/base/blazor-cpu.x64.firefox.png")]
		[InlineData("mac/base/visual-text.png")]
		[InlineData("mac/base/visual-text.firefox.png")]
		[InlineData("mac/base/visual-text.x64.ganesh-gl.png")]
		[InlineData("mac/base/visual-text.ganesh-gl.arm64.extra.png")]
		[InlineData("mac/base/visual-text.ganesh-gl.ganesh-vulkan.png")]
		[InlineData("mac/base/con.png")]
		[InlineData("mac/base/aux.x64.png")]
		[InlineData("mac/base/com1.png")]
		[InlineData("mac/base/lpt9.png")]
		[InlineData("mac/base/x64.png")]
		[InlineData("mac/base/raster.png")]
		[InlineData("mac/base/noto-sans.png")]
		[InlineData("windows/26100/basic-console.png")]
		[InlineData("linux/22.04/basic-console.png")]
		[InlineData("browser/130/basic-console.png")]
		[InlineData("portable/12/visual-text.raster.png")]
		[InlineData("windows-client/10.0.26100.2033/basic-console.png")]
		[InlineData("windows-server/10.0/basic-console.png")]
		[InlineData("windows-nanoserver/ltsc2022/basic-console.png")]
		[InlineData("ubuntu/22/basic-console.png")]
		[InlineData("ubuntu/22.4/basic-console.png")]
		[InlineData("alpine/3/basic-console.png")]
		[InlineData("azurelinux/3/basic-console.png")]
		[InlineData("debian/12.1.2/basic-console.png")]
		[InlineData("pixel-9/android-36/native-basic-cpu.png")]
		[InlineData("pixel-9/16.0.0/native-basic-cpu.png")]
		[InlineData("pixel-9/36.1.8/native-basic-cpu.png")]
		[InlineData("pixel-9/36.1.r08/native-basic-cpu.png")]
		[InlineData("pixel-9/36.r0/native-basic-cpu.png")]
		public void AmbiguousUnsafeOrNoncanonicalKeysAreRejected(string relative) =>
			Assert.Throws<ArgumentException>(() => GoldenKey.Parse(relative));

		[Theory]
		[InlineData("DiagonalLines", "visual-diagonal-lines")]
		[InlineData("RedRoundedRectOnWhite", "visual-red-rounded-rect-on-white")]
		[InlineData("GPUReadback", "visual-gpu-readback")]
		[InlineData("RGBAGradient", "visual-rgba-gradient")]
		[InlineData("XMLParser", "visual-xml-parser")]
		[InlineData("Text", "visual-text")]
		[InlineData("visual-text", "visual-text")]
		public void SceneNamesHaveStableAcronymAwareSemanticKeys(string scene, string expected) =>
			Assert.Equal(expected, GoldenNames.Case(scene));

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("latest")]
		[InlineData("unknown")]
		[InlineData("26.x")]
		public void VersionedRequestsRequireActualMetadata(string version) =>
			Assert.Throws<ArgumentException>(() => new GoldenRequest("iphone-16-pro", version, "maui-basic-cpu", "arm64"));

		[Fact]
		public void RequestsRejectAliasesAndInvalidBarriers()
		{
			Assert.Throws<ArgumentException>(() => new GoldenRequest("macos", "12.6", "basic-console", "x64"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "12.6", "basic-console", "X64"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "12.6", "basic-console", "x64", minimumVersion: "13"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "12.6", "basic-console", "x64", minimumVersion: "12.7"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "12.6", "basic-console", "x64", minimumVersion: "base"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "basic-console", "x64", minimumVersion: "12"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("ubuntu", "22.04", "basic-console", "x64", minimumVersion: "22.10"));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("mac", "base", "visual-text", "arm64", "ganesh-gl", true));
			Assert.Throws<ArgumentException>(() => new GoldenRequest("linux", "base", "basic-console", "x64", "noto-sans", true));
		}

		[Fact]
		public void ExactMinorMajorAndBaseEachTryCurrentThenNeutralArchitecture()
		{
			var result = Store().Resolve(new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64"));
			Assert.Null(result.Asset);
			Assert.Equal(new[]
			{
				"mac/12.6.2/blazor-cpu.x64.png", "mac/12.6.2/blazor-cpu.png",
				"mac/12.6/blazor-cpu.x64.png", "mac/12.6/blazor-cpu.png",
				"mac/12/blazor-cpu.x64.png", "mac/12/blazor-cpu.png",
				"mac/base/blazor-cpu.x64.png", "mac/base/blazor-cpu.png",
			}, result.Candidates);
		}

		[Fact]
		public void MatchingMinorAndFamilyBaselinesBeatOlderExactCaptures()
		{
			var request = new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64");
			AssertChosen(Store("mac/12.6.1/blazor-cpu.x64.png", "mac/12.6/blazor-cpu.png").Resolve(request),
				"mac/12.6/blazor-cpu.png");
			AssertChosen(Store("mac/12.6.1/blazor-cpu.x64.png", "mac/12/blazor-cpu.png").Resolve(request),
				"mac/12/blazor-cpu.png");
		}

		[Fact]
		public void ExactNeutralArchitectureBeatsOlderSpecificArchitecture()
		{
			var result = Store("mac/12.6.2/blazor-cpu.png", "mac/12.6.1/blazor-cpu.x64.png")
				.Resolve(new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64"));
			AssertChosen(result, "mac/12.6.2/blazor-cpu.png");
			Assert.Equal(2, result.Candidates.Count);
		}

		[Fact]
		public void BaseArchitectureNeutralityNeverMeansAnotherNamedArchitecture()
		{
			var request = new GoldenRequest("mac", "base", "blazor-cpu", "arm64");
			Assert.Null(Store("mac/base/blazor-cpu.x64.png").Resolve(request).Asset);
			AssertChosen(Store("mac/base/blazor-cpu.png", "mac/base/blazor-cpu.x64.png").Resolve(request),
				"mac/base/blazor-cpu.png");
		}

		[Fact]
		public void NearestOlderVersionsAreNumericCaseScopedAndFamilyBounded()
		{
			var result = Store(
				"mac/12.9/blazor-cpu.x64.png", "mac/12.10/blazor-cpu.arm64.png",
				"mac/12.8/blazor-cpu.png", "mac/12.11.2/blazor-cpu.x64.png",
				"mac/13/blazor-cpu.x64.png", "mac/11.9/blazor-cpu.x64.png",
				"mac/12.10.5/uno-cpu.x64.png", "mac/base/blazor-cpu.png")
				.Resolve(new GoldenRequest("mac", "12.11.1", "blazor-cpu", "x64"));
			AssertChosen(result, "mac/12.9/blazor-cpu.x64.png");
			Assert.DoesNotContain(result.Candidates, c => c.Contains("/12.10/") || c.Contains("/12.10.5/") ||
				c.Contains("/12.11.2/") || c.Contains("/13/") || c.Contains("/11.9/"));
			AssertChosen(Store("mac/12.9/blazor-cpu.png", "mac/12.10/blazor-cpu.png")
				.Resolve(new GoldenRequest("mac", "12.11", "blazor-cpu", null)), "mac/12.10/blazor-cpu.png");
		}

		[Fact]
		public void MinorBaselineAndExactZeroPatchRemainDistinct()
		{
			var store = Store("mac/12.6/blazor-cpu.png", "mac/12.6.0/blazor-cpu.png");
			AssertChosen(store.Resolve(new GoldenRequest("mac", "12.6.2", "blazor-cpu", null)), "mac/12.6/blazor-cpu.png");
			AssertChosen(store.Resolve(new GoldenRequest("mac", "12.6.0", "blazor-cpu", null)), "mac/12.6.0/blazor-cpu.png");
			AssertChosen(Store("mac/12.6.0/blazor-cpu.png", "mac/base/blazor-cpu.png")
				.Resolve(new GoldenRequest("mac", "12.6", "blazor-cpu", null)), "mac/base/blazor-cpu.png");
			AssertChosen(Store("mac/12.6.0/blazor-cpu.png")
				.Resolve(new GoldenRequest("mac", "12.6.1", "blazor-cpu", null)), "mac/12.6.0/blazor-cpu.png");
		}

		[Theory]
		[InlineData("ios", "26.3.2", "26.3.3")]
		[InlineData("ios", "26.3.2", "18.6")]
		[InlineData("tvos", "26.3.2", "25.9")]
		[InlineData("maccatalyst", "26.3.2", "27")]
		[InlineData("mac", "10.15.7", "10.14.9")]
		[InlineData("mac", "10.15.7", "10.15.8")]
		[InlineData("mac", "12.6.0", "12.6.1")]
		[InlineData("pixel-9", "36.1.r8", "35.9.r9")]
		[InlineData("pixel-9", "36.1.r8", "36.1.r9")]
		[InlineData("pixel-9", "36.r8", "36.1")]
		[InlineData("windows-client", "26100.2033", "26100.2034")]
		[InlineData("windows-client", "26100.2033", "22621.9999")]
		[InlineData("ubuntu", "22.04.5", "22.10")]
		[InlineData("ubuntu", "22.04.5", "22.04.6")]
		[InlineData("alpine", "3.23.3", "3.22.9")]
		[InlineData("azurelinux", "3.0.7", "2.0.9")]
		[InlineData("debian", "12.11", "11.99")]
		public void FutureAndSiblingFamilyReferencesAreNeverCandidates(string profile, string runtime, string forbidden)
		{
			var result = Store(profile + "/" + forbidden + "/basic-console.png", profile + "/base/basic-console.png")
				.Resolve(new GoldenRequest(profile, runtime, "basic-console", "x64"));
			AssertChosen(result, profile + "/base/basic-console.png");
			Assert.DoesNotContain(result.Candidates, c => c.StartsWith(profile + "/" + forbidden + "/", StringComparison.Ordinal));
		}

		[Theory]
		[InlineData("mac", "10.15.7", "10.15.7,10.15,base")]
		[InlineData("iphone-16-pro", "26.3.1", "26.3.1,26.3,26,base")]
		[InlineData("android", "36.1.r8", "36.1.r8,36.1,36,base")]
		[InlineData("pixel-9", "36.r8", "36.r8,36,base")]
		[InlineData("pixel-9", "36", "36,base")]
		[InlineData("windows-client", "26100.2033", "26100.2033,26100,base")]
		[InlineData("windows-server", "26100", "26100,base")]
		[InlineData("ubuntu", "22.04.5", "22.04.5,22.04,base")]
		[InlineData("alpine", "3.23.3", "3.23.3,3.23,base")]
		[InlineData("azurelinux", "3.0.7", "3.0.7,3.0,base")]
		[InlineData("debian", "12.11", "12.11,12,base")]
		public void PlatformDomainsProduceOnlyTheirDeclaredVersionTiers(string profile, string runtime, string versions)
		{
			var result = Store().Resolve(new GoldenRequest(profile, runtime, "basic-console", null));
			Assert.Equal(versions.Split(','), result.Candidates.Where(c => c.StartsWith(profile + "/", StringComparison.Ordinal)).Select(c => c.Split('/')[1]));
		}

		[Fact]
		public void AndroidImageRevisionAndWindowsServicingUseBoundedOlderFallback()
		{
			AssertChosen(Store("pixel-9/36.1.r6/native-basic-cpu.png", "pixel-9/36.1.r7/native-basic-cpu.png")
				.Resolve(new GoldenRequest("pixel-9", "36.1.r8", "native-basic-cpu", "arm64")), "pixel-9/36.1.r7/native-basic-cpu.png");
			AssertChosen(Store("pixel-9/36.1.r7/native-basic-cpu.png", "pixel-9/36.1/native-basic-cpu.png")
				.Resolve(new GoldenRequest("pixel-9", "36.1.r8", "native-basic-cpu", "arm64")), "pixel-9/36.1/native-basic-cpu.png");
			AssertChosen(Store("windows-client/26100.999/basic-console.png", "windows-client/26100.2032/basic-console.png")
				.Resolve(new GoldenRequest("windows-client", "26100.2033", "basic-console", "x64")), "windows-client/26100.2032/basic-console.png");
		}

		[Theory]
		[InlineData("windows", "10.0.26100.2033")]
		[InlineData("linux", "6.8.0")]
		[InlineData("browser", "130.0.1.2")]
		[InlineData("portable", "26.3.1")]
		public void ExplicitVersionNeutralProfilesRetainRequestMetadataButUseOnlyBase(string profile, string runtime)
		{
			var request = new GoldenRequest(profile, runtime, "blazor-cpu", "x64");
			var result = Store(profile + "/base/blazor-cpu.png").Resolve(request);
			Assert.Same(request, result.Request);
			Assert.Equal(runtime, result.Request.Version);
			AssertChosen(result, profile + "/base/blazor-cpu.png");
			Assert.Equal(new[] { profile + "/base/blazor-cpu.x64.png", profile + "/base/blazor-cpu.png" }, result.Candidates);
		}

		[Fact]
		public void BrowserEngineFallbackIsExplicitAndNeverUsesASiblingEngine()
		{
			var request = new GoldenRequest("mac", "base", "blazor-cpu", "x64", "firefox", true);
			var result = Store("mac/base/blazor-cpu.chromium.x64.png").Resolve(request);
			Assert.Null(result.Asset);
			Assert.Equal(new[]
			{
				"mac/base/blazor-cpu.firefox.x64.png", "mac/base/blazor-cpu.firefox.png",
				"mac/base/blazor-cpu.x64.png", "mac/base/blazor-cpu.png",
			}, result.Candidates);
			AssertChosen(Store("mac/base/blazor-cpu.firefox.png", "mac/base/blazor-cpu.x64.png").Resolve(request), "mac/base/blazor-cpu.firefox.png");
			AssertChosen(Store("mac/base/blazor-cpu.x64.png").Resolve(request), "mac/base/blazor-cpu.x64.png");
			Assert.Null(Store("mac/base/blazor-cpu.png")
				.Resolve(new GoldenRequest("mac", "base", "blazor-cpu", "x64", "firefox")).Asset);
			Assert.Null(Store("mac/base/blazor-cpu.chromium.png")
				.Resolve(new GoldenRequest("mac", "base", "blazor-cpu", "x64")).Asset);
		}

		[Fact]
		public void BundledConsoleFontIsAnExactFunctionalVariantNotADriverDimension()
		{
			var request = new GoldenRequest("linux", "base", "basic-console", "x64", "noto-sans");
			Assert.Null(Store("linux/base/basic-console.png", "linux/base/basic-console.x64.png").Resolve(request).Asset);
			AssertChosen(Store("linux/base/basic-console.noto-sans.png", "linux/base/basic-console.x64.png").Resolve(request),
				"linux/base/basic-console.noto-sans.png");
			Assert.Null(Store("linux/base/basic-console.noto-sans.png")
				.Resolve(new GoldenRequest("linux", "base", "basic-console", "x64")).Asset);
			Assert.Throws<ArgumentException>(() => new GoldenRequest("browser", "base", "basic-console", "x64", "noto-sans", true));
		}

		[Fact]
		public void VersionSpecificityAlsoPrecedesBrowserEngineSpecificity()
		{
			var request = new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64", "firefox", true);
			AssertChosen(Store("mac/12.6.2/blazor-cpu.png", "mac/12.6.1/blazor-cpu.firefox.x64.png")
				.Resolve(request), "mac/12.6.2/blazor-cpu.png");
			var missing = Store("mac/12.6.1/blazor-cpu.chromium.png").Resolve(request);
			Assert.Null(missing.Asset);
			Assert.DoesNotContain(missing.Candidates, c => c.Contains("chromium") || c.Contains("/12.6.1/"));
		}

		[Theory]
		[InlineData("mac")]
		[InlineData("windows")]
		[InlineData("linux")]
		[InlineData("ios")]
		[InlineData("android")]
		[InlineData("maccatalyst")]
		[InlineData("tvos")]
		[InlineData("browser")]
		[InlineData("ubuntu")]
		[InlineData("windows-nanoserver")]
		public void OsVisualProfilesCanUsePortableButNeverChangeBackend(string profile)
		{
			var request = new GoldenRequest(profile, "base", "visual-diagonal-lines", "arm64", "ganesh-gl");
			Assert.Null(Store("portable/base/visual-diagonal-lines.raster.png",
				profile + "/base/visual-diagonal-lines.ganesh-metal.png").Resolve(request).Asset);
			AssertChosen(Store("portable/base/visual-diagonal-lines.ganesh-gl.png").Resolve(request),
				"portable/base/visual-diagonal-lines.ganesh-gl.png");
		}

		[Fact]
		public void NanoWindowsParentIsVisualOnlyAndPrecedesPortable()
		{
			AssertChosen(Store("windows/base/visual-text.raster.png", "portable/base/visual-text.raster.png")
				.Resolve(new GoldenRequest("windows-nanoserver", "26100.2033", "visual-text", "x64", "raster")),
				"windows/base/visual-text.raster.png");
			Assert.Null(Store("windows/base/basic-console.noto-sans.png")
				.Resolve(new GoldenRequest("windows-nanoserver", "26100.2033", "basic-console", "x64", "noto-sans")).Asset);
			Assert.Null(Store("windows-server/26100/basic-console.png")
				.Resolve(new GoldenRequest("windows-client", "26100", "basic-console", "x64")).Asset);
		}

		[Theory]
		[InlineData("windows-client")]
		[InlineData("windows-server")]
		[InlineData("windows-servercore")]
		public void WindowsProductSamplesCanUsePreservedBroadWindowsBaselines(string profile)
		{
			var request = new GoldenRequest(profile, "26100.2033", "basic-console", "x64");
			AssertChosen(Store("windows/base/basic-console.png").Resolve(request), "windows/base/basic-console.png");
			AssertChosen(Store(profile + "/base/basic-console.png", "windows/base/basic-console.x64.png").Resolve(request),
				profile + "/base/basic-console.png");
			AssertChosen(Store("windows/base/basic-console.noto-sans.png")
				.Resolve(new GoldenRequest(profile, "26100.2033", "basic-console", "x64", "noto-sans")),
				"windows/base/basic-console.noto-sans.png");
			Assert.Null(Store("windows/base/basic-console.png")
				.Resolve(new GoldenRequest(profile, "26100.2033", "basic-console", "x64", exactVersion: true)).Asset);
			Assert.Null(Store("windows/base/basic-console.png")
				.Resolve(new GoldenRequest(profile, "26100.2033", "basic-console", "x64", minimumVersion: "26100")).Asset);
		}

		[Theory]
		[InlineData("Client", "windows-client")]
		[InlineData("Server", "windows-server")]
		[InlineData("Server Core", "windows-servercore")]
		[InlineData("Nano Server", "windows-nanoserver")]
		public void WindowsInstallationKindsRemainDistinct(string kind, string profile)
		{
			Assert.Equal(profile, GoldenEnvironment.WindowsProfile(kind));
			foreach (var sibling in new[] { "windows-client", "windows-server", "windows-servercore", "windows-nanoserver" })
			{
				if (sibling == profile)
					continue;
				Assert.Null(Store(sibling + "/26100/basic-console.png")
					.Resolve(new GoldenRequest(profile, "26100.2033", "basic-console", "x64")).Asset);
			}
		}

		[Theory]
		[InlineData("")]
		[InlineData("Windows PE")]
		[InlineData("Unrecognized")]
		public void UnknownWindowsInstallationKindsNeverDefaultToServer(string kind) =>
			Assert.Throws<InvalidOperationException>(() => GoldenEnvironment.WindowsProfile(kind));

		[Fact]
		public void DistroParentsAreExplicitAndNeverBorrowNumericVersionsOrLibcSiblings()
		{
			var request = new GoldenRequest("alpine", "3.23.3", "basic-console", "x64", "noto-sans");
			Assert.Null(Store("ubuntu/22.04/basic-console.noto-sans.png", "azurelinux/3.0/basic-console.noto-sans.png")
				.Resolve(request).Asset);
			AssertChosen(Store("linux/base/basic-console.noto-sans.png").Resolve(request), "linux/base/basic-console.noto-sans.png");
			Assert.Null(Store("linux/base/basic-console.png").Resolve(request).Asset);
			AssertChosen(Store("alpine/3.23/basic-console.noto-sans.png", "linux/base/basic-console.noto-sans.png")
				.Resolve(request), "alpine/3.23/basic-console.noto-sans.png");
		}

		[Fact]
		public void ExactDeviceProfilesNeverUseGenericOsOrAnotherDevice()
		{
			Assert.Null(Store("ios/26.3.1/maui-basic-cpu.png", "mac/base/maui-basic-cpu.png", "pixel-9/base/maui-basic-cpu.png")
				.Resolve(new GoldenRequest("iphone-16-pro", "26.3.1", "maui-basic-cpu", "arm64")).Asset);
			Assert.Null(Store("android/36/native-basic-cpu.png", "iphone-16-pro/base/native-basic-cpu.png")
				.Resolve(new GoldenRequest("pixel-9", "36", "native-basic-cpu", "x64")).Asset);
			AssertChosen(Store("iphone-16-pro/base/gallery-gradient-cpu.png")
				.Resolve(new GoldenRequest("iphone-16-pro", "26.4.2", "gallery-gradient-cpu", "arm64")),
				"iphone-16-pro/base/gallery-gradient-cpu.png");
		}

		[Fact]
		public void MinimumVersionExcludesOlderSharedBaselinesAndProfileParents()
		{
			var request = new GoldenRequest("mac", "12.6.2", "visual-text", "arm64", "raster", minimumVersion: "12.6.1");
			var result = Store("mac/12.6/visual-text.raster.png", "mac/12/visual-text.raster.png",
				"mac/12.6.0/visual-text.raster.png", "mac/base/visual-text.raster.png", "portable/base/visual-text.raster.png").Resolve(request);
			Assert.Null(result.Asset);
			Assert.DoesNotContain(result.Candidates, c => c.Contains("/base/") || c.StartsWith("portable/", StringComparison.Ordinal));
			AssertChosen(Store("mac/12.6.1/visual-text.raster.png").Resolve(request), "mac/12.6.1/visual-text.raster.png");
			AssertChosen(Store("mac/12.6/visual-text.raster.png")
				.Resolve(new GoldenRequest("mac", "12.6.2", "visual-text", "arm64", "raster", minimumVersion: "12.6")),
				"mac/12.6/visual-text.raster.png");
		}

		[Fact]
		public void ExactVersionStillAllowsArchitectureNeutralityButBlocksAllVersionGeneralization()
		{
			var request = new GoldenRequest("mac", "12.6.2", "visual-text", "arm64", "raster", exactVersion: true);
			var result = Store("mac/12.6/visual-text.raster.png", "mac/12.6.1/visual-text.raster.png",
				"mac/base/visual-text.raster.png", "portable/base/visual-text.raster.png").Resolve(request);
			Assert.Null(result.Asset);
			Assert.Equal(new[] { "mac/12.6.2/visual-text.raster.arm64.png", "mac/12.6.2/visual-text.raster.png" }, result.Candidates);
			AssertChosen(Store("mac/12.6.2/visual-text.raster.png").Resolve(request), "mac/12.6.2/visual-text.raster.png");
			Assert.Null(Store("linux/base/basic-console.png")
				.Resolve(new GoldenRequest("linux", "6.8", "basic-console", "x64", exactVersion: true)).Asset);
			Assert.Null(Store("linux/base/basic-console.png")
				.Resolve(new GoldenRequest("linux", "6.8", "basic-console", "x64", minimumVersion: "6.8")).Asset);
		}

		[Fact]
		public void AssetsLoadLazilyOnceAndDoNotExposeMutableCachedBytes()
		{
			var reads = 0;
			var supplied = new byte[] { 1, 2, 3 };
			var asset = new GoldenAsset(GoldenKey.Parse("mac/base/blazor-cpu.png"), "memory:exact", () => { reads++; return supplied; });
			var index = GoldenStoreIndex.FromAssets(asset);
			var result = index.Resolve(new GoldenRequest("mac", "base", "blazor-cpu", null));
			Assert.Same(asset, result.Asset);
			Assert.Equal(0, reads);
			var bytes = result.Asset.ReadBytes();
			bytes[0] = 9;
			supplied[1] = 9;
			Assert.Equal(new byte[] { 1, 2, 3 }, result.Asset.ReadBytes());
			Assert.Equal(1, reads);
		}

		[Fact]
		public void IdenticalDuplicateProvidersDeduplicateButConflictingBytesFail()
		{
			const string key = "mac/base/blazor-cpu.png";
			var first = GoldenStoreIndex.FromAssets(Asset(key, "first", 1, 2));
			var identical = GoldenStoreIndex.FromAssets(Asset(key, "second", 1, 2));
			var combined = GoldenStoreIndex.Combine(first, identical);
			AssertChosen(combined.Resolve(new GoldenRequest("mac", "base", "blazor-cpu", null)), key);
			Assert.Equal("first", combined.Resolve(new GoldenRequest("mac", "base", "blazor-cpu", null)).Asset.Location);
			var error = Assert.Throws<InvalidOperationException>(() => GoldenStoreIndex.Combine(first,
				GoldenStoreIndex.FromAssets(Asset(key, "conflict", 1, 3))));
			Assert.Contains(key, error.Message);
			Assert.Contains("first", error.Message);
			Assert.Contains("conflict", error.Message);
		}

		[Fact]
		public void DirectoryAndResourceProvidersUseTheSameCanonicalKeysAndBytes()
		{
			using (var files = new GoldenDirectory())
			{
				const string key = "mac/12.6/blazor-cpu.x64.png";
				files.Write(key, 1, 2, 3);
				var assembly = new GoldenAssembly(new Dictionary<string, byte[]>
				{
					[GoldenStoreIndex.ResourcePrefix + key] = new byte[] { 1, 2, 3 },
					["SkiaSharp.Goldens.v1.mac.12.6.blazor-cpu.x64.png"] = new byte[] { 9 },
				});
				var directory = GoldenStoreIndex.FromDirectory(files.Root);
				var resources = GoldenStoreIndex.FromAssembly(assembly);
				Assert.Equal(1, assembly.Enumerations);
				Assert.Equal(0, assembly.Opens);
				var request = new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64");
				var diskResult = directory.Resolve(request);
				var resourceResult = resources.Resolve(request);
				AssertChosen(diskResult, key);
				AssertChosen(resourceResult, key);
				Assert.Equal(diskResult.Candidates, resourceResult.Candidates);
				Assert.Equal(diskResult.Asset.ReadBytes(), resourceResult.Asset.ReadBytes());
				Assert.Equal(1, assembly.Opens);
				AssertChosen(GoldenStoreIndex.Combine(directory, resources).Resolve(request), key);
				resources.Resolve(request);
				Assert.Equal(1, assembly.Enumerations);
				Assert.Equal(1, assembly.Opens);
			}
		}

		[Fact]
		public void KeySpecificityPrecedesProviderOrder()
		{
			var request = new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64");
			var disk = Store("mac/base/blazor-cpu.x64.png");
			var embedded = Store("mac/12.6.2/blazor-cpu.png");
			AssertChosen(GoldenStoreIndex.Combine(disk, embedded).Resolve(request), "mac/12.6.2/blazor-cpu.png");
			AssertChosen(GoldenStoreIndex.Combine(embedded, disk).Resolve(request), "mac/12.6.2/blazor-cpu.png");
		}

		[Fact]
		public void ProviderEnumerationRejectsAliasesRatherThanNormalizingThem()
		{
			using (var files = new GoldenDirectory())
			{
				files.Write("mac/base/blazor-cpu.PNG", 1);
				Assert.Throws<ArgumentException>(() => GoldenStoreIndex.FromDirectory(files.Root));
			}
			var badResource = new GoldenAssembly(new Dictionary<string, byte[]>
			{
				[GoldenStoreIndex.ResourcePrefix + "mac/base/BlazorCpu.png"] = new byte[] { 1 },
			});
			Assert.Throws<ArgumentException>(() => GoldenStoreIndex.FromAssembly(badResource));
		}

		[Fact]
		public void MissingLookupsAreReadOnlyExplicitFailuresWithOrderedDiagnostics()
		{
			using (var files = new GoldenDirectory())
			{
				var result = GoldenStoreIndex.FromDirectory(files.Root).Resolve(new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64"));
				Assert.Null(result.Asset);
				Assert.Empty(Directory.GetFiles(files.Root, "*", SearchOption.AllDirectories));
				Assert.Contains("MISSING", result.Describe());
				Assert.Contains("mac/12.6.2/blazor-cpu.x64.png", result.Describe());
				Assert.Contains("mac/base/blazor-cpu.png", result.Describe());
				Assert.Throws<NotSupportedException>(() => ((IList<string>)result.Candidates).Add("injected"));
				var absent = Path.Combine(files.Root, "not-created");
				Assert.Throws<DirectoryNotFoundException>(() => GoldenStoreIndex.FromDirectory(absent));
				Assert.False(Directory.Exists(absent));
			}
		}

		[Fact]
		public void IndexIsASnapshotAndNeverReenumeratesDuringResolution()
		{
			using (var files = new GoldenDirectory())
			{
				files.Write("mac/base/blazor-cpu.png", 1);
				var index = GoldenStoreIndex.FromDirectory(files.Root);
				files.Write("mac/12.6.2/blazor-cpu.x64.png", 2);
				AssertChosen(index.Resolve(new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64")), "mac/base/blazor-cpu.png");
			}
		}

		[Fact]
		public void IndexedReadFailureIsTerminalAndDoesNotAdvanceTheResolvedReference()
		{
			using (var files = new GoldenDirectory())
			{
				const string exact = "mac/12.6.2/blazor-cpu.png";
				files.Write(exact, 1);
				files.Write("mac/base/blazor-cpu.png", 2);
				var index = GoldenStoreIndex.FromDirectory(files.Root);
				var request = new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64");
				var resolved = index.Resolve(request);
				File.Delete(files.PathFor(exact));
				Assert.Throws<FileNotFoundException>(() => resolved.Asset.ReadBytes());
				AssertChosen(index.Resolve(request), exact);
				Assert.Equal(resolved.Candidates, index.Resolve(request).Candidates);
			}
		}

		[Fact]
		public void SelectionIsStableAndNeverExaminesPngGeometryDecodeOrPixels()
		{
			var reads = 0;
			var exact = Asset("mac/12.6.2/blazor-cpu.png", "not-a-png", 0, 1);
			var fallback = new GoldenAsset(GoldenKey.Parse("mac/base/blazor-cpu.png"), "fallback", () => { reads++; return new byte[] { 2 }; });
			var index = GoldenStoreIndex.FromAssets(exact, fallback);
			var request = new GoldenRequest("mac", "12.6.2", "blazor-cpu", "x64");
			var first = index.Resolve(request);
			AssertChosen(first, exact.Key.ToString());
			Assert.Equal(new byte[] { 0, 1 }, first.Asset.ReadBytes());
			Assert.Same(first.Asset, index.Resolve(request).Asset);
			Assert.Equal(0, reads);
			Assert.Contains("not-a-png", first.Describe());
			Assert.DoesNotContain("mac/base/", first.Describe());
		}

		private static GoldenStoreIndex Store(params string[] keys) =>
			GoldenStoreIndex.FromAssets(keys.Select(k => Asset(k, "memory:" + k, 1)).ToArray());

		private static GoldenAsset Asset(string key, string location, params byte[] bytes) =>
			new GoldenAsset(GoldenKey.Parse(key), location, () => bytes);

		private static void AssertChosen(GoldenResolution result, string key)
		{
			Assert.NotNull(result.Asset);
			Assert.Equal(key, result.Asset.Key.ToString());
			Assert.Equal(key, result.Candidates[result.Candidates.Count - 1]);
		}

		private sealed class GoldenDirectory : IDisposable
		{
			public GoldenDirectory()
			{
				Root = Path.Combine(Environment.CurrentDirectory, "golden-reference-tests-" + Guid.NewGuid().ToString("N"));
				Directory.CreateDirectory(Root);
			}

			public string Root { get; }
			public string PathFor(string key) => Path.Combine(Root, key.Replace('/', Path.DirectorySeparatorChar));
			public void Write(string key, params byte[] bytes)
			{
				var path = PathFor(key);
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				File.WriteAllBytes(path, bytes);
			}

			public void Dispose() => Directory.Delete(Root, true);
		}

		private sealed class GoldenAssembly : Assembly
		{
			private readonly Dictionary<string, byte[]> resources;
			public GoldenAssembly(Dictionary<string, byte[]> resources) => this.resources = resources;
			public int Enumerations { get; private set; }
			public int Opens { get; private set; }
			public override string FullName => "GoldenReferenceFixture";
			public override string[] GetManifestResourceNames()
			{
				Enumerations++;
				return resources.Keys.ToArray();
			}

			public override Stream GetManifestResourceStream(string name)
			{
				Opens++;
				return resources.TryGetValue(name, out var bytes) ? new MemoryStream(bytes, false) : null;
			}
		}
	}
}
