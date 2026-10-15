using System.Runtime.InteropServices;
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

	private protected static string HostArchitecture() => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("Appium desktop/simulator cases declare only arm64 and x64 hosts.")
	};

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

	private protected static async Task MauiDrawing(
		AppiumRunningApp app, string platform,
		Action<AppiumRunningApp, string> navigate)
	{
		const string canvasId = "basic-canvas-drawing";
		navigate(app, "Drawing");
		await Compare(app, canvasId, "basic-drawing-page", "Maui", platform);

		// Axis-aligned strokes avoid timing-dependent diagonal interpolation.
		app.Drag(app.Find(canvasId), 0.3, 0.45, 0.7, 0.45);
		app.Drag(app.Find(canvasId), 0.5, 0.3, 0.5, 0.6);
		await Assert.MultipleAsync(
			() => Compare(app, canvasId, "basic-drawing-two-lines", "Maui", platform),
			async () =>
			{
				app.Click("basic-drawing-clear");
				await Compare(app, canvasId, "basic-drawing-page", "Maui", platform);
			});
	}

	private protected static async Task Gallery(AppiumRunningApp app, string platform, string? canvasHostId = null)
	{
		app.Click("gallery-header-cpu");
		app.Find("gallery-search", "gallery-filter-search").SendKeys("Gradient");
		app.SubmitSearch();
		app.WaitText("gallery-result-count", text => !text.StartsWith("0 ", StringComparison.Ordinal));

		var filterTrigger = app.FindOptional("gallery-filter-trigger");
		if (filterTrigger is not null)
		{
			filterTrigger.Click();
			app.Find("gallery-filter-popup-count");
		}
		else
			app.Find("gallery-filter-sidebar-count");
		app.Click("category-shaders---effects");
		if (filterTrigger is not null)
			app.Click("gallery-filter-done");
		app.Find("gallery-active-category-shaders---effects");

		app.Click("sample-gradient");
		app.WaitText("sample-title", text => text == "Gradient");
		app.WaitText("sample-render-status", text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
		await Compare(app, canvasHostId ?? "sample-canvas-cpu", "gallery-gradient-cpu", "Maui", platform);

		if (app.FindOptional("sample-controls-toggle")?.Text == "Controls ▸")
			app.Click("sample-controls-toggle");
		app.ScrollIntoView("sample-controls", "control-angle");
		app.Drag(app.Find("control-angle"), 0.2, 0.5, 0.0, 0.5);
		app.WaitText("control-angle-value", text => text == "Angle: 0");
		await Compare(app, canvasHostId ?? "sample-canvas-cpu", "gallery-gradient-adjusted", "Maui", platform);

		app.Click("sample-back");
		app.Find("gallery-search", "gallery-filter-search");
		app.Click("sample-gradient");
		app.WaitText("sample-render-status", text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
		app.Click("gallery-header-gpu");
		app.WaitText("sample-render-status", text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
		await Compare(app, canvasHostId ?? "sample-canvas-gpu", "gallery-gradient-gpu", "Maui", platform);
		app.Click("sample-back");
		app.Find("gallery-search", "gallery-filter-search");
	}

	private static Task Compare(
		AppiumRunningApp app, string canvasId, string name, string family, string platform) =>
		SampleImage.Validate(() => app.Capture(canvasId, name), Path.Combine(
			Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected", "Appium",
			family, platform, app.DeviceProfile.Replace('/', Path.DirectorySeparatorChar), name + ".png"));
}
