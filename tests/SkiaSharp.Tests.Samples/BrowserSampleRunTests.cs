using Microsoft.Playwright;
using SkiaSharp.Tests.Shared;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

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
            SampleImage.Browser("Web/page").Verify(GoldenInput.Path(screenshot));
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
            SampleImage.Browser("Browser/wasm-page").Verify(GoldenInput.Path(screenshot));
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyCpuSampleRuns() =>
        RunBrowserSample("Basic/BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".canvas-container canvas");
            await SampleImage.Browser("Blazor/cpu").VerifyAsync(
                GoldenInput.Capture(app.Screenshot),
                TimeSpan.FromSeconds(10),
                TestContext.Current.CancellationToken);
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
            SampleImage.Browser("Blazor/gpu").Verify(
                GoldenInput.Path(screenshot),
                new(maxAverageColorErrorFraction: 0.06));
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

            SampleImage.Browser("Blazor/drawing-drawn").Verify(GoldenInput.Path(drawn));
            SampleImage.Browser("Blazor/drawing-cleared").Verify(GoldenInput.Path(cleared));
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "Browser")]
    public Task BrowserScreenshotsFinishFiniteTransitions() =>
        RunBrowserSample("Basic/Web", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Page.SetContentAsync("""
                <style>
                    body { margin: 0; }
                    #swatch {
                        width: 100px;
                        height: 100px;
                        background-color: rgb(255, 0, 0);
                        transition: background-color 60s linear;
                    }
                </style>
                <div id="swatch"></div>
                """);
            var swatch = app.Page.Locator("#swatch");
            await swatch.EvaluateAsync("""
                element => {
                    getComputedStyle(element).backgroundColor;
                    element.style.backgroundColor = 'rgb(0, 0, 255)';
                    const [transition] = element.getAnimations();
                    // Hold the transition halfway so scheduling cannot hide the race.
                    transition.pause();
                    transition.currentTime = 30000;
                }
                """);
            await Assertions.Expect(swatch).ToHaveCSSAsync("background-color", "rgb(128, 0, 128)");

            var screenshot = await app.Screenshot();
            TestContext.Current.AddFileAttachment(screenshot, "image/png");
            using var bitmap = SKBitmap.Decode(screenshot);
            Assert.NotNull(bitmap);
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(50, 50));
            await Assertions.Expect(swatch).ToHaveCSSAsync("background-color", "rgb(0, 0, 255)");
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "Browser")]
    public Task BrowserScreenshotsRetryUntilRendered() =>
        RunBrowserSample("Basic/Web", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Page.SetContentAsync("<style>html { background: blue; }</style>");
            var expected = await app.Screenshot();
            await app.Page.EvaluateAsync("document.documentElement.style.background = 'red'");
            var attempts = 0;

            var matched = await Golden.VerifyAsync(
                GoldenInput.Capture(async () =>
                {
                    var actual = await app.Screenshot();
                    if (++attempts == 1)
                        await app.Page.EvaluateAsync("document.documentElement.style.background = 'blue'");
                    return actual;
                }),
                GoldenInput.Path(expected),
                SampleImage.DefaultTimeout,
                TestContext.Current.CancellationToken);

            Assert.Equal(2, attempts);
            Assert.True(File.Exists(matched));
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
                    case "screenshot":
                        await SampleImage.Browser("Web/page").VerifyAsync(
                            GoldenInput.Capture(app.Screenshot),
                            SampleImage.DefaultTimeout,
                            TestContext.Current.CancellationToken);
                        break;
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
