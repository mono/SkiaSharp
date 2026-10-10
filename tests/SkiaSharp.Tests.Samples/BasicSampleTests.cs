using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Builds and runs the ordinary generated samples.
public class BasicSampleTests : SampleTestBase
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind.HasFlag(SampleLookup.EntryKind.Basic) && !sample.Kind.HasFlag(SampleLookup.EntryKind.Docker))
            .Select(sample => new object[] { sample.Folder, sample.FileName, "Release" });

    private async Task RunSample(string name, IEnumerable<string> arguments, Func<DotNetRunningApp, string, Task> test)
    {
        var folder = Path.Combine("Basic", name);
        var sample = Cases().Single(sample => (string)sample[0] == folder);
        var configuration = (string)sample[2];
        var workspace = await BuildSample(folder, (string)sample[1], configuration);
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        await DotNet.Run(workspace, project, configuration, arguments, app => test(app, project));
    }

    private Task RunBrowserSample(string name, IEnumerable<string> arguments, Func<BrowserRunningApp, Task> test) =>
        RunSample(name, arguments, (app, _) => BrowserRunningApp.Run(app, test));

    [Theory]
    [Trait("Category", "SampleBuild")]
    [MemberData(nameof(Cases))]
    public Task GeneratedSampleBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, configuration);

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    public Task ConsoleSampleRuns() =>
        RunSample("Console", ["SkiaSharp", "--output", "output.png"], async (app, project) =>
        {
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(app.Diagnostics, "output.png");
            File.Copy(Path.Combine(Path.GetDirectoryName(project)!, "output.png"), image);
            SampleImage.ValidateFile(image, 800, 600, $"Host/{SampleLookup.HostPlatform}/console.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    public Task WebSampleReturnsImage() =>
        RunSample("Web", ["--urls", "http://127.0.0.1:0"], async (app, _) =>
        {
            using var home = await app.WaitForResponse("/");
            Assert.Equal(System.Net.HttpStatusCode.OK, home.StatusCode);
            Assert.Equal("text/html", home.Content.Headers.ContentType?.MediaType);

            using var response = await app.GetResponse("/api/images");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

            await SampleImage.SaveAndValidate(
                await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken),
                Path.Combine(app.Diagnostics, "output.png"),
                512, 512,
                $"Host/{SampleLookup.HostPlatform}/web.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task WebSamplePageRenders() =>
        RunBrowserSample("Web", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".card img");
            var screenshot = await app.Screenshot();
            SampleImage.ValidateFile(screenshot, $"Host/{SampleLookup.HostPlatform}/web-page.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BrowserWebAssemblySampleRuns() =>
        RunBrowserSample("BrowserWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement("#output");
            var screenshot = await app.Screenshot();
            SampleImage.ValidateFile(screenshot, $"Host/{SampleLookup.HostPlatform}/browser-wasm-page.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyCpuSampleRuns() =>
        RunBrowserSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/");
            await app.WaitForElement(".canvas-container canvas");
            var screenshot = await app.Screenshot();
            SampleImage.ValidateFile(screenshot, $"Host/{SampleLookup.HostPlatform}/blazor-cpu-page.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyGpuSampleRuns() =>
        RunBrowserSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            await app.Navigate("/gpu");
            await app.WaitForElement(".canvas-container canvas");
            var screenshot = await app.Screenshot();
            SampleImage.ValidateFile(screenshot, $"Host/{SampleLookup.HostPlatform}/blazor-gpu-page.png",
                maxAverageColorErrorFraction: 0.06);
        });

    [Theory]
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
