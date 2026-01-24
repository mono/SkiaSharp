using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Android applications.
/// Device and version can be configured via MSBuild properties:
///   dotnet test -p:AndroidDevice="Pixel 8" -p:AndroidVersion="14"
/// </summary>
public class MauiAndroidTests(ITestOutputHelper output) : MauiTestBase(output)
{
    // Default device configuration (can be overridden via -p:AndroidDevice and -p:AndroidVersion)
    private const string DefaultDevice = "Android Emulator";
    private const string DefaultVersion = "";  // Empty = use whatever is running
    
    // Read from RuntimeHostConfigurationOption (set via MSBuild properties)
    private static string DeviceName => 
        AppContext.GetData("AndroidDevice") as string is { Length: > 0 } device ? device : DefaultDevice;
    private static string? PlatformVersion => 
        AppContext.GetData("AndroidVersion") as string is { Length: > 0 } version ? version : null;
    
    protected override string PlatformName => "Android";
    protected override string TargetFramework => "net10.0-android";

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        Output.WriteLine($"Android Device: {DeviceName}, Version: {PlatformVersion ?? "(default)"}");
        
        options.PlatformName = "Android";
        options.AutomationName = "UiAutomator2";
        options.App = appPath;
        options.DeviceName = DeviceName;
        if (PlatformVersion != null)
            options.PlatformVersion = PlatformVersion;
        options.AddAdditionalAppiumOption("appPackage", bundleId);
        options.AddAdditionalAppiumOption("appWaitActivity", "*");
        options.AddAdditionalAppiumOption("autoGrantPermissions", true);
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new AndroidDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(120));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetFiles(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework),
            "*.apk", SearchOption.AllDirectories).FirstOrDefault();
}
