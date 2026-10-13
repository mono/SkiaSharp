using System.Runtime.InteropServices;
using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public abstract class AppiumSampleRunTestBase : DotNetSampleTestBase
{
	private protected sealed record BasicPageCommand(
		string NavigationId, string CanvasId, string ScreenshotName,
		PageElementCheck? ElementCheck = null, bool Animated = false);

	private protected sealed record PageElementCheck(string Id, Func<string, bool>? HasValue = null);

	private protected static bool HasFps(string text) =>
		text.Contains("FPS:", StringComparison.Ordinal) && !text.Contains("--", StringComparison.Ordinal);

	private protected static string Output(SampleWorkspace workspace, string folder, string project, string framework) =>
		Path.Combine(workspace.Root, "samples", folder, Path.GetDirectoryName(project)!, "bin", "Debug", framework);

	private protected static string? ProjectApplicationId(string project)
	{
		var applicationId = XDocument.Load(project).Descendants("ApplicationId").SingleOrDefault()?.Value.Trim();
		return string.IsNullOrEmpty(applicationId) ? null : applicationId;
	}

	private protected static string AppleApplicationId(string project)
	{
		if (ProjectApplicationId(project) is { } applicationId)
			return applicationId;

		var plist = Path.Combine(Path.GetDirectoryName(project)!, "Info.plist");
		var identifier = XDocument.Load(plist).Descendants("key")
			.Single(key => key.Value == "CFBundleIdentifier").ElementsAfterSelf().First();
		if (identifier.Name != "string" || string.IsNullOrWhiteSpace(identifier.Value))
			throw new InvalidOperationException($"Source CFBundleIdentifier is missing: {plist}");
		return identifier.Value.Trim();
	}

	private protected static string AndroidApplicationId(string project)
	{
		if (ProjectApplicationId(project) is { } applicationId)
			return applicationId;

		var manifest = Path.Combine(Path.GetDirectoryName(project)!, "Properties", "AndroidManifest.xml");
		var package = XDocument.Load(manifest).Root?.Attribute("package")?.Value;
		if (string.IsNullOrWhiteSpace(package))
			throw new InvalidOperationException($"Source application ID is missing: {manifest}");
		return package.Trim();
	}

	private protected static string HostArchitecture() => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("Appium desktop/simulator cases declare only arm64 and x64 hosts.")
	};

	private protected static string RequiredEnvironment(string name) =>
		Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value :
			throw new InvalidOperationException($"Set {name} to the explicitly installed platform driver's path before selecting its tests.");

	private protected static async Task Basic(
		AppiumRunningApp app,
		string family, string platform,
		IReadOnlyList<BasicPageCommand> pages,
		Action<AppiumRunningApp, string> navigate)
	{
		Assert.NotEmpty(pages);
		foreach (var page in pages)
		{
			navigate(app, page.NavigationId);
			app.Find(page.CanvasId);

			if (page.ElementCheck is { } check)
			{
				if (check.HasValue is { } hasValue)
					app.WaitText(check.Id, hasValue);
				else
					app.Find(check.Id);
			}

			if (page.Animated)
			{
				var capture = app.Capture(page.CanvasId, page.ScreenshotName);
				TestContext.Current.AddFileAttachment(capture, "image/png");
			}
			else
			{
				await Compare(app, page.CanvasId, page.ScreenshotName, family, platform);
			}
		}
	}

	private protected static Task MauiDrawing(
		AppiumRunningApp app, string platform,
		Action<AppiumRunningApp, string> navigate)
	{
		const string canvasId = "basic-canvas-drawing";
		navigate(app, "Drawing");

		// Element-relative crossing strokes stay clear of the floating page controls.
		app.Drag(app.Find(canvasId), 0.3, 0.3, 0.7, 0.6);
		app.Drag(app.Find(canvasId), 0.3, 0.6, 0.7, 0.3);
		return Assert.MultipleAsync(
			() => Compare(app, canvasId, "basic-drawing-two-lines", "Maui", platform),
			async () =>
			{
				app.Click("basic-drawing-clear");
				await Compare(app, canvasId, "basic-drawing-page", "Maui", platform);
			});
	}

	private protected static async Task Gallery(AppiumRunningApp app, string platform)
	{
		app.Click("gallery-header-cpu");
		app.Find("gallery-search").SendKeys("Gradient");
		app.SubmitSearch();
		app.WaitText("gallery-result-count", text => !text.StartsWith("0 ", StringComparison.Ordinal));

		app.Click("gallery-filter-trigger");
		app.Click("category-shaders---effects");
		app.Find("gallery-filter-popup-count");
		app.Click("gallery-filter-done");
		app.Find("gallery-active-category-shaders---effects");

		app.Click("sample-gradient");
		app.WaitText("sample-title", text => text == "Gradient");
		app.WaitText("sample-render-status", text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
		await Compare(app, "sample-canvas-cpu", "gallery-gradient-cpu", "Maui", platform);

		var before = app.Text("control-angle-value");
		app.Drag(app.Find("control-angle"), 0.2, 0.5, 0.7, 0.5);
		app.WaitText("control-angle-value", text => text != before);
		await Compare(app, "sample-canvas-cpu", "gallery-gradient-adjusted", "Maui", platform);

		app.Click("sample-back");
		app.Find("gallery-search");
		app.Click("gallery-header-gpu");
		app.Click("sample-gradient");
		app.WaitText("sample-render-status", text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
		await Compare(app, "sample-canvas-gpu", "gallery-gradient-gpu", "Maui", platform);
		app.Click("sample-back");
		app.Find("gallery-search");
	}

	private static Task Compare(
		AppiumRunningApp app, string canvasId, string name, string family, string platform) =>
		SampleImage.Validate(() => app.Capture(canvasId, name), Path.Combine(
			Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", "Appium",
			family, platform, app.DeviceProfile.Replace('/', Path.DirectorySeparatorChar), name + ".png"));
}
