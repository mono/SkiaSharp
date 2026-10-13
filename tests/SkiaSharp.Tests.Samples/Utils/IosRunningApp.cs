using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp(SampleWorkspace workspace, string artifact, string applicationId, IosDevice device)
    : AppiumRunningApp(workspace, artifact, applicationId)
{
    internal override PointerKind PointerKind => PointerKind.Touch;

    internal override void SubmitSearch() =>
        Find(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND name == 'Return'")).Click();

    protected override Task Start(Uri address)
    {
        Require(OperatingSystem.IsMacOS(), "iOS tests require macOS, Xcode and the declared preinstalled simulator runtime.");
        Require(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
            "Provide the actual built simulator .app bundle.");
        device.Validate();
        AppiumFixture.RequireFreePort(AppiumFixture.IosPort);
        var options = Options("iOS", "XCUITest");
        options.App = Artifact;
        options.DeviceName = device.Name;
        options.PlatformVersion = device.Version;
        options.AddAdditionalAppiumOption("bundleId", ApplicationId);
        options.AddAdditionalAppiumOption("enforceFreshSimulatorCreation", true);
        options.AddAdditionalAppiumOption("shutdownOtherSimulators", false);
        options.AddAdditionalAppiumOption("fullReset", true);
        options.AddAdditionalAppiumOption("useNewWDA", true);
        options.AddAdditionalAppiumOption("wdaLocalPort", AppiumFixture.IosPort);
        options.AddAdditionalAppiumOption("wdaRemotePort", AppiumFixture.IosPort);
        options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(Workspace.Root, "wda-ios"));
        options.AddAdditionalAppiumOption("showXcodeLog", true);
        // Animated GPU pages never become continuously idle; retain bounded XCTest idling checks.
        options.AddAdditionalAppiumOption("waitForIdleTimeout", 1);
        SetDriver(new IOSDriver(address, options, TimeSpan.FromMinutes(6)));
        Wait(() => Driver.FindElement(By.XPath("//XCUIElementTypeApplication"))
            is { Displayed: true } element ? element : null, $"{ApplicationId} foreground UI");
        return Task.CompletedTask;
    }

    protected override Task CapturePlatformDiagnostics() =>
        SaveText("device.log", string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog")));
}
