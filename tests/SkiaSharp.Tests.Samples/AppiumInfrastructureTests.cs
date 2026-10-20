using System.Collections.ObjectModel;
using System.Drawing;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;
using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests : IDisposable
{
	private readonly string root = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("appium-infrastructure"));

	private static AppiumAppSpec AndroidSpec => new(
		"android", "arm64", "Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj", "net10.0-android", "android-arm64");

	private static AppiumRunningApp ArtifactSession(SampleWorkspace workspace, string artifact) =>
		new DiagnosticApp(workspace, artifact, []);

	private string ArtifactPackages()
	{
		var directory = Path.Combine(root, "artifact-package-fixtures");
		Directory.CreateDirectory(directory);
		foreach (var id in new[] { "SkiaSharp", "HarfBuzzSharp" })
		{
			var path = Path.Combine(directory, $"{id}.1.0.0-unit-fixture.nupkg");
			if (File.Exists(path))
				continue;
			using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
			using var writer = new StreamWriter(archive.CreateEntry(id + ".nuspec").Open());
			writer.Write($"<package><metadata><id>{id}</id><version>1.0.0-unit-fixture</version></metadata></package>");
		}
		return directory;
	}

	[Theory]
	[InlineData(null, "All")]
	[InlineData("all", "All")]
	[InlineData("build", "Build")]
	[InlineData("run", "Run")]
	public void AppiumPhasesAreExplicit(string? value, string expected) =>
		Assert.Equal(expected, AppiumAppArtifact.ParseMode(value).ToString());

	[Theory]
	[InlineData("")]
	[InlineData("BUILD")]
	[InlineData("run-only")]
	public void AppiumRejectsInvalidPhases(string value) =>
		Assert.Throws<ArgumentException>(() => AppiumAppArtifact.ParseMode(value));

	[Fact]
	public async Task BuildPhaseExportsOnceWithoutDeviceOrCallbackAndDoesNotCacheAcrossInvocations()
	{
		var apps = Path.Combine(root, "staged-apps");
		var packages = ArtifactPackages();
		var apk = Path.Combine(root, "Fixture-Signed.apk");
		File.WriteAllText(apk, "unit-fixture-apk");
		var builds = 0;
		Task<(string, string)> Build()
		{
			builds++;
			return Task.FromResult((apk, "com.skiasharp.fixture"));
		}
		var fixture = new AppiumFixture(AppiumMode.Build, packages, apps);
		var first = await fixture.Prepare(AndroidSpec, Build, ArtifactSession);
		await fixture.Run(first, _ => throw new InvalidOperationException("Build phase must never invoke the scenario."));
		var second = await fixture.Prepare(AndroidSpec, Build, ArtifactSession);
		Assert.Equal(1, builds);
		Assert.Equal(first.Manifest.Sha256, second.Manifest.Sha256);
		Assert.True(File.Exists(Path.Combine(second.Directory, "app.apk")));
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			fixture.Prepare(AndroidSpec with { Project = "other.csproj" }, Build, ArtifactSession));
		await new AppiumFixture(AppiumMode.All, packages, apps).Prepare(AndroidSpec, Build, ArtifactSession);
		Assert.Equal(2, builds);
	}

	[Fact]
	public async Task RunPhaseConsumesOnlyAnExactAcquiredAppAndNeverCallsBuild()
	{
		var apps = Path.Combine(root, "staged-apps");
		var packages = ArtifactPackages();
		var fixture = new AppiumFixture(AppiumMode.Run, packages, apps);
		Task<(string, string)> ForbiddenBuild() => throw new InvalidOperationException("Run must not build.");
		await Assert.ThrowsAsync<FileNotFoundException>(() => fixture.Prepare(AndroidSpec, ForbiddenBuild, ArtifactSession));
		var apk = Path.Combine(root, "Fixture-Signed.apk");
		File.WriteAllText(apk, "unit-fixture-apk");
		var directory = AppiumAppArtifact.DirectoryFor(apps, AndroidSpec);
		AppiumAppArtifact.Export(directory, AndroidSpec, apk, "com.skiasharp.fixture", AppiumAppArtifact.ReadPackages(packages));
		var prepared = await fixture.Prepare(AndroidSpec, ForbiddenBuild, ArtifactSession);
		using var workspace = Workspace();
		var materialized = AppiumAppArtifact.Materialize(prepared.Directory, prepared.Manifest, workspace.Root);
		Assert.Equal("unit-fixture-apk", File.ReadAllText(materialized));
		File.Delete(Path.Combine(directory, "app.apk"));
		await Assert.ThrowsAsync<InvalidDataException>(() => fixture.Prepare(AndroidSpec, ForbiddenBuild, ArtifactSession));
	}

	[Fact]
	public void RunWorkspaceHasNoCopiedSampleOrPrivateBuildEnvironment()
	{
		string directory;
		using (var workspace = SampleWorkspace.ForAppiumRun())
		{
			directory = workspace.Root;
			Assert.Empty(Directory.EnumerateFileSystemEntries(directory));
			Assert.False(Directory.Exists(Path.Combine(directory, "samples")));
			Assert.False(Directory.Exists(Path.Combine(directory, "cache")));
		}
		Assert.False(Directory.Exists(directory));
	}

	[Theory]
	[InlineData("cohort")]
	[InlineData("target")]
	[InlineData("schema")]
	[InlineData("traversal")]
	[InlineData("absolute")]
	[InlineData("corrupt")]
	public void RunPhaseRejectsIncompatibleOrUnsafeManifests(string mutation)
	{
		var packages = AppiumAppArtifact.ReadPackages(ArtifactPackages());
		var directory = AppiumAppArtifact.DirectoryFor(Path.Combine(root, "staged-apps"), AndroidSpec);
		var apk = Path.Combine(root, "Fixture-Signed.apk");
		File.WriteAllText(apk, "unit-fixture-apk");
		var manifest = AppiumAppArtifact.Export(directory, AndroidSpec, apk, "com.skiasharp.fixture", packages);
		manifest = mutation switch
		{
			"cohort" => manifest with { Packages = [packages[0] with { Sha256 = "foreign" }, .. packages.Skip(1)] },
			"target" => manifest with { App = AndroidSpec with { Architecture = "x64", RuntimeIdentifier = "android-x64" } },
			"schema" => manifest with { Schema = 99 },
			"traversal" => manifest with { ArtifactRelativePath = "../foreign.apk" },
			"absolute" => manifest with { ArtifactRelativePath = Path.GetFullPath(apk) },
			_ => manifest with { Sha256 = "corrupt" }
		};
		File.WriteAllText(Path.Combine(directory, "manifest.json"), JsonSerializer.Serialize(manifest));
		Assert.Throws<InvalidDataException>(() => AppiumAppArtifact.Load(directory, AndroidSpec, packages));
	}

	[Fact]
	public void PackageCohortComesFromActualPackageMetadataAndBytes()
	{
		var directory = ArtifactPackages();
		var original = AppiumAppArtifact.ReadPackages(directory);
		Assert.Equal(new[] { "HarfBuzzSharp", "SkiaSharp" }, original.Select(package => package.Id));
		Assert.All(original, package => Assert.Equal("1.0.0-unit-fixture", package.Version));
		using (var archive = ZipFile.Open(Path.Combine(directory, "SkiaSharp.1.0.0-unit-fixture.nupkg"), ZipArchiveMode.Update))
		using (var writer = new StreamWriter(archive.CreateEntry("changed-fixture.txt").Open()))
			writer.Write("changed bytes with the same package version");
		Assert.NotEqual(original[1].Sha256, AppiumAppArtifact.ReadPackages(directory)[1].Sha256);
	}

	[Fact]
	public void SimulatorArchiveRoundTripsExecutableBitsAndSymbolicLinks()
	{
		var source = Path.Combine(root, "Fixture.app");
		Directory.CreateDirectory(source);
		File.WriteAllText(Path.Combine(source, "Info.plist"), "unit-fixture-plist");
		var executable = Path.Combine(source, "Fixture");
		File.WriteAllText(executable, "unit-fixture-executable");
		var mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
			UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;
		if (!OperatingSystem.IsWindows())
		{
			File.SetUnixFileMode(executable, mode);
			File.CreateSymbolicLink(Path.Combine(source, "FixtureLink"), "Fixture");
			Directory.CreateDirectory(Path.Combine(source, "Versions", "A"));
			Directory.CreateSymbolicLink(Path.Combine(source, "Versions", "Current"), "A");
		}
		var spec = new AppiumAppSpec("ios", "arm64", "Basic/iOS", "SkiaSharpSample/SkiaSharpSample.csproj",
			"net10.0-ios", "iossimulator-arm64");
		var directory = AppiumAppArtifact.DirectoryFor(Path.Combine(root, "staged-apps"), spec);
		var packages = AppiumAppArtifact.ReadPackages(ArtifactPackages());
		AppiumAppArtifact.Export(directory, spec, source, "com.skiasharp.fixture", packages);
		var manifest = AppiumAppArtifact.Load(directory, spec, packages);
		SampleWorkspace.DeleteDirectory(source);
		using var workspace = Workspace();
		var bundle = AppiumAppArtifact.Materialize(directory, manifest, workspace.Root);
		Assert.Equal("unit-fixture-executable", File.ReadAllText(Path.Combine(bundle, "Fixture")));
		if (!OperatingSystem.IsWindows())
		{
			Assert.Equal(mode, File.GetUnixFileMode(Path.Combine(bundle, "Fixture")));
			Assert.Equal("Fixture", new FileInfo(Path.Combine(bundle, "FixtureLink")).LinkTarget);
			Assert.Equal("A", new DirectoryInfo(Path.Combine(bundle, "Versions", "Current")).LinkTarget);
			Assert.Equal("unit-fixture-executable", File.ReadAllText(Path.Combine(bundle, "FixtureLink")));
		}
	}

	[Fact]
	public async Task ManifestIdentityMismatchFailsBeforeServerOrScenario()
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order) { ExpectedApplicationId = "com.foreign.fixture" };
		await Assert.ThrowsAsync<InvalidDataException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => throw new InvalidOperationException("Scenario must not run."),
			() => throw new InvalidOperationException("Server must not start."), () => Task.CompletedTask));
		Assert.Equal(new[] { "diagnostics" }, order);
	}

	private SampleWorkspace Workspace()
	{
		var inputs = Path.Combine(root, "inputs");
		var packages = Path.Combine(root, "fixture-packages");
		Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
		Directory.CreateDirectory(packages);
		File.WriteAllText(Path.Combine(packages, "unit-fixture.nupkg"), "Workspace isolation sentinel; not a package cohort.");
		return new("Sample", inputs, packages, Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task FailedSessionStillRunsDiagnosticsAndStopsTheServer(bool android)
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order, android: android);
		var failure = await Assert.ThrowsAsync<NotSupportedException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => throw new InvalidOperationException("Scenario must not run."),
			() => { order.Add("server-start"); return Task.CompletedTask; },
			() => { order.Add("server-stop"); return Task.CompletedTask; }));
		Assert.Same(app.StartFailure, failure);
		Assert.Equal(new[] { "server-start", "session-start", "diagnostics", "server-stop" }, order);
		Assert.True(File.Exists(Path.Combine(app.Diagnostics, "failure.log")));
		await new IosRunningApp(workspace, artifact).SaveDiagnostics();
		await Assert.ThrowsAsync<InvalidOperationException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask,
			() => Task.CompletedTask, () => Task.CompletedTask));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task DiagnosticsFailuresNeverHideStartupOrPreventServerCleanup(bool android)
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order, failDiagnostics: true, android: android);
		var failure = await Assert.ThrowsAsync<AggregateException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask, () => Task.CompletedTask,
			() => { order.Add("server-stop"); throw new IOException("server-stop"); }));
		Assert.Same(app.StartFailure, failure.InnerExceptions[0]);
		Assert.Equal(3, failure.InnerExceptions.Count);
		Assert.Equal(new[] { "session-start", "diagnostics", "server-stop" }, order);
	}

	[Theory]
	[InlineData("18.6")]
	[InlineData("26.2")]
	[InlineData("26.3.1")]
	public void IosGoldenProfileUsesTheSelectedRuntime(string version)
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", version);
		Assert.Equal($"iPhone-16-Pro/{version}", profile);

		foreach (var family in new[] { "Maui", "Native" })
		{
			var expected = AppiumSampleRunTestBase.Expected(profile, "basic-cpu", family);
			Assert.Equal(Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
				"Appium", family, "iOS", "iPhone-16-Pro", version, "basic-cpu.png"), expected);
		}
	}

	[Fact]
	public void IosGoldenProfileUsesAReviewedArchitectureOverride()
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", "26.2");

		var x64 = AppiumSampleRunTestBase.Expected(profile, "basic-cpu", "Maui", "iOS", "x64");
		var arm64 = AppiumSampleRunTestBase.Expected(profile, "basic-cpu", "Maui", "iOS", "arm64");

		Assert.EndsWith(Path.Combine("26.2", "basic-cpu.x64.png"), x64);
		Assert.EndsWith(Path.Combine("26.2", "basic-cpu.png"), arm64);
		Assert.True(File.Exists(x64));
		Assert.True(File.Exists(arm64));
	}

	[Theory]
	[InlineData("18.6", "not-reviewed")]
	[InlineData("99.0", "basic-cpu")]
	public void IosGoldensFailWithoutFallingBackToAnotherProfile(string version, string name)
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", version);
		var expected = AppiumSampleRunTestBase.Expected(profile, name, "Maui");
		var actual = AppiumSampleRunTestBase.Expected("iPhone-16-Pro/26.2", "basic-cpu", "Maui");
		Assert.True(File.Exists(actual));
		Assert.False(File.Exists(expected));

		// Missing references fail before image decoding and must never be created by a test.
		var failure = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
		Assert.Contains($"Missing reviewed sample golden: {expected}", failure.Message);
		Assert.False(File.Exists(expected));
	}

	[Fact]
	public async Task ArtifactOutsideWorkspaceFailsBeforeServerStartup()
	{
		using var workspace = Workspace();
		var app = new DiagnosticApp(workspace, Path.Combine(root, "foreign.app"), []);
		var started = false;
		var failure = await Assert.ThrowsAnyAsync<Exception>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask,
			() => { started = true; return Task.CompletedTask; }, () => Task.CompletedTask));
		Assert.Contains("must belong to the selected workspace", failure.Message);
		Assert.False(started);
	}

	private sealed class DiagnosticApp(
		SampleWorkspace workspace, string artifact, List<string> order, bool failDiagnostics = false, bool android = false)
		: AppiumRunningApp(workspace, artifact, android ? new AndroidAppiumOperations() : new IosAppiumOperations())
	{
		internal Exception StartFailure { get; } = new NotSupportedException("Session construction failed.");
		internal override string DeviceProfile => "unit-fixture";
		internal override string Platform => android ? "Android" : "iOS";
		internal override Task<string> ReadApplicationId() => Task.FromResult("unit.fixture");
		protected override Task Start(Uri address)
		{
			order.Add("session-start");
			throw StartFailure;
		}
		protected override Task CapturePlatformDiagnostics()
		{
			order.Add("diagnostics");
			if (failDiagnostics)
				throw new IOException("diagnostics");
			return Task.CompletedTask;
		}
	}

	[Fact]
	public void IosOperationsReuseTheAppsOwnedSessionWithoutStartingOne()
	{
		using var workspace = Workspace();
		var app = new IosRunningApp(workspace, Path.Combine(workspace.Root, "Sample.app"));
		AppiumRunningApp running = app;
		var operations = Assert.IsType<IosAppiumOperations>(running.Operations);
		Assert.Same(operations, app.Operations);
		Assert.Equal(app.DeviceProfile, operations.DeviceProfile);
		Assert.Equal("iOS", operations.Platform);
		Assert.Throws<InvalidOperationException>(() => operations.Attach(app));

		var failure = Assert.Throws<InvalidOperationException>(() => operations.Find("canvas"));
		Assert.Equal("The owned Appium session is not ready.", failure.Message);
	}

	[Fact]
	public void OperationsRequireAnOwnerBeforeUsingTheSession()
	{
		var operations = new IosAppiumOperations();
		var failure = Assert.Throws<InvalidOperationException>(() => operations.Find("canvas"));
		Assert.Equal("Appium operations have not been attached to an app.", failure.Message);
		Assert.Throws<InvalidOperationException>(() => operations.DeviceProfile);
		Assert.Throws<InvalidOperationException>(() => operations.Platform);
		Assert.Throws<ArgumentNullException>(() => operations.Attach(null!));
	}

	[Theory]
	[InlineData("8.0", "android-26;google_apis")]
	[InlineData("17", "android-37.1;google_apis_ps16k")]
	public void AndroidGoldenProfilesRouteOnlyToTheirExactPlatformAndImage(string version, string image)
	{
		var profile = AndroidRunningApp.ReferenceProfile("pixel_9", version, image);
		Assert.Equal($"pixel_9/{image}", profile);
		foreach (var family in new[] { "Maui", "Native" })
		{
			var expected = AppiumSampleRunTestBase.Expected(profile, "basic-cpu", family, "Android");
			Assert.Equal(Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
				"Appium", family, "Android", "pixel_9", image, "basic-cpu.png"), expected);
			var missing = AppiumSampleRunTestBase.Expected(profile, "unit-fixture-not-reviewed", family, "Android");
			Assert.False(File.Exists(missing));
			var actual = AppiumSampleRunTestBase.Expected("iPhone-16-Pro/26.3.1", "basic-cpu", "Maui");
			var failure = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, missing));
			Assert.Contains($"Missing reviewed sample golden: {missing}", failure.Message);
			Assert.False(File.Exists(missing));
		}
	}

	[Theory]
	[InlineData(null, "8.0", "android-26;google_apis")]
	[InlineData("pixel_9", null, "android-26;google_apis")]
	[InlineData("pixel_9", "latest", "android-26;google_apis")]
	[InlineData("pixel_9", "8.0", null)]
	[InlineData("pixel_9", "8.0", "latest")]
	[InlineData("../pixel_9", "8.0", "android-26;google_apis")]
	[InlineData("pixel_9", "8.0", "android-26;google_apis/foreign")]
	public void AndroidRequiresAnExplicitSafeProfile(string? model, string? version, string? image) =>
		Assert.Throws<InvalidOperationException>(() => AndroidRunningApp.ReferenceProfile(model, version, image));

	[Theory]
	[InlineData("com.skiasharp.built")]
	[InlineData(" com.skiasharp.built\n")]
	public void AndroidIdentityUsesTheBuiltApkAnalyzerOutput(string output) =>
		Assert.Equal("com.skiasharp.built", AndroidRunningApp.ParseApplicationId(output));

	[Theory]
	[InlineData("")]
	[InlineData("com.skiasharp.built\ncom.foreign.app")]
	[InlineData("package: name='com.skiasharp.built'")]
	[InlineData("com.skiasharp/foreign")]
	public void AndroidRejectsInvalidOrAmbiguousApkIdentity(string output) =>
		Assert.Throws<InvalidOperationException>(() => AndroidRunningApp.ParseApplicationId(output));

	[Fact]
	public void AndroidOperationsBelongToTheAppAndUseItsPackageResourceIds()
	{
		using var workspace = Workspace();
		var app = new AndroidRunningApp(workspace, Path.Combine(workspace.Root, "Built-Signed.apk"));
		var operations = Assert.IsType<AndroidAppiumOperations>(app.Operations);
		Assert.Same(operations, app.Operations);
		Assert.Equal("Android", app.Platform);
		Assert.Equal(app.Platform, operations.Platform);
		Assert.Throws<InvalidOperationException>(() => operations.Attach(app));
		Assert.Throws<InvalidOperationException>(() => operations.Find("canvas"));
		Assert.Equal(By.Id("com.skiasharp.built:id/skiaView").ToString(),
			AndroidAppiumOperations.ElementLocator("com.skiasharp.built", "skiaView").ToString());
		Assert.Equal(By.Id("com.skiasharp.built:id/gallery-header-cpu").ToString(),
			AndroidAppiumOperations.ElementLocator("com.skiasharp.built", "gallery-header-cpu").ToString());
	}

	[Fact]
	public void CaptureReviewRetainsEachImageAndRegionAssertionUntilScenarioEnd()
	{
		List<Exception> failures = [];
		List<string> order = [];
		var actual = AppiumSampleRunTestBase.Expected("iPhone-16-Pro/26.3.1", "basic-cpu", "Maui");
		var expected = AppiumSampleRunTestBase.Expected("unit-fixture/not-reviewed", "missing", "Maui", "Android");
		foreach (var phase in new[] { "blank", "strokes", "clear" })
		{
			var captured = AppiumSampleRunTestBase.CaptureAndCheck(
				() => { order.Add($"capture-{phase}"); return actual; },
				file => SampleImage.Validate(file, expected), failures);
			if (phase != "blank")
				AppiumSampleRunTestBase.CheckImage(
					() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(captured, expected), failures);
			order.Add($"after-{phase}");
		}
		Assert.Equal(new[] { "capture-blank", "after-blank", "capture-strokes", "after-strokes", "capture-clear", "after-clear" }, order);
		var failure = Assert.Throws<AggregateException>(() => FailureHandling.ThrowFailures(failures));
		Assert.Equal(5, failure.InnerExceptions.Count);
		Assert.All(failure.InnerExceptions, error => Assert.Contains($"Missing reviewed sample golden: {expected}", error.Message));
		Assert.False(File.Exists(expected));
	}

	[Fact]
	public void NormalImageValidationFailsFastWithoutCollectingAssertions()
	{
		var continued = false;
		Assert.ThrowsAny<Xunit.Sdk.XunitException>(() =>
		{
			AppiumSampleRunTestBase.CaptureAndCheck(() => "captured.png", _ => Assert.Fail("strict mismatch"), null);
			continued = true;
		});
		Assert.False(continued);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void CaptureReviewNeverRetainsInteractionOrCaptureAssertions(bool duringCapture)
	{
		List<Exception> failures = [];
		var fatal = new Xunit.Sdk.XunitException(duringCapture ? "capture failed" : "interaction failed");
		var continued = false;
		var failure = Assert.Throws<Xunit.Sdk.XunitException>(() =>
		{
			AppiumSampleRunTestBase.CheckImage(() => Assert.Fail("earlier comparison mismatch"), failures);
			if (duringCapture)
				AppiumSampleRunTestBase.CaptureAndCheck(() => throw fatal, _ => Assert.Fail("must not validate"), failures);
			else
				throw fatal;
			continued = true;
		});
		Assert.Same(fatal, failure);
		Assert.Single(failures);
		Assert.False(continued);
	}

	[Fact]
	public void CaptureReviewNeverRetainsNonAssertionValidationErrors()
	{
		List<Exception> failures = [];
		Assert.Throws<IOException>(() => AppiumSampleRunTestBase.CaptureAndCheck(
			() => "captured.png", _ => throw new IOException("comparison infrastructure failed"), failures));
		Assert.Empty(failures);
	}

	[Fact]
	public void CapturesOfTheSameSceneKeepBothFiles()
	{
		using var workspace = Workspace();
		var app = new IosRunningApp(workspace, Path.Combine(workspace.Root, "Sample.app"));
		var before = app.Operations.SaveCapture([1, 2], "drawing");
		var after = app.Operations.SaveCapture([3, 4], "drawing");
		Assert.NotEqual(before, after);
		Assert.Equal(Path.Combine(workspace.DiagnosticsRoot, "screenshots", "01-drawing.png"), before);
		Assert.Equal(Path.Combine(workspace.DiagnosticsRoot, "screenshots", "02-drawing.png"), after);
		Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(before));
		Assert.Equal(new byte[] { 3, 4 }, File.ReadAllBytes(after));
		Assert.Throws<ArgumentException>(() => app.Operations.SaveCapture([], "../foreign"));
	}

	[Fact]
	public void IosCapturePolicyRemovesOnlyTheKnownSystemBottomBand()
	{
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ScreenshotInsets(
			"iPhone 16 Pro", "18.6", new(1206, 2622)));
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ScreenshotInsets(
			"iPhone 16 Pro", "26.2", new(1206, 2622)));
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ScreenshotInsets(
			"iPhone 16 Pro", "26.3.1", new(1206, 2622)));
		Assert.Equal(SKRectI.Empty, IosRunningApp.ScreenshotInsets(
			"iPhone 16", "26.2", new(1206, 2622)));
		Assert.Equal(SKRectI.Empty, IosRunningApp.ScreenshotInsets(
			"iPhone 16 Pro", "26.2", new(1032, 2592)));

		using var source = new SKBitmap(4, 5);
		source.Erase(SKColors.Red);
		source.SetPixel(1, 1, SKColors.Blue);
		source.SetPixel(2, 2, SKColors.Green);
		using var image = SKImage.FromBitmap(source);
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);

		var croppedBytes = AppiumRunningApp.Crop(data.ToArray(), new(1, 1, 1, 2));
		using var cropped = SKBitmap.Decode(croppedBytes);
		Assert.NotNull(cropped);
		Assert.Equal(2, cropped.Width);
		Assert.Equal(2, cropped.Height);
		Assert.Equal(SKColors.Blue, cropped.GetPixel(0, 0));
		Assert.Equal(SKColors.Green, cropped.GetPixel(1, 1));
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			AppiumRunningApp.Crop(data.ToArray(), new(-1, 0, 0, 0)));
		Assert.Throws<ArgumentOutOfRangeException>(() =>
			AppiumRunningApp.Crop(data.ToArray(), new(2, 0, 2, 0)));
	}

	[Theory]
	[InlineData("18.6", true)]
	[InlineData("18.6", false)]
	[InlineData("26.2", true)]
	[InlineData("26.2", false)]
	[InlineData("26.3.1", true)]
	[InlineData("26.3.1", false)]
	public void DrawingChecksRejectEitherMissingStrokeAndIncompleteClear(string version, bool horizontal)
	{
		var references = Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
			"Appium", "Maui", "iOS", "iPhone-16-Pro", version);
		var drawn = Path.Combine(references, "basic-drawing-two-lines.png");
		var blank = Path.Combine(references, "basic-drawing-page.png");
		AppiumSampleRunTestBase.AssertDrawingStrokeRegions(drawn, drawn);
		AppiumSampleRunTestBase.AssertDrawingStrokeRegions(blank, blank);

		using var workspace = Workspace();
		using var drawnBitmap = SKBitmap.Decode(drawn);
		using var blankBitmap = SKBitmap.Decode(blank);
		Assert.NotNull(drawnBitmap);
		Assert.NotNull(blankBitmap);
		using var missingStroke = drawnBitmap.Copy();
		using var incompleteClear = blankBitmap.Copy();
		var region = horizontal
			? SKRectI.Round(new SKRect(drawnBitmap.Width * 0.35f, drawnBitmap.Height * 0.43f,
				drawnBitmap.Width * 0.45f, drawnBitmap.Height * 0.47f))
			: SKRectI.Round(new SKRect(drawnBitmap.Width * 0.48f, drawnBitmap.Height * 0.35f,
				drawnBitmap.Width * 0.52f, drawnBitmap.Height * 0.40f));
		for (var y = region.Top; y < region.Bottom; y++)
			for (var x = region.Left; x < region.Right; x++)
			{
				missingStroke.SetPixel(x, y, blankBitmap.GetPixel(x, y));
				incompleteClear.SetPixel(x, y, drawnBitmap.GetPixel(x, y));
			}
		var missing = Path.Combine(workspace.Root, "missing-stroke.png");
		var residual = Path.Combine(workspace.Root, "incomplete-clear.png");
		using (var data = missingStroke.Encode(SKEncodedImageFormat.Png, 100))
			File.WriteAllBytes(missing, data.ToArray());
		using (var data = incompleteClear.Encode(SKEncodedImageFormat.Png, 100))
			File.WriteAllBytes(residual, data.ToArray());
		Assert.ThrowsAny<Exception>(() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(missing, drawn));
		Assert.ThrowsAny<Exception>(() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(residual, blank));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task IosAppIdentityReadsXmlAndBinaryBundlePlists(bool binary)
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Apple bundle metadata requires macOS plutil.");
		using var workspace = Workspace();
		var bundle = Path.Combine(workspace.Root, "Built.app");
		Directory.CreateDirectory(bundle);
		var plist = Path.Combine(bundle, "Info.plist");
		File.WriteAllText(plist, """
			<?xml version="1.0" encoding="UTF-8"?>
			<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
			<plist version="1.0"><dict><key>CFBundleIdentifier</key><string>com.skiasharp.built</string></dict></plist>
			""");
		if (binary)
		{
			var result = await ProcessRunner.Run("plutil", ["-convert", "binary1", plist], workspace.Root, TimeSpan.FromSeconds(30));
			Assert.Equal(0, result.ExitCode);
		}
		Assert.Equal("com.skiasharp.built", await new IosRunningApp(workspace, bundle).ReadApplicationId());
	}

	[Theory]
	[InlineData(610, 207, 819, 18, 114)]
	[InlineData(610, 207, 841, 35, 124)]
	[InlineData(610, 207, 590, 18, -114)]
	[InlineData(610, 207, 610, 18, 0)]
	[InlineData(610, 207, 799, 18, 0)]
	public void ScrollAmountComesFromCurrentBounds(int top, int height, int targetTop, int targetHeight, int expected) =>
		Assert.Equal(expected, AppiumOperations.ScrollAmount(
			new Rectangle(12, top, 378, height), new Rectangle(24, targetTop, 80, targetHeight)));

	[Theory]
	[InlineData(0, 100, 20, 20)]
	[InlineData(100, 0, 20, 20)]
	[InlineData(100, 100, 0, 20)]
	[InlineData(100, 100, 101, 20)]
	[InlineData(100, 100, 20, 101)]
	public void ScrollRejectsImpossibleGeometry(int width, int height, int targetWidth, int targetHeight) =>
		Assert.Throws<InvalidOperationException>(() => AppiumOperations.ScrollAmount(
			new Rectangle(10, 20, width, height), new Rectangle(10, 20, targetWidth, targetHeight)));

	[Theory]
	[InlineData(3)]
	[InlineData(207)]
	[InlineData(901)]
	public void ScrollTouchGestureStaysInsideChangingContainers(int height)
	{
		var container = new Rectangle(12, 610, 378, height);
		foreach (var direction in new[] { -1, 1 })
		{
			var amount = AppiumOperations.ScrollAmount(container, new Rectangle(24, 610 + direction * height * 2, 20, 1));
			var (start, end) = AppiumOperations.ScrollGesture(container, amount);
			Assert.True(container.Contains(start));
			Assert.True(container.Contains(end));
			Assert.Equal(amount, start.Y - end.Y);
		}
	}

	[Theory]
	[InlineData(841)]
	[InlineData(590)]
	[InlineData(1500)]
	public void ScrollFindsHiddenTargetsWithRepeatedGeometryChecks(int top)
	{
		var container = new Element(new Rectangle(12, 610, 378, 207));
		var target = new Element(new Rectangle(24, top, 80, 35), displayed: false);
		Assert.Same(target, ScrollTest(container, () => target, (bounds, amount) =>
		{
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = bounds.Contains(target.Bounds);
		}));
		Assert.True(container.Bounds.Contains(target.Bounds));
	}

	[Fact]
	public void ScrollWaitsForTransientPlaceholdersBeforeGesturing()
	{
		var container = new Element(new Rectangle(13, 372, 728, 169));
		var placeholder = new Element(new Rectangle(0, 0, 0, 0), displayed: false);
		var target = new Element(new Rectangle(34, 450, 686, 32));
		var probes = 0;
		Assert.Same(target, ScrollTest(container, () => ++probes < 3 ? placeholder : target,
			(_, _) => Assert.Fail("The target should settle before scrolling.")));
		Assert.Equal(3, probes);
	}

	[Fact]
	public void ScrollRevealsUnrealizedChildrenWithFreshBounds()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var target = new Element(new Rectangle(10, 150, 20, 20), displayed: false);
		var gestures = 0;
		Assert.Same(target, ScrollTest(container, () =>
		{
			if (gestures == 0)
				throw new NoSuchElementException("Offscreen child is not exposed.");
			return target;
		}, (bounds, amount) =>
		{
			gestures++;
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = bounds.Contains(target.Bounds);
		}));
		Assert.Equal(2, gestures);
	}

	[Fact]
	public void ScrollDiscoversMissingChildrenButCapsGestures()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var gestures = 0;
		var error = Assert.Throws<InvalidOperationException>(() => AppiumOperations.ScrollIntoView(
			() => container, () => throw new NoSuchElementException("Missing."),
			(_, _) => gestures++, "missing", TimeSpan.FromSeconds(5), 2, CancellationToken.None));
		Assert.Equal(2, gestures);
		Assert.Contains("exhausted 2 scrolls", error.Message);
	}

	[Fact]
	public void ScrollRejectsNoProgressAndDoesNotRetryGestureFailures()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var target = new Element(new Rectangle(10, 110, 20, 20), displayed: false);
		Assert.Contains("no progress", Assert.Throws<InvalidOperationException>(() =>
			ScrollTest(container, () => target, (_, _) => { })).Message);
		var failure = new StaleElementReferenceException("Gesture failed.");
		Assert.Same(failure, Assert.Throws<StaleElementReferenceException>(() =>
			ScrollTest(container, () => target, (_, _) => throw failure)));
	}

	[Fact]
	public void ScrollNeverAcceptsHiddenTargetsAndHonorsCancellation()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var target = new Element(new Rectangle(10, 10, 20, 20), displayed: false);
		Assert.Throws<TimeoutException>(() => AppiumOperations.ScrollIntoView(
			() => container, () => target, (_, _) => Assert.Fail("Contained target must not scroll."),
			"hidden", TimeSpan.FromMilliseconds(20), 2, CancellationToken.None));
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(() => AppiumOperations.ScrollIntoView(
			() => container, () => target, (_, _) => Assert.Fail("Cancelled."),
			"cancelled", TimeSpan.FromSeconds(5), 2, cancellation.Token));
	}

	private static IWebElement ScrollTest(Element container, Func<IWebElement> target, Action<Rectangle, int> scroll) =>
		AppiumOperations.ScrollIntoView(() => container, target, scroll, "'target' inside 'container'",
			TimeSpan.FromSeconds(5), 12, CancellationToken.None);

	[Fact]
	public void ClickRequeriesStaleElementsWithoutRetryingOtherDriverFailures()
	{
		var queries = 0;
		var clicks = 0;
		var stale = new Element(new Rectangle(0, 0, 100, 40),
			onClick: () => throw new StaleElementReferenceException("Menu replaced."));
		var current = new Element(new Rectangle(0, 0, 100, 40), onClick: () => clicks++);
		Assert.Same(current, AppiumOperations.ClickElement(
			() => ++queries == 1 ? stale : current, "CPU", TimeSpan.FromSeconds(2), CancellationToken.None));
		Assert.Equal(2, queries);
		Assert.Equal(1, clicks);
		var failure = new WebDriverException("Pointer command failed.");
		var broken = new Element(new Rectangle(0, 0, 100, 40), onClick: () => throw failure);
		Assert.Same(failure, Assert.Throws<WebDriverException>(() =>
			AppiumOperations.ClickElement(() => broken, "CPU", TimeSpan.FromSeconds(2), CancellationToken.None)));
	}

	[Fact]
	public void ClickNeverActsOnHiddenTargetsOrAfterCancellation()
	{
		var hidden = new Element(new Rectangle(0, 0, 100, 40), displayed: false,
			onClick: () => Assert.Fail("Hidden target."));
		Assert.Throws<TimeoutException>(() =>
			AppiumOperations.ClickElement(() => hidden, "CPU", TimeSpan.FromMilliseconds(20), CancellationToken.None));
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(() => AppiumOperations.ClickElement(
			() => throw new InvalidOperationException("Cancelled clicks must not query."),
			"CPU", TimeSpan.FromSeconds(2), cancellation.Token));
	}

	private sealed class Element(Rectangle bounds, bool displayed = true, Action? onClick = null) : IWebElement
	{
		internal Rectangle Bounds { get; set; } = bounds;
		public bool Displayed { get; set; } = displayed;
		public Point Location => Bounds.Location;
		public Size Size => Bounds.Size;
		public string TagName => throw new NotSupportedException();
		public string Text => throw new NotSupportedException();
		public bool Enabled => throw new NotSupportedException();
		public bool Selected => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public void Click() => (onClick ?? throw new NotSupportedException()).Invoke();
		public void SendKeys(string text) => throw new NotSupportedException();
		public void Submit() => throw new NotSupportedException();
		public string GetAttribute(string name) => throw new NotSupportedException();
		public string GetDomAttribute(string name) => throw new NotSupportedException();
		public string GetDomProperty(string name) => throw new NotSupportedException();
		public string GetCssValue(string name) => throw new NotSupportedException();
		public ISearchContext GetShadowRoot() => throw new NotSupportedException();
		public IWebElement FindElement(By by) => throw new NotSupportedException();
		public ReadOnlyCollection<IWebElement> FindElements(By by) => throw new NotSupportedException();
	}

	public void Dispose()
	{
		if (Directory.Exists(root))
			SampleWorkspace.DeleteDirectory(root);
	}
}
