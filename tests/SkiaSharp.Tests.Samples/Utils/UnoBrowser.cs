using Microsoft.Playwright;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class UnoBrowser
{
    internal static Task EnableAccessibility(BrowserRunningApp app) =>
        // Uno's documented screen-reader activation button is intentionally outside the viewport.
        app.Page.GetByRole(AriaRole.Button, new() { Name = "Enable accessibility", Exact = true })
            .EvaluateAsync("button => button.click()");

    internal static async Task Click(BrowserRunningApp app, ILocator control)
    {
        await control.WaitForAsync();
        var bounds = await control.BoundingBoxAsync();
        var viewport = app.Page.ViewportSize;
        Assert.NotNull(bounds);
        Assert.NotNull(viewport);
        Assert.True(bounds.Width > 0 && bounds.Height > 0);
        var x = bounds.X + bounds.Width / 2;
        var y = bounds.Y + bounds.Height / 2;
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"Uno input '{await control.GetAttributeAsync("aria-label")}': " +
            $"bounds {bounds.X},{bounds.Y} {bounds.Width}x{bounds.Height}; " +
            $"style {await control.GetAttributeAsync("style")}.");
        Assert.InRange(x, 0, viewport.Width - 1);
        Assert.InRange(y, 0, viewport.Height - 1);
        // The semantic overlay has pointer-events:none; target the real rendered control using its semantic bounds.
        await app.Page.Mouse.ClickAsync(x, y);
    }
}
