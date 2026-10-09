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

    [Theory]
    [Trait("Category", "SampleBuild")]
    [MemberData(nameof(Cases))]
    public Task GeneratedSampleBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, configuration);

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    public async Task ConsoleSampleRuns()
    {
        var folder = Path.Combine("Basic", "Console");
        var workspace = await BuildSample(folder, "SkiaSharpSample.slnx", "Release");
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");

        await DotNet.Run(workspace, project, "Release", ["SkiaSharp", "--output", "output.png"], async app =>
        {
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(workspace.DiagnosticsRoot, "run", "output.png");
            File.Copy(Path.Combine(Path.GetDirectoryName(project)!, "output.png"), image);
            SampleImage.ValidateFile(image, 800, 600, $"Host/{SampleLookup.HostPlatform}/console.png");
        });
    }

    [Fact]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    public async Task WebSampleReturnsImage()
    {
        var folder = Path.Combine("Basic", "Web");
        var workspace = await BuildSample(folder, "SkiaSharpSample.slnx", "Release");
        var project = Path.Combine(workspace.Root, "samples", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");

        await DotNet.Run(workspace, project, "Release", ["--urls", "http://127.0.0.1:0"], async app =>
        {
            using var home = await app.WaitForResponse("/");
            Assert.Equal(System.Net.HttpStatusCode.OK, home.StatusCode);
            Assert.Equal("text/html", home.Content.Headers.ContentType?.MediaType);

            using var response = await app.GetResponse("/api/images");
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("image/png", response.Content.Headers.ContentType?.MediaType);

            await SampleImage.SaveAndValidate(
                await response.Content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken),
                Path.Combine(workspace.DiagnosticsRoot, "run", "output.png"),
                512, 512,
                $"Host/{SampleLookup.HostPlatform}/web.png");
        });
    }
}
