using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using System.IO.Compression;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Host)]
[Collection(nameof(BrowserSampleCollection))]
public class GalleryBlazorBrowserSampleRunTests : DotNetSampleTestBase
{
    private readonly HashSet<string> pendingCaptures = new();

    private async Task RunGallery(Func<BrowserRunningApp, Task> test)
    {
        const string projectRelative = "Blazor/SkiaSharpSample.Blazor.csproj";
        var options = new DotNetOptions { Framework = "net10.0" };
        var workspace = await BuildSample("Gallery", projectRelative, options);
        var project = Path.Combine(workspace.Root, "samples", "Gallery", projectRelative);
        await DotNet.Run(workspace, project, ["--urls", "http://127.0.0.1:0"],
            app => BrowserRunningApp.Run(app, async browser =>
            {
                try
                {
                    await test(browser);
                }
                finally
                {
                    // Validate attaches its own actual; retain every other capture even on an early failure.
                    foreach (var capture in pendingCaptures)
                        TestContext.Current.AddFileAttachment(capture, "image/png");
                    pendingCaptures.Clear();
                }
            }), options);
    }

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    [Trait("Category", "GalleryBrowser")]
    public Task CatalogSearchResetAndNavigationWork() =>
        RunGallery(async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".sample-grid");
            await SelectBackend(app, "CPU");
            var search = app.Page.Locator(".sidebar-desktop")
                .GetByPlaceholder("Search samples, APIs…", new() { Exact = true });
            var sort = app.Page.Locator(".results-sort select");
            var titles = app.Page.Locator(".sample-grid .card-title");
            await sort.SelectOptionAsync("OldestFirst");
            await Assertions.Expect(sort).ToHaveValueAsync("OldestFirst");
            var originalTitles = await titles.AllTextContentsAsync();
            Assert.True(originalTitles.Count > 10, "The generated app must load the real Gallery catalog.");
            var shared = XDocument.Load(Path.Combine(Repo.SamplesDir, "Gallery", "Shared", "SkiaSharpSample.Shared.csproj"));
            foreach (var (id, index) in new[] { ("SkiaSharp", 0), ("HarfBuzzSharp", 1) })
            {
                var reference = Assert.Single(shared.Descendants("PackageReference"),
                    element => element.Attribute("Include")?.Value == id);
                var version = reference.Attribute("Version")!.Value;
                using var archive = ZipFile.OpenRead(Path.Combine(Repo.PackagesDir, $"{id}.{version}.nupkg"));
                var entry = archive.GetEntry($"{id}.nuspec");
                Assert.NotNull(entry);
                using var stream = entry.Open();
                var metadata = XDocument.Load(stream);
                var repository = Assert.Single(metadata.Descendants(),
                    element => element.Name.LocalName == "repository");
                var commit = repository.Attribute("commit");
                Assert.NotNull(commit);
                await Assertions.Expect(app.Page.Locator(".gallery-footer strong").Nth(index))
                    .ToContainTextAsync(version.Split('-')[0]);
                await Assertions.Expect(app.Page.Locator(".gallery-footer strong").Nth(index))
                    .ToContainTextAsync(commit.Value);
            }
            Assert.True(DateTimeOffset.TryParse(await app.Page.Locator(".gallery-footer time").GetAttributeAsync("datetime"),
                out var built));
            Assert.Equal(TimeSpan.Zero, built.Offset);
            var catalogCapture = await CaptureRepeated(app, "catalog, Oldest");

            await search.FillAsync("Gradient");
            await Assertions.Expect(app.Page.Locator(".chip-search")).ToContainTextAsync("Gradient");
            var gradient = app.Page.Locator(".grid-link[href='sample/Gradient']");
            await Assertions.Expect(gradient).ToBeVisibleAsync();
            Assert.InRange(await titles.CountAsync(), 1, originalTitles.Count - 1);

            await search.FillAsync("SKShader.CreateRadialGradient");
            await Assertions.Expect(app.Page.Locator(".chip-search")).ToContainTextAsync("SKShader.CreateRadialGradient");
            await Assertions.Expect(gradient).ToBeVisibleAsync();
            Assert.InRange(await titles.CountAsync(), 1, originalTitles.Count - 1);

