using System.Runtime.InteropServices;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public abstract class AppiumSampleRunTestBase : DotNetSampleTestBase
{
	private protected static string Output(SampleWorkspace workspace, string folder, string project, string framework) =>
		Path.Combine(workspace.Root, "samples", folder, Path.GetDirectoryName(project)!, "bin", "Debug", framework);

	private protected static string HostArchitecture() => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("Appium desktop/simulator cases declare only arm64 and x64 hosts.")
	};

	private protected static string RequiredEnvironment(string name) =>
		Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value :
			throw new InvalidOperationException($"Set {name} to the explicitly installed platform driver's path before selecting its tests.");

	private protected static void Basic(
		AppiumRunningApp app,
		string family, string platform, string device,
		string cpuId, string drawingId, string clearId,
		string gpuId, string fpsId,
		Action navigateDrawing, Action navigateGpu, Action navigateCpu)
	{
		var cpu = app.CaptureStable(cpuId, "basic-cpu");
		AppiumRunningApp.AssertRendered(cpu);

		// Verify drawing changes the canvas, and clearing restores its original pixels.
		navigateDrawing();
		var empty = app.CaptureStable(drawingId, "drawing-empty");
		app.Drag(app.Find(drawingId), 0.25, 0.3, 0.7, 0.5);
		var drawn = app.CaptureStable(drawingId, "drawing-stroke");
		AppiumRunningApp.AssertSceneChanged(empty, drawn);
		app.Click(clearId);
		SampleImage.Validate(app.CaptureStable(drawingId, "drawing-cleared"), empty);

		// Animated Basic GPU scenes use a rendering smoke check, not a frozen golden.
		navigateGpu();
		app.WaitText(fpsId, text => text.Contains("FPS:", StringComparison.Ordinal) && !text.Contains("--"));
		AppiumRunningApp.AssertRendered(app.Capture(gpuId, "basic-gpu-smoke"));

		navigateCpu();
		SampleImage.Validate(app.CaptureStable(cpuId, "basic-cpu-returned"), cpu);
		Compare(cpu, family, platform, device, "basic-cpu");
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
		app.WaitText("sample-render-status",
			text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
		var cpu = app.CaptureStable("sample-canvas-cpu", "gallery-gradient-cpu");
		AppiumRunningApp.AssertRendered(cpu);

		var before = app.Text("control-angle-value");
		app.Drag(app.Find("control-angle"), 0.2, 0.5, 0.7, 0.5);
		app.WaitText("control-angle-value", text => text != before);
		AppiumRunningApp.AssertSceneChanged(cpu, app.CaptureStable("sample-canvas-cpu", "gallery-gradient-adjusted"));

		app.Click("sample-back");
		app.Find("gallery-search");
		app.Click("gallery-header-gpu");
		app.Click("sample-gradient");
		app.WaitText("sample-render-status",
			text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
		var gpu = app.CaptureStable("sample-canvas-gpu", "gallery-gradient-gpu");
		AppiumRunningApp.AssertRendered(gpu);
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
