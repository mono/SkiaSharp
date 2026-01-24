using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI iOS applications.
/// Device and version can be configured via MSBuild properties:
///   dotnet test -p:iOSDevice="iPad Pro 13-inch (M4)" -p:iOSVersion="18.5"
/// </summary>
public class MauiiOSTests(ITestOutputHelper output) : MauiTestBase(output)
{
    // Default device configuration (can be overridden via -p:iOSDevice and -p:iOSVersion)
    private const string DefaultDevice = "iPhone 16 Pro";
    private const string DefaultVersion = "18.5";
    
    // Read from RuntimeHostConfigurationOption (set via MSBuild properties)
    private static string DeviceName => 
        AppContext.GetData("iOSDevice") as string is { Length: > 0 } device ? device : DefaultDevice;
    private static string PlatformVersion => 
        AppContext.GetData("iOSVersion") as string is { Length: > 0 } version ? version : DefaultVersion;
    
    protected override string PlatformName => "iOS";
    protected override string TargetFramework => "net10.0-ios";

    protected override string? CanRunOnCurrentMachine() =>
        OperatingSystem.IsMacOS() ? null : "iOS requires macOS (hardware requirement)";

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        Output.WriteLine($"iOS Device: {DeviceName}, Version: {PlatformVersion}");
        
        options.PlatformName = "iOS";
        options.AutomationName = "XCUITest";
        options.App = appPath;
        options.DeviceName = DeviceName;
        options.PlatformVersion = PlatformVersion;
        options.AddAdditionalAppiumOption("bundleId", bundleId);
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new IOSDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(180));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetDirectories(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework, "iossimulator-arm64"),
            "*.app", SearchOption.AllDirectories).FirstOrDefault();
}
