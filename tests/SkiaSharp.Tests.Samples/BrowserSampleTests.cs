using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Browser")]
public class BrowserSampleTests(ITestOutputHelper output) : SampleTestBase(output)
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(DotNet.Setting("SamplesDirectory"), SampleLookup.HostPlatform,
                AppContext.GetData("SampleTest.Filter") as string ?? "")
            .Where(sample => sample.Kind == SampleKind.Sample &&
                new[] { "Web", "BrowserWebAssembly", "BlazorWebAssembly" }.Any(name =>
                    sample.Folder == Path.Combine("Basic", name)))
            .Select(sample => new object[] { sample.Folder, sample.FileName });

    [Theory(DisableDiscoveryEnumeration = true, SkipTestWithoutData = true)]
    [MemberData(nameof(Cases))]
    public async Task WebSampleRendersInChromium(string folder, string solutionFile)
    {
        var profile = PrepareSample(folder);
        var samples = Path.Combine(profile.Root, "samples");
        var directory = Path.Combine(samples, folder);
        var diagnostics = Path.Combine(profile.DiagnosticsRoot, "browser", Path.GetFileName(folder));
        await profile.BuildSample(Path.Combine(directory, solutionFile), diagnostics, Output);
        await using var server = await profile.StartWebServer(
            Path.Combine(directory, "SkiaSharpSample", "SkiaSharpSample.csproj"), Output);
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();
        var errors = new List<string>();
        page.PageError += (_, error) =>
        {
            errors.Add(error);
            Output.WriteLine($"[browser error] {error}");
        };
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
                Output.WriteLine($"[browser console] {message.Text}");
        };
        await page.GotoAsync(server.Url, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        var name = Path.GetFileName(folder);
        if (name == "Web")
        {
            await page.WaitForFunctionAsync(
                "() => [...document.querySelectorAll('.card img')].length === 3 && [...document.querySelectorAll('.card img')].every(img => img.complete && img.naturalWidth === 512 && img.naturalHeight === 512)");
            var response = await page.APIRequest.GetAsync(server.Url + "/api/images/SkiaSharp");
            Assert.True(response.Ok, $"Image endpoint returned HTTP {response.Status}");
            var bytes = await response.BodyAsync();
            SampleImage.Validate(bytes, 512, 512);
            await File.WriteAllBytesAsync(Path.Combine(diagnostics, "output.png"), bytes, TestContext.Current.CancellationToken);
            await page.ScreenshotAsync(new() { Path = Path.Combine(diagnostics, "page.png") });
        }
        else if (name == "BrowserWebAssembly")
        {
            await page.WaitForFunctionAsync(
                "() => { const image = document.querySelector('#output'); return image && image.complete && image.naturalWidth === 800 && image.naturalHeight === 600; }",
                options: new() { Timeout = 60000 });
            var source = await page.Locator("#output").GetAttributeAsync("src");
            const string prefix = "data:image/png;base64,";
            Assert.NotNull(source);
            Assert.StartsWith(prefix, source);
            var bytes = Convert.FromBase64String(source[prefix.Length..]);
            SampleImage.Validate(bytes);
            await File.WriteAllBytesAsync(Path.Combine(diagnostics, "output.png"), bytes, TestContext.Current.CancellationToken);
        }
        else
        {
            await SaveRenderedCanvas(page, Path.Combine(diagnostics, "cpu.png"));
            await page.GotoAsync(server.Url + "/gpu");
            await SaveRenderedCanvas(page, Path.Combine(diagnostics, "gpu.png"));
        }
        Assert.Empty(errors);
    }

    private static async Task SaveRenderedCanvas(IPage page, string path)
    {
        var canvas = page.Locator(".canvas-container canvas");
        await canvas.WaitForAsync(new() { Timeout = 60000 });
        await page.WaitForFunctionAsync(
            "() => { const canvas = document.querySelector('.canvas-container canvas'); return canvas && canvas.width > 0 && canvas.height > 0; }");
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            var bytes = await canvas.ScreenshotAsync();
            using var bitmap = SKBitmap.Decode(bytes);
            Assert.NotNull(bitmap);
            var corner = bitmap.GetPixel(0, 0);
            if (Enumerable.Range(1, 4).Any(index =>
                bitmap.GetPixel(bitmap.Width * index / 5, bitmap.Height * index / 5) != corner))
            {
                await File.WriteAllBytesAsync(path, bytes, TestContext.Current.CancellationToken);
                return;
            }
            await Task.Delay(250, TestContext.Current.CancellationToken);
        }
        Assert.Fail($"Canvas did not render nonuniform scene pixels: {page.Url}");
    }
}
