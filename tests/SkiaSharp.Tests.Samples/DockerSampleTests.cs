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

            SampleImage.ValidateFile(image, 800, 600, $"Docker/{ContainerPlatform}/console.png");
        });

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Browser")]
    public Task WebApiSampleRuns() =>
        RunSample("DockerWebApi", [], app =>
            BrowserSampleApp.Capture(app, "/api/images", "img",
                $"Docker/{ContainerPlatform}/web-page.{SampleLookup.HostPlatform}.png"));
}
