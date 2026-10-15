using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(AppiumCollection))]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class AppiumWindowsSampleRunTests(AppiumFixture appium) : AppiumSampleRunTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicMauiWindowsNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Maui", "Windows",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMauiWindows));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicNativeWindowsNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Basic/WinUI", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => Basic(running, "Native", "Windows",
		[
			new("basic-navigation-cpu", "basic-canvas-cpu", "basic-cpu"),
			new("basic-navigation-gpu", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("basic-navigation-drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateNativeWindows));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicMauiWindowsDrawing()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");

		await appium.Run(app, running => MauiDrawing(running, "Windows", NavigateMauiWindows));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task GalleryMauiWindows()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");

		await appium.Run(app, running => Gallery(running, "Windows"));
	}

	private async Task<WindowsRunningApp> BuildWindowsApp(string folder, string project)
	{
		const string framework = "net10.0-windows10.0.19041.0";
		var architecture = HostArchitecture();

		var properties = new Dictionary<string, string>
		{
			["Platform"] = architecture,
			["WindowsPackageType"] = "None",
			["WindowsAppSDKSelfContained"] = "true"
		};
		// A global Windows RID would also apply to MAUI's Android restore target.
		if (folder == "Basic/WinUI")
			properties["RuntimeIdentifier"] = "win-" + architecture;

		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = properties
		});

		var selectedProject = Path.Combine(workspace.Root, "samples", folder, project);
		var output = Path.Combine(Path.GetDirectoryName(selectedProject)!, "bin");
		var executable = Assert.Single(Directory.EnumerateFiles(
			output, Path.GetFileNameWithoutExtension(project) + ".exe", SearchOption.AllDirectories));

		return new WindowsRunningApp(workspace, executable, architecture);
	}

	private static void NavigateMauiWindows(AppiumRunningApp app, string title)
	{
		app.Click(By.Name("Open navigation menu"));
		app.Click(By.Name(title));
	}

	private static void NavigateNativeWindows(AppiumRunningApp app, string id) =>
		app.Click(id);
}
