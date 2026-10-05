using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleRun")]
public class SampleRunTests(SampleWorkspace workspace, ITestOutputHelper output) : IClassFixture<SampleWorkspace>
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(DotNet.Setting("SamplesDirectory"), SampleLookup.HostPlatform,
                AppContext.GetData("SampleTest.Filter") as string ?? "")
            .Where(sample => sample.Kind == SampleKind.Sample &&
                sample.Folder == Path.Combine("Basic", "Console"))
            .Select(sample => new object[] { sample.Folder, sample.FileName });

    [Theory(DisableDiscoveryEnumeration = true, SkipTestWithoutData = true)]
    [MemberData(nameof(Cases))]
    public async Task ConsoleRendersPng(string folder, string solutionFile)
    {
        var profile = workspace.Profile();
        var samples = Path.Combine(profile.Root, "samples");
        var solution = Path.Combine(samples, folder, solutionFile);
        var diagnostics = Path.Combine(profile.DiagnosticsRoot, "runs", "Console");
        var image = Path.Combine(diagnostics, "output.png");
        try
        {
            await profile.BuildSample(solution, diagnostics, output);
            await profile.RunConsoleSample(solution, image, output);
            Assert.True(File.Exists(image), $"Console sample did not create {image}");
            SampleImage.Validate(File.ReadAllBytes(image));
        }
        finally
        {
            SampleWorkspace.CleanProducts(samples);
        }
    }
}
