using System.Xml.Linq;
using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(AppiumCollection))]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AppiumAndroidSampleRunTests(AppiumFixture appium) : AppiumSampleRunTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroidNavigatePages()
	{
		var app = await BuildAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Maui", "Android", app.DeviceProfile,
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMauiAndroid));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicNativeAndroidNavigatePages()
	{
		var app = await BuildAndroidApp("Basic/Android", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Native", "Android", app.DeviceProfile,
		[
			new("nav_cpu", "skiaView", "basic-cpu"),
			new("nav_gpu_surface", "skiaView", "basic-gpu-surface-smoke", new("fpsLabel", HasFps), Animated: true),
			new("nav_gpu_texture", "skiaView", "basic-gpu-texture-smoke", new("fpsLabel", HasFps), Animated: true),
			new("nav_drawing", "skiaView", "basic-drawing-page", new("btnClear"))
		], NavigateNativeAndroid));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroidDrawing()
	{
		var app = await BuildAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => MauiDrawing(running, "Android", app.DeviceProfile, NavigateMauiAndroid));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task GalleryMauiAndroid()
	{
		var app = await BuildAndroidApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");

		await appium.Run(app, running => Gallery(running, "Android", app.DeviceProfile));
	}

	private async Task<AndroidRunningApp> BuildAndroidApp(string folder, string project)
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
		var selectedProject = Path.Combine(workspace.Root, "samples", folder, project);
		var manifest = Path.Combine(Path.GetDirectoryName(selectedProject)!, "Properties", "AndroidManifest.xml");
		var applicationId = ProjectApplicationId(selectedProject)
			?? (string?)XDocument.Load(manifest).Root?.Attribute("package")
			?? throw new InvalidOperationException($"Source application ID is missing: {selectedProject}");

		return new AndroidRunningApp(workspace, artifact, applicationId);
	}

	private static void NavigateMauiAndroid(AppiumRunningApp app, string title)
	{
		app.Click(MobileBy.AccessibilityId("Open navigation drawer"));
		app.Click(MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{title}\")"));
	}

	private static void NavigateNativeAndroid(AppiumRunningApp app, string id) =>
		app.Click(id);
}
