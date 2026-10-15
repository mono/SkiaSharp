using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Exercises discovery, isolation, cleanup, and diagnostics without building real samples.
[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public class SampleInfrastructureTests : IDisposable
{
    private readonly string root = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("infrastructure"));

    private void FileIn(string relative, string text = "<Project />")
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    [Fact]
    public async Task ProcessCaptureCanKeepDiscoveryOutputOutOfTestLogs()
    {
        var marker = Guid.NewGuid().ToString("N");
        var windows = OperatingSystem.IsWindows();
        var file = windows ? "capture.cmd" : "capture.sh";
        FileIn(file, windows ? $"@echo off\r\necho {marker}\r\n" : $"printf '%s' '{marker}'\n");
        var arguments = windows ? new[] { "/d", "/c", file } : new[] { file };
        var output = TestContext.Current.TestOutputHelper;
        Assert.NotNull(output);

        var captured = await ProcessRunner.Run(windows ? "cmd.exe" : "/bin/sh", arguments,
            root, TimeSpan.FromSeconds(10), publishOutput: false);
        Assert.Equal(0, captured.ExitCode);
        Assert.Contains(marker, captured.Output);
        Assert.DoesNotContain(marker, output.Output);

        var published = await ProcessRunner.Run(windows ? "cmd.exe" : "/bin/sh", arguments,
            root, TimeSpan.FromSeconds(10));
        Assert.Equal(0, published.ExitCode);
        Assert.Contains(marker, output.Output);
    }

    [Fact]
    public async Task QuietProcessTimeoutDoesNotDiscloseCapturedStreams()
    {
        var marker = Guid.NewGuid().ToString("N");
        var windows = OperatingSystem.IsWindows();
        var file = windows ? "capture.cmd" : "capture.sh";
        FileIn(file, windows
            ? $"@echo off\r\necho {marker}\r\nping -n 6 127.0.0.1 >nul\r\n"
            : $"printf '%s' '{marker}'\nsleep 5\n");
        var arguments = windows ? new[] { "/d", "/c", file } : new[] { file };
        var failure = await Assert.ThrowsAsync<TimeoutException>(() => ProcessRunner.Run(
            windows ? "cmd.exe" : "/bin/sh", arguments, root,
            TimeSpan.FromMilliseconds(500), publishOutput: false));
        Assert.DoesNotContain(marker, failure.Message);
        Assert.DoesNotContain(marker, TestContext.Current.TestOutputHelper!.Output);
    }

    [Theory]
    [InlineData("Windows", "Gallery.Windows.slnx", "windows.Dockerfile")]
    [InlineData("Mac", "Gallery.Mac.slnx", "linux.Dockerfile")]
    [InlineData("Linux", "Gallery.Linux.slnx", "linux.Dockerfile")]
    public void SelectsHostSolutionsAndDockerfiles(string platform, string solution, string dockerfile)
    {
        FileIn("Gallery/Gallery.slnx");
        foreach (var host in new[] { "Windows", "Mac", "Linux" })
            FileIn($"Gallery/Gallery.{host}.slnx");
        FileIn("Basic/Console/Console.slnx");
        FileIn("Basic/Docker/App.slnx");
        FileIn("Basic/Docker/linux.Dockerfile");
        FileIn("Basic/Docker/windows.Dockerfile");
        FileIn("Basic/Console/obj/Nested.slnx");
        var entries = SampleLookup.Discover(root, platform);
        Assert.Equal(4, entries.Count);
        Assert.Contains(new SampleLookup.Entry("Gallery", solution, SampleLookup.EntryKind.Gallery), entries);
        Assert.Contains(new SampleLookup.Entry(Path.Combine("Basic", "Console"), "Console.slnx",
            SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Host), entries);
        Assert.Contains(new SampleLookup.Entry(Path.Combine("Basic", "Docker"), "App.slnx", SampleLookup.EntryKind.Basic), entries);
        Assert.Contains(new SampleLookup.Entry(Path.Combine("Basic", "Docker"), dockerfile,
            SampleLookup.EntryKind.Basic | SampleLookup.EntryKind.Docker), entries);
    }

    [Theory]
    [InlineData("Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj")]
    [InlineData("Gallery/Maui/SkiaSharpSample.Maui.csproj")]
    public void MauiSourceProjectsDeclareOnlyHostSupportedFrameworks(string relativeProject)
    {
        var project = XDocument.Load(Path.Combine(Repo.RootDir, "samples", relativeProject));
        var frameworks = project.Descendants("TargetFrameworks").ToArray();
        Assert.Equal(3, frameworks.Length);
        Assert.Equal("net10.0-android", Assert.Single(frameworks,
            element => element.Attribute("Condition") is null).Value);
        Assert.Equal("net10.0-android;net10.0-ios;net10.0-maccatalyst", Assert.Single(frameworks,
            element => (string?)element.Attribute("Condition") == "$([MSBuild]::IsOSPlatform('OSX'))").Value);
        Assert.Equal("net10.0-android;net10.0-windows10.0.19041.0", Assert.Single(frameworks,
            element => (string?)element.Attribute("Condition") == "$([MSBuild]::IsOSPlatform('Windows'))").Value);
    }

    [Fact]
    public void NativeAndroidSourceProjectSupportsLinuxEmulatorArchitectures()
    {
        var project = XDocument.Load(Path.Combine(Repo.RootDir, "samples", "Basic", "Android",
            "SkiaSharpSample", "SkiaSharpSample.csproj"));
        var frameworks = Assert.Single(project.Descendants("TargetFrameworks"));
        Assert.Null(frameworks.Attribute("Condition"));
        Assert.Equal("net10.0-android", frameworks.Value);
        var runtimes = Assert.Single(project.Descendants("RuntimeIdentifiers"));
        Assert.Null(runtimes.Attribute("Condition"));
        Assert.Equal("android-arm;android-x86;android-arm64;android-x64", runtimes.Value);
    }

    [Theory]
    [InlineData("CpuPage.xaml", "basic-canvas-cpu")]
    [InlineData("GpuPage.xaml", "basic-canvas-gpu")]
    [InlineData("DrawingPage.xaml", "basic-canvas-drawing")]
    public void WindowsMauiCanvasIdentifiersUseExistingAccessibleHosts(string page, string id)
    {
        var document = XDocument.Load(Path.Combine(Repo.RootDir, "samples", "Basic", "Maui", "SkiaSharpSample", page));
        var canvas = Assert.Single(document.Descendants(),
            element => element.Name.LocalName is "SKCanvasView" or "SKGLView");
        Assert.Equal($"{{OnPlatform WinUI={id}}}", (string?)canvas.Parent!.Attribute("AutomationId"));
        Assert.Equal($"{{OnPlatform Default={id}, WinUI={id}-content}}", (string?)canvas.Attribute("AutomationId"));
    }

    [Fact]
    public void WindowsGalleryUsesTheExistingCanvasHost()
    {
        var document = XDocument.Load(Path.Combine(Repo.RootDir, "samples", "Gallery", "Maui", "SampleDetailPage.xaml"));
        XNamespace xaml = "http://schemas.microsoft.com/winfx/2009/xaml";
        var host = Assert.Single(document.Descendants(),
            element => (string?)element.Attribute(xaml + "Name") == "surfaceHost");
        Assert.Equal("Grid", host.Name.LocalName);
        Assert.Equal("{OnPlatform WinUI=sample-canvas-host}", (string?)host.Attribute("AutomationId"));
    }

    [Theory]
    [InlineData("Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj")]
    [InlineData("Gallery/Maui/SkiaSharpSample.Maui.csproj")]
    public void MauiSourceProjectsUseTheSameMauiPackage(string relativeProject)
    {
        var project = XDocument.Load(Path.Combine(Repo.RootDir, "samples", relativeProject));
        Assert.Equal("10.0.110", Assert.Single(project.Descendants("MauiVersion")).Value);
    }

    [Theory]
    [InlineData("Basic/iOS/SkiaSharpSample/SkiaSharpSample.csproj", "ios", "15.0")]
    [InlineData("Basic/MacCatalyst/SkiaSharpSample/SkiaSharpSample.csproj", "maccatalyst", "17.0")]
    [InlineData("Basic/macOS/SkiaSharpSample/SkiaSharpSample.csproj", "macos", "14.0")]
    [InlineData("Basic/tvOS/SkiaSharpSample/SkiaSharpSample.csproj", "tvos", "15.0")]
    [InlineData("Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj", "ios", "15.0")]
    [InlineData("Basic/Maui/SkiaSharpSample/SkiaSharpSample.csproj", "maccatalyst", "17.0")]
    [InlineData("Gallery/Maui/SkiaSharpSample.Maui.csproj", "ios", "15.0")]
    [InlineData("Gallery/Maui/SkiaSharpSample.Maui.csproj", "maccatalyst", "17.0")]
    public void AppleSourceProjectsMeetApprovedDeploymentFloors(string relativeProject, string platform, string minimum)
    {
        var projectPath = Path.Combine(Repo.RootDir, "samples", relativeProject);
        var project = XDocument.Load(projectPath);
        var declared = Assert.Single(project.Descendants("SupportedOSPlatformVersion"), element =>
            element.Attribute("Condition") is null ||
            element.Attribute("Condition")!.Value.Contains($"'-{platform}'", StringComparison.Ordinal));
        Assert.True(Version.Parse(declared.Value) >= Version.Parse(minimum));
        var maui = project.Descendants("UseMaui").Any(element => element.Value.Equals("true", StringComparison.OrdinalIgnoreCase));
        var manifestDirectory = maui
            ? Path.Combine("Platforms", platform == "ios" ? "iOS" : "MacCatalyst")
            : "";
        var manifest = XDocument.Load(Path.Combine(Path.GetDirectoryName(projectPath)!, manifestDirectory, "Info.plist"));
        var minimumKey = platform == "macos" ? "LSMinimumSystemVersion" : "MinimumOSVersion";
        var key = manifest.Root!.Element("dict")!.Elements("key").SingleOrDefault(element => element.Value == minimumKey);
        if (key is not null)
            Assert.Equal(Version.Parse(declared.Value), Version.Parse(key.ElementsAfterSelf().First().Value));
    }

    [Theory]
    [InlineData("Basic/iOS/SkiaSharpSample", false)]
    [InlineData("Basic/MacCatalyst/SkiaSharpSample", false)]
    [InlineData("Basic/Maui/SkiaSharpSample/Platforms/iOS", true)]
    [InlineData("Basic/Maui/SkiaSharpSample/Platforms/MacCatalyst", true)]
    [InlineData("Gallery/Maui/Platforms/iOS", true)]
    [InlineData("Gallery/Maui/Platforms/MacCatalyst", true)]
    public void AppleSourceManifestsBindTheCorrectSceneOwner(string relativeHead, bool maui)
    {
        var head = Path.Combine(Repo.RootDir, "samples", relativeHead);
        var root = XDocument.Load(Path.Combine(head, "Info.plist")).Root!.Element("dict")!;
        var manifest = PlistValue(root, "UIApplicationSceneManifest");
        Assert.Equal("false", PlistValue(manifest, "UIApplicationSupportsMultipleScenes").Name.LocalName);
        var configurations = PlistValue(manifest, "UISceneConfigurations");
        var configuration = Assert.Single(PlistValue(configurations, "UIWindowSceneSessionRoleApplication").Elements("dict"));
        Assert.Equal(maui ? "__MAUI_DEFAULT_SCENE_CONFIGURATION__" : "Default Configuration",
            PlistValue(configuration, "UISceneConfigurationName").Value);
        Assert.Equal("SceneDelegate", PlistValue(configuration, "UISceneDelegateClassName").Value);
        var source = File.ReadAllText(Path.Combine(head, "SceneDelegate.cs"));
        Assert.Contains("[Register(\"SceneDelegate\")]", source);
        Assert.Contains(maui ? "SceneDelegate : MauiUISceneDelegate" : "SceneDelegate : UIResponder, IUIWindowSceneDelegate", source);
        if (maui)
        {
            Assert.DoesNotContain("UISceneStoryboardFile", configuration.Descendants("key").Select(element => element.Value));
            Assert.Contains("AppDelegate : MauiUIApplicationDelegate", File.ReadAllText(Path.Combine(head, "AppDelegate.cs")));
        }
        else
        {
            Assert.Equal("Main", PlistValue(configuration, "UISceneStoryboardFile").Value);
            Assert.Equal("Main", PlistValue(root, "UIMainStoryboardFile").Value);
        }
    }

    private static XElement PlistValue(XElement dictionary, string key) =>
        Assert.Single(dictionary.Elements("key"), element => element.Value == key).ElementsAfterSelf().First();

    [Fact]
    public void NativeIosDefaultPageSelectionBelongsToItsStoryboardScene()
    {
        var head = Path.Combine(Repo.RootDir, "samples", "Basic", "iOS", "SkiaSharpSample");
        var app = File.ReadAllText(Path.Combine(head, "AppDelegate.cs"));
        Assert.DoesNotContain("FinishedLaunching", app);
        Assert.DoesNotContain("Window", app);
        Assert.Contains("SamplePage.Cpu", app);
        var scene = File.ReadAllText(Path.Combine(head, "SceneDelegate.cs"));
        Assert.Contains("[Export(\"scene:willConnectToSession:options:\")]", scene);
        Assert.Contains("NSRunLoop.Main.BeginInvokeOnMainThread", scene);
        Assert.Contains("Window?.RootViewController is UITabBarController tabs", scene);
        Assert.Contains("tabs.SelectedIndex = (nint)(int)AppDelegate.DefaultPage;", scene);
        var root = XDocument.Load(Path.Combine(head, "Info.plist")).Root!.Element("dict")!;
        Assert.Equal("LaunchScreen", PlistValue(root, "UILaunchStoryboardName").Value);
    }

    [Fact]
    public void DockerContextContainsSelectedSampleAndLocalPackagesOnly()
    {
        FileIn("inputs/Basic/DockerConsole/linux.Dockerfile", "FROM scratch");
        FileIn("inputs/Basic/DockerConsole/App.csproj");
        FileIn("inputs/Basic/DockerConsole/bin/App.dll");
        FileIn("inputs/Basic/DockerConsole/obj/assets.json");
        FileIn("inputs/Basic/Web/App.csproj");
        FileIn("packages/nested/SkiaSharp.1.2.3.nupkg", "package sentinel");
        FileIn("packages/not-a-package.txt");
        var context = Path.Combine(root, "context");

        DockerSampleTestBase.PrepareContext(Path.Combine(root, "inputs", "Basic", "DockerConsole"),
            Path.Combine(root, "packages"), context);

        Assert.True(File.Exists(Path.Combine(context, "linux.Dockerfile")));
        Assert.True(File.Exists(Path.Combine(context, "App.csproj")));
        Assert.False(Directory.Exists(Path.Combine(context, "bin")));
        Assert.False(Directory.Exists(Path.Combine(context, "obj")));
        Assert.False(Directory.Exists(Path.Combine(context, "Basic")));
        Assert.Equal("package sentinel", File.ReadAllText(Path.Combine(context, "packages", "SkiaSharp.1.2.3.nupkg")));
        Assert.Single(Directory.EnumerateFiles(Path.Combine(context, "packages")));
        var config = XDocument.Load(Path.Combine(context, "nuget.config"));
        var artifacts = Assert.Single(config.Descendants("packageSources").Elements("add"),
            source => (string?)source.Attribute("key") == "artifacts");
        Assert.Equal("packages", (string?)artifacts.Attribute("value"));
        Assert.False(File.Exists(Path.Combine(context, "global.json")));
    }

    [Fact]
    public void DockerContextRejectsAnEmptyPackageCohort()
    {
        FileIn("inputs/App.csproj");
        Directory.CreateDirectory(Path.Combine(root, "packages"));
        Assert.Throws<Xunit.Sdk.NotEmptyException>(() =>
            DockerSampleTestBase.PrepareContext(Path.Combine(root, "inputs"),
                Path.Combine(root, "packages"), Path.Combine(root, "context")));
    }

    [Fact]
    public void FiltersAndMissingInputsFailExplicitly()
    {
        FileIn("Console/Console.slnx");
        FileIn("Console/Console.Core.slnx");
        Assert.Equal(2, SampleLookup.Discover(root, "Mac", "Console").Count);
        Assert.Throws<InvalidOperationException>(() => SampleLookup.Discover(root, "Mac", "Missing"));
        Assert.Throws<DirectoryNotFoundException>(() => SampleLookup.Discover(Path.Combine(root, "Missing"), "Mac"));
    }

    [Fact]
    public void ScopedCopyKeepsAncestorsAndGallerySiblings()
    {
        FileIn("inputs/Directory.Build.props");
        FileIn("inputs/NuGet.Config");
        FileIn("inputs/Basic/Directory.Build.targets");
        FileIn("inputs/Basic/Console/App.csproj");
        FileIn("inputs/Basic/Console/obj/assets.json");
        FileIn("inputs/Basic/Web/App.csproj");
        FileIn("inputs/Gallery/App/App.csproj");
        FileIn("inputs/Gallery/Shared/Shared.csproj");
        var source = Path.Combine(root, "inputs");
        var copy = Path.Combine(root, "console-copy");
        SampleWorkspace.CopySample(source, Path.Combine("Basic", "Console"), copy);
        Assert.True(File.Exists(Path.Combine(copy, "Basic", "Console", "App.csproj")));
        Assert.True(File.Exists(Path.Combine(copy, "Directory.Build.props")));
        Assert.True(File.Exists(Path.Combine(copy, "NuGet.Config")));
        Assert.True(File.Exists(Path.Combine(copy, "Basic", "Directory.Build.targets")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Basic", "Console", "obj")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Basic", "Web")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Gallery")));
        copy = Path.Combine(root, "gallery-copy");
        SampleWorkspace.CopySample(source, "Gallery", copy);
        Assert.True(File.Exists(Path.Combine(copy, "Gallery", "Shared", "Shared.csproj")));
        Assert.False(Directory.Exists(Path.Combine(copy, "Basic")));
    }

    [Theory]
    [InlineData("")]
    [InlineData("../outside")]
    public void RejectsEscapingFolders(string folder) =>
        Assert.Throws<ArgumentException>(() => SampleWorkspace.CopySample(root, folder, Path.Combine(root, "copy")));

    [Fact]
    public void IdentitiesAreUtcSortableUniqueAndPathSafeAcrossCulturesAndConcurrentCalls()
    {
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            var identities = Enumerable.Range(0, 1000)
                .Select(index => SampleWorkspace.CreateIdentity(index % 2 == 0 ? "Basic/Console" : "App"))
                .ToArray();
            Assert.Equal(identities, identities.Order(StringComparer.Ordinal));
            foreach (var identity in identities)
            {
                Assert.Matches(@"^[0-9]{8}T[0-9]{13}Z-[a-z0-9-]+-[a-f0-9]+-[a-f0-9]+$", identity);
                var timestamp = DateTime.ParseExact(identity[..23], "yyyyMMdd'T'HHmmssfffffff'Z'",
                    CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                Assert.Equal(DateTimeKind.Utc, timestamp.Kind);
            }
            var concurrent = new string[1000];
            Parallel.For(0, concurrent.Length, index => concurrent[index] = SampleWorkspace.CreateIdentity("Basic/Console"));
            Assert.Equal(identities.Length + concurrent.Length,
                identities.Concat(concurrent).Distinct(StringComparer.Ordinal).Count());
            Assert.Contains("-basic-console-", identities[0]);
            Assert.Throws<ArgumentException>(() => SampleWorkspace.CreateIdentity(""));
        }
        finally
        {
            CultureInfo.CurrentCulture = culture;
        }
    }

    [Fact]
    public async Task RunningAppsKeepSortableDistinctLogPairsInTheirOwnedDirectory()
    {
        var diagnostics = Path.Combine(root, "run");
        FileIn("other-run/keep.txt", "Keep this run.");
        await new AddressRunningApp(diagnostics).Run(() => Task.CompletedTask);
        var first = Assert.Single(Directory.EnumerateFiles(diagnostics, "*.stdout.log"));
        await new AddressRunningApp(diagnostics).Run(() => Task.CompletedTask);
        var outputs = Directory.EnumerateFiles(diagnostics, "*.stdout.log").Order(StringComparer.Ordinal).ToArray();
        Assert.Equal(2, outputs.Length);
        Assert.Equal(first, outputs[0]);
        Assert.Equal(4, Directory.EnumerateFiles(diagnostics).Count());
        foreach (var output in outputs)
        {
            Assert.Matches(@"^[0-9]{8}T[0-9]{13}Z-run-[a-f0-9]+-[a-f0-9]+\.stdout\.log$", Path.GetFileName(output));
            Assert.True(File.Exists(output.Replace(".stdout.log", ".stderr.log", StringComparison.Ordinal)));
        }
        Assert.Equal("Keep this run.", File.ReadAllText(Path.Combine(root, "other-run", "keep.txt")));
    }

    [Fact]
    public void RejectsSampleLocalSdkPins()
    {
        FileIn("inputs/Basic/App/global.json", "{}");
        Assert.Throws<InvalidOperationException>(() =>
            SampleWorkspace.CopySample(Path.Combine(root, "inputs"), "Basic", Path.Combine(root, "copy")));
    }

    [Fact]
    public void FailedPreparationCleansOnlyItsOwnedWorkspace()
    {
        FileIn("inputs/App/global.json", "{}");
        FileIn("packages/input.nupkg", "");
        FileIn("workspaces/other-test/keep.txt", "Keep this workspace.");
        Assert.Throws<InvalidOperationException>(() => new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Path.Combine(root, "packages"),
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics")));
        Assert.Equal(Path.Combine(root, "workspaces", "other-test"),
            Assert.Single(Directory.EnumerateDirectories(Path.Combine(root, "workspaces"))));
        Assert.True(File.Exists(Path.Combine(root, "workspaces", "other-test", "keep.txt")));
        Assert.True(File.Exists(Path.Combine(root, "inputs", "App", "global.json")));
    }

    [Fact]
    public async Task ConsumerUsesTheHostSdkWithoutRepositoryPins()
    {
        FileIn("repository/global.json", """
            {"sdk":{"version":"1.2.345","paths":[".dotnet","$host$"],"workloadVersion":"1.2.345"},
             "tools":{"dotnet":"1.2.345"},"msbuild-sdks":{"Example.Sdk":"2.3.456"}}
            """);
        FileIn("repository/output/nugets/input.nupkg");
        FileIn("repository/output/samples/Basic/Console/App.slnx");
        var repository = Path.Combine(root, "repository");
        using var workspace = new SampleWorkspace(
            Path.Combine("Basic", "Console"),
            Path.Combine(repository, "output", "samples"),
            Path.Combine(repository, "output", "nugets"),
            Path.Combine(repository, "output", "samples-test-workspaces"),
            Path.Combine(root, "diagnostics"), sdkVersion: null);
        var working = workspace.Root;
        Assert.Contains("-basic-console-", Path.GetFileName(working));
        Assert.Equal(Path.GetFileName(working), Path.GetFileName(workspace.DiagnosticsRoot));
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(working, "global.json")));
        Assert.Single(json.RootElement.EnumerateObject());
        var sdk = json.RootElement.GetProperty("sdk");
        Assert.False(sdk.TryGetProperty("version", out _));
        Assert.False(sdk.TryGetProperty("workloadVersion", out _));
        Assert.Equal("$host$", sdk.GetProperty("paths")[0].GetString());
        Assert.True(sdk.GetProperty("allowPrerelease").GetBoolean());
        foreach (var file in new[] { "Directory.Build.props", "Directory.Build.targets" })
            Assert.Empty(XDocument.Load(Path.Combine(working, file)).Root!.Elements());
        var start = new ProcessStartInfo();
        start.Environment["MSBUILD_EXAMPLE"] = "pollution";
        var path = start.Environment["PATH"];
        DotNet.ConfigureProcess(start, workspace);
        Assert.False(start.Environment.ContainsKey("MSBUILD_EXAMPLE"));
        Assert.Equal(path, start.Environment["PATH"]);
        foreach (var key in new[] { "NUGET_PACKAGES", "NUGET_HTTP_CACHE_PATH", "NUGET_SCRATCH", "DOTNET_CLI_HOME" })
            Assert.StartsWith(working + Path.DirectorySeparatorChar, start.Environment[key]);
        var selected = await DotNet.GetVersion(workspace);
        Assert.NotEqual("1.2.345", selected);
        Assert.Matches(@"^\d+\.\d+\.\d+", selected);
        Directory.CreateDirectory(workspace.DiagnosticsRoot);
        workspace.Dispose();
        Assert.False(Directory.Exists(working));
        Assert.True(Directory.Exists(workspace.DiagnosticsRoot));
    }

    [Fact]
    public async Task ConsumerSdkPinCannotRollForwardOrFallBack()
    {
        FileIn("inputs/App/App.csproj");
        FileIn("packages/input.nupkg");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Path.Combine(root, "packages"),
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"), "1.2.345");
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.Root, "global.json")));
        var sdk = json.RootElement.GetProperty("sdk");
        Assert.Equal("1.2.345", sdk.GetProperty("version").GetString());
        Assert.Equal("disable", sdk.GetProperty("rollForward").GetString());
        Assert.True(sdk.GetProperty("allowPrerelease").GetBoolean());
        Assert.Equal("$host$", sdk.GetProperty("paths")[0].GetString());
        Assert.False(sdk.TryGetProperty("workloadVersion", out _));
        await Assert.ThrowsAsync<InvalidOperationException>(() => DotNet.GetVersion(workspace));
    }

    [Fact]
    public void ConsumerSdkSelectionAndRootFrameworkSelectionPreserveProjectDeclarations()
    {
        const string project = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """;
        FileIn("inputs/App/App.csproj", project);
        FileIn("packages/input.nupkg");
        using var baseline = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Path.Combine(root, "packages"),
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"), "10.0.401");
        using var preview = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Path.Combine(root, "packages"),
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"), "11.0.100-rc.1.26425.128");
        foreach (var workspace in new[] { baseline, preview })
        {
            var ownedProject = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
            var arguments = DotNet.BuildArguments(workspace, ownedProject, workspace.DiagnosticsRoot,
                new DotNetOptions { Framework = "net10.0" });
            Assert.Contains("--framework", arguments);
            Assert.DoesNotContain(arguments, argument => argument.StartsWith("-p:TargetFramework", StringComparison.Ordinal));
            Assert.Equal(project, File.ReadAllText(ownedProject));
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(workspace.Root, "global.json")));
            Assert.Equal(workspace.SdkVersion, json.RootElement.GetProperty("sdk").GetProperty("version").GetString());
            Assert.Equal("disable", json.RootElement.GetProperty("sdk").GetProperty("rollForward").GetString());
        }
        Assert.Equal(project, File.ReadAllText(Path.Combine(root, "inputs", "App", "App.csproj")));
    }

    [Fact]
    public void ArtifactPackagesCannotFallBackToPublicFeeds()
    {
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "NuGet.Config");
        DotNet.WriteNuGetConfig(file, "packages");
        var mappings = XDocument.Load(file).Root!.Element("packageSourceMapping")!;
        var artifact = Assert.Single(mappings.Elements("packageSource"), element => (string?)element.Attribute("key") == "artifacts");
        Assert.Equal(new[] { "SkiaSharp*", "HarfBuzzSharp*" },
            artifact.Elements("package").Select(element => (string?)element.Attribute("pattern")));
    }

    [Fact]
    public async Task HttpRequestsUseTheOwnedOriginAndPreserveQueries()
    {
        var app = new AddressRunningApp(Path.Combine(root, "http"));
        await app.Run(async () =>
        {
            Assert.Equal("http://127.0.0.1:49152/api/images/SkiaSharp?x=1",
                (await app.RequestUri("/api/images/SkiaSharp?x=1")).AbsoluteUri);
            Assert.Equal("http://127.0.0.1:49152/health",
                (await app.RequestUri("/health")).AbsoluteUri);
            Assert.Equal(1, app.AddressRequests);
        });
    }

    [Fact]
    public async Task HttpContentCanTakeLongerThanAReadinessProbe()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var address = new Uri($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}");
        var server = Respond();
        try
        {
            var app = new AddressRunningApp(Path.Combine(root, "slow-http")) { Address = address };
            await app.Run(async () =>
            {
                using var response = await app.GetResponse("/api/images");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("OK", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
            });
        }
        finally
        {
            listener.Stop();
            await server;
        }

        async Task Respond()
        {
            using var connection = await listener.AcceptTcpClientAsync(TestContext.Current.CancellationToken);
            await using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, leaveOpen: true);
            while (!string.IsNullOrEmpty(await reader.ReadLineAsync(TestContext.Current.CancellationToken))) { }
            await Task.Delay(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            await stream.WriteAsync("HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nOK"u8.ToArray(),
                TestContext.Current.CancellationToken);
        }
    }

    [Theory]
    [InlineData("Now listening on: http://127.0.0.1:54321", "http://127.0.0.1:54321/")]
    [InlineData("  Now listening on: http://127.0.0.1:54321  ", "http://127.0.0.1:54321/")]
    [InlineData("App url: http://127.0.0.1:60542/?arg=--urls&arg=http%3a%2f%2f127.0.0.1%3a0",
        "http://127.0.0.1:60542/?arg=--urls&arg=http%3a%2f%2f127.0.0.1%3a0")]
    public void DotNetRecognizesOwnedHttpAddress(string output, string expected) =>
        Assert.Equal(new Uri(expected), DotNetRunningApp.ParseAddress(output));

    [Theory]
    [InlineData("App url: https://127.0.0.1:60545/")]
    [InlineData("App url: http://example.com:54321/")]
    [InlineData("App url: http://127.0.0.1:0/")]
    [InlineData("App url: not-an-address")]
    [InlineData("Debug at url: http://127.0.0.1:60542/_framework/debug")]
    public void DotNetRejectsOtherAddresses(string output) =>
        Assert.Null(DotNetRunningApp.ParseAddress(output));

    [Theory]
    [InlineData("https://localhost/health")]
    [InlineData("//example.com/health")]
    [InlineData("/\\example.com/health")]
    [InlineData("/health#fragment")]
    [InlineData("")]
    public async Task HttpRequestsCannotEscapeTheOwnedOrigin(string path)
    {
        var app = new AddressRunningApp(Path.Combine(root, "http"));
        await app.Run(() => Assert.ThrowsAsync<ArgumentException>(() => app.RequestUri(path)));
    }

    [Fact]
    public void SmallAttachmentsArePublishedAndLargeFilesRemainOnDisk()
    {
        FileIn("small.txt", "Sample diagnostic attachment.");
        Assert.NotNull(TestContext.Current.TestOutputHelper);
        TestContext.Current.AddFileAttachment(Path.Combine(root, "small.txt"), "text/plain");
        using (var file = File.Create(Path.Combine(root, "large.binlog")))
            file.SetLength(SampleArtifacts.AttachmentLimit + 1);
        TestContext.Current.AddFileAttachment(Path.Combine(root, "large.binlog"), "application/octet-stream");
        Assert.True(File.Exists(Path.Combine(root, "large.binlog")));
    }

    [Fact]
    public void RepeatedAttachmentsPreserveEachDiagnosticSnapshot()
    {
        FileIn("snapshot.txt", "First diagnostic.");
        var path = Path.Combine(root, "snapshot.txt");
        TestContext.Current.AddFileAttachment(path, "text/plain");
        File.WriteAllText(path, "Second diagnostic.");
        TestContext.Current.AddFileAttachment(path, "text/plain");
        TestContext.Current.AddFileAttachment(path, "text/plain", "snapshot.txt");
        Assert.Contains("snapshot.txt", TestContext.Current.Attachments!.Keys);
        Assert.Contains("snapshot.txt-2", TestContext.Current.Attachments.Keys);
        Assert.Contains("snapshot.txt-3", TestContext.Current.Attachments.Keys);
    }

    [Fact]
    public void GoldenMismatchKeepsActualAndExpectedAttachmentsDistinct()
    {
        var actual = Path.Combine(root, "actual", "canvas.png");
        var expected = Path.Combine(root, "expected", "canvas.png");
        Save(actual, SKColors.Red);
        Save(expected, SKColors.Blue);
        TestContext.Current.AddFileAttachment(actual, "image/png");
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("Sample pixels differ", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        Assert.Contains("canvas.png-2", TestContext.Current.Attachments!.Keys);
        Assert.Contains("expected-canvas.png", TestContext.Current.Attachments.Keys);

        static void Save(string path, SKColor color)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var bitmap = new SKBitmap(4, 4);
            bitmap.Erase(color);
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(path, data.ToArray());
        }
    }

    [Fact]
    public async Task ImageMustDecodeAndHaveTheExpectedSize()
    {
        using var bitmap = new SKBitmap(800, 600);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var expected = SavePng(bitmap, "decode.expected.png");
        var options = new SampleImageOptions { ExpectedSize = new(800, 600) };
        await SampleImage.Validate(data.ToArray(), Path.Combine(root, "decode.actual.png"), expected, options);
        await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(data.ToArray().AsSpan(0, 12).ToArray(),
                Path.Combine(root, "invalid.actual.png"), expected, options));
        var invalidSignature = Path.Combine(root, "invalid-signature.actual.png");
        var error = await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(new byte[] { 1, 2, 3 }, invalidSignature, expected, options));
        Assert.Contains("Expected a PNG image", error.Message);
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(invalidSignature));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(data.ToArray(), Path.Combine(root, "wrong-size.actual.png"), expected,
                options with { ExpectedSize = new(400, 300) }));
    }

    [Theory]
    [InlineData(80, 50, 0, true)]
    [InlineData(80, 50, 3, true)]
    [InlineData(80, 50, 4, false)]
    [InlineData(80, 51, 3, true)]
    [InlineData(80, 51, 4, false)]
    [InlineData(800, 600, 259, true)]
    [InlineData(800, 600, 360, true)]
    [InlineData(800, 600, 361, false)]
    [InlineData(512, 512, 177, true)]
    [InlineData(512, 512, 196, true)]
    [InlineData(512, 512, 197, false)]
    public void GoldenComparisonAllowsAtMostPointZeroSevenFivePercentOfPixels(
        int width, int height, int changedPixels, bool accepted)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i % width, i / width, SKColors.White);
        var actual = SavePng(bitmap, "actual.png");

        if (accepted)
        {
            SampleImage.Validate(actual, expected);
            Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
        else
        {
            var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected));
            Assert.Contains($"{changedPixels}/{width * height} pixels", error.Message);
            Assert.Contains("exceeding", error.Message);
            Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
        }
    }

    [Theory]
    [InlineData(1, 0, 0, 255)]
    [InlineData(0, 0, 0, 254)]
    public void GoldenComparisonCountsEvenSingleChannelAndAlphaChanges(byte red, byte green, byte blue, byte alpha)
    {
        using var bitmap = new SKBitmap(80, 50);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "expected.png");
        bitmap.Erase(new SKColor(red, green, blue, alpha));
        var actual = SavePng(bitmap, "actual.png");

        var error = Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("4000/4000 pixels", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    private string SavePng(SKBitmap bitmap, string name)
    {
        Directory.CreateDirectory(root);
        var path = Path.Combine(root, name);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    [Fact]
    public async Task ImageOptionsApplyToSavedFilesAndDirectComparisonsWithoutMutation()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "options.expected.png");
        var actual = Path.Combine(root, "options.actual.png");
        var options = new SampleImageOptions { ExpectedSize = new(16, 12) };

        await SampleImage.Validate(File.ReadAllBytes(expected), actual, expected, options);
        SampleImage.Validate(actual, expected, options);
        Assert.Contains("options.actual.png-2", TestContext.Current.Attachments!.Keys);
        Assert.Equal(new SKSizeI(16, 12), options.ExpectedSize);
        Assert.Equal(0.00075m, options.MaxErrorPixelFraction);
        Assert.Null(options.MaxAverageColorErrorFraction);

        var wrongSize = options with { ExpectedSize = new(15, 12) };
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected, wrongSize));
        await Assert.ThrowsAnyAsync<Exception>(() =>
            SampleImage.Validate(File.ReadAllBytes(expected),
                Path.Combine(root, "wrong-size.actual.png"), expected, wrongSize));
        Assert.Equal(new SKSizeI(16, 12), options.ExpectedSize);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    [InlineData(double.NaN)]
    public void ImageOptionsRejectInvalidAverageColorErrorLimits(double limit)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "invalid-options.expected.png");
        var actual = SavePng(bitmap, "invalid-options.actual.png");
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected,
            new() { MaxAverageColorErrorFraction = limit }));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    public void ImageOptionsApplyAnExplicitPixelBudget(int changedPixels, bool accepted)
    {
        using var bitmap = new SKBitmap(10, 10);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "pixel-options.expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i, 0, SKColors.White);
        var actual = SavePng(bitmap, "pixel-options.actual.png");
        var options = new SampleImageOptions { MaxErrorPixelFraction = 0.01m };
        if (accepted)
            SampleImage.Validate(actual, expected, options);
        else
            Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected, options));
        Assert.Equal(0.00075m, new SampleImageOptions().MaxErrorPixelFraction);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void ImageOptionsRejectInvalidPixelBudgets(int limit)
    {
        using var bitmap = new SKBitmap(4, 4);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "invalid-pixel-options.expected.png");
        var actual = SavePng(bitmap, "invalid-pixel-options.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxErrorPixelFraction = limit }));
    }

    [Fact]
    public void GoldenComparisonSupportsPageSizesAndRetainsPixelDifferences()
    {
        Directory.CreateDirectory(root);
        var actual = Path.Combine(root, "page.actual.png");
        var expected = Path.Combine(root, "page.expected.png");
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        File.Copy(actual, expected);
        SampleImage.Validate(actual, expected);

        bitmap.Erase(SKColors.Blue);
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("192/192", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void MissingReviewedGoldenFailsWithoutCreatingAReference()
    {
        Directory.CreateDirectory(root);
        var actual = Path.Combine(root, "missing-reference.actual.png");
        var expected = Path.Combine(root, "missing-reference.png");
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(actual, data.ToArray());
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("Missing reviewed sample golden", error.Message);
        Assert.True(File.Exists(actual));
        Assert.False(File.Exists(expected));
    }

    [Fact]
    public void AverageColorErrorAllowsExactlyFivePercentButRejectsOneChannelStepOver()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "average.expected.png");
        bitmap.Erase(new SKColor(238, 17, 17));
        var actual = SavePng(bitmap, "average.actual.png");
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });
        Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));

        bitmap.SetPixel(0, 0, new SKColor(238, 18, 17));
        SavePng(bitmap, "average.actual.png");
        var error = Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 }));
        Assert.Contains("average RGBA color error", error.Message);
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void AverageColorErrorDoesNotChangeTheHeadlessPixelBudgetOrExplicitExactComparison()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "channel.expected.png");
        bitmap.Erase(new SKColor(247, 8, 8));
        var actual = SavePng(bitmap, "channel.actual.png");
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });
        var error = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
        Assert.Contains("192/192", error.Message);
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0 }));
    }

    [Fact]
    public void AverageColorErrorIncludesAlphaInItsFivePercentBoundary()
    {
        using var bitmap = new SKBitmap(16, 12);
        bitmap.Erase(SKColors.Black);
        var expected = SavePng(bitmap, "alpha.expected.png");
        bitmap.Erase(new SKColor(0, 0, 0, 204));
        var actual = SavePng(bitmap, "alpha.actual.png");
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 });

        bitmap.SetPixel(0, 0, new SKColor(0, 0, 0, 203));
        SavePng(bitmap, "alpha.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.05 }));
    }

    [Fact]
    public void AverageColorErrorAllowsExactlyTheGpuSixPercentBudget()
    {
        using var bitmap = new SKBitmap(5, 1);
        bitmap.Erase(SKColors.Red);
        var expected = SavePng(bitmap, "gpu-budget.expected.png");
        bitmap.Erase(new SKColor(234, 20, 20));
        bitmap.SetPixel(0, 0, new SKColor(233, 20, 20));
        var actual = SavePng(bitmap, "gpu-budget.actual.png");
        SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.06 });

        bitmap.SetPixel(1, 0, new SKColor(233, 20, 20));
        SavePng(bitmap, "gpu-budget.actual.png");
        Assert.ThrowsAny<Exception>(() =>
            SampleImage.Validate(actual, expected, new() { MaxAverageColorErrorFraction = 0.06 }));
    }

    [Fact]
    public void DotNetSampleTestBaseOwnsOnlyItsPreparedWorkspace()
    {
        FileIn("inputs/Basic/Console/App.slnx");
        string working;
        using (var test = new LifetimeTest(Path.Combine(root, "inputs")))
        {
            working = test.Prepare();
            Assert.Equal(working, test.Prepare());
            Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "App.slnx")));
        }
        Assert.False(Directory.Exists(working));
        Assert.True(Directory.Exists(Path.Combine(root, "inputs")));
    }

    private sealed class LifetimeTest(string samplesDir) : DotNetSampleTestBase(samplesDir)
    {
        internal string Prepare() => PrepareSample(Path.Combine("Basic", "Console")).Root;
        internal string Diagnostics => PrepareSample(Path.Combine("Basic", "Console")).DiagnosticsRoot;
        internal SampleWorkspace Workspace => PrepareSample(Path.Combine("Basic", "Console"));
        internal Task Build(DotNetOptions? options = null) =>
            BuildSample(Path.Combine("Basic", "Console"), "App.csproj", options);
    }

    [Fact]
    public async Task DotNetSampleTestBaseBuildsWithRetainedDiagnostics()
    {
        FileIn("inputs/Basic/Console/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
            </Project>
            """);
        using var test = new LifetimeTest(Path.Combine(root, "inputs"));
        await test.Build();
        var working = test.Prepare();
        var diagnostics = Path.Combine(test.Diagnostics, "builds", "Basic", "Console", "App", "Release");
        test.Dispose();
        Assert.False(Directory.Exists(working));
        Assert.Contains("Build succeeded.", File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log")));
        Assert.DoesNotContain("Build succeeded.", File.ReadAllText(Path.Combine(diagnostics, "build.stderr.log")));
        Assert.True(new FileInfo(Path.Combine(diagnostics, "build.binlog")).Length > 0);
    }

    [Fact]
    public async Task ExplicitFrameworkBuildKeepsDeclaredSharedReferencesAndFrameworkDiagnostics()
    {
        FileIn("inputs/Basic/Console/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFrameworks>net10.0;net10.0-windows</TargetFrameworks>
                <EnableWindowsTargeting>true</EnableWindowsTargeting>
                <OutputType>Exe</OutputType>
              </PropertyGroup>
              <Target Name="VerifySelectedFramework" BeforeTargets="CoreCompile">
                <Error Condition="'$(TargetFramework)' != 'net10.0-windows' or '$(TargetFrameworks)' != 'net10.0;net10.0-windows'"
                       Text="Build must select the declared root framework without rewriting its framework list." />
                <Error Condition="'$(ContractSentinel)' != 'forwarded-by-buildsample'"
                       Text="BuildSample must forward the test-owned property dictionary." />
                <Message Importance="high" Text="Selected $(TargetFramework) from $(TargetFrameworks)" />
                <Message Importance="high" Text="Contract sentinel: $(ContractSentinel)" />
                <Message Importance="high" Text="Build working directory: $(MSBuildStartupDirectory)" />
              </Target>
              <ItemGroup>
                <ProjectReference Include="Library/Library.csproj" />
                <ProjectReference Include="Shared/Shared.csproj" />
                <Compile Remove="Library/**/*.cs" />
                <Compile Remove="Shared/**/*.cs" />
              </ItemGroup>
            </Project>
            """);
        FileIn("inputs/Basic/Console/Library/Library.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>netstandard2.1</TargetFramework></PropertyGroup>
              <Target Name="VerifyLibraryFramework" BeforeTargets="CoreCompile">
                <Error Condition="'$(TargetFramework)' != 'netstandard2.1'"
                       Text="The library's declared framework must not be retargeted." />
              </Target>
            </Project>
            """);
        FileIn("inputs/Basic/Console/Shared/Shared.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net10.0</TargetFramework>
                <Nullable>enable</Nullable>
                <ImplicitUsings>enable</ImplicitUsings>
                <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
              </PropertyGroup>
              <Target Name="VerifySharedFramework" BeforeTargets="CoreCompile">
                <Error Condition="'$(TargetFramework)' != 'net10.0' or '$(TargetFrameworks)' != ''"
                       Text="The ordinary shared project's declared framework must not be retargeted." />
              </Target>
            </Project>
            """);
        FileIn("inputs/Basic/Console/Program.cs", """
            System.Console.WriteLine(System.AppContext.TargetFrameworkName);
            System.Console.WriteLine(string.Join("|", args));
            System.Console.WriteLine("Run working directory: " + System.Environment.CurrentDirectory);
            if (System.Array.IndexOf(args, "--wait") >= 0)
                System.Threading.Thread.Sleep(60000);
            """);
        using var test = new LifetimeTest(Path.Combine(root, "inputs"));
        var properties = new Dictionary<string, string> { ["ContractSentinel"] = "forwarded-by-buildsample" };
        var workingDirectory = Path.Combine(test.Workspace.Root, "custom working directory");
        Directory.CreateDirectory(workingDirectory);
        var options = new DotNetOptions
        {
            Framework = "net10.0-windows", Properties = properties,
            Timeout = TimeSpan.FromMinutes(1), WorkingDirectory = Path.Combine(workingDirectory, ".")
        };
        await test.Build(options);
        var working = test.Prepare();
        var diagnostics = Path.Combine(test.Diagnostics, "builds", "Basic", "Console", "App", "Release", "net10.0-windows");
        var output = File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log"));
        Assert.Contains("Selected net10.0-windows from net10.0;net10.0-windows", output);
        Assert.Contains("Contract sentinel: forwarded-by-buildsample", output);
        Assert.Contains($"Build working directory: {workingDirectory}", output);
        Assert.Equal(new KeyValuePair<string, string>("ContractSentinel", "forwarded-by-buildsample"), Assert.Single(properties));
        Assert.True(new FileInfo(Path.Combine(diagnostics, "build.binlog")).Length > 0);
        Assert.False(Directory.Exists(Path.Combine(working, "samples", "Basic", "Console", "bin", "Release", "net10.0")));
        Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "Library", "bin", "Release", "netstandard2.1", "Library.dll")));
        Assert.True(File.Exists(Path.Combine(working, "samples", "Basic", "Console", "Shared", "bin", "Release", "net10.0", "Shared.dll")));

        var workspace = test.Workspace;
        var project = Path.Combine(working, "samples", "Basic", "Console", "App.csproj");
        var called = false;
        await DotNet.Run(workspace, project, ["argument with spaces", "--sentinel"], async app =>
        {
            called = true;
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains(".NETCoreApp,Version=v10.0", result.Output);
            Assert.Contains("argument with spaces|--sentinel", result.Output);
            Assert.Contains($"Run working directory: {workingDirectory}", result.Output);
        }, options);
        Assert.True(called);
        Assert.Contains("argument with spaces|--sentinel",
            File.ReadAllText(Assert.Single(Directory.EnumerateFiles(
                Path.Combine(workspace.DiagnosticsRoot, "run"), "*.stdout.log"))));
        foreach (var waitForAddress in new[] { false, true })
        {
            await Assert.ThrowsAsync<TimeoutException>(() =>
                DotNet.Run(workspace, project, ["--wait"], async app =>
                {
                    if (waitForAddress)
                        await app.GetAddress();
                    else
                        await app.WaitForExit();
                }, options with { Timeout = TimeSpan.FromMilliseconds(100) }));
        }
        Assert.Equal(3, Directory.EnumerateFiles(
            Path.Combine(workspace.DiagnosticsRoot, "run"), "*.stderr.log").Count());
        test.Dispose();
        Assert.False(Directory.Exists(working));
        Assert.True(Directory.Exists(diagnostics));
    }

    [Fact]
    public async Task DefaultBuildAndCallbackRunKeepTheirOriginalContract()
    {
        FileIn("inputs/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework><OutputType>Exe</OutputType></PropertyGroup>
            </Project>
            """);
        FileIn("inputs/App/Program.cs", """System.Console.WriteLine(string.Join("|", args));""");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        await DotNet.Build(workspace, project, Path.Combine(workspace.DiagnosticsRoot, "build"));
        var called = false;
        await DotNet.Run(workspace, project, ["old argument", "--sentinel"], async app =>
        {
            called = true;
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("old argument|--sentinel", result.Output);
        });
        Assert.True(called);
    }

    [Fact]
    public void ExplicitBuildPropertiesKeepDefaultFlagsAndDoNotMutateTheCaller()
    {
        FileIn("inputs/App/App.csproj", "<Project><PropertyGroup><TargetFramework>net10.0-ios</TargetFramework></PropertyGroup></Project>");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "build");
        var properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = "iossimulator-arm64" };
        var options = new DotNetOptions { Framework = "net10.0-ios", Properties = properties };
        var defaults = DotNet.BuildArguments(workspace, project, diagnostics);
        var arguments = DotNet.BuildArguments(workspace, project, diagnostics, options);
        Assert.Equal(defaults, arguments.Take(defaults.Length));
        Assert.Equal(new[]
        {
            "--framework", "net10.0-ios",
            "-p:RuntimeIdentifier=iossimulator-arm64"
        }, arguments.Skip(defaults.Length));
        Assert.Equal(Path.GetFullPath(project), arguments[1]);
        Assert.Single(arguments, argument => argument.StartsWith("-bl:", StringComparison.Ordinal));
        Assert.Single(arguments, argument => argument.StartsWith("-p:RestoreConfigFile=", StringComparison.Ordinal));
        Assert.DoesNotContain(arguments, argument => argument.StartsWith("-p:TargetFramework", StringComparison.Ordinal));
        Assert.Equal(new KeyValuePair<string, string>("RuntimeIdentifier", "iossimulator-arm64"), Assert.Single(properties));
        Assert.Equal(arguments, DotNet.BuildArguments(workspace, project, diagnostics, options));
        var run = DotNet.RunArguments(project, ["argument with spaces", "--sentinel"], options);
        Assert.Equal(new[]
        {
            "run", "--project", Path.GetFullPath(project), "-c", "Release", "--no-build", "--no-launch-profile",
            "--framework", "net10.0-ios", "-p:RuntimeIdentifier=iossimulator-arm64", "--", "argument with spaces", "--sentinel"
        }, run);
        foreach (var name in new[] { "TargetFramework", "TargetFrameworks", "configuration", "RestoreConfigFile",
            "RestoreNoCache", "UseSharedCompilation", " " })
        {
            var conflicting = new Dictionary<string, string> { [name] = "override" };
            Assert.Throws<ArgumentException>(() =>
                DotNet.BuildArguments(workspace, project, diagnostics, options with { Properties = conflicting }));
            Assert.Throws<ArgumentException>(() =>
                DotNet.RunArguments(project, [], options with { Properties = conflicting }));
        }
    }

    [Fact]
    public void FrameworkSelectionValidatesDeclarationsWithoutWritingProjectsOrTargets()
    {
        const string projectXml = "<Project><PropertyGroup><TargetFrameworks>net10.0;net8.0</TargetFrameworks></PropertyGroup></Project>";
        FileIn("inputs/App/App.csproj", projectXml);
        FileIn("inputs/Directory.Build.targets", "<Project><Target Name=\"ExistingSampleTarget\" /></Project>");
        using var workspace = new SampleWorkspace("App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "build");
        var targets = Path.Combine(workspace.Root, "samples", "Directory.Build.targets");
        var originalTargets = File.ReadAllText(targets);
        DotNet.BuildArguments(workspace, project, diagnostics, new() { Framework = "net10.0" });
        DotNet.BuildArguments(workspace, project, diagnostics, new() { Framework = "net8.0" });
        var original = System.Xml.Linq.XDocument.Load(targets);
        Assert.Equal("ExistingSampleTarget", Assert.Single(original.Descendants("Target")).Attribute("Name")!.Value);
        Assert.Empty(original.Descendants("Import"));
        Assert.Equal(originalTargets, File.ReadAllText(targets));
        Assert.False(File.Exists(Path.Combine(diagnostics, "selected-root.targets")));
        Assert.Equal(projectXml, File.ReadAllText(project));
        Assert.Equal(projectXml, File.ReadAllText(Path.Combine(root, "inputs", "App", "App.csproj")));
        Assert.Throws<ArgumentException>(() => DotNet.ValidateDeclaredFramework(workspace, project, "net11.0"));
        Assert.Throws<ArgumentException>(() => DotNet.ValidateDeclaredFramework(workspace, project, "net10.0;net8.0"));
        Assert.Throws<ArgumentException>(() => DotNet.ValidateDeclaredFramework(workspace,
            Path.Combine(workspace.Root, "..", "outside.csproj"), "net10.0"));
        Assert.Throws<ArgumentException>(() => DotNet.ValidateDeclaredFramework(workspace,
            Path.Combine(root, "inputs", "App", "App.csproj"), "net10.0"));
    }

    [Fact]
    public void DotNetOptionsDefaultsAndCopiesKeepArgumentsIndependent()
    {
        var defaults = new DotNetOptions();
        Assert.Equal("Release", defaults.Configuration);
        Assert.Null(defaults.Framework);
        Assert.Empty(defaults.Properties);
        Assert.Null(defaults.Timeout);
        Assert.Null(defaults.WorkingDirectory);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<string, string>)defaults.Properties).Add("Sentinel", "value"));
        var options = defaults with { Configuration = "Debug", Framework = "net10.0", WorkingDirectory = "." };
        Assert.Equal("Release", defaults.Configuration);
        Assert.Null(defaults.Framework);
        Assert.Null(defaults.WorkingDirectory);
        Assert.Equal(Path.GetFullPath("."), options.GetWorkingDirectory(root));
        Assert.Equal(Path.GetFullPath(root), defaults.GetWorkingDirectory(root));
        var project = Path.Combine(root, "project with spaces.csproj");
        Assert.Equal(new[]
        {
            "run", "--project", project, "-c", "Release", "--no-build", "--no-launch-profile", "--", "argument with spaces"
        }, DotNet.RunArguments(project, ["argument with spaces"]));
        var run = DotNet.RunArguments(project, [], options);
        Assert.Equal("Debug", run[4]);
        Assert.Equal(new[] { "--framework", "net10.0", "--" }, run.Skip(7));
    }

    [Fact]
    public async Task InvalidDotNetOptionsFailBeforeStartingProcesses()
    {
        FileIn("inputs/App/App.csproj");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "invalid-build");
        var invalid = new DotNetOptions[]
        {
            new() { Configuration = "" }, new() { Configuration = " " }, new() { Configuration = null! },
            new() { Framework = "" }, new() { Framework = " " },
            new() { WorkingDirectory = "" }, new() { WorkingDirectory = " " },
            new() { Properties = null! }, new() { Timeout = TimeSpan.Zero },
            new() { Timeout = Timeout.InfiniteTimeSpan }, new() { Timeout = TimeSpan.MaxValue }
        };
        foreach (var options in invalid)
        {
            Assert.ThrowsAny<ArgumentException>(() => DotNet.BuildArguments(workspace, project, diagnostics, options));
            Assert.ThrowsAny<ArgumentException>(() => DotNet.RunArguments(project, [], options));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => DotNet.Build(workspace, project, diagnostics, options));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => DotNet.Run(workspace, project, [], _ => Task.CompletedTask, options));
            await Assert.ThrowsAnyAsync<ArgumentException>(() => DotNet.Execute(workspace, ["--version"], options));
        }
        Assert.False(Directory.Exists(diagnostics));
        Assert.False(Directory.Exists(Path.Combine(workspace.DiagnosticsRoot, "run")));
    }

    [Fact]
    public async Task DotNetFailedBuildKeepsLogsInTheProvidedDirectory()
    {
        FileIn("inputs/App/App.csproj", """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup>
              <Target Name="FailBuild" BeforeTargets="CoreCompile">
                <Error Text="Expected sample build failure" />
              </Target>
            </Project>
            """);
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        var diagnostics = Path.Combine(root, "provided-build-logs");
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            DotNet.Build(workspace, Path.Combine(workspace.Root, "samples", "App", "App.csproj"), diagnostics));
        Assert.Contains(diagnostics, error.Message);
        Assert.Contains("Expected sample build failure", File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log")));
        Assert.DoesNotContain("Expected sample build failure", File.ReadAllText(Path.Combine(diagnostics, "build.stderr.log")));
        Assert.DoesNotContain(error.ToString(), File.ReadAllText(Path.Combine(diagnostics, "build.stdout.log")));
        Assert.Contains(error.Message, File.ReadAllText(Path.Combine(diagnostics, "build.failure.log")));
        Assert.True(new FileInfo(Path.Combine(diagnostics, "build.binlog")).Length > 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CleanupRecoversOnlyMacMetadataCreatedDuringDeletion(bool unrelatedFile)
    {
        FileIn("owned/obj/initial.bin", "Build output.");
        if (unrelatedFile)
            FileIn("owned/obj/late.bin", "Unrelated residue.");
        FileIn("other-workspace/.DS_Store", "Keep this metadata.");
        FileIn("diagnostics/build.log", "Keep these diagnostics.");
        var owned = Path.Combine(root, "owned");
        var obj = Path.Combine(owned, "obj");
        var calls = 0;
        void Delete(string path)
        {
            calls++;
            if (calls == 1)
            {
                File.Delete(Path.Combine(obj, "initial.bin"));
                File.WriteAllText(Path.Combine(path, ".DS_Store"), "Late parent metadata.");
                File.WriteAllText(Path.Combine(obj, ".DS_Store"), "Late metadata.");
                Directory.Delete(obj);
            }
            Directory.Delete(path, recursive: true);
        }

        if (OperatingSystem.IsMacOS() && !unrelatedFile)
        {
            SampleWorkspace.DeleteDirectory(owned, Delete);
            Assert.Equal(2, calls);
            Assert.False(Directory.Exists(owned));
        }
        else
        {
            Assert.Throws<IOException>(() => SampleWorkspace.DeleteDirectory(owned, Delete));
            Assert.Equal(1, calls);
            Assert.True(Directory.Exists(owned));
        }
        Assert.True(File.Exists(Path.Combine(root, "other-workspace", ".DS_Store")));
        Assert.True(File.Exists(Path.Combine(root, "diagnostics", "build.log")));
    }

    [Fact]
    public void CleanupDoesNotRetryUnrelatedErrorsWithMetadataPresent()
    {
        FileIn("owned/obj/.DS_Store", "Metadata.");
        var owned = Path.Combine(root, "owned");
        var expected = new IOException("Expected unrelated I/O failure.", 1234);
        var calls = 0;
        var actual = Assert.Throws<IOException>(() => SampleWorkspace.DeleteDirectory(owned, _ =>
        {
            calls++;
            throw expected;
        }));
        Assert.Same(expected, actual);
        Assert.Equal(1, calls);
        Assert.True(File.Exists(Path.Combine(owned, "obj", ".DS_Store")));
    }

    [Theory]
    [InlineData("late.bin", 1)]
    [InlineData(".DS_Store", 2)]
    public void CleanupDoesNotHidePersistentOrNonMetadataWriters(string name, int macCalls)
    {
        FileIn($"owned/obj/{name}", "Late file.");
        var owned = Path.Combine(root, "owned");
        var obj = Path.Combine(owned, "obj");
        var calls = 0;
        void Delete(string path)
        {
            calls++;
            File.WriteAllText(Path.Combine(obj, name), "Late file.");
            Directory.Delete(obj);
        }

        Assert.Throws<IOException>(() => SampleWorkspace.DeleteDirectory(owned, Delete));
        Assert.Equal(OperatingSystem.IsMacOS() ? macCalls : 1, calls);
        Assert.True(File.Exists(Path.Combine(obj, name)));
    }

    [Fact]
    public async Task CleanupWaitsForOwnedWindowsFileHandles()
    {
        FileIn("inputs/App/App.csproj");
        using var workspace = new SampleWorkspace(
            "App", Path.Combine(root, "inputs"), Repo.PackagesDir,
            Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
        using var file = new FileStream(Path.Combine(workspace.Root, "owned.dll"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        var release = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(250, TestContext.Current.CancellationToken);
            }
            finally
            {
                file.Dispose();
            }
        }, TestContext.Current.CancellationToken);
        try
        {
            workspace.Dispose();
            Assert.False(Directory.Exists(workspace.Root));
        }
        finally
        {
            await release;
        }
    }

    private sealed class AddressRunningApp(string diagnostics) : RunningApp(diagnostics)
    {
        internal int AddressRequests { get; private set; }
        internal Uri Address { get; init; } = new("http://127.0.0.1:49152");

        internal override Task<Uri> GetAddress()
        {
            AddressRequests++;
            return Task.FromResult(Address);
        }

        internal override Task<(int ExitCode, string Output, string Error)> WaitForExit() =>
            Task.FromResult((0, "", ""));

        protected override Task Stop() => Task.CompletedTask;

        protected override Task<(string Output, string Error)> ReadOutput() =>
            Task.FromResult(("", ""));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            SampleWorkspace.DeleteDirectory(root);
    }
}
