using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Xunit;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Android applications.
/// Device and version can be configured via MSBuild properties:
///   dotnet test -p:AndroidDevice="Pixel_API_26" -p:AndroidVersion="8.0"
///   dotnet test -p:AndroidDeviceId="emulator-5554" -p:AndroidApiLevel="26"
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
    /// Perform preflight checks before running tests.
    /// Validates device availability and API level.
    /// </summary>
    protected override async Task PerformPreflightChecks()
    {
        using var devices = JsonDocument.Parse(await RunAndroidCommand("device", "list", "--format", "json"));
        var lines = devices.RootElement.EnumerateArray()
            .Select(device => device.GetProperty("serial").GetString()
                ?? throw new InvalidOperationException("Android device inventory is missing a serial.")).ToList();

        Output.WriteLine($"Connected Android devices: {lines.Count}");
        if (lines.Count == 0)
            throw new InvalidOperationException("No Android devices are connected.");
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
            var deviceExists = lines.Contains(DeviceUdid, StringComparer.Ordinal);
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
        var arguments = new List<string> {
            "device", "info", "--property", "^ro\\.build\\.version\\.sdk$", "--format", "json"
        };
        if (!string.IsNullOrEmpty(DeviceUdid))
            arguments.AddRange(["--id", "^" + Regex.Escape(DeviceUdid) + "$"]);
        using var json = JsonDocument.Parse(await RunAndroidCommand(arguments.ToArray()));
        var device = json.RootElement.EnumerateArray().Single();
        return device.GetProperty("properties").GetProperty("ro.build.version.sdk").GetString();
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

    private async Task<string> RunAndroidCommand(params string[] arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = FindRepoRoot(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        psi.ArgumentList.Add("android");
        foreach (var argument in arguments)
            psi.ArgumentList.Add(argument);
        if (Environment.GetEnvironmentVariable("ANDROID_HOME") is { Length: > 0 } sdk)
        {
            psi.ArgumentList.Add("--home");
            psi.ArgumentList.Add(sdk);
        }
        
        using var process = Process.Start(psi)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        var output = await stdout;
        var error = await stderr;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet android failed: {output}\n{error}");
        return output;
    }
}
