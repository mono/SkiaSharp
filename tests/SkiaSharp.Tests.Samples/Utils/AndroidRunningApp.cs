using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp : AppiumRunningApp
{
	private readonly string? image = Environment.GetEnvironmentVariable("TEST_ANDROID_IMAGE");
	private readonly string? model = Environment.GetEnvironmentVariable("TEST_ANDROID_MODEL");
	private readonly string? serial = Environment.GetEnvironmentVariable("TEST_ANDROID_UDID");

	internal AndroidRunningApp(SampleWorkspace workspace, string artifact)
		: base(workspace, artifact, new AndroidAppiumOperations())
	{
	}

	internal override string Platform => "Android";

	internal override string DeviceProfile => ReferenceProfile(model, image);

	internal static string ReferenceProfile(string? model, string? image)
	{
		if (string.IsNullOrWhiteSpace(model) || !Regex.IsMatch(model, @"^[a-zA-Z0-9_-]+$") ||
			string.IsNullOrWhiteSpace(image) || !Regex.IsMatch(image, @"^android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+$"))
			throw new InvalidOperationException("Declare the exact TEST_ANDROID_MODEL and TEST_ANDROID_IMAGE profile.");

		return $"{model}/{image[..image.IndexOf(';')]}";
	}

	internal override async Task<string> ReadApplicationId()
	{
		_ = DeviceProfile;
		return ParseApplicationId(await CaptureCommand(Android.ReadManifest(Artifact)));
	}

	internal static string ParseApplicationId(string output)
	{
		var manifest = XDocument.Parse(output);
		var id = manifest.Root?.Name.LocalName == "manifest"
			? manifest.Root.Attribute("package")?.Value : null;
		if (id is null || !Regex.IsMatch(id, @"^[a-zA-Z_][a-zA-Z0-9_]*(?:\.[a-zA-Z_][a-zA-Z0-9_]*)+$"))
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
			List<Exception> failures = [];
			await FailureHandling.Attempt(failures, async () =>
				await SaveText("device.log", await CaptureCommand(Android.Logcat(target))));
			await FailureHandling.Attempt(failures, async () =>
				await SaveText("last-anr.log", await CaptureCommand(Android.LastAnr(target))));
			FailureHandling.ThrowFailures(failures);
		}
	}
}
