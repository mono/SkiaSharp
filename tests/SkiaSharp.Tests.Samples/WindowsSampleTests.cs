using System.Xml.Linq;
using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(AppiumCollection.Name)]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class WindowsSampleTests(AppiumFixture appium) : AppiumSampleTestBase
{
	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicMauiWindows()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
			"win-" + HostArchitecture());

		await appium.Run(app, running =>
		{
			Basic(running, "Maui", "Windows", HostArchitecture(),
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu", "basic-gpu-fps",
				() => NavigateMauiWindows(running, "Drawing"),
				() => NavigateMauiWindows(running, "GPU"),
				() => NavigateMauiWindows(running, "CPU"));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task BasicNativeWindows()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Basic/WinUI", "SkiaSharpSample/SkiaSharpSample.csproj",
			"win10-" + HostArchitecture());

		await appium.Run(app, running =>
		{
			Basic(running, "Native", "Windows", HostArchitecture(),
				"basic-canvas-cpu", "basic-canvas-drawing", "basic-drawing-clear",
				"basic-canvas-gpu", "basic-gpu-fps",
				() => running.Click("basic-navigation-drawing"),
				() => running.Click("basic-navigation-gpu"),
				() => running.Click("basic-navigation-cpu"));

			return Task.CompletedTask;
		});
	}

	[Fact]
	[RunGroup(SampleRunGroup.Windows)]
	[Trait("Platform", "Windows")]
	public async Task GalleryMauiWindows()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows desktop tests require Windows.");

		var app = await BuildWindowsApp("Gallery", "Maui/SkiaSharpSample.Maui.csproj",
			"win-" + HostArchitecture());

		await appium.Run(app, running => Gallery(running, "Windows", HostArchitecture()));
	}

	private async Task<WindowsRunningApp> BuildWindowsApp(
		string folder, string project, string runtimeIdentifier)
	{
		const string framework = "net10.0-windows10.0.19041.0";
		var architecture = HostArchitecture();

		var properties = new Dictionary<string, string>
		{
			["Platform"] = architecture,
			["RuntimeIdentifier"] = runtimeIdentifier,
			["AppxPackageSigningEnabled"] = "false"
		};
		var workspace = await BuildSample(folder, project, new DotNetOptions
		{
			Configuration = "Debug",
			Framework = framework,
			Properties = properties
		});

		var output = await TargetDirectory(workspace, folder, project, framework, properties);
		var manifest = Assert.Single(Directory.EnumerateFiles(
			output, "AppxManifest.xml", SearchOption.AllDirectories));
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
		var result = await DotNet.Execute(workspace,
			["msbuild", Path.Combine(workspace.Root, "samples", folder, project), "-nologo",
				"-getProperty:TargetDir", "-p:Configuration=Debug", $"-p:TargetFramework={framework}",
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
}
