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
    [Trait("Category", "Browser")]
    public Task WebSampleRuns() =>
        RunSample("Web", ["--urls", "http://127.0.0.1:0"], (app, _) =>
            BrowserSampleApp.Capture(app, "/", ".card img", $"Host/{SampleLookup.HostPlatform}/web-page.png"));

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BrowserWebAssemblySampleRuns() =>
        RunSample("BrowserWebAssembly", ["--urls", "http://127.0.0.1:0"], (app, _) =>
            BrowserSampleApp.Capture(app, "/", "#output", $"Host/{SampleLookup.HostPlatform}/browser-wasm-page.png"));

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyCpuSampleRuns() =>
        RunSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], (app, _) =>
            BrowserSampleApp.Capture(app, "/", ".canvas-container canvas", $"Host/{SampleLookup.HostPlatform}/blazor-cpu-page.png"));

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [Trait("Category", "Browser")]
    public Task BlazorWebAssemblyGpuSampleRuns() =>
        RunSample("BlazorWebAssembly", ["--urls", "http://127.0.0.1:0"], (app, _) =>
            BrowserSampleApp.Capture(app, "/gpu", ".canvas-container canvas", $"Host/{SampleLookup.HostPlatform}/blazor-gpu-page.png",
                maxAverageColorErrorFraction: 0.06));
}
