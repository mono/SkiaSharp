using System.Diagnostics;
using System.Drawing;
using System.Runtime.ExceptionServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal readonly record struct ToolCommandResult(string Command, int ExitCode, string Output, string Error);

// Native apps own a driver and platform resources, not an HTTP sample server.
internal abstract class AppiumRunningApp
{
	private readonly List<Func<Task>> cleanup = [];
	private AppiumDriver? driver;
	private bool used;

	protected AppiumRunningApp(SampleWorkspace workspace, string artifact, string applicationId)
	{
		Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
		ArgumentException.ThrowIfNullOrWhiteSpace(artifact);
		ArgumentException.ThrowIfNullOrWhiteSpace(applicationId);
		Artifact = Path.GetFullPath(artifact);
		ApplicationId = applicationId;
		Diagnostics = Path.Combine(workspace.DiagnosticsRoot, "appium");
	}

	internal SampleWorkspace Workspace { get; }
	internal string Artifact { get; }
	internal string ApplicationId { get; }
	internal string Diagnostics { get; }
	internal AppiumDriver Driver => driver ?? throw new InvalidOperationException("The owned Appium session is not ready.");
	internal abstract PointerKind PointerKind { get; }
	internal virtual By Locator(string id) => MobileBy.AccessibilityId(id);
	internal virtual void SubmitSearch() => Driver.SwitchTo().ActiveElement().SendKeys(Keys.Enter);

	protected abstract Task Start(Uri address);
	internal virtual void ConfigureServer(ProcessStartInfo start) { }
	protected virtual Task CapturePlatformDiagnostics() => Task.CompletedTask;
	protected void Own(Func<Task> release) => cleanup.Add(release);

	protected void SetDriver(AppiumDriver value)
	{
		driver = value;
		driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
	}

	internal IWebElement Find(By selector) =>
		Wait(() => Driver.FindElement(selector) is { Displayed: true } element ? element : null, selector.ToString());

	internal IWebElement Find(string id) => Find(Locator(id));
	internal string Text(string id) => Find(id).Text;
	internal void Click(string id) => Find(id).Click();
	internal void Click(By selector) => Find(selector).Click();

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

