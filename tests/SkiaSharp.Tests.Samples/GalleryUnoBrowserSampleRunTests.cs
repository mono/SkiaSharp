using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using System.Text.RegularExpressions;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleRun")]
[Trait("Category", "Host")]
[Trait("Category", "Browser")]
[Trait("Category", "GalleryBrowser")]
[RunGroup(SampleRunGroup.Host)]
[Collection(nameof(BrowserSampleCollection))]
public class GalleryUnoBrowserSampleRunTests : DotNetSampleTestBase
{
    private readonly HashSet<string> captures = new();

    [Fact]
    public async Task CatalogSearchAndGradientControlsRender()
    {
        const string framework = "net10.0-browserwasm";
        const string projectRelative = "Uno/SkiaSharpSample.Uno.csproj";
        var options = new DotNetOptions { Framework = framework };
        var workspace = await BuildSample("Gallery", projectRelative, options);
        var project = Path.Combine(workspace.Root, "samples", "Gallery", projectRelative);
        await DotNet.Run(workspace, project, ["--urls", "http://127.0.0.1:0"],
            host => BrowserRunningApp.Run(host, async app =>
            {
                await app.Navigate("/");
                await app.WaitForElement("#uno-canvas");
                await app.Page.Locator(".uno-loader").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
                await UnoBrowser.EnableAccessibility(app);
                try
                {
                    var search = app.Page.GetByRole(AriaRole.Textbox,
                        new() { Name = "Search samples...", Exact = true });
                    var links = app.Page.Locator(
                        "#uno-semantics-root a[aria-label]:not([aria-label='SkiaSharp Gallery']):not([aria-label='GitHub']):not([aria-label*='Back to Gallery'])");
                    var gradient = app.Page.Locator("#uno-semantics-root a[aria-label='Gradient']");
                    await search.WaitForAsync();
                    await Assertions.Expect(gradient).ToHaveCountAsync(1);
                    var count = await links.CountAsync();
                    Assert.True(count > 10, "The real Gallery catalog must load before filtering.");
                    var shaders = app.Page.GetByRole(AriaRole.Button,
                        new() { NameRegex = new Regex("^● Shaders & Effects ") });
                    var label = await shaders.GetAttributeAsync("aria-label");
                    Assert.NotNull(label);
                    var shaderCount = int.Parse(Regex.Match(label, @"\d+$").Value);
                    await UnoBrowser.Click(app, shaders);
                    await Assertions.Expect(links).ToHaveCountAsync(shaderCount);
                    await UnoBrowser.Click(app, app.Page.GetByRole(AriaRole.Button,
                        new() { NameRegex = new Regex("^All categories ") }));
                    await Assertions.Expect(links).ToHaveCountAsync(count);

                    await SetSearch(app, search, "Gradient");
                    await gradient.WaitForAsync();
                    Assert.InRange(await links.CountAsync(), 1, count - 1);
                    await Capture(app);

                    await SetSearch(app, search, "no-such-gallery-sample-42");
                    await Assertions.Expect(links).ToHaveCountAsync(0);
                    await UnoBrowser.Click(app, app.Page.GetByRole(AriaRole.Button,
                        new() { Name = "Reset filters", Exact = true }));
                    await Assertions.Expect(search).ToHaveValueAsync("");
                    await Assertions.Expect(links).ToHaveCountAsync(count);
                    await SetSearch(app, search, "Gradient");
                    await UnoBrowser.Click(app, gradient);
                    var linear = await WaitForGradient(app, radial: false);
                    var settledLinear = await WaitForStablePage(app);

                    var picker = app.Page.GetByRole(AriaRole.Combobox,
                        new() { Name = "Gradient Type", Exact = true });
                    await picker.FocusAsync();
                    await picker.PressAsync("Home");
                    await picker.PressAsync("ArrowDown");
                    await WaitForGradient(app, radial: true, changedFrom: linear);
                    var settledRadial = await WaitForStablePage(app);
                    var back = app.Page.Locator("#uno-semantics-root a[aria-label*='Back to Gallery']");
                    await back.FocusAsync();
                    await back.PressAsync("Enter");
                    await Assertions.Expect(search).ToHaveValueAsync("");
                    await Assertions.Expect(links).ToHaveCountAsync(count);
                    await Capture(app);

                    captures.Remove(settledLinear);
                    SampleImage.Validate(settledLinear, $"Host/{SampleLookup.HostPlatform}/gallery-uno-gradient-linear.png");
                    captures.Remove(settledRadial);
                    SampleImage.Validate(settledRadial, $"Host/{SampleLookup.HostPlatform}/gallery-uno-gradient-radial.png");
                }
                finally
                {
                    var dom = Path.Combine(app.HostApp.Diagnostics, "catalog.html");
                    await File.WriteAllTextAsync(dom, await app.Page.ContentAsync());
                    TestContext.Current.AddFileAttachment(dom, "text/html");
                    foreach (var capture in captures)
                        TestContext.Current.AddFileAttachment(capture, "image/png");
                }
            }), options);
    }

