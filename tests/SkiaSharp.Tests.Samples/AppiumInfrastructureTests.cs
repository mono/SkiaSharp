using System.Collections.ObjectModel;
using System.Drawing;
using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests : IDisposable
{
	private readonly string root = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("appium-infrastructure"));

	private SampleWorkspace Workspace()
	{
		var inputs = Path.Combine(root, "inputs");
		var packages = Path.Combine(root, "fixture-packages");
		Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
		Directory.CreateDirectory(packages);
		File.WriteAllText(Path.Combine(packages, "unit-fixture.nupkg"), "Workspace isolation sentinel; not a package cohort.");
		return new("Sample", inputs, packages, Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
	}

	[Fact]
	public async Task FailedSessionStillRunsDiagnosticsAndStopsTheServer()
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order);
		var failure = await Assert.ThrowsAsync<NotSupportedException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => throw new InvalidOperationException("Scenario must not run."),
			() => { order.Add("server-start"); return Task.CompletedTask; },
			() => { order.Add("server-stop"); return Task.CompletedTask; }));
		Assert.Same(app.StartFailure, failure);
		Assert.Equal(new[] { "server-start", "session-start", "diagnostics", "server-stop" }, order);
		Assert.True(File.Exists(Path.Combine(app.Diagnostics, "failure.log")));
		await new IosRunningApp(workspace, artifact).SaveDiagnostics();
		await Assert.ThrowsAsync<InvalidOperationException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask,
			() => Task.CompletedTask, () => Task.CompletedTask));
	}

	[Fact]
	public async Task DiagnosticsFailuresNeverHideStartupOrPreventServerCleanup()
	{
		using var workspace = Workspace();
		var artifact = Path.Combine(workspace.Root, "Sample.app");
		Directory.CreateDirectory(artifact);
		var order = new List<string>();
		var app = new DiagnosticApp(workspace, artifact, order, failDiagnostics: true);
		var failure = await Assert.ThrowsAsync<AggregateException>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask, () => Task.CompletedTask,
			() => { order.Add("server-stop"); throw new IOException("server-stop"); }));
		Assert.Same(app.StartFailure, failure.InnerExceptions[0]);
		Assert.Equal(3, failure.InnerExceptions.Count);
		Assert.Equal(new[] { "session-start", "diagnostics", "server-stop" }, order);
	}

	[Theory]
	[InlineData(null, null, "latest")]
	[InlineData("", null, "latest")]
	[InlineData("latest", null, "latest")]
	[InlineData("LATEST", null, "latest")]
	[InlineData("26.5", "latest", "latest")]
	[InlineData("27.0", "latest", "latest")]
	[InlineData("18.6", null, "18.6")]
	[InlineData("18.6", "18.6", "18.6")]
	[InlineData("18.6.1", "18.6", "18.6")]
	[InlineData("26.5", null, "26.5")]
	public void IosGoldenProfileIsIndependentOfTheSelectedRuntime(
		string? version, string? referenceVersion, string expectedVersion)
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", version, referenceVersion);
		Assert.Equal($"iPhone-16-Pro/{expectedVersion}", profile);

		foreach (var family in new[] { "Maui", "Native" })
		{
			var expected = AppiumSampleRunTestBase.Expected(profile, "basic-cpu", family);
			Assert.Equal(Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
				"Appium", family, "iOS", "iPhone-16-Pro", expectedVersion, "basic-cpu.png"), expected);
		}
	}

	[Theory]
	[InlineData("latest", "not-reviewed")]
	[InlineData("18.6", "not-reviewed")]
	[InlineData("99.0", "basic-cpu")]
	public void IosGoldensFailWithoutFallingBackToAnotherProfile(string referenceVersion, string name)
	{
		var profile = IosRunningApp.ReferenceProfile("iPhone 16 Pro", "26.5", referenceVersion);
		var expected = AppiumSampleRunTestBase.Expected(profile, name, "Maui");
		var actual = AppiumSampleRunTestBase.Expected("iPhone-16-Pro/latest", "basic-cpu", "Maui");
		Assert.True(File.Exists(actual));
		Assert.False(File.Exists(expected));

		// Missing references fail before image decoding and must never be created by a test.
		var failure = Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, expected));
		Assert.Contains($"Missing reviewed sample golden: {expected}", failure.Message);
		Assert.False(File.Exists(expected));
	}

	[Fact]
	public async Task ArtifactOutsideWorkspaceFailsBeforeServerStartup()
	{
		using var workspace = Workspace();
		var app = new DiagnosticApp(workspace, Path.Combine(root, "foreign.app"), []);
		var started = false;
		var failure = await Assert.ThrowsAnyAsync<Exception>(() => app.Run(
			new Uri("http://127.0.0.1:1"), _ => Task.CompletedTask,
			() => { started = true; return Task.CompletedTask; }, () => Task.CompletedTask));
		Assert.Contains("must belong to the selected workspace", failure.Message);
		Assert.False(started);
	}

	private sealed class DiagnosticApp(
		SampleWorkspace workspace, string artifact, List<string> order, bool failDiagnostics = false)
		: AppiumRunningApp(workspace, artifact, new IosAppiumOperations())
	{
		internal Exception StartFailure { get; } = new NotSupportedException("Session construction failed.");
		internal override string DeviceProfile => "unit-fixture";
		internal override Task<string> ReadApplicationId() => Task.FromResult("unit.fixture");
		protected override Task Start(Uri address)
		{
			order.Add("session-start");
			throw StartFailure;
		}
		protected override Task CapturePlatformDiagnostics()
		{
			order.Add("diagnostics");
			if (failDiagnostics)
				throw new IOException("diagnostics");
			return Task.CompletedTask;
		}
	}

	[Fact]
	public void IosOperationsReuseTheAppsOwnedSessionWithoutStartingOne()
	{
		using var workspace = Workspace();
		var app = new IosRunningApp(workspace, Path.Combine(workspace.Root, "Sample.app"));
		AppiumRunningApp running = app;
		var operations = Assert.IsType<IosAppiumOperations>(running.Operations);
		Assert.Same(operations, app.Operations);
		Assert.Equal(app.DeviceProfile, operations.DeviceProfile);
		Assert.Throws<InvalidOperationException>(() => operations.Attach(app));

		var failure = Assert.Throws<InvalidOperationException>(() => operations.Find("canvas"));
		Assert.Equal("The owned Appium session is not ready.", failure.Message);
	}

	[Fact]
	public void OperationsRequireAnOwnerBeforeUsingTheSession()
	{
		var operations = new IosAppiumOperations();
		var failure = Assert.Throws<InvalidOperationException>(() => operations.Find("canvas"));
		Assert.Equal("Appium operations have not been attached to an app.", failure.Message);
		Assert.Throws<InvalidOperationException>(() => operations.DeviceProfile);
		Assert.Throws<ArgumentNullException>(() => operations.Attach(null!));
	}

	[Fact]
	public void CapturesOfTheSameSceneKeepBothFiles()
	{
		using var workspace = Workspace();
		var app = new IosRunningApp(workspace, Path.Combine(workspace.Root, "Sample.app"));
		var before = app.Operations.SaveCapture([1, 2], "drawing");
		var after = app.Operations.SaveCapture([3, 4], "drawing");
		Assert.NotEqual(before, after);
		Assert.Equal(Path.Combine(workspace.DiagnosticsRoot, "screenshots", "01-drawing.png"), before);
		Assert.Equal(Path.Combine(workspace.DiagnosticsRoot, "screenshots", "02-drawing.png"), after);
		Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(before));
		Assert.Equal(new byte[] { 3, 4 }, File.ReadAllBytes(after));
		Assert.Throws<ArgumentException>(() => app.Operations.SaveCapture([], "../foreign"));
	}

	[Theory]
	[InlineData("18.6", true)]
	[InlineData("18.6", false)]
	[InlineData("latest", true)]
	[InlineData("latest", false)]
	public void DrawingChecksRejectEitherMissingStrokeAndIncompleteClear(string version, bool horizontal)
	{
		var references = Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
			"Appium", "Maui", "iOS", "iPhone-16-Pro", version);
		var drawn = Path.Combine(references, "basic-drawing-two-lines.png");
		var blank = Path.Combine(references, "basic-drawing-page.png");
		AppiumSampleRunTestBase.AssertDrawingStrokeRegions(drawn, drawn);
		AppiumSampleRunTestBase.AssertDrawingStrokeRegions(blank, blank);

		using var workspace = Workspace();
		using var drawnBitmap = SKBitmap.Decode(drawn);
		using var blankBitmap = SKBitmap.Decode(blank);
		Assert.NotNull(drawnBitmap);
		Assert.NotNull(blankBitmap);
		using var missingStroke = drawnBitmap.Copy();
		using var incompleteClear = blankBitmap.Copy();
		var region = horizontal
			? SKRectI.Round(new SKRect(drawnBitmap.Width * 0.35f, drawnBitmap.Height * 0.43f,
				drawnBitmap.Width * 0.45f, drawnBitmap.Height * 0.47f))
			: SKRectI.Round(new SKRect(drawnBitmap.Width * 0.48f, drawnBitmap.Height * 0.35f,
				drawnBitmap.Width * 0.52f, drawnBitmap.Height * 0.40f));
		for (var y = region.Top; y < region.Bottom; y++)
			for (var x = region.Left; x < region.Right; x++)
			{
				missingStroke.SetPixel(x, y, blankBitmap.GetPixel(x, y));
				incompleteClear.SetPixel(x, y, drawnBitmap.GetPixel(x, y));
			}
		var missing = Path.Combine(workspace.Root, "missing-stroke.png");
		var residual = Path.Combine(workspace.Root, "incomplete-clear.png");
		using (var data = missingStroke.Encode(SKEncodedImageFormat.Png, 100))
			File.WriteAllBytes(missing, data.ToArray());
		using (var data = incompleteClear.Encode(SKEncodedImageFormat.Png, 100))
			File.WriteAllBytes(residual, data.ToArray());
		Assert.ThrowsAny<Exception>(() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(missing, drawn));
		Assert.ThrowsAny<Exception>(() => AppiumSampleRunTestBase.AssertDrawingStrokeRegions(residual, blank));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task IosAppIdentityReadsXmlAndBinaryBundlePlists(bool binary)
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Apple bundle metadata requires macOS plutil.");
		using var workspace = Workspace();
		var bundle = Path.Combine(workspace.Root, "Built.app");
		Directory.CreateDirectory(bundle);
		var plist = Path.Combine(bundle, "Info.plist");
		File.WriteAllText(plist, """
			<?xml version="1.0" encoding="UTF-8"?>
			<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
			<plist version="1.0"><dict><key>CFBundleIdentifier</key><string>com.skiasharp.built</string></dict></plist>
			""");
		if (binary)
		{
			var result = await ProcessRunner.Run("plutil", ["-convert", "binary1", plist], workspace.Root, TimeSpan.FromSeconds(30));
			Assert.Equal(0, result.ExitCode);
		}
		Assert.Equal("com.skiasharp.built", await new IosRunningApp(workspace, bundle).ReadApplicationId());
	}

	[Theory]
	[InlineData(610, 207, 819, 18, 114)]
	[InlineData(610, 207, 841, 35, 124)]
	[InlineData(610, 207, 590, 18, -114)]
	[InlineData(610, 207, 610, 18, 0)]
	[InlineData(610, 207, 799, 18, 0)]
	public void ScrollAmountComesFromCurrentBounds(int top, int height, int targetTop, int targetHeight, int expected) =>
		Assert.Equal(expected, AppiumOperations.ScrollAmount(
			new Rectangle(12, top, 378, height), new Rectangle(24, targetTop, 80, targetHeight)));

	[Theory]
	[InlineData(0, 100, 20, 20)]
	[InlineData(100, 0, 20, 20)]
	[InlineData(100, 100, 0, 20)]
	[InlineData(100, 100, 101, 20)]
	[InlineData(100, 100, 20, 101)]
	public void ScrollRejectsImpossibleGeometry(int width, int height, int targetWidth, int targetHeight) =>
		Assert.Throws<InvalidOperationException>(() => AppiumOperations.ScrollAmount(
			new Rectangle(10, 20, width, height), new Rectangle(10, 20, targetWidth, targetHeight)));

	[Theory]
	[InlineData(3)]
	[InlineData(207)]
	[InlineData(901)]
	public void ScrollTouchGestureStaysInsideChangingContainers(int height)
	{
		var container = new Rectangle(12, 610, 378, height);
		foreach (var direction in new[] { -1, 1 })
		{
			var amount = AppiumOperations.ScrollAmount(container, new Rectangle(24, 610 + direction * height * 2, 20, 1));
			var (start, end) = AppiumOperations.ScrollGesture(container, amount);
			Assert.True(container.Contains(start));
			Assert.True(container.Contains(end));
			Assert.Equal(amount, start.Y - end.Y);
		}
	}

	[Theory]
	[InlineData(841)]
	[InlineData(590)]
	[InlineData(1500)]
	public void ScrollFindsHiddenTargetsWithRepeatedGeometryChecks(int top)
	{
		var container = new Element(new Rectangle(12, 610, 378, 207));
		var target = new Element(new Rectangle(24, top, 80, 35), displayed: false);
		Assert.Same(target, ScrollTest(container, () => target, (bounds, amount) =>
		{
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = bounds.Contains(target.Bounds);
		}));
		Assert.True(container.Bounds.Contains(target.Bounds));
	}

	[Fact]
	public void ScrollWaitsForTransientPlaceholdersBeforeGesturing()
	{
		var container = new Element(new Rectangle(13, 372, 728, 169));
		var placeholder = new Element(new Rectangle(0, 0, 0, 0), displayed: false);
		var target = new Element(new Rectangle(34, 450, 686, 32));
		var probes = 0;
		Assert.Same(target, ScrollTest(container, () => ++probes < 3 ? placeholder : target,
			(_, _) => Assert.Fail("The target should settle before scrolling.")));
		Assert.Equal(3, probes);
	}

	[Fact]
	public void ScrollRevealsUnrealizedChildrenWithFreshBounds()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var target = new Element(new Rectangle(10, 150, 20, 20), displayed: false);
		var gestures = 0;
		Assert.Same(target, ScrollTest(container, () =>
		{
			if (gestures == 0)
				throw new NoSuchElementException("Offscreen child is not exposed.");
			return target;
		}, (bounds, amount) =>
		{
			gestures++;
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = bounds.Contains(target.Bounds);
		}));
		Assert.Equal(2, gestures);
	}

	[Fact]
	public void ScrollDiscoversMissingChildrenButCapsGestures()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var gestures = 0;
		var error = Assert.Throws<InvalidOperationException>(() => AppiumOperations.ScrollIntoView(
			() => container, () => throw new NoSuchElementException("Missing."),
			(_, _) => gestures++, "missing", TimeSpan.FromSeconds(5), 2, CancellationToken.None));
		Assert.Equal(2, gestures);
		Assert.Contains("exhausted 2 scrolls", error.Message);
	}

	[Fact]
	public void ScrollRejectsNoProgressAndDoesNotRetryGestureFailures()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var target = new Element(new Rectangle(10, 110, 20, 20), displayed: false);
		Assert.Contains("no progress", Assert.Throws<InvalidOperationException>(() =>
			ScrollTest(container, () => target, (_, _) => { })).Message);
		var failure = new StaleElementReferenceException("Gesture failed.");
		Assert.Same(failure, Assert.Throws<StaleElementReferenceException>(() =>
			ScrollTest(container, () => target, (_, _) => throw failure)));
	}

	[Fact]
	public void ScrollNeverAcceptsHiddenTargetsAndHonorsCancellation()
	{
		var container = new Element(new Rectangle(0, 0, 100, 100));
		var target = new Element(new Rectangle(10, 10, 20, 20), displayed: false);
		Assert.Throws<TimeoutException>(() => AppiumOperations.ScrollIntoView(
			() => container, () => target, (_, _) => Assert.Fail("Contained target must not scroll."),
			"hidden", TimeSpan.FromMilliseconds(20), 2, CancellationToken.None));
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(() => AppiumOperations.ScrollIntoView(
			() => container, () => target, (_, _) => Assert.Fail("Cancelled."),
			"cancelled", TimeSpan.FromSeconds(5), 2, cancellation.Token));
	}

	private static IWebElement ScrollTest(Element container, Func<IWebElement> target, Action<Rectangle, int> scroll) =>
		AppiumOperations.ScrollIntoView(() => container, target, scroll, "'target' inside 'container'",
			TimeSpan.FromSeconds(5), 12, CancellationToken.None);

	[Fact]
	public void ClickRequeriesStaleElementsWithoutRetryingOtherDriverFailures()
	{
		var queries = 0;
		var clicks = 0;
		var stale = new Element(new Rectangle(0, 0, 100, 40),
			onClick: () => throw new StaleElementReferenceException("Menu replaced."));
		var current = new Element(new Rectangle(0, 0, 100, 40), onClick: () => clicks++);
		Assert.Same(current, AppiumOperations.ClickElement(
			() => ++queries == 1 ? stale : current, "CPU", TimeSpan.FromSeconds(2), CancellationToken.None));
		Assert.Equal(2, queries);
		Assert.Equal(1, clicks);
		var failure = new WebDriverException("Pointer command failed.");
		var broken = new Element(new Rectangle(0, 0, 100, 40), onClick: () => throw failure);
		Assert.Same(failure, Assert.Throws<WebDriverException>(() =>
			AppiumOperations.ClickElement(() => broken, "CPU", TimeSpan.FromSeconds(2), CancellationToken.None)));
	}

	[Fact]
	public void ClickNeverActsOnHiddenTargetsOrAfterCancellation()
	{
		var hidden = new Element(new Rectangle(0, 0, 100, 40), displayed: false,
			onClick: () => Assert.Fail("Hidden target."));
		Assert.Throws<TimeoutException>(() =>
			AppiumOperations.ClickElement(() => hidden, "CPU", TimeSpan.FromMilliseconds(20), CancellationToken.None));
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(() => AppiumOperations.ClickElement(
			() => throw new InvalidOperationException("Cancelled clicks must not query."),
			"CPU", TimeSpan.FromSeconds(2), cancellation.Token));
	}

	private sealed class Element(Rectangle bounds, bool displayed = true, Action? onClick = null) : IWebElement
	{
		internal Rectangle Bounds { get; set; } = bounds;
		public bool Displayed { get; set; } = displayed;
		public Point Location => Bounds.Location;
		public Size Size => Bounds.Size;
		public string TagName => throw new NotSupportedException();
		public string Text => throw new NotSupportedException();
		public bool Enabled => throw new NotSupportedException();
		public bool Selected => throw new NotSupportedException();
		public void Clear() => throw new NotSupportedException();
		public void Click() => (onClick ?? throw new NotSupportedException()).Invoke();
		public void SendKeys(string text) => throw new NotSupportedException();
		public void Submit() => throw new NotSupportedException();
		public string GetAttribute(string name) => throw new NotSupportedException();
		public string GetDomAttribute(string name) => throw new NotSupportedException();
		public string GetDomProperty(string name) => throw new NotSupportedException();
		public string GetCssValue(string name) => throw new NotSupportedException();
		public ISearchContext GetShadowRoot() => throw new NotSupportedException();
		public IWebElement FindElement(By by) => throw new NotSupportedException();
		public ReadOnlyCollection<IWebElement> FindElements(By by) => throw new NotSupportedException();
	}

	public void Dispose()
	{
		if (Directory.Exists(root))
			SampleWorkspace.DeleteDirectory(root);
	}
}

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumImageInfrastructureTests
{
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public Task StaticCanvasWaitsForTheReviewedFrame(bool resize) =>
		WithImages(async (actual, expected) =>
		{
			if (resize)
				WriteImage(actual, SKColors.Red, 8);
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				if (++captures == 2)
					File.Copy(expected, actual, overwrite: true);
				return actual;
			}, expected);
			Assert.Equal(2, captures);
			Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
		});

	[Fact]
	public Task MatchingCanvasNeedsOnlyOneCapture() =>
		WithImages(async (actual, expected) =>
		{
			File.Copy(expected, actual, overwrite: true);
			var captures = 0;
			await SampleImage.Validate(() => { captures++; return actual; }, expected);
			Assert.Equal(1, captures);
		});

	[Fact]
	public Task StaticCanvasReportsTheFinalMismatchWithStrictLimits() =>
		WithImages(async (actual, expected) =>
		{
			using (var bitmap = new SKBitmap(16, 16))
			{
				bitmap.Erase(SKColors.White);
				bitmap.SetPixel(0, 0, new SKColor(254, 255, 255));
				using var image = SKImage.FromBitmap(bitmap);
				using var data = image.Encode(SKEncodedImageFormat.Png, 100);
				File.WriteAllBytes(actual, data.ToArray());
			}
			var captures = 0;
			var failure = await Assert.ThrowsAsync<Xunit.Sdk.FailException>(() =>
				SampleImage.Validate(() => { captures++; return actual; }, expected, TimeSpan.Zero));
			Assert.Contains("Sample pixels differ", failure.Message);
			Assert.Equal(1, captures);
			Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
			Assert.Equal(0.00075m, SampleImageOptions.DefaultMaxErrorPixelFraction);
		});

	[Fact]
	public Task MissingGoldenAndCaptureErrorsAreNotRetriedOrAccepted() =>
		WithImages(async (actual, expected) =>
		{
			File.Delete(expected);
			var captures = 0;
			var failure = await Assert.ThrowsAnyAsync<Exception>(() =>
				SampleImage.Validate(() => { captures++; return actual; }, expected));
			Assert.Contains("Missing reviewed sample golden", failure.Message);
			Assert.Equal(1, captures);
			Assert.False(File.Exists(expected));
			var captureFailure = new InvalidOperationException("Capture failed.");
			Assert.Same(captureFailure, await Assert.ThrowsAsync<InvalidOperationException>(() =>
				SampleImage.Validate(() => throw captureFailure, expected)));
		});

	private static async Task WithImages(Func<string, string, Task> test)
	{
		var directory = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("appium-golden"));
		Directory.CreateDirectory(directory);
		try
		{
			var actual = Path.Combine(directory, "actual.png");
			var expected = Path.Combine(directory, "expected.png");
			WriteImage(actual, SKColors.Red);
			WriteImage(expected, SKColors.White);
			await test(actual, expected);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(directory);
		}
	}

	private static void WriteImage(string path, SKColor color, int size = 16)
	{
		using var bitmap = new SKBitmap(size, size);
		bitmap.Erase(color);
		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Png, 100);
		File.WriteAllBytes(path, data.ToArray());
	}
}
