using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection(nameof(BrowserSampleCollection))]
public class BrowserUnoSampleRunTests : DotNetSampleTestBase
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
            await SampleImage.Validate(app.Screenshot, $"Host/{SampleLookup.HostPlatform}/uno-cpu-page.png");
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
            await SampleImage.Validate(app.Screenshot, $"Host/{SampleLookup.HostPlatform}/uno-cpu-page.png");

            // SkiaRenderer draws the navigation and controls into the 1280x900 canvas.
            await app.Page.Mouse.ClickAsync(220, 24);
            await app.Page.Mouse.MoveAsync(0, 0);

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
            await SampleImage.Validate(app.Screenshot, $"Host/{SampleLookup.HostPlatform}/uno-drawing-page.png");

            await app.Page.Mouse.ClickAsync(1237, 76);
            await app.Page.Mouse.MoveAsync(0, 0);
            await SampleImage.Validate(app.Screenshot, $"Host/{SampleLookup.HostPlatform}/uno-drawing-cleared-page.png");

            await app.Page.Mouse.ClickAsync(50, 24);
            await app.Page.Mouse.MoveAsync(0, 0);
            await app.WaitForElement("#uno-canvas");
            await SampleImage.Validate(app.Screenshot, $"Host/{SampleLookup.HostPlatform}/uno-cpu-page.png");
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
}
