using System.Runtime.InteropServices;
using SkiaSharp.Tests.Samples.Utils;
using SkiaSharp.Tests.Shared;
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

	private protected static string HostArchitecture() => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.Arm64 => "arm64",
		Architecture.X64 => "x64",
		_ => throw new InvalidOperationException("iOS simulator cases declare only arm64 and x64 hosts.")
	};

	private protected static async Task Basic(
		AppiumOperations app,
		string family,
		IReadOnlyList<BasicPageCommand> pages,
		Action<AppiumOperations, string> navigate)
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
				await Compare(app, page.CanvasId, page.ScreenshotName, family);
			}
		}
	}

	private protected static async Task MauiDrawing(
		AppiumOperations app,
		Action<AppiumOperations, string> navigate)
	{
		const string canvasId = "basic-canvas-drawing";
		navigate(app, "Drawing");
		await Compare(app, canvasId, "basic-drawing-initial", "Maui");

		// Axis-aligned strokes avoid timing-dependent diagonal interpolation.
		app.Drag(app.Find(canvasId), 0.3, 0.45, 0.7, 0.45);
		app.Drag(app.Find(canvasId), 0.5, 0.3, 0.5, 0.6);
		await Assert.MultipleAsync(
			async () =>
			{
				var drawn = await Compare(app, canvasId, "basic-drawing-two-lines", "Maui");
				AssertDrawingStrokeRegions(drawn.Actual, drawn.Expected);
			},
			async () =>
			{
				app.Click("basic-drawing-clear");
				var cleared = await Compare(app, canvasId, "basic-drawing-page", "Maui");
				AssertDrawingStrokeRegions(cleared.Actual, cleared.Expected);
			});
	}

	internal static void AssertDrawingStrokeRegions(string actual, string expected)
	{
		Assert.True(File.Exists(expected), $"Missing reviewed sample golden: {expected}.");
		using var actualImage = SKImage.FromEncodedData(actual);
		using var expectedImage = SKImage.FromEncodedData(expected);
		Assert.NotNull(actualImage);
		Assert.NotNull(expectedImage);
		Assert.Equal(expectedImage.Width, actualImage.Width);
		Assert.Equal(expectedImage.Height, actualImage.Height);
		// Check each stroke away from the intersection: either one fits inside the full-canvas error budget.
		foreach (var region in new[]
		{
			SKRectI.Round(new SKRect(actualImage.Width * 0.35f, actualImage.Height * 0.44f,
				actualImage.Width * 0.45f, actualImage.Height * 0.46f)),
			SKRectI.Round(new SKRect(actualImage.Width * 0.49f, actualImage.Height * 0.35f,
				actualImage.Width * 0.51f, actualImage.Height * 0.40f))
		})
		{
			using var actualStroke = actualImage.Subset(region);
			using var expectedStroke = expectedImage.Subset(region);
			Assert.NotNull(actualStroke);
			Assert.NotNull(expectedStroke);
			var result = SKPixelComparer.Compare(expectedStroke, actualStroke, 0);
			Assert.True(result.ErrorPixelCount <= result.TotalPixels * GoldenOptions.DefaultMaxErrorPixelFraction,
				$"Drawing stroke region {region} differs: {result.ErrorPixelCount}/{result.TotalPixels} pixels; actual {actual}; expected {expected}.");
		}
	}

	private protected static async Task Gallery(AppiumOperations app)
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
		await Assert.MultipleAsync(
			async () => await Compare(app, "sample-canvas-cpu", "gallery-gradient-cpu", "Maui"),
			async () =>
			{
				if (app.FindOptional("sample-controls-toggle")?.Text == "Controls ▸")
					app.Click("sample-controls-toggle");
				app.ScrollIntoView("sample-controls", "control-angle");
				app.Drag(app.Find("control-angle"), 0.2, 0.5, 0.0, 0.5);
				app.WaitText("control-angle-value", text => text == "Angle: 0");
				await Compare(app, "sample-canvas-cpu", "gallery-gradient-adjusted", "Maui");
			},
			async () =>
			{
				app.Click("sample-back");
				app.Find("gallery-search", "gallery-filter-search");
				app.Click("sample-gradient");
				app.WaitText("sample-render-status", text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
				app.Click("gallery-header-gpu");
				app.WaitText("sample-render-status", text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
				await Compare(app, "sample-canvas-gpu", "gallery-gradient-gpu", "Maui");
			});
		app.Click("sample-back");
		app.Find("gallery-search", "gallery-filter-search");
	}

	private static Task<SampleImage.Comparison> Compare(
		AppiumOperations app, string canvasId, string name, string family) =>
		SampleImage.Compare(
			() => Task.FromResult(app.Capture(canvasId, name)),
			app.Reference(CaseName(name, family)), transformReference: app.TransformReference,
			rawActual: AppiumOperations.RawCapture);

	internal static string CaseName(string name, string family) =>
		name.StartsWith("basic-", StringComparison.Ordinal)
			? family.ToLowerInvariant() + "-" + name.Replace("-page", "")
			: name;
}
