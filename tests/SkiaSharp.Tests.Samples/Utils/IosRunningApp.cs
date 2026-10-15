using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp(SampleWorkspace workspace, string artifact, string? version = null, string? model = null)
	: AppiumRunningApp(workspace, artifact)
{
	private readonly string version = version
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_VERSION")
		?? "26.5";
	private readonly string model = model
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_IOS_MODEL")
		?? "iPhone 16 Pro";

	internal override string DeviceProfile => $"{model.Replace(' ', '-')}/{version}";

	internal override PointerKind PointerKind => PointerKind.Touch;

	internal override Task<string> ReadApplicationId() => ReadAppleApplicationId();

	internal override void SubmitSearch() =>
		Find(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND name == 'Return'")).Click();

	protected override Task Start(Uri address)
	{
		Assert.True(OperatingSystem.IsMacOS(),
			"iOS tests require macOS, Xcode and the declared preinstalled simulator runtime.");
		Assert.True(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
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

		// Normalize only this fresh simulator's system chrome; retain the full canvas capture.
		var status = Assert.IsAssignableFrom<IDictionary<string, object>>(
			Driver.ExecuteScript("mobile: simctl", new Dictionary<string, object>
			{
				["command"] = "status_bar",
				["args"] = new[]
				{
					"override", "--time", "10:10",
					"--dataNetwork", "wifi", "--wifiMode", "active", "--wifiBars", "3",
					"--cellularMode", "active", "--cellularBars", "4",
					"--batteryState", "charged", "--batteryLevel", "100"
				}
			}));
		Assert.NotNull(status["code"]);
		Assert.Equal(0, Convert.ToInt32(status["code"]));

		Wait(() => Driver.FindElement(By.XPath("//XCUIElementTypeApplication"))
			is { Displayed: true } element ? element : null, $"{ApplicationId} foreground UI");
		return Task.CompletedTask;
	}

	protected override Task CapturePlatformDiagnostics() =>
		HasDriver
			? SaveText("device.log", string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog")))
			: Task.CompletedTask;
}
