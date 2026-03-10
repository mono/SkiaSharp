using System.Diagnostics;
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

    /// <summary>
    /// Mac Catalyst recovery: kill only test-related stale processes.
    /// NOTE: We intentionally avoid destructive actions like:
    /// - tccutil reset (would affect ALL apps' permissions)
    /// - killing all xcodebuild (could kill unrelated builds)
    /// </summary>
    protected override async Task PerformRecoveryActions()
    {
        Output.WriteLine("Performing Mac Catalyst recovery actions...");
        
        try
        {
            // Find and terminate only WebDriverAgentRunner processes (test-specific)
            // This is safe because these are only created by Appium for testing
            var processes = Process.GetProcessesByName("WebDriverAgentRunner");
            foreach (var p in processes)
            {
                Output.WriteLine($"Terminating stale WebDriverAgentRunner process (PID: {p.Id})");
                try { p.Kill(); } catch { }
            }
            
            // Give time for cleanup
            await Task.Delay(2000);
            
            Output.WriteLine("Mac Catalyst recovery actions completed");
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Recovery action failed (non-fatal): {ex.Message}");
        }
    }
}
