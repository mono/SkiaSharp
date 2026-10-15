using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;
using static SkiaSharp.Tests.Samples.Utils.SampleImage;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(BrowserSampleCollection))]
public class UnoBrowserSampleRunTests : DotNetSampleTestBase
{
    [Theory]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    [InlineData(1)]
    [InlineData(4)]
    public Task UnoPlatformCpuSampleRuns(int cpuRate) =>
        RunUnoBrowserSample(async app =>
        {
            var session = await app.Page.Context.NewCDPSessionAsync(app.Page);
            await session.SendAsync("Emulation.setCPUThrottlingRate", new() { ["rate"] = cpuRate });
            await app.Page.AddInitScriptAsync("""
                (() => {
                    const querySelector = document.querySelector;
                    globalThis.unoSplashChecks = 0;
                    document.querySelector = function(selector) {
                        if (selector === '.uno-loader')
                            globalThis.unoSplashChecks++;
                        return querySelector.call(this, selector);
                    };
                })();
                """);
            await app.Navigate("/");
            await app.WaitForElement("#uno-canvas");
            var screenshot = await WaitForUnoCpu(app);
            SampleImage.Validate(screenshot, $"Host/{SampleLookup.HostPlatform}/uno-cpu-page.png");

            // Observe subsequent frames, not a sleep: the temporary render subscription must stop.
            var checks = await app.Page.EvaluateAsync<int[]>("""
                async () => {
                    await new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));
                    const before = globalThis.unoSplashChecks;
                    for (let frame = 0; frame < 20; frame++)
                        await new Promise(resolve => requestAnimationFrame(resolve));
                    return [before, globalThis.unoSplashChecks];
                }
                """);
            Assert.True(checks[0] > 0, "The startup rendering workaround must observe the loader.");
            Assert.Equal(checks[0], checks[1]);
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
}
