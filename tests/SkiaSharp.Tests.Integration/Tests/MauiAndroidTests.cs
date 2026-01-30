using System.Diagnostics;
using OpenQA.Selenium;
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

    /// <summary>
    /// On Android, MAUI's AutomationId maps to resource-id, not content-desc (accessibility id).
    /// Use the "id" locator strategy with the full resource ID.
    /// </summary>
    protected override IWebElement FindCanvasElement(AppiumDriver driver, string bundleId) =>
        driver.FindElement("id", $"{bundleId}:id/SkiaCanvas");

    /// <summary>
    /// Android-specific recovery: dismiss system dialogs and verify emulator is healthy.
    /// </summary>
    protected override async Task PerformRecoveryActions()
    {
        Output.WriteLine("Performing Android recovery actions...");
        
        try
        {
            // Find adb path
            var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME") 
                ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
                ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Android/sdk");
            
            var adbPath = Path.Combine(androidHome, "platform-tools", "adb");
            
            if (!File.Exists(adbPath))
            {
                Output.WriteLine($"adb not found at {adbPath}, skipping recovery");
                return;
            }
            
            // Check if emulator is connected
            var devices = await RunAdbCommand(adbPath, "devices");
            if (!devices.Contains("emulator") && !devices.Contains("device"))
            {
                Output.WriteLine("No Android device found. Emulator may need to be restarted manually.");
                return;
            }
            
            // Dismiss any system dialogs by pressing Back and Home
            Output.WriteLine("Dismissing any system dialogs...");
            await RunAdbCommand(adbPath, "shell input keyevent KEYCODE_BACK");
            await Task.Delay(500);
            await RunAdbCommand(adbPath, "shell input keyevent KEYCODE_HOME");
            await Task.Delay(1000);
            
            // Clear any ANR dialogs with specific button clicks
            // The "Wait" button in ANR dialogs typically has resource ID android:id/aerr_wait
            await RunAdbCommand(adbPath, "shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS");
            await Task.Delay(500);
            
            Output.WriteLine("Android recovery actions completed");
        }
        catch (Exception ex)
        {
            Output.WriteLine($"Recovery action failed (non-fatal): {ex.Message}");
        }
    }

    private async Task<string> RunAdbCommand(string adbPath, string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = adbPath,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        
        using var process = Process.Start(psi)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return output;
    }
}
