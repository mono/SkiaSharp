using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.FailureHandling;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp : AppiumRunningApp
{
	private readonly string version =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_VERSION") ?? "26.2";

	private readonly string model =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_MODEL") ?? "iPhone 16 Pro";

	private readonly string? simulator =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_UDID");

	internal IosRunningApp(SampleWorkspace workspace, string artifact)
		: base(workspace, artifact, new IosAppiumOperations())
	{
	}

	internal override string DeviceProfile => ReferenceProfile(model, version);

	protected override SKRectI ScreenshotInsets(SKSizeI size) =>
		ScreenshotInsets(model, version, size);

	internal static SKRectI ScreenshotInsets(string model, string version, SKSizeI size) =>
		model == "iPhone 16 Pro" &&
		version is "18.6" or "26.2" or "26.3.1" &&
		size == new SKSizeI(1206, 2622)
			? new SKRectI(0, 0, 0, 39)
			: SKRectI.Empty;

	internal override async Task<string> ReadApplicationId()
	{
		var plist = Path.Combine(Artifact, "Info.plist");
		var id = (await Command("plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-", plist])).Trim();

		if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsWhiteSpace))
			throw new InvalidOperationException($"Built app bundle has an invalid CFBundleIdentifier: {plist}");

		return id;
	}

	internal static string ReferenceProfile(string model, string version) =>
		$"{model.Replace(' ', '-')}/{version}";

	protected override Task Start(Uri address)
	{
		Assert.True(OperatingSystem.IsMacOS(),
			"iOS tests require macOS, Xcode and the declared preinstalled simulator runtime.");
		Assert.True(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
			"Provide the actual built simulator .app bundle.");

		var options = new AppiumOptions
		{
			PlatformName = "iOS",
			AutomationName = "XCUITest",
			App = Artifact,
			DeviceName = model
		};
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);

		options.PlatformVersion = version;

		if (!string.IsNullOrWhiteSpace(simulator))
			options.AddAdditionalAppiumOption("udid", simulator);

		options.AddAdditionalAppiumOption("bundleId", ApplicationId);

		// Reset/reinstall the app, not Cake's simulator; direct runs leave selection to Appium.
		options.AddAdditionalAppiumOption("fullReset", false);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("resetOnSessionStartOnly", true);
		options.AddAdditionalAppiumOption("shutdownOtherSimulators", false);

		var wdaLocalPort = AppiumFixture.GetFreePort();
		var wda = Path.Combine(
			Repo.RootDir, "output", "appium", "wda", "WebDriverAgentRunner-Runner.app");
		Assert.True(Directory.Exists(wda), $"Install the prebuilt simulator WDA app before running iOS samples: {wda}");
		options.AddAdditionalAppiumOption("usePreinstalledWDA", true);
		options.AddAdditionalAppiumOption("prebuiltWDAPath", wda);
		options.AddAdditionalAppiumOption("wdaLocalPort", wdaLocalPort);
		options.AddAdditionalAppiumOption("showXcodeLog", true);
		options.AddAdditionalAppiumOption("waitForIdleTimeout", 1);

		SetDriver(new IOSDriver(address, options, TimeSpan.FromMinutes(6)));

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"iOS runtime: {Driver.Capabilities.GetCapability("platformVersion")}; golden profile: {DeviceProfile}.");

		// Simulator defaults can inherit accessibility and appearance settings from
		// the host. Normalize both before capturing system chrome with the canvas.
		Driver.ExecuteScript("mobile: setAppearance",
			new Dictionary<string, object> { ["style"] = "light" });
		Driver.ExecuteScript("mobile: setIncreaseContrast",
			new Dictionary<string, object> { ["increaseContrast"] = "disabled" });

		// Normalize only the session's simulator status bar. Full-screen iOS comparisons
		// separately exclude the system-owned home-indicator band.
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

		Operations.Wait(() => Convert.ToInt32(Driver.ExecuteScript("mobile: queryAppState",
			new Dictionary<string, object> { ["bundleId"] = ApplicationId })) == 4
			? ApplicationId : null, $"{ApplicationId} running in the foreground");
		return Task.CompletedTask;
	}

	protected override async Task CapturePlatformDiagnostics()
	{
		List<Exception> failures = [];
		if (HasDriver)
		{
			await Attempt(failures, async () => await CaptureCommand(Apple.ListSimulators()));
			await Attempt(failures, () => SaveText("device.log",
				string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog"))));
		}

		ThrowFailures(failures);
	}
}
