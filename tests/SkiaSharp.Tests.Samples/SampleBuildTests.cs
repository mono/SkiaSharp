using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleBuild")]
public class SampleBuildTests(SampleWorkspace workspace, ITestOutputHelper output) : IClassFixture<SampleWorkspace>
{
    public static IEnumerable<object[]> Cases()
    {
        return SampleLookup.Discover(DotNet.Setting("SamplesDirectory"), SampleLookup.HostPlatform,
                AppContext.GetData("SampleTest.Filter") as string ?? "")
            .Where(sample => sample.Kind != SampleKind.Docker)
            .Select(sample => new object[] { sample.Folder, sample.FileName });
    }

    [Fact]
    public void GeneratedSampleInputsExist()
    {
        Assert.NotEmpty(SampleLookup.Discover(DotNet.Setting("SamplesDirectory"), SampleLookup.HostPlatform,
            AppContext.GetData("SampleTest.Filter") as string ?? ""));
    }

    [Theory(DisableDiscoveryEnumeration = true, SkipTestWithoutData = true)]
    [MemberData(nameof(Cases))]
    public async Task GeneratedSampleBuilds(string folder, string solution)
    {
        var profile = workspace.Profile();
        var samples = Path.Combine(profile.Root, "samples");
        var diagnostics = Path.Combine(profile.DiagnosticsRoot, "builds", folder, Path.GetFileNameWithoutExtension(solution));
        try
        {
            await profile.BuildSample(Path.Combine(samples, folder, solution), diagnostics, output);
        }
        finally
        {
            SampleWorkspace.CleanProducts(samples);
        }
    }
}
