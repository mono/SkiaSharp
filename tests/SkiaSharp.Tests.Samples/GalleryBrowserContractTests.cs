using System.IO.Compression;
using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Build)]
[Trait("Category", "Infrastructure")]
public sealed class GalleryBrowserContractTests
{
    [Theory]
    [InlineData("Gallery", "Blazor/SkiaSharpSample.Blazor.csproj", "net10.0")]
    [InlineData("Gallery", "Uno/SkiaSharpSample.Uno.csproj", "net10.0-browserwasm")]
    [InlineData("SkiaFiddle", "SkiaFiddle.csproj", "net10.0-browserwasm")]
    public void BrowserProjectsSelectTheirDeclaredFrameworkWithoutRewritingInputs(
        string folder, string relativeProject, string framework)
    {
        using var workspace = new SampleWorkspace(folder);
        var project = Path.Combine(workspace.Root, "samples", folder, relativeProject);
        var before = File.ReadAllText(project);
        var options = new DotNetOptions { Framework = framework };
        var arguments = DotNet.BuildArguments(workspace, project, workspace.DiagnosticsRoot, options);
        Assert.Contains("--framework", arguments);
        Assert.Contains(framework, arguments);
        Assert.DoesNotContain(arguments, argument => argument.StartsWith("-p:TargetFramework", StringComparison.Ordinal));
        Assert.Equal(before, File.ReadAllText(project));
        if (folder == "Gallery")
        {
            var shared = XDocument.Load(Path.Combine(workspace.Root, "samples", folder, "Shared", "SkiaSharpSample.Shared.csproj"));
            Assert.Equal("net10.0", Assert.Single(shared.Descendants("TargetFramework")).Value);
        }
        Assert.Throws<ArgumentException>(() =>
            DotNet.BuildArguments(workspace, project, workspace.DiagnosticsRoot, options with { Framework = "net10.0-wrongplatform" }));
    }

    [Fact]
    public void BrowserClassesShareTheAuthoritativeBrowserCollection()
    {
        foreach (var type in new[]
        {
            typeof(GalleryBlazorBrowserSampleRunTests), typeof(GalleryUnoBrowserSampleRunTests),
            typeof(SkiaFiddleBrowserSampleRunTests)
        })
        {
            var collection = Assert.Single(type.GetCustomAttributesData(),
                attribute => attribute.AttributeType == typeof(CollectionAttribute));
            Assert.Equal(nameof(BrowserSampleCollection), Assert.Single(collection.ConstructorArguments).Value);
        }
    }

    [Fact]
    public void FiddleDeclaresTheProducingHarfBuzzPackage()
    {
        var project = XDocument.Load(Path.Combine(Repo.SamplesDir, "SkiaFiddle", "SkiaFiddle.csproj"));
        var reference = Assert.Single(project.Descendants("PackageReference"),
            item => item.Attribute("Include")?.Value == "HarfBuzzSharp");
        var package = Assert.Single(Directory.EnumerateFiles(Repo.PackagesDir, "HarfBuzzSharp.*.nupkg"),
            path => char.IsDigit(Path.GetFileName(path)["HarfBuzzSharp.".Length]) &&
                !path.EndsWith(".symbols.nupkg", StringComparison.Ordinal));
        using var archive = ZipFile.OpenRead(package);
        using var stream = Assert.Single(archive.Entries,
            entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal)).Open();
        var metadata = XDocument.Load(stream).Root!.Elements().Single(element => element.Name.LocalName == "metadata");
        Assert.Equal("HarfBuzzSharp", metadata.Elements().Single(element => element.Name.LocalName == "id").Value);
        Assert.Equal(metadata.Elements().Single(element => element.Name.LocalName == "version").Value,
            reference.Attribute("Version")?.Value);
    }

    [Theory]
    [InlineData(99, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void CatalogPixelAllowanceStopsAtOnePercentWithoutChangingTheDefault(int changedPixels, bool accepted)
    {
        var directory = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("gallery-pixel-budget"));
        Directory.CreateDirectory(directory);
        using var bitmap = new SKBitmap(100, 100);
        bitmap.Erase(SKColors.Black);
        var expected = Save("expected.png");
        for (var i = 0; i < changedPixels; i++)
            bitmap.SetPixel(i % bitmap.Width, i / bitmap.Width, new SKColor(1, 0, 0));
        var actual = Save("actual.png");
        if (accepted)
            SampleImage.Validate(actual, expected, new() { MaxErrorPixelFraction = 0.01m });
        else
            Assert.Throws<Xunit.Sdk.FailException>(() =>
                SampleImage.Validate(actual, expected, new() { MaxErrorPixelFraction = 0.01m }));
        Assert.Throws<Xunit.Sdk.FailException>(() => SampleImage.Validate(actual, expected));

        string Save(string name)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var path = Path.Combine(directory, name);
            File.WriteAllBytes(path, data.ToArray());
            return path;
        }
    }
}
