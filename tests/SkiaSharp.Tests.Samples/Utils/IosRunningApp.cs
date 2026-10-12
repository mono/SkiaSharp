using System.Text.Json;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp(
    SampleWorkspace workspace, string artifact, string applicationId, IosDevice device)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    private string? udid;
    private bool ownsApp;
    private string? executableName;
    private readonly string wdaBundleId = $"com.skiasharp.samples.tests.wda{Guid.NewGuid():N}";
    private readonly DateTime started = DateTime.Now;

    internal override PointerKind PointerKind => PointerKind.Touch;
    internal override void SubmitSearch() =>
        Find(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND name == 'Return'")).Click();

    protected override async Task Start(Uri address)
    {
        Require(OperatingSystem.IsMacOS(), "Selected iOS tests require macOS with the exact simulator runtime and Xcode installed.");
        Require(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
            "iOS simulator tests require the actual built simulator .app bundle.");
        ArgumentNullException.ThrowIfNull(device);
        device.Validate();
        Require(Regex.IsMatch(ApplicationId, @"^[A-Za-z0-9-]+(\.[A-Za-z0-9-]+)+$"),
            $"Invalid bundle ID: {ApplicationId}");
        Require((await CaptureCommand(Apple.BundleIdentifier(Artifact))).Trim() == ApplicationId,
            $"The exact built simulator bundle does not declare {ApplicationId}; refusing installation.");
        executableName = (await CaptureCommand(Apple.BundleExecutable(Artifact))).Trim();
        AppiumFixture.RequireFreePort(AppiumFixture.IosPort);
        var selected = RequireSimulator(await ListDevices(), device.Name, device.Version);
        udid = selected.Id;
        Require(selected.State is "Shutdown" or "Booted",
            $"Selected simulator {udid} is {selected.State}; stabilize its state before running tests.");
        if (selected.State == "Shutdown")
        {
            Own(async () =>
            {
                var current = RequireSimulator(await ListDevices(), device.Name, device.Version);
                Require(current.Id == udid, "Selected simulator identity changed; refusing to shut down a different device.");
                if (current.State != "Shutdown")
                    await CaptureCommand(Apple.ShutdownSimulator(udid));
                Require(RequireSimulator(await ListDevices(), device.Name, device.Version) == (udid, "Shutdown"),
                    $"Owned simulator {udid} did not return to its original Shutdown state.");
            });
            await CaptureCommand(Apple.BootSimulator(udid));
        }
        else
        {
            Require(Environment.GetEnvironmentVariable("SKIASHARP_APPIUM_USE_BOOTED_DEVICE") == "1",
                $"Dedicate already-booted simulator {udid} with SKIASHARP_APPIUM_USE_BOOTED_DEVICE=1. Its original Booted state will be retained.");
            // Never call boot on an already Booted simulator. Querying its app service checks readiness.
        }
        Require(RequireSimulator(await ListDevices(), device.Name, device.Version) == (udid, "Booted"),
            $"Selected simulator {udid} did not become Booted.");
        var installed = await Apps();
        Require(!installed.Contains(ApplicationId), $"Simulator already contains {ApplicationId}; refusing to overwrite existing app/data.");
        Require(!installed.Any(id => id.Contains("WebDriverAgent", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("com.skiasharp.samples.tests.wda", StringComparison.Ordinal)),
            "Simulator contains a WDA runner; remove the conflicting installation explicitly before running tests.");

        ownsApp = true;
        Own(() => RemoveApp(ApplicationId));
        await CaptureCommand(Apple.Install(udid, Artifact));
        Require((await Apps()).Contains(ApplicationId), $"Built .app did not install declared bundle {ApplicationId}.");
        Own(() => RemoveApp(wdaBundleId + ".xctrunner"));
        var options = Options("iOS", "XCUITest");
        options.DeviceName = device.Name;
        options.PlatformVersion = device.Version;
        options.AddAdditionalAppiumOption("udid", udid);
        options.AddAdditionalAppiumOption("bundleId", ApplicationId);
        options.AddAdditionalAppiumOption("noReset", true);
        options.AddAdditionalAppiumOption("autoLaunch", false);
        options.AddAdditionalAppiumOption("useNewWDA", true);
        options.AddAdditionalAppiumOption("wdaStartupRetries", 1);
        options.AddAdditionalAppiumOption("wdaLocalPort", AppiumFixture.IosPort);
        options.AddAdditionalAppiumOption("wdaRemotePort", AppiumFixture.IosPort);
        options.AddAdditionalAppiumOption("updatedWDABundleId", wdaBundleId);
        options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(Workspace.Root, "wda-ios"));
        options.AddAdditionalAppiumOption("showXcodeLog", true);
        SetDriver(new IOSDriver(address, options, TimeSpan.FromMinutes(6)));
        await CaptureCommand(Apple.Launch(udid, ApplicationId));
        Wait(() => Driver.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeApplication"))
            is { Displayed: true } element ? element : null, $"{ApplicationId} foreground UI");
    }

    protected override async Task CapturePlatformDiagnostics()
    {
        if (ownsApp)
            await CaptureCommand(Apple.RuntimeLogs(udid!, started, executableName!));
    }

    // Uninstall stops the newly-owned application even if it crashed or never launched.
    private async Task RemoveApp(string appId)
    {
        if (!(await Apps()).Contains(appId))
            return;
        await CaptureCommand(Apple.Uninstall(udid!, appId));
        Require(!(await Apps()).Contains(appId), $"Owned bundle {appId} was not removed from {udid}.");
    }

    private async Task<HashSet<string>> Apps()
    {
        using var apps = JsonDocument.Parse(await CaptureCommand(Apple.ListApps(udid!)));
        return apps.RootElement.EnumerateArray().Select(app => app.GetProperty("CFBundleIdentifier").GetString()!)
            .ToHashSet(StringComparer.Ordinal);
    }

    private Task<string> ListDevices() => CaptureCommand(Apple.ListSimulators(device));

    internal static (string Id, string State) RequireSimulator(string json, string name, string version)
    {
        using var devices = JsonDocument.Parse(json);
        var matches = devices.RootElement.EnumerateArray().Where(item =>
            item.GetProperty("name").GetString() == name && item.GetProperty("isAvailable").GetBoolean() &&
            item.GetProperty("runtime").GetProperty("name").GetString() == $"iOS {version}" &&
            item.GetProperty("runtime").GetProperty("version").GetString() == version).ToArray();
        if (matches.Length != 1)
            throw new InvalidOperationException(
                $"Required exactly one available '{name}' simulator on iOS {version}, found {matches.Length}. Install the exact runtime/device; no fallback is selected.");
        return (matches[0].GetProperty("udid").GetString() ?? throw new InvalidOperationException("Simulator UDID is missing."),
            matches[0].GetProperty("state").GetString() ?? "");
    }
}
