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
    internal string Text(By selector) => Find(selector).Text;
    internal void Click(string id) => Find(id).Click();
    internal void Click(By selector) => Find(selector).Click();

    internal void WaitText(string id, Func<string, bool> predicate) => WaitText(Locator(id), predicate);
    internal void WaitText(By selector, Func<string, bool> predicate) =>
        Wait(() =>
        {
            var element = Driver.FindElement(selector);
            return element.Displayed && predicate(element.Text) ? element : null;
        }, $"text of {selector}");

    internal void Drag(IWebElement element, double fromX, double fromY, double toX, double toY)
    {
        if (new[] { fromX, fromY, toX, toY }.Any(value => !double.IsFinite(value) || value < 0 || value > 1))
            throw new ArgumentOutOfRangeException(nameof(fromX), "Element-relative drag coordinates must be finite values from zero to one.");
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
        try { Driver.PerformActions([sequence]); }
        catch (Exception error) { failures.Add(error); }
        try { Driver.ResetInputState(); }
        catch (Exception error) { failures.Add(error); }
        ThrowFailures(failures);
    }

    internal T Wait<T>(Func<T?> probe, string description, TimeSpan? timeout = null) where T : class
    {
        var limit = timeout ?? TimeSpan.FromSeconds(30);
        if (limit <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeout));
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

    internal virtual byte[] ElementScreenshot(IWebElement element)
    {
        // All four native drivers implement GET /session/:id/element/:id/screenshot.
        // In particular, desktop window size is NOT the coordinate space of a screen capture.
        if (element is not ITakesScreenshot native)
            throw new NotSupportedException("The selected native element does not implement element screenshots.");
        return native.GetScreenshot().AsByteArray;
    }

    internal string Capture(string id, string name) => Capture(Locator(id), name);
    internal string Capture(By selector, string name) => SaveCapture(NormalizeScreenshot(ElementScreenshot(Find(selector))), name);
    internal string CaptureStable(string id, string name) => CaptureStable(Locator(id), name);

    internal string CaptureStable(By selector, string name)
    {
        var previous = ElementScreenshot(Find(selector));
        var previousPixels = ReadScreenshotPixels(previous);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        do
        {
            TestContext.Current.CancellationToken.ThrowIfCancellationRequested();
            Thread.Sleep(200);
            var next = ElementScreenshot(Find(selector));
            var nextPixels = ReadScreenshotPixels(next);
            if (previousPixels.Width == nextPixels.Width && previousPixels.Height == nextPixels.Height &&
                previousPixels.Pixels.AsSpan().SequenceEqual(nextPixels.Pixels))
                return SaveCapture(NormalizeScreenshot(next), name);
            previous = next;
            previousPixels = nextPixels;
        } while (DateTime.UtcNow < deadline);
        SaveCapture(NormalizeScreenshot(previous), name);
        throw new TimeoutException($"Static native element {selector} did not settle within 10 seconds.");
    }

    private static (int Width, int Height, byte[] Pixels) ReadScreenshotPixels(byte[] png)
    {
        // Ancillary PNG chunks are not rendering changes. Compare every decoded pixel,
        // with fixed channel order and unpremultiplied alpha, and no difference tolerance.
        using var bitmap = DecodeScreenshot(png);
        return (bitmap.Width, bitmap.Height, bitmap.GetPixelSpan().ToArray());
    }

    internal static byte[] NormalizeScreenshot(byte[] png)
    {
        using var bitmap = DecodeScreenshot(png);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("Could not encode the native screenshot's decoded pixels.");
        return data.ToArray();
    }

    private static SKBitmap DecodeScreenshot(byte[] png)
    {
        var bounds = SKBitmap.DecodeBounds(png);
        if (bounds.Width <= 0 || bounds.Height <= 0)
            throw new InvalidOperationException("The native element screenshot has no valid image dimensions.");
        return SKBitmap.Decode(png, new SKImageInfo(bounds.Width, bounds.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul))
            ?? throw new InvalidOperationException("Could not decode the native element screenshot.");
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
        }
        await Attempt(failures, CapturePlatformDiagnostics);
        ThrowFailures(failures);
    }

    internal async Task Run(Uri address, Func<AppiumRunningApp, Task> scenario,
        Func<Task> startServer, Func<Task> stopServer)
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
            // before removing WDA/installations or restoring the device's original state.
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

    protected async Task<string> Command(string executable, string[] arguments, TimeSpan? timeout = null,
        string? directory = null, Action<ProcessStartInfo>? configure = null)
    {
        var result = await ProcessRunner.Run(executable, arguments, directory ?? Workspace.Root,
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
        try { await action(); }
        catch (Exception error) { failures.Add(error); }
    }

    internal static void ThrowFailures(List<Exception> failures)
    {
        if (failures.Count == 1)
            ExceptionDispatchInfo.Capture(failures[0]).Throw();
        if (failures.Count > 1)
            throw new AggregateException("Appium lifecycle failed; all diagnostics and owned cleanup were attempted.", failures);
    }

    internal static SKRectI ScreenshotBounds(Point location, Size element, Size viewport, Size screenshot)
    {
        if (viewport.Width <= 0 || viewport.Height <= 0 || element.Width <= 0 || element.Height <= 0 ||
            screenshot.Width <= 0 || screenshot.Height <= 0)
            throw new InvalidOperationException("Screenshot and element coordinate dimensions must be positive.");
        var scaleX = screenshot.Width / (double)viewport.Width;
        var scaleY = screenshot.Height / (double)viewport.Height;
        var bounds = new SKRectI(
            (int)Math.Round(location.X * scaleX), (int)Math.Round(location.Y * scaleY),
            (int)Math.Round((location.X + element.Width) * scaleX),
            (int)Math.Round((location.Y + element.Height) * scaleY));
        if (bounds.Left < 0 || bounds.Top < 0 || bounds.Right > screenshot.Width || bounds.Bottom > screenshot.Height)
            throw new InvalidOperationException($"Canvas is outside the screenshot: {bounds}, screenshot {screenshot}.");
        return bounds;
    }

    internal static void AssertRendered(string path)
    {
        using var bitmap = SKBitmap.Decode(path);
        Assert.NotNull(bitmap);
        var colors = new HashSet<SKColor>();
        var colorful = 0;
        var count = 0;
        for (var y = 0; y < bitmap.Height; y += 4)
            for (var x = 0; x < bitmap.Width; x += 4)
            {
                var color = bitmap.GetPixel(x, y);
                colors.Add(color);
                if (Math.Max(color.Red, Math.Max(color.Green, color.Blue)) -
                    Math.Min(color.Red, Math.Min(color.Green, color.Blue)) > 25)
                    colorful++;
                count++;
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

    protected async Task StopProcess(Process process, Task<string> stdout, Task<string> stderr, string name)
    {
        List<Exception> failures = [];
        await Attempt(failures, async () =>
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
        });
        await Attempt(failures, async () => await SaveText(name + ".stdout.log", await stdout.WaitAsync(TimeSpan.FromSeconds(30))));
        await Attempt(failures, async () => await SaveText(name + ".stderr.log", await stderr.WaitAsync(TimeSpan.FromSeconds(30))));
        process.Dispose();
        ThrowFailures(failures);
    }
}
