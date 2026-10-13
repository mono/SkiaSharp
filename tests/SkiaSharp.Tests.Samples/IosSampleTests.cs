using OpenQA.Selenium.Appium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(AppiumCollection.Name)]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class IosSampleTests(AppiumFixture appium) : AppiumSampleTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task BasicMauiIos()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var device = IosDevice.Selected;
		var app = await BuildIosApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
			"com.companyname.SkiaSharpSample", device);

		await appium.Run(app, running =>
		{
			Basic(running, "Maui", "iOS", $"{device.Name.Replace(' ', '-')}/{device.Version}",
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu", "basic-gpu-fps",
				() => NavigateMauiIos(running, "Drawing"),
				() => NavigateMauiIos(running, "GPU"),
				() => NavigateMauiIos(running, "CPU"));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task BasicNativeIos()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var device = IosDevice.Selected;
		var app = await BuildIosApp("Basic/iOS", "SkiaSharpSample/SkiaSharpSample.csproj",
			"com.companyname.skiasharpsample", device);

		await appium.Run(app, running =>
		{
			Basic(running, "Native", "iOS", $"{device.Name.Replace(' ', '-')}/{device.Version}",
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu-gl", "basic-gpu-gl-fps",
				() => running.Click(MobileBy.IosNSPredicate(
					"type == 'XCUIElementTypeButton' AND label == 'Drawing'")),
				() => running.Click(MobileBy.IosNSPredicate(
					"type == 'XCUIElementTypeButton' AND label == 'GL'")),
				() => running.Click(MobileBy.IosNSPredicate(
					"type == 'XCUIElementTypeButton' AND label == 'CPU'")));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Ios)]
	[Trait("Platform", "iOS")]
	public async Task GalleryMauiIos()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");

		var device = IosDevice.Selected;
		var app = await BuildIosApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj",
			"com.skiasharp.gallery.maui", device);

		await appium.Run(app, running =>
			Gallery(running, "iOS", $"{device.Name.Replace(' ', '-')}/{device.Version}"));
	}

	private async Task<IosRunningApp> BuildIosApp(
		string folder, string project, string applicationId, IosDevice device)
	{
		const string framework = "net10.0-ios";
		var rid = "iossimulator-" + HostArchitecture();

		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = rid }
		});

		var artifact = Assert.Single(Directory.EnumerateDirectories(
			Path.Combine(Output(workspace, folder, project, framework), rid), "*.app"));

		return new IosRunningApp(workspace, artifact, applicationId, device);
	}

	private static void NavigateMauiIos(AppiumRunningApp app, string title)
	{
		app.Click(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND label == 'Menu'"));
		app.Click(MobileBy.IosNSPredicate($"type == 'XCUIElementTypeButton' AND label == '{title}'"));
	}
}
