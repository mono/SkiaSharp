using SkiaSharp.Tests.Samples.Utils;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public class BasicSampleRunTests : DotNetSampleTestBase
{
    public static IEnumerable<object[]> ConsoleCases()
    {
        var frameworks = (Environment.GetEnvironmentVariable("SAMPLE_TEST_CONSOLE_FRAMEWORKS") ?? "net10.0").Split(',');
        if (frameworks.Length == 0 || frameworks.Distinct(StringComparer.Ordinal).Count() != frameworks.Length)
            throw new InvalidOperationException("Declare distinct Console frameworks.");

        foreach (var framework in frameworks)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(framework, @"^net[1-9][0-9]*\.[0-9]+$"))
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

        await RunSample(folder, ["SkiaSharp", "--output", "output.png"], async (app, project) =>
        {
            using var runtimeConfig = JsonDocument.Parse(File.ReadAllText(Path.Combine(
                Path.GetDirectoryName(project)!, "bin", "Release", targetFramework,
                "SkiaSharpSample.runtimeconfig.json")));
            var runtime = runtimeConfig.RootElement.GetProperty("runtimeOptions");
            Assert.Equal(targetFramework, runtime.GetProperty("tfm").GetString());
            Assert.Equal("Microsoft.NETCore.App", runtime.GetProperty("framework").GetProperty("name").GetString());
            Assert.StartsWith(targetFramework[3..] + ".",
                runtime.GetProperty("framework").GetProperty("version").GetString());
            TestContext.Current.TestOutputHelper?.WriteLine(
                $"Consumer TFM: {targetFramework}; runtime configuration: {runtime}");

            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(app.Diagnostics, "output.png");
            File.Copy(Path.Combine(Path.GetDirectoryName(project)!, "output.png"), image);
            SampleImage.Validate(image, $"Host/{SampleLookup.HostPlatform}/console.png",
                new() { ExpectedSize = new(800, 600) });
        }, new() { Framework = targetFramework });
    }

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    public Task WebSampleReturnsImage() =>
        RunSample("Basic/Web", ["--urls", "http://127.0.0.1:0"], async (app, _) =>
        {
            using var home = await app.WaitForResponse("/");
            Assert.Equal(System.Net.HttpStatusCode.OK, home.StatusCode);
            Assert.Equal("text/html", home.Content.Headers.ContentType?.MediaType);

            using var response = await app.GetResponse("/api/images");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
            var png = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            await SampleImage.Validate(png,
                Path.Combine(app.Diagnostics, "output.png"),
                $"Host/{SampleLookup.HostPlatform}/web.png", new() { ExpectedSize = new(512, 512) });
        });
}