	internal virtual void Drag(Point start, Point end)
	{
		var pointer = new PointerInputDevice(PointerKind, "sample-pointer");
		var button = PointerKind == PointerKind.Touch ? MouseButton.Touch : MouseButton.Left;
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

	internal T Wait<T>(Func<T?> probe, string description) where T : class
	{
		var limit = TimeSpan.FromSeconds(30);
		var clock = Stopwatch.StartNew();
		do
		{
			TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
			try
			{
				if (probe() is { } result)
					return result;
			}
			catch (NoSuchElementException) { }
			catch (StaleElementReferenceException) { }
			Thread.Sleep(200);
		} while (clock.Elapsed < limit);
		throw new TimeoutException($"Appium did not expose {description} within {limit}.");
	}

	internal string Capture(string id, string name) =>
		SaveCapture(((ITakesScreenshot)Find(id)).GetScreenshot().AsByteArray, name);

	internal string CaptureStable(string id, string name)
	{
		var previous = ((ITakesScreenshot)Find(id)).GetScreenshot().AsByteArray;
		using var first = SKBitmap.Decode(previous);
		Assert.NotNull(first);

		var width = first.Width;
		var height = first.Height;
		var pixels = first.GetPixelSpan().ToArray();
		var deadline = DateTime.UtcNow.AddSeconds(10);

		do
		{
			TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
			Thread.Sleep(200);

			var next = ((ITakesScreenshot)Find(id)).GetScreenshot().AsByteArray;
			using var bitmap = SKBitmap.Decode(next);
			Assert.NotNull(bitmap);

			// Compare decoded pixels; retain the driver's original PNG without re-encoding.
			if (width == bitmap.Width && height == bitmap.Height &&
				pixels.AsSpan().SequenceEqual(bitmap.GetPixelSpan()))
				return SaveCapture(next, name);

			previous = next;
			width = bitmap.Width;
			height = bitmap.Height;
			pixels = bitmap.GetPixelSpan().ToArray();
		} while (DateTime.UtcNow < deadline);

		SaveCapture(previous, name);
		throw new TimeoutException($"Static native element {id} did not settle within 10 seconds.");
	}

	private string SaveCapture(byte[] png, string name)
	{
		if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name) || name is "." or "..")
			throw new ArgumentException("Screenshot names must be a single nonempty filename.", nameof(name));
		var path = Path.Combine(Workspace.DiagnosticsRoot, "screenshots", name + ".png");
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllBytes(path, png);
		TestContext.Current.AddFileAttachment(path, "image/png");
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
			await Attempt(failures, CapturePlatformDiagnostics);
		}
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
			RequireArtifact();
			Directory.CreateDirectory(Diagnostics);
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

	protected AppiumOptions Options(string platform, string automation)
	{
		var options = new AppiumOptions { PlatformName = platform, AutomationName = automation };
		options.AddAdditionalAppiumOption("newCommandTimeout", 120);
		return options;
	}

	protected async Task<string> Command(
		string executable, string[] arguments, TimeSpan? timeout = null,
		Action<ProcessStartInfo>? configure = null)
	{
		var result = await ProcessRunner.Run(executable, arguments, Workspace.Root,
			timeout ?? TimeSpan.FromSeconds(30), configure);
		return await CaptureCommand(Task.FromResult(new ToolCommandResult(
			$"{executable} {string.Join(" ", arguments)}", result.ExitCode, result.Output, result.Error)));
	}

	protected async Task<string> CaptureCommand(Task<ToolCommandResult> command)
	{
		var result = await command;
		List<Exception> failures = [];
		if (result.ExitCode != 0)
			failures.Add(new InvalidOperationException($"{result.Command} failed ({result.ExitCode}); inspect {Diagnostics}.\n{result.Error}"));
		await Attempt(failures, () => SaveText($"command-{Guid.NewGuid():N}.log",
			$"{result.Command}\nExit: {result.ExitCode}\n{result.Output}\n{result.Error}"));
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

	protected void Require(bool condition, string message)
	{
		if (!condition)
			throw new InvalidOperationException(message);
	}

	private void RequireArtifact()
	{
		var relative = Path.GetRelativePath(Workspace.Root, Artifact);
		Require(!Path.IsPathRooted(relative) && relative != ".." &&
			!relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal),
			$"The exact built artifact must belong to the selected workspace: {Artifact}");
		Require(File.Exists(Artifact) || Directory.Exists(Artifact), $"Built artifact is missing: {Artifact}");
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

	internal static void AssertRendered(string path)
	{
		using var bitmap = SKBitmap.Decode(path);
		Assert.NotNull(bitmap);
		var colors = new HashSet<SKColor>();
		var colorful = 0;
		var count = 0;
		for (var y = 0; y < bitmap.Height; y += 4)
		{
			for (var x = 0; x < bitmap.Width; x += 4)
			{
				var color = bitmap.GetPixel(x, y);
				colors.Add(color);
				if (Math.Max(color.Red, Math.Max(color.Green, color.Blue)) -
					Math.Min(color.Red, Math.Min(color.Green, color.Blue)) > 25)
					colorful++;
				count++;
			}
		}
		Assert.True(colors.Count > 32 && colorful > count * 0.05,
			$"Expected a rendered colored scene, not a blank canvas: {colors.Count} colors, {colorful}/{count} colored pixels; {path}.");
	}

	internal static void AssertSceneChanged(string before, string after)
	{
		using var original = SKBitmap.Decode(before);
		using var changed = SKBitmap.Decode(after);
		Assert.NotNull(original);
		Assert.NotNull(changed);
		Assert.Equal(original.Width, changed.Width);
		Assert.Equal(original.Height, changed.Height);
		for (var y = original.Height / 4; y < original.Height / 2; y++)
			for (var x = original.Width / 4; x < original.Width * 3 / 4; x++)
				if (original.GetPixel(x, y) != changed.GetPixel(x, y))
					return;
		Assert.Fail($"The rendered scene did not change: {before}; {after}.");
	}

}
