using System.Xml.Linq;
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

		await appium.Run(app, running =>
			Basic(running, "Maui", "Windows", HostArchitecture(),
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

		await appium.Run(app, running =>
			Basic(running, "Native", "Windows", HostArchitecture(),
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

		await appium.Run(app, running =>
			MauiDrawing(running, "Windows", HostArchitecture(), NavigateMauiWindows));
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task GalleryMauiWindows()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj");

		await appium.Run(app, running => Gallery(running, "Windows", HostArchitecture()));
	}

	private async Task<WindowsRunningApp> BuildWindowsApp(string folder, string project)
	{
		const string framework = "net10.0-windows10.0.19041.0";
		var architecture = HostArchitecture();

		var properties = new Dictionary<string, string>
		{
			["Platform"] = architecture,
			["AppxPackageSigningEnabled"] = "false"
		};

		var prepared = PrepareSample(folder);
		var selectedProject = Path.Combine(prepared.Root, "samples", folder, project);
		var declaredRids = XDocument.Load(selectedProject)
			.Descendants("RuntimeIdentifiers").SingleOrDefault()?.Value;
		properties["RuntimeIdentifier"] = string.IsNullOrWhiteSpace(declaredRids)
			? "win-" + architecture
			: declaredRids.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Single(rid => rid.EndsWith("-" + architecture, StringComparison.Ordinal));

		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = properties
		});

		var output = await TargetDirectory(workspace, folder, project, framework, properties);
		var manifest = Assert.Single(Directory.EnumerateFiles(output, "AppxManifest.xml", SearchOption.AllDirectories));
		var document = XDocument.Load(manifest);

		var applicationId = (string?)Assert.Single(document.Descendants(),
			element => element.Name.LocalName == "Application").Attribute("Id")
			?? throw new InvalidOperationException($"Generated application ID is missing: {manifest}");
		return new WindowsRunningApp(
			workspace, manifest, applicationId, RequiredEnvironment("SKIASHARP_APPIUM_WINAPPDRIVER"));
	}

	private static async Task<string> TargetDirectory(
		SampleWorkspace workspace, string folder, string project,
		string framework, IReadOnlyDictionary<string, string> properties)
	{
		var selectedProject = Path.Combine(workspace.Root, "samples", folder, project);
		var result = await DotNet.Execute(workspace,
			["msbuild", selectedProject, "-nologo",
				"-getProperty:TargetDir", "-p:Configuration=Debug", $"-p:TargetFramework={framework}",
				$"-p:SampleRootProject={selectedProject}", $"-p:SampleRootFramework={framework}",
				.. properties.Select(property => $"-p:{property.Key}={property.Value}")]);
		if (result.ExitCode != 0)
			throw new InvalidOperationException($"Could not evaluate the built sample output: {result.Output}\n{result.Error}");
		// WinUI's Platform-specific output path is evaluated from the built project.
		var directory = result.Output.Trim();
		var relative = Path.GetRelativePath(workspace.Root, directory);
		if (!Path.IsPathRooted(directory) || Path.IsPathRooted(relative) || relative == ".." ||
			relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !Directory.Exists(directory))
			throw new InvalidOperationException($"Built output must exist inside the owned workspace: {directory}");
		return directory;
	}

	private static void NavigateMauiWindows(AppiumRunningApp app, string title)
	{
		app.Click(By.Name("Open navigation menu"));
		app.Click(By.Name(title));
	}

	private static void NavigateNativeWindows(AppiumRunningApp app, string id) =>
		app.Click(id);
}
