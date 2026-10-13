using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public class BrowserSampleTests : BasicSampleTestBase
{
    private Task RunBrowserSample(string name, IEnumerable<string> arguments, Func<BrowserRunningApp, Task> test) =>
        RunSample(name, arguments, (app, _) => BrowserRunningApp.Run(app, test));

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task WebSamplePageRenders() =>
        RunBrowserSample("Web", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".card img");
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/web-page.png");
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BrowserWebAssemblySampleRuns() =>
        RunBrowserSample("BrowserWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement("#output");
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/browser-wasm-page.png");
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyCpuSampleRuns() =>
        RunBrowserSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".canvas-container canvas");
            var screenshot = await app.Screenshot();
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/blazor-cpu-page.png");
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyGpuSampleRuns() =>
        RunBrowserSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
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
        RunBrowserSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
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

    private async Task RunUnoBrowserSample(Func<BrowserRunningApp, Task> test)
    {
        const string framework = "net10.0-browserwasm";
        var folder = Path.Combine("Basic", "UnoPlatform");
        var projectRelative = Path.Combine("SkiaSharpSample", "SkiaSharpSample.csproj");
        var options = new DotNetOptions { Framework = framework };
        var workspace = await BuildSample(folder, projectRelative, options);
        var project = Path.Combine(workspace.Root, "samples", folder, projectRelative);
        await DotNet.Run(workspace, project, ["--urls", "http://127.0.0.1:0"],
            app => BrowserRunningApp.Run(app, test), options);
    }

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task UnoPlatformCpuSampleRuns() =>
        RunUnoBrowserSample(async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement("#uno-canvas");
            var screenshot = await WaitForUnoCpu(app);
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/uno-cpu-page.png");
        });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task UnoPlatformDrawingSampleNavigatesDrawsAndClears() =>
        RunUnoBrowserSample(async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement("#uno-canvas");
            var cpu = await WaitForUnoCpu(app);

            // SkiaRenderer draws the navigation and controls into the 1280x900 canvas.
            await app.Page.Mouse.ClickAsync(220, 24);
            await app.Page.Mouse.MoveAsync(0, 0);
            var region = new SKRectI(150, 150, 850, 550);
            var empty = await app.Screenshot();
            Assert.Equal(region.Width * region.Height, CountPixels(empty, region, c => c == SKColors.White));

            await app.Page.Mouse.ClickAsync(471, 852);
            await app.Page.Mouse.MoveAsync(1000, 600);
            for (var i = 0; i < 4; i++)
                await app.Page.Mouse.WheelAsync(0, -100);
            await app.Page.Mouse.MoveAsync(200, 250);
            await app.Page.Mouse.DownAsync();
            await app.Page.Mouse.MoveAsync(600, 250, new() { Steps = 20 });
            await app.Page.Mouse.UpAsync();
            await app.Page.Mouse.ClickAsync(515, 852);
            await app.Page.Mouse.MoveAsync(200, 400);
            await app.Page.Mouse.DownAsync();
            await app.Page.Mouse.MoveAsync(600, 500, new() { Steps = 20 });
            await app.Page.Mouse.UpAsync();
            await app.Page.Mouse.MoveAsync(0, 0);
            var drawn = await app.Screenshot();
            Assert.True(CountPixels(drawn, region, c => c == new SKColor(229, 57, 53)) > 2000);
            Assert.True(CountPixels(drawn, region, c => c == new SKColor(30, 136, 229)) > 2000);

            await app.Page.Mouse.ClickAsync(1237, 76);
            await app.Page.Mouse.MoveAsync(0, 0);
            var cleared = await ScreenshotAfterUnoClear(app, drawn, region);
            Assert.Equal(region.Width * region.Height, CountPixels(cleared, region, c => c == SKColors.White));

            await app.Page.Mouse.ClickAsync(50, 24);
            await app.Page.Mouse.MoveAsync(0, 0);
            var returned = await WaitForUnoCpu(app);
            SampleImage.Validate(returned, cpu);
            SampleImage.Validate(drawn, $"Host/{SampleLookup.HostPlatform}/uno-drawing-page.png");
            SampleImage.Validate(cleared, $"Host/{SampleLookup.HostPlatform}/uno-drawing-cleared-page.png");
            SampleImage.Validate(returned, $"Host/{SampleLookup.HostPlatform}/uno-cpu-page.png");
        });

    private static async Task<string> WaitForUnoCpu(BrowserRunningApp app)
    {
        // Uno creates its canvas while the splash screen still covers the application.
        await app.Page.Locator(".uno-loader").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        var screenshot = await app.Screenshot();
        Assert.True(CountPixels(screenshot, new SKRectI(100, 150, 1180, 750),
            c => c.Blue > 150 && c.Blue > c.Red && c.Blue > c.Green) > 100000,
            "Uno CPU scene must render its blue/purple background, not the splash or a blank canvas.");
        return screenshot;
    }

    private static async Task<string> ScreenshotAfterUnoClear(BrowserRunningApp app, string drawn, SKRectI region)
    {
        using var before = SKBitmap.Decode(drawn);
        Assert.NotNull(before);
        var normalButtonColor = before.GetPixel(1237, 85);
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var screenshot = await app.Screenshot();
            using var bitmap = SKBitmap.Decode(screenshot);
            Assert.NotNull(bitmap);
            if (bitmap.GetPixel(1237, 85) == normalButtonColor &&
                CountPixels(screenshot, region, c => c == SKColors.White) == region.Width * region.Height)
                return screenshot;
            // Observe the native pressed-state transition without changing the app's clock.
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException("Uno did not render a cleared canvas with the Clear button in its normal state.");
    }

    private static int CountPixels(string screenshot, SKRectI region, Func<SKColor, bool> matches)
    {
        using var bitmap = SKBitmap.Decode(screenshot);
        Assert.NotNull(bitmap);
        Assert.InRange(region.Left, 0, bitmap.Width - 1);
        Assert.InRange(region.Top, 0, bitmap.Height - 1);
        Assert.InRange(region.Right, region.Left + 1, bitmap.Width);
        Assert.InRange(region.Bottom, region.Top + 1, bitmap.Height);
        var count = 0;
        for (var y = region.Top; y < region.Bottom; y++)
            for (var x = region.Left; x < region.Right; x++)
                if (matches(bitmap.GetPixel(x, y)))
                    count++;
        TestContext.Current.TestOutputHelper?.WriteLine($"Measured {count} matching pixels in {region} from {screenshot}.");
        return count;
    }

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
            RunBrowserSample("Web", ["--urls", "http://127.0.0.1:0"], async app =>
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
            RunBrowserSample("Web", ["--urls", "http://127.0.0.1:0"], async app =>
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
