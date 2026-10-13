using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(AppiumCollection.Name)]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class MacSampleRunTests(AppiumFixture appium) : AppiumSampleRunTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicMauiMacCatalyst()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
			"net10.0-maccatalyst", "com.companyname.SkiaSharpSample");

		await appium.Run(app, running =>
		{
			Basic(running, "Maui", "MacCatalyst", HostArchitecture(),
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu", "basic-gpu-fps",
				() => NavigateMauiMacCatalyst(running, "Drawing"),
				() => NavigateMauiMacCatalyst(running, "GPU"),
				() => NavigateMauiMacCatalyst(running, "CPU"));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task BasicNativeMacCatalyst()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Basic/MacCatalyst", "SkiaSharpSample/SkiaSharpSample.csproj",
			"net10.0-maccatalyst", "com.companyname.SkiaSharpSample");

		await appium.Run(app, running =>
		{
			Basic(running, "Native", "MacCatalyst", HostArchitecture(),
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu-metal", "basic-gpu-metal-fps",
				() => running.Click(By.Name("Drawing")),
				() => running.Click(By.Name("Metal")),
				() => running.Click(By.Name("CPU")));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "macOS")]
	public async Task BasicNativeMacOs()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Native AppKit tests require macOS.");

		var app = await BuildMacApp("Basic/macOS", "SkiaSharpSample/SkiaSharpSample.csproj",
			"net10.0-macos", "com.companyname.skiasharpsample");

		await appium.Run(app, running =>
		{
			Basic(running, "Native", "macOS", HostArchitecture(),
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu-gl", "basic-gpu-gl-fps",
				() => running.Click(By.Name("Drawing")),
				() => running.Click(By.Name("GPU (GL)")),
				() => running.Click(By.Name("CPU")));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Mac)]
	[Trait("Platform", "MacCatalyst")]
	public async Task GalleryMauiMacCatalyst()
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "MacCatalyst tests require macOS.");

		var app = await BuildMacApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj",
			"net10.0-maccatalyst", "com.skiasharp.gallery.maui");

		await appium.Run(app, running => Gallery(running, "MacCatalyst", HostArchitecture()));
	}

	private async Task<MacRunningApp> BuildMacApp(
		string folder, string project, string framework, string applicationId)
	{
		var rid = (framework == "net10.0-macos" ? "osx-" : "maccatalyst-") + HostArchitecture();

		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = new Dictionary<string, string> { ["RuntimeIdentifier"] = rid }
		});

		var artifact = Assert.Single(Directory.EnumerateDirectories(
			Path.Combine(Output(workspace, folder, project, framework), rid), "*.app"));

		return new MacRunningApp(
			workspace, artifact, applicationId, RequiredEnvironment("SKIASHARP_APPIUM_MAC_WDA_ROOT"));
	}

	private static void NavigateMauiMacCatalyst(AppiumRunningApp app, string title)
	{
		app.Click(By.Name("Menu"));
		app.Click(By.Name(title));
	}
}
