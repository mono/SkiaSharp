using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Builds the host Gallery solution and its shared projects without running the UI.
[Trait("Category", "SampleBuild")]
[Trait("Category", "GalleryBuild")]
public class GallerySampleTests : SampleTestBase
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind == SampleLookup.EntryKind.Gallery)
            .Select(sample => new object[] { sample.Folder, sample.FileName, "Release" });

    [Theory]
    [MemberData(nameof(Cases))]
    public Task GeneratedGalleryBuilds(string folder, string solution, string configuration) =>
        BuildSample(folder, solution, configuration);
}
