using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

/// <summary>
/// Tests that verify SkiaSharp packages work in MAUI Android applications.
/// </summary>
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public class MauiAndroidTests(AppiumFixture appium, ITestOutputHelper output) : MauiTestBase(appium, output)
{
    private sealed record DeviceCase(string Name, string Version, string Serial, int ApiLevel);
    private DeviceCase? device;
    private DeviceCase Device => device ?? throw new InvalidOperationException("The Android theory must declare its device.");

    [Theory]
    [InlineData("SKCanvasView", "SKPaintSurfaceEventArgs", TestDevices.AndroidName, TestDevices.AndroidVersion,
        TestDevices.AndroidSerial, TestDevices.AndroidApiLevel)]
    [InlineData("SKGLView", "SKPaintGLSurfaceEventArgs", TestDevices.AndroidName, TestDevices.AndroidVersion,
        TestDevices.AndroidSerial, TestDevices.AndroidApiLevel)]
    public Task MatchesGolden(string view, string eventArgs, string name, string version, string serial, int apiLevel)
    {
        device = new(name, version, serial, apiLevel);
        return RunMauiTest(view, eventArgs);
    }
    
    protected override string PlatformName => "Android";
    protected override string TargetFramework => $"{BaseFramework}-android";

    /// <summary>
    /// Get the ADB path for this machine.
    /// </summary>
    private static string GetAdbPath()
    {
        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME") 
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
            ?? Environment.GetEnvironmentVariable("AndroidSdkDirectory")
            ?? (OperatingSystem.IsWindows()
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk")
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    OperatingSystem.IsMacOS() ? "Library/Android/sdk" : "Android/Sdk"));
        return Path.Combine(androidHome, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
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
        // Get connected devices
        var devicesOutput = await RunAdbCommand(adbPath, "devices", "-l");
        var lines = devicesOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(l => l.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) is [_, "device", ..])
            .ToList();

        Output.WriteLine($"Connected Android devices: {lines.Count}");
        foreach (var line in lines)
            Output.WriteLine($"  {line.Trim()}");

        var deviceExists = lines.Any(l => l.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)[0] == Device.Serial);
        if (!deviceExists)
            throw new InvalidOperationException(
                $"Required Android emulator '{Device.Name}' at '{Device.Serial}' is not ready. Start that exact AVD and rerun.\n" +
                $"Available devices:\n{string.Join("\n", lines.Select(l => "  " + l.Trim()))}");
        var avd = await RunAdbCommand(adbPath, "-s", Device.Serial, "emu", "avd", "name");
        if (avd.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim() != Device.Name)
            throw new InvalidOperationException($"Expected AVD '{Device.Name}' at {Device.Serial}, got:\n{avd}");
        await ValidateDeviceAsync();
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

        var args = new[] { "-s", Device.Serial, "shell", "getprop", "ro.build.version.sdk" };
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
        if (actualApiLevel != Device.ApiLevel.ToString(System.Globalization.CultureInfo.InvariantCulture))
            throw new InvalidOperationException(
                $"Expected Android API {Device.ApiLevel} on {Device.Serial}, got {actualApiLevel}.");
        var version = (await RunAdbCommand(GetAdbPath(), "-s", Device.Serial, "shell", "getprop", "ro.build.version.release")).Trim();
        if (version != Device.Version)
            throw new InvalidOperationException($"Expected Android {Device.Version} on {Device.Serial}, got {version}.");
    }

    protected override void ConfigureAppiumOptions(AppiumOptions options, string appPath, string bundleId)
    {
        Output.WriteLine($"Android: {Device.Name}, version {Device.Version}, API {Device.ApiLevel}, serial {Device.Serial}");
        
        options.PlatformName = "Android";
        options.AutomationName = "UiAutomator2";
        options.App = appPath;
        options.DeviceName = Device.Name;
        options.AddAdditionalAppiumOption("udid", Device.Serial);
        options.PlatformVersion = Device.Version;
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
        
        var adbPath = GetAdbPath();
        await RunAdbCommand(adbPath, "-s", Device.Serial, "shell", "input", "keyevent", "KEYCODE_BACK");
        await Task.Delay(500);
        await RunAdbCommand(adbPath, "-s", Device.Serial, "shell", "input", "keyevent", "KEYCODE_HOME");
        Output.WriteLine($"Android recovery actions completed for {Device.Serial}");
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
