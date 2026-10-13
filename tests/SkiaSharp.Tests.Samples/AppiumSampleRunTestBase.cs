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

	private protected static string HostArchitecture() => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("Appium desktop/simulator cases declare only arm64 and x64 hosts.")
	};

	private protected static string RequiredEnvironment(string name) =>
		Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value :
			throw new InvalidOperationException($"Set {name} to the explicitly installed platform driver's path before selecting its tests.");

	private protected static Task Basic(
		AppiumRunningApp app,
		string family, string platform, string device,
		IReadOnlyList<BasicPageCommand> pages,
		Action<AppiumRunningApp, string> navigate)
	{
		Assert.NotEmpty(pages);
		List<string> captures = [];
		for (var index = 0; index < pages.Count; index++)
		{
			var page = pages[index];
			navigate(app, page.NavigationId);
			app.Find(page.CanvasId);

			if (page.ElementCheck is { } check)
			{
				if (check.HasValue is { } hasValue)
					app.WaitText(check.Id, hasValue);
				else
					app.Find(check.Id);
			}

			string capture;
			if (page.Animated)
			{
				// Basic GPU pages remain animated; capture a render smoke, not a stable golden.
				capture = app.Capture(page.CanvasId, page.ScreenshotName);
				SampleImage.AssertRendered(capture);
			}
			else
			{
				capture = app.CaptureStable(page.CanvasId, page.ScreenshotName);
				if (index == 0)
					SampleImage.AssertRendered(capture);
			}
			captures.Add(capture);
		}

		var cpu = captures[0];
		var cpuPage = pages[0];
		navigate(app, cpuPage.NavigationId);
		var returned = app.CaptureStable(cpuPage.CanvasId, "basic-cpu-returned");
		Assert.Multiple(
			() => SampleImage.Validate(returned, cpu),
			() => Compare(cpu, family, platform, device, "basic-cpu"));

		return Task.CompletedTask;
	}

	private protected static Task MauiDrawing(
		AppiumRunningApp app, string platform, string device,
		Action<AppiumRunningApp, string> navigate)
	{
		const string canvasId = "basic-canvas-drawing";
		navigate(app, "Drawing");
		var empty = app.CaptureStable(canvasId, "drawing-empty");

		// Element-relative crossing strokes stay clear of the floating page controls.
		app.Drag(app.Find(canvasId), 0.3, 0.3, 0.7, 0.6);
		var firstLine = app.CaptureStable(canvasId, "drawing-first-line");
		SampleImage.AssertSceneChanged(empty, firstLine);

		app.Drag(app.Find(canvasId), 0.3, 0.6, 0.7, 0.3);
		var twoLines = app.CaptureStable(canvasId, "basic-drawing-two-lines");
		SampleImage.AssertSceneChanged(firstLine, twoLines);

		app.Click("basic-drawing-clear");
		var cleared = app.CaptureStable(canvasId, "drawing-cleared");
		navigate(app, "CPU");
		app.Find("basic-canvas-cpu");

		Assert.Multiple(
			() => SampleImage.Validate(cleared, empty),
			() => Compare(twoLines, "Maui", platform, device, "basic-drawing-two-lines"));

		return Task.CompletedTask;
	}

	private protected static Task Gallery(AppiumRunningApp app, string platform, string device)
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
		var cpu = app.CaptureStable("sample-canvas-cpu", "gallery-gradient-cpu");
		SampleImage.AssertRendered(cpu);

		var before = app.Text("control-angle-value");
		app.Drag(app.Find("control-angle"), 0.2, 0.5, 0.7, 0.5);
		app.WaitText("control-angle-value", text => text != before);
		SampleImage.AssertSceneChanged(cpu, app.CaptureStable("sample-canvas-cpu", "gallery-gradient-adjusted"));

		app.Click("sample-back");
		app.Find("gallery-search");
		app.Click("gallery-header-gpu");
		app.Click("sample-gradient");
		app.WaitText("sample-render-status", text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
		var gpu = app.CaptureStable("sample-canvas-gpu", "gallery-gradient-gpu");
		SampleImage.AssertRendered(gpu);
		app.Click("sample-back");
		app.Find("gallery-search");

		Assert.Multiple(
			() => Compare(cpu, "Maui", platform, device, "gallery-gradient-cpu"),
			() => Compare(gpu, "Maui", platform, device, "gallery-gradient-gpu"));

		return Task.CompletedTask;
	}

	private static void Compare(string actual, string family, string platform, string device, string name) =>
		SampleImage.Validate(actual, Path.Combine(
			Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", "Appium",
			family, platform, device.Replace('/', Path.DirectorySeparatorChar), name + ".png"));
}
