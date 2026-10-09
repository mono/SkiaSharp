using SkiaSharp.Tests.Samples.Utils;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
public class BrowserInfrastructureTests
{
    [Fact]
    public void GeneratedBrowserWebAssemblyNativeDependencyUsesTheArtifactCohort()
    {
        const string package = "SkiaSharp.NativeAssets.WebAssembly";
        var project = XDocument.Load(Path.Combine(Repo.SamplesDir, "Basic", "BrowserWebAssembly",
            "SkiaSharpSample", "SkiaSharpSample.csproj"));
        var reference = Assert.Single(project.Descendants("PackageReference"),
            element => (string?)element.Attribute("Include") == package);
        var artifact = Assert.Single(Directory.EnumerateFiles(Repo.PackagesDir, package + ".*.nupkg"),
            path => char.IsDigit(Path.GetFileName(path)[package.Length + 1]) &&
                !path.EndsWith(".symbols.nupkg", StringComparison.Ordinal));
        Assert.Equal(Path.GetFileName(artifact), $"{package}.{(string?)reference.Attribute("Version")}.nupkg");
        var managed = Assert.Single(project.Descendants("PackageReference"),
            element => (string?)element.Attribute("Include") == "SkiaSharp");
        Assert.Equal((string?)managed.Attribute("Version"), (string?)reference.Attribute("Version"));
    }

    [Theory]
    [InlineData("Now listening on: http://127.0.0.1:54321", "http://127.0.0.1:54321/")]
    [InlineData("  Now listening on: http://127.0.0.1:54321  ", "http://127.0.0.1:54321/")]
    [InlineData("App url: http://127.0.0.1:60542/?arg=--urls&arg=http%3a%2f%2f127.0.0.1%3a0",
        "http://127.0.0.1:60542/?arg=--urls&arg=http%3a%2f%2f127.0.0.1%3a0")]
    public void DotNetRecognizesOwnedHttpAddress(string output, string expected) =>
        Assert.Equal(new Uri(expected), DotNetRunningApp.ParseAddress(output));

    [Theory]
    [InlineData("App url: https://127.0.0.1:60545/")]
    [InlineData("App url: http://example.com:54321/")]
    [InlineData("App url: http://127.0.0.1:0/")]
    [InlineData("App url: not-an-address")]
    [InlineData("Debug at url: http://127.0.0.1:60542/_framework/debug")]
    public void DotNetRejectsOtherAddresses(string output) =>
        Assert.Null(DotNetRunningApp.ParseAddress(output));
}
