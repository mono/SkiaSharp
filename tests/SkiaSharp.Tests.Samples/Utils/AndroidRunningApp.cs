using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp(
    SampleWorkspace workspace, string artifact, string applicationId, AndroidDevice device)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    private Android? android;
    private bool ownsApp;
    private string? activity;
    private static readonly string[] DriverPackages =
        ["io.appium.uiautomator2.server", "io.appium.uiautomator2.server.test", "io.appium.settings"];

    internal override PointerKind PointerKind => PointerKind.Touch;
    internal override By Locator(string id) => By.Id($"{ApplicationId}:id/{id}");
    internal override void SubmitSearch() => ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Enter);

    protected override async Task Start(Uri address)
    {
        Require(Path.GetExtension(Artifact).Equals(".apk", StringComparison.OrdinalIgnoreCase),
            "Android requires the exact built APK, not a generated replacement or an AAB.");
        Require(Regex.IsMatch(ApplicationId, @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$"),
            $"Invalid Android application ID: {ApplicationId}");
        ArgumentNullException.ThrowIfNull(device);
        var port = device.Validate();
        var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME")
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
            ?? throw new InvalidOperationException("Set ANDROID_HOME or ANDROID_SDK_ROOT to the explicitly installed Android SDK.");
        android = new Android(sdk);
        var manifest = XDocument.Parse(await CaptureCommand(android.Manifest(Artifact)));
        Require((string?)manifest.Root?.Attribute("package") == ApplicationId,
            $"The exact built APK does not declare application {ApplicationId}; refusing to install an unowned package.");
        AppiumFixture.RequireFreePort(AppiumFixture.AndroidPort);
        Require((await AdbForwards()).All(parts => parts[1] != $"tcp:{AppiumFixture.AndroidPort}"),
            $"An existing adb forward uses port {AppiumFixture.AndroidPort}; refusing to replace a foreign mapping.");

        using var avds = JsonDocument.Parse(await CaptureCommand(android.ListAvds()));
        Require(avds.RootElement.EnumerateArray().Count(avd => avd.GetProperty("name").GetString() == device.Name) == 1,
            $"Create exactly one explicitly selected {device.Name} AVD beforehand; no AVD is created or erased by tests.");
        using var devices = JsonDocument.Parse(await CaptureCommand(android.ListDevices()));
        var booted = devices.RootElement.EnumerateArray()
            .Any(item => item.GetProperty("serial").GetString() == device.Serial);
        if (booted)
        {
            Require(Environment.GetEnvironmentVariable("SKIASHARP_APPIUM_USE_BOOTED_DEVICE") == "1",
                $"Dedicate the already-booted {device.Serial} using SKIASHARP_APPIUM_USE_BOOTED_DEVICE=1. It will not be stopped.");
        }
        else
        {
            AppiumFixture.RequireFreePort(port);
            AppiumFixture.RequireFreePort(port + 1);
            var emulator = android.StartAvd(device);
            var ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var stdout = ReadBootOutput(emulator, ready);
            var stderr = emulator.StandardError.ReadToEndAsync();
            Own(() => StopProcess(emulator, stdout, stderr, "emulator"));
            await Task.WhenAny(ready.Task, emulator.WaitForExitAsync()).WaitAsync(TimeSpan.FromMinutes(6));
            Require(ready.Task.IsCompletedSuccessfully && !emulator.HasExited,
                $"Owned {device.Name} emulator did not report boot completion; inspect emulator logs.");
        }

        Require((await CaptureCommand(android.AvdName(device))).Split('\n')[0].Trim() == device.Name,
            $"Serial {device.Serial} is not the explicitly selected {device.Name}; refusing foreign device reuse.");
        await Property("sys.boot_completed", "1");
        await Property("ro.build.version.release", device.Version);
        await Property("ro.build.version.sdk", device.Api.ToString());
        Require((await CaptureCommand(android.PageSize(device))).Trim() == device.PageSize.ToString(),
            $"Selected {device.Serial} must use {device.PageSize}-byte pages.");
        foreach (var package in DriverPackages.Prepend(ApplicationId))
            Require(!await HasApp(package),
                $"Selected {device.Serial} already contains {package}; remove it explicitly or choose a clean dedicated device. Existing app/data will not be overwritten.");

        ownsApp = true;
        Own(() => RemoveApp(ApplicationId));
        await CaptureCommand(android.Install(device, Artifact));
        Require(await HasApp(ApplicationId), $"Built APK did not install declared application {ApplicationId}.");
        var resolved = (await CaptureCommand(android.LauncherActivity(device, ApplicationId)))
            .Split('\n', StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim() ?? "";
        Require(resolved.StartsWith(ApplicationId + "/", StringComparison.Ordinal),
            $"No launcher activity for the actual installed APK: {resolved}");
        activity = resolved[(ApplicationId.Length + 1)..];

        // These packages were absent at preflight. Appium may install them even when session creation fails.
        foreach (var package in DriverPackages)
            Own(() => RemoveApp(package));
        Own(RemoveForward);
        var options = Options("Android", "UiAutomator2");
        options.DeviceName = device.Name;
        options.PlatformVersion = device.Version;
        options.AddAdditionalAppiumOption("udid", device.Serial);
        options.AddAdditionalAppiumOption("appPackage", ApplicationId);
        options.AddAdditionalAppiumOption("appActivity", activity);
        options.AddAdditionalAppiumOption("systemPort", AppiumFixture.AndroidPort);
        options.AddAdditionalAppiumOption("noReset", true);
        options.AddAdditionalAppiumOption("autoLaunch", false);
        SetDriver(new AndroidDriver(address, options, TimeSpan.FromMinutes(3)));
        await CaptureCommand(android.Launch(device, ApplicationId, activity));
        Wait(() => Driver.PageSource.Contains($"package=\"{ApplicationId}\"", StringComparison.Ordinal)
            ? "foreground app" : null, $"{ApplicationId} foreground UI");
    }

    protected override async Task StopApplication()
    {
        if (ownsApp && await HasApp(ApplicationId))
            await CaptureCommand(Tools.ForceStop(device, ApplicationId));
    }

    protected override async Task CapturePlatformDiagnostics()
    {
        if (ownsApp)
            await CaptureCommand(Tools.RuntimeLogs(device));
    }

    private async Task Property(string name, string value) =>
        Require((await CaptureCommand(Tools.Property(device, name))).Trim() == value,
            $"Selected {device.Serial} must have {name}={value}.");

    private Android Tools => android ?? throw new InvalidOperationException("Android preflight has not completed.");

    private async Task<bool> HasApp(string appId)
    {
        using var apps = JsonDocument.Parse(await CaptureCommand(Tools.ListPackages(device)));
        return apps.RootElement.EnumerateArray().Any(app => app.GetProperty("packageName").GetString() == appId);
    }

    private async Task RemoveApp(string appId)
    {
        if (!await HasApp(appId))
            return;
        await CaptureCommand(Tools.Uninstall(device, appId));
        Require(!await HasApp(appId), $"Owned package {appId} was not removed from {device.Serial}.");
    }

    private async Task<string[][]> AdbForwards()
    {
        var rows = (await CaptureCommand(Tools.ListForwards())).Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
        Require(rows.All(parts => parts.Length == 3), "adb returned an unrecognized forward list; refusing uncertain mapping ownership.");
        return rows;
    }

    private async Task RemoveForward()
    {
        foreach (var parts in (await AdbForwards()).Where(parts => parts[1] == $"tcp:{AppiumFixture.AndroidPort}"))
        {
            Require(parts[0] == device.Serial,
                $"Port {AppiumFixture.AndroidPort} now belongs to foreign device {parts[0]}; refusing to remove its adb forward.");
            await CaptureCommand(Tools.RemoveForward(device, AppiumFixture.AndroidPort));
        }
        Require((await AdbForwards()).All(parts => parts[1] != $"tcp:{AppiumFixture.AndroidPort}"),
            $"Owned adb forward on {AppiumFixture.AndroidPort} was not removed.");
    }

    private static async Task<string> ReadBootOutput(Process process, TaskCompletionSource ready)
    {
        var text = new StringBuilder();
        while (await process.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
        {
            text.AppendLine(line);
            if (line.Contains("Boot wait completed in", StringComparison.Ordinal))
                ready.TrySetResult();
        }
        return text.ToString();
    }
}
