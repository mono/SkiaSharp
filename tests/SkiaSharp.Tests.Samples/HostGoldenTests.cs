using SkiaSharp;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Golden")]
public class HostGoldenTests(ITestOutputHelper output)
{
    [Fact]
    public async Task RasterMatchesTheBaseReferenceImage()
    {
        var screenshots = Path.Combine(DotNet.Setting("ArtifactsDirectory"), "platform");
        Directory.CreateDirectory(screenshots);

        using var bitmap = TestImage.Render();
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        await new ScreenshotVerifier(screenshots, output).VerifyScreenshot(
            encoded.ToArray(), "host-raster");
    }
}
