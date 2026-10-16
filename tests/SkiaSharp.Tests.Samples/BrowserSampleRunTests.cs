using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.SampleImage;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(BrowserSampleCollection))]
public class BrowserSampleRunTests : DotNetSampleTestBase
{
    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task WebSamplePageRenders() =>
        RunBrowserSample("Basic/Web", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".card img");
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/web-page.png");

            await app.Page.SetViewportSizeAsync(375, 812);
            var toggle = app.Page.GetByRole(AriaRole.Button, new() { Name = "Toggle navigation" });
            var navigation = app.Page.Locator(".navbar-collapse");
            await Assertions.Expect(navigation).ToBeHiddenAsync();
            await toggle.ClickAsync();
            await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "true");
            await Assertions.Expect(app.Page.Locator(".navbar-collapse.collapse.show")).ToBeVisibleAsync();
            await toggle.ClickAsync();
            await Assertions.Expect(toggle).ToHaveAttributeAsync("aria-expanded", "false");
            await Assertions.Expect(navigation).ToBeHiddenAsync();
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BrowserWebAssemblySampleRuns() =>
        RunBrowserSample("Basic/BrowserWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement("#output");
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/browser-wasm-page.png");
        });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyCpuSampleRuns(bool delayFirstPaint) =>
        RunBrowserSample("Basic/BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            if (delayFirstPaint)
            {
                // Keep the canvas visible while its raster interop is still loading.
                await app.Page.RouteAsync("**/_content/SkiaSharp.Views.Blazor/SKHtmlCanvas.js", async route =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
                    await route.ContinueAsync();
                });
            }

            await app.Navigate("/");
            await app.WaitForElement(".canvas-container canvas");
            // The element is visible before raster interop presents its first opaque frame.
            await app.Page.WaitForFunctionAsync("""
                () => {
                    const canvas = document.querySelector('.canvas-container canvas');
                    return canvas?.getContext('2d')?.getImageData(0, 0, 1, 1).data[3] === 255;
                }
                """).WaitAsync(TestContext.Current.CancellationToken);
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/blazor-cpu-page.png");
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyGpuSampleRuns() =>
        RunBrowserSample("Basic/BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/gpu");
            await app.WaitForElement(".canvas-container canvas");
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/blazor-gpu-page.png",
                new() { MaxAverageColorErrorFraction = 0.06 });
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyDrawingSampleDrawsAndClears() =>
        RunBrowserSample("Basic/BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/drawing");
            await app.WaitForElement(".canvas-container canvas");
            var canvas = app.Page.Locator(".canvas-container canvas");
            var bounds = await canvas.BoundingBoxAsync();
            Assert.NotNull(bounds);

            await app.Page.Locator(".color-swatch").Nth(1).ClickAsync();
            await Assertions.Expect(app.Page.Locator(".color-swatch").Nth(1)).ToHaveClassAsync("color-swatch selected");
            var brush = app.Page.Locator(".brush-slider");
            await brush.FocusAsync();
            await brush.PressAsync("Home");
            for (var i = 0; i < 14; i++)
                await brush.PressAsync("ArrowRight");
            await Assertions.Expect(brush).ToHaveValueAsync("8");
            await Assertions.Expect(app.Page.Locator(".size-label")).ToHaveTextAsync("8");

            await app.Page.Mouse.MoveAsync(bounds.X + 200, bounds.Y + 200);
            await app.Page.Mouse.DownAsync();
            await app.Page.Mouse.MoveAsync(bounds.X + 600, bounds.Y + 200, new() { Steps = 20 });
            await app.Page.Mouse.UpAsync();
            await app.Page.Locator(".color-swatch").Nth(2).ClickAsync();
            await app.Page.Mouse.MoveAsync(bounds.X + 200, bounds.Y + 350);
            await app.Page.Mouse.DownAsync();
            await app.Page.Mouse.MoveAsync(bounds.X + 600, bounds.Y + 450, new() { Steps = 20 });
            await app.Page.Mouse.UpAsync();
            await app.Page.Mouse.MoveAsync(0, 0);

            await app.Page.WaitForFunctionAsync("""
                () => {
                    const c = document.querySelector('.canvas-container canvas');
                    const p = c.getContext('2d').getImageData(400, 200, 1, 1).data;
                    const q = c.getContext('2d').getImageData(400, 400, 1, 1).data;
                    return p[0] === 229 && p[1] === 57 && p[2] === 53 &&
                           q[0] === 30 && q[1] === 136 && q[2] === 229;
                }
                """);
            var drawn = await app.Screenshot();
            var region = new SKRectI((int)bounds.X + 150, (int)bounds.Y + 150,
                (int)bounds.X + 650, (int)bounds.Y + 500);
            Assert.True(CountPixels(drawn, region, c => c == new SKColor(229, 57, 53)) > 2000);
            Assert.True(CountPixels(drawn, region, c => c == new SKColor(30, 136, 229)) > 2000);

            await app.Page.GetByRole(AriaRole.Button, new() { Name = "Clear", Exact = true }).ClickAsync();
            await app.Page.Mouse.MoveAsync(0, 0);
            await app.Page.WaitForFunctionAsync("""
                () => {
                    const c = document.querySelector('.canvas-container canvas');
                    const p = c.getContext('2d').getImageData(400, 200, 1, 1).data;
                    const q = c.getContext('2d').getImageData(400, 400, 1, 1).data;
                    return [...p, ...q].every(channel => channel === 255);
                }
                """);
            var cleared = await app.Screenshot();
            Assert.Equal(0, CountPixels(cleared, region, c =>
                c == new SKColor(229, 57, 53) || c == new SKColor(30, 136, 229)));

            SampleImage.Validate(drawn, $"Host/{SampleLookup.HostPlatform}/blazor-drawing-page.png");
            SampleImage.Validate(cleared, $"Host/{SampleLookup.HostPlatform}/blazor-drawing-cleared-page.png");
        });

    [Theory]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "Browser")]
    [InlineData("navigate")]
    [InlineData("wait")]
    [InlineData("screenshot")]
    [InlineData("callback")]
    public async Task BrowserOperationsReportAllConsoleErrors(string operation)
    {
        var error = await Assert.ThrowsAsync<Xunit.Sdk.TrueException>(() =>
            RunBrowserSample("Basic/Web", ["--urls", "http://127.0.0.1:0"], async app =>
            {
                Assert.IsType<DotNetRunningApp>(app.HostApp);
                await app.Navigate("/");
                await app.Page.EvaluateAsync("console.error('first browser sentinel'); console.error('second browser sentinel')");
                switch (operation)
                {
                    case "navigate": await app.Navigate("/"); break;
                    case "wait": await app.WaitForElement("body"); break;
                    case "screenshot": await app.Screenshot(); break;
                }
            }));
        Assert.Contains("first browser sentinel", error.Message);
        Assert.Contains("second browser sentinel", error.Message);
    }

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "Browser")]
    public async Task BrowserCallbackReportsPageErrors()
    {
        var error = await Assert.ThrowsAsync<Xunit.Sdk.TrueException>(() =>
            RunBrowserSample("Basic/Web", ["--urls", "http://127.0.0.1:0"], async app =>
            {
                await app.Navigate("/");
                var received = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                app.Page.PageError += (_, _) => received.TrySetResult();
                await app.Page.EvaluateAsync("setTimeout(() => { throw new Error('page failure sentinel'); }, 0)");
                await received.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            }));
        Assert.Contains("page failure sentinel", error.Message);
    }
}
