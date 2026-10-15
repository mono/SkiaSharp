using System.Collections.ObjectModel;
using System.Drawing;
using OpenQA.Selenium;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests
{
	[Fact]
	public void CapturesOfTheSameSceneKeepBothFiles()
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-captures"));
		try
		{
			var inputs = Path.Combine(root, "inputs");
			Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
			using var workspace = new SampleWorkspace("Sample", inputs, Repo.PackagesDir,
				Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
			var app = new MacRunningApp(workspace, Path.Combine(workspace.Root, "Sample.app"));

			var before = app.SaveCapture([1, 2], "drawing");
			var after = app.SaveCapture([3, 4], "drawing");

			Assert.NotEqual(before, after);
			Assert.Equal(new byte[] { 1, 2 }, File.ReadAllBytes(before));
			Assert.Equal(new byte[] { 3, 4 }, File.ReadAllBytes(after));
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	[Theory]
	[InlineData("<manifest package=\"com.skiasharp.sample\"><application name=\"OtherClass\" /></manifest>")]
	[InlineData("<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" package=\"com.skiasharp.sample\" />")]
	public void AndroidPackageIdComesFromDecodedApkManifest(string manifest)
	{
		Assert.Equal("com.skiasharp.sample", AndroidRunningApp.PackageId(manifest));
	}

	[Theory]
	[InlineData("<manifest><application name=\"com.skiasharp.sample\" /></manifest>")]
	[InlineData("<manifest package=\"\" />")]
	[InlineData("<manifest package=\"com.skiasharp. sample\" />")]
	public void AndroidPackageIdRejectsMissingOrWrappedValues(string manifest)
	{
		Assert.Throws<InvalidOperationException>(() => AndroidRunningApp.PackageId(manifest));
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public async Task AppleAppIdentityReadsXmlAndBinaryBundlePlists(bool macOS, bool binary)
	{
		Assert.SkipUnless(OperatingSystem.IsMacOS(), "Apple bundle metadata requires macOS plutil.");
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-bundle"));
		try
		{
			var inputs = Path.Combine(root, "inputs");
			Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
			using var workspace = new SampleWorkspace("Sample", inputs, Repo.PackagesDir,
				Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
			var bundle = Path.Combine(workspace.Root, "Built.app");
			var directory = macOS ? Path.Combine(bundle, "Contents") : bundle;
			Directory.CreateDirectory(directory);
			var plist = Path.Combine(directory, "Info.plist");
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
			AppiumRunningApp app = macOS ? new MacRunningApp(workspace, bundle) : new IosRunningApp(workspace, bundle);
			Assert.Equal("com.skiasharp.built", await app.ReadApplicationId());
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	[Fact]
	public void AndroidReferenceProfileUsesTheConfiguredModel()
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-model"));
		try
		{
			var inputs = Path.Combine(root, "inputs");
			Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
			using var workspace = new SampleWorkspace(
				"Sample", inputs, Repo.PackagesDir,
				Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
			var app = new AndroidRunningApp(
				workspace, Path.Combine(workspace.Root, "sample.apk"));

			var model = Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_MODEL") ?? "pixel_9";
			var version = Environment.GetEnvironmentVariable("SAMPLE_TEST_ANDROID_VERSION") ?? "17";
			Assert.Equal($"{model}/{version}", app.DeviceProfile);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	[Fact]
	public async Task WindowsAppUsesItsExecutablePathAndBuiltArchitecture()
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-windows"));
		try
		{
			var inputs = Path.Combine(root, "inputs");
			Directory.CreateDirectory(Path.Combine(inputs, "Sample"));
			using var workspace = new SampleWorkspace(
				"Sample", inputs, Repo.PackagesDir,
				Path.Combine(root, "workspaces"), Path.Combine(root, "diagnostics"));
			var executable = Path.Combine(workspace.Root, "samples", "Sample", "bin", "Sample.exe");
			var app = new WindowsRunningApp(workspace, executable, "x64");
			Assert.Equal(Path.GetFullPath(executable), app.Artifact);
			Assert.Equal(app.Artifact, await app.ReadApplicationId());
			Assert.Equal("x64", app.DeviceProfile);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
		}
	}

	[Fact]
	public async Task MissingInstalledAndroidImageFailsBeforeDeviceCreation()
	{
		var sdk = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-sdk"));
		try
		{
			Directory.CreateDirectory(sdk);
			var android = new Android(sdk);
			var error = await Assert.ThrowsAsync<DirectoryNotFoundException>(() =>
				android.CreateAvd("SkiaSharpSamples_missing", "android-37.1;google_apis_ps16k", "pixel_9"));
			Assert.Contains("Tests never download images", error.Message);
		}
		finally
		{
			if (Directory.Exists(sdk))
				SampleWorkspace.DeleteDirectory(sdk);
		}
	}

	[Theory]
	[InlineData(610, 207, 819, 18, 114)]
	[InlineData(610, 207, 841, 35, 124)]
	[InlineData(610, 207, 793, 35, 97)]
	[InlineData(610, 207, 590, 18, -114)]
	[InlineData(610, 207, 610, 18, 0)]
	[InlineData(610, 207, 799, 18, 0)]
	[InlineData(20, 100, 500, 20, 60)]
	[InlineData(20, 100, -500, 20, -60)]
	public void ScrollAmountComesFromCurrentBounds(int top, int height, int targetTop, int targetHeight, int expected) =>
		Assert.Equal(expected, AppiumRunningApp.ScrollAmount(
			new Rectangle(12, top, 378, height), new Rectangle(24, targetTop, 80, targetHeight)));

	[Theory]
	[InlineData(0, 100, 20, 20, "positive dimensions")]
	[InlineData(100, 0, 20, 20, "positive dimensions")]
	[InlineData(100, 100, 0, 20, "positive dimensions")]
	[InlineData(100, 100, 20, 0, "positive dimensions")]
	[InlineData(100, 100, 101, 20, "larger")]
	[InlineData(100, 100, 20, 101, "larger")]
	public void ScrollRejectsImpossibleDimensions(int width, int height, int targetWidth, int targetHeight, string message)
	{
		var error = Assert.Throws<InvalidOperationException>(() => AppiumRunningApp.ScrollAmount(
			new Rectangle(10, 20, width, height), new Rectangle(10, 20, targetWidth, targetHeight)));
		Assert.Contains(message, error.Message);
		Assert.Contains("container=", error.Message);
		Assert.Contains("target=", error.Message);
	}

	[Theory]
	[InlineData(9)]
	[InlineData(91)]
	public void ScrollRejectsHorizontalClipping(int targetLeft)
	{
		var error = Assert.Throws<InvalidOperationException>(() => AppiumRunningApp.ScrollAmount(
			new Rectangle(10, 20, 100, 100), new Rectangle(targetLeft, 30, 20, 20)));
		Assert.Contains("horizontally", error.Message);
	}

	[Theory]
	[InlineData(3)]
	[InlineData(5)]
	[InlineData(100)]
	[InlineData(207)]
	[InlineData(901)]
	public void ScrollTouchGestureStaysInsideChangingContainers(int height)
	{
		var container = new Rectangle(12, 610, 378, height);
		foreach (var direction in new[] { -1, 1 })
		{
			var amount = AppiumRunningApp.ScrollAmount(container, new Rectangle(24, 610 + direction * height * 2, 20, 1));
			var (start, end) = AppiumRunningApp.ScrollGesture(container, amount);
			Assert.True(container.Contains(start));
			Assert.True(container.Contains(end));
			Assert.Equal(amount, start.Y - end.Y);
			Assert.Equal(start.X, end.X);
		}
	}

	[Fact]
	public void ScrollAlreadyVisibleRequiresNoGesture()
	{
		var container = new ScrollElement(new Rectangle(12, 610, 378, 207));
		var target = new ScrollElement(new Rectangle(24, 700, 80, 18));
		Assert.Same(target, ScrollTest(container, () => target, (_, _) => Assert.Fail("Visible targets must not scroll.")));
	}

	[Theory]
	[InlineData(841, 124)]
	[InlineData(590, -106)]
	[InlineData(1500, 744)]
	public void ScrollFindsHiddenTargetsAboveAndBelowWithRepeatedGeometryChecks(int top, int distance)
	{
		var container = new ScrollElement(new Rectangle(12, 610, 378, 207));
		var target = new ScrollElement(new Rectangle(24, top, 80, 35), displayed: false);
		var amounts = new List<int>();
		Assert.Same(target, ScrollTest(container, () => target, (bounds, amount) =>
		{
			Assert.Equal(container.Bounds, bounds);
			Assert.Equal(Math.Sign(distance), Math.Sign(amount));
			amounts.Add(amount);
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = bounds.Contains(target.Bounds);
		}));
		Assert.Equal(distance, amounts.Sum());
		Assert.Equal(Math.Abs(distance) > 124 ? 6 : 1, amounts.Count);
		Assert.True(container.Bounds.Contains(target.Bounds));
	}

	[Fact]
	public void ScrollRetriesMissingAndStaleReadsWithoutExtraGestures()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 110, 20, 20), displayed: false);
		var probes = 0;
		var gestures = 0;
		Assert.Same(target, ScrollTest(container, () => ++probes switch
		{
			1 => throw new NoSuchElementException("Target is not present yet."),
			2 => throw new StaleElementReferenceException("Native tree refreshed."),
			4 => throw new StaleElementReferenceException("Native tree refreshed after scrolling."),
			_ => target
		}, (_, amount) =>
		{
			gestures++;
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = true;
		}));
		Assert.Equal(1, gestures);
		Assert.True(probes >= 5);
	}

	[Fact]
	public void ScrollDiscoversChildrenNotExposedUntilTheViewportMoves()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 150, 20, 20), displayed: false);
		var gestures = 0;
		Assert.Same(target, ScrollTest(container, () =>
		{
			if (gestures == 0)
				throw new NoSuchElementException("Offscreen child is not in the native tree.");
			return target;
		}, (bounds, amount) =>
		{
			gestures++;
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			target.Displayed = bounds.Contains(target.Bounds);
		}));
		Assert.Equal(2, gestures);
		Assert.True(container.Bounds.Contains(target.Bounds));
	}

	[Fact]
	public void ScrollRecomputesAmountsWhenTheContainerResizes()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 500, 20, 20), displayed: false);
		var amounts = new List<int>();
		Assert.Same(target, ScrollTest(container, () => target, (_, amount) =>
		{
			amounts.Add(amount);
			target.Bounds = target.Bounds with { Y = target.Bounds.Y - amount };
			container.Bounds = container.Bounds with { Height = 200 };
			target.Displayed = container.Bounds.Contains(target.Bounds);
		}));
		Assert.Equal(new[] { 60, 120, 120, 110 }, amounts);
	}

	[Fact]
	public void ScrollWaitsForDisplayedEvenWhenBoundsAlreadyFit()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 10, 20, 20), displayed: false);
		var probes = 0;
		Assert.Same(target, ScrollTest(container, () =>
		{
			target.Displayed = ++probes > 1;
			return target;
		}, (_, _) => Assert.Fail("A contained target only needs to become displayed.")));
		Assert.Equal(2, probes);
	}

	[Fact]
	public void ScrollStopsActionablyWhenGeometryDoesNotAdvance()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 110, 20, 20), displayed: false);
		var gestures = 0;
		var error = Assert.Throws<InvalidOperationException>(() => ScrollTest(container, () => target, (_, _) => gestures++));
		Assert.Equal(1, gestures);
		Assert.Contains("no progress", error.Message);
		Assert.Contains("'target' inside 'container'", error.Message);
		Assert.Contains("container=", error.Message);
		Assert.Contains("Displayed=False", error.Message);
	}

	[Fact]
	public void ScrollStopsWhenDriverMovesInTheWrongDirection()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 110, 20, 20), displayed: false);
		var error = Assert.Throws<InvalidOperationException>(() => ScrollTest(container, () => target,
			(_, _) => target.Bounds = target.Bounds with { Y = target.Bounds.Y + 10 }));
		Assert.Contains("did not advance", error.Message);
	}

	[Fact]
	public void ScrollCapsGesturesEvenWhenTheTargetKeepsMoving()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 1000, 20, 20), displayed: false);
		var gestures = 0;
		var error = Assert.Throws<InvalidOperationException>(() => AppiumRunningApp.ScrollIntoView(
			() => container, () => target, (_, _) =>
			{
				gestures++;
				target.Bounds = target.Bounds with { Y = target.Bounds.Y - 1 };
			}, "'target' inside 'container'", TimeSpan.FromSeconds(5), 2, CancellationToken.None));
		Assert.Equal(2, gestures);
		Assert.Contains("exhausted 2 scrolls", error.Message);
	}

	[Fact]
	public void ScrollNeverAcceptsHiddenTargets()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 10, 20, 20), displayed: false);
		var error = Assert.Throws<TimeoutException>(() => AppiumRunningApp.ScrollIntoView(
			() => container, () => target,
			(_, _) => Assert.Fail("There is no geometry-directed scroll to perform."),
			"'target' inside 'container'", TimeSpan.FromMilliseconds(100), 2, CancellationToken.None));
		Assert.Contains("'target' inside 'container'", error.Message);
		Assert.Contains("Displayed=False", error.Message);
	}

	[Fact]
	public void ScrollBoundsDiscoveryForMissingTargets()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var gestures = 0;
		var error = Assert.Throws<InvalidOperationException>(() => AppiumRunningApp.ScrollIntoView(
			() => container, () => throw new NoSuchElementException("Target is missing."),
			(_, amount) =>
			{
				Assert.Equal(60, amount);
				gestures++;
			},
			"'target' inside 'container'", TimeSpan.FromSeconds(30), 2, CancellationToken.None));
		Assert.Equal(2, gestures);
		Assert.Contains("exhausted 2 scrolls", error.Message);
		Assert.Contains("target not exposed", error.Message);
	}

	[Fact]
	public void ScrollHonorsCancellationBeforeAnyGesture()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 110, 20, 20), displayed: false);
		Assert.Throws<OperationCanceledException>(() => AppiumRunningApp.ScrollIntoView(
			() => container, () => target, (_, _) => Assert.Fail("Cancelled scrolling must not gesture."),
			"'target' inside 'container'", TimeSpan.FromSeconds(5), 2, cancellation.Token));
	}

	[Fact]
	public void ScrollHonorsCancellationAfterTheFirstGesture()
	{
		using var cancellation = new CancellationTokenSource();
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 1000, 20, 20), displayed: false);
		var gestures = 0;
		Assert.Throws<OperationCanceledException>(() => AppiumRunningApp.ScrollIntoView(
			() => container, () => target, (_, _) =>
			{
				gestures++;
				cancellation.Cancel();
			}, "'target' inside 'container'", TimeSpan.FromSeconds(5), 12, cancellation.Token));
		Assert.Equal(1, gestures);
	}

	[Fact]
	public void ScrollDoesNotRetryGestureFailuresAsStaleReads()
	{
		var container = new ScrollElement(new Rectangle(0, 0, 100, 100));
		var target = new ScrollElement(new Rectangle(10, 110, 20, 20), displayed: false);
		var failure = new StaleElementReferenceException("Gesture failed.");
		var gestures = 0;
		Assert.Same(failure, Assert.Throws<StaleElementReferenceException>(() => ScrollTest(container, () => target, (_, _) =>
		{
			gestures++;
			throw failure;
		})));
		Assert.Equal(1, gestures);
	}

	private static IWebElement ScrollTest(ScrollElement container, Func<IWebElement> target, Action<Rectangle, int> scroll) =>
		AppiumRunningApp.ScrollIntoView(() => container, target, scroll, "'target' inside 'container'",
			TimeSpan.FromSeconds(5), 12, CancellationToken.None);

	[Fact]
	public void ClickRequeriesAnElementReplacedDuringNavigation()
	{
		var queries = 0;
		var clicks = 0;
		var stale = new ScrollElement(new Rectangle(0, 0, 100, 40),
			onClick: () => throw new StaleElementReferenceException("Menu snapshot replaced."));
		var current = new ScrollElement(new Rectangle(0, 0, 100, 40), onClick: () => clicks++);
		Assert.Same(current, AppiumRunningApp.ClickElement(
			() => ++queries == 1 ? stale : current, "CPU menu item", TimeSpan.FromSeconds(2), CancellationToken.None));
		Assert.Equal(2, queries);
		Assert.Equal(1, clicks);
	}

	[Fact]
	public void ClickWaitsForTheTargetToAppear()
	{
		var queries = 0;
		var clicks = 0;
		var target = new ScrollElement(new Rectangle(0, 0, 100, 40), onClick: () => clicks++);
		Assert.Same(target, AppiumRunningApp.ClickElement(
			() => ++queries == 1 ? throw new NoSuchElementException("Menu opening.") : target,
			"CPU menu item", TimeSpan.FromSeconds(2), CancellationToken.None));
		Assert.Equal(2, queries);
		Assert.Equal(1, clicks);
	}

	[Fact]
	public void ClickDoesNotRetryOtherDriverFailures()
	{
		var queries = 0;
		var failure = new WebDriverException("Pointer command failed.");
		var target = new ScrollElement(new Rectangle(0, 0, 100, 40), onClick: () => throw failure);
		Assert.Same(failure, Assert.Throws<WebDriverException>(() =>
			AppiumRunningApp.ClickElement(() =>
			{
				queries++;
				return target;
			}, "CPU menu item", TimeSpan.FromSeconds(2), CancellationToken.None)));
		Assert.Equal(1, queries);
	}

	[Fact]
	public void ClickHonorsCancellationBeforeFindingTheTarget()
	{
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		Assert.Throws<OperationCanceledException>(() => AppiumRunningApp.ClickElement(
			() => throw new InvalidOperationException("Cancelled clicks must not query the UI."),
			"CPU menu item", TimeSpan.FromSeconds(2), cancellation.Token));
	}

	[Fact]
	public void ClickNeverActsOnAHiddenTarget()
	{
		var target = new ScrollElement(new Rectangle(0, 0, 100, 40), displayed: false,
			onClick: () => Assert.Fail("Hidden elements must not be clicked."));
		Assert.Throws<TimeoutException>(() => AppiumRunningApp.ClickElement(
			() => target, "CPU menu item", TimeSpan.FromMilliseconds(20), CancellationToken.None));
	}

	private sealed class ScrollElement(Rectangle bounds, bool displayed = true, Action? onClick = null) : IWebElement
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

	[Fact]
	public Task GoldenRetryStopsAtTheExpectedFrame() =>
		WithImages(async (actual, expected) =>
		{
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				captures++;
				if (captures == 2)
				{
					Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
					File.Copy(expected, actual, overwrite: true);
				}
				return actual;
			}, expected, TimeSpan.FromSeconds(30));
			Assert.Equal(2, captures);
			Assert.False(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
		});

	[Fact]
	public Task MatchingGoldenNeedsOnlyOneCapture() =>
		WithImages(async (actual, expected) =>
		{
			File.Copy(expected, actual, overwrite: true);
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				captures++;
				return actual;
			}, expected, TimeSpan.FromSeconds(2));
			Assert.Equal(1, captures);
		});

	[Fact]
	public Task AsyncCaptureUsesTheSameGoldenRetry() =>
		WithImages(async (actual, expected) =>
		{
			var captures = 0;
			await SampleImage.Validate(async () =>
			{
				await Task.Yield();
				if (++captures == 2)
					File.Copy(expected, actual, overwrite: true);
				return actual;
			}, expected, TimeSpan.FromSeconds(30));
			Assert.Equal(2, captures);
		});

	[Fact]
	public Task GoldenRetryReportsTheFinalMismatch() =>
		WithImages(async (actual, expected) =>
		{
			var captures = 0;
			var error = await Assert.ThrowsAsync<Xunit.Sdk.FailException>(() =>
				SampleImage.Validate(() =>
				{
					captures++;
					return actual;
				}, expected, TimeSpan.Zero));
			Assert.Contains("Sample pixels differ", error.Message);
			Assert.Equal(1, captures);
			Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
		});

	[Fact]
	public Task MissingGoldenIsNotRetriedOrAccepted() =>
		WithImages(async (actual, expected) =>
		{
			File.Delete(expected);
			var captures = 0;
			var error = await Assert.ThrowsAnyAsync<Exception>(() =>
				SampleImage.Validate(() =>
				{
					captures++;
					return actual;
				}, expected, TimeSpan.FromSeconds(2)));
			Assert.Contains("Missing reviewed sample golden", error.Message);
			Assert.Equal(1, captures);
			Assert.True(File.Exists(actual));
			Assert.False(File.Exists(expected));
		});

	[Fact]
	public Task GoldenRetryWaitsForTheExpectedDimensions() =>
		WithImages(async (actual, expected) =>
		{
			WriteImage(actual, SKColors.White, 8);
			var captures = 0;
			await SampleImage.Validate(() =>
			{
				captures++;
				if (captures == 2)
					File.Copy(expected, actual, overwrite: true);
				return actual;
			}, expected, TimeSpan.FromSeconds(2));
			Assert.Equal(2, captures);
		});

	[Fact]
	public Task CaptureFailureIsNotRetried() =>
		WithImages(async (_, expected) =>
		{
			var captures = 0;
			var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
				SampleImage.Validate((Func<string>)(() =>
				{
					captures++;
					throw new InvalidOperationException("Capture failed.");
				}), expected, TimeSpan.FromSeconds(2)));
			Assert.Equal("Capture failed.", error.Message);
			Assert.Equal(1, captures);
		});

	[Fact]
	public Task ReferenceCaptureModeRecordsTheDestinationWithoutWritingIt() =>
		WithImages(async (actual, expected) =>
		{
			File.Delete(expected);
			var enabled = AppContext.TryGetSwitch("SampleTest.CaptureReferences", out var captureReferences) && captureReferences;
			if (!enabled)
			{
				await Assert.ThrowsAnyAsync<Exception>(() =>
					SampleImage.Validate(() => actual, expected, TimeSpan.Zero));
				Assert.False(File.Exists(Path.ChangeExtension(actual, ".reference.json")));
			}
			else
			{
				await SampleImage.Validate(() => actual, expected, TimeSpan.Zero);
				using var record = System.Text.Json.JsonDocument.Parse(
					File.ReadAllText(Path.ChangeExtension(actual, ".reference.json")));
				Assert.Equal(actual, record.RootElement.GetProperty("actual").GetString());
				Assert.Equal(expected, record.RootElement.GetProperty("expected").GetString());
			}
			Assert.False(File.Exists(expected));
		});

	private static async Task WithImages(Func<string, string, Task> test)
	{
		var root = Path.Combine(Path.GetTempPath(), SampleWorkspace.CreateIdentity("appium-golden"));
		Directory.CreateDirectory(root);
		try
		{
			var actual = Path.Combine(root, "actual.png");
			var expected = Path.Combine(root, "expected.png");
			WriteImage(actual, SKColors.Red);
			WriteImage(expected, SKColors.White);
			await test(actual, expected);
		}
		finally
		{
			SampleWorkspace.DeleteDirectory(root);
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
