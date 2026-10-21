using SkiaSharp.Tests.Goldens;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
[RunGroup(SampleRunGroup.Build)]
public sealed class GoldenMigrationTests : IDisposable
{
    private readonly string root = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("golden-migration"));

    [Theory]
    [InlineData("gallery-gradient-cpu")]
    [InlineData("gallery-gradient-gpu")]
    [InlineData("gallery-gradient-adjusted")]
    public void AndroidKeepsTheDemonstratedArchitectureDifferences(string name)
    {
        var store = GoldenStoreIndex.FromDirectory(SampleReferences.Root);
        var oldArm = store.Resolve(new("pixel-9", "26", name, "arm64")).Asset!;
        var oldIntel = store.Resolve(new("pixel-9", "26", name, "x64")).Asset!;
        Assert.Equal("arm64", oldArm.Key.Architecture);
        Assert.Equal("x64", oldIntel.Key.Architecture);
        Assert.NotEqual(oldArm.ReadBytes(), oldIntel.ReadBytes());
        var currentArm = store.Resolve(new("pixel-9", "36", name, "arm64")).Asset!;
        var currentIntel = store.Resolve(new("pixel-9", "36", name, "x64")).Asset!;
        Assert.Null(currentArm.Key.Architecture);
        if (name == "gallery-gradient-cpu")
        {
            Assert.Equal("x64", currentIntel.Key.Architecture);
            Assert.NotEqual(currentArm.ReadBytes(), currentIntel.ReadBytes());
        }
        else
        {
            Assert.Equal(currentArm.Key.ToString(), currentIntel.Key.ToString());
            Assert.Equal(currentArm.ReadBytes(), currentIntel.ReadBytes());
        }
    }

    [Fact]
    public void AndroidDrawingInitialAndPostInteractionAreExplicitCases()
    {
        var store = GoldenStoreIndex.FromDirectory(SampleReferences.Root);
        var initial = store.Resolve(new("pixel-9", "36", "maui-basic-drawing-initial", "x64")).Asset!;
        var afterInteraction = store.Resolve(new("pixel-9", "36", "maui-basic-drawing", "x64")).Asset!;
        Assert.Null(initial.Key.Architecture);
        Assert.Equal("x64", afterInteraction.Key.Architecture);
        Assert.NotEqual(initial.Key.CaseName, afterInteraction.Key.CaseName);
        using var first = SKImage.FromEncodedData(initial.Location);
        using var after = SKImage.FromEncodedData(afterInteraction.Location);
        var difference = SkiaSharp.Extended.SKPixelComparer.Compare(first, after, 0);
        Assert.True(difference.ErrorPixelCount > difference.TotalPixels * SampleImageOptions.DefaultMaxErrorPixelFraction);
    }

    [Theory]
    [InlineData("18.6")]
    [InlineData("26.3.1")]
    [InlineData("26.3.2")]
    public void IosGallerySharesReviewedPixelsWithoutCrossingDeviceModels(string version)
    {
        var store = GoldenStoreIndex.FromDirectory(SampleReferences.Root);
        var resolution = store.Resolve(new("iphone-16-pro", version, "gallery-gradient-adjusted", "arm64"));
        Assert.Equal("iphone-16-pro/base/gallery-gradient-adjusted.png", resolution.Asset!.Key.ToString());
        Assert.All(resolution.Candidates, candidate => Assert.StartsWith("iphone-16-pro/", candidate));
    }

    [Fact]
    public void PatchUpdatesUseReviewedReferencesAndTheSameCropContract()
    {
        var store = GoldenStoreIndex.FromDirectory(SampleReferences.Root);
        var resolution = store.Resolve(new("iphone-16-pro", "26.3.2", "maui-basic-cpu", "arm64"));
        Assert.Equal("26.3.1", resolution.Asset!.Key.Version);
        Assert.Equal(IosRunningApp.ScreenshotInsets("iPhone 16 Pro", "26.3.1", new(1206, 2622)),
            IosRunningApp.ScreenshotInsets("iPhone 16 Pro", "26.3.2", new(1206, 2622)));
    }

    [Fact]
    public void TerminalPixelFailureNeverChoosesAnOlderMatchingImage()
    {
        var goldens = Path.Combine(root, "goldens");
        Save(Path.Combine(goldens, "mac", "12.6", "blazor-cpu.png"), 4, SKColors.Red);
        Save(Path.Combine(goldens, "mac", "12", "blazor-cpu.png"), 4, SKColors.Blue);
        var actual = Path.Combine(root, "actual.png");
        Save(actual, 4, SKColors.Blue);
        var resolution = GoldenStoreIndex.FromDirectory(goldens).Resolve(new("mac", "12.6.2", "blazor-cpu", "arm64"));
        Assert.Equal("12.6", resolution.Asset!.Key.Version);
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, resolution.Asset.Location));
        Assert.True(File.Exists(Path.ChangeExtension(actual, ".diff.png")));
    }

    [Fact]
    public void TerminalDimensionFailureNeverFindsAnOlderMatchingSize()
    {
        var goldens = Path.Combine(root, "goldens");
        Save(Path.Combine(goldens, "mac", "12.6", "blazor-cpu.png"), 4, SKColors.Blue);
        Save(Path.Combine(goldens, "mac", "12", "blazor-cpu.png"), 2, SKColors.Blue);
        var actual = Path.Combine(root, "actual.png");
        Save(actual, 2, SKColors.Blue);
        var resolution = GoldenStoreIndex.FromDirectory(goldens).Resolve(new("mac", "12.6.2", "blazor-cpu", "arm64"));
        Assert.Equal("12.6", resolution.Asset!.Key.Version);
        Assert.ThrowsAny<Exception>(() => SampleImage.Validate(actual, resolution.Asset.Location));
    }

    private static void Save(string path, int size, SKColor color)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var bitmap = new SKBitmap(size, size);
        bitmap.Erase(color);
        using var data = bitmap.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }
}
