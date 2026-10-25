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
	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroidNavigatePages()
	{
		await RunAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
			running => Basic(running.Operations, "Maui",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-initial", new("basic-drawing-clear"))
		], NavigateMauiAndroid));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicNativeAndroidNavigatePages()
	{
		await RunAndroidApp("Basic/Android", "SkiaSharpSample/SkiaSharpSample.csproj",
			running => Basic(running.Operations, "Native",
		[
			new("nav_cpu", "skiaView", "basic-cpu"),
			new("nav_gpu_surface", "gpuSurfaceCanvas", "basic-gpu-surface-smoke", new("fpsLabel", HasFps), Animated: true),
			new("nav_gpu_texture", "gpuTextureCanvas", "basic-gpu-texture-smoke", new("fpsLabel", HasFps), Animated: true),
			new("nav_drawing", "skiaView", "basic-drawing-page", new("btnClear"))
		], (operations, id) => operations.Click(id)));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task BasicMauiAndroidDrawing()
	{
		await RunAndroidApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
			running => MauiDrawing(running.Operations, NavigateMauiAndroid));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Android)]
	[Trait("Platform", "Android")]
	public async Task GalleryMauiAndroid()
	{
		await RunAndroidApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj",
			running => Gallery(running.Operations));
	}

	private async Task RunAndroidApp(string folder, string project, Func<AppiumRunningApp, Task> scenario)
	{
		const string framework = "net10.0-android";
		var architecture = HostArchitecture();
		var rid = "android-" + architecture;
		string FindApp(SampleWorkspace workspace) => AndroidRunningApp.FindApp(Path.Combine(
			workspace.Root, "samples", folder, Path.GetDirectoryName(project)!, "bin", "Debug", framework, rid));

		var workspace = await appium.PrepareWorkspace($"android/{architecture}", folder, project, async prepared =>
		{
			await BuildSample(prepared, folder, project, new DotNetOptions
			{
				Configuration = "Debug",
				Framework = framework,
				Properties = new Dictionary<string, string>
				{
					["RuntimeIdentifier"] = rid,
					["AndroidPackageFormats"] = "apk",
					["EmbedAssembliesIntoApk"] = "true"
				}
			});
			FindApp(prepared);
		});
		var artifact = FindApp(workspace);
		if (appium.Mode == AppiumMode.Build)
			return;
		await appium.Run(new AndroidRunningApp(workspace, artifact,
			Environment.GetEnvironmentVariable("TEST_ANDROID_UDID")), scenario);
	}

	private static void NavigateMauiAndroid(AppiumOperations app, string title)
	{
		app.Click(MobileBy.AccessibilityId("Open navigation drawer"));
		app.Click(By.XPath($"//android.widget.TextView[@text='{title}']"));
	}
}
