using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Builds the ordinary generated samples without running their UIs.
[Trait("Category", "SampleBuild")]
public class BasicSampleTests : SampleTestBase
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind == SampleLookup.EntryKind.Sample)
            .Select(sample => new object[] { sample.Folder, sample.FileName, "Release" });

    [Theory]
    [MemberData(nameof(Cases))]
    public Task GeneratedSampleBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, configuration);
}
