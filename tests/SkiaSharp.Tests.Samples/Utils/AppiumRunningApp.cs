using System.Diagnostics;
using System.Drawing;
using System.Runtime.ExceptionServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Owns one app session; simulator ownership belongs to the collection fixture.
internal abstract class AppiumRunningApp
{
	private readonly List<Func<Task>> cleanup = [];
	private AppiumDriver? driver;
	private string? applicationId;
	private bool used;
	private int captureIndex;

	protected AppiumRunningApp(SampleWorkspace workspace, string artifact)
	{
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		ArgumentException.ThrowIfNullOrWhiteSpace(artifact);
		Artifact = Path.GetFullPath(artifact);
		Diagnostics = Path.Combine(workspace.DiagnosticsRoot, "appium");
	}

	internal SampleWorkspace Workspace { get; }
	internal string Artifact { get; }
	internal string ApplicationId => applicationId ?? throw new InvalidOperationException("The built application's identity has not been read.");
	internal string Diagnostics { get; }
	internal abstract string DeviceProfile { get; }
	internal AppiumDriver Driver => driver ?? throw new InvalidOperationException("The owned Appium session is not ready.");
	protected bool HasDriver => driver is not null;
	private static By Locator(string id) => MobileBy.AccessibilityId(id);
	internal void SubmitSearch() =>
		Find(MobileBy.IosNSPredicate("type == 'XCUIElementTypeButton' AND name == 'Return'")).Click();

	protected abstract Task Start(Uri address);
	internal abstract Task<string> ReadApplicationId();

	protected async Task<string> ReadAppleApplicationId()
	{
		var plist = Path.Combine(Artifact, "Info.plist");
		var id = (await Command("plutil", ["-extract", "CFBundleIdentifier", "raw", "-o", "-", plist])).Trim();
		if (string.IsNullOrWhiteSpace(id) || id.Any(char.IsWhiteSpace))
			throw new InvalidOperationException($"Built app bundle has an invalid CFBundleIdentifier: {plist}");
		return id;
	}
	protected virtual Task CapturePlatformDiagnostics() => Task.CompletedTask;
	protected void Own(Func<Task> release) => cleanup.Add(release);

	protected void SetDriver(AppiumDriver value)
	{
		driver = value;
		driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
	}

	internal IWebElement Find(By selector) =>
		Wait(() => Driver.FindElement(selector) is { Displayed: true } element ? element : null, selector.ToString());

	internal IWebElement Find(string id, string alternative) =>
		Wait(() => FindOptional(id) ?? FindOptional(alternative), $"'{id}' or '{alternative}'");

	internal IWebElement? FindOptional(string id) =>
		Driver.FindElements(Locator(id)).FirstOrDefault(element => element.Displayed);

