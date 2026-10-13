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
    private bool booted;
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
        device = device with { Name = $"{device.Name}-test-{Guid.NewGuid():N}" };
        using (var existing = JsonDocument.Parse(await ListDevices()))
            Require(existing.RootElement.GetArrayLength() == 0,
                $"Owned simulator name already exists: {device.Name}; refusing reuse.");
        Own(async () =>
        {
            using var current = JsonDocument.Parse(await ListDevices());
            var matches = current.RootElement.EnumerateArray()
                .Where(item => item.GetProperty("name").GetString() == device.Name).ToArray();
            Require(matches.Length <= 1, $"Owned simulator name is ambiguous: {device.Name}");
            if (matches.Length == 1)
            {
                var ownedId = matches[0].GetProperty("udid").GetString()
                    ?? throw new InvalidOperationException("Created simulator UDID is missing.");
                Require(udid is null || ownedId == udid, "Owned simulator identity changed; refusing deletion.");
                List<Exception> failures = [];
                if (matches[0].GetProperty("state").GetString() != "Shutdown")
                    await Attempt(failures, async () => await CaptureCommand(Apple.ShutdownSimulator(ownedId)));
                await Attempt(failures, async () => await CaptureCommand(Apple.DeleteSimulator(ownedId)));
                ThrowFailures(failures);
            }
            using var remaining = JsonDocument.Parse(await ListDevices());
            Require(remaining.RootElement.GetArrayLength() == 0, $"Owned simulator was not deleted: {device.Name}");
        });
        await CaptureCommand(Apple.CreateSimulator(device));
        var selected = RequireSimulator(await ListDevices(), device.Name, device.Version);
        udid = selected.Id;
        Require(selected.State == "Shutdown", $"New simulator {udid} must start Shutdown, not {selected.State}.");
        await CaptureCommand(Apple.BootSimulator(udid));
        Require(RequireSimulator(await ListDevices(), device.Name, device.Version) == (udid, "Booted"),
            $"Selected simulator {udid} did not become Booted.");
        booted = true;
        var options = Options("iOS", "XCUITest");
        options.App = Artifact;
        options.DeviceName = device.Name;
        options.PlatformVersion = device.Version;
        options.AddAdditionalAppiumOption("udid", udid);
        options.AddAdditionalAppiumOption("bundleId", ApplicationId);
        options.AddAdditionalAppiumOption("noReset", false);
        options.AddAdditionalAppiumOption("fullReset", true);
        options.AddAdditionalAppiumOption("useNewWDA", true);
        options.AddAdditionalAppiumOption("wdaStartupRetries", 1);
        options.AddAdditionalAppiumOption("wdaLocalPort", AppiumFixture.IosPort);
        options.AddAdditionalAppiumOption("wdaRemotePort", AppiumFixture.IosPort);
        options.AddAdditionalAppiumOption("updatedWDABundleId", wdaBundleId);
        options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(Workspace.Root, "wda-ios"));
        options.AddAdditionalAppiumOption("showXcodeLog", true);
        SetDriver(new IOSDriver(address, options, TimeSpan.FromMinutes(6)));
        Wait(() => Driver.FindElement(OpenQA.Selenium.By.XPath("//XCUIElementTypeApplication"))
            is { Displayed: true } element ? element : null, $"{ApplicationId} foreground UI");
    }

    protected override async Task CapturePlatformDiagnostics()
    {
        if (booted)
            await CaptureCommand(Apple.RuntimeLogs(udid!, started, executableName!));
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
