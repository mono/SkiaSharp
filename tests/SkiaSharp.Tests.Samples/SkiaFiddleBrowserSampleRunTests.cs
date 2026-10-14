using Microsoft.Playwright;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleRun")]
[Trait("Category", "Host")]
[Trait("Category", "Browser")]
[Trait("Category", "FiddleBrowser")]
[RunGroup(SampleRunGroup.Host)]
[Collection(nameof(BrowserSampleCollection))]
public class SkiaFiddleBrowserSampleRunTests : DotNetSampleTestBase
{
    private readonly HashSet<string> captures = new();

    [Fact]
    public async Task BundledSnippetCompilesAndRecoversFromEditorError()
    {
        const string framework = "net10.0-browserwasm";
        const string projectRelative = "SkiaFiddle.csproj";
        var options = new DotNetOptions { Framework = framework };
        var workspace = await BuildSample("SkiaFiddle", projectRelative, options);
        var project = Path.Combine(workspace.Root, "samples", "SkiaFiddle", projectRelative);
        await DotNet.Run(workspace, project, ["--urls", "http://127.0.0.1:0"],
            host => BrowserRunningApp.Run(host, async app =>
            {
                await app.Navigate("/");
                await app.WaitForElement("#uno-canvas");
                await app.Page.Locator(".uno-loader").WaitForAsync(new() { State = WaitForSelectorState.Hidden });
                await app.WaitForElement(".monaco-editor");
                await UnoBrowser.EnableAccessibility(app);
                try
                {
                    var sample = app.Page.GetByRole(AriaRole.Combobox, new() { Name = "Sample", Exact = true });
                    await UnoBrowser.Click(app, sample);
                    await UnoBrowser.Click(app,
                        app.Page.GetByRole(AriaRole.Option, new() { Name = "Static · Hello world", Exact = true }));
                    await app.Page.WaitForFunctionAsync("""
                        () => JSON.parse(globalThis.skiaFiddleGetMonacoValues())[1]?.includes('Hello SkiaFiddle')
                        """);
                    var original = (await EditorValues(app))[1];
                    var rendered = await WaitForOutput(app, error: false);
                    await Assertions.Expect(app.Page.Locator("#uno-semantics-root [aria-label='engine: Roslyn (browser-wasm)']"))
                        .ToHaveCountAsync(1);
                    await Assertions.Expect(app.Page.Locator("#uno-semantics-root [aria-label^='Compiled in ']"))
                        .ToHaveCountAsync(1);
                    await app.Page.GetByRole(AriaRole.Button, new() { Name = "Run", Exact = true }).FocusAsync();
                    await app.Page.Mouse.MoveAsync(0, 0);
                    var first = await WaitForStablePage(app, "Static snippet after UI transitions");
                    await Task.Delay(1100, TestContext.Current.CancellationToken);
                    var repeated = await Capture(app);
                    ReportDifference("Static snippet repeated capture", first, repeated);

                    var editor = app.Page.Locator(".monaco-editor").Nth(1);
                    await editor.ClickAsync(new() { Position = new() { X = 150, Y = 40 } });
                    await app.Page.Keyboard.PressAsync("ControlOrMeta+A");
                    await app.Page.Keyboard.InsertTextAsync("canvas.NoSuchFiddleMethod();");
                    await app.Page.WaitForFunctionAsync("""
                        () => JSON.parse(globalThis.skiaFiddleGetMonacoValues())[1] === 'canvas.NoSuchFiddleMethod();'
                        """);
                    await UnoBrowser.Click(app,
                        app.Page.GetByRole(AriaRole.Button, new() { Name = "Run", Exact = true }));
                    var failed = await WaitForOutput(app, error: true);
                    await Assertions.Expect(app.Page.Locator("#uno-semantics-root [aria-label*='NoSuchFiddleMethod']"))
                        .ToHaveCountAsync(1);

                    await editor.ClickAsync(new() { Position = new() { X = 150, Y = 40 } });
                    await app.Page.Keyboard.PressAsync("ControlOrMeta+A");
                    await app.Page.Keyboard.InsertTextAsync(original);
                    await app.Page.WaitForFunctionAsync("""
                        expected => {
                            const lines = code => code.split(/\r?\n/).map(line => line.trim()).join('\n');
                            return lines(JSON.parse(globalThis.skiaFiddleGetMonacoValues())[1]) === lines(expected);
                        }
                        """, original);
                    await app.Page.Keyboard.PressAsync("ControlOrMeta+Home");
                    await UnoBrowser.Click(app,
                        app.Page.GetByRole(AriaRole.Button, new() { Name = "Run", Exact = true }));
                    var recovered = await WaitForOutput(app, error: false);
                    await Assertions.Expect(app.Page.Locator("#uno-semantics-root [aria-label^='Compiled in ']"))
                        .ToHaveCountAsync(1);
                    await app.Page.GetByRole(AriaRole.Button, new() { Name = "Run", Exact = true }).FocusAsync();
                    await app.Page.Mouse.MoveAsync(0, 0);
                    var final = await WaitForStablePage(app, "Recovered snippet after UI transitions");
                    ReportDifference("Static snippet after Roslyn error recovery", first, final);
                    ReportDifference("Compiled scene versus compile-error scene", rendered, failed);
                    captures.Remove(first);
                    SampleImage.Validate(first, $"Host/{SampleLookup.HostPlatform}/fiddle-static-page.png");
                }
                finally
                {
                    var dom = Path.Combine(app.HostApp.Diagnostics, "fiddle.html");
                    await File.WriteAllTextAsync(dom, await app.Page.ContentAsync());
                    TestContext.Current.AddFileAttachment(dom, "text/html");
                    var values = Path.Combine(app.HostApp.Diagnostics, "editor-values.json");
                    await File.WriteAllTextAsync(values, System.Text.Json.JsonSerializer.Serialize(await EditorValues(app)));
                    TestContext.Current.AddFileAttachment(values, "application/json");
                    foreach (var capture in captures)
                        TestContext.Current.AddFileAttachment(capture, "image/png");
                }
            }), options);
    }

