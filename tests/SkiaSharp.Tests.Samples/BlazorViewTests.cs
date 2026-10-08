using Microsoft.Playwright;
using SkiaSharp;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

/// <summary>
/// Tests that verify SkiaSharp packages work in Blazor WebAssembly applications.
/// </summary>
[Trait("Category", "Browser")]
public class BlazorViewTests(ITestOutputHelper output) : GeneratedAppTestBase(output)
{
    [Theory]
    [InlineData("SKCanvasView", "SKPaintSurfaceEventArgs")]
    [InlineData("SKGLView", "SKPaintGLSurfaceEventArgs")]
    public Task RendersCanvas(string view, string eventArgs) => TestBlazor(view, eventArgs, compareGolden: false);

    [Theory]
    [Trait("Category", "Golden")]
    [InlineData("SKCanvasView", "SKPaintSurfaceEventArgs")]
    [InlineData("SKGLView", "SKPaintGLSurfaceEventArgs")]
    public Task MatchesGolden(string view, string eventArgs) => TestBlazor(view, eventArgs, compareGolden: true);

    private async Task TestBlazor(string canvasView, string eventArgsType, bool compareGolden)
    {
        Output.WriteLine($"Testing SkiaSharp {SkiaVersion} in Blazor WASM ({canvasView})");
        var projectName = $"Blazor{canvasView}";
        var projectDir = Path.Combine(TestDir, projectName);

        // Run from TestDir (which has global.json) using relative paths. Pin the template
        // framework to keep the generated TFM aligned with the .NET 10 SDK band even when a
        // newer (e.g. net11.0) SDK is installed on the machine.
        await RunDotNet(["new", "blazorwasm", "-n", projectName, "-o", projectName, "-f", BaseFramework, "--no-https"]);
        await RunDotNet(["add", projectName, "package", "SkiaSharp.Views.Blazor", "--version", SkiaVersion]);

        await File.WriteAllTextAsync(Path.Combine(projectDir, "Pages", "Home.razor"), $$"""
            @page "/"
            @using SkiaSharp
            @using SkiaSharp.Views.Blazor

            <div id="canvas-container">
                <{{canvasView}} OnPaintSurface="OnPaintSurface"
                    WidthRequest="{{TestImage.Width}}" HeightRequest="{{TestImage.Height}}"
                    style="display:block;width:{{TestImage.Width}}px;height:{{TestImage.Height}}px;" />
            </div>
            <p id="render-status">@_status</p>

            @code {
                private string _status = "Not rendered";
                private void OnPaintSurface({{eventArgsType}} e)
                {
                    var canvas = e.Surface.Canvas;
                    {{TestImage.GetDrawCode()}}
                    _status = $"Rendered {e.Info.Width}x{e.Info.Height}";
                }
            }
            """);

        await RunDotNet(["build", projectName, "-c", "Release", "-p:WasmBuildNative=true"], timeoutSeconds: 300);

        await VerifyWithPlaywright(projectName, canvasView, compareGolden);
    }

    private async Task VerifyWithPlaywright(string projectName, string canvasView, bool compareGolden)
    {
        await using var server = await StartWebServer(Path.Combine(TestDir, projectName, $"{projectName}.csproj"));
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync(server.Url, new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
        await page.WaitForSelectorAsync("#render-status:has-text('Rendered')", new() { Timeout = 30000 });

        var canvasElement = await page.QuerySelectorAsync("#canvas-container canvas");
        Assert.NotNull(canvasElement);
        Assert.Equal(TestImage.Width, await canvasElement.EvaluateAsync<int>("canvas => canvas.width"));
        Assert.Equal(TestImage.Height, await canvasElement.EvaluateAsync<int>("canvas => canvas.height"));

        var screenshotBytes = await canvasElement.ScreenshotAsync();
        using var bitmap = SKBitmap.Decode(screenshotBytes);
        Assert.NotNull(bitmap);
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(200, 150));
        Assert.Equal(new SKColor(0, 150, 80), bitmap.GetPixel(320, 230));
        if (compareGolden)
            await VerifyScreenshot(screenshotBytes, $"blazor-{canvasView}", "blazor");
        else
            await SaveScreenshot(screenshotBytes, $"blazor-{canvasView}");
    }
}
