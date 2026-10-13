using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp(
	SampleWorkspace workspace,
	string artifact,
	string applicationId,
	string? version = null,
	string? model = null)
	: AppiumRunningApp(workspace, artifact, applicationId)
{
	private readonly string version = version
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_VERSION") ?? "26.2";
	private readonly string model = model
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_MODEL") ?? "iPhone 16 Pro";

	internal string DeviceProfile => $"{model.Replace(' ', '-')}/{version}";

	internal override PointerKind PointerKind => PointerKind.Touch;

	internal override void SubmitSearch() =>
		Find(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND name == 'Return'")).Click();

	protected override Task Start(Uri address)
	{
		Require(OperatingSystem.IsMacOS(),
			"iOS tests require macOS, Xcode and the declared preinstalled simulator runtime.");
		Require(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
			"Provide the actual built simulator .app bundle.");

		var wdaLocalPort = AppiumFixture.GetFreePort();
		var options = Options("iOS", "XCUITest");
		options.App = Artifact;
		options.DeviceName = model;
		options.PlatformVersion = version;
		options.AddAdditionalAppiumOption("bundleId", ApplicationId);
		options.AddAdditionalAppiumOption("enforceFreshSimulatorCreation", true);
		options.AddAdditionalAppiumOption("shutdownOtherSimulators", false);
		options.AddAdditionalAppiumOption("fullReset", true);
		options.AddAdditionalAppiumOption("useNewWDA", true);
		options.AddAdditionalAppiumOption("wdaLocalPort", wdaLocalPort);
		options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(Workspace.Root, "wda-ios"));
		options.AddAdditionalAppiumOption("showXcodeLog", true);
		options.AddAdditionalAppiumOption("waitForIdleTimeout", 1);

		SetDriver(new IOSDriver(address, options, TimeSpan.FromMinutes(6)));
		Wait(() => Driver.FindElement(By.XPath("//XCUIElementTypeApplication"))
			is { Displayed: true } element ? element : null, $"{ApplicationId} foreground UI");
		return Task.CompletedTask;
	}

	protected override Task CapturePlatformDiagnostics() =>
		SaveText("device.log", string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog")));
}
