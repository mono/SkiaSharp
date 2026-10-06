using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI iOS applications.
/// Device and version can be configured via MSBuild properties:
///   dotnet test -p:iOSDevice="iPhone 16 Pro" -p:iOSVersion="26.5" -p:iOSDeviceId="SIMULATOR-UDID"
/// </summary>
[Trait("Category", "ManualPlatform")]
public class MauiiOSTests(ITestOutputHelper output) : MauiTestBase(output)
{
    // Default device configuration (can be overridden via -p:iOSDevice and -p:iOSVersion)
    private const string DefaultVersion = "18.5";
    
    // Read from RuntimeHostConfigurationOption (set via MSBuild properties)
    private static string? DeviceName => AppContext.GetData("iOSDevice") as string is { Length: > 0 } device ? device : null;
    private static string PlatformVersion => 
        AppContext.GetData("iOSVersion") as string is { Length: > 0 } version ? version : DefaultVersion;
    private static string DeviceUdid =>
        AppContext.GetData("iOSDeviceId") as string is { Length: > 0 } udid
            ? udid : throw new InvalidOperationException("Specify -p:iOSDeviceId=<owned-simulator-udid>; refusing to select another booted simulator.");
    
    protected override string PlatformName => "iOS";
    protected override string TargetFramework => $"{BaseFramework}-ios";

    protected override string? CanRunOnCurrentMachine() =>
        OperatingSystem.IsMacOS() ? null : "iOS requires macOS (hardware requirement)";

    /// <summary>
    /// Get the iOS version for screenshot naming.
    /// </summary>
    protected override Task<string?> GetDeviceVersionAsync() =>
        Task.FromResult<string?>(PlatformVersion);

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        Output.WriteLine($"iOS Device: {DeviceName ?? "(selected by UDID)"}, Version: {PlatformVersion}, UDID: {DeviceUdid}");
        
        options.PlatformName = "iOS";
        options.AutomationName = "XCUITest";
        options.App = appPath;
        if (DeviceName is { } deviceName)
            options.DeviceName = deviceName;
        options.PlatformVersion = PlatformVersion;
        options.AddAdditionalAppiumOption("udid", DeviceUdid);
        options.AddAdditionalAppiumOption("bundleId", bundleId);
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new IOSDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(180));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetDirectories(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework, "iossimulator-arm64"),
            "*.app", SearchOption.AllDirectories).FirstOrDefault();
}
