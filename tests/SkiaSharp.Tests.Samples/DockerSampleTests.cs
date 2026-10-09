using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Docker")]
public class DockerSampleTests(DockerSampleFixture docker) : IClassFixture<DockerSampleFixture>
{
    private static string ContainerPlatform =>
        OperatingSystem.IsWindows() ? "Windows" : "Linux";

    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind.HasFlag(SampleLookup.EntryKind.Docker))
            .Select(sample => new object[] { sample.Folder, sample.FileName });

    [Theory]
    [Trait("Category", "DockerBuild")]
    [MemberData(nameof(Cases))]
    public async Task DockerImageBuilds(string folder, string dockerfile) =>
        await docker.Image(folder, dockerfile);

    private Task<string> Image(string name)
    {
        var entry = SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Single(sample => sample.Folder == Path.Combine("Basic", name) && sample.Kind.HasFlag(SampleLookup.EntryKind.Docker));

        return docker.Image(entry.Folder, entry.FileName);
    }

    [Fact]
    [Trait("Category", "SampleRun")]
    public async Task ConsoleSampleRuns()
    {
        var tag = await Image("DockerConsole");

        await docker.Run(tag, ["SkiaSharp", "--output", "output.png"], async app =>
        {
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(app.Diagnostics, "output.png");
            Directory.CreateDirectory(app.Diagnostics);

            var output = OperatingSystem.IsWindows() ? @"C:\app\output.png" : "/app/output.png";
            await app.DownloadFile(output, image);

            SampleImage.ValidateFile(image, 800, 600, $"Docker/{ContainerPlatform}/console.png");
        });
    }

    [Fact]
    [Trait("Category", "SampleRun")]
    public async Task WebApiSampleReturnsImage()
    {
        var tag = await Image("DockerWebApi");
        await docker.Run(tag, [], async app =>
        {
            using var health = await app.WaitForResponse("/health");
            Assert.Equal(System.Net.HttpStatusCode.OK, health.StatusCode);
            Assert.Equal("text/plain", health.Content.Headers.ContentType?.MediaType);
            Assert.Equal("Healthy", await health.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));

            using var response = await app.GetResponse("/api/images");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

            await SampleImage.SaveAndValidate(
                await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken),
                Path.Combine(app.Diagnostics, "output.png"),
                800, 600,
                $"Docker/{ContainerPlatform}/web.png");
        });
    }
}
