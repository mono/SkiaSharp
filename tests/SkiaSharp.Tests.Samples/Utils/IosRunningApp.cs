using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.FailureHandling;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class IosRunningApp : AppiumRunningApp
{
	private readonly string? version =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_VERSION");

	private readonly string? referenceVersion =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_REFERENCE_VERSION");

	private readonly string model =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_MODEL") ?? "iPhone 16 Pro";

	private readonly string? simulator =
		Environment.GetEnvironmentVariable("TEST_IOS_SIMULATOR_UDID");

	internal IosRunningApp(SampleWorkspace workspace, string artifact)
		: base(workspace, artifact)
	{
		Operations = new IosAppiumOperations(this);
	}

	internal override string DeviceProfile => ReferenceProfile(model, version, referenceVersion);

	internal override IosAppiumOperations Operations { get; }

	internal override async Task<string> ReadApplicationId()
	{
		var plist = Path.Combine(Artifact, "Info.plist");
		var id = (await Command("plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-", plist])).Trim();

		if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsWhiteSpace))
			throw new InvalidOperationException($"Built app bundle has an invalid CFBundleIdentifier: {plist}");

		return id;
	}

	internal static string ReferenceProfile(string model, string? version, string? referenceVersion)
	{
		var profileVersion = string.IsNullOrEmpty(referenceVersion) ? version : referenceVersion;
		if (string.IsNullOrEmpty(profileVersion) || profileVersion.Equals("latest", StringComparison.OrdinalIgnoreCase))
			profileVersion = "latest";

		return $"{model.Replace(' ', '-')}/{profileVersion}";
	}

	internal static AppiumOptions SessionOptions(
		string artifact, string? simulator, string applicationId, string model, string? version)
	{
		var options = new AppiumOptions
		{
			PlatformName = "iOS",
			AutomationName = "XCUITest",
			App = artifact,
			DeviceName = model
		};
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);

		// A direct latest run lets Appium select the runtime; Cake passes the resolved version.
		if (!string.IsNullOrEmpty(version) && !version.Equals("latest", StringComparison.OrdinalIgnoreCase))
			options.PlatformVersion = version;

		if (!string.IsNullOrWhiteSpace(simulator))
			options.AddAdditionalAppiumOption("udid", simulator);

		options.AddAdditionalAppiumOption("bundleId", applicationId);

		// Reset/reinstall the app, not Cake's simulator; direct runs leave selection to Appium.
		options.AddAdditionalAppiumOption("fullReset", false);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("resetOnSessionStartOnly", true);
		options.AddAdditionalAppiumOption("shutdownOtherSimulators", false);

		return options;
	}

	protected override Task Start(Uri address)
	{
		Assert.True(OperatingSystem.IsMacOS(),
			"iOS tests require macOS, Xcode and the declared preinstalled simulator runtime.");
		Assert.True(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
			"Provide the actual built simulator .app bundle.");

		var wdaLocalPort = AppiumFixture.GetFreePort();
		var options = SessionOptions(Artifact, simulator, ApplicationId, model, version);
		options.AddAdditionalAppiumOption("useNewWDA", true);
		options.AddAdditionalAppiumOption("wdaLocalPort", wdaLocalPort);
		options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(Workspace.Root, "wda-ios"));
		options.AddAdditionalAppiumOption("showXcodeLog", true);
		options.AddAdditionalAppiumOption("waitForIdleTimeout", 1);

		SetDriver(new IOSDriver(address, options, TimeSpan.FromMinutes(6)));

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"iOS runtime: {Driver.Capabilities.GetCapability("platformVersion")}; golden profile: {DeviceProfile}.");

		// Normalize only the session's simulator chrome; retain the full canvas capture.
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
			await Attempt(failures, async () => await Command("xcrun", ["simctl", "list", "devices", "--json"]));
			await Attempt(failures, () => SaveText("device.log",
				string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog"))));
		}

		ThrowFailures(failures);
	}
}
