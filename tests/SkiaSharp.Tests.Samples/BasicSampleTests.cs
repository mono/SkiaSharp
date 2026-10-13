using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public class BasicSampleTests : BasicSampleTestBase
{
    [Theory]
    [RunGroup(SampleRunGroup.Build)]
    [Trait("Category", "SampleBuild")]
    [MemberData(nameof(Cases))]
    public Task GeneratedSampleBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, new() { Configuration = configuration });

    [Fact]
    [RunGroup(SampleRunGroup.Host)]
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
            SampleImage.Validate(image, $"Host/{SampleLookup.HostPlatform}/console.png",
                new() { ExpectedSize = new(800, 600) });
        });

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
            var png = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            await SampleImage.Validate(png,
                Path.Combine(app.Diagnostics, "output.png"),
                $"Host/{SampleLookup.HostPlatform}/web.png", new() { ExpectedSize = new(512, 512) });
        });
}
