using System.Runtime.InteropServices;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(AppiumCollection.Name)]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AppiumSampleTests(AppiumFixture appium) : SampleTestBase
{
    [Theory]
    [Trait("Platform", "Android")]
    [MemberData(nameof(BasicAndroidCases))]
    public async Task BasicAndroidRendersAndDraws(string folder, string applicationId, bool maui, int api)
    {
        const string project = "SkiaSharpSample/SkiaSharpSample.csproj";
        var workspace = await BuildSample(folder, project, new DotNetBuildOptions
        {
            Configuration = "Debug", Framework = "net10.0-android",
            Properties = new Dictionary<string, string> { ["AndroidPackageFormats"] = "apk" }
        });
        var artifact = Assert.Single(Directory.EnumerateFiles(
            Output(workspace, folder, project, "net10.0-android"), "*-Signed.apk", SearchOption.AllDirectories));
        var device = AndroidProfile(api);
        await appium.Run(new AndroidRunningApp(workspace, artifact, applicationId, device), app =>
        {
            Basic(app, maui ? "Maui" : "Native", "Android", $"{device.Name}/{device.Version}",
                maui ? "basic-canvas-cpu" : "skiaView",
                maui ? "basic-canvas-drawing" : "skiaView",
                maui ? "basic-drawing-clear" : "btnClear",
                maui ? "basic-canvas-gpu" : "skiaView",
                maui ? "basic-gpu-fps" : "fpsLabel",
                title =>
                {
                    if (maui)
                    {
                        app.Click(MobileBy.AccessibilityId("Open navigation drawer"));
                        app.Click(MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{title}\")"));
                    }
                    else
                        app.Click(title switch
                        {
                            "CPU" => "nav_cpu",
                            "Drawing" => "nav_drawing",
                            "GPU" => "nav_gpu_surface",
                            _ => throw new ArgumentOutOfRangeException(nameof(title))
                        });
                });
            return Task.CompletedTask;
        });
    }

    [Theory]
    [Trait("Platform", "iOS")]
    [InlineData("Basic/Maui", "com.companyname.SkiaSharpSample", true, "18.6")]
    [InlineData("Basic/Maui", "com.companyname.SkiaSharpSample", true, "26.2")]
    [InlineData("Basic/iOS", "com.companyname.skiasharpsample", false, "18.6")]
    [InlineData("Basic/iOS", "com.companyname.skiasharpsample", false, "26.2")]
    public async Task BasicIosRendersAndDraws(string folder, string applicationId, bool maui, string version)
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");
        const string project = "SkiaSharpSample/SkiaSharpSample.csproj";
        var app = await AppleApp(folder, project, "net10.0-ios", applicationId, simulator: true, IosProfile(version));
        await appium.Run(app, running =>
        {
            Basic(running, maui ? "Maui" : "Native", "iOS", $"iPhone-16-Pro/{version}",
                "basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
                maui ? "basic-canvas-gpu" : "basic-canvas-gpu-gl",
                maui ? "basic-gpu-fps" : "basic-gpu-gl-fps",
                title =>
                {
                    if (maui)
                        running.Click(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND label == 'Menu'"));
                    running.Click(MobileBy.IosNSPredicate(
                        $"type == 'XCUIElementTypeButton' AND label == '{(!maui && title == "GPU" ? "GL" : title)}'"));
                });
            return Task.CompletedTask;
        });
    }

    [Theory]
    [Trait("Platform", "MacCatalyst")]
    [InlineData("Basic/Maui", "com.companyname.SkiaSharpSample", true)]
    [InlineData("Basic/MacCatalyst", "com.companyname.SkiaSharpSample", false)]
    public async Task BasicMacCatalystRendersAndDraws(string folder, string applicationId, bool maui)
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");
        const string project = "SkiaSharpSample/SkiaSharpSample.csproj";
        var app = await AppleApp(folder, project, "net10.0-maccatalyst", applicationId, simulator: false);
        await appium.Run(app, running =>
        {
            Basic(running, maui ? "Maui" : "Native", "MacCatalyst", HostArchitecture(),
                "basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
                maui ? "basic-canvas-gpu" : "basic-canvas-gpu-metal",
                maui ? "basic-gpu-fps" : "basic-gpu-metal-fps",
                title =>
                {
                    if (maui)
                        running.Click(By.Name("Menu"));
                    running.Click(By.Name(!maui && title == "GPU" ? "Metal" : title));
                });
            return Task.CompletedTask;
        });
    }

    [Fact]
    [Trait("Platform", "macOS")]
    public async Task BasicMacOsRendersAndDraws()
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "Native AppKit tests require macOS.");
        var app = await AppleApp("Basic/macOS", "SkiaSharpSample/SkiaSharpSample.csproj",
            "net10.0-macos", "com.companyname.skiasharpsample", simulator: false);
        await appium.Run(app, running =>
        {
            Basic(running, "Native", "macOS", HostArchitecture(),
                "basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
                "basic-canvas-gpu-gl", "basic-gpu-gl-fps",
                title => running.Click(By.Name(title == "GPU" ? "GPU (GL)" : title)));
            return Task.CompletedTask;
        });
    }

    [Theory]
    [Trait("Platform", "Windows")]
    [InlineData("Basic/Maui", true)]
    [InlineData("Basic/WinUI", false)]
    public async Task BasicWindowsRendersAndDraws(string folder, bool maui)
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");
        var app = await WindowsApp(folder, "SkiaSharpSample/SkiaSharpSample.csproj", nativeWinUi: !maui);
        await appium.Run(app, running =>
        {
            Basic(running, maui ? "Maui" : "Native", "Windows", HostArchitecture(),
                "basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
                "basic-canvas-gpu", "basic-gpu-fps",
                title =>
                {
                    if (maui)
                    {
                        running.Click(By.Name("Open navigation menu"));
                        running.Click(By.Name(title));
                    }
                    else
                        running.Click("basic-navigation-" + title.ToLowerInvariant());
                });
            return Task.CompletedTask;
        });
    }

    [Theory]
    [Trait("Platform", "Android")]
    [MemberData(nameof(AndroidApiCases))]
    public async Task GalleryAndroidFiltersAndAdjustsGradient(int api)
    {
        const string folder = "Gallery";
        const string project = "Maui/SkiaSharpSample.Maui.csproj";
        var workspace = await BuildSample(folder, project, new DotNetBuildOptions
        {
            Configuration = "Debug", Framework = "net10.0-android",
            Properties = new Dictionary<string, string> { ["AndroidPackageFormats"] = "apk" }
        });
        var artifact = Assert.Single(Directory.EnumerateFiles(
            Output(workspace, folder, project, "net10.0-android"), "*-Signed.apk", SearchOption.AllDirectories));
        var device = AndroidProfile(api);
        await appium.Run(new AndroidRunningApp(workspace, artifact, "com.skiasharp.gallery.maui", device),
            app => Gallery(app, "Android", $"{device.Name}/{device.Version}"));
    }

    [Theory]
    [Trait("Platform", "iOS")]
    [InlineData("18.6")]
    [InlineData("26.2")]
    public async Task GalleryIosFiltersAndAdjustsGradient(string version)
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");
        await appium.Run(await AppleApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj", "net10.0-ios",
            "com.skiasharp.gallery.maui", simulator: true, IosProfile(version)),
            app => Gallery(app, "iOS", $"iPhone-16-Pro/{version}"));
    }

    [Fact]
    [Trait("Platform", "MacCatalyst")]
    public async Task GalleryMacCatalystFiltersAndAdjustsGradient()
    {
        Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");
        await appium.Run(await AppleApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj", "net10.0-maccatalyst",
            "com.skiasharp.gallery.maui", simulator: false), app => Gallery(app, "MacCatalyst", HostArchitecture()));
    }

    [Fact]
    [Trait("Platform", "Windows")]
    public async Task GalleryWindowsFiltersAndAdjustsGradient()
    {
        Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");
        await appium.Run(await WindowsApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj", nativeWinUi: false),
            app => Gallery(app, "Windows", HostArchitecture()));
    }

    private async Task<AppiumRunningApp> AppleApp(string folder, string project, string framework,
        string applicationId, bool simulator, IosDevice? device = null)
    {
        var rid = (simulator ? "iossimulator" : framework == "net10.0-macos" ? "osx" : "maccatalyst")
            + "-" + HostArchitecture();
        var workspace = await BuildSample(folder, project, new DotNetBuildOptions
        {
            Configuration = "Debug", Framework = framework,
            Properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = rid }
        });
        var artifact = Assert.Single(Directory.EnumerateDirectories(
            Path.Combine(Output(workspace, folder, project, framework), rid), "*.app"));
        return simulator
            ? new IosRunningApp(workspace, artifact, applicationId,
                device ?? throw new ArgumentException("Simulator cases must declare an exact runtime profile.", nameof(device)))
            : new MacRunningApp(workspace, artifact, applicationId, RequiredEnvironment("SKIASHARP_APPIUM_MAC_WDA_ROOT"));
    }

    private async Task<WindowsRunningApp> WindowsApp(string folder, string project, bool nativeWinUi)
    {
        const string framework = "net10.0-windows10.0.19041.0";
        var architecture = HostArchitecture();
        var properties = new Dictionary<string, string>
            {
                ["Platform"] = architecture,
                ["RuntimeIdentifier"] = (nativeWinUi ? "win10-" : "win-") + architecture,
                ["AppxPackageSigningEnabled"] = "false"
            };
        var workspace = await BuildSample(folder, project, new DotNetBuildOptions
        {
            Configuration = "Debug", Framework = framework, Properties = properties
        });
        var output = await TargetDirectory(workspace, folder, project, framework, properties);
        var manifest = Assert.Single(Directory.EnumerateFiles(
            output, "AppxManifest.xml", SearchOption.AllDirectories));
        var document = XDocument.Load(manifest);
        var applicationId = (string?)Assert.Single(document.Descendants(),
            element => element.Name.LocalName == "Application").Attribute("Id")
            ?? throw new InvalidOperationException($"Generated application ID is missing: {manifest}");
        return new WindowsRunningApp(workspace, manifest, applicationId, RequiredEnvironment("SKIASHARP_APPIUM_WINAPPDRIVER"));
    }

    private static async Task<string> TargetDirectory(SampleWorkspace workspace, string folder, string project,
        string framework, IReadOnlyDictionary<string, string> properties)
    {
        var result = await DotNet.Execute(workspace,
            ["msbuild", Path.Combine(workspace.Root, "samples", folder, project), "-nologo",
                "-getProperty:TargetDir", "-p:Configuration=Debug", $"-p:TargetFramework={framework}",
                .. properties.Select(property => $"-p:{property.Key}={property.Value}")]);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Could not evaluate the built sample output: {result.Output}\n{result.Error}");
        var directory = result.Output.Trim();
        var relative = Path.GetRelativePath(workspace.Root, directory);
        if (!Path.IsPathRooted(directory) || Path.IsPathRooted(relative) || relative == ".." ||
            relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !Directory.Exists(directory))
            throw new InvalidOperationException($"Built output must exist inside the owned workspace: {directory}");
        return directory;
    }

    private static string Output(SampleWorkspace workspace, string folder, string project, string framework) =>
        Path.Combine(workspace.Root, "samples", folder, Path.GetDirectoryName(project)!, "bin", "Debug", framework);

    private static string HostArchitecture() => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.Arm64 => "arm64",
        Architecture.X64 => "x64",
        _ => throw new InvalidOperationException("Appium desktop/simulator cases declare only arm64 and x64 hosts.")
    };

    private static AndroidDevice AndroidProfile(int api) => api switch
    {
        24 => AndroidDevice.PixelApi24,
        37 => AndroidDevice.PixelApi37,
        _ => throw new ArgumentOutOfRangeException(nameof(api), "Declare an assigned Android profile.")
    };

    public static IEnumerable<TheoryDataRow<string, string, bool, int>> BasicAndroidCases()
    {
        foreach (var api in new[] { 24, 37 })
        {
            yield return new("Basic/Maui", "com.companyname.SkiaSharpSample", true, api)
            {
                Traits = new() { ["AndroidApi"] = [api.ToString()] }
            };
            yield return new("Basic/Android", "com.companyname.skiasharpsample", false, api)
            {
                Traits = new() { ["AndroidApi"] = [api.ToString()] }
            };
        }
    }

    public static IEnumerable<TheoryDataRow<int>> AndroidApiCases() =>
    [
        new(24) { Traits = new() { ["AndroidApi"] = ["24"] } },
        new(37) { Traits = new() { ["AndroidApi"] = ["37"] } }
    ];

    private static IosDevice IosProfile(string version) => version switch
    {
        "18.6" => IosDevice.Iphone16ProIos18_6,
        "26.2" => IosDevice.Iphone16Pro,
        _ => throw new ArgumentOutOfRangeException(nameof(version), "Declare an assigned iOS profile.")
    };

    private static string RequiredEnvironment(string name) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value :
            throw new InvalidOperationException($"Set {name} to the explicitly installed platform driver's path before selecting its tests.");

    private static void Basic(AppiumRunningApp app, string family, string platform, string device,
        string cpuId, string drawingId, string clearId, string gpuId, string fpsId, Action<string> navigate)
    {
        var cpu = app.CaptureStable(cpuId, "basic-cpu");
        AppiumRunningApp.AssertRendered(cpu);
        navigate("Drawing");
        var empty = app.CaptureStable(drawingId, "drawing-empty");
        app.Drag(app.Find(drawingId), 0.25, 0.3, 0.7, 0.5);
        var drawn = app.CaptureStable(drawingId, "drawing-stroke");
        AppiumRunningApp.AssertSceneChanged(empty, drawn);
        app.Click(clearId);
        SampleImage.CompareGolden(app.CaptureStable(drawingId, "drawing-cleared"), empty);
        navigate("GPU");
        app.WaitText(fpsId, text => text.Contains("FPS:", StringComparison.Ordinal) && !text.Contains("--"));
        AppiumRunningApp.AssertRendered(app.Capture(gpuId, "basic-gpu-smoke"));
        navigate("CPU");
        SampleImage.CompareGolden(app.CaptureStable(cpuId, "basic-cpu-returned"), cpu);
        Compare(cpu, family, platform, device, "basic-cpu");
    }

    private static Task Gallery(AppiumRunningApp app, string platform, string device)
    {
        app.Click("gallery-header-cpu");
        app.Find("gallery-search").SendKeys("Gradient");
        app.SubmitSearch();
        app.WaitText("gallery-result-count", text => !text.StartsWith("0 ", StringComparison.Ordinal));
        app.Click("gallery-filter-trigger");
        app.Click("category-shaders---effects");
        app.Find("gallery-filter-popup-count");
        app.Click("gallery-filter-done");
        app.Find("gallery-active-category-shaders---effects");
        app.Click("sample-gradient");
        app.WaitText("sample-title", text => text == "Gradient");
        app.WaitText("sample-render-status", text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
        var cpu = app.CaptureStable("sample-canvas-cpu", "gallery-gradient-cpu");
        AppiumRunningApp.AssertRendered(cpu);
        var before = app.Text("control-angle-value");
        app.Drag(app.Find("control-angle"), 0.2, 0.5, 0.7, 0.5);
        app.WaitText("control-angle-value", text => text != before);
        AppiumRunningApp.AssertSceneChanged(cpu, app.CaptureStable("sample-canvas-cpu", "gallery-gradient-adjusted"));
        app.Click("sample-back");
        app.Find("gallery-search");
        app.Click("gallery-header-gpu");
        app.Click("sample-gradient");
        app.WaitText("sample-render-status", text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
        var gpu = app.CaptureStable("sample-canvas-gpu", "gallery-gradient-gpu");
        AppiumRunningApp.AssertRendered(gpu);
        app.Click("sample-back");
        app.Find("gallery-search");
        Assert.Multiple(
            () => Compare(cpu, "Maui", platform, device, "gallery-gradient-cpu"),
            () => Compare(gpu, "Maui", platform, device, "gallery-gradient-gpu"));
        return Task.CompletedTask;
    }

    private static void Compare(string actual, string family, string platform, string device, string name) =>
        SampleImage.CompareGolden(actual, Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
            "Appium", family, platform, device.Replace('/', Path.DirectorySeparatorChar), name + ".png"));
}
