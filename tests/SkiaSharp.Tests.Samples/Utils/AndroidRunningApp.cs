using OpenQA.Selenium;
using System.Xml.Linq;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp(SampleWorkspace workspace, string artifact, string? version = null, string? image = null)
	: AppiumRunningApp(workspace, artifact)
{
	private (Android Sdk, string Name)? ownedAvd;
	private readonly string model =
		Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_MODEL")
		?? "pixel_9";
	private readonly string version = version
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_VERSION")
		?? "17";
	private readonly string image = image
		?? Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_IMAGE")
		?? "android-37.1;google_apis_ps16k";

	internal override string DeviceProfile => $"{model}/{version}";

	internal override PointerKind PointerKind => PointerKind.Touch;

	internal override async Task<string> ReadApplicationId()
	{
		var manifest = await CaptureCommand(DotNet.ExecuteTool(
			"android", ["apk", "manifest", "info", "--pkg", Artifact, "--format", "xml"]));
		return PackageId(manifest);
	}

	internal static string PackageId(string manifest)
	{
		var package = XDocument.Parse(manifest).Root?.Attribute("package")?.Value.Trim();
		if (string.IsNullOrWhiteSpace(package) || package.Any(char.IsWhiteSpace))
			throw new InvalidOperationException("Built APK manifest has no valid package ID.");
		return package;
	}

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
		var name = SampleWorkspace.CreateIdentity($"SkiaSharpSamples-{version}");
		ownedAvd = (android, name);

		// This lifetime owns only its unique AVD; Appium boots it and selects its serial.
		Own(async () =>
		{
			await CaptureCommand(android.DeleteAvd(name));
		});
		await CaptureCommand(android.CreateAvd(name, image, model));

		var options = Options("Android", "UiAutomator2");
		options.App = Artifact;
		options.DeviceName = name;
		options.PlatformVersion = version;
		options.AddAdditionalAppiumOption("avd", name);
		options.AddAdditionalAppiumOption("isHeadless", OperatingSystem.IsLinux());
		options.AddAdditionalAppiumOption("avdArgs", new[] { "-no-snapshot", "-no-boot-anim" });
		options.AddAdditionalAppiumOption("appPackage", ApplicationId);
		options.AddAdditionalAppiumOption("suppressKillServer", true);
		options.AddAdditionalAppiumOption("fullReset", true);

		SetDriver(new AndroidDriver(address, options, TimeSpan.FromMinutes(3)));
		Wait(() => Driver.PageSource.Contains($"package=\"{ApplicationId}\"", StringComparison.Ordinal)
			? "foreground app" : null, $"{ApplicationId} foreground UI");
	}

	protected override Task CapturePlatformDiagnostics() =>
		ownedAvd is { } avd
			? avd.Sdk.CaptureAvdDiagnostics(avd.Name, (file, text) => SaveText(file, text))
			: SaveText("android-avd.log", "Startup did not select an owned AVD. No device diagnostics were attempted.");
}
