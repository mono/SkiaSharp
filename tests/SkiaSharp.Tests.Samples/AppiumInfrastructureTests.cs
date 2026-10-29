using System.Collections.ObjectModel;
using System.Drawing;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using SkiaSharp.Tests.Shared;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests : IDisposable
{
	private readonly string root = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("appium-infrastructure"));

	private const string Target = "ios/arm64";
	private const string Folder = "Basic/Maui";
	private const string Project = "SkiaSharpSample/SkiaSharpSample.csproj";

	private static string AppOutput(string directory, string folder = Folder) =>
		Path.Combine(directory, "samples", folder, "SkiaSharpSample", "bin", "Debug", "net10.0-ios", "iossimulator-arm64");

	private static string FindApp(SampleWorkspace workspace) => IosRunningApp.FindApp(AppOutput(workspace.Root));

	private AppiumFixture Fixture(AppiumMode mode, string apps)
	{
		var inputs = Path.Combine(root, "inputs");
		var packages = Path.Combine(root, "packages");
		Directory.CreateDirectory(Path.Combine(inputs, Folder));
		Directory.CreateDirectory(packages);
		File.WriteAllText(Path.Combine(packages, "fixture.nupkg"), "Workspace package sentinel.");
		return new(mode, packages, apps, inputs);
	}

	private static string CreateApp(string directory, string folder = Folder)
	{
		var output = AppOutput(directory, folder);
		Directory.CreateDirectory(output);
		var app = Path.Combine(output, "Fixture.app");
		Directory.CreateDirectory(app);
		File.WriteAllText(Path.Combine(app, "Info.plist"), "fixture-plist");
		return app;
	}

	[Theory]
	[InlineData(null, "All")]
	[InlineData("all", "All")]
	[InlineData("build", "Build")]
	[InlineData("run", "Run")]
	public void AppiumPhasesAreExplicit(string? value, string expected) =>
		Assert.Equal(expected, AppiumFixture.ParseMode(value).ToString());

	[Theory]
	[InlineData("")]
	[InlineData("BUILD")]
	[InlineData("run-only")]
	public void AppiumRejectsInvalidPhases(string value) =>
		Assert.Throws<ArgumentException>(() => AppiumFixture.ParseMode(value));

	[Fact]
	public async Task BuildPhaseCleansOnceBuildsOnceAndRetainsOutputAfterDisposal()
	{
		var apps = Path.Combine(root, "sample-apps");
		var directory = Path.Combine(apps, Target, Folder);
		Directory.CreateDirectory(directory);
		var stale = Path.Combine(directory, "stale.txt");
		File.WriteAllText(stale, "stale");
		var builds = 0;
		Task Build(SampleWorkspace workspace)
		{
			Assert.Equal(directory, workspace.Root);
			Assert.False(File.Exists(stale));
			builds++;
			CreateApp(workspace.Root);
			return Task.CompletedTask;
		}
		var fixture = Fixture(AppiumMode.Build, apps);
		var first = await fixture.PrepareWorkspace(Target, Folder, Project, Build);
		await fixture.Run(new DiagnosticApp(first, FindApp(first), []),
			_ => throw new InvalidOperationException("Build phase must never invoke the scenario."));
		var second = await fixture.PrepareWorkspace(Target, Folder, Project, Build);
		Assert.Equal(1, builds);
		Assert.Equal(FindApp(first), FindApp(second));
		Assert.True(Directory.Exists(FindApp(second)));
		await Assert.ThrowsAsync<InvalidOperationException>(() =>
			fixture.PrepareWorkspace(Target, Folder, "other.csproj", Build));
		await fixture.DisposeAsync();
		Assert.True(Directory.Exists(FindApp(first)));
		File.WriteAllText(stale, "stale");
		await using var next = Fixture(AppiumMode.Build, apps);
		await next.PrepareWorkspace(Target, Folder, Project, Build);
		Assert.Equal(2, builds);
	}

	[Fact]
	public async Task RunPhaseUsesExistingOutputWithoutBuildingAndCleansOnlyAfterAllScenarios()
	{
		var apps = Path.Combine(root, "sample-apps");
		var directory = Path.Combine(apps, Target, Folder);
		var artifact = CreateApp(directory);
		var fixture = new AppiumFixture(AppiumMode.Run, "missing-packages", apps, "missing-sources");
		Task ForbiddenBuild(SampleWorkspace _) => throw new InvalidOperationException("Run must not build.");
		var first = await fixture.PrepareWorkspace(Target, Folder, Project, ForbiddenBuild);
		Assert.Equal(artifact, FindApp(first));
		Assert.False(File.Exists(Path.Combine(directory, "NuGet.Config")));
		var running = new DiagnosticApp(first, FindApp(first), []);
		await Assert.ThrowsAsync<NotSupportedException>(() => running.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask,
			() => Task.CompletedTask, () => Task.CompletedTask));
		Assert.True(Directory.Exists(artifact));
		var second = await fixture.PrepareWorkspace(Target, Folder, Project, ForbiddenBuild);
		Assert.Equal(artifact, FindApp(second));
		Assert.NotEqual(first.DiagnosticsRoot, second.DiagnosticsRoot);
		await fixture.DisposeAsync();
		Assert.False(Directory.Exists(directory));
		Assert.True(File.Exists(Path.Combine(running.Diagnostics, "failure.log")));
	}

	[Theory]
	[InlineData("missing-workspace")]
	[InlineData("missing-app")]
	[InlineData("ambiguous-app")]
	[InlineData("incomplete-bundle")]
	public async Task RunPhaseRejectsMissingOrAmbiguousAppsWithoutBuilding(string problem)
	{
		var apps = Path.Combine(root, "sample-apps");
		var directory = Path.Combine(apps, Target, Folder);
		if (problem == "missing-app")
			Directory.CreateDirectory(directory);
		if (problem == "ambiguous-app")
		{
			var app = CreateApp(directory);
			Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(app)!, "Second.app"));
		}
		if (problem == "incomplete-bundle")
			File.Delete(Path.Combine(CreateApp(directory), "Info.plist"));
		var fixture = Fixture(AppiumMode.Run, apps);
		await Assert.ThrowsAnyAsync<IOException>(async () =>
		{
			var workspace = await fixture.PrepareWorkspace(Target, Folder, Project,
				_ => throw new InvalidOperationException("Run must not build."));
			FindApp(workspace);
		});
		await fixture.DisposeAsync();
		Assert.False(Directory.Exists(directory));
	}

	[Theory]
	[InlineData("all")]
	[InlineData("run")]
	public async Task SeparateLocalBuildAndRunNeedNoArchiveOrManifest(string mode)
	{
		var apps = Path.Combine(root, "sample-apps");
		var builds = 0;
		Task Build(SampleWorkspace workspace)
		{
			builds++;
			CreateApp(workspace.Root);
			return Task.CompletedTask;
		}
		if (mode == "run")
		{
			await using var build = Fixture(AppiumMode.Build, apps);
			await build.PrepareWorkspace(Target, Folder, Project, Build);
		}
		var fixture = Fixture(AppiumFixture.ParseMode(mode), apps);
		var prepared = await fixture.PrepareWorkspace(Target, Folder, Project, Build);
		Assert.Equal(1, builds);
		Assert.True(Directory.Exists(FindApp(prepared)));
		Assert.Empty(Directory.GetFiles(apps, "manifest.json", SearchOption.AllDirectories));
		await fixture.DisposeAsync();
		Assert.False(Directory.Exists(prepared.Root));
	}

	[Theory]
	[InlineData("all")]
	[InlineData("build")]
	public async Task FailedBuildIsNotReusedAndRunModesStillClean(string mode)
	{
		var apps = Path.Combine(root, "sample-apps");
		var fixture = Fixture(AppiumFixture.ParseMode(mode), apps);
		await Assert.ThrowsAsync<IOException>(() => fixture.PrepareWorkspace(Target, Folder, Project, workspace =>
		{
			File.WriteAllText(Path.Combine(workspace.Root, "partial.txt"), "partial");
			throw new IOException("Build failed.");
		}));
		var prepared = await fixture.PrepareWorkspace(Target, Folder, Project, workspace =>
		{
			Assert.False(File.Exists(Path.Combine(workspace.Root, "partial.txt")));
			CreateApp(workspace.Root);
			return Task.CompletedTask;
		});
		await fixture.DisposeAsync();
		Assert.Equal(mode == "build", Directory.Exists(prepared.Root));
	}

	[Fact]
	public async Task SuccessfulBuildIsNotRepeatedWhenItsAppOutputIsMissing()
	{
		var apps = Path.Combine(root, "sample-apps");
		var directory = Path.Combine(apps, Target, Folder);
		var stale = CreateApp(directory);
		await using var fixture = Fixture(AppiumMode.All, apps);
		var builds = 0;
		var prepared = await fixture.PrepareWorkspace(Target, Folder, Project, _ =>
		{
			builds++;
			return Task.CompletedTask;
		});
		Assert.False(Directory.Exists(stale));
		Assert.Throws<FileNotFoundException>(() => FindApp(prepared));
		var reused = await fixture.PrepareWorkspace(Target, Folder, Project,
			_ => throw new InvalidOperationException("A successful build must not be repeated."));
		Assert.Throws<FileNotFoundException>(() => FindApp(reused));
		Assert.Equal(1, builds);
	}

	[Theory]
	[InlineData("../foreign")]
	[InlineData("/absolute")]
	[InlineData("folder\\project")]
	[InlineData("folder//project")]
	[InlineData("folder/./project")]
	public async Task PreparedPathsCannotEscapeTheSelectedApp(string path)
	{
		await using var fixture = Fixture(AppiumMode.Run, Path.Combine(root, "sample-apps"));
		await Assert.ThrowsAsync<ArgumentException>(() =>
			fixture.PrepareWorkspace(Target, path, Project, _ => Task.CompletedTask));
	}

	[Theory]
	[InlineData("../foreign", Project)]
	[InlineData("/absolute", Project)]
	[InlineData(Target, "../foreign.csproj")]
	[InlineData(Target, "/absolute.csproj")]
	public async Task PreparedTargetsAndProjectsCannotEscapeTheWorkspace(string target, string project)
	{
		await using var fixture = Fixture(AppiumMode.Run, Path.Combine(root, "sample-apps"));
		await Assert.ThrowsAsync<ArgumentException>(() =>
			fixture.PrepareWorkspace(target, Folder, project, _ => Task.CompletedTask));
	}

	[Fact]
	public async Task RunPhaseUsesIosBundleInPlaceWithoutCopyingLinksOrPermissions()
	{
		var apps = Path.Combine(root, "sample-apps");
		const string folder = "Basic/iOS";
		var source = CreateApp(Path.Combine(apps, Target, folder), folder);
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
		await using var fixture = Fixture(AppiumMode.Run, apps);
		var prepared = await fixture.PrepareWorkspace(Target, folder, Project,
			_ => throw new InvalidOperationException("Run must not build."));
		var bundle = IosRunningApp.FindApp(AppOutput(prepared.Root, folder));
		Assert.Equal(source, bundle);
		Assert.Equal("unit-fixture-executable", File.ReadAllText(Path.Combine(bundle, "Fixture")));
		if (!OperatingSystem.IsWindows())
		{
			Assert.Equal(mode, File.GetUnixFileMode(Path.Combine(bundle, "Fixture")));
			Assert.Equal("Fixture", new FileInfo(Path.Combine(bundle, "FixtureLink")).LinkTarget);
			Assert.Equal("A", new DirectoryInfo(Path.Combine(bundle, "Versions", "Current")).LinkTarget);
			Assert.Equal("unit-fixture-executable", File.ReadAllText(Path.Combine(bundle, "FixtureLink")));
		}
	}

	private static string Reference(string profile, string version, string name, string architecture = "arm64")
	{
		var resolved = SampleImage.Store.Resolve(new(
			profile,
			name,
			ReferenceVersion(version),
			architecture));
		Assert.NotNull(resolved.ReferenceLocation);
		return resolved.ReferenceLocation;
	}

	private static Version ReferenceVersion(string value)
	{
		var version = value.Contains('.', StringComparison.Ordinal)
			? Version.Parse(value)
			: new Version(int.Parse(value), 0, 0);
		return version.Build < 0 ? new Version(version.Major, version.Minor, 0) : version;
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

	[Fact]
	public async Task FailedSessionStillRunsDiagnosticsAndStopsTheServer()
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order);
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

	[Fact]
	public async Task DiagnosticsFailuresNeverHideStartupOrPreventServerCleanup()
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order, failDiagnostics: true);
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
		Assert.Equal("iphone-16-pro", profile);

		foreach (var family in new[] { "Maui", "Native" })
		{
			var name = AppiumSampleRunTestBase.CaseName("basic-cpu", family);
			var expected = Reference(profile, version, name);
			Assert.Equal(Path.Combine(SampleImage.Root, profile, ReferenceVersion(version).ToString(3), name + ".png"), expected);
		}
	}

	[Fact]
	public void IosGoldenProfileUsesAReviewedArchitectureOverride()
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", "26.2");

		var x64 = Reference(profile, "26.2", "maui-basic-cpu", "x64");
		var arm64 = Reference(profile, "26.2", "maui-basic-cpu", "arm64");

		Assert.EndsWith(Path.Combine("26.2.0", "maui-basic-cpu.x64.png"), x64);
		Assert.EndsWith(Path.Combine("26.2.0", "maui-basic-cpu.png"), arm64);
		Assert.True(File.Exists(x64));
		Assert.True(File.Exists(arm64));
	}

	[Theory]
	[InlineData("18.6", "not-reviewed")]
	[InlineData("99.0", "basic-cpu")]
	public void IosGoldensFailWithoutFallingBackToAnotherProfile(string version, string name)
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", version);
		var request = new GoldenCase(
			profile,
			AppiumSampleRunTestBase.CaseName(name, "Maui"),
			ReferenceVersion(version),
			"arm64");
		var expected = Path.Combine(
			SampleImage.Root,
			profile,
			request.Version!.ToString(3),
			request.CasePath + ".png");
		var actual = Reference("iphone-16-pro", "26.2", "maui-basic-cpu");
		Assert.True(File.Exists(actual));
		Assert.False(File.Exists(expected));

		// Missing references fail before image decoding and must never be created by a test.
		var failure = Assert.ThrowsAny<Exception>(() =>
			SampleImage.Store.Resolve(request).Verify(GoldenInput.Path(actual)));
		Assert.Contains("Missing reviewed golden", failure.Message);
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
		SampleWorkspace workspace, string artifact, List<string> order, bool failDiagnostics = false)
		: AppiumRunningApp(workspace, artifact, new IosAppiumOperations())
	{
		internal Exception StartFailure { get; } = new NotSupportedException("Session construction failed.");
		internal override string DeviceProfile => "unit-fixture";
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
		Assert.Equal(app.Reference("fixture"), operations.Reference("fixture"));
		Assert.Equal(app.ComparisonInsets(new(1206, 2622)), operations.ComparisonInsets(new(1206, 2622)));
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
		Assert.Throws<InvalidOperationException>(() => operations.Reference("fixture"));
		Assert.Throws<InvalidOperationException>(() => operations.ComparisonInsets(new(4, 5)));
		Assert.Throws<ArgumentNullException>(() => operations.Attach(null!));
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
	public void IosComparisonPolicyExcludesOnlyTheKnownSystemBottomBand()
	{
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ComparisonInsets(
			"iPhone 16 Pro", "18.6", new(1206, 2622)));
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ComparisonInsets(
			"iPhone 16 Pro", "26.2", new(1206, 2622)));
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ComparisonInsets(
			"iPhone 16 Pro", "26.3.1", new(1206, 2622)));
		Assert.Equal(new SKRectI(0, 0, 0, 39), IosRunningApp.ComparisonInsets(
			"iPhone 16 Pro", "26.3.2", new(1206, 2622)));
		Assert.Equal(SKRectI.Empty, IosRunningApp.ComparisonInsets(
			"iPhone 16", "26.2", new(1206, 2622)));
		Assert.Equal(SKRectI.Empty, IosRunningApp.ComparisonInsets(
			"iPhone 16 Pro", "26.2", new(1032, 2592)));

	}

	[Theory]
	[InlineData("18.6", true)]
	[InlineData("18.6", false)]
	[InlineData("26.2", true)]
	[InlineData("26.2", false)]
	[InlineData("26.3.1", true)]
	[InlineData("26.3.1", false)]
	public void DrawingChecksRejectEitherMissingStrokeAndIncompleteClear(
		string version, bool horizontal, string profile = "iphone-16-pro", string architecture = "arm64")
	{
		var drawn = Reference(profile, version, "maui-basic-drawing-two-lines", architecture);
		var blank = Reference(profile, version, "maui-basic-drawing", architecture);
		using var workspace = Workspace();
		using var drawnBitmap = SKBitmap.Decode(drawn);
		using var blankBitmap = SKBitmap.Decode(blank);
		Assert.NotNull(drawnBitmap);
		Assert.NotNull(blankBitmap);
		var insets = IosRunningApp.ComparisonInsets("iPhone 16 Pro", version, drawnBitmap.Info.Size);
		var drawnBytes = File.ReadAllBytes(drawn);
		var blankBytes = File.ReadAllBytes(blank);
		AppiumSampleRunTestBase.AssertDrawingStrokeRegions(drawn, drawnBytes, _ => insets);
		AppiumSampleRunTestBase.AssertDrawingStrokeRegions(blank, blankBytes, _ => insets);
		var width = drawnBitmap.Width - insets.Left - insets.Right;
		var height = drawnBitmap.Height - insets.Top - insets.Bottom;
		using var missingStroke = drawnBitmap.Copy();
		using var incompleteClear = blankBitmap.Copy();
		var region = horizontal
			? SKRectI.Round(new SKRect(insets.Left + width * 0.35f, insets.Top + height * 0.43f,
				insets.Left + width * 0.45f, insets.Top + height * 0.47f))
			: SKRectI.Round(new SKRect(insets.Left + width * 0.48f, insets.Top + height * 0.35f,
				insets.Left + width * 0.52f, insets.Top + height * 0.40f));
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
		Assert.ThrowsAny<Exception>(() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(missing, drawnBytes, _ => insets));
		Assert.ThrowsAny<Exception>(() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(residual, blankBytes, _ => insets));
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
