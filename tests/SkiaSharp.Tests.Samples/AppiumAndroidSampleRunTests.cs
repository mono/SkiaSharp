using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(AppiumCollection))]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AppiumAndroidSampleRunTests(AppiumFixture appium) : AppiumSampleRunTestBase
{
	private static bool CaptureReview =>
		bool.Parse(Environment.GetEnvironmentVariable("TEST_ANDROID_CAPTURE_REVIEW") ?? "false");

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroidNavigatePages()
	{
		var app = await BuildAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");
		await appium.Run(app, running => Basic(running.Operations, "Maui",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMauiAndroid, captureReview: CaptureReview));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicNativeAndroidNavigatePages()
	{
		var app = await BuildAndroidApp("Basic/Android", "SkiaSharpSample/SkiaSharpSample.csproj");
		await appium.Run(app, running => Basic(running.Operations, "Native",
		[
			new("nav_cpu", "skiaView", "basic-cpu"),
			new("nav_gpu_surface", "gpuSurfaceCanvas", "basic-gpu-surface-smoke", new("fpsLabel", HasFps), Animated: true),
			new("nav_gpu_texture", "gpuTextureCanvas", "basic-gpu-texture-smoke", new("fpsLabel", HasFps), Animated: true),
			new("nav_drawing", "skiaView", "basic-drawing-page", new("btnClear"))
		], (operations, id) => operations.Click(id), captureReview: CaptureReview));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroidDrawing()
	{
		var app = await BuildAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");
		await appium.Run(app, running => MauiDrawing(running.Operations, NavigateMauiAndroid, captureReview: CaptureReview));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task GalleryMauiAndroid()
	{
		var app = await BuildAndroidApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");
		await appium.Run(app, running => Gallery(running.Operations, captureReview: CaptureReview));
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
				["EmbedAssembliesIntoApk"] = "true"
			}
		});
		var artifact = Assert.Single(Directory.EnumerateFiles(
			Output(workspace, folder, project, framework), "*-Signed.apk"));
		return new AndroidRunningApp(workspace, artifact);
	}

	private static void NavigateMauiAndroid(AppiumOperations app, string title)
	{
		app.Click(MobileBy.AccessibilityId("Open navigation drawer"));
		app.Click(By.XPath($"//android.widget.TextView[@text='{title}']"));
	}
}
