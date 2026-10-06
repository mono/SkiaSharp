using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Android applications.
/// Device and version can be configured via MSBuild properties:
///   dotnet test -p:AndroidDevice="Pixel_API_26" -p:AndroidVersion="8.0"
///   dotnet test -p:AndroidDeviceId="emulator-5554" -p:AndroidApiLevel="26"
/// </summary>
[Trait("Category", "ManualPlatform")]
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
    internal static string? ExpectedApiLevel =>
        AppContext.GetData("AndroidApiLevel")?.ToString() is { Length: > 0 } api ? api : null;
    
    protected override string PlatformName => "Android";
    protected override string TargetFramework => $"{BaseFramework}-android";

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
            throw new FileNotFoundException("Android preflight requires adb", adbPath);
        if (DeviceUdid is not { Length: > 0 })
            throw new InvalidOperationException("Specify -p:AndroidDeviceId=<selected-device-serial>; refusing to select another connected device.");

        // Get connected devices
        var devicesOutput = await RunAdbCommand(adbPath, "devices", "-l");
        var lines = devicesOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) is [_, "device", ..])
            .ToList();

        Output.WriteLine($"Connected Android devices: {lines.Count}");
        foreach (var line in lines)
            Output.WriteLine($"  {line.Trim()}");

        var deviceExists = lines.Any(l => l.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0] == DeviceUdid);
        if (!deviceExists)
            throw new InvalidOperationException(
                $"Requested device '{DeviceUdid}' not found or not ready.\n" +
                $"Available devices:\n{string.Join("\n", lines.Select(l => "  " + l.Trim()))}");
        Output.WriteLine($"✓ Device {DeviceUdid} is available");
    }

    /// <summary>
    /// Get the actual API level of the connected device for screenshot naming.
    /// Returns format like "api26" or "api36".
    /// </summary>
    protected override async Task<string?> GetDeviceVersionAsync()
    {
        var rawLevel = await GetRawApiLevelAsync();
        return !string.IsNullOrEmpty(rawLevel) ? $"api{rawLevel}" : null;
    }

    /// <summary>
    /// Get the raw API level number (e.g., "26", "36").
    /// </summary>
    private async Task<string?> GetRawApiLevelAsync()
    {
        var adbPath = GetAdbPath();
        if (!File.Exists(adbPath))
            throw new FileNotFoundException("Android device validation requires adb", adbPath);

        var args = DeviceUdid is { Length: > 0 } udid
            ? new[] { "-s", udid, "shell", "getprop", "ro.build.version.sdk" }
            : new[] { "shell", "getprop", "ro.build.version.sdk" };
        var apiLevel = await RunAdbCommand(adbPath, args);
        return apiLevel.Trim();
    }

    /// <summary>
    /// Validate that connected device matches expected configuration.
    /// </summary>
    protected override async Task ValidateDeviceAsync()
    {
        var actualApiLevel = await GetRawApiLevelAsync();
        if (string.IsNullOrEmpty(actualApiLevel))
            throw new InvalidOperationException("Could not determine the selected Android device's API level.");
        Output.WriteLine($"Device API level: {actualApiLevel}");
        if (!string.IsNullOrEmpty(ExpectedApiLevel) && actualApiLevel != ExpectedApiLevel)
            throw new InvalidOperationException(
                $"API level mismatch! Expected API {ExpectedApiLevel} but device has API {actualApiLevel}. " +
                "Wrong emulator selected?");
        if (!string.IsNullOrEmpty(ExpectedApiLevel))
            Output.WriteLine($"✓ API level verified: {actualApiLevel}");
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
    /// Android-specific recovery is limited to a device explicitly selected for this run.
    /// </summary>
    protected override async Task PerformRecoveryActions()
    {
        Output.WriteLine("Performing Android recovery actions...");
        
        if (DeviceUdid is not { Length: > 0 } udid)
        {
            Output.WriteLine("No explicitly selected Android device; recovery skipped.");
            return;
        }

        var adbPath = GetAdbPath();
        await RunAdbCommand(adbPath, "-s", udid, "shell", "input", "keyevent", "KEYCODE_BACK");
        await Task.Delay(500);
        await RunAdbCommand(adbPath, "-s", udid, "shell", "input", "keyevent", "KEYCODE_HOME");
        Output.WriteLine($"Android recovery actions completed for {udid}");
    }

    private async Task<string> RunAdbCommand(string adbPath, params string[] args)
    {
        if (!File.Exists(adbPath))
            throw new FileNotFoundException("Android test requires adb", adbPath);
        var (exitCode, output, error) = await DotNet.RunProcess(adbPath, args, TestDir, TimeSpan.FromSeconds(30), Output);
        if (exitCode != 0)
            throw new InvalidOperationException($"adb {string.Join(" ", args)} failed ({exitCode}):\n{output}\n{error}");
        return output;
    }
}
