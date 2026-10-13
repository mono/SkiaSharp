using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Docker")]
[RunGroup(SampleRunGroup.Docker)]
public class DockerSampleTests(DockerSampleFixture docker) : IClassFixture<DockerSampleFixture>
{
    private static string ContainerPlatform =>
        OperatingSystem.IsWindows() ? "Windows" : "Linux";

    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind.HasFlag(SampleLookup.EntryKind.Docker))
            .Select(sample => new object[] { sample.Folder, sample.FileName });

    private async Task RunSample(string name, IEnumerable<string> arguments, Func<DockerRunningApp, Task> test)
    {
        var folder = Path.Combine("Basic", name);
        var sample = Cases().Single(sample => (string)sample[0] == folder);
        var tag = await docker.Image(folder, (string)sample[1]);
        await docker.Run(tag, arguments, test);
    }

    [Theory]
    [Trait("Category", "DockerBuild")]
    [MemberData(nameof(Cases))]
    public async Task DockerImageBuilds(string folder, string dockerfile) =>
        await docker.Image(folder, dockerfile);

    [Fact]
    [Trait("Category", "SampleRun")]
    public Task ConsoleSampleRuns() =>
        RunSample("DockerConsole", ["SkiaSharp", "--output", "output.png"], async app =>
        {
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(app.Diagnostics, "output.png");
            Directory.CreateDirectory(app.Diagnostics);

            var output = OperatingSystem.IsWindows() ? @"C:\app\output.png" : "/app/output.png";
            await app.DownloadFile(output, image);

            SampleImage.Validate(image, $"Docker/{ContainerPlatform}/console.png",
                new() { ExpectedSize = new(800, 600) });
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    public Task WebApiSampleReturnsImage() =>
        RunSample("DockerWebApi", [], async app =>
        {
            using var health = await app.WaitForResponse("/health");
            Assert.Equal(System.Net.HttpStatusCode.OK, health.StatusCode);
            Assert.Equal("text/plain", health.Content.Headers.ContentType?.MediaType);
            Assert.Equal("Healthy", await health.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            using var response = await app.GetResponse("/api/images");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);
            var png = await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken);
            Assert.True(png.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "Expected a PNG response.");

            await SampleImage.Validate(png,
                Path.Combine(app.Diagnostics, "output.png"),
                $"Docker/{ContainerPlatform}/web.png", new() { ExpectedSize = new(800, 600) });
        });
}