	internal IWebElement Find(string id) => Find(Locator(id));
	internal void Click(string id) => Click(Locator(id));
	internal void Click(By selector) =>
		ClickElement(() => Driver.FindElement(selector), selector.ToString(),
			TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

	internal static IWebElement ClickElement(
		Func<IWebElement> find, string description, TimeSpan limit, CancellationToken cancellationToken) =>
		Wait(() =>
		{
			var element = find();
			if (!element.Displayed)
				return null;
			element.Click();
			return element;
		}, description, limit, cancellationToken);

	internal void WaitText(string id, Func<string, bool> predicate) =>
		Wait(() =>
		{
			var element = Driver.FindElement(Locator(id));
			return element.Displayed && predicate(element.Text) ? element : null;
		}, $"text of {id}");

	internal void Drag(IWebElement element, double fromX, double fromY, double toX, double toY)
	{
		var location = element.Location;
		var size = element.Size;
		Drag(new Point(location.X + (int)(size.Width * fromX), location.Y + (int)(size.Height * fromY)),
			new Point(location.X + (int)(size.Width * toX), location.Y + (int)(size.Height * toY)));
	}

	private void Drag(Point start, Point end)
	{
		var pointer = new PointerInputDevice(PointerKind.Touch, "sample-pointer");
		var button = MouseButton.Touch;
		var sequence = new ActionSequence(pointer);
		sequence.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, start.X, start.Y, TimeSpan.Zero));
		sequence.AddAction(pointer.CreatePointerDown(button));
		sequence.AddAction(pointer.CreatePointerMove(CoordinateOrigin.Viewport, end.X, end.Y, TimeSpan.FromMilliseconds(500)));
		sequence.AddAction(pointer.CreatePointerUp(button));
		List<Exception> failures = [];
		try
		{ Driver.PerformActions([sequence]); }
		catch (Exception error) { failures.Add(error); }
		try
		{ Driver.ResetInputState(); }
		catch (Exception error) { failures.Add(error); }
		ThrowFailures(failures);
	}

	internal IWebElement ScrollIntoView(string containerId, string targetId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(containerId);
		ArgumentException.ThrowIfNullOrWhiteSpace(targetId);
		return ScrollIntoView(
			() => Driver.FindElement(Locator(containerId)),
			() => Driver.FindElement(Locator(targetId)), Scroll,
			$"'{targetId}' inside '{containerId}'", TimeSpan.FromSeconds(30), 12,
			TestContext.Current.CancellationToken);
	}

	// Positive amounts reveal content below the viewport; touch moves in the opposite direction.
	private void Scroll(Rectangle container, int amount)
	{
		var (start, end) = ScrollGesture(container, amount);
		Drag(start, end);
	}

	internal static (Point Start, Point End) ScrollGesture(Rectangle container, int amount)
	{
		if (container.Width <= 0 || container.Height < 3)
			throw new InvalidOperationException($"Scroll container has no usable gesture area: {container}.");
		var margin = Math.Max(1, container.Height / 5);
		if (amount == 0 || Math.Abs((long)amount) > container.Height - 2 * margin)
			throw new ArgumentOutOfRangeException(nameof(amount), amount, $"Gesture must remain inside {container}.");
		var x = container.Left + container.Width / 2;
		var start = new Point(x, amount > 0 ? container.Bottom - margin - 1 : container.Top + margin);
		return (start, new Point(x, start.Y - amount));
	}

	internal static int ScrollAmount(Rectangle container, Rectangle target)
	{
		if (container.Width <= 0 || container.Height <= 0 || target.Width <= 0 || target.Height <= 0)
			throw new InvalidOperationException($"Scroll bounds must have positive dimensions: container={container}, target={target}.");
		if (target.Width > container.Width || target.Height > container.Height)
			throw new InvalidOperationException($"Target is larger than its scroll container: container={container}, target={target}.");
		if (target.Left < container.Left || target.Right > container.Right)
			throw new InvalidOperationException($"Vertical scrolling cannot reveal the target horizontally: container={container}, target={target}.");
		// Aim inside the viewport; tiny edge corrections may not exceed native touch slop.
		var centerPadding = (container.Height - target.Height) / 2;
		var amount = target.Top < container.Top ? target.Top - container.Top - centerPadding :
			target.Bottom > container.Bottom ? target.Bottom - container.Bottom + centerPadding : 0;
		return Math.Clamp(amount, -Math.Max(1, container.Height * 3 / 5), Math.Max(1, container.Height * 3 / 5));
	}

	internal static IWebElement ScrollIntoView(
		Func<IWebElement> findContainer, Func<IWebElement> findTarget, Action<Rectangle, int> scroll,
		string description, TimeSpan timeout, int maxScrolls, CancellationToken cancellationToken)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(maxScrolls, 1);
		if (timeout <= TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(timeout));
		var clock = Stopwatch.StartNew();
		var gestures = 0;
		(Rectangle Container, Rectangle Target, int Amount)? previous = null;
		(Rectangle Container, Rectangle Target)? observed = null;
		var changedAt = TimeSpan.Zero;
		var missingTargetProbes = 0;
		var last = "container/target not yet present";
		try
		{
			while (true)
			{
				Rectangle bounds = default, targetBounds = default;
				var amount = 0;
				var element = Wait(() =>
				{
					var container = findContainer();
					bounds = new Rectangle(container.Location, container.Size);
					if (!container.Displayed)
						return null;
					IWebElement? target = null;
					var displayed = false;
					try
					{
						target = findTarget();
					}
					catch (NoSuchElementException) { }
					if (target is not null)
					{
						targetBounds = new Rectangle(target.Location, target.Size);
						displayed = target.Displayed;
						last = $"container={bounds}, target={targetBounds}, Displayed={displayed}, gestures={gestures}/{maxScrolls}";
					}
					else
						last = $"container={bounds}, target not exposed, gestures={gestures}/{maxScrolls}";
					if (target is null || (!displayed && targetBounds.Width == 0 && targetBounds.Height == 0))
					{
						if (++missingTargetProbes < 3)
							return null;
						missingTargetProbes = 0;
						if (bounds.Width <= 0 || bounds.Height < 3)
							throw new InvalidOperationException($"Scroll container has no usable bounds: {bounds}.");
						// Unrealized children can be absent or exposed as hidden zero-size UIA placeholders.
						amount = Math.Max(1, bounds.Height * 3 / 5);
						previous = null;
						return container;
					}
					missingTargetProbes = 0;
					amount = ScrollAmount(bounds, targetBounds);
					if (amount == 0 && displayed)
						return target;

					if (previous is { } before)
					{
						var current = (bounds, targetBounds);
						if (observed != current)
						{
							observed = current;
							changedAt = clock.Elapsed;
							return null;
						}
						// Allow native scrolling/animation to settle before comparing and scrolling again.
						if (clock.Elapsed - changedAt < TimeSpan.FromMilliseconds(200))
							return null;
						if (bounds == before.Container && targetBounds == before.Target)
						{
							if (clock.Elapsed - changedAt < TimeSpan.FromSeconds(1))
								return null;
							throw new InvalidOperationException($"Appium scroll made no progress for {description}: {last}.");
						}
						if (bounds.Size == before.Container.Size && targetBounds.Size == before.Target.Size &&
							Math.Sign((long)before.Target.Top - before.Container.Top - targetBounds.Top + bounds.Top) != Math.Sign(before.Amount))
							throw new InvalidOperationException($"Appium scroll did not advance toward {description}: before={before}; {last}.");
						previous = null;
					}
					if (amount == 0)
						return null; // Bounds alone are not proof that a native target is displayed.
					return target;
				}, description, timeout - clock.Elapsed, cancellationToken);
				cancellationToken.ThrowIfCancellationRequested();
				if (amount == 0)
					return element;
				if (gestures == maxScrolls)
					throw new InvalidOperationException($"Appium exhausted {maxScrolls} scrolls for {description}: {last}.");
				if (clock.Elapsed >= timeout)
					throw new TimeoutException("Scroll deadline elapsed before the next gesture.");
				// Gestures are outside Wait: only stale/missing reads may be retried.
				scroll(bounds, amount);
				gestures++;
				previous = (bounds, targetBounds, amount);
				observed = (bounds, targetBounds);
				changedAt = clock.Elapsed;
			}
		}
		catch (TimeoutException error)
		{
			throw new TimeoutException($"Appium could not scroll {description} fully into view within {timeout}: {last}.", error);
		}
	}

	internal T Wait<T>(Func<T?> probe, string description) where T : class =>
		Wait(probe, description, TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);

	internal static T Wait<T>(Func<T?> probe, string description, TimeSpan limit, CancellationToken cancellationToken) where T : class
	{
		var clock = Stopwatch.StartNew();
		do
		{
			cancellationToken.ThrowIfCancellationRequested();
			try
			{
				if (probe() is { } result)
					return result;
			}
			catch (NoSuchElementException) { }
			catch (StaleElementReferenceException) { }
			var remaining = limit - clock.Elapsed;
			if (remaining <= TimeSpan.Zero)
				break;
			if (cancellationToken.WaitHandle.WaitOne(remaining < TimeSpan.FromMilliseconds(200) ? remaining : TimeSpan.FromMilliseconds(200)))
				cancellationToken.ThrowIfCancellationRequested();
		} while (clock.Elapsed < limit);
		throw new TimeoutException($"Appium did not expose {description} within {limit}.");
	}

	internal string Capture(string id, string name) =>
		SaveCapture(((ITakesScreenshot)Find(id)).GetScreenshot().AsByteArray, name);

	internal string SaveCapture(byte[] png, string name)
	{
		if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || name is "." or "..")
			throw new ArgumentException("Screenshot names must be a single nonempty filename.", nameof(name));
		var path = Path.Combine(Workspace.DiagnosticsRoot, "screenshots", $"{++captureIndex:D2}-{name}.png");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, png);
		return path;
	}

	internal async Task SaveDiagnostics()
	{
		List<Exception> failures = [];
		if (driver is not null)
		{
			await Attempt(failures, () => SaveText("page-source.xml", driver.PageSource, "application/xml"));
			await Attempt(failures, () =>
			{
				var path = Path.Combine(Diagnostics, "screen.png");
				driver.GetScreenshot().SaveAsFile(path);
				TestContext.Current.AddFileAttachment(path, "image/png");
				return Task.CompletedTask;
			});
		}
		await Attempt(failures, CapturePlatformDiagnostics);
		ThrowFailures(failures);
	}

	internal async Task Run(Uri address, Func<AppiumRunningApp, Task> scenario, Func<Task> startServer, Func<Task> stopServer)
	{
		if (used)
			throw new InvalidOperationException("A running app owns one test lifetime and cannot be reused.");
		used = true;

		List<Exception> failures = [];
		try
		{
			AssertArtifact();
			Directory.CreateDirectory(Diagnostics);
			applicationId = await ReadApplicationId();
			if (string.IsNullOrWhiteSpace(applicationId))
				throw new InvalidOperationException($"Built application identity is invalid: {Artifact}");
			await startServer();
			await Start(address);
			await scenario(this);
		}
		catch (Exception error)
		{
			failures.Add(error);
			await Attempt(failures, () => SaveText("failure.log", error.ToString()));
		}
		finally
		{
			await Attempt(failures, SaveDiagnostics);
			await Attempt(failures, () =>
			{
				driver?.Dispose();
				driver = null;
				return Task.CompletedTask;
			});
			// Failed session constructors can leave driver children alive. Stop our server tree
			// before deleting resources created by this test.
			await Attempt(failures, stopServer);
			foreach (var release in cleanup.AsEnumerable().Reverse())
				await Attempt(failures, release);
		}
		ThrowFailures(failures);
	}

	protected static AppiumOptions Options()
	{
		var options = new AppiumOptions { PlatformName = "iOS", AutomationName = "XCUITest" };
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);
		return options;
	}

	protected async Task<string> Command(
		string executable, string[] arguments, TimeSpan? timeout = null,
		Action<ProcessStartInfo>? configure = null)
	{
		return await CaptureCommand(ProcessRunner.Run(
			executable, arguments, Workspace.Root, timeout ?? TimeSpan.FromSeconds(30), configure));
	}

	protected async Task<string> CaptureCommand(Task<(int ExitCode, string Output, string Error)> command)
	{
		var result = await command;
		List<Exception> failures = [];
		await Attempt(failures, () =>
		{
			Assert.True(result.ExitCode == 0,
				$"Device command failed ({result.ExitCode}); inspect {Diagnostics}.\n{result.Error}");
			return Task.CompletedTask;
		});
		await Attempt(failures, () => SaveText($"{SampleWorkspace.CreateIdentity("command")}.log",
			$"Exit: {result.ExitCode}\n{result.Output}\n{result.Error}"));
		ThrowFailures(failures);
		return result.Output;
	}

	protected async Task SaveText(string name, string text, string contentType = "text/plain")
	{
		Directory.CreateDirectory(Diagnostics);
		var path = Path.Combine(Diagnostics, name);
		await File.WriteAllTextAsync(path, text);
		TestContext.Current.AddFileAttachment(path, contentType);
	}

	private void AssertArtifact()
	{
		var relative = Path.GetRelativePath(Workspace.Root, Artifact);
		Assert.True(!Path.IsPathRooted(relative) && relative != ".." &&
			!relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal),
			$"The exact built artifact must belong to the selected workspace: {Artifact}");
		Assert.True(File.Exists(Artifact) || Directory.Exists(Artifact), $"Built artifact is missing: {Artifact}");
	}

	internal static async Task Attempt(List<Exception> failures, Func<Task> action)
	{
		try
		{ await action(); }
		catch (Exception error) { failures.Add(error); }
	}

	internal static void ThrowFailures(List<Exception> failures)
	{
		if (failures.Count == 1)
			ExceptionDispatchInfo.Capture(failures[0]).Throw();
		if (failures.Count > 1)
			throw new AggregateException("Appium lifecycle failed; all diagnostics and owned cleanup were attempted.", failures);
	}

}