    private static async Task SetSearch(BrowserRunningApp app, ILocator search, string text)
    {
        await UnoBrowser.Click(app, search);
        await app.Page.Keyboard.PressAsync("ControlOrMeta+A");
        await app.Page.Keyboard.InsertTextAsync(text);
        await Assertions.Expect(search).ToHaveValueAsync(text);
    }

    private async Task<string> Capture(BrowserRunningApp app)
    {
        var path = await app.Screenshot();
        captures.Add(path);
        return path;
    }

    private async Task<string> WaitForStablePage(BrowserRunningApp app)
    {
        await app.Page.Mouse.MoveAsync(0, 0);
        var before = await Capture(app);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            await Task.Delay(1100, TestContext.Current.CancellationToken);
            var after = await Capture(app);
            using var first = SKImage.FromEncodedData(before);
            using var second = SKImage.FromEncodedData(after);
            Assert.NotNull(first);
            Assert.NotNull(second);
            var difference = SkiaSharp.Extended.SKPixelComparer.Compare(first, second, 0);
            TestContext.Current.TestOutputHelper?.WriteLine(
                $"Uno full-page settling: {difference.ErrorPixelCount}/{difference.TotalPixels} differing pixels; {before}; {after}.");
            if (difference.ErrorPixelCount <= difference.TotalPixels * SampleImageOptions.DefaultMaxErrorPixelFraction)
                return after;
            before = after;
        }
        throw new TimeoutException("Gallery Uno's full page did not settle after native control transitions.");
    }

    private async Task<string> WaitForGradient(BrowserRunningApp app, bool radial, string? changedFrom = null)
    {
        using var before = changedFrom is null ? null : SKBitmap.Decode(changedFrom);
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            var screenshot = await Capture(app);
            using var bitmap = SKBitmap.Decode(screenshot);
            Assert.NotNull(bitmap);
            var blue = 0;
            var pink = 0;
            var changed = 0;
            for (var y = 220; y < 825; y++)
                for (var x = 24; x < 942; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.Blue > 200 && color.Red < 100 && color.Green > 105)
                        blue++;
                    if (color.Red > 200 && color.Green < 110 && color.Blue > 140)
                        pink++;
                    if (before is not null && color != before.GetPixel(x, y))
                        changed++;
                }
            var center = bitmap.GetPixel(483, 522);
            var correctCenter = radial
                ? center.Red < 100 && center.Green > 110 && center.Blue > 200
                : center.Red > 110 && center.Red < 180 && center.Green < 115 && center.Blue > 200;
            if (correctCenter && blue > 5000 && pink > 5000 && (before is null || changed > 10000))
            {
                TestContext.Current.TestOutputHelper?.WriteLine(
                    $"Uno Gradient {(radial ? "radial" : "linear")}: {blue} blue, {pink} pink, {changed} changed pixels; {screenshot}.");
                return screenshot;
            }
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }
        throw new TimeoutException($"Gallery Uno did not render its {(radial ? "radial" : "linear")} Gradient.");
    }
}
