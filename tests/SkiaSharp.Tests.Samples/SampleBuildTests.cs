using SkiaSharp.Tests.Samples.Utils;
using System.Xml.Linq;
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

    [Fact]
    public void UnoImplicitPackagesUseTheArtifactVersions()
    {
        foreach (var file in Directory.EnumerateFiles(DotNet.Setting("SamplesDirectory"), "*.csproj", SearchOption.AllDirectories))
        {
            var project = XDocument.Load(file);
            var sdk = (string?)project.Root?.Attribute("Sdk");
            if (sdk is null || !sdk.StartsWith("Uno.Sdk/", StringComparison.Ordinal))
                continue;
            foreach (var package in new[] {
                "SkiaSharp.Views", "SkiaSharp.Skottie",
                "SkiaSharp.NativeAssets.Linux", "HarfBuzzSharp.NativeAssets.Linux" })
            {
                var reference = Assert.Single(project.Descendants("PackageReference"),
                    element => (string?)element.Attribute("Include") == package);
                var family = package.StartsWith("HarfBuzzSharp", StringComparison.Ordinal) ? "HarfBuzzSharp" : "SkiaSharp";
                Assert.Equal(DotNet.Setting(family + "Version"), (string?)reference.Attribute("Version"));
            }
        }
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
