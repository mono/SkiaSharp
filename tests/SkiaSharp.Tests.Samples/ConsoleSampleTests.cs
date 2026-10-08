using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleRun")]
public class ConsoleSampleTests(ITestOutputHelper output) : SampleTestBase(output)
{
    protected override string SampleFolder => Path.Combine("Basic", "Console");

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
        var profile = PrepareSample(folder);
        var samples = Path.Combine(profile.Root, "samples");
        var solution = Path.Combine(samples, folder, solutionFile);
        var diagnostics = Path.Combine(profile.DiagnosticsRoot, "runs", "Console");
        var image = Path.Combine(diagnostics, "output.png");
        await profile.BuildSample(solution, diagnostics, Output);
        await profile.RunConsoleSample(solution, image, Output);
        Assert.True(File.Exists(image), $"Console sample did not create {image}");
        SampleImage.Validate(File.ReadAllBytes(image));
    }
}
