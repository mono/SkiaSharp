using System.Diagnostics;
using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleRun")]
[Trait("Category", "Browser")]
public class BrowserSampleTests : SampleTestBase
{
    // At 1280x900 the responsive drawer is 240px and the app bar is 64px.
    // MainLayout fills 100vh; its remaining flex child is the canvas container.
    private const int CanvasWidth = 1040;
    private const int CanvasHeight = 836;

    [Fact]
    public Task WebSampleRendersImages() =>
        RunSample("Web", "/", async (app, page, diagnostics) =>
        {
            var cancellation = TestContext.Current.CancellationToken;
            await page.WaitForFunctionAsync(
                "() => { const images = [...document.querySelectorAll('.card img')]; " +
                "return images.length === 3 && images.every(img => img.complete && img.naturalWidth === 512 && img.naturalHeight === 512); }")
                .WaitAsync(cancellation);

            var images = page.Locator(".card img");
            Assert.Equal(3, await images.CountAsync().WaitAsync(cancellation));
            string[] names = ["web-text", "web-generated", "web-custom"];
            var address = await app.GetAddress();
            for (var index = 0; index < names.Length; index++)
            {
                var source = await images.Nth(index).EvaluateAsync<string>("img => img.src").WaitAsync(cancellation);
                var uri = new Uri(source);
                Assert.Equal(RunningApp.RequestUri(address, uri.PathAndQuery), uri);
                using var response = await app.GetResponse(uri.PathAndQuery);
                Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
                Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
                await RetainImage(await response.Content.ReadAsByteArrayAsync(cancellation),
                    Path.Combine(diagnostics, names[index] + ".actual.png"));
            }

            // Retain all three actual UI outputs before a missing golden can fail the test.
            foreach (var name in names)
            {
                var golden = name == "web-text"
                    ? $"Host/{SampleLookup.HostPlatform}/web.png"
                    : $"Browser/{SampleLookup.HostPlatform}/{name}.png";
                CompareGolden(Path.Combine(diagnostics, name + ".actual.png"), 512, 512, golden);
            }
        });

    [Fact]
    public Task BrowserWebAssemblySampleRendersImage() =>
        RunSample("BrowserWebAssembly", "/", async (_, page, diagnostics) =>
        {
            var cancellation = TestContext.Current.CancellationToken;
            await page.WaitForFunctionAsync(
                "() => { const image = document.querySelector('#output'); " +
                "return image && image.complete && image.naturalWidth === 800 && image.naturalHeight === 600; }")
                .WaitAsync(cancellation);
            var source = await page.Locator("#output").GetAttributeAsync("src").WaitAsync(cancellation);
            const string prefix = "data:image/png;base64,";
            Assert.NotNull(source);
            Assert.StartsWith(prefix, source);
            var actual = Path.Combine(diagnostics, "browser-wasm.actual.png");
            var bytes = Convert.FromBase64String(source[prefix.Length..]);
            await RetainImage(bytes, actual);
            CompareGolden(actual, 800, 600, $"Browser/{SampleLookup.HostPlatform}/browser-wasm.png");
        });

    [Fact]
    public Task BlazorWebAssemblySampleRendersCpuCanvas() =>
        RunSample("BlazorWebAssembly", "/", async (_, page, diagnostics) =>
        {
            var actual = Path.Combine(diagnostics, "blazor-cpu.actual.png");
            await SaveRenderedCanvas(page, actual, stable: true);
            CompareGolden(actual, CanvasWidth, CanvasHeight, $"Browser/{SampleLookup.HostPlatform}/blazor-cpu.png");
        });

    [Fact]
    public Task BlazorWebAssemblySampleRendersGpuCanvas() =>
        RunSample("BlazorWebAssembly", "/gpu", async (_, page, diagnostics) =>
        {
            var actual = Path.Combine(diagnostics, "blazor-gpu.png");
            await SaveRenderedCanvas(page, actual, stable: false);
            SampleImage.Validate(await File.ReadAllBytesAsync(actual, TestContext.Current.CancellationToken),
                CanvasWidth, CanvasHeight);
        });

    private async Task RunSample(string name, string path, Func<RunningApp, IPage, string, Task> test)
    {
        var folder = Path.Combine("Basic", name);
        var workspace = await BuildSample(folder, "SkiaSharpSample.slnx", "Release");
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        await DotNet.Run(workspace, project, "Release", ["--urls", "http://127.0.0.1:0"],
            app => BrowserSampleApp.Run(app, path, (page, diagnostics) => test(app, page, diagnostics)));
    }

    private static void CompareGolden(string actual, int width, int height, string golden)
    {
        SampleImage.Validate(File.ReadAllBytes(actual), width, height);
        SampleImage.CompareGolden(actual, Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples",
            "Expected", golden));
    }

    private static async Task RetainImage(byte[] bytes, string path)
    {
        await File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
        TestContext.Current.AddFileAttachment(path, "image/png");
    }

    private static async Task SaveRenderedCanvas(IPage page, string path, bool stable)
    {
        var cancellation = TestContext.Current.CancellationToken;
        var canvas = page.Locator(".canvas-container canvas");
        await canvas.WaitForAsync(new() { State = WaitForSelectorState.Visible }).WaitAsync(cancellation);
        await page.WaitForFunctionAsync(
            "() => { const canvas = document.querySelector('.canvas-container canvas'); " +
            "return canvas && canvas.width > 0 && canvas.height > 0; }").WaitAsync(cancellation);

        var timer = Stopwatch.StartNew();
        byte[]? previous = null;
        byte[]? last = null;
        try
        {
            while (timer.Elapsed < TimeSpan.FromSeconds(60))
            {
                cancellation.ThrowIfCancellationRequested();
                last = await canvas.ScreenshotAsync(new() { Timeout = 10000 }).WaitAsync(cancellation);
                using var bitmap = SKBitmap.Decode(last);
                Assert.NotNull(bitmap);
                if (bitmap.Width == CanvasWidth && bitmap.Height == CanvasHeight && HasScenePixels(bitmap))
                {
                    if (!stable || (previous is not null && previous.AsSpan().SequenceEqual(last)))
                        return;
                    previous = last;
                }
                else
                {
                    previous = null;
                }
                await Task.Delay(250, cancellation);
            }
            Assert.Fail($"Canvas did not render a {(stable ? "stable " : "")}nonuniform {CanvasWidth}x{CanvasHeight} scene: {page.Url}");
        }
        finally
        {
            // Keep the last canvas even when readiness, dimensions or pixel checks fail.
            if (last is not null)
            {
                await File.WriteAllBytesAsync(path, last);
                TestContext.Current.AddFileAttachment(path, "image/png");
            }
        }
    }

    private static bool HasScenePixels(SKBitmap bitmap)
    {
        var first = bitmap.GetPixel(bitmap.Width / 16, bitmap.Height / 16);
        var different = 0;
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                var pixel = bitmap.GetPixel(bitmap.Width * (2 * x + 1) / 16, bitmap.Height * (2 * y + 1) / 16);
                if (pixel.Alpha > 0 &&
                    Math.Max(Math.Abs(pixel.Red - first.Red),
                        Math.Max(Math.Abs(pixel.Green - first.Green), Math.Abs(pixel.Blue - first.Blue))) > 12)
                    different++;
            }
        }
        return different >= 16;
    }
}
