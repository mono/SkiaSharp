using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Interactions;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class MauiUi(AppiumDriver driver, bool ios, string appId, SampleWorkspace workspace)
{
    internal IWebElement Find(string id) => Wait(() =>
    {
        var element = driver.FindElement(ios ? MobileBy.AccessibilityId(id) : By.Id($"{appId}:id/{id}"));
        return element.Displayed ? element : null;
    }, id);

    internal string Text(string id) => Find(id).Text;

    internal void WaitText(string id, Func<string, bool> predicate) =>
        Wait(() => predicate(Text(id)) ? Find(id) : null, $"text of {id}");

    internal void Click(string id) => Find(id).Click();

    internal void NavigateBasic(string title)
    {
        var size = driver.Manage().Window.Size;
        Drag(new Point(2, size.Height / 2), new Point(size.Width * 3 / 4, size.Height / 2));
        Wait(() =>
        {
            var element = driver.FindElement(ios
                ? MobileBy.IosNSPredicate($"label == '{title}'")
                : MobileBy.AndroidUIAutomator($"new UiSelector().text(\"{title}\")"));
            return element.Displayed ? element : null;
        }, $"Basic flyout item {title}").Click();
    }

    internal void Drag(IWebElement element, double fromX, double fromY, double toX, double toY)
    {
        var location = element.Location;
        var size = element.Size;
        Drag(new Point(location.X + (int)(size.Width * fromX), location.Y + (int)(size.Height * fromY)),
            new Point(location.X + (int)(size.Width * toX), location.Y + (int)(size.Height * toY)));
    }

    private void Drag(Point start, Point end)
    {
        var touch = new PointerInputDevice(PointerKind.Touch, "sample-finger");
        var sequence = new ActionSequence(touch);
        sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, start.X, start.Y, TimeSpan.Zero));
        sequence.AddAction(touch.CreatePointerDown(MouseButton.Touch));
        sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, end.X, end.Y, TimeSpan.FromMilliseconds(500)));
        sequence.AddAction(touch.CreatePointerUp(MouseButton.Touch));
        driver.PerformActions([sequence]);
        driver.ResetInputState();
    }

    internal string Capture(string id, string name) => SaveCapture(ReadCanvas(id), name);

    internal string CaptureStable(string id, string name)
    {
        var previous = ReadCanvas(id);
        var deadline = DateTime.UtcNow.AddSeconds(10);
        do
        {
            Thread.Sleep(200);
            var next = ReadCanvas(id);
            if (previous.AsSpan().SequenceEqual(next))
                return SaveCapture(next, name);
            previous = next;
        } while (DateTime.UtcNow < deadline);
        SaveCapture(previous, name);
        throw new TimeoutException($"Static MAUI canvas {id} did not settle within 10 seconds.");
    }

    private byte[] ReadCanvas(string id)
    {
        var element = Find(id);
        var location = element.Location;
        var size = element.Size;
        using var bitmap = SKBitmap.Decode(driver.GetScreenshot().AsByteArray);
        Assert.NotNull(bitmap);
        var viewport = driver.Manage().Window.Size;
        var bounds = ScreenshotBounds(location, size, viewport, new Size(bitmap.Width, bitmap.Height));
        using var crop = new SKBitmap(bounds.Width, bounds.Height);
        Assert.True(bitmap.ExtractSubset(crop, bounds), $"Cannot crop {id}: {bounds}.");
        using var image = SKImage.FromBitmap(crop);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private string SaveCapture(byte[] png, string name)
    {
        var path = Path.Combine(workspace.DiagnosticsRoot, "screenshots", name + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, png);
        TestContext.Current.AddFileAttachment(path, "image/png");
        return path;
    }

    internal static SKRectI ScreenshotBounds(Point location, Size element, Size viewport, Size screenshot)
    {
        if (viewport.Width <= 0 || viewport.Height <= 0 || element.Width <= 0 || element.Height <= 0)
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

    internal void SaveDiagnostics()
    {
        var directory = Path.Combine(workspace.DiagnosticsRoot, "appium");
        Directory.CreateDirectory(directory);
        var source = Path.Combine(directory, "page-source.xml");
        File.WriteAllText(source, driver.PageSource);
        TestContext.Current.AddFileAttachment(source, "application/xml");
        var screenshot = Path.Combine(directory, "screen.png");
        driver.GetScreenshot().SaveAsFile(screenshot);
        TestContext.Current.AddFileAttachment(screenshot, "image/png");
    }

    private static T Wait<T>(Func<T?> probe, string description) where T : class
    {
        var deadline = DateTime.UtcNow.AddSeconds(30);
        do
        {
            try
            {
                if (probe() is { } result)
                    return result;
            }
            catch (NoSuchElementException) { }
            catch (StaleElementReferenceException) { }
            Thread.Sleep(200);
        } while (DateTime.UtcNow < deadline);
        throw new TimeoutException($"MAUI UI did not expose {description} within 30 seconds.");
    }
}
