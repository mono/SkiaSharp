using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(AppiumCollection.Name)]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AndroidSampleTests(AppiumFixture appium) : AppiumSampleTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroid()
	{
		var app = await BuildAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
			"com.companyname.SkiaSharpSample");

		await appium.Run(app, running =>
		{
			Basic(running, "Maui", "Android", app.DeviceProfile,
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu", "basic-gpu-fps",
				() => NavigateMauiAndroid(running, "Drawing"),
				() => NavigateMauiAndroid(running, "GPU"),
				() => NavigateMauiAndroid(running, "CPU"));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicNativeAndroid()
	{
		var app = await BuildAndroidApp("Basic/Android", "SkiaSharpSample/SkiaSharpSample.csproj",
			"com.companyname.skiasharpsample");

		await appium.Run(app, running =>
		{
			Basic(running, "Native", "Android", app.DeviceProfile,
				"skiaView", "skiaView", "btnClear", "skiaView", "fpsLabel",
				() => running.Click("nav_drawing"),
				() => running.Click("nav_gpu_surface"),
				() => running.Click("nav_cpu"));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task GalleryMauiAndroid()
	{
		var app = await BuildAndroidApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj",
			"com.skiasharp.gallery.maui");

		await appium.Run(app, running => Gallery(running, "Android", app.DeviceProfile));
	}

	private async Task<AndroidRunningApp> BuildAndroidApp(
		string folder, string project, string applicationId)
	{
		const string framework = "net10.0-android";
		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = new Dictionary<string, string>
			{
				["AndroidPackageFormats"] = "apk",
				// Appium installs the APK without dotnet's separate fast-deployment step.
				["EmbedAssembliesIntoApk"] = "true"
			}
		});

		var artifact = Assert.Single(Directory.EnumerateFiles(
			Output(workspace, folder, project, framework), "*-Signed.apk", SearchOption.AllDirectories));

		return new AndroidRunningApp(workspace, artifact, applicationId);
	}

	private static void NavigateMauiAndroid(AppiumRunningApp app, string title)
	{
		app.Click(MobileBy.AccessibilityId("Open navigation drawer"));
		app.Click(MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{title}\")"));
	}
}
