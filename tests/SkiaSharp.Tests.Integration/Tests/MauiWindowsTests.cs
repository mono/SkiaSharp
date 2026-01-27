using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
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
}
