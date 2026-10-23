using SkiaSharp.Tests.Shared;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public class BasicSampleRunTests : DotNetSampleTestBase
{
    [Fact]
    [RunGroup(SampleRunGroup.Host)]
    [Trait("Category", "SampleRun")]
    [Trait("Category", "Host")]
    public Task ConsoleSampleRuns() =>
        RunSample("Basic/Console", ["SkiaSharp", "--output", "output.png"], async (app, project) =>
        {
            var result = await app.WaitForExit();
            Assert.Equal(0, result.ExitCode);
            Assert.Contains("Rendering \"SkiaSharp\" to output.png", result.Output);
            Assert.Contains("Saved ", result.Output);

            var image = Path.Combine(app.Diagnostics, "output.png");
            File.Copy(Path.Combine(Path.GetDirectoryName(project)!, "output.png"), image);
            SampleImage.Verifier.Verify(SampleImage.Host("Console/output"), GoldenImageSource.FromFile(image),
                new() { ExpectedSize = new(800, 600) });
        });

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
            SampleImage.Verifier.Verify(
                SampleImage.Host("Web/output"),
                GoldenImageSource.WritePng(png, Path.Combine(app.Diagnostics, "output.png")),
                new() { ExpectedSize = new(512, 512) });
        });
}
