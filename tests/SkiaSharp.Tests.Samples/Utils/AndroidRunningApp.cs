using System.Globalization;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp(SampleWorkspace workspace, string artifact, string applicationId, AndroidDevice device)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    internal override PointerKind PointerKind => PointerKind.Touch;

    internal override By Locator(string id) => By.Id($"{ApplicationId}:id/{id}");

    internal override void SubmitSearch() =>
        ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Enter);

    protected override async Task Start(Uri address)
    {
        Require(Path.GetExtension(Artifact).Equals(".apk", StringComparison.OrdinalIgnoreCase), "Provide the actual built APK, not an AAB or synthetic app.");
        var port = device.Validate();
        var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME")
            ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
            ?? throw new InvalidOperationException("Set ANDROID_HOME or ANDROID_SDK_ROOT to the preinstalled SDK.");
        var android = new Android(sdk);
        AppiumFixture.RequireFreePort(AppiumFixture.AndroidPort);
        AppiumFixture.RequireFreePort(port);
        AppiumFixture.RequireFreePort(port + 1);
        using (var connected = JsonDocument.Parse(await CaptureCommand(android.ListDevices())))
            Require(!HasSerial(connected), $"Serial {device.Serial} is occupied; existing devices are never reused.");
        device = device with { Name = $"{device.Name}-test-{Guid.NewGuid():N}" };
        using (var avds = JsonDocument.Parse(await CaptureCommand(android.ListAvds())))
            Require(!HasAvd(avds), $"Owned AVD name already exists: {device.Name}");
        Own(async () =>
        {
            using var current = JsonDocument.Parse(await CaptureCommand(android.ListAvds()));
            if (HasAvd(current))
            {
                using var connected = JsonDocument.Parse(await CaptureCommand(android.ListDevices()));
                if (HasSerial(connected))
                    Require((await CaptureCommand(android.AvdName(device.Serial))).Split('\n')[0].Trim() == device.Name,
                        "The serial now belongs to another AVD; refusing cleanup.");
                await CaptureCommand(android.DeleteAvd(device.Name));
            }
            using var remaining = JsonDocument.Parse(await CaptureCommand(android.ListAvds()));
            Require(!HasAvd(remaining), $"Owned AVD was not deleted: {device.Name}");
            using var devices = JsonDocument.Parse(await CaptureCommand(android.ListDevices()));
            Require(!HasSerial(devices), $"Owned emulator did not stop: {device.Serial}");
        });
        await CaptureCommand(android.CreateAvd(device.Name, device.ImagePrefix, device.Model));
        using (var created = JsonDocument.Parse(await CaptureCommand(android.ListAvds())))
            Require(HasAvd(created), $"The tool did not create {device.Name}");
        var options = Options("Android", "UiAutomator2");
        options.App = Artifact;
        options.DeviceName = device.Name;
        options.PlatformVersion = device.Version;
        options.AddAdditionalAppiumOption("udid", device.Serial);
        options.AddAdditionalAppiumOption("avd", device.Name);
        options.AddAdditionalAppiumOption("avdArgs", new[]
        {
            "-port", port.ToString(CultureInfo.InvariantCulture), "-no-snapshot", "-no-boot-anim"
        });
        options.AddAdditionalAppiumOption("appPackage", ApplicationId);
        options.AddAdditionalAppiumOption("systemPort", AppiumFixture.AndroidPort);
        options.AddAdditionalAppiumOption("suppressKillServer", true);
        options.AddAdditionalAppiumOption("fullReset", true);
        SetDriver(new AndroidDriver(address, options, TimeSpan.FromMinutes(3)));
        Require((await CaptureCommand(android.PageSize(device.Serial))).Trim() == device.PageSize.ToString(CultureInfo.InvariantCulture),
            "The selected Android page size does not match the profile.");
        Wait(() => Driver.PageSource.Contains($"package=\"{ApplicationId}\"", StringComparison.Ordinal)
            ? "foreground app" : null, $"{ApplicationId} foreground UI");

        bool HasSerial(JsonDocument devices) => devices.RootElement.EnumerateArray()
            .Any(item => item.GetProperty("serial").GetString() == device.Serial);
        bool HasAvd(JsonDocument avds) => avds.RootElement.EnumerateArray()
            .Any(item => item.GetProperty("name").GetString() == device.Name);
    }

    protected override Task CapturePlatformDiagnostics() =>
        SaveText("device.log", string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("logcat")));
}
