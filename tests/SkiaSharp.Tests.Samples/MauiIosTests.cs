using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI iOS applications.
/// </summary>
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public class MauiIosTests(AppiumFixture appium, ITestOutputHelper output) : MauiTestBase(appium, output)
{
    private string deviceName = "";
    private string platformVersion = "";
    private string deviceUdid = "";

    [Theory]
    [InlineData("SKCanvasView", "SKPaintSurfaceEventArgs", TestDevices.IosName, TestDevices.IosVersion)]
    [InlineData("SKGLView", "SKPaintGLSurfaceEventArgs", TestDevices.IosName, TestDevices.IosVersion)]
    public Task MatchesGolden(string view, string eventArgs, string name, string version)
    {
        deviceName = name;
        platformVersion = version;
        return RunMauiTest(view, eventArgs);
    }

    protected override string PlatformName => "iOS";
    protected override string TargetFramework => $"{BaseFramework}-ios";

    protected override string? CanRunOnCurrentMachine() =>
        OperatingSystem.IsMacOS() ? null : "iOS requires macOS (hardware requirement)";

    protected override async Task PerformPreflightChecks()
    {
        var (exitCode, json, error) = await DotNet.RunProcess("xcrun", ["simctl", "list", "devices", "--json"],
            TestDir, TimeSpan.FromSeconds(30), Output);
        if (exitCode != 0)
            throw new InvalidOperationException($"Could not inspect the required iOS simulator ({exitCode}): {error}");
        deviceUdid = TestDevices.RequireIosSimulator(json, deviceName, platformVersion);
        var (bootExit, bootOutput, bootError) = await DotNet.RunProcess("xcrun",
            ["simctl", "bootstatus", deviceUdid, "-b"], TestDir, TimeSpan.FromMinutes(5), Output);
        if (bootExit != 0)
            throw new InvalidOperationException($"Required simulator {deviceUdid} did not finish booting ({bootExit}):\n{bootOutput}\n{bootError}");
    }

    /// <summary>
    /// Get the iOS version for screenshot naming.
    /// </summary>
    protected override Task<string?> GetDeviceVersionAsync() =>
        Task.FromResult<string?>(platformVersion);

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        Output.WriteLine($"iOS: {deviceName}, version {platformVersion}, UDID {deviceUdid}");

        options.PlatformName = "iOS";
        options.AutomationName = "XCUITest";
        options.App = appPath;
        options.DeviceName = deviceName;
        options.PlatformVersion = platformVersion;
        options.AddAdditionalAppiumOption("udid", deviceUdid);
        options.AddAdditionalAppiumOption("bundleId", bundleId);
        options.AddAdditionalAppiumOption("isHeadless", true);
        options.AddAdditionalAppiumOption("showXcodeLog", true);
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new IOSDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(180));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetDirectories(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework, "iossimulator-arm64"),
            "*.app", SearchOption.AllDirectories).FirstOrDefault();
}
