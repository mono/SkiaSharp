using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class AndroidRunningApp : AppiumRunningApp
{
	private readonly string? image = Environment.GetEnvironmentVariable("TEST_ANDROID_IMAGE");
	private readonly string? model = Environment.GetEnvironmentVariable("TEST_ANDROID_MODEL");
	private readonly string serial;
	private string? actualArchitecture;

	internal AndroidRunningApp(SampleWorkspace workspace, string artifact, string? serial)
		: base(workspace, artifact, new AndroidAppiumOperations())
	{
		if (string.IsNullOrWhiteSpace(serial))
			throw new InvalidOperationException("Declare the exact TEST_ANDROID_UDID; Android tests must not select the first available device.");
		this.serial = serial;
	}

	internal override string Platform => "Android";

	internal override string DeviceProfile => ReferenceProfile(model, image, RuntimeInformation.OSArchitecture);

	internal override GoldenRequest Reference(string caseName)
	{
		_ = DeviceProfile;
		return new(DeviceProfile, image![8..image.IndexOf(';')], caseName,
			actualArchitecture ?? RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant());
	}

	internal static string ReferenceProfile(string? model, string? image, Architecture architecture)
	{
		if (string.IsNullOrWhiteSpace(model) || !Regex.IsMatch(model, @"^[a-zA-Z0-9_-]+$") ||
			string.IsNullOrWhiteSpace(image) || !Regex.IsMatch(image, @"^android-[0-9]+(?:\.[0-9]+)?;[a-zA-Z0-9_]+$"))
			throw new InvalidOperationException("Declare the exact TEST_ANDROID_MODEL and TEST_ANDROID_IMAGE profile.");

		_ = architecture switch
		{
			Architecture.X64 => "x86_64",
			Architecture.Arm64 => "arm64-v8a",
			_ => throw new InvalidOperationException("Android emulators require an arm64 or x64 host.")
		};
		if (model != "pixel_9" || image[(image.IndexOf(';') + 1)..] != "google_apis")
			throw new InvalidOperationException("Only the declared Pixel 9 google_apis capture profile is registered; other image flavors must not reuse it.");
		return "pixel-9";
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

	protected override async Task Start(Uri address)
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
		options.AddAdditionalAppiumOption("udid", serial);
		options.AddAdditionalAppiumOption("fullReset", true);
		options.AddAdditionalAppiumOption("noReset", false);
		options.AddAdditionalAppiumOption("suppressKillServer", true);
		options.AddAdditionalAppiumOption("systemPort", AppiumFixture.GetFreePort());
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);
		SetDriver(new AndroidDriver(address, options));

		var api = (await CaptureCommand(Android.DeviceProperty(serial, "ro.build.version.sdk"))).Trim();
		Assert.Equal(image![8..image.IndexOf(';')].Split('.')[0], api);
		var abi = (await CaptureCommand(Android.DeviceProperty(serial, "ro.product.cpu.abi"))).Trim();
		actualArchitecture = abi switch
		{
			"arm64-v8a" => "arm64",
			"x86_64" => "x64",
			_ => throw new InvalidOperationException($"Unsupported actual emulator ABI: {abi}")
		};

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"Android runtime: {Driver.Capabilities.GetCapability("platformVersion")}; golden profile: {DeviceProfile}.");
	}

	protected override async Task CapturePlatformDiagnostics()
	{
		List<Exception> failures = [];
		await FailureHandling.Attempt(failures, async () =>
			await SaveText("device.log", await CaptureCommand(Android.Logcat(serial))));
		await FailureHandling.Attempt(failures, async () =>
			await SaveText("last-anr.log", await CaptureCommand(Android.LastAnr(serial))));
		FailureHandling.ThrowFailures(failures);
	}
}
