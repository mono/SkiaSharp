using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Mac;
using SkiaSharp;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Mac Catalyst applications.
/// </summary>
[Trait("Category", "Desktop")]
[Trait("Category", "Golden")]
public class MauiMacCatalystTests(AppiumFixture appium, ITestOutputHelper output) : MauiTestBase(appium, output)
{
    [Theory]
    [InlineData("SKCanvasView", "SKPaintSurfaceEventArgs")]
    [InlineData("SKGLView", "SKPaintGLSurfaceEventArgs")]
    public Task MatchesGolden(string view, string eventArgs) => RunMauiTest(view, eventArgs);

    protected override string PlatformName => "MacCatalyst";
    protected override string TargetFramework => $"{BaseFramework}-maccatalyst";
    
    // Longer retry delay for Mac Catalyst - automation mode issues need more recovery time
    protected override TimeSpan RetryDelay => TimeSpan.FromSeconds(30);

    protected override string? CanRunOnCurrentMachine() =>
        OperatingSystem.IsMacOS() ? null : "Mac Catalyst requires macOS (hardware requirement)";

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        options.PlatformName = "mac";
        options.AutomationName = "mac2";
        options.AddAdditionalAppiumOption("bundleId", bundleId);
        options.AddAdditionalAppiumOption("appPath", appPath);
        // Enable server logs to debug WebDriverAgentMac issues
        options.AddAdditionalAppiumOption("showServerLogs", true);
        // Increase server startup timeout (default is 120s, automation mode can take longer)
        options.AddAdditionalAppiumOption("serverStartupTimeout", 180000);
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new MacDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(180));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetDirectories(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework),
            "*.app", SearchOption.AllDirectories).FirstOrDefault();
    
    // Mac Catalyst: Most Macs are 2x Retina. Screenshot is full monitor but coordinates are app-relative.
    protected override double GetScreenScaleFactor(SKSizeI screenshotSize, SKSizeI windowSize) => 2.0;

}
