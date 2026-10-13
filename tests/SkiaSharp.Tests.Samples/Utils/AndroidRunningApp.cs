using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp(
	SampleWorkspace workspace,
	string artifact,
	string applicationId,
	string? version = null,
	string? imagePrefix = null)
	: AppiumRunningApp(workspace, artifact, applicationId)
{
	private readonly string model =
		Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_MODEL")
		?? "pixel_9";
	private readonly string version = version
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_VERSION")
		?? "17";
	private readonly string imagePrefix = imagePrefix
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_IMAGE_PREFIX")
		?? "system-images;android-37.1;google_apis_ps16k";

	internal string DeviceProfile => $"{model}/{version}";

	internal override PointerKind PointerKind => PointerKind.Touch;

	internal override By Locator(string id) => By.Id($"{ApplicationId}:id/{id}");

	internal override void SubmitSearch() =>
		((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Enter);

	protected override async Task Start(Uri address)
	{
		Assert.True(Path.GetExtension(Artifact).Equals(".apk", StringComparison.OrdinalIgnoreCase),
			"Provide the actual built APK, not an AAB or synthetic app.");

		var sdk = Environment.GetEnvironmentVariable("ANDROID_HOME")
			?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT")
			?? throw new InvalidOperationException("Set ANDROID_HOME or ANDROID_SDK_ROOT to the preinstalled SDK.");
		var android = new Android(sdk);
		var name = $"SkiaSharpSamples_{version}_{Guid.NewGuid():N}";

		// This lifetime owns only its unique AVD; Appium boots it and selects its serial.
		Own(async () =>
		{
			await CaptureCommand(android.DeleteAvd(name));
		});
		await CaptureCommand(android.CreateAvd(name, imagePrefix, model));

		var options = Options("Android", "UiAutomator2");
		options.App = Artifact;
		options.DeviceName = name;
		options.PlatformVersion = version;
		options.AddAdditionalAppiumOption("avd", name);
		options.AddAdditionalAppiumOption("avdArgs", new[] { "-no-snapshot", "-no-boot-anim" });
		options.AddAdditionalAppiumOption("appPackage", ApplicationId);
		options.AddAdditionalAppiumOption("suppressKillServer", true);
		options.AddAdditionalAppiumOption("fullReset", true);

		SetDriver(new AndroidDriver(address, options, TimeSpan.FromMinutes(3)));
		Wait(() => Driver.PageSource.Contains($"package=\"{ApplicationId}\"", StringComparison.Ordinal)
			? "foreground app" : null, $"{ApplicationId} foreground UI");
	}

	protected override Task CapturePlatformDiagnostics() =>
		SaveText("device.log", string.Join(Environment.NewLine, Driver.Manage().Logs.GetLog("logcat")));
}