            await search.FillAsync("no-such-gallery-sample-42");
            await Assertions.Expect(app.Page.Locator(".empty-state h3")).ToHaveTextAsync("No samples match");
            await Assertions.Expect(titles).ToHaveCountAsync(0);
            await app.Page.GetByRole(AriaRole.Button, new() { Name = "Reset all filters", Exact = true }).ClickAsync();
            await Assertions.Expect(search).ToHaveValueAsync("");
            await Assertions.Expect(app.Page.Locator(".active-filters")).ToHaveCountAsync(0);
            await Assertions.Expect(sort).ToHaveValueAsync("NewestFirst");
            await Assertions.Expect(titles).ToHaveCountAsync(originalTitles.Count);

            // Reset restores Featured; choose the deterministic order again without changing app defaults.
            await sort.SelectOptionAsync("OldestFirst");
            await Assertions.Expect(sort).ToHaveValueAsync("OldestFirst");
            Assert.Equal(originalTitles.ToArray(), (await titles.AllTextContentsAsync()).ToArray());
            await CaptureRepeated(app, "catalog after reset, Oldest");

            await search.FillAsync("Gradient");
            await Assertions.Expect(app.Page.Locator(".chip-search")).ToContainTextAsync("Gradient");
            await SelectBackend(app, "CPU");
            await gradient.ClickAsync();
            await Assertions.Expect(app.Page.Locator(".sample-header h2")).ToHaveTextAsync("Gradient");
            await WaitForGradient(app, radial: false);
            await app.Page.GetByRole(AriaRole.Link, new() { Name = "Back to Gallery" }).ClickAsync();
            await app.WaitForElement(".sample-grid");
            await Assertions.Expect(search).ToHaveValueAsync("");
            await Assertions.Expect(sort).ToHaveValueAsync("NewestFirst");
            await Assertions.Expect(titles).ToHaveCountAsync(originalTitles.Count);
            await sort.SelectOptionAsync("OldestFirst");
            await Assertions.Expect(sort).ToHaveValueAsync("OldestFirst");
            Assert.Equal(originalTitles.ToArray(), (await titles.AllTextContentsAsync()).ToArray());
            var returned = await CaptureRepeated(app, "catalog after Back to Gallery, Oldest");
            ReportDifference("Catalog navigation round trip", catalogCapture, returned);
            pendingCaptures.Remove(catalogCapture);
            // The reviewed catalog-only allowance includes real build footer metadata; scene captures stay strict.
            SampleImage.Validate(catalogCapture, $"Host/{SampleLookup.HostPlatform}/gallery-blazor-catalog.png",
                new() { MaxErrorPixelFraction = 0.01m });
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    [Trait("Category", "GalleryBrowser")]
    public Task GradientCpuControlsChangeTheScene() =>
        RunGallery(async app =>
        {
            await OpenGradientOnCpu(app);
            var controls = app.Page.Locator(".control-panel select");
            await Assertions.Expect(controls).ToHaveCountAsync(2);
            await Assertions.Expect(controls.Nth(0).Locator("option")).ToHaveTextAsync(
                new[] { "Linear", "Radial", "Sweep", "Two-Point Conical" });
            await Assertions.Expect(controls.Nth(1).Locator("option")).ToHaveTextAsync(
                new[] { "Clamp", "Repeat", "Mirror" });
            await Assertions.Expect(controls.Nth(0)).ToHaveValueAsync("0");
            await Assertions.Expect(controls.Nth(1)).ToHaveValueAsync("0");
            var linear = await WaitForGradient(app, radial: false);
            var settledLinear = await CaptureRepeated(app, "Gradient CPU linear", linear);

            await controls.Nth(0).SelectOptionAsync("1");
            await Assertions.Expect(controls.Nth(0)).ToHaveValueAsync("1");
            var radial = await WaitForGradient(app, radial: true, changedFrom: linear);
            var settledRadial = await CaptureRepeated(app, "Gradient CPU radial", radial);

            await controls.Nth(1).SelectOptionAsync("1");
            await Assertions.Expect(controls.Nth(1)).ToHaveValueAsync("1");
            var repeat = await WaitForGradient(app, radial: true, changedFrom: radial);
            await controls.Nth(1).SelectOptionAsync("0");
            await Assertions.Expect(controls.Nth(1)).ToHaveValueAsync("0");
            var restored = await WaitForGradient(app, radial: true, changedFrom: repeat);
            ReportDifference("Gradient CPU radial after restoring Clamp", radial, restored);

            ValidateCapture(settledLinear, $"Host/{SampleLookup.HostPlatform}/gallery-blazor-gradient-cpu-linear.png");
            ValidateCapture(settledRadial, $"Host/{SampleLookup.HostPlatform}/gallery-blazor-gradient-cpu-radial.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    [Trait("Category", "GalleryBrowser")]
    public Task GradientRadialSwitchesBetweenCpuAndGpu() =>
        RunGallery(async app =>
        {
            await OpenGradientOnCpu(app);
            var gradientType = app.Page.Locator(".control-panel select").Nth(0);
            var linear = await WaitForGradient(app, radial: false);
            await gradientType.SelectOptionAsync("1");
            await Assertions.Expect(gradientType).ToHaveValueAsync("1");
            var cpu = await WaitForGradient(app, radial: true, changedFrom: linear);
            await CaptureRepeated(app, "Gradient CPU radial before backend switch", cpu);

            await SelectBackend(app, "GPU");
            await Assertions.Expect(gradientType).ToHaveValueAsync("1");
            var gpu = await WaitForGradient(app, radial: true);
            var settledGpu = await CaptureRepeated(app, "Gradient GPU radial", gpu);
            ReportDifference("Gradient radial CPU versus GPU", cpu, gpu);

            await SelectBackend(app, "CPU");
            await Assertions.Expect(gradientType).ToHaveValueAsync("1");
            var returned = await WaitForGradient(app, radial: true);
            await CaptureRepeated(app, "Gradient CPU radial after backend switch", returned);
            ReportDifference("Gradient radial CPU round trip", cpu, returned);

            ValidateCapture(settledGpu, $"Host/{SampleLookup.HostPlatform}/gallery-blazor-gradient-gpu-radial.png");
        });

    private static async Task OpenGradientOnCpu(BrowserRunningApp app)
    {
        await app.Navigate("/");
        await app.WaitForElement(".sample-grid");
        await Assertions.Expect(app.Page.GetByRole(AriaRole.Button, new() { Name = "GPU", Exact = true }))
            .ToHaveClassAsync("btn btn-secondary btn-sm");
        await SelectBackend(app, "CPU");
        await app.Page.Locator(".sidebar-desktop")
            .GetByPlaceholder("Search samples, APIs…", new() { Exact = true }).FillAsync("Gradient");
        await Assertions.Expect(app.Page.Locator(".chip-search")).ToContainTextAsync("Gradient");
        await app.Page.Locator(".grid-link[href='sample/Gradient']").ClickAsync();
        await Assertions.Expect(app.Page.Locator(".sample-header h2")).ToHaveTextAsync("Gradient");
        await app.WaitForElement(".sample-canvas canvas");
    }

    private static async Task SelectBackend(BrowserRunningApp app, string backend)
    {
        var button = app.Page.GetByRole(AriaRole.Button, new() { Name = backend, Exact = true });
        await button.ClickAsync();
        await Assertions.Expect(button).ToHaveClassAsync("btn btn-secondary btn-sm");
        await Assertions.Expect(app.Page.GetByRole(AriaRole.Button,
            new() { Name = backend == "CPU" ? "GPU" : "CPU", Exact = true }))
            .ToHaveClassAsync("btn btn-outline-secondary btn-sm");
    }

    private async Task<string> Capture(BrowserRunningApp app)
    {
        var screenshot = await app.Screenshot();
        pendingCaptures.Add(screenshot);
        return screenshot;
    }

    private void ValidateCapture(string actual, string golden)
    {
        pendingCaptures.Remove(actual);
        SampleImage.Validate(actual, golden);
    }

    private async Task<string> CaptureRepeated(BrowserRunningApp app, string scene, string? first = null)
    {
        await app.Page.Mouse.MoveAsync(0, 0);
        await app.Page.WaitForFunctionAsync("""
            () => document.getAnimations().every(a => a.playState === 'finished' || a.playState === 'idle')
            """);
        first ??= await Capture(app);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            await Task.Delay(200, TestContext.Current.CancellationToken);
            var repeated = await Capture(app);
            ReportDifference($"{scene}, repeated full-page capture", first, repeated);
            using var before = SKImage.FromEncodedData(first);
            using var after = SKImage.FromEncodedData(repeated);
            Assert.NotNull(before);
            Assert.NotNull(after);
            var difference = SkiaSharp.Extended.SKPixelComparer.Compare(before, after, 0);
            if (difference.ErrorPixelCount <= difference.TotalPixels * SampleImageOptions.DefaultMaxErrorPixelFraction)
                return repeated;
            first = repeated;
        }
        throw new TimeoutException($"Blazor Gallery's full page did not settle: {scene}.");
    }

    private static void ReportDifference(string scene, string first, string second)
    {
        using var before = SKImage.FromEncodedData(first);
        using var after = SKImage.FromEncodedData(second);
        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.Equal(before.Width, after.Width);
        Assert.Equal(before.Height, after.Height);
        var difference = SkiaSharp.Extended.SKPixelComparer.Compare(before, after, 0);
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"{scene}: {difference.ErrorPixelCount}/{difference.TotalPixels} pixels differ " +
            $"({(double)difference.ErrorPixelCount / difference.TotalPixels:P6}); " +
            $"average RGBA error {difference.AbsoluteError / (difference.TotalPixels * 4.0 * 255):P6}. " +
            $"Captures: {first}; {second}.");
    }

    private async Task<string> WaitForGradient(BrowserRunningApp app, bool radial, string? changedFrom = null)
    {
        await app.WaitForElement(".sample-canvas canvas");
        await app.Page.Mouse.MoveAsync(0, 0);
        using var before = changedFrom is null ? null : SKBitmap.Decode(changedFrom);
        if (changedFrom is not null)
            Assert.NotNull(before);
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var bounds = await app.Page.Locator(".sample-canvas canvas").EvaluateAsync<double[]>("""
                c => {
                    const r = c.getBoundingClientRect();
                    return [r.x + window.scrollX, r.y + window.scrollY, r.width, r.height];
                }
                """);
            var screenshot = await Capture(app);
            using var bitmap = SKBitmap.Decode(screenshot);
            Assert.NotNull(bitmap);
            var region = new SKRectI((int)Math.Ceiling(bounds[0]), (int)Math.Ceiling(bounds[1]),
                (int)Math.Floor(bounds[0] + bounds[2]), (int)Math.Floor(bounds[1] + bounds[3]));
            Assert.True(region.Width > 100 && region.Height > 100, "Gradient canvas must have usable dimensions.");
            Assert.InRange(region.Left, 0, bitmap.Width - 1);
            Assert.InRange(region.Top, 0, bitmap.Height - 1);
            Assert.InRange(region.Right, region.Left + 1, bitmap.Width);
            Assert.InRange(region.Bottom, region.Top + 1, bitmap.Height);
            var colored = 0;
            var blue = 0;
            var pink = 0;
            var changed = 0;
            for (var y = region.Top; y < region.Bottom; y++)
                for (var x = region.Left; x < region.Right; x++)
                {
                    var color = bitmap.GetPixel(x, y);
                    if (color.Blue > 140 && color.Blue - color.Green > 40)
                        colored++;
                    if (color.Blue > 200 && color.Red < 100 && color.Green > 105)
                        blue++;
                    if (color.Red > 200 && color.Green < 110 && color.Blue > 140)
                        pink++;
                    if (before is not null && before.GetPixel(x, y) != color)
                        changed++;
                }
            var center = bitmap.GetPixel(region.Left + region.Width / 2, region.Top + region.Height / 2);
            var correctCenter = radial
                ? center.Red < 90 && center.Green > 110 && center.Blue > 200
                : center.Red > 110 && center.Red < 180 && center.Green < 115 && center.Blue > 200;
            var area = (long)region.Width * region.Height;
            if (correctCenter && colored > area / 10 && blue > area / 100 && pink > area / 100 &&
                (before is null || changed > area / 100))
            {
                TestContext.Current.TestOutputHelper?.WriteLine(
                    $"Gradient {(radial ? "radial" : "linear")}: {colored} colored, {blue} blue, {pink} pink " +
                    $"and {changed} changed pixels in {region}; center {center}; capture {screenshot}.");
                return screenshot;
            }
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }
        Assert.Fail($"Gradient {(radial ? "radial" : "linear")} did not render its colored scene" +
            (changedFrom is null ? "." : $" with a meaningful canvas change from {changedFrom}."));
        throw new InvalidOperationException("Unreachable.");
    }
}
