using System.Text.Json;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Builds the ordinary generated samples without running their UIs.
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
        var options = new DotNetOptions { Configuration = (string)sample[2] };
        var workspace = await BuildSample(folder, (string)sample[1], options);
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        await DotNet.Run(workspace, project, arguments, app => test(app, project), options);
    }

    [Theory]
    [RunGroup(SampleRunGroup.Build)]
    [Trait("Category", "SampleBuild")]
    [MemberData(nameof(Cases))]
    public Task GeneratedSampleBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, new() { Configuration = configuration });

    public static IEnumerable<object[]> ConsoleCases()
    {
        var frameworks = (Environment.GetEnvironmentVariable("SAMPLE_TEST_CONSOLE_FRAMEWORKS") ?? "net10.0").Split(',');
        if (frameworks.Length == 0 || frameworks.Distinct(StringComparer.Ordinal).Count() != frameworks.Length)
            throw new InvalidOperationException("Declare distinct Console frameworks.");
        foreach (var framework in frameworks)
        {
            if (framework is null || !System.Text.RegularExpressions.Regex.IsMatch(framework, @"^net[1-9][0-9]*\.[0-9]+$"))
                throw new InvalidOperationException($"Invalid Console framework: {framework}");
            yield return new object[] { framework };
        }
    }

    [Theory]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    [MemberData(nameof(ConsoleCases))]
    public async Task ConsoleSampleRuns(string targetFramework)
    {
        var folder = Path.Combine("Basic", "Console");
        var relativeProject = Path.Combine(folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        if (targetFramework != "net10.0")
            PrepareSample(folder).RetargetProject(relativeProject, "net10.0", targetFramework);

        await RunSample("Console", ["SkiaSharp", "--output", "output.png"], async (app, project) =>
        {
            using var runtimeConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                Path.GetDirectoryName(project)!, "bin", "Release", targetFramework, "SkiaSharpSample.runtimeconfig.json")));
            var runtime = runtimeConfig.RootElement.GetProperty("runtimeOptions");
            Assert.Equal(targetFramework, runtime.GetProperty("tfm").GetString());
            Assert.Equal("Microsoft.NETCore.App", runtime.GetProperty("framework").GetProperty("name").GetString());
            Assert.StartsWith(targetFramework[3..] + ".",
                runtime.GetProperty("framework").GetProperty("version").GetString());
            TestContext.Current.TestOutputHelper?.WriteLine($"Consumer TFM: {targetFramework}; runtime configuration: {runtime}");

            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(app.Diagnostics, "output.png");
            File.Copy(Path.Combine(Path.GetDirectoryName(project)!, "output.png"), image);
            SampleImage.ValidateFile(image, 800, 600, $"Host/{SampleLookup.HostPlatform}/console.png");
        });
    }

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
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
}
