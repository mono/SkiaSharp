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
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Mac Catalyst tests require macOS.");
		var app = await BuildMacApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj", "maccatalyst");
		await appium.Run(app, running => Basic(running.Operations, "Maui",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMaui, collectReferenceFailures: true));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicNativeMacCatalystNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Mac Catalyst tests require macOS.");
		var app = await BuildMacApp("Basic/MacCatalyst", "SkiaSharpSample/SkiaSharpSample.csproj", "maccatalyst");
		await appium.Run(app, running => Basic(running.Operations, "Native",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("Metal", "basic-canvas-gpu-metal", "basic-gpu-metal-smoke", new("basic-gpu-metal-fps", HasFps), Animated: true),
			new("Draw", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateNative, collectReferenceFailures: true));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicMauiMacCatalystDrawing()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Mac Catalyst tests require macOS.");
		var app = await BuildMacApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj", "maccatalyst");
		await appium.Run(app, running => MauiDrawing(running.Operations, NavigateMaui, collectReferenceFailures: true));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task GalleryMauiMacCatalyst()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Mac Catalyst tests require macOS.");
		var app = await BuildMacApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj", "maccatalyst");
		await appium.Run(app, running => Gallery(running.Operations));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "macOS")]
	public async Task BasicNativeMacOsNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Native macOS tests require macOS.");
		var app = await BuildMacApp("Basic/macOS", "SkiaSharpSample/SkiaSharpSample.csproj", "mac");
		await appium.Run(app, running => Basic(running.Operations, "Native",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU (GL)", "basic-canvas-gpu-gl", "basic-gpu-gl-smoke", new("basic-gpu-gl-fps", HasFps), Animated: true),
			new("GPU (Metal)", "basic-canvas-gpu-metal", "basic-gpu-metal-smoke", new("basic-gpu-metal-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateNative, collectReferenceFailures: true));
	}

	private async Task<MacRunningApp> BuildMacApp(string folder, string project, string platform)
	{
		var framework = platform == "mac" ? "net10.0-macos" : "net10.0-maccatalyst";
		var architecture = HostArchitecture();
		var rid = (platform == "mac" ? "osx-" : "maccatalyst-") + architecture;
		string FindApp(SampleWorkspace workspace) => MacRunningApp.FindApp(Path.Combine(
			workspace.Root, "samples", folder, Path.GetDirectoryName(project)!, "bin", "Debug", framework, rid));
		var workspace = await appium.PrepareWorkspace($"{platform}/{architecture}", folder, project, async prepared =>
		{
			await BuildSample(prepared, folder, project, new DotNetOptions
			{
				Configuration = "Debug",
				Framework = framework,
				Properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = rid }
			});
			FindApp(prepared);
		});
		return new MacRunningApp(workspace, FindApp(workspace), platform == "maccatalyst");
	}

	private static void NavigateMaui(AppiumOperations operations, string title)
	{
		operations.Click(By.XPath("//*[@label='Menu']"));
		operations.Click(By.XPath($"//*[@label='{title}']"));
	}

	private static void NavigateNative(AppiumOperations operations, string title) =>
		operations.Click(By.XPath($"//*[@label='{title}']"));
}
