using System.Text.RegularExpressions;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp : AppiumRunningApp
{
	private readonly string? version = Environment.GetEnvironmentVariable("TEST_ANDROID_VERSION");
	private readonly string? image = Environment.GetEnvironmentVariable("TEST_ANDROID_IMAGE");
	private readonly string? model = Environment.GetEnvironmentVariable("TEST_ANDROID_MODEL");
	private readonly string? serial = Environment.GetEnvironmentVariable("TEST_ANDROID_UDID");

	internal AndroidRunningApp(SampleWorkspace workspace, string artifact)
		: base(workspace, artifact, new AndroidAppiumOperations())
	{
	}

	internal override string Platform => "Android";

	internal override string DeviceProfile => ReferenceProfile(model, version, image);

	internal static string ReferenceProfile(string? model, string? version, string? image)
	{
		if (string.IsNullOrWhiteSpace(model) || !Regex.IsMatch(model, @"^[a-zA-Z0-9_-]+$") ||
			string.IsNullOrWhiteSpace(version) || !Regex.IsMatch(version, @"^[0-9]+(?:\.[0-9]+)*$") ||
			string.IsNullOrWhiteSpace(image) || !Regex.IsMatch(image, @"^android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+$"))
			throw new InvalidOperationException("Declare the exact TEST_ANDROID_MODEL, TEST_ANDROID_VERSION and TEST_ANDROID_IMAGE profile.");

		return $"{model}/{image}";
	}

	internal override async Task<string> ReadApplicationId()
	{
		_ = DeviceProfile;
		var manager = Environment.GetEnvironmentVariable("ANDROID_SDK_MANAGER_PATH");
		if (string.IsNullOrWhiteSpace(manager) || !File.Exists(manager))
			throw new FileNotFoundException("The selected ANDROID_SDK_MANAGER_PATH must exist.", manager);

		var analyzer = Path.Combine(Path.GetDirectoryName(manager)!,
			OperatingSystem.IsWindows() ? "apkanalyzer.bat" : "apkanalyzer");
		if (!File.Exists(analyzer))
			throw new FileNotFoundException("The selected SDK must provide apkanalyzer beside sdkmanager.", analyzer);

		return ParseApplicationId(await Command(
			OperatingSystem.IsWindows() ? "cmd.exe" : analyzer,
			OperatingSystem.IsWindows() ? ["/d", "/c", analyzer, "manifest", "application-id", Artifact] :
				["manifest", "application-id", Artifact]));
	}

	internal static string ParseApplicationId(string output)
	{
		var id = output.Trim();
		if (!Regex.IsMatch(id, @"^[a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*)+$"))
			throw new InvalidOperationException("Built APK has an invalid application ID.");
		return id;
	}

	protected override Task Start(Uri address)
	{
		Assert.True(File.Exists(Artifact) && Artifact.EndsWith(".apk", StringComparison.OrdinalIgnoreCase),
			"Provide the actual built signed APK.");
		_ = DeviceProfile;

		var options = new AppiumOptions
		{
			PlatformName = "Android",
			AutomationName = "UiAutomator2",
			PlatformVersion = version,
			DeviceName = model,
			App = Artifact
		};
		options.AddAdditionalAppiumOption("appPackage", ApplicationId);
		if (!string.IsNullOrWhiteSpace(serial))
			options.AddAdditionalAppiumOption("udid", serial);
		options.AddAdditionalAppiumOption("fullReset", true);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("suppressKillServer", true);
		options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);
		SetDriver(new AndroidDriver(address, options));

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"Android runtime: {Driver.Capabilities.GetCapability("platformVersion")}; golden profile: {DeviceProfile}.");
		return Task.CompletedTask;
	}

	protected override async Task CapturePlatformDiagnostics()
	{
		// Cake supplies its owned serial; a direct run can use only its established session's serial.
		var target = serial ?? (HasDriver ? Driver.Capabilities.GetCapability("udid")?.ToString() : null);
		if (!string.IsNullOrWhiteSpace(target))
		{
			var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME");
			if (string.IsNullOrWhiteSpace(sdk))
				throw new InvalidOperationException("ANDROID_HOME must select the SDK for exact-device diagnostics.");
			var adb = Path.Combine(sdk, "platform-tools", OperatingSystem.IsWindows() ? "adb.exe" : "adb");
			List<Exception> failures = [];
			foreach (var (name, arguments) in new (string, string[])[]
			{
				("device.log", ["logcat", "-b", "all", "-d", "-v", "threadtime"]),
				("last-anr.log", ["shell", "dumpsys", "activity", "lastanr"])
			})
				await FailureHandling.Attempt(failures, async () =>
					await SaveText(name, await Command(adb, ["-s", target, .. arguments])));
			FailureHandling.ThrowFailures(failures);
		}
	}
}
