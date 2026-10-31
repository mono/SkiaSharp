using System.Xml.Linq;
using OpenQA.Selenium.Appium;
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
		await appium.Run(app, running => Basic(running.Operations, "Maui",
		[
			new("CPU", "basic-canvas-cpu", "basic-cpu"),
			new("GPU", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("Drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], NavigateMaui, collectReferenceFailures: true));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicNativeWinUiNavigatePages()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");
		var app = await BuildWindowsApp("Basic/WinUI", "SkiaSharpSample/SkiaSharpSample.csproj");
		await appium.Run(app, running => Basic(running.Operations, "Native",
		[
			new("basic-nav-cpu", "basic-canvas-cpu", "basic-cpu"),
			new("basic-nav-gpu", "basic-canvas-gpu", "basic-gpu-smoke", new("basic-gpu-fps", HasFps), Animated: true),
			new("basic-nav-drawing", "basic-canvas-drawing", "basic-drawing-page", new("basic-drawing-clear"))
		], static (operations, id) => operations.Click(id), collectReferenceFailures: true));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicMauiWindowsDrawing()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");
		var app = await BuildWindowsApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj");
		await appium.Run(app, running => MauiDrawing(running.Operations, NavigateMaui, collectReferenceFailures: true));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task GalleryMauiWindows()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");
		var app = await BuildWindowsApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");
		await appium.Run(app, running => Gallery(running.Operations));
	}

	private async Task<WindowsRunningApp> BuildWindowsApp(string folder, string project)
	{
		const string framework = "net10.0-windows10.0.19041.0";
		const string rid = "win-x64";
		string FindApp(SampleWorkspace workspace) => WindowsRunningApp.FindApp(Path.Combine(
			workspace.Root, "samples", folder, Path.GetDirectoryName(project)!, "bin", "Debug", framework, rid), project);
		var workspace = await appium.PrepareWorkspace("windows/x64", folder, project, async prepared =>
		{
			// A global Windows RID would also reach MAUI's Android restore.
			// Set it early for output paths and late for single-target project restore.
			foreach (var name in new[] { "Directory.Build.props", "Directory.Build.targets" })
			{
				var path = Path.Combine(prepared.Root, "samples", name);
				var properties = XElement.Load(path);
				properties.Add(new XElement("PropertyGroup",
					new XAttribute("Condition", $"'$(TargetFramework)' == '{framework}'"),
					new XElement("RuntimeIdentifier", rid)));
				properties.Save(path);
			}
			await BuildSample(prepared, folder, project, new DotNetOptions
			{
				Configuration = "Debug",
				Framework = framework,
				Properties = new Dictionary<string, string>
				{
					["WindowsPackageType"] = "None",
					["WindowsAppSDKSelfContained"] = "true"
				}
			});
		});
		return new WindowsRunningApp(workspace, FindApp(workspace),
			folder == "Gallery" ? "SkiaSharp Gallery" : "SkiaSharp Sample");
	}

	private static void NavigateMaui(AppiumOperations operations, string title)
	{
		operations.Click(MobileBy.AccessibilityId("OK"));
		operations.Click(new ByName(title));
	}
}
