using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Mac;
using SkiaSharp;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Mac Catalyst applications.
/// </summary>
public class MauiMacCatalystTests(ITestOutputHelper output) : MauiTestBase(output)
{
    protected override string PlatformName => "MacCatalyst";
    protected override string TargetFramework => "net10.0-maccatalyst";

    protected override string? CanRunOnCurrentMachine() =>
        OperatingSystem.IsMacOS() ? null : "Mac Catalyst requires macOS (hardware requirement)";

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        options.PlatformName = "mac";
        options.AutomationName = "mac2";
        options.AddAdditionalAppiumOption("bundleId", bundleId);
        options.AddAdditionalAppiumOption("appPath", appPath);
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new MacDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(120));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetDirectories(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework),
            "*.app", SearchOption.AllDirectories).FirstOrDefault();
    
    // Mac Catalyst: Most Macs are 2x Retina. Screenshot is full monitor but coordinates are app-relative.
    protected override double GetScreenScaleFactor(SKSizeI screenshotSize, SKSizeI windowSize) => 2.0;
}
