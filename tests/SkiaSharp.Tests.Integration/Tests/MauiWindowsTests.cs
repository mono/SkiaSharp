using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using SkiaSharp;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Windows applications.
/// </summary>
public class MauiWindowsTests(ITestOutputHelper output) : MauiTestBase(output)
{
    protected override string PlatformName => "Windows";
    protected override string TargetFramework => "net10.0-windows10.0.19041.0";

    protected override string? CanRunOnCurrentMachine() =>
        OperatingSystem.IsWindows() ? null : "Windows requires Windows (hardware requirement)";

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        options.PlatformName = "Windows";
        options.AutomationName = "Windows";
        options.App = appPath;
        options.DeviceName = "WindowsPC";
    }

    protected override AppiumDriver CreateDriver(AppiumOptions options) =>
        new WindowsDriver(new Uri($"http://127.0.0.1:{AppiumPort}"), options, TimeSpan.FromSeconds(120));

    protected override string? FindAppArtifact(string projectDir, string projectName) =>
        Directory.GetFiles(
            Path.Combine(projectDir, "bin", BuildConfiguration, TargetFramework),
            $"{projectName}.exe", SearchOption.AllDirectories).FirstOrDefault();

    /// <summary>
    /// Returns the canvas bounds in window-relative coordinates.
    ///
    /// WinUI MAUI apps don't expose individual page elements in the UIA tree — the entire page
    /// content is a single opaque Custom element with no children. We anchor on the commandBar
    /// AppBar (which IS in the UIA tree) to find where the content area starts, then calculate
    /// the canvas position geometrically. The MAUI XAML centers the canvas (VerticalOptions=Center,
    /// HorizontalOptions=Center) within the content area below the commandBar.
    /// </summary>
    protected override (SKPointI location, SKSizeI size) GetCanvasBounds(AppiumDriver driver, string bundleId)
    {
        var appBar = driver.FindElement("accessibility id", "commandBar");
        int contentTop = appBar.Location.Y + appBar.Size.Height;
        Output.WriteLine($"commandBar: y={appBar.Location.Y} h={appBar.Size.Height} → contentTop={contentTop}");

        var windowSize = driver.Manage().Window.Size;
        int x = (windowSize.Width  - CanvasWidth)  / 2;
        int y = contentTop + (windowSize.Height - contentTop - CanvasHeight) / 2;
        Output.WriteLine($"Canvas position: ({x},{y}) in {windowSize.Width}x{windowSize.Height} window");
        return (new SKPointI(x, y), new SKSizeI(CanvasWidth, CanvasHeight));
    }
}
