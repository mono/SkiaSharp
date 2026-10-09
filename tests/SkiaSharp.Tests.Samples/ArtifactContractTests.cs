using SkiaSharp.Tests.Samples.Utils;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Checks package-cohort consistency and generated dependency versions.
[Trait("Category", "Infrastructure")]
public class ArtifactContractTests
{
    public static TheoryData<string[]> MatchingPackages => new()
    {
        new[] { "SkiaSharp.1.2.3-pr.1.nupkg", "SkiaSharp.1.2.3-pr.1.symbols.nupkg", "HarfBuzzSharp.4.5.6-pr.1.nupkg" }
    };

    public static TheoryData<string[]> InvalidPackages => new()
    {
        Array.Empty<string>(),
        new[] { "SkiaSharp.1.2.3-pr.1.nupkg", "SkiaSharp.1.2.3-pr.2.nupkg", "SkiaSharp.1.2.3-pr.1.symbols.nupkg", "HarfBuzzSharp.4.5.6-pr.1.nupkg" },
        new[] { "SkiaSharp.1.2.3-pr.1.nupkg", "SkiaSharp.1.2.3-pr.1.symbols.nupkg", "HarfBuzzSharp.4.5.6-pr.2.nupkg" }
    };

    [Theory]
    [InlineData("Basic/UnoPlatform/SkiaSharpSample/SkiaSharpSample.csproj", "SkiaSharp.Views")]
    [InlineData("Basic/UnoPlatform/SkiaSharpSample/SkiaSharpSample.csproj", "SkiaSharp.Skottie")]
    [InlineData("Basic/UnoPlatform/SkiaSharpSample/SkiaSharpSample.csproj", "SkiaSharp.NativeAssets.Linux")]
    [InlineData("Basic/UnoPlatform/SkiaSharpSample/SkiaSharpSample.csproj", "HarfBuzzSharp.NativeAssets.Linux")]
    [InlineData("Gallery/Uno/SkiaSharpSample.Uno.csproj", "SkiaSharp.Views")]
    [InlineData("Gallery/Uno/SkiaSharpSample.Uno.csproj", "SkiaSharp.Skottie")]
    [InlineData("Gallery/Uno/SkiaSharpSample.Uno.csproj", "SkiaSharp.NativeAssets.Linux")]
    [InlineData("Gallery/Uno/SkiaSharpSample.Uno.csproj", "HarfBuzzSharp.NativeAssets.Linux")]
    public void GeneratedUnoDependenciesUseTheArtifactCohort(string relative, string package)
    {
        var project = XDocument.Load(Path.Combine(Repo.SamplesDir, relative));
        var reference = Assert.Single(
            project.Descendants("PackageReference"),
            element => (string?)element.Attribute("Include") == package);
        var artifact = Assert.Single(
            Directory.EnumerateFiles(Repo.PackagesDir, package + ".*.nupkg"),
            path => char.IsDigit(Path.GetFileName(path)[package.Length + 1]) &&
                !path.EndsWith(".symbols.nupkg", StringComparison.Ordinal));
        Assert.Equal(Path.GetFileName(artifact), $"{package}.{(string?)reference.Attribute("Version")}.nupkg");
    }

    [Theory]
    [InlineData("Basic/UnoPlatform/SkiaSharpSample/SkiaSharpSample.csproj")]
    [InlineData("Gallery/Uno/SkiaSharpSample.Uno.csproj")]
    public void GeneratedUnoVersionPropertyUsesTheArtifactCohort(string relative)
    {
        var project = XDocument.Load(Path.Combine(Repo.SamplesDir, relative));
        var reference = Assert.Single(
            project.Descendants("PackageReference"),
            element => (string?)element.Attribute("Include") == "SkiaSharp.Views");
        Assert.Equal((string?)reference.Attribute("Version"),
            Assert.Single(project.Descendants("SkiaSharpVersion")).Value);
    }

    [Theory]
    [MemberData(nameof(MatchingPackages))]
    public async Task MatchingPackageIdentityPassesBeforeRestore(string[] packageNames)
    {
        var result = await ValidatePackages(packageNames);
        Assert.Equal(0, result.ExitCode);
    }

    [Theory]
    [MemberData(nameof(InvalidPackages))]
    public async Task InvalidPackageIdentityFailsBeforeRestore(string[] packageNames)
    {
        var result = await ValidatePackages(packageNames);
        Assert.NotEqual(0, result.ExitCode);
        Assert.Contains("error", result.Output + result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<(int ExitCode, string Output, string Error)> ValidatePackages(string[] packageNames)
    {
        var root = Path.Combine(Repo.ArtifactsDir, $"contract-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            foreach (var package in packageNames)
                File.WriteAllText(Path.Combine(root, package), "");

            var repository = Repo.RootDir;
            var arguments = new List<string>
            {
                "msbuild", Path.Combine(repository, "tests", "SkiaSharp.Tests.Samples", "SkiaSharp.Tests.Samples.csproj"),
                "-nologo", "-v:minimal", "-t:ValidateSamplePlatformPackages", $"-p:_SamplePackageDirectory={root}"
            };
            return await ProcessRunner.Run("dotnet", arguments, repository, TimeSpan.FromMinutes(1));
        }
        finally
        {
            SampleWorkspace.DeleteDirectory(root);
        }
    }
}
