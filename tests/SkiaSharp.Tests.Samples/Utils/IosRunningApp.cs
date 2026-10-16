using System.Text.Json;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
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
	private string? simulator;

	internal override string DeviceProfile => $"{model.Replace(' ', '-')}/{version}";

	internal override Task<string> ReadApplicationId() => ReadAppleApplicationId();

	internal static string RuntimeBuild(string version) => version switch
	{
		"18.6" => "22G86",
		"26.5" => "23F77",
		_ => throw new InvalidOperationException($"No reviewed iOS sample profile exists for {version}.")
	};

	protected override async Task Prepare()
	{
		Assert.True(OperatingSystem.IsMacOS(),
			"iOS tests require macOS, Xcode and the declared preinstalled simulator runtime.");
		Assert.True(Directory.Exists(Artifact) && Artifact.EndsWith(".app", StringComparison.OrdinalIgnoreCase),
			"Provide the actual built simulator .app bundle.");
		Assert.Equal("iPhone 16 Pro", model);
		var build = RuntimeBuild(version);
		using var inventory = JsonDocument.Parse(await Command("xcrun", ["simctl", "list", "--json"]));
		var runtime = SelectRuntime(inventory.RootElement.GetProperty("runtimes"), version, build);
		var deviceType = Assert.Single(inventory.RootElement.GetProperty("devicetypes").EnumerateArray(),
			device => device.GetProperty("name").GetString() == model).GetProperty("identifier").GetString()!;

		// Create and register our own UUID before session construction, which can fail.
		simulator = (await Command("xcrun", ["simctl", "create",
			SampleWorkspace.CreateIdentity("skiasharp-ios"), deviceType, runtime])).Trim();
		Assert.True(Guid.TryParse(simulator, out _), $"simctl returned an invalid simulator UUID: {simulator}");
		Own(DeleteSimulator);
		await SaveText("simulator.json", JsonSerializer.Serialize(new { simulator, model, version, build }), "application/json");

		await PrepareApplication(simulator, Artifact, ApplicationId, async arguments =>
			await CaptureCommand(ProcessRunner.Run("dotnet", arguments, Repo.RootDir, TimeSpan.FromMinutes(3))));
	}

	internal static async Task PrepareApplication(
		string simulator, string artifact, string applicationId, Func<string[], Task> execute)
	{
		await execute(["apple", "simulator", "boot", simulator, "--wait"]);
		await execute(["apple", "simulator", "app", "install", simulator, artifact]);
		await execute(["apple", "simulator", "app", "launch", simulator, applicationId]);
	}

	internal static AppiumOptions AttachOptions(
		string simulator, string applicationId, string model, string version)
	{
		var options = Options();
		options.DeviceName = model;
		options.PlatformVersion = version;
		options.AddAdditionalAppiumOption("udid", simulator);
		options.AddAdditionalAppiumOption("bundleId", applicationId);
		options.AddAdditionalAppiumOption("noReset", true);
		// Keep the AUT bound to WDA; forceAppLaunch=false preserves the prelaunched process.
		options.AddAdditionalAppiumOption("autoLaunch", true);
		options.AddAdditionalAppiumOption("forceAppLaunch", false);
		options.AddAdditionalAppiumOption("shutdownOtherSimulators", false);
		return options;
	}

	protected override Task Start(Uri address)
	{
		Assert.NotNull(simulator);
		var wdaLocalPort = AppiumFixture.GetFreePort();
		var options = AttachOptions(simulator, ApplicationId, model, version);
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

		Wait(() => Convert.ToInt32(Driver.ExecuteScript("mobile: queryAppState",
			new Dictionary<string, object> { ["bundleId"] = ApplicationId })) == 4
			? ApplicationId : null, $"{ApplicationId} running in the foreground");
		return Task.CompletedTask;
	}

	internal static string SelectRuntime(JsonElement runtimes, string version, string build) =>
		Assert.Single(runtimes.EnumerateArray(), runtime =>
			runtime.GetProperty("name").GetString()!.StartsWith("iOS ", StringComparison.Ordinal) &&
			runtime.GetProperty("version").GetString() == version &&
			runtime.GetProperty("buildversion").GetString() == build &&
			runtime.GetProperty("isAvailable").GetBoolean()).GetProperty("identifier").GetString()!;

	protected override async Task CapturePlatformDiagnostics()
	{
		List<Exception> failures = [];
		if (simulator is not null)
			await Attempt(failures, async () => await Command("xcrun", ["simctl", "list", "devices", "--json"]));
		if (HasDriver)
			await Attempt(failures, () => SaveText("device.log",
				string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("syslog"))));
		ThrowFailures(failures);
	}

	private async Task DeleteSimulator()
	{
		using var inventory = JsonDocument.Parse(await Command("xcrun", ["simctl", "list", "devices", "--json"]));
		var owned = inventory.RootElement.GetProperty("devices").EnumerateObject()
			.SelectMany(runtime => runtime.Value.EnumerateArray())
			.Where(device => device.GetProperty("udid").GetString() == simulator).ToArray();
		List<Exception> failures = [];
		if (owned.Any(device => device.GetProperty("state").GetString() == "Booted"))
			await Attempt(failures, async () => await Command("xcrun", ["simctl", "shutdown", simulator!]));
		if (owned.Length != 0)
			await Attempt(failures, async () => await Command("xcrun", ["simctl", "delete", simulator!]));
		ThrowFailures(failures);
	}
}