    private static Task<string[]> EditorValues(BrowserRunningApp app) =>
        app.Page.EvaluateAsync<string[]>("JSON.parse(globalThis.skiaFiddleGetMonacoValues())");

    private async Task<string> Capture(BrowserRunningApp app)
    {
        var screenshot = await app.Screenshot();
        captures.Add(screenshot);
        return screenshot;
    }

    private async Task<string> WaitForStablePage(BrowserRunningApp app, string scene)
    {
        var before = await Capture(app);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            await Task.Delay(1100, TestContext.Current.CancellationToken);
            var after = await Capture(app);
            ReportDifference(scene, before, after);
            using var first = SKImage.FromEncodedData(before);
            using var second = SKImage.FromEncodedData(after);
            Assert.NotNull(first);
            Assert.NotNull(second);
            var difference = SkiaSharp.Extended.SKPixelComparer.Compare(first, second, 0);
            if (difference.ErrorPixelCount <= difference.TotalPixels * SampleImageOptions.DefaultMaxErrorPixelFraction)
                return after;
            before = after;
        }
        throw new TimeoutException("Fiddle's full page did not settle after the real controls and scrollbar transitions.");
    }

    private async Task<string> WaitForOutput(BrowserRunningApp app, bool error)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(60);
        while (DateTime.UtcNow < deadline)
        {
            var screenshot = await Capture(app);
            using var bitmap = SKBitmap.Decode(screenshot);
            Assert.NotNull(bitmap);
            var matching = 0;
            for (var y = 200; y < 800; y++)
                for (var x = 680; x < 1240; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (error
                        ? pixel == new SKColor(64, 20, 20)
                        : pixel.Blue > 130 && pixel.Blue > pixel.Red * 1.5 && pixel.Blue > pixel.Green * 1.1)
                        matching++;
                }
            if (matching > (error ? 250000 : 50000))
            {
                TestContext.Current.TestOutputHelper?.WriteLine(
                    $"Fiddle {(error ? "compile error" : "static blue disc")}: {matching} matching output pixels; {screenshot}.");
                return screenshot;
            }
            await Task.Delay(100, TestContext.Current.CancellationToken);
        }
        Assert.Fail($"Fiddle did not render the {(error ? "Roslyn compilation error" : "bundled static blue disc")}.");
        throw new InvalidOperationException("Unreachable.");
    }

    private static void ReportDifference(string name, string first, string second)
    {
        using var before = SKImage.FromEncodedData(first);
        using var after = SKImage.FromEncodedData(second);
        Assert.NotNull(before);
        Assert.NotNull(after);
        Assert.Equal(before.Width, after.Width);
        Assert.Equal(before.Height, after.Height);
        var difference = SkiaSharp.Extended.SKPixelComparer.Compare(before, after, 0);
        TestContext.Current.TestOutputHelper?.WriteLine(
            $"{name}: {difference.ErrorPixelCount}/{difference.TotalPixels} differing pixels " +
            $"({(double)difference.ErrorPixelCount / difference.TotalPixels:P6}); {first}; {second}.");
    }
}
