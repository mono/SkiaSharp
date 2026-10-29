using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(AppiumCollection))]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AppiumIosSampleRunTests(AppiumFixture appium) : AppiumSampleRunTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task BasicMauiIosNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var app = await BuildIosApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running.Operations, "Maui",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMauiIos));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task BasicNativeIosNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var app = await BuildIosApp("Basic/iOS", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running.Operations, "Native",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GL", "basic-canvas-gpu-gl", "basic-gpu-gl-smoke", new("basic-gpu-gl-fps", HasFps), Animated: true),
			new("Metal", "basic-canvas-gpu-metal", "basic-gpu-metal-smoke", new("basic-gpu-metal-fps", HasFps), Animated: true),
			new("Draw", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateNativeIos));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task BasicMauiIosDrawing()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var app = await BuildIosApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => MauiDrawing(running.Operations, NavigateMauiIos));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task GalleryMauiIos()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var app = await BuildIosApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");

		await appium.Run(app, running => Gallery(running.Operations));
	}

	private Task<AppiumPreparedApp> BuildIosApp(string folder, string project)
	{
		const string framework = "net10.0-ios";
		var rid = "iossimulator-" + HostArchitecture();

		return appium.Prepare(new("ios", HostArchitecture(), folder, project, framework, rid), async workspace =>
		{
			await BuildSample(workspace, folder, project, new DotNetOptions
			{
				Configuration = "Debug",
				Framework = framework,
				Properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = rid }
			});
		}, static (workspace, artifact) => new IosRunningApp(workspace, artifact));
	}

	private static void NavigateMauiIos(AppiumOperations app, string title)
	{
		app.Click(By.XPath("//XCUIElementTypeNavigationBar//XCUIElementTypeButton[@label='Menu']"));
		app.Click(MobileBy.IosNSPredicate($"type == 'XCUIElementTypeButton' AND label == '{title}'"));
	}

	private static void NavigateNativeIos(AppiumOperations app, string title) =>
		app.Click(MobileBy.IosNSPredicate($"type == 'XCUIElementTypeButton' AND label == '{title}'"));
}
