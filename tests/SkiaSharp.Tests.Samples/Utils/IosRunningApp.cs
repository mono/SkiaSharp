using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.FailureHandling;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp : AppiumRunningApp
{
	private readonly string version =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_VERSION") ?? "26.3.1";

	private readonly string model =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_MODEL") ?? "iPhone 16 Pro";

	private readonly string? simulator =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_UDID");

	internal IosRunningApp(SampleWorkspace workspace, string artifact)
		: base(workspace, artifact, new IosAppiumOperations())
	{
	}

	internal override string DeviceProfile => ReferenceProfile(model, version);

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
		options.AddAdditionalAppiumOption("useNewWDA", true);
		options.AddAdditionalAppiumOption("wdaLocalPort", wdaLocalPort);
		options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(Workspace.Root, "wda-ios"));
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

		// Normalize only the session's simulator status bar. Native comparisons separately
		// exclude the system-owned home-indicator band from otherwise full canvas captures.
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
			// Resolve the pinned local Apple tool from the repository manifest.
			await Attempt(failures, async () => await CaptureCommand(ProcessRunner.Run(
				"dotnet", ["apple", "simulator", "list", "--format", "json"],
				Repo.RootDir, TimeSpan.FromSeconds(30))));
			await Attempt(failures, () => SaveText("device.log",
				string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog"))));
		}

		ThrowFailures(failures);
	}
}
