using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Checks that sample output is a decodable image with the expected dimensions.
internal static class SampleImage
{
    public static void Validate(ReadOnlySpan<byte> png) => Validate(png, 800, 600);

    public static void Validate(ReadOnlySpan<byte> png, int width, int height)
    {
        using var bitmap = SKBitmap.Decode(png);
        Assert.NotNull(bitmap);
        Assert.Equal(width, bitmap.Width);
        Assert.Equal(height, bitmap.Height);
    }
}
