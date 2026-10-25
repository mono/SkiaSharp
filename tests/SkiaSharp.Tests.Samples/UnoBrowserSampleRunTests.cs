using System.Diagnostics;
using Microsoft.Playwright;
using SkiaSharp.Tests.Shared;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(BrowserSampleCollection))]
public class UnoBrowserSampleRunTests : DotNetSampleTestBase
{
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
            await WaitForUnoCpu(app);
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
            await WaitForUnoCpu(app);

            // SkiaRenderer draws the navigation and controls into the 1280x900 canvas.
            await app.Page.Mouse.ClickAsync(220, 24);
            await app.Page.Mouse.MoveAsync(0, 0);

            await DrawUnoStrokes(app);
            await SampleImage.Browser("Uno/drawing-drawn").VerifyAsync(
                GoldenInput.Capture(app.Screenshot),
                TimeSpan.FromSeconds(10),
                TestContext.Current.CancellationToken);

            await app.Page.Mouse.ClickAsync(1237, 76);
            await app.Page.Mouse.MoveAsync(0, 0);
            await SampleImage.Browser("Uno/drawing-cleared").VerifyAsync(
                GoldenInput.Capture(app.Screenshot),
                TimeSpan.FromSeconds(4),
                TestContext.Current.CancellationToken);

            await app.Page.Mouse.ClickAsync(50, 24);
            await app.Page.Mouse.MoveAsync(0, 0);
            await WaitForUnoCpu(app);
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

    private static async Task DrawUnoStrokes(BrowserRunningApp app)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            await app.Page.Mouse.ClickAsync(471, 852);
            // Clear retains the brush size, so retries must not increment it again.
            if (attempt == 1)
            {
                await app.Page.Mouse.MoveAsync(1000, 600);
                for (var i = 0; i < 4; i++)
                    await app.Page.Mouse.WheelAsync(0, -100);
            }
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

            try
            {
                await WaitForUnoStrokeColors(app, new SKColor(229, 57, 53), new SKColor(30, 136, 229));
                return;
            }
            catch (TimeoutException error) when (attempt < 3)
            {
                TestContext.Current.TestOutputHelper?.WriteLine(
                    $"Uno stroke colors were not ready on attempt {attempt}; clearing before retry: {error.Message}");
                await app.Page.Mouse.ClickAsync(1237, 76);
                await app.Page.Mouse.MoveAsync(0, 0);
                await WaitForUnoStrokeColors(app, SKColors.White, SKColors.White);
            }
        }
    }

    private static async Task WaitForUnoStrokeColors(BrowserRunningApp app, SKColor red, SKColor blue)
    {
        var cancellation = TestContext.Current.CancellationToken;
        var timer = Stopwatch.StartNew();
        while (true)
        {
            // Uno uses WebGL without preserveDrawingBuffer; sample the compositor, not getContext('2d').
            using var bitmap = SKBitmap.Decode(await app.Page.ScreenshotAsync(new()
            {
                FullPage = true,
                Animations = ScreenshotAnimations.Disabled,
                Timeout = Math.Max(1, 10000 - (int)timer.ElapsedMilliseconds)
            }).WaitAsync(cancellation));
            app.AssertNoErrors();
            Assert.NotNull(bitmap);
            Assert.Equal(1280, bitmap.Width);
            Assert.Equal(900, bitmap.Height);
            var actualRed = bitmap.GetPixel(400, 250);
            var actualBlue = bitmap.GetPixel(400, 450);
            if (actualRed == red && actualBlue == blue)
                return;
            if (timer.Elapsed >= TimeSpan.FromSeconds(10))
                throw new TimeoutException(
                    $"Uno stroke colors did not settle: (400,250) expected {red}, actual {actualRed}; " +
                    $"(400,450) expected {blue}, actual {actualBlue}.");
            await Task.Delay(100, cancellation);
        }
    }

    private static async Task<string> WaitForUnoCpu(BrowserRunningApp app)
    {
        // Uno creates its canvas while the splash screen still covers the application.
        await app.Page.Locator(".uno-loader").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
        return await SampleImage.Browser("Uno/cpu").VerifyAsync(
            GoldenInput.Capture(app.Screenshot),
            TimeSpan.FromSeconds(10),
            TestContext.Current.CancellationToken);
    }
}
