using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Builds the actual generated samples, including Gallery, without running their UIs.
[Trait("Category", "SampleBuild")]
public class SampleBuildTests : SampleTestBase
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind != SampleLookup.EntryKind.Docker)
            .Select(sample => new object[] { sample.Folder, sample.FileName, "Release" });

    [Fact]
    public void GeneratedSampleInputsExist() =>
        Assert.NotEmpty(SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform));

    [Theory]
    [MemberData(nameof(Cases))]
    public Task GeneratedSampleBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, configuration);
}
