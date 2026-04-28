using OpenQA.Selenium;
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
    /// On Windows, WinAppDriver's GetScreenshot() captures the app window itself (not the full
    /// desktop), and all element coordinates are window-relative with (0,0) at the window's
    /// top-left. So canvas window-relative coords map directly to screenshot coordinates — no
    /// screen-position offset or DPI scaling is needed.
    ///
    /// WinUI raw rendering surfaces (SKCanvasView, SKGLView) don't implement UIA automation
    /// peers, so FindElement by AccessibilityId fails. We fall back to a geometric calculation
    /// using the commandBar AppBar, which IS accessible, to find where content starts.
    /// </summary>
    protected override (SKPointI location, SKSizeI size) GetCanvasBounds(AppiumDriver driver, string bundleId)
    {
        // UIA lookup first — works if the app exposes an accessible SkiaCanvas element
        try
        {
            var element = driver.FindElement("accessibility id", "SkiaCanvas");
            Output.WriteLine($"Found SkiaCanvas via UIA: {element.Location} {element.Size}");
            return (new SKPointI(element.Location.X, element.Location.Y),
                    new SKSizeI(element.Size.Width, element.Size.Height));
        }
        catch (NoSuchElementException)
        {
            Output.WriteLine("AutomationId 'SkiaCanvas' not in UIA tree — WinUI raw surfaces lack automation peers. Using geometric fallback.");
        }

        // Geometric fallback: canvas is centered in the content area below commandBar.
        // commandBar.Location is window-relative (WinAppDriver session coords = window coords).
        int contentTop = 100; // safe default matching the MAUI app layout
        try
        {
            var appBar = driver.FindElement("accessibility id", "commandBar");
            contentTop = appBar.Location.Y + appBar.Size.Height;
            Output.WriteLine($"commandBar window-relative: y={appBar.Location.Y} h={appBar.Size.Height} → contentTop={contentTop}");
        }
        catch (NoSuchElementException)
        {
            Output.WriteLine($"commandBar not found; using default contentTop={contentTop}");
        }

        var windowSize = driver.Manage().Window.Size;
        int x = (windowSize.Width  - CanvasWidth)  / 2;
        int y = contentTop + (windowSize.Height - contentTop - CanvasHeight) / 2;
        Output.WriteLine($"Geometric canvas: ({x},{y}) in {windowSize.Width}x{windowSize.Height} window");
        return (new SKPointI(x, y), new SKSizeI(CanvasWidth, CanvasHeight));
    }
}
