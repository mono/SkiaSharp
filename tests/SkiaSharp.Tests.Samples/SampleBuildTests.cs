using SkiaSharp.Tests.Samples.Utils;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "SampleBuild")]
public class SampleBuildTests(ITestOutputHelper output) : SampleTestBase(output)
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

    [Theory]
    [InlineData("Console", "SkiaSharp.NativeAssets.Linux.NoDependencies")]
    [InlineData("Web", "SkiaSharp.NativeAssets.Linux.NoDependencies")]
    [InlineData("BrowserWebAssembly", "SkiaSharp.NativeAssets.WebAssembly")]
    public void GeneratedHostSamplesIncludeTheirRuntimeAssets(string folder, string package)
    {
        var file = Path.Combine(DotNet.Setting("SamplesDirectory"), "Basic", folder, "SkiaSharpSample", "SkiaSharpSample.csproj");
        var project = XDocument.Load(file);
        var reference = Assert.Single(project.Descendants("PackageReference"),
            element => (string?)element.Attribute("Include") == package);
        Assert.Equal(DotNet.Setting("SkiaSharpVersion"), (string?)reference.Attribute("Version"));
    }

    [Fact]
    public void UnoProjectsDeclareArtifactPackagesAfterTheSourceOnlyImport()
    {
        var repository = DotNet.Setting("RepositoryDirectory");
        foreach (var relative in new[]
        {
            "samples/Basic/UnoPlatform/SkiaSharpSample/SkiaSharpSample.csproj",
            "samples/Gallery/Uno/SkiaSharpSample.Uno.csproj",
            "samples/SkiaFiddle/SkiaFiddle.csproj"
        })
        {
            var file = Path.Combine(repository, relative);
            var project = XDocument.Load(file);
            var import = Assert.Single(project.Root!.Elements("Import"),
                element => ((string?)element.Attribute("Project"))?.EndsWith("_UnoPlatformSamples.targets") == true);
            var declarations = project.Root.Elements("ItemGroup").Where(group =>
                group.Elements("ProjectReference").Any(element =>
                    UnoPackages.Any(package => ((string?)element.Attribute("Include"))?.EndsWith(
                        $"{package}.csproj", StringComparison.Ordinal) == true))).ToArray();
            // The source import clears earlier ProjectReferences; the dedicated declarations must follow it.
            Assert.Single(declarations);
            Assert.All(declarations, group => Assert.True(group.IsAfter(import)));
            foreach (var package in UnoPackages)
            {
                var projectName = package == "SkiaSharp.Views"
                    ? @"source\SkiaSharp.Views\SkiaSharp.Views\SkiaSharp.Views.csproj"
                    : package == "SkiaSharp.Skottie"
                        ? @"binding\SkiaSharp.Skottie\SkiaSharp.Skottie.csproj"
                        : $@"binding\{package}\{package}.csproj";
                Assert.Contains(project.Descendants("ProjectReference"), reference =>
                    Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file)!,
                        ((string)reference.Attribute("Include")!).Replace('\\', Path.DirectorySeparatorChar)))
                    == Path.GetFullPath(Path.Combine(repository,
                        projectName.Replace('\\', Path.DirectorySeparatorChar))));
            }
        }

        foreach (var file in Directory.EnumerateFiles(DotNet.Setting("SamplesDirectory"), "*.csproj", SearchOption.AllDirectories))
        {
            var project = XDocument.Load(file);
            var sdk = (string?)project.Root?.Attribute("Sdk");
            if (sdk is null || !sdk.StartsWith("Uno.Sdk/", StringComparison.Ordinal))
                continue;
            Assert.Equal(DotNet.Setting("SkiaSharpVersion"),
                (string?)Assert.Single(project.Descendants("SkiaSharpVersion")).Value);
            foreach (var package in UnoPackages)
            {
                var reference = Assert.Single(project.Descendants("PackageReference"),
                    element => (string?)element.Attribute("Include") == package);
                var family = package.StartsWith("HarfBuzzSharp", StringComparison.Ordinal) ? "HarfBuzzSharp" : "SkiaSharp";
                Assert.Equal(DotNet.Setting(family + "Version"), (string?)reference.Attribute("Version"));
            }
        }

    }

    private static readonly string[] UnoPackages =
        ["SkiaSharp.Views", "SkiaSharp.Skottie", "SkiaSharp.NativeAssets.Linux", "HarfBuzzSharp.NativeAssets.Linux"];

    [Theory(DisableDiscoveryEnumeration = true, SkipTestWithoutData = true)]
    [MemberData(nameof(Cases))]
    public async Task GeneratedSampleBuilds(string folder, string solution)
    {
        var profile = PrepareSample(folder);
        var samples = Path.Combine(profile.Root, "samples");
        var diagnostics = Path.Combine(profile.DiagnosticsRoot, "builds", folder, Path.GetFileNameWithoutExtension(solution));
        await profile.BuildSample(Path.Combine(samples, folder, solution), diagnostics, Output);
    }
}
