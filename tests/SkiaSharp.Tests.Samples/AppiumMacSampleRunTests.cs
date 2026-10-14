using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(AppiumCollection))]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AppiumMacSampleRunTests(AppiumFixture appium) : AppiumSampleRunTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicMauiMacCatalystNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Maui", "MacCatalyst",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMauiMacCatalyst));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicNativeMacCatalystNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Basic/MacCatalyst", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Native", "MacCatalyst",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("Metal", "basic-canvas-gpu-metal", "basic-gpu-metal-smoke", new("basic-gpu-metal-fps", HasFps), Animated: true),
			new("Draw", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateNativeMac));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicMauiMacCatalystDrawing()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => MauiDrawing(running, "MacCatalyst", NavigateMauiMacCatalyst));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "macOS")]
	public async Task BasicNativeMacOsNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Native AppKit tests require macOS.");

		var app = await BuildMacApp("Basic/macOS", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Native", "macOS",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU (GL)", "basic-canvas-gpu-gl", "basic-gpu-gl-smoke", new("basic-gpu-gl-fps", HasFps), Animated: true),
			new("GPU (Metal)", "basic-canvas-gpu-metal", "basic-gpu-metal-smoke", new("basic-gpu-metal-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateNativeMac));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task GalleryMauiMacCatalyst()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");

		await appium.Run(app, running => Gallery(running, "MacCatalyst"));
	}

	private async Task<MacRunningApp> BuildMacApp(string folder, string project)
	{
		var macOS = folder == "Basic/macOS";
		var framework = macOS ? "net10.0-macos" : "net10.0-maccatalyst";
		var rid = (macOS ? "osx-" : "maccatalyst-") + MacRunningApp.HostArchitecture;

		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = rid }
		});

		var artifact = Assert.Single(Directory.EnumerateDirectories(
			Path.Combine(Output(workspace, folder, project, framework), rid), "*.app"));
		return new MacRunningApp(workspace, artifact);
	}

	private static void NavigateMauiMacCatalyst(AppiumRunningApp app, string title)
	{
		app.Click(By.Name("Menu"));
		app.Click(By.Name(title));
	}

	private static void NavigateNativeMac(AppiumRunningApp app, string title) =>
		app.Click(By.Name(title));
}
