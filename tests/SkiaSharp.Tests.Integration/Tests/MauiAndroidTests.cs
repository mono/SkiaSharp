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
///   dotnet test -p:AndroidDeviceId="emulator-5554" -p:AndroidApiLevel="23"
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
    
    // New: Specific device UDID (e.g., "emulator-5554") and expected API level
    private static string? DeviceUdid => 
        AppContext.GetData("AndroidDeviceId") as string is { Length: > 0 } udid ? udid : null;
    private static string? ExpectedApiLevel => 
        AppContext.GetData("AndroidApiLevel") as string is { Length: > 0 } api ? api : null;
    
    protected override string PlatformName => "Android";
    protected override string TargetFramework => "net10.0-android";

    /// <summary>
    /// Get the ADB path for this machine.
    /// </summary>
    private static string GetAdbPath()
    {
        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME") 
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Android/sdk");
        return Path.Combine(androidHome, "platform-tools", "adb");
    }

    /// <summary>
    /// Perform preflight checks before running tests.
    /// Validates device availability and API level.
    /// </summary>
    protected override async Task PerformPreflightChecks()
    {
        var adbPath = GetAdbPath();
        if (!File.Exists(adbPath))
        {
            Output.WriteLine($"Warning: adb not found at {adbPath}");
            return;
        }

        // Get connected devices
        var devicesOutput = await RunAdbCommand(adbPath, "devices -l");
        var lines = devicesOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.Contains("device") && !l.StartsWith("List"))
            .ToList();

        Output.WriteLine($"Connected Android devices: {lines.Count}");
        foreach (var line in lines)
            Output.WriteLine($"  {line.Trim()}");

        // Check for multiple devices without specific UDID
        if (lines.Count > 1 && string.IsNullOrEmpty(DeviceUdid))
        {
            throw new InvalidOperationException(
                $"Multiple Android devices connected ({lines.Count}). " +
                $"Specify -p:AndroidDeviceId=<udid> to select one.\n" +
                $"Available devices:\n{string.Join("\n", lines.Select(l => "  " + l.Trim()))}");
        }

        // If specific device requested, verify it exists
        if (!string.IsNullOrEmpty(DeviceUdid))
        {
            var deviceExists = lines.Any(l => l.Contains(DeviceUdid));
            if (!deviceExists)
            {
                throw new InvalidOperationException(
                    $"Requested device '{DeviceUdid}' not found.\n" +
                    $"Available devices:\n{string.Join("\n", lines.Select(l => "  " + l.Trim()))}");
            }
            Output.WriteLine($"✓ Device {DeviceUdid} is available");
        }
    }

    /// <summary>
    /// Get the actual API level of the connected device for screenshot naming.
    /// Returns format like "api23" or "api36".
    /// </summary>
    protected override async Task<string?> GetDeviceVersionAsync()
    {
        var rawLevel = await GetRawApiLevelAsync();
        return !string.IsNullOrEmpty(rawLevel) ? $"api{rawLevel}" : null;
    }

    /// <summary>
    /// Get the raw API level number (e.g., "23", "36").
    /// </summary>
    private async Task<string?> GetRawApiLevelAsync()
    {
        var adbPath = GetAdbPath();
        if (!File.Exists(adbPath))
            return null;

        var deviceArg = !string.IsNullOrEmpty(DeviceUdid) ? $"-s {DeviceUdid}" : "";
        var apiLevel = await RunAdbCommand(adbPath, $"{deviceArg} shell getprop ro.build.version.sdk");
        return apiLevel.Trim();
    }

    /// <summary>
    /// Validate that connected device matches expected configuration.
    /// </summary>
    protected override async Task ValidateDeviceAsync()
    {
        var actualApiLevel = await GetRawApiLevelAsync();
        
        if (!string.IsNullOrEmpty(actualApiLevel))
        {
            Output.WriteLine($"Device API level: {actualApiLevel}");
            
            if (!string.IsNullOrEmpty(ExpectedApiLevel) && actualApiLevel != ExpectedApiLevel)
            {
                throw new InvalidOperationException(
                    $"API level mismatch! Expected API {ExpectedApiLevel} but device has API {actualApiLevel}. " +
                    $"Wrong emulator selected?");
            }
            
            if (!string.IsNullOrEmpty(ExpectedApiLevel))
                Output.WriteLine($"✓ API level verified: {actualApiLevel}");
        }
    }

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        Output.WriteLine($"Android Device: {DeviceName}, Version: {PlatformVersion ?? "(default)"}");
        if (!string.IsNullOrEmpty(DeviceUdid))
            Output.WriteLine($"Device UDID: {DeviceUdid}");
        if (!string.IsNullOrEmpty(ExpectedApiLevel))
            Output.WriteLine($"Expected API Level: {ExpectedApiLevel}");
        
        options.PlatformName = "Android";
        options.AutomationName = "UiAutomator2";
        options.App = appPath;
        options.DeviceName = DeviceName;
        
        // If specific device UDID requested, use it
        if (!string.IsNullOrEmpty(DeviceUdid))
            options.AddAdditionalAppiumOption("udid", DeviceUdid);
        
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
            var adbPath = GetAdbPath();
            
            if (!File.Exists(adbPath))
            {
                Output.WriteLine($"adb not found at {adbPath}, skipping recovery");
                return;
            }
            
            var deviceArg = !string.IsNullOrEmpty(DeviceUdid) ? $"-s {DeviceUdid}" : "";
            
            // Check if emulator is connected
            var devices = await RunAdbCommand(adbPath, "devices");
            if (!devices.Contains("emulator") && !devices.Contains("device"))
            {
                Output.WriteLine("No Android device found. Emulator may need to be restarted manually.");
                return;
            }
            
            // Dismiss any system dialogs by pressing Back and Home
            Output.WriteLine("Dismissing any system dialogs...");
            await RunAdbCommand(adbPath, $"{deviceArg} shell input keyevent KEYCODE_BACK");
            await Task.Delay(500);
            await RunAdbCommand(adbPath, $"{deviceArg} shell input keyevent KEYCODE_HOME");
            await Task.Delay(1000);
            
            // Clear any ANR dialogs with specific button clicks
            await RunAdbCommand(adbPath, $"{deviceArg} shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS");
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
